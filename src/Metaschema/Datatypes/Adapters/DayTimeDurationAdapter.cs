// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "day-time-duration" data type.
/// An amount of time quantified in days, hours, minutes, and seconds based on ISO-8601 durations.
/// </summary>
public sealed partial class DayTimeDurationAdapter : DataTypeAdapter<TimeSpan>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.DayTimeDuration;

    // ISO 8601 day-time duration pattern
    [GeneratedRegex(@"^-?P((\d+D(T((\d+H(\d+M)?((\d+|\d+(\.\d+)?)S)?)|(\d+M((\d+|\d+(\.\d+)?)S)?)|(\d+|\d+(\.\d+)?)S))?)|T((\d+H(\d+M)?((\d+|\d+(\.\d+)?)S)?)|(\d+M((\d+|\d+(\.\d+)?)S)?)|(\d+|\d+(\.\d+)?)S))$",
        RegexOptions.Compiled)]
    private static partial Regex DayTimeDurationPattern();

    /// <inheritdoc />
    public override TimeSpan Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!DayTimeDurationPattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid day-time duration (e.g., 'P1DT12H45M' or '-PT3H')");
        }

        return ParseDuration(trimmed);
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out TimeSpan result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        var trimmed = value.Trim();
        if (!DayTimeDurationPattern().IsMatch(trimmed))
        {
            result = default;
            return false;
        }

        try
        {
            result = ParseDuration(trimmed);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    private static TimeSpan ParseDuration(string value)
    {
        var negative = value.StartsWith('-');
        var remaining = negative ? value[2..] : value[1..]; // Skip -?P

        var days = 0;
        var hours = 0;
        var minutes = 0;
        var seconds = 0.0;

        // Parse day part if present
        var tIndex = remaining.IndexOf('T');
        if (tIndex > 0)
        {
            var dayPart = remaining[..tIndex];
            if (dayPart.EndsWith('D'))
            {
                days = int.Parse(dayPart[..^1], CultureInfo.InvariantCulture);
            }
            remaining = remaining[(tIndex + 1)..];
        }
        else if (remaining.Contains('D'))
        {
            var dIndex = remaining.IndexOf('D');
            days = int.Parse(remaining[..dIndex], CultureInfo.InvariantCulture);
            remaining = remaining[(dIndex + 1)..];
            if (remaining.StartsWith('T'))
            {
                remaining = remaining[1..];
            }
        }
        else if (remaining.StartsWith('T'))
        {
            remaining = remaining[1..];
        }

        // Parse time parts
        if (remaining.Length > 0)
        {
            var hIndex = remaining.IndexOf('H');
            if (hIndex >= 0)
            {
                hours = int.Parse(remaining[..hIndex], CultureInfo.InvariantCulture);
                remaining = remaining[(hIndex + 1)..];
            }

            var mIndex = remaining.IndexOf('M');
            if (mIndex >= 0)
            {
                minutes = int.Parse(remaining[..mIndex], CultureInfo.InvariantCulture);
                remaining = remaining[(mIndex + 1)..];
            }

            var sIndex = remaining.IndexOf('S');
            if (sIndex >= 0)
            {
                seconds = double.Parse(remaining[..sIndex], CultureInfo.InvariantCulture);
            }
        }

        var result = new TimeSpan(days, hours, minutes, 0) + TimeSpan.FromSeconds(seconds);
        return negative ? -result : result;
    }

    /// <inheritdoc />
    public override string Format(TimeSpan value)
    {
        var negative = value < TimeSpan.Zero;
        if (negative)
        {
            value = -value;
        }

        var parts = new System.Text.StringBuilder();
        parts.Append(negative ? "-P" : "P");

        if (value.Days > 0)
        {
            parts.Append(CultureInfo.InvariantCulture, $"{value.Days}D");
        }

        if (value.Hours > 0 || value.Minutes > 0 || value.Seconds > 0 || value.Milliseconds > 0)
        {
            parts.Append('T');
            if (value.Hours > 0)
            {
                parts.Append(CultureInfo.InvariantCulture, $"{value.Hours}H");
            }
            if (value.Minutes > 0)
            {
                parts.Append(CultureInfo.InvariantCulture, $"{value.Minutes}M");
            }
            if (value.Seconds > 0 || value.Milliseconds > 0)
            {
                var totalSeconds = value.Seconds + value.Milliseconds / 1000.0;
                parts.Append(totalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
                parts.Append('S');
            }
        }

        // Handle zero duration
        if (parts.Length == 2 || parts.Length == 1)
        {
            parts.Append("T0S");
        }

        return parts.ToString();
    }
}
