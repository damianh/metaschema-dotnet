// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Markup;

/// <summary>
/// Multi-line markup content.
/// </summary>
/// <remarks>
/// This is a simple wrapper for now. In a later phase, it will be parsed by Markdig
/// to support block-level markup elements like paragraphs, lists, and code blocks.
/// </remarks>
/// <param name="Value">The raw markup string.</param>
public readonly record struct MarkupMultiline(string Value)
{
    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a string to a <see cref="MarkupMultiline"/>.
    /// </summary>
    public static implicit operator MarkupMultiline(string s) => new(s);

    /// <summary>
    /// Implicitly converts a <see cref="MarkupMultiline"/> to a string.
    /// </summary>
    public static implicit operator string(MarkupMultiline m) => m.Value;
}
