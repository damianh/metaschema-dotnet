using Metaschema.Model;

namespace Metaschema.Nodes;

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
