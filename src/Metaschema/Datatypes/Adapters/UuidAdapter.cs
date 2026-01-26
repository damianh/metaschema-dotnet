// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "uuid" data type.
/// A version 4 or 5 Universally Unique Identifier as defined by RFC4122.
/// </summary>
public sealed partial class UuidAdapter : DataTypeAdapter<Guid>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.Uuid;

    // Pattern: 8-4-4-4-12 hex digits with version 4 or 5 constraints
    // Version digit (position 13) must be 4 or 5
    // Variant digit (position 17) must be 8, 9, A, B (case insensitive)
    [GeneratedRegex(@"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[45][0-9A-Fa-f]{3}-[89ABab][0-9A-Fa-f]{3}-[0-9A-Fa-f]{12}$",
        RegexOptions.Compiled)]
    private static partial Regex UuidPattern();

    /// <inheritdoc />
    public override Guid Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!UuidPattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid version 4 or 5 UUID in the format xxxxxxxx-xxxx-[45]xxx-[89ab]xxx-xxxxxxxxxxxx");
        }

        return Guid.Parse(trimmed);
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out Guid result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = Guid.Empty;
            return false;
        }

        var trimmed = value.Trim();
        if (!UuidPattern().IsMatch(trimmed))
        {
            result = Guid.Empty;
            return false;
        }

        return Guid.TryParse(trimmed, out result);
    }

    /// <inheritdoc />
    public override string Format(Guid value) => value.ToString("D").ToLowerInvariant();
}
