// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Nodes;

/// <summary>
/// Represents a node in a Metaschema-based document.
/// </summary>
public interface IDocumentNode
{
    /// <summary>
    /// Gets the type of this node.
    /// </summary>
    NodeType NodeType { get; }

    /// <summary>
    /// Gets the name of this node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the parent node, or null if this is the root.
    /// </summary>
    IDocumentNode? Parent { get; }

    /// <summary>
    /// Gets the child nodes.
    /// </summary>
    IReadOnlyList<IDocumentNode> Children { get; }
}