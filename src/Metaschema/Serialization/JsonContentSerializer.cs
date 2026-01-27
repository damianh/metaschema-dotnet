// Licensed under the MIT License.

using System.Text.Json;
using Metaschema.Nodes;

namespace Metaschema.Serialization;

/// <summary>
/// Serializes document nodes to JSON format.
/// </summary>
public sealed class JsonContentSerializer : ISerializer
{
    private readonly BindingContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContentSerializer"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    public JsonContentSerializer(BindingContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public Format Format => Format.Json;

    /// <inheritdoc />
    public void Serialize(IDocumentRootNode node, Stream output)
    {
        using var writer = new Utf8JsonWriter(output, CreateWriterOptions());
        SerializeCore(node, writer);
    }

    /// <inheritdoc />
    public void Serialize(IDocumentRootNode node, TextWriter writer)
    {
        using var memoryStream = new MemoryStream();
        Serialize(node, memoryStream);
        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream);
        writer.Write(reader.ReadToEnd());
    }

    /// <inheritdoc />
    public string SerializeToString(IDocumentRootNode node)
    {
        using var memoryStream = new MemoryStream();
        Serialize(node, memoryStream);
        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream);
        return reader.ReadToEnd();
    }

    private static JsonWriterOptions CreateWriterOptions()
    {
        return new JsonWriterOptions
        {
            Indented = true
        };
    }

    private void SerializeCore(IDocumentRootNode node, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        if (node is DocumentNode doc && doc.RootAssembly is not null)
        {
            writer.WritePropertyName(doc.Name);
            WriteAssembly(doc.RootAssembly, writer);
        }

        writer.WriteEndObject();
        writer.Flush();
    }

    private void WriteAssembly(AssemblyNode assembly, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        // Write flags as properties
        foreach (var flag in assembly.Flags.Values)
        {
            WriteFlag(flag, writer);
        }

        // Write model children, grouping by name for arrays
        var groupedChildren = assembly.ModelChildren
            .GroupBy(c => c.Name)
            .ToList();

        foreach (var group in groupedChildren)
        {
            var children = group.ToList();
            if (children.Count == 1)
            {
                // Single child
                var child = children[0];
                writer.WritePropertyName(child.Name);
                WriteModelChild(child, writer);
            }
            else
            {
                // Array of children
                writer.WritePropertyName(group.Key);
                writer.WriteStartArray();
                foreach (var child in children)
                {
                    WriteModelChild(child, writer);
                }
                writer.WriteEndArray();
            }
        }

        writer.WriteEndObject();
    }

    private void WriteModelChild(IDocumentNode node, Utf8JsonWriter writer)
    {
        if (node is AssemblyNode assembly)
        {
            WriteAssembly(assembly, writer);
        }
        else if (node is FieldNode field)
        {
            WriteField(field, writer);
        }
    }

    private static void WriteField(FieldNode field, Utf8JsonWriter writer)
    {
        // If field has flags, write as object
        if (field.Flags.Count > 0)
        {
            writer.WriteStartObject();

            foreach (var flag in field.Flags.Values)
            {
                WriteFlag(flag, writer);
            }

            var valueKeyName = field.Definition.JsonValueKeyName ?? "STRVALUE";
            if (field.RawValue is not null)
            {
                writer.WritePropertyName(valueKeyName);
                WriteValue(field.RawValue, field.Definition.DataTypeName, writer);
            }

            writer.WriteEndObject();
        }
        else
        {
            // Simple value
            if (field.RawValue is not null)
            {
                WriteValue(field.RawValue, field.Definition.DataTypeName, writer);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

    private static void WriteFlag(IFlagNode flag, Utf8JsonWriter writer)
    {
        writer.WritePropertyName(flag.Name);
        if (flag.RawValue is not null)
        {
            WriteValue(flag.RawValue, flag.Definition.DataTypeName, writer);
        }
        else
        {
            writer.WriteNullValue();
        }
    }

    private static void WriteValue(string rawValue, string dataTypeName, Utf8JsonWriter writer)
    {
        // Map data types to appropriate JSON types
        switch (dataTypeName)
        {
            case "integer":
            case "non-negative-integer":
            case "positive-integer":
                if (long.TryParse(rawValue, out var longVal))
                {
                    writer.WriteNumberValue(longVal);
                    return;
                }
                break;

            case "decimal":
                if (decimal.TryParse(rawValue, out var decimalVal))
                {
                    writer.WriteNumberValue(decimalVal);
                    return;
                }
                break;

            case "boolean":
                if (bool.TryParse(rawValue, out var boolVal))
                {
                    writer.WriteBooleanValue(boolVal);
                    return;
                }
                break;
        }

        // Default to string
        writer.WriteStringValue(rawValue);
    }
}
