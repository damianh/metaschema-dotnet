// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Metaschema.Core.Metapath.Item;

/// <summary>
/// Represents a node in the Metapath data model.
/// Nodes represent structured content such as assemblies, fields, and flags.
/// </summary>
public interface INodeItem : IItem
{
    /// <summary>
    /// Gets the type of this node.
    /// </summary>
    NodeType NodeType { get; }

    /// <summary>
    /// Gets the name of this node.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the parent node, or <c>null</c> if this is the root.
    /// </summary>
    INodeItem? Parent { get; }

    /// <summary>
    /// Gets the child nodes of this node.
    /// </summary>
    /// <returns>The child nodes.</returns>
    IEnumerable<INodeItem> GetChildren();

    /// <summary>
    /// Gets the flag (attribute) children of this node.
    /// </summary>
    /// <returns>The flag children.</returns>
    IEnumerable<INodeItem> GetFlags();

    /// <summary>
    /// Gets the model (element) children of this node.
    /// </summary>
    /// <returns>The model children.</returns>
    IEnumerable<INodeItem> GetModelItems();

    /// <summary>
    /// Gets a flag by name.
    /// </summary>
    /// <param name="name">The flag name.</param>
    /// <returns>The flag node, or <c>null</c> if not found.</returns>
    INodeItem? GetFlag(string name);

    /// <summary>
    /// Gets the base URI of the document containing this node.
    /// </summary>
    Uri? BaseUri { get; }

    /// <summary>
    /// Gets the document URI of the document containing this node.
    /// </summary>
    Uri? DocumentUri { get; }

    /// <summary>
    /// Gets the path to this node from the document root.
    /// </summary>
    /// <returns>The path as a Metapath expression string.</returns>
    string GetPath();

    /// <summary>
    /// Gets the definition associated with this node, if any.
    /// This may be a <see cref="Model.FlagDefinition"/>, <see cref="Model.FieldDefinition"/>, 
    /// or <see cref="Model.AssemblyDefinition"/>.
    /// </summary>
    object? Definition { get; }
}
