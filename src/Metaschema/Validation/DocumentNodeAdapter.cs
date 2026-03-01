// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Metapath.Item;
using Metaschema.Nodes;
using DatabindNodeType = Metaschema.Nodes.NodeType;
using MetapathNodeType = Metaschema.Metapath.Item.NodeType;

namespace Metaschema.Validation;

/// <summary>
/// Adapts an <see cref="IDocumentNode"/> to implement <see cref="INodeItem"/>
/// for use with constraint validation.
/// </summary>
public sealed class DocumentNodeAdapter : INodeItem
{
    private readonly IDocumentNode _node;
    private readonly DocumentNodeAdapter? _parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentNodeAdapter"/> class.
    /// </summary>
    /// <param name="node">The document node to wrap.</param>
    /// <param name="parent">The parent adapter, if any.</param>
    public DocumentNodeAdapter(IDocumentNode node, DocumentNodeAdapter? parent = null)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        _parent = parent;
    }

    /// <summary>
    /// Creates an adapter for a document node tree.
    /// </summary>
    /// <param name="node">The document node to adapt.</param>
    /// <returns>The adapted node item.</returns>
    public static INodeItem Adapt(IDocumentNode node) => new DocumentNodeAdapter(node);

    /// <inheritdoc />
    public MetapathNodeType NodeType => _node.NodeType switch
    {
        DatabindNodeType.Document => MetapathNodeType.Document,
        DatabindNodeType.Assembly => MetapathNodeType.Assembly,
        DatabindNodeType.Field => MetapathNodeType.Field,
        DatabindNodeType.Flag => MetapathNodeType.Flag,
        _ => MetapathNodeType.Document
    };

    /// <inheritdoc />
    public string? Name => _node.Name;

    /// <inheritdoc />
    public INodeItem? Parent => _parent;

    /// <inheritdoc />
    public Uri? BaseUri => null;

    /// <inheritdoc />
    public Uri? DocumentUri => null;

    /// <inheritdoc />
    public string? NamespaceUri => null;

    /// <inheritdoc />
    public object? Definition => _node switch
    {
        IDocumentRootNode root => root.Definition,
        IAssemblyNode assembly => assembly.Definition,
        IFieldNode fieldNode => fieldNode.Definition,
        IFlagNode flag => flag.Definition,
        _ => null
    };

    /// <inheritdoc />
    public IEnumerable<INodeItem> GetChildren()
    {
        foreach (var child in _node.Children)
        {
            yield return new DocumentNodeAdapter(child, this);
        }
    }

    /// <inheritdoc />
    public IEnumerable<INodeItem> GetFlags()
    {
        var flags = _node switch
        {
            IAssemblyNode assembly => assembly.Flags.Values,
            IFieldNode fieldNode => fieldNode.Flags.Values,
            _ => Enumerable.Empty<IFlagNode>()
        };

        foreach (var flag in flags)
        {
            yield return new DocumentNodeAdapter(flag, this);
        }
    }

    /// <inheritdoc />
    public IEnumerable<INodeItem> GetModelItems()
    {
        var modelItems = _node switch
        {
            IAssemblyNode assembly => assembly.ModelChildren,
            IDocumentRootNode root => root.Children,
            _ => Enumerable.Empty<IDocumentNode>()
        };

        foreach (var item in modelItems)
        {
            yield return new DocumentNodeAdapter(item, this);
        }
    }

    /// <inheritdoc />
    public INodeItem? GetFlag(string name)
    {
        var flag = _node switch
        {
            IAssemblyNode assembly when assembly.Flags.TryGetValue(name, out var f) => f,
            IFieldNode fieldNode when fieldNode.Flags.TryGetValue(name, out var f) => f,
            _ => null
        };

        return flag is not null ? new DocumentNodeAdapter(flag, this) : null;
    }

    /// <inheritdoc />
    public string GetPath()
    {
        var parts = new List<string>();
        INodeItem? current = this;

        while (current is not null)
        {
            if (current.Name is not null)
            {
                parts.Add(current.Name);
            }
            current = current.Parent;
        }

        parts.Reverse();
        return "/" + string.Join("/", parts);
    }

    /// <inheritdoc />
    public string GetStringValue() => _node switch
    {
        IFieldNode fieldNode => fieldNode.RawValue ?? fieldNode.Value?.ToString() ?? string.Empty,
        IFlagNode flag => flag.RawValue ?? flag.Value?.ToString() ?? string.Empty,
        _ => string.Empty
    };

    /// <inheritdoc />
    public object? GetTypedValue() => _node switch
    {
        IFieldNode fieldNode => fieldNode.Value,
        IFlagNode flag => flag.Value,
        _ => null
    };

    /// <inheritdoc />
    public bool GetEffectiveBooleanValue()
    {
        var stringValue = GetStringValue();
        return !string.IsNullOrEmpty(stringValue);
    }

    /// <summary>
    /// Gets the underlying document node.
    /// </summary>
    public IDocumentNode UnderlyingNode => _node;
}
