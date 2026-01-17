// Licensed under the MIT License.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Metaschema.Core.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "date-time-with-timezone" data type.
/// A string representing a point in time in a given timezone, formatted according to RFC3339.
/// </summary>
public sealed partial class DateTimeWithTimezoneAdapter : DataTypeAdapter<DateTimeOffset>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.DateTimeWithTimezone;

    // RFC3339 date-time pattern with required timezone
    // This is a simplified pattern - the full pattern from the spec is very complex
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DateTimeWithTzPattern();

    /// <inheritdoc />
    public override DateTimeOffset Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!DateTimeWithTzPattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid date-time with timezone (e.g., '2019-09-28T23:20:50.52Z')");
        }

        if (!DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var result))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid date-time with timezone");
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out DateTimeOffset result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        var trimmed = value.Trim();
        if (!DateTimeWithTzPattern().IsMatch(trimmed))
        {
            result = default;
            return false;
        }

        return DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out result);
    }

    /// <inheritdoc />
    public override string Format(DateTimeOffset value) =>
        value.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture);
}
