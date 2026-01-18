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
