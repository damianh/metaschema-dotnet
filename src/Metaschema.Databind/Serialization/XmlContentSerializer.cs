// Licensed under the MIT License.

using System.Xml;
using Metaschema.Databind.Nodes;

namespace Metaschema.Databind.Serialization;

/// <summary>
/// Serializes document nodes to XML format.
/// </summary>
public sealed class XmlContentSerializer : ISerializer
{
    private readonly IBindingContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlContentSerializer"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    public XmlContentSerializer(IBindingContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public Format Format => Format.Xml;

    /// <inheritdoc />
    public void Serialize(IDocumentRootNode node, Stream output)
    {
        using var writer = XmlWriter.Create(output, CreateWriterSettings());
        SerializeCore(node, writer);
    }

    /// <inheritdoc />
    public void Serialize(IDocumentRootNode node, TextWriter writer)
    {
        using var xmlWriter = XmlWriter.Create(writer, CreateWriterSettings());
        SerializeCore(node, xmlWriter);
    }

    /// <inheritdoc />
    public string SerializeToString(IDocumentRootNode node)
    {
        using var stringWriter = new StringWriter();
        Serialize(node, stringWriter);
        return stringWriter.ToString();
    }

    private static XmlWriterSettings CreateWriterSettings()
    {
        return new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = false
        };
    }

    private void SerializeCore(IDocumentRootNode node, XmlWriter writer)
    {
        writer.WriteStartDocument();

        if (node is DocumentNode doc && doc.RootAssembly is not null)
        {
            WriteAssembly(doc.RootAssembly, writer, doc.Definition.ContainingModule.XmlNamespace);
        }

        writer.WriteEndDocument();
        writer.Flush();
    }

    private void WriteAssembly(AssemblyNode assembly, XmlWriter writer, Uri? namespaceUri)
    {
        if (namespaceUri is not null)
        {
            writer.WriteStartElement(assembly.Name, namespaceUri.ToString());
        }
        else
        {
            writer.WriteStartElement(assembly.Name);
        }

        // Write flags as attributes
        foreach (var flag in assembly.Flags.Values)
        {
            if (flag.RawValue is not null)
            {
                writer.WriteAttributeString(flag.Name, flag.RawValue);
            }
        }

        // Write model children
        foreach (var child in assembly.ModelChildren)
        {
            WriteModelChild(child, writer);
        }

        writer.WriteEndElement();
    }

    private void WriteModelChild(IDocumentNode node, XmlWriter writer)
    {
        if (node is AssemblyNode assembly)
        {
            WriteAssembly(assembly, writer, null);
        }
        else if (node is FieldNode field)
        {
            WriteField(field, writer);
        }
    }

    private static void WriteField(FieldNode field, XmlWriter writer)
    {
        writer.WriteStartElement(field.Name);

        // Write flags as attributes
        foreach (var flag in field.Flags.Values)
        {
            if (flag.RawValue is not null)
            {
                writer.WriteAttributeString(flag.Name, flag.RawValue);
            }
        }

        // Write value
        if (field.RawValue is not null)
        {
            // For markup types, write as raw XML; otherwise as text
            if (field.Definition.DataTypeName is "markup-line" or "markup-multiline")
            {
                writer.WriteRaw(field.RawValue);
            }
            else
            {
                writer.WriteString(field.RawValue);
            }
        }

        writer.WriteEndElement();
    }
}
