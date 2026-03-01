// Licensed under the MIT License.

using Metaschema.Model;

namespace Metaschema.Nodes;

/// <summary>
/// Represents a document root containing deserialized content.
/// </summary>
public sealed class DocumentNode : IDocumentRootNode
{
    private readonly List<IDocumentNode> _children = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentNode"/> class.
    /// </summary>
    /// <param name="name">The root element name.</param>
    /// <param name="definition">The assembly definition for the root.</param>
    public DocumentNode(string name, AssemblyDefinition definition)
    {
        Name = name;
        Definition = definition;
    }

    /// <inheritdoc />
    public NodeType NodeType => NodeType.Document;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IDocumentNode? Parent => null;

    /// <inheritdoc />
    public IReadOnlyList<IDocumentNode> Children => _children;

    /// <inheritdoc />
    public AssemblyDefinition Definition { get; }

    /// <summary>
    /// Gets or sets the root assembly node.
    /// </summary>
    public AssemblyNode? RootAssembly { get; set; }

    /// <summary>
    /// Adds a child node.
    /// </summary>
    /// <param name="child">The child node to add.</param>
    public void AddChild(IDocumentNode child)
    {
        _children.Add(child);
    }
}