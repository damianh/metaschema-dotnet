// Licensed under the MIT License.

using Metaschema.Core.Model;

namespace Metaschema.Databind.Nodes;

/// <summary>
/// Represents the type of a document node.
/// </summary>
public enum NodeType
{
    /// <summary>
    /// A document node (root container).
    /// </summary>
    Document,

    /// <summary>
    /// An assembly node (complex composite object).
    /// </summary>
    Assembly,

    /// <summary>
    /// A field node (value container with optional flags).
    /// </summary>
    Field,

    /// <summary>
    /// A flag node (simple named value).
    /// </summary>
    Flag
}

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

/// <summary>
/// Represents a document root node.
/// </summary>
public interface IDocumentRootNode : IDocumentNode
{
    /// <summary>
    /// Gets the assembly definition for this document root.
    /// </summary>
    AssemblyDefinition Definition { get; }
}

/// <summary>
/// Represents an assembly node in a document.
/// </summary>
public interface IAssemblyNode : IDocumentNode
{
    /// <summary>
    /// Gets the assembly definition for this node.
    /// </summary>
    AssemblyDefinition Definition { get; }

    /// <summary>
    /// Gets the flag nodes for this assembly.
    /// </summary>
    IReadOnlyDictionary<string, IFlagNode> Flags { get; }

    /// <summary>
    /// Gets the model child nodes (fields and assemblies).
    /// </summary>
    IReadOnlyList<IDocumentNode> ModelChildren { get; }
}

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

/// <summary>
/// Represents a flag node in a document.
/// </summary>
public interface IFlagNode : IDocumentNode
{
    /// <summary>
    /// Gets the flag definition for this node.
    /// </summary>
    FlagDefinition Definition { get; }

    /// <summary>
    /// Gets the flag value.
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// Gets the raw string value.
    /// </summary>
    string? RawValue { get; }
}
