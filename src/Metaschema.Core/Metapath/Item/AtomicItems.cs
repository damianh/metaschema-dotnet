// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

namespace Metaschema.Core.Metapath.Item;

/// <summary>
/// Abstract base class for atomic items.
/// </summary>
/// <typeparam name="T">The underlying .NET type.</typeparam>
public abstract class AtomicItem<T> : IAtomicItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AtomicItem{T}"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    protected AtomicItem(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    public T Value { get; }

    /// <inheritdoc/>
    object IAtomicItem.Value => Value!;

    /// <inheritdoc/>
    public abstract string TypeName { get; }

    /// <inheritdoc/>
    public object? GetTypedValue() => Value;

    /// <inheritdoc/>
    public abstract string GetStringValue();

    /// <inheritdoc/>
    public abstract bool GetEffectiveBooleanValue();

    /// <inheritdoc/>
    public override string ToString() => GetStringValue();

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is AtomicItem<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
}

/// <summary>
/// Represents a string atomic value.
/// </summary>
public sealed class StringItem : AtomicItem<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringItem"/> class.
    /// </summary>
    /// <param name="value">The string value.</param>
    public StringItem(string value) : base(value ?? string.Empty) { }

    /// <inheritdoc/>
    public override string TypeName => "string";

    /// <inheritdoc/>
    public override string GetStringValue() => Value;

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => !string.IsNullOrEmpty(Value);

    /// <summary>
    /// Creates a new string item from the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A new string item.</returns>
    public static StringItem Of(string value) => new(value);
}

/// <summary>
/// Represents a boolean atomic value.
/// </summary>
public sealed class BooleanItem : AtomicItem<bool>
{
    /// <summary>
    /// The true value.
    /// </summary>
    public static readonly BooleanItem True = new(true);

    /// <summary>
    /// The false value.
    /// </summary>
    public static readonly BooleanItem False = new(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanItem"/> class.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public BooleanItem(bool value) : base(value) { }

    /// <inheritdoc/>
    public override string TypeName => "boolean";

    /// <inheritdoc/>
    public override string GetStringValue() => Value ? "true" : "false";

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => Value;

    /// <summary>
    /// Gets the boolean item for the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The boolean item.</returns>
    public static BooleanItem Of(bool value) => value ? True : False;
}

/// <summary>
/// Represents an integer atomic value.
/// </summary>
public sealed class IntegerItem : AtomicItem<long>
{
    /// <summary>
    /// The zero value.
    /// </summary>
    public static readonly IntegerItem Zero = new(0);

    /// <summary>
    /// The one value.
    /// </summary>
    public static readonly IntegerItem One = new(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerItem"/> class.
    /// </summary>
    /// <param name="value">The integer value.</param>
    public IntegerItem(long value) : base(value) { }

    /// <inheritdoc/>
    public override string TypeName => "integer";

    /// <inheritdoc/>
    public override string GetStringValue() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => Value != 0;

    /// <summary>
    /// Creates a new integer item from the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A new integer item.</returns>
    public static IntegerItem Of(long value) => value == 0 ? Zero : value == 1 ? One : new IntegerItem(value);
}

/// <summary>
/// Represents a decimal atomic value.
/// </summary>
public sealed class DecimalItem : AtomicItem<decimal>
{
    /// <summary>
    /// The zero value.
    /// </summary>
    public static readonly DecimalItem Zero = new(0m);

    /// <summary>
    /// Initializes a new instance of the <see cref="DecimalItem"/> class.
    /// </summary>
    /// <param name="value">The decimal value.</param>
    public DecimalItem(decimal value) : base(value) { }

    /// <inheritdoc/>
    public override string TypeName => "decimal";

    /// <inheritdoc/>
    public override string GetStringValue() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => Value != 0m;

    /// <summary>
    /// Creates a new decimal item from the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A new decimal item.</returns>
    public static DecimalItem Of(decimal value) => value == 0m ? Zero : new DecimalItem(value);
}

/// <summary>
/// Represents a double-precision floating-point atomic value.
/// </summary>
public sealed class DoubleItem : AtomicItem<double>
{
    /// <summary>
    /// The zero value.
    /// </summary>
    public static readonly DoubleItem Zero = new(0.0);

    /// <summary>
    /// The NaN value.
    /// </summary>
    public static readonly DoubleItem NaN = new(double.NaN);

    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleItem"/> class.
    /// </summary>
    /// <param name="value">The double value.</param>
    public DoubleItem(double value) : base(value) { }

    /// <inheritdoc/>
    public override string TypeName => "double";

