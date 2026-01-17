// Licensed under the MIT License.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Metaschema.Core.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "date" data type.
/// A string representing a 24-hour period, optionally qualified by a timezone.
/// </summary>
public sealed partial class DateAdapter : DataTypeAdapter<DateOnly>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.Date;

    // Date pattern with optional timezone
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}(Z|[+-]\d{2}:\d{2})?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DatePattern();

    /// <inheritdoc />
    public override DateOnly Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!DatePattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid date (e.g., '2019-09-28' or '2019-09-28Z')");
        }

        // Extract just the date portion (ignore timezone for DateOnly)
        var dateStr = trimmed.Length > 10 ? trimmed[..10] : trimmed;

        if (!DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var result))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid date");
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out DateOnly result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        var trimmed = value.Trim();
        if (!DatePattern().IsMatch(trimmed))
        {
            result = default;
            return false;
        }

        var dateStr = trimmed.Length > 10 ? trimmed[..10] : trimmed;

        return DateOnly.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out result);
    }

    /// <inheritdoc />
    public override string Format(DateOnly value) =>
        value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
