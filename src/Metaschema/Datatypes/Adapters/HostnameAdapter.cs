// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Text.RegularExpressions;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "hostname" data type.
/// An internationalized Internet host name string formatted according to RFC5890.
/// </summary>
public sealed partial class HostnameAdapter : DataTypeAdapter<string>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.Hostname;

    // Pattern: non-empty, trimmed (per spec, a better pattern is needed)
    [GeneratedRegex(@"^\S(.*\S)?$", RegexOptions.Compiled)]
    private static partial Regex HostnamePattern();

    /// <inheritdoc />
    public override string Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!HostnamePattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid hostname with no leading or trailing whitespace");
        }

        return trimmed;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out string? result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return false;
        }

        var trimmed = value.Trim();
        if (!HostnamePattern().IsMatch(trimmed))
        {
            result = null;
            return false;
        }

        result = trimmed;
        return true;
    }

    /// <inheritdoc />
    public override string Format(string value) => value;
}
