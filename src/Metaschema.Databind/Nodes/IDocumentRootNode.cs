using Metaschema.Core.Model;

namespace Metaschema.Databind.Nodes;

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
