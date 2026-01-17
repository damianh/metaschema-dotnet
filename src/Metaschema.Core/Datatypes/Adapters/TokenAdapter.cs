// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Metaschema.Core.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "token" data type.
/// A non-colonized name (NCName-like pattern).
/// </summary>
public sealed partial class TokenAdapter : DataTypeAdapter<string>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.Token;

    // Pattern: starts with letter or underscore, followed by letters, numbers, dots, hyphens, underscores
    [GeneratedRegex(@"^(\p{L}|_)(\p{L}|\p{N}|[.\-_])*$", RegexOptions.Compiled)]
    private static partial Regex TokenPattern();

    /// <inheritdoc />
    public override string Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!TokenPattern().IsMatch(value))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must start with a letter or underscore and contain only letters, numbers, dots, hyphens, and underscores");
        }

        return value;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out string? result)
    {
        if (value is null || !TokenPattern().IsMatch(value))
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
