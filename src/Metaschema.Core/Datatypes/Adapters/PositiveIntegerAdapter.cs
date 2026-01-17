// Licensed under the MIT License.

using System.Globalization;

namespace Metaschema.Core.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "positive-integer" data type.
/// An integer value that is greater than 0.
/// </summary>
public sealed class PositiveIntegerAdapter : DataTypeAdapter<ulong>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.PositiveInteger;

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
                "Value must be a valid positive integer");
        }

        if (result == 0)
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be greater than 0");
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

        if (!ulong.TryParse(value.Trim(), out result))
        {
            return false;
        }

        return result > 0;
    }

    /// <inheritdoc />
    public override string Format(ulong value) => value.ToString(CultureInfo.InvariantCulture);
}
