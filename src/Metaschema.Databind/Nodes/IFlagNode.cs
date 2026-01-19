using Metaschema.Core.Model;

namespace Metaschema.Databind.Nodes;

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
