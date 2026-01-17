// Licensed under the MIT License.

using Metaschema.Core.Datatypes;
using Metaschema.Core.Model;
using Metaschema.Databind.Nodes;
using YamlDotNet.RepresentationModel;

namespace Metaschema.Databind.Serialization;

/// <summary>
/// Deserializes YAML content into document nodes.
/// </summary>
public sealed class YamlContentDeserializer : IDeserializer
{
    private readonly IBindingContext _context;
    private readonly IDataTypeProvider _dataTypeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlContentDeserializer"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    public YamlContentDeserializer(IBindingContext context)
        : this(context, DataTypeProvider.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlContentDeserializer"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    /// <param name="dataTypeProvider">The data type provider.</param>
    public YamlContentDeserializer(IBindingContext context, IDataTypeProvider dataTypeProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dataTypeProvider = dataTypeProvider ?? throw new ArgumentNullException(nameof(dataTypeProvider));
    }

    /// <inheritdoc />
    public Format Format => Format.Yaml;

    /// <inheritdoc />
    public DocumentNode Deserialize(Stream input)
    {
        using var reader = new StreamReader(input);
        return Deserialize(reader);
    }

    /// <inheritdoc />
    public DocumentNode Deserialize(TextReader reader)
    {
        var yaml = new YamlStream();
        yaml.Load(reader);

        if (yaml.Documents.Count == 0)
        {
            throw new SerializationException("No YAML document found.");
        }

        return DeserializeCore(yaml.Documents[0]);
    }

    /// <inheritdoc />
    public DocumentNode Deserialize(string content)
    {
        using var reader = new StringReader(content);
        return Deserialize(reader);
    }

    private DocumentNode DeserializeCore(YamlDocument doc)
    {
        if (doc.RootNode is not YamlMappingNode root)
        {
            throw new SerializationException("YAML root must be a mapping.");
        }

        // Find the root element name (first key that matches a root assembly)
        foreach (var entry in root.Children)
        {
            if (entry.Key is YamlScalarNode keyNode)
            {
                var rootName = keyNode.Value ?? "";
                var rootDefinition = _context.ResolveRootAssembly(rootName);

                if (rootDefinition is not null && entry.Value is YamlMappingNode valueNode)
                {
                    var document = new DocumentNode(rootName, rootDefinition);
                    var rootAssembly = ReadAssembly(valueNode, rootName, rootDefinition, document);
                    document.RootAssembly = rootAssembly;
                    document.AddChild(rootAssembly);
                    return document;
                }
            }
        }

        throw new SerializationException("No recognized root element found in YAML content.");
    }

    private AssemblyNode ReadAssembly(YamlMappingNode node, string name, AssemblyDefinition definition, IDocumentNode parent)
    {
        var assembly = new AssemblyNode(name, definition, parent);

        foreach (var entry in node.Children)
        {
            if (entry.Key is not YamlScalarNode keyNode || keyNode.Value is null)
            {
                continue;
            }

            var propertyName = keyNode.Value;

            // Check if it's a flag
            var flagInstance = definition.FlagInstances.FirstOrDefault(f => f.EffectiveName == propertyName);
            if (flagInstance?.ResolvedDefinition is not null)
            {
                var flagNode = ReadFlag(entry.Value, propertyName, flagInstance.ResolvedDefinition, assembly);
                assembly.AddFlag(flagNode);
                continue;
            }

            // Check if it's a model child
            if (definition.Model is not null)
            {
                var modelChild = ReadModelChildFromEntry(propertyName, entry.Value, definition, assembly);
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

    private object? ReadModelChildFromEntry(string propertyName, YamlNode value, AssemblyDefinition parentDefinition, IDocumentNode parent)
    {
        foreach (var modelElement in parentDefinition.Model?.Elements ?? [])
        {
            if (modelElement is FieldInstance fieldInstance)
            {
                var effectiveName = fieldInstance.GroupAs?.Name ?? fieldInstance.EffectiveName;
                if (effectiveName == propertyName && fieldInstance.ResolvedDefinition is not null)
                {
                    return ReadFieldOrArray(value, fieldInstance, parent);
                }
            }
            else if (modelElement is AssemblyInstance assemblyInstance)
            {
                var effectiveName = assemblyInstance.GroupAs?.Name ?? assemblyInstance.EffectiveName;
                if (effectiveName == propertyName && assemblyInstance.ResolvedDefinition is not null)
                {
                    return ReadAssemblyOrArray(value, assemblyInstance, parent);
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
                            return ReadFieldOrArray(value, choiceField, parent);
                        }
                    }
                    else if (choice is AssemblyInstance choiceAssembly)
                    {
                        var effectiveName = choiceAssembly.GroupAs?.Name ?? choiceAssembly.EffectiveName;
                        if (effectiveName == propertyName && choiceAssembly.ResolvedDefinition is not null)
                        {
                            return ReadAssemblyOrArray(value, choiceAssembly, parent);
                        }
                    }
                }
            }
        }

        return null;
    }

    private object ReadFieldOrArray(YamlNode node, FieldInstance instance, IDocumentNode parent)
    {
        if (node is YamlSequenceNode sequence)
        {
            var fields = new List<FieldNode>();
            foreach (var item in sequence.Children)
            {
                fields.Add(ReadField(item, instance.EffectiveName, instance.ResolvedDefinition!, parent));
            }
            return fields;
        }

        return ReadField(node, instance.EffectiveName, instance.ResolvedDefinition!, parent);
    }

    private object ReadAssemblyOrArray(YamlNode node, AssemblyInstance instance, IDocumentNode parent)
    {
        if (node is YamlSequenceNode sequence)
        {
            var assemblies = new List<AssemblyNode>();
            foreach (var item in sequence.Children)
            {
                if (item is YamlMappingNode mappingNode)
                {
                    assemblies.Add(ReadAssembly(mappingNode, instance.EffectiveName, instance.ResolvedDefinition!, parent));
                }
            }
            return assemblies;
        }

        if (node is YamlMappingNode mapping)
        {
            return ReadAssembly(mapping, instance.EffectiveName, instance.ResolvedDefinition!, parent);
        }

        throw new SerializationException($"Expected mapping or sequence for assembly '{instance.EffectiveName}'.");
    }

    private FieldNode ReadField(YamlNode node, string name, FieldDefinition definition, IDocumentNode parent)
    {
        var field = new FieldNode(name, definition, parent);

        // If field has flags and node is a mapping, read flags
        if (definition.FlagInstances.Count > 0 && node is YamlMappingNode mapping)
        {
            foreach (var entry in mapping.Children)
            {
                if (entry.Key is not YamlScalarNode keyNode || keyNode.Value is null)
                {
                    continue;
                }

                var propertyName = keyNode.Value;

                // Check if it's a flag
                var flagInstance = definition.FlagInstances.FirstOrDefault(f => f.EffectiveName == propertyName);
                if (flagInstance?.ResolvedDefinition is not null)
                {
                    var flagNode = ReadFlag(entry.Value, propertyName, flagInstance.ResolvedDefinition, field);
                    field.AddFlag(flagNode);
                    continue;
                }

                // Check if it's the value key
                var valueKeyName = definition.JsonValueKeyName ?? "STRVALUE";
                if (propertyName == valueKeyName)
                {
                    SetFieldValue(field, entry.Value, definition.DataTypeName);
                }
            }
        }
        else
        {
            // Simple value
            SetFieldValue(field, node, definition.DataTypeName);
        }

        return field;
    }

    private void SetFieldValue(FieldNode field, YamlNode node, string dataTypeName)
    {
        var rawValue = GetRawValue(node);
        field.RawValue = rawValue;
        field.Value = ParseValue(rawValue, dataTypeName);
    }

    private FlagNode ReadFlag(YamlNode node, string name, FlagDefinition definition, IDocumentNode parent)
    {
        var rawValue = GetRawValue(node);
        return new FlagNode(name, definition, parent)
        {
            RawValue = rawValue,
            Value = ParseValue(rawValue, definition.DataTypeName)
        };
    }

    private static string? GetRawValue(YamlNode node)
    {
        return node switch
        {
            YamlScalarNode scalar => scalar.Value,
            _ => node.ToString()
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
