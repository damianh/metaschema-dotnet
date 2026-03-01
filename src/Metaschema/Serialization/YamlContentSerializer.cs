// Licensed under the MIT License.

using Metaschema.Nodes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Metaschema.Serialization;

/// <summary>
/// Serializes document nodes to YAML format.
/// </summary>
public sealed class YamlContentSerializer : ISerializer
{
    private readonly BindingContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlContentSerializer"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    public YamlContentSerializer(BindingContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public Format Format => Format.Yaml;

    /// <inheritdoc />
    public void Serialize(IDocumentRootNode node, Stream output)
    {
        using var writer = new StreamWriter(output, leaveOpen: true);
        Serialize(node, writer);
    }

    /// <inheritdoc />
    public void Serialize(IDocumentRootNode node, TextWriter writer)
    {
        var content = SerializeToString(node);
        writer.Write(content);
    }

    /// <inheritdoc />
    public string SerializeToString(IDocumentRootNode node)
    {
        if (node is not DocumentNode doc || doc.RootAssembly is null)
        {
            return "{}";
        }

        var root = new Dictionary<string, object?>
        {
            [doc.Name] = BuildAssemblyObject(doc.RootAssembly)
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        return serializer.Serialize(root);
    }

    private Dictionary<string, object?> BuildAssemblyObject(AssemblyNode assembly)
    {
        var result = new Dictionary<string, object?>();

        // Add flags
        foreach (var flag in assembly.Flags.Values)
        {
            result[flag.Name] = ConvertValue(flag.RawValue, flag.Definition.DataTypeName);
        }

        // Add model children, grouping by name for arrays
        var groupedChildren = assembly.ModelChildren
            .GroupBy(c => c.Name)
            .ToList();

        foreach (var group in groupedChildren)
        {
            var children = group.ToList();
            if (children.Count == 1)
            {
                result[group.Key] = BuildModelChildObject(children[0]);
            }
            else
            {
                result[group.Key] = children.Select(BuildModelChildObject).ToList();
            }
        }

        return result;
    }

    private object? BuildModelChildObject(IDocumentNode node)
    {
        return node switch
        {
            AssemblyNode assembly => BuildAssemblyObject(assembly),
            FieldNode field => BuildFieldObject(field),
            _ => null
        };
    }

    private static object? BuildFieldObject(FieldNode field)
    {
        // If field has flags, return as object
        if (field.Flags.Count > 0)
        {
            var result = new Dictionary<string, object?>();

            foreach (var flag in field.Flags.Values)
            {
                result[flag.Name] = ConvertValue(flag.RawValue, flag.Definition.DataTypeName);
            }

            var valueKeyName = field.Definition.JsonValueKeyName ?? "STRVALUE";
            result[valueKeyName] = ConvertValue(field.RawValue, field.Definition.DataTypeName);

            return result;
        }

        // Simple value
        return ConvertValue(field.RawValue, field.Definition.DataTypeName);
    }

    private static object? ConvertValue(string? rawValue, string dataTypeName)
    {
        if (rawValue is null)
        {
            return null;
        }

        // Convert to appropriate type for YAML serialization
        switch (dataTypeName)
        {
            case "integer":
            case "non-negative-integer":
            case "positive-integer":
                if (long.TryParse(rawValue, out var longVal))
                {
                    return longVal;
                }
                break;

            case "decimal":
                if (decimal.TryParse(rawValue, out var decimalVal))
                {
                    return decimalVal;
                }
                break;

            case "boolean":
                if (bool.TryParse(rawValue, out var boolVal))
                {
                    return boolVal;
                }
                break;
        }

        return rawValue;
    }
}
