// Licensed under the MIT License.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Represents a year-month duration as defined by ISO 8601.
/// </summary>
/// <param name="Years">The number of years.</param>
/// <param name="Months">The number of months.</param>
/// <param name="IsNegative">Whether the duration is negative.</param>
public readonly record struct YearMonthDuration(int Years, int Months, bool IsNegative = false)
{
    /// <summary>
    /// Gets the total months represented by this duration.
    /// </summary>
    public int TotalMonths => (IsNegative ? -1 : 1) * (Years * 12 + Months);

    /// <inheritdoc />
    public override string ToString()
    {
        var prefix = IsNegative ? "-P" : "P";
        if (Years > 0 && Months > 0)
        {
            return $"{prefix}{Years}Y{Months}M";
        }
        if (Years > 0)
        {
            return $"{prefix}{Years}Y";
        }
        return $"{prefix}{Months}M";
    }
}

/// <summary>
/// Adapter for the Metaschema "year-month-duration" data type.
/// An amount of time quantified in years and months based on ISO-8601 durations.
/// </summary>
public sealed partial class YearMonthDurationAdapter : DataTypeAdapter<YearMonthDuration>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.YearMonthDuration;

    // ISO 8601 year-month duration pattern
    [GeneratedRegex(@"^-?P((\d+Y(\d+M)?)|(\d+M))$", RegexOptions.Compiled)]
    private static partial Regex YearMonthDurationPattern();

    /// <inheritdoc />
    public override YearMonthDuration Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!YearMonthDurationPattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid year-month duration (e.g., 'P1Y6M' or '-P9M')");
        }

        var negative = trimmed.StartsWith('-');
        var remaining = negative ? trimmed[2..] : trimmed[1..]; // Skip -?P

        var years = 0;
        var months = 0;

        var yIndex = remaining.IndexOf('Y');
        if (yIndex >= 0)
        {
            years = int.Parse(remaining[..yIndex], CultureInfo.InvariantCulture);
            remaining = remaining[(yIndex + 1)..];
        }

        var mIndex = remaining.IndexOf('M');
        if (mIndex >= 0)
        {
            months = int.Parse(remaining[..mIndex], CultureInfo.InvariantCulture);
        }

        return new YearMonthDuration(years, months, negative);
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out YearMonthDuration result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        var trimmed = value.Trim();
        if (!YearMonthDurationPattern().IsMatch(trimmed))
        {
            result = default;
            return false;
        }

        try
        {
            result = Parse(trimmed);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc />
    public override string Format(YearMonthDuration value) => value.ToString();
}
