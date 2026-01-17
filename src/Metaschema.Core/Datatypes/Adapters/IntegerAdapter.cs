// Licensed under the MIT License.

using System.Globalization;

namespace Metaschema.Core.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "integer" data type.
/// A whole number value.
/// </summary>
public sealed class IntegerAdapter : DataTypeAdapter<long>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.IntegerType;

    /// <inheritdoc />
    public override long Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!long.TryParse(trimmed, out var result))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value must be a valid integer");
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out long result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = 0;
            return false;
        }

        return long.TryParse(value.Trim(), out result);
    }

    /// <inheritdoc />
    public override string Format(long value) => value.ToString(CultureInfo.InvariantCulture);
}
