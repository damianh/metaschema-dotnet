// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Text.RegularExpressions;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "email-address" data type.
/// An email address string formatted according to RFC6531.
/// </summary>
public sealed partial class EmailAddressAdapter : DataTypeAdapter<string>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.EmailAddress;

    // Simple pattern: something @ something
    [GeneratedRegex(@"^.+@.+$", RegexOptions.Compiled)]
    private static partial Regex EmailPattern();

    /// <inheritdoc />
    public override string Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!EmailPattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid email address (containing '@')");
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
        if (!EmailPattern().IsMatch(trimmed))
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