    /// <inheritdoc/>
    public override string GetStringValue()
    {
        if (double.IsNaN(Value)) return "NaN";
        if (double.IsPositiveInfinity(Value)) return "INF";
        if (double.IsNegativeInfinity(Value)) return "-INF";
        return Value.ToString("G", CultureInfo.InvariantCulture);
    }

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => !double.IsNaN(Value) && Value != 0.0;

    /// <summary>
    /// Creates a new double item from the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A new double item.</returns>
    public static DoubleItem Of(double value)
    {
        if (double.IsNaN(value)) return NaN;
        if (value == 0.0) return Zero;
        return new DoubleItem(value);
    }
}

/// <summary>
/// Represents a dateTime atomic value.
/// </summary>
public sealed class DateTimeItem : AtomicItem<DateTimeOffset>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeItem"/> class.
    /// </summary>
    /// <param name="value">The dateTime value.</param>
    public DateTimeItem(DateTimeOffset value) : base(value) { }

    /// <inheritdoc/>
    public override string TypeName => "dateTime";

    /// <inheritdoc/>
    public override string GetStringValue() => Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => true;

    /// <summary>
    /// Creates a new dateTime item from the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A new dateTime item.</returns>
    public static DateTimeItem Of(DateTimeOffset value) => new(value);

    /// <summary>
    /// Tries to parse a dateTime string.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="result">The parsed result.</param>
    /// <returns>True if parsing succeeded.</returns>
    public static bool TryParse(string s, out DateTimeItem? result)
    {
        if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value))
        {
            result = new DateTimeItem(value);
            return true;
        }
        result = null;
        return false;
    }
}

/// <summary>
/// Represents a date atomic value.
/// </summary>
public sealed class DateItem : AtomicItem<DateOnly>
{
    /// <summary>
    /// Gets the timezone offset, if present.
    /// </summary>
    public TimeSpan? Timezone { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DateItem"/> class.
    /// </summary>
    /// <param name="value">The date value.</param>
    /// <param name="timezone">The optional timezone.</param>
    public DateItem(DateOnly value, TimeSpan? timezone = null) : base(value)
    {
        Timezone = timezone;
    }

    /// <inheritdoc/>
    public override string TypeName => "date";

    /// <inheritdoc/>
    public override string GetStringValue()
    {
        var dateStr = Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (Timezone.HasValue)
        {
            var tz = Timezone.Value;
            if (tz == TimeSpan.Zero)
                return dateStr + "Z";
            var sign = tz < TimeSpan.Zero ? "-" : "+";
            return dateStr + sign + tz.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        }
        return dateStr;
    }

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => true;

    /// <summary>
    /// Creates a new date item from the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="timezone">The optional timezone.</param>
    /// <returns>A new date item.</returns>
    public static DateItem Of(DateOnly value, TimeSpan? timezone = null) => new(value, timezone);

    /// <summary>
    /// Tries to parse a date string.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="result">The parsed result.</param>
    /// <returns>True if parsing succeeded.</returns>
    public static bool TryParse(string s, out DateItem? result)
    {
        // Try parsing with timezone
        if (DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            result = new DateItem(DateOnly.FromDateTime(dto.DateTime), dto.Offset);
            return true;
        }
        // Try parsing without timezone
        if (DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            result = new DateItem(date);
            return true;
        }
        result = null;
        return false;
    }
}

/// <summary>
/// Represents a time atomic value.
/// </summary>
public sealed class TimeItem : AtomicItem<TimeOnly>
{
    /// <summary>
    /// Gets the timezone offset, if present.
    /// </summary>
    public TimeSpan? Timezone { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeItem"/> class.
    /// </summary>
    /// <param name="value">The time value.</param>
    /// <param name="timezone">The optional timezone.</param>
    public TimeItem(TimeOnly value, TimeSpan? timezone = null) : base(value)
    {
        Timezone = timezone;
    }

    /// <inheritdoc/>
    public override string TypeName => "time";

    /// <inheritdoc/>
    public override string GetStringValue()
    {
        var timeStr = Value.ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture);
        if (Timezone.HasValue)
        {
            var tz = Timezone.Value;
            if (tz == TimeSpan.Zero)
                return timeStr + "Z";
            var sign = tz < TimeSpan.Zero ? "-" : "+";
            return timeStr + sign + tz.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        }
        return timeStr;
    }

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => true;

    /// <summary>
    /// Creates a new time item from the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="timezone">The optional timezone.</param>
    /// <returns>A new time item.</returns>
    public static TimeItem Of(TimeOnly value, TimeSpan? timezone = null) => new(value, timezone);

