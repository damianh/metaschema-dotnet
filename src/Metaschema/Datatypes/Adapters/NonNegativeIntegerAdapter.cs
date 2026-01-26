// Licensed under the MIT License.

using System.Globalization;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "non-negative-integer" data type.
/// An integer value that is equal to or greater than 0.
/// </summary>
public sealed class NonNegativeIntegerAdapter : DataTypeAdapter<ulong>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.NonNegativeInteger;

    /// <inheritdoc />
    public override ulong Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!ulong.TryParse(trimmed, out var result))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid non-negative integer");
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out ulong result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = 0;
            return false;
        }

        return ulong.TryParse(value.Trim(), out result);
    }

    /// <inheritdoc />
    public override string Format(ulong value) => value.ToString(CultureInfo.InvariantCulture);
}
