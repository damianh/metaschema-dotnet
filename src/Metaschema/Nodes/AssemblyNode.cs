using Metaschema.Model;

namespace Metaschema.Nodes;

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
