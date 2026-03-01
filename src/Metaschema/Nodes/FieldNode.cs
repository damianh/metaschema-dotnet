// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Model;

namespace Metaschema.Nodes;

/// <summary>
/// Represents a field node in a document.
/// </summary>
public sealed class FieldNode : IFieldNode
{
    private readonly List<IDocumentNode> _children = [];
    private readonly Dictionary<string, IFlagNode> _flags = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldNode"/> class.
    /// </summary>
    /// <param name="name">The element name.</param>
    /// <param name="definition">The field definition.</param>
    /// <param name="parent">The parent node.</param>
    public FieldNode(string name, FieldDefinition definition, IDocumentNode? parent)
    {
        Name = name;
        Definition = definition;
        Parent = parent;
    }

    /// <inheritdoc />
    public NodeType NodeType => NodeType.Field;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IDocumentNode? Parent { get; }

    /// <inheritdoc />
    public IReadOnlyList<IDocumentNode> Children => _children;

    /// <inheritdoc />
    public FieldDefinition Definition { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IFlagNode> Flags => _flags;

    /// <inheritdoc />
    public object? Value { get; set; }

    /// <inheritdoc />
    public string? RawValue { get; set; }

    /// <summary>
    /// Adds a flag node.
    /// </summary>
    /// <param name="flag">The flag node to add.</param>
    public void AddFlag(FlagNode flag)
    {
        _flags[flag.Name] = flag;
        _children.Add(flag);
    }
}
