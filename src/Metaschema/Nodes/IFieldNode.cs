// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Model;

namespace Metaschema.Nodes;

/// <summary>
/// Represents a field node in a document.
/// </summary>
public interface IFieldNode : IDocumentNode
{
    /// <summary>
    /// Gets the field definition for this node.
    /// </summary>
    FieldDefinition Definition { get; }

    /// <summary>
    /// Gets the flag nodes for this field.
    /// </summary>
    IReadOnlyDictionary<string, IFlagNode> Flags { get; }

    /// <summary>
    /// Gets the field value.
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// Gets the raw string value.
    /// </summary>
    string? RawValue { get; }
}
