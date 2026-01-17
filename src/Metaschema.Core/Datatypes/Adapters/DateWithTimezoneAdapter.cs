// Licensed under the MIT License.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Metaschema.Core.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "date-with-timezone" data type.
/// A string representing a 24-hour period in a given timezone.
/// </summary>
public sealed partial class DateWithTimezoneAdapter : DataTypeAdapter<DateTimeOffset>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.DateWithTimezone;

    // Date pattern with required timezone
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}(Z|[+-]\d{2}:\d{2})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DateWithTzPattern();

    /// <inheritdoc />
    public override DateTimeOffset Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!DateWithTzPattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid date with timezone (e.g., '2019-09-28Z' or '2019-12-02-08:00')");
        }

        // Parse as date-time at midnight with timezone
        var dateStr = trimmed[..10];
        var tzStr = trimmed[10..];

        if (!DateTimeOffset.TryParseExact($"{dateStr}T00:00:00{tzStr}", "yyyy-MM-ddTHH:mm:ssK",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid date with timezone");
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
        if (!DateWithTzPattern().IsMatch(trimmed))
        {
            result = default;
            return false;
        }

        var dateStr = trimmed[..10];
        var tzStr = trimmed[10..];

        return DateTimeOffset.TryParseExact($"{dateStr}T00:00:00{tzStr}", "yyyy-MM-ddTHH:mm:ssK",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }

    /// <inheritdoc />
    public override string Format(DateTimeOffset value) =>
        value.Offset == TimeSpan.Zero
            ? value.ToString("yyyy-MM-ddZ", CultureInfo.InvariantCulture)
            : $"{value:yyyy-MM-dd}{value:zzz}";
}