    /// <summary>
    /// Tries to parse a time string.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="result">The parsed result.</param>
    /// <returns>True if parsing succeeded.</returns>
    public static bool TryParse(string s, out TimeItem? result)
    {
        // Try parsing with timezone
        if (DateTimeOffset.TryParse("1970-01-01T" + s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            result = new TimeItem(TimeOnly.FromDateTime(dto.DateTime), dto.Offset);
            return true;
        }
        // Try parsing without timezone
        if (TimeOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
        {
            result = new TimeItem(time);
            return true;
        }
        result = null;
        return false;
    }
}

/// <summary>
/// Represents a duration atomic value (dayTimeDuration or yearMonthDuration).
/// </summary>
public sealed class DurationItem : AtomicItem<TimeSpan>
{
    /// <summary>
    /// Gets the number of months (for yearMonthDuration).
    /// </summary>
    public int Months { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DurationItem"/> class.
    /// </summary>
    /// <param name="value">The time span value.</param>
    /// <param name="months">The number of months.</param>
    public DurationItem(TimeSpan value, int months = 0) : base(value)
    {
        Months = months;
    }

    /// <inheritdoc/>
    public override string TypeName => "duration";

    /// <inheritdoc/>
    public override string GetStringValue()
    {
        var sb = new System.Text.StringBuilder();
        var isNegative = Value < TimeSpan.Zero || Months < 0;
        if (isNegative) sb.Append('-');
        sb.Append('P');

        var absMonths = Math.Abs(Months);
        var years = absMonths / 12;
        var months = absMonths % 12;
        var span = Value.Duration();

        if (years > 0) sb.Append(years).Append('Y');
        if (months > 0) sb.Append(months).Append('M');
        if (span.Days > 0) sb.Append(span.Days).Append('D');

        if (span.Hours > 0 || span.Minutes > 0 || span.Seconds > 0 || span.Milliseconds > 0)
        {
            sb.Append('T');
            if (span.Hours > 0) sb.Append(span.Hours).Append('H');
            if (span.Minutes > 0) sb.Append(span.Minutes).Append('M');
            if (span.Seconds > 0 || span.Milliseconds > 0)
            {
                if (span.Milliseconds > 0)
                    sb.Append(span.Seconds).Append('.').Append(span.Milliseconds.ToString("D3", CultureInfo.InvariantCulture).TrimEnd('0')).Append('S');
                else
                    sb.Append(span.Seconds).Append('S');
            }
        }

        if (sb.Length == 1 || (sb.Length == 2 && isNegative))
            sb.Append("T0S");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => Value != TimeSpan.Zero || Months != 0;

    /// <summary>
    /// Creates a new duration item from the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="months">The number of months.</param>
    /// <returns>A new duration item.</returns>
    public static DurationItem Of(TimeSpan value, int months = 0) => new(value, months);

    /// <summary>
    /// Creates a dayTimeDuration item.
    /// </summary>
    public static DurationItem DayTime(TimeSpan value) => new(value, 0);

    /// <summary>
    /// Creates a yearMonthDuration item.
    /// </summary>
    public static DurationItem YearMonth(int years, int months) => new(TimeSpan.Zero, years * 12 + months);

    /// <summary>
    /// Tries to parse an ISO 8601 duration string.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="result">The parsed result.</param>
    /// <returns>True if parsing succeeded.</returns>
    public static bool TryParse(string s, out DurationItem? result)
    {
        result = null;
        if (string.IsNullOrEmpty(s)) return false;

        var isNegative = s.StartsWith('-');
        var input = isNegative ? s[1..] : s;
        if (!input.StartsWith('P')) return false;
        input = input[1..];

        var months = 0;
        var timeSpan = TimeSpan.Zero;
        var inTime = false;

        var numStart = 0;
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == 'T')
            {
                inTime = true;
                numStart = i + 1;
            }
            else if (char.IsLetter(c))
            {
                if (!int.TryParse(input[numStart..i], CultureInfo.InvariantCulture, out var num))
                {
                    if (!double.TryParse(input[numStart..i], CultureInfo.InvariantCulture, out var dnum))
                        return false;
                    num = (int)dnum;
                    // Handle fractional seconds
                    if (c == 'S')
                    {
                        timeSpan += TimeSpan.FromSeconds(dnum);
                        numStart = i + 1;
                        continue;
                    }
                }

                if (!inTime)
                {
                    switch (c)
                    {
                        case 'Y': months += num * 12; break;
                        case 'M': months += num; break;
                        case 'D': timeSpan += TimeSpan.FromDays(num); break;
                        default: return false;
                    }
                }
                else
                {
                    switch (c)
                    {
                        case 'H': timeSpan += TimeSpan.FromHours(num); break;
                        case 'M': timeSpan += TimeSpan.FromMinutes(num); break;
                        case 'S': timeSpan += TimeSpan.FromSeconds(num); break;
                        default: return false;
                    }
                }
                numStart = i + 1;
            }
        }

        if (isNegative)
        {
            timeSpan = -timeSpan;
            months = -months;
        }

        result = new DurationItem(timeSpan, months);
        return true;
    }
}

/// <summary>
/// Represents a dayTimeDuration atomic value.
/// </summary>
public sealed class DayTimeDurationItem : AtomicItem<TimeSpan>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DayTimeDurationItem"/> class.
    /// </summary>
    /// <param name="value">The time span value.</param>
    public DayTimeDurationItem(TimeSpan value) : base(value) { }

    /// <inheritdoc/>
    public override string TypeName => "dayTimeDuration";

    /// <inheritdoc/>
    public override string GetStringValue()
    {
        var span = Value;
        var isNegative = span < TimeSpan.Zero;
        if (isNegative) span = span.Negate();

        var sb = new System.Text.StringBuilder();
        if (isNegative) sb.Append('-');
        sb.Append('P');
        if (span.Days > 0) sb.Append(span.Days).Append('D');
        if (span.Hours > 0 || span.Minutes > 0 || span.Seconds > 0 || span.Milliseconds > 0)
        {
            sb.Append('T');
            if (span.Hours > 0) sb.Append(span.Hours).Append('H');
            if (span.Minutes > 0) sb.Append(span.Minutes).Append('M');
            if (span.Seconds > 0 || span.Milliseconds > 0)
            {
                var secs = span.Seconds + span.Milliseconds / 1000.0;
                sb.Append(secs.ToString("0.###", CultureInfo.InvariantCulture)).Append('S');
            }
        }
        if (sb.Length <= 2) sb.Append("T0S");
        return sb.ToString();
    }

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => Value != TimeSpan.Zero;

    /// <summary>
    /// Creates a new dayTimeDuration item.
    /// </summary>
    public static DayTimeDurationItem Of(TimeSpan value) => new(value);
}

/// <summary>
/// Represents an array item (XPath 3.1 array).
/// </summary>
public sealed class ArrayItem : IItem
{
    private readonly IReadOnlyList<ISequence> _members;

