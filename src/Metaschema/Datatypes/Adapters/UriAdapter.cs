// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Text.RegularExpressions;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "uri" data type.
/// A universal resource identifier formatted according to RFC3986.
/// Must be an absolute URI with a scheme.
/// </summary>
public sealed partial class UriAdapter : DataTypeAdapter<Uri>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.Uri;

    // Pattern: scheme with colon required
    [GeneratedRegex(@"^[a-zA-Z][a-zA-Z0-9+\-.]+:.+$", RegexOptions.Compiled)]
    private static partial Regex UriPattern();

    /// <inheritdoc />
    public override Uri Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!UriPattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be an absolute URI with a scheme (e.g., 'http:', 'urn:')");
        }

        if (!System.Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value must be a valid absolute URI");
        }

        return uri;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out Uri? result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return false;
        }

        var trimmed = value.Trim();
        if (!UriPattern().IsMatch(trimmed))
        {
            result = null;
            return false;
        }

        return System.Uri.TryCreate(trimmed, UriKind.Absolute, out result);
    }

    /// <inheritdoc />
    public override string Format(Uri value) => value.ToString();
}
