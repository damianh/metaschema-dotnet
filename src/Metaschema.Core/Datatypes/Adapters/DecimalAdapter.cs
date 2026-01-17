// Licensed under the MIT License.

using System.Globalization;

namespace Metaschema.Core.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "decimal" data type.
/// A real number expressed using a whole and optional fractional part separated by a period.
/// </summary>
public sealed class DecimalAdapter : DataTypeAdapter<decimal>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.DecimalType;

    /// <inheritdoc />
    public override decimal Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!decimal.TryParse(trimmed, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var result))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value must be a valid decimal number");
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out decimal result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = 0;
            return false;
        }

        return decimal.TryParse(value.Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out result);
    }

    /// <inheritdoc />
    public override string Format(decimal value) => value.ToString(CultureInfo.InvariantCulture);
}
