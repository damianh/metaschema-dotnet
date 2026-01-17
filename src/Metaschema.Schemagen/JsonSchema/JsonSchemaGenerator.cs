// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Metaschema.Core.Model;

namespace Metaschema.Schemagen.JsonSchema;

/// <summary>
/// Generates JSON Schema documents from Metaschema modules.
/// </summary>
public sealed class JsonSchemaGenerator
{
    private readonly SchemaGenerationOptions _options;
    private readonly HashSet<string> _generatedDefs = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSchemaGenerator"/> class.
    /// </summary>
    /// <param name="options">The generation options.</param>
    public JsonSchemaGenerator(SchemaGenerationOptions? options = null)
    {
        _options = options ?? SchemaGenerationOptions.Default;
    }

    /// <summary>
    /// Generates a JSON Schema document from the specified module.
    /// </summary>
    /// <param name="module">The Metaschema module.</param>
    /// <returns>The generated JSON Schema document.</returns>
    public JsonDocument Generate(MetaschemaModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        _generatedDefs.Clear();

        var baseUri = _options.BaseUri ?? module.JsonBaseUri;

        var schema = new JsonObject
        {
            ["$schema"] = _options.JsonSchemaDialect,
            ["$id"] = baseUri.ToString(),
            ["type"] = "object"
        };

        if (_options.IncludeDocumentation)
        {
            schema["title"] = module.Name;
            if (module.Remarks is not null)
            {
                schema["description"] = module.Remarks.Value.Value;
            }
        }

        // Generate root properties for root assemblies
        var properties = new JsonObject();
        foreach (var rootAssembly in module.RootAssemblyDefinitions)
        {
            var rootName = rootAssembly.RootName ?? rootAssembly.EffectiveName;
            properties[rootName] = new JsonObject
            {
                ["$ref"] = $"#/$defs/{rootAssembly.Name}"
            };
        }

        if (properties.Count > 0)
        {
            schema["properties"] = properties;
        }

        // Generate $defs for all definitions
        var defs = new JsonObject();

        foreach (var assembly in GetAllAssemblyDefinitions(module))
        {
            if (_generatedDefs.Add(assembly.Name))
            {
                defs[assembly.Name] = GenerateAssemblySchema(assembly, module);
            }
        }

        foreach (var field in GetAllFieldDefinitions(module))
        {
            if (_generatedDefs.Add(field.Name))
            {
                defs[field.Name] = GenerateFieldSchema(field, module);
            }
        }

        if (defs.Count > 0)
        {
            schema["$defs"] = defs;
        }

        return JsonDocument.Parse(schema.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    private JsonObject GenerateAssemblySchema(AssemblyDefinition assembly, MetaschemaModule module)
    {
        var schema = new JsonObject { ["type"] = "object" };

        if (_options.IncludeDocumentation)
        {
            if (assembly.FormalName is not null)
            {
                schema["title"] = assembly.FormalName;
            }
            if (assembly.Description is not null)
            {
                schema["description"] = assembly.Description.Value.Value;
            }
        }

        var properties = new JsonObject();
        var required = new JsonArray();

        // Add flag properties
        foreach (var flag in assembly.FlagInstances)
        {
            var flagDef = flag.ResolvedDefinition ?? module.GetFlagDefinition(flag.Ref);
            var propName = flag.EffectiveName;

            properties[propName] = JsonSchemaTypeMapper.GetTypeSchema(flagDef?.DataTypeName ?? "string");

            if (flag.IsRequired)
            {
                required.Add(propName);
            }
        }

        // Add model element properties
        if (assembly.Model?.Elements.Count > 0)
        {
            foreach (var element in assembly.Model.Elements)
            {
                AddModelElementProperty(properties, required, element, module);
            }
        }

        if (properties.Count > 0)
        {
            schema["properties"] = properties;
        }

        if (required.Count > 0)
        {
            schema["required"] = required;
        }

        schema["additionalProperties"] = false;

        return schema;
    }

    private JsonObject GenerateFieldSchema(FieldDefinition field, MetaschemaModule module)
    {
        // If field has no flags, it's just a simple type
        if (field.FlagInstances.Count == 0)
        {
            var typeSchema = JsonSchemaTypeMapper.GetTypeSchema(field.DataTypeName);
            if (_options.IncludeDocumentation && field.Description is not null)
            {
                typeSchema["description"] = field.Description.Value.Value;
            }
            return typeSchema;
        }

        // Field with flags becomes an object
        var schema = new JsonObject { ["type"] = "object" };

        if (_options.IncludeDocumentation)
        {
            if (field.FormalName is not null)
            {
                schema["title"] = field.FormalName;
            }
            if (field.Description is not null)
            {
                schema["description"] = field.Description.Value.Value;
            }
        }

        var properties = new JsonObject();
        var required = new JsonArray();

        // Add the value property
        var valueKey = field.JsonValueKeyName ?? "STRVALUE";
        properties[valueKey] = JsonSchemaTypeMapper.GetTypeSchema(field.DataTypeName);
        required.Add(valueKey);

        // Add flag properties
        foreach (var flag in field.FlagInstances)
        {
            var flagDef = flag.ResolvedDefinition ?? module.GetFlagDefinition(flag.Ref);
            var propName = flag.EffectiveName;

            properties[propName] = JsonSchemaTypeMapper.GetTypeSchema(flagDef?.DataTypeName ?? "string");

            if (flag.IsRequired)
            {
                required.Add(propName);
            }
        }

        schema["properties"] = properties;
        schema["required"] = required;
        schema["additionalProperties"] = false;

        return schema;
    }

    private static void AddModelElementProperty(JsonObject properties, JsonArray required,
        ModelElement element, MetaschemaModule module)
    {
        switch (element)
        {
            case FieldInstance field:
                AddFieldProperty(properties, required, field, module);
                break;

            case AssemblyInstance assembly:
                AddAssemblyProperty(properties, required, assembly, module);
                break;

            case ChoiceGroup choice:
                // For choice groups, add all choices as optional properties
                // A more sophisticated approach would use oneOf
                foreach (var item in choice.Choices)
                {
                    AddModelElementProperty(properties, required, item, module);
                }
                break;
        }
    }

    private static void AddFieldProperty(JsonObject properties, JsonArray required,
        FieldInstance field, MetaschemaModule module)
    {
        var fieldDef = field.ResolvedDefinition ?? module.GetFieldDefinition(field.Ref);
        var propName = field.EffectiveName;

        JsonNode propSchema;

        // Determine if it's a collection
        if (field.MaxOccurs is null || field.MaxOccurs > 1)
        {
            propSchema = new JsonObject
            {
                ["type"] = "array",
                ["items"] = fieldDef?.FlagInstances.Count > 0
                    ? new JsonObject { ["$ref"] = $"#/$defs/{fieldDef.Name}" }
                    : JsonSchemaTypeMapper.GetTypeSchema(fieldDef?.DataTypeName ?? "string")
            };

            if (field.MinOccurs > 0)
            {
                ((JsonObject)propSchema)["minItems"] = field.MinOccurs;
            }
        }
        else
        {
            propSchema = fieldDef?.FlagInstances.Count > 0
                ? new JsonObject { ["$ref"] = $"#/$defs/{fieldDef.Name}" }
                : JsonSchemaTypeMapper.GetTypeSchema(fieldDef?.DataTypeName ?? "string");
        }

        properties[propName] = propSchema;

        if (field.MinOccurs > 0)
        {
            required.Add(propName);
        }
    }

    private static void AddAssemblyProperty(JsonObject properties, JsonArray required,
        AssemblyInstance assembly, MetaschemaModule module)
    {
        var propName = assembly.EffectiveName;

        JsonNode propSchema;

        // Determine if it's a collection
        if (assembly.MaxOccurs is null || assembly.MaxOccurs > 1)
        {
            propSchema = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["$ref"] = $"#/$defs/{assembly.Ref}" }
            };

            if (assembly.MinOccurs > 0)
            {
                ((JsonObject)propSchema)["minItems"] = assembly.MinOccurs;
            }
        }
        else
        {
            propSchema = new JsonObject { ["$ref"] = $"#/$defs/{assembly.Ref}" };
        }

        properties[propName] = propSchema;

        if (assembly.MinOccurs > 0)
        {
            required.Add(propName);
        }
    }

    private static IEnumerable<FieldDefinition> GetAllFieldDefinitions(MetaschemaModule module)
    {
        foreach (var field in module.FieldDefinitions)
        {
            yield return field;
        }
        foreach (var import in module.ImportedModules)
        {
            foreach (var field in GetAllFieldDefinitions(import))
            {
                yield return field;
            }
        }
    }

    private static IEnumerable<AssemblyDefinition> GetAllAssemblyDefinitions(MetaschemaModule module)
    {
        foreach (var assembly in module.AssemblyDefinitions)
        {
            yield return assembly;
        }
        foreach (var import in module.ImportedModules)
        {
            foreach (var assembly in GetAllAssemblyDefinitions(import))
            {
                yield return assembly;
            }
        }
    }
}
