// Licensed under the MIT License.

using System.Text.Json;
using Metaschema.Core.Datatypes;
using Metaschema.Core.Model;
using Metaschema.Databind.Nodes;

namespace Metaschema.Databind.Serialization;

/// <summary>
/// Deserializes JSON content into document nodes.
/// </summary>
public sealed class JsonContentDeserializer : IDeserializer
{
    private readonly IBindingContext _context;
    private readonly IDataTypeProvider _dataTypeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContentDeserializer"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    public JsonContentDeserializer(IBindingContext context)
        : this(context, DataTypeProvider.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContentDeserializer"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    /// <param name="dataTypeProvider">The data type provider.</param>
    public JsonContentDeserializer(IBindingContext context, IDataTypeProvider dataTypeProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dataTypeProvider = dataTypeProvider ?? throw new ArgumentNullException(nameof(dataTypeProvider));
    }

    /// <inheritdoc />
    public Format Format => Format.Json;

    /// <inheritdoc />
    public DocumentNode Deserialize(Stream input)
    {
        using var doc = JsonDocument.Parse(input);
        return DeserializeCore(doc);
    }

    /// <inheritdoc />
    public DocumentNode Deserialize(TextReader reader)
    {
        var content = reader.ReadToEnd();
        return Deserialize(content);
    }

    /// <inheritdoc />
    public DocumentNode Deserialize(string content)
    {
        using var doc = JsonDocument.Parse(content);
        return DeserializeCore(doc);
    }

    private DocumentNode DeserializeCore(JsonDocument doc)
    {
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new SerializationException("JSON root must be an object.");
        }

        // Find the root element name (first property that matches a root assembly)
        foreach (var property in root.EnumerateObject())
        {
            var rootName = property.Name;
            var rootDefinition = _context.ResolveRootAssembly(rootName);

            if (rootDefinition is not null)
            {
                var document = new DocumentNode(rootName, rootDefinition);
                var rootAssembly = ReadAssembly(property.Value, rootName, rootDefinition, document);
                document.RootAssembly = rootAssembly;
                document.AddChild(rootAssembly);
                return document;
            }
        }

        throw new SerializationException("No recognized root element found in JSON content.");
    }