    /// <summary>
    /// Gets an empty array.
    /// </summary>
    public static readonly ArrayItem Empty = new([]);

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayItem"/> class.
    /// </summary>
    /// <param name="members">The array members.</param>
    public ArrayItem(IReadOnlyList<ISequence> members)
    {
        _members = members;
    }

    /// <summary>
    /// Gets the members of the array.
    /// </summary>
    public IReadOnlyList<ISequence> Members => _members;

    /// <summary>
    /// Gets the size of the array.
    /// </summary>
    public int Size => _members.Count;

    /// <summary>
    /// Gets a member at the specified 1-based index.
    /// </summary>
    public ISequence Get(int index) => index >= 1 && index <= _members.Count ? _members[index - 1] : Sequence.Empty;

    /// <inheritdoc/>
    public object? GetTypedValue() => _members;

    /// <inheritdoc/>
    public string GetStringValue() => $"[array of {_members.Count} items]";

    /// <inheritdoc/>
    public bool GetEffectiveBooleanValue() => _members.Count > 0;

    /// <summary>
    /// Creates a new array item.
    /// </summary>
    public static ArrayItem Of(IReadOnlyList<ISequence> members) => members.Count == 0 ? Empty : new ArrayItem(members);

    /// <summary>
    /// Creates a new array item from items.
    /// </summary>
    public static ArrayItem Of(IEnumerable<IItem> items) =>
        new(items.Select(i => Sequence.Of(i)).ToList());
}

/// <summary>
/// Represents a map item (XPath 3.1 map).
/// </summary>
public sealed class MapItem : IItem
{
    private readonly IReadOnlyDictionary<IAtomicItem, ISequence> _entries;

    /// <summary>
    /// Gets an empty map.
    /// </summary>
    public static readonly MapItem Empty = new(new Dictionary<IAtomicItem, ISequence>());

    /// <summary>
    /// Initializes a new instance of the <see cref="MapItem"/> class.
    /// </summary>
    /// <param name="entries">The map entries.</param>
    public MapItem(IReadOnlyDictionary<IAtomicItem, ISequence> entries)
    {
        _entries = entries;
    }

