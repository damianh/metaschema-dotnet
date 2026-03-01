// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Xml;
using Metaschema.Datatypes;
using Metaschema.Model;
using Metaschema.Nodes;

namespace Metaschema.Serialization;

/// <summary>
/// Deserializes XML content into document nodes.
/// </summary>
public sealed class XmlContentDeserializer : IDeserializer
{
    private readonly BindingContext _context;
    private readonly IDataTypeProvider _dataTypeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlContentDeserializer"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    public XmlContentDeserializer(BindingContext context)
        : this(context, DataTypeProvider.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlContentDeserializer"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    /// <param name="dataTypeProvider">The data type provider.</param>
    public XmlContentDeserializer(BindingContext context, IDataTypeProvider dataTypeProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dataTypeProvider = dataTypeProvider ?? throw new ArgumentNullException(nameof(dataTypeProvider));
    }

    /// <inheritdoc />
    public Format Format => Format.Xml;

    /// <inheritdoc />
    public DocumentNode Deserialize(Stream input)
    {
        using var reader = XmlReader.Create(input, CreateReaderSettings());
        return DeserializeCore(reader);
    }

    /// <inheritdoc />
    public DocumentNode Deserialize(TextReader reader)
    {
        using var xmlReader = XmlReader.Create(reader, CreateReaderSettings());
        return DeserializeCore(xmlReader);
    }

    /// <inheritdoc />
    public DocumentNode Deserialize(string content)
    {
        using var stringReader = new StringReader(content);
        return Deserialize(stringReader);
    }

    private static XmlReaderSettings CreateReaderSettings() => new XmlReaderSettings
    {
        IgnoreWhitespace = true,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true
    };

    private DocumentNode DeserializeCore(XmlReader reader)
    {
        // Move to the root element
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                break;
            }
        }

        if (reader.NodeType != XmlNodeType.Element)
        {
            throw new SerializationException("No root element found in XML content.");
        }

        var rootName = reader.LocalName;
        var namespaceUri = reader.NamespaceURI;

        // Resolve the root assembly definition
        AssemblyDefinition? rootDefinition = null;
        if (!string.IsNullOrEmpty(namespaceUri))
        {
            rootDefinition = _context.ResolveRootAssembly(rootName, new Uri(namespaceUri));
        }
        rootDefinition ??= _context.ResolveRootAssembly(rootName);

        if (rootDefinition is null)
        {
            throw new SerializationException($"No assembly definition found for root element '{rootName}' with namespace '{namespaceUri}'.");
        }

        var document = new DocumentNode(rootName, rootDefinition);
        var rootAssembly = ReadAssembly(reader, rootName, rootDefinition, document);
        document.RootAssembly = rootAssembly;
        document.AddChild(rootAssembly);

        return document;
    }

    private AssemblyNode ReadAssembly(XmlReader reader, string elementName, AssemblyDefinition definition, IDocumentNode parent)
    {
        var assembly = new AssemblyNode(elementName, definition, parent);

        // Read attributes as flags
        if (reader.HasAttributes)
        {
            while (reader.MoveToNextAttribute())
            {
                // Skip namespace declarations
                if (reader.Prefix == "xmlns" || reader.Name == "xmlns")
                {
                    continue;
                }

                var flagName = reader.LocalName;
                var flagInstance = definition.FlagInstances.FirstOrDefault(f => f.EffectiveName == flagName);

                if (flagInstance?.ResolvedDefinition is not null)
                {
                    var flagNode = new FlagNode(flagName, flagInstance.ResolvedDefinition, assembly)
                    {
                        RawValue = reader.Value,
                        Value = ParseValue(reader.Value, flagInstance.ResolvedDefinition.DataTypeName)
                    };
                    assembly.AddFlag(flagNode);
                }
            }

            // Move back to element
            reader.MoveToElement();
        }

        // Read child elements
        if (!reader.IsEmptyElement)
        {
            var depth = reader.Depth;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth)
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    var childName = reader.LocalName;
                    var childNode = ReadModelChild(reader, childName, definition, assembly);
                    if (childNode is not null)
                    {
                        assembly.AddModelChild(childNode);
                    }
                }
            }
        }

        return assembly;
    }

    private IDocumentNode? ReadModelChild(XmlReader reader, string elementName, AssemblyDefinition parentDefinition, IDocumentNode parent)
    {
        // Look for field or assembly instance in the model
        if (parentDefinition.Model is not null)
        {
            foreach (var element in parentDefinition.Model.Elements)
            {
                if (element is FieldInstance fieldInstance && fieldInstance.EffectiveName == elementName)
                {
                    if (fieldInstance.ResolvedDefinition is not null)
                    {
                        return ReadField(reader, elementName, fieldInstance.ResolvedDefinition, parent);
                    }
                }
                else if (element is AssemblyInstance assemblyInstance && assemblyInstance.EffectiveName == elementName)
                {
                    if (assemblyInstance.ResolvedDefinition is not null)
                    {
                        return ReadAssembly(reader, elementName, assemblyInstance.ResolvedDefinition, parent);
                    }
                }
                else if (element is ChoiceGroup choiceGroup)
                {
                    // Look inside choice group
                    foreach (var choice in choiceGroup.Choices)
                    {
                        if (choice is FieldInstance choiceField && choiceField.EffectiveName == elementName)
                        {
                            if (choiceField.ResolvedDefinition is not null)
                            {
                                return ReadField(reader, elementName, choiceField.ResolvedDefinition, parent);
                            }
                        }
                        else if (choice is AssemblyInstance choiceAssembly && choiceAssembly.EffectiveName == elementName)
                        {
                            if (choiceAssembly.ResolvedDefinition is not null)
                            {
                                return ReadAssembly(reader, elementName, choiceAssembly.ResolvedDefinition, parent);
                            }
                        }
                    }
                }
            }
        }

        // Unknown element - skip it
        reader.Skip();
        return null;
    }

    private FieldNode ReadField(XmlReader reader, string elementName, FieldDefinition definition, IDocumentNode parent)
    {
        var field = new FieldNode(elementName, definition, parent);

        // Read attributes as flags
        if (reader.HasAttributes)
        {
            while (reader.MoveToNextAttribute())
            {
                // Skip namespace declarations
                if (reader.Prefix == "xmlns" || reader.Name == "xmlns")
                {
                    continue;
                }

                var flagName = reader.LocalName;
                var flagInstance = definition.FlagInstances.FirstOrDefault(f => f.EffectiveName == flagName);

                if (flagInstance?.ResolvedDefinition is not null)
                {
                    var flagNode = new FlagNode(flagName, flagInstance.ResolvedDefinition, field)
                    {
                        RawValue = reader.Value,
                        Value = ParseValue(reader.Value, flagInstance.ResolvedDefinition.DataTypeName)
                    };
                    field.AddFlag(flagNode);
                }
            }

            // Move back to element
            reader.MoveToElement();
        }

        // Read field value
        if (!reader.IsEmptyElement)
        {
            var content = reader.ReadInnerXml();
            field.RawValue = content;
            field.Value = ParseValue(content, definition.DataTypeName);
        }

        return field;
    }

    private object? ParseValue(string rawValue, string dataTypeName)
    {
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
