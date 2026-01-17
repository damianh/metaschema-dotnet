// Licensed under the MIT License.

using Metaschema.Databind.Nodes;

namespace Metaschema.Databind;

/// <summary>
/// Serializes document nodes to a specific format.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Gets the format this serializer produces.
    /// </summary>
    Format Format { get; }

    /// <summary>
    /// Serializes a document node to a stream.
    /// </summary>
    /// <param name="node">The document node to serialize.</param>
    /// <param name="output">The output stream.</param>
    void Serialize(IDocumentRootNode node, Stream output);

    /// <summary>
    /// Serializes a document node to a text writer.
    /// </summary>
    /// <param name="node">The document node to serialize.</param>
    /// <param name="writer">The text writer.</param>
    void Serialize(IDocumentRootNode node, TextWriter writer);

    /// <summary>
    /// Serializes a document node to a string.
    /// </summary>
    /// <param name="node">The document node to serialize.</param>
    /// <returns>The serialized string.</returns>
    string SerializeToString(IDocumentRootNode node);
}
