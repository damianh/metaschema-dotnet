// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Nodes;

namespace Metaschema;

/// <summary>
/// Deserializes content from a specific format into document nodes.
/// </summary>
public interface IDeserializer
{
    /// <summary>
    /// Gets the format this deserializer reads.
    /// </summary>
    Format Format { get; }

    /// <summary>
    /// Deserializes content from a stream.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <returns>The deserialized document node.</returns>
    DocumentNode Deserialize(Stream input);

    /// <summary>
    /// Deserializes content from a text reader.
    /// </summary>
    /// <param name="reader">The text reader.</param>
    /// <returns>The deserialized document node.</returns>
    DocumentNode Deserialize(TextReader reader);

    /// <summary>
    /// Deserializes content from a string.
    /// </summary>
    /// <param name="content">The string content.</param>
    /// <returns>The deserialized document node.</returns>
    DocumentNode Deserialize(string content);
}
