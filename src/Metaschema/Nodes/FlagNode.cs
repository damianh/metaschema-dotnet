// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Model;

namespace Metaschema.Nodes;

/// <summary>
/// Represents a flag node in a document.
/// </summary>
public sealed class FlagNode : IFlagNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlagNode"/> class.
    /// </summary>
    /// <param name="name">The attribute/property name.</param>
    /// <param name="definition">The flag definition.</param>
    /// <param name="parent">The parent node.</param>
    public FlagNode(string name, FlagDefinition definition, IDocumentNode? parent)
    {
        Name = name;
        Definition = definition;
        Parent = parent;
    }

    /// <inheritdoc />
    public NodeType NodeType => NodeType.Flag;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IDocumentNode? Parent { get; }

    /// <inheritdoc />
    public IReadOnlyList<IDocumentNode> Children => [];

    /// <inheritdoc />
    public FlagDefinition Definition { get; }

    /// <inheritdoc />
    public object? Value { get; set; }

    /// <inheritdoc />
    public string? RawValue { get; set; }
}
