// Licensed under the MIT License.

namespace Metaschema.Markup;

/// <summary>
/// Single-line markup content.
/// </summary>
/// <remarks>
/// This is a simple wrapper for now. In a later phase, it will be parsed by Markdig
/// to support inline markup elements.
/// </remarks>
/// <param name="Value">The raw markup string.</param>
public readonly record struct MarkupLine(string Value)
{
    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a string to a <see cref="MarkupLine"/>.
    /// </summary>
    public static implicit operator MarkupLine(string s) => new(s);

    /// <summary>
    /// Implicitly converts a <see cref="MarkupLine"/> to a string.
    /// </summary>
    public static implicit operator string(MarkupLine m) => m.Value;
}