    private AssemblyNode ReadAssembly(JsonElement element, string name, AssemblyDefinition definition, IDocumentNode parent)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new SerializationException($"Expected object for assembly '{name}', got {element.ValueKind}.");
        }

        var assembly = new AssemblyNode(name, definition, parent);

        foreach (var property in element.EnumerateObject())
        {
            var propertyName = property.Name;

            // Check if it's a flag
            var flagInstance = definition.FlagInstances.FirstOrDefault(f => f.EffectiveName == propertyName);
            if (flagInstance?.ResolvedDefinition is not null)
            {
                var flagNode = ReadFlag(property.Value, propertyName, flagInstance.ResolvedDefinition, assembly);
                assembly.AddFlag(flagNode);
                continue;
            }

            // Check if it's a model child
            if (definition.Model is not null)
            {
                var modelChild = ReadModelChildFromProperty(property, definition, assembly);
                if (modelChild is not null)
                {
                    if (modelChild is IEnumerable<IDocumentNode> children)
                    {
                        foreach (var child in children)
                        {
                            assembly.AddModelChild(child);
                        }
                    }
                    else
                    {
                        assembly.AddModelChild((IDocumentNode)modelChild);
                    }
                }
            }
        }

        return assembly;
    }

    private object? ReadModelChildFromProperty(JsonProperty property, AssemblyDefinition parentDefinition, IDocumentNode parent)
    {
        var propertyName = property.Name;
        var element = property.Value;

        foreach (var modelElement in parentDefinition.Model?.Elements ?? [])
        {
            if (modelElement is FieldInstance fieldInstance)
            {
                // Check direct name or group-as name
                var effectiveName = fieldInstance.GroupAs?.Name ?? fieldInstance.EffectiveName;
                if (effectiveName == propertyName && fieldInstance.ResolvedDefinition is not null)
                {
                    return ReadFieldOrArray(element, fieldInstance, parent);
                }
            }
            else if (modelElement is AssemblyInstance assemblyInstance)
            {
                var effectiveName = assemblyInstance.GroupAs?.Name ?? assemblyInstance.EffectiveName;
                if (effectiveName == propertyName && assemblyInstance.ResolvedDefinition is not null)
                {
                    return ReadAssemblyOrArray(element, assemblyInstance, parent);
                }
            }
            else if (modelElement is ChoiceGroup choiceGroup)
            {
                foreach (var choice in choiceGroup.Choices)
                {
                    if (choice is FieldInstance choiceField)
                    {
                        var effectiveName = choiceField.GroupAs?.Name ?? choiceField.EffectiveName;
                        if (effectiveName == propertyName && choiceField.ResolvedDefinition is not null)
                        {
                            return ReadFieldOrArray(element, choiceField, parent);
                        }
                    }
                    else if (choice is AssemblyInstance choiceAssembly)
                    {
                        var effectiveName = choiceAssembly.GroupAs?.Name ?? choiceAssembly.EffectiveName;
                        if (effectiveName == propertyName && choiceAssembly.ResolvedDefinition is not null)
                        {
                            return ReadAssemblyOrArray(element, choiceAssembly, parent);
                        }
                    }
                }
            }
        }

        return null;
    }

    private object ReadFieldOrArray(JsonElement element, FieldInstance instance, IDocumentNode parent)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            var fields = new List<FieldNode>();
            foreach (var item in element.EnumerateArray())
            {
                fields.Add(ReadField(item, instance.EffectiveName, instance.ResolvedDefinition!, parent));
            }
            return fields;
        }

        return ReadField(element, instance.EffectiveName, instance.ResolvedDefinition!, parent);
    }

    private object ReadAssemblyOrArray(JsonElement element, AssemblyInstance instance, IDocumentNode parent)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            var assemblies = new List<AssemblyNode>();
            foreach (var item in element.EnumerateArray())
            {
                assemblies.Add(ReadAssembly(item, instance.EffectiveName, instance.ResolvedDefinition!, parent));
            }
            return assemblies;
        }

        return ReadAssembly(element, instance.EffectiveName, instance.ResolvedDefinition!, parent);
    }

    private FieldNode ReadField(JsonElement element, string name, FieldDefinition definition, IDocumentNode parent)
    {
        var field = new FieldNode(name, definition, parent);

        // If field has flags, it must be an object
        if (definition.FlagInstances.Count > 0 && element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var propertyName = property.Name;

                // Check if it's a flag
                var flagInstance = definition.FlagInstances.FirstOrDefault(f => f.EffectiveName == propertyName);
                if (flagInstance?.ResolvedDefinition is not null)
                {
                    var flagNode = ReadFlag(property.Value, propertyName, flagInstance.ResolvedDefinition, field);
                    field.AddFlag(flagNode);
                    continue;
                }

                // Check if it's the value key
                var valueKeyName = definition.JsonValueKeyName ?? "STRVALUE";
                if (propertyName == valueKeyName)
                {
                    SetFieldValue(field, property.Value, definition.DataTypeName);
                }
            }
        }
        else
        {
            // Simple field - value directly
            SetFieldValue(field, element, definition.DataTypeName);
        }

        return field;
    }

    private void SetFieldValue(FieldNode field, JsonElement element, string dataTypeName)
    {
        var rawValue = GetRawValue(element);
        field.RawValue = rawValue;
        field.Value = ParseValue(rawValue, dataTypeName);
    }

    private FlagNode ReadFlag(JsonElement element, string name, FlagDefinition definition, IDocumentNode parent)
    {
        var rawValue = GetRawValue(element);
        return new FlagNode(name, definition, parent)
        {
            RawValue = rawValue,
            Value = ParseValue(rawValue, definition.DataTypeName)
        };
    }

    private static string? GetRawValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    private object? ParseValue(string? rawValue, string dataTypeName)
    {
        if (rawValue is null)
        {
            return null;
        }

        var adapter = _dataTypeProvider.GetAdapter(dataTypeName);
        if (adapter is null)
        {
            return rawValue;
        }

        if (adapter.TryParse(rawValue, out var result))
        {
            return result;
        }

        return rawValue;
    }
}
