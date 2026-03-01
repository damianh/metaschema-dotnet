// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Text.RegularExpressions;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "string" data type.
/// A non-empty string of unicode characters with leading and trailing whitespace disallowed.
/// </summary>
public sealed partial class StringAdapter : DataTypeAdapter<string>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.StringType;

    // Pattern: non-empty, no leading/trailing whitespace
    [GeneratedRegex(@"^\S(.*\S)?$", RegexOptions.Compiled)]
    private static partial Regex StringPattern();

    /// <inheritdoc />
    public override string Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!StringPattern().IsMatch(value))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be non-empty with no leading or trailing whitespace");
        }

        return value;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out string? result)
    {
        if (value is null || !StringPattern().IsMatch(value))
        {
            result = null;
            return false;
        }

        result = value;
        return true;
    }

    /// <inheritdoc />
    public override string Format(string value) => value;
}
