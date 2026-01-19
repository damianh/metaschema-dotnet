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
