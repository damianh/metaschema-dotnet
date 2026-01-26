// Licensed under the MIT License.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "date-time" data type.
/// A string representing a point in time, optionally qualified by a timezone.
/// </summary>
public sealed partial class DateTimeAdapter : DataTypeAdapter<DateTime>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.DateTime;

    // RFC3339 date-time pattern with optional timezone
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DateTimePattern();

    /// <inheritdoc />
    public override DateTime Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!DateTimePattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid date-time (e.g., '2019-09-28T23:20:50.52' or '2019-09-28T23:20:50Z')");
        }

        if (!DateTime.TryParse(trimmed, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var result))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid date-time");
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out DateTime result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        var trimmed = value.Trim();
        if (!DateTimePattern().IsMatch(trimmed))
        {
            result = default;
            return false;
        }

        return DateTime.TryParse(trimmed, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out result);
    }

    /// <inheritdoc />
    public override string Format(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)
            : value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
}