    /// <summary>
    /// Gets the entries of the map.
    /// </summary>
    public IReadOnlyDictionary<IAtomicItem, ISequence> Entries => _entries;

    /// <summary>
    /// Gets the size of the map.
    /// </summary>
    public int Size => _entries.Count;

    /// <summary>
    /// Gets the keys of the map.
    /// </summary>
    public IEnumerable<IAtomicItem> Keys => _entries.Keys;

    /// <summary>
    /// Gets a value by key.
    /// </summary>
    public ISequence Get(IAtomicItem key) => _entries.TryGetValue(key, out var value) ? value : Sequence.Empty;

    /// <summary>
    /// Checks if the map contains a key.
    /// </summary>
    public bool ContainsKey(IAtomicItem key) => _entries.ContainsKey(key);

    /// <inheritdoc/>
    public object? GetTypedValue() => _entries;

    /// <inheritdoc/>
    public string GetStringValue() => $"[map of {_entries.Count} entries]";

    /// <inheritdoc/>
    public bool GetEffectiveBooleanValue() => _entries.Count > 0;

    /// <summary>
    /// Creates a new map item.
    /// </summary>
    public static MapItem Of(IReadOnlyDictionary<IAtomicItem, ISequence> entries) =>
        entries.Count == 0 ? Empty : new MapItem(entries);

    /// <summary>
    /// Creates a new map item with an additional entry.
    /// </summary>
    public MapItem Put(IAtomicItem key, ISequence value)
    {
        var newEntries = new Dictionary<IAtomicItem, ISequence>(_entries, new AtomicItemComparer()) { [key] = value };
        return new MapItem(newEntries);
    }

    /// <summary>
    /// Creates a new map item with a key removed.
    /// </summary>
    public MapItem Remove(IAtomicItem key)
    {
        var newEntries = new Dictionary<IAtomicItem, ISequence>(_entries, new AtomicItemComparer());
        newEntries.Remove(key);
        return new MapItem(newEntries);
    }

    private sealed class AtomicItemComparer : IEqualityComparer<IAtomicItem>
    {
        public bool Equals(IAtomicItem? x, IAtomicItem? y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            return string.Equals(x.GetStringValue(), y.GetStringValue(), StringComparison.Ordinal);
        }

        public int GetHashCode(IAtomicItem obj) => obj.GetStringValue().GetHashCode(StringComparison.Ordinal);
    }
}

/// <summary>
/// Represents a QName atomic value.
/// </summary>
public sealed class QNameItem : AtomicItem<(string? Prefix, string? NamespaceUri, string LocalName)>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QNameItem"/> class.
    /// </summary>
    public QNameItem(string? prefix, string? namespaceUri, string localName)
        : base((prefix, namespaceUri, localName)) { }

    /// <summary>
    /// Gets the prefix.
    /// </summary>
    public string? Prefix => Value.Prefix;

    /// <summary>
    /// Gets the namespace URI.
    /// </summary>
    public string? NamespaceUri => Value.NamespaceUri;

    /// <summary>
    /// Gets the local name.
    /// </summary>
    public string LocalName => Value.LocalName;

    /// <inheritdoc/>
    public override string TypeName => "QName";

    /// <inheritdoc/>
    public override string GetStringValue() =>
        string.IsNullOrEmpty(Prefix) ? LocalName : $"{Prefix}:{LocalName}";

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => true;

    /// <summary>
    /// Creates a new QName item.
    /// </summary>
    public static QNameItem Of(string? prefix, string? namespaceUri, string localName) =>
        new(prefix, namespaceUri, localName);

    /// <summary>
    /// Creates a new QName item from a local name only.
    /// </summary>
    public static QNameItem Of(string localName) => new(null, null, localName);
}

/// <summary>
/// Represents a URI atomic value.
/// </summary>
public sealed class UriItem : AtomicItem<Uri>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UriItem"/> class.
    /// </summary>
    public UriItem(Uri value) : base(value) { }

    /// <inheritdoc/>
    public override string TypeName => "anyURI";

    /// <inheritdoc/>
    public override string GetStringValue() => Value.ToString();

    /// <inheritdoc/>
    public override bool GetEffectiveBooleanValue() => true;

    /// <summary>
    /// Creates a new URI item.
    /// </summary>
    public static UriItem Of(Uri value) => new(value);

    /// <summary>
    /// Tries to create a URI item from a string.
    /// </summary>
    public static bool TryParse(string s, out UriItem? result)
    {
        if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var uri))
        {
            result = new UriItem(uri);
            return true;
        }
        result = null;
        return false;
    }
}
