// Licensed under the MIT License.

using Metaschema.Core.Model;

namespace Metaschema.Databind.Nodes;

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

/// <summary>
/// Represents an assembly node in a document.
/// </summary>
public sealed class AssemblyNode : IAssemblyNode
{
    private readonly List<IDocumentNode> _children = [];
    private readonly List<IDocumentNode> _modelChildren = [];
    private readonly Dictionary<string, IFlagNode> _flags = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyNode"/> class.
    /// </summary>
    /// <param name="name">The element name.</param>
    /// <param name="definition">The assembly definition.</param>
    /// <param name="parent">The parent node.</param>
    public AssemblyNode(string name, AssemblyDefinition definition, IDocumentNode? parent)
    {
        Name = name;
        Definition = definition;
        Parent = parent;
    }

    /// <inheritdoc />
    public NodeType NodeType => NodeType.Assembly;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IDocumentNode? Parent { get; }

    /// <inheritdoc />
    public IReadOnlyList<IDocumentNode> Children => _children;

    /// <inheritdoc />
    public AssemblyDefinition Definition { get; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IFlagNode> Flags => _flags;

    /// <inheritdoc />
    public IReadOnlyList<IDocumentNode> ModelChildren => _modelChildren;

    /// <summary>
    /// Adds a flag node.
    /// </summary>
    /// <param name="flag">The flag node to add.</param>
    public void AddFlag(FlagNode flag)
    {
        _flags[flag.Name] = flag;
        _children.Add(flag);
    }

    /// <summary>
    /// Adds a model child node (field or assembly).
    /// </summary>
    /// <param name="child">The child node to add.</param>
    public void AddModelChild(IDocumentNode child)
    {
        _modelChildren.Add(child);
        _children.Add(child);
    }
}

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
