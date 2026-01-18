// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Core.Metapath.Item;

namespace Metaschema.Core.Metapath.Functions;

/// <summary>
/// Default implementation of <see cref="IFunctionLibrary"/>.
/// </summary>
public sealed class FunctionLibrary : IFunctionLibrary
{
    /// <summary>
    /// XPath 3.1 array functions namespace.
    /// </summary>
    public const string ArrayNamespace = "http://www.w3.org/2005/xpath-functions/array";

    /// <summary>
    /// XPath 3.1 map functions namespace.
    /// </summary>
    public const string MapNamespace = "http://www.w3.org/2005/xpath-functions/map";

    /// <summary>
    /// Metaschema-specific functions namespace.
    /// </summary>
    public const string MetaschemaNamespace = "http://csrc.nist.gov/ns/metaschema";

    /// <summary>
    /// Gets a shared instance of the function library with all built-in functions.
    /// This instance is thread-safe for read operations.
    /// </summary>
    public static FunctionLibrary Default { get; } = new(includeBuiltIn: true);

    private readonly Dictionary<FunctionKey, IMetapathFunction> _functions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionLibrary"/> class.
    /// </summary>
    public FunctionLibrary()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionLibrary"/> class with built-in functions.
    /// </summary>
    /// <param name="includeBuiltIn">Whether to include built-in functions.</param>
    public FunctionLibrary(bool includeBuiltIn)
    {
        if (includeBuiltIn)
        {
            RegisterBuiltInFunctions();
        }
    }

    /// <inheritdoc/>
    public IMetapathFunction? GetFunction(string name, int arity)
    {
        return GetFunction(null, name, arity);
    }

    /// <inheritdoc/>
    public IMetapathFunction? GetFunction(string? namespaceUri, string localName, int arity)
    {
        var key = new FunctionKey(namespaceUri, localName, arity);
        if (_functions.TryGetValue(key, out var func))
        {
            return func;
        }

        // Try to find a variadic function
        var variadicKey = new FunctionKey(namespaceUri, localName, -1);
        if (_functions.TryGetValue(variadicKey, out func) &&
            arity >= func.MinArity && (func.MaxArity < 0 || arity <= func.MaxArity))
        {
            return func;
        }

        return null;
    }

    /// <inheritdoc/>
    public void RegisterFunction(IMetapathFunction metapathFunction)
    {
        ArgumentNullException.ThrowIfNull(metapathFunction);
        var key = new FunctionKey(metapathFunction.NamespaceUri, metapathFunction.Name, metapathFunction.Arity);
        _functions[key] = metapathFunction;
    }

    /// <inheritdoc/>
    public IEnumerable<IMetapathFunction> GetAllFunctions() => _functions.Values;

    private void RegisterBuiltInFunctions()
    {
        // Boolean functions
        RegisterFunction(new BuiltInFunction("true", 0, (_, _) => Sequence.Of(BooleanItem.True)));
        RegisterFunction(new BuiltInFunction("false", 0, (_, _) => Sequence.Of(BooleanItem.False)));
        RegisterFunction(new BuiltInFunction("not", 1, NotFunction));
        RegisterFunction(new BuiltInFunction("boolean", 1, BooleanFunction));

        // Sequence functions
        RegisterFunction(new BuiltInFunction("empty", 1, EmptyFunction));
        RegisterFunction(new BuiltInFunction("exists", 1, ExistsFunction));
        RegisterFunction(new BuiltInFunction("count", 1, CountFunction));
        RegisterFunction(new BuiltInFunction("head", 1, HeadFunction));
        RegisterFunction(new BuiltInFunction("tail", 1, TailFunction));

        // String functions
        RegisterFunction(new BuiltInFunction("string", 0, 1, StringFunction));
        RegisterFunction(new BuiltInFunction("concat", 0, -1, ConcatFunction));
        RegisterFunction(new BuiltInFunction("string-length", 0, 1, StringLengthFunction));
        RegisterFunction(new BuiltInFunction("normalize-space", 0, 1, NormalizeSpaceFunction));
        RegisterFunction(new BuiltInFunction("contains", 2, ContainsFunction));
        RegisterFunction(new BuiltInFunction("starts-with", 2, StartsWithFunction));
        RegisterFunction(new BuiltInFunction("ends-with", 2, EndsWithFunction));
        RegisterFunction(new BuiltInFunction("substring", 2, 3, SubstringFunction));
        RegisterFunction(new BuiltInFunction("upper-case", 1, UpperCaseFunction));
        RegisterFunction(new BuiltInFunction("lower-case", 1, LowerCaseFunction));

        // Numeric functions
        RegisterFunction(new BuiltInFunction("abs", 1, AbsFunction));
        RegisterFunction(new BuiltInFunction("round", 1, RoundFunction));
        RegisterFunction(new BuiltInFunction("floor", 1, FloorFunction));
        RegisterFunction(new BuiltInFunction("ceiling", 1, CeilingFunction));

        // Aggregate functions
        RegisterFunction(new BuiltInFunction("sum", 1, SumFunction));
        RegisterFunction(new BuiltInFunction("avg", 1, AvgFunction));
        RegisterFunction(new BuiltInFunction("min", 1, MinFunction));
        RegisterFunction(new BuiltInFunction("max", 1, MaxFunction));

        // Context functions
        RegisterFunction(new BuiltInFunction("position", 0, PositionFunction));
        RegisterFunction(new BuiltInFunction("last", 0, LastFunction));
        RegisterFunction(new BuiltInFunction("data", 0, 1, DataFunction));

        // Additional sequence functions
        RegisterFunction(new BuiltInFunction("distinct-values", 1, DistinctValuesFunction));
        RegisterFunction(new BuiltInFunction("index-of", 2, IndexOfFunction));
        RegisterFunction(new BuiltInFunction("reverse", 1, ReverseFunction));
        RegisterFunction(new BuiltInFunction("subsequence", 2, 3, SubsequenceFunction));
        RegisterFunction(new BuiltInFunction("insert-before", 3, InsertBeforeFunction));
        RegisterFunction(new BuiltInFunction("remove", 2, RemoveFunction));
        RegisterFunction(new BuiltInFunction("unordered", 1, UnorderedFunction));
        RegisterFunction(new BuiltInFunction("zero-or-one", 1, ZeroOrOneFunction));
        RegisterFunction(new BuiltInFunction("one-or-more", 1, OneOrMoreFunction));
        RegisterFunction(new BuiltInFunction("exactly-one", 1, ExactlyOneFunction));
        RegisterFunction(new BuiltInFunction("deep-equal", 2, DeepEqualFunction));

        // Additional string functions
        RegisterFunction(new BuiltInFunction("string-join", 1, 2, StringJoinFunction));
        RegisterFunction(new BuiltInFunction("substring-before", 2, SubstringBeforeFunction));
        RegisterFunction(new BuiltInFunction("substring-after", 2, SubstringAfterFunction));
        RegisterFunction(new BuiltInFunction("translate", 3, TranslateFunction));
        RegisterFunction(new BuiltInFunction("compare", 2, CompareFunction));
        RegisterFunction(new BuiltInFunction("codepoints-to-string", 1, CodepointsToStringFunction));
        RegisterFunction(new BuiltInFunction("string-to-codepoints", 1, StringToCodepointsFunction));

        // Regex functions
        RegisterFunction(new BuiltInFunction("matches", 2, 3, MatchesFunction));
        RegisterFunction(new BuiltInFunction("replace", 3, 4, ReplaceFunction));
        RegisterFunction(new BuiltInFunction("tokenize", 1, 3, TokenizeFunction));

        // Node functions
        RegisterFunction(new BuiltInFunction("path", 0, 1, PathFunction));
        RegisterFunction(new BuiltInFunction("root", 0, 1, RootFunction));
        RegisterFunction(new BuiltInFunction("base-uri", 0, 1, BaseUriFunction));
        RegisterFunction(new BuiltInFunction("document-uri", 0, 1, DocumentUriFunction));
        RegisterFunction(new BuiltInFunction("has-children", 0, 1, HasChildrenFunction));

        // Type/Number functions
        RegisterFunction(new BuiltInFunction("number", 0, 1, NumberFunction));

        // Date/Time functions
        RegisterFunction(new BuiltInFunction("current-date", 0, CurrentDateFunction));
        RegisterFunction(new BuiltInFunction("current-dateTime", 0, CurrentDateTimeFunction));
        RegisterFunction(new BuiltInFunction("current-time", 0, CurrentTimeFunction));
        RegisterFunction(new BuiltInFunction("dateTime", 2, DateTimeConstructorFunction));
        RegisterFunction(new BuiltInFunction("year-from-date", 1, YearFromDateFunction));
        RegisterFunction(new BuiltInFunction("year-from-dateTime", 1, YearFromDateTimeFunction));
        RegisterFunction(new BuiltInFunction("month-from-date", 1, MonthFromDateFunction));
        RegisterFunction(new BuiltInFunction("month-from-dateTime", 1, MonthFromDateTimeFunction));
        RegisterFunction(new BuiltInFunction("day-from-date", 1, DayFromDateFunction));
        RegisterFunction(new BuiltInFunction("day-from-dateTime", 1, DayFromDateTimeFunction));
        RegisterFunction(new BuiltInFunction("hours-from-dateTime", 1, HoursFromDateTimeFunction));
        RegisterFunction(new BuiltInFunction("hours-from-time", 1, HoursFromTimeFunction));
        RegisterFunction(new BuiltInFunction("minutes-from-dateTime", 1, MinutesFromDateTimeFunction));
        RegisterFunction(new BuiltInFunction("minutes-from-time", 1, MinutesFromTimeFunction));
        RegisterFunction(new BuiltInFunction("seconds-from-dateTime", 1, SecondsFromDateTimeFunction));
        RegisterFunction(new BuiltInFunction("seconds-from-time", 1, SecondsFromTimeFunction));
        RegisterFunction(new BuiltInFunction("timezone-from-date", 1, TimezoneFromDateFunction));
        RegisterFunction(new BuiltInFunction("timezone-from-dateTime", 1, TimezoneFromDateTimeFunction));
        RegisterFunction(new BuiltInFunction("timezone-from-time", 1, TimezoneFromTimeFunction));
        RegisterFunction(new BuiltInFunction("years-from-duration", 1, YearsFromDurationFunction));
        RegisterFunction(new BuiltInFunction("months-from-duration", 1, MonthsFromDurationFunction));
        RegisterFunction(new BuiltInFunction("days-from-duration", 1, DaysFromDurationFunction));
        RegisterFunction(new BuiltInFunction("hours-from-duration", 1, HoursFromDurationFunction));
        RegisterFunction(new BuiltInFunction("minutes-from-duration", 1, MinutesFromDurationFunction));
        RegisterFunction(new BuiltInFunction("seconds-from-duration", 1, SecondsFromDurationFunction));
        RegisterFunction(new BuiltInFunction("implicit-timezone", 0, ImplicitTimezoneFunction));
        RegisterFunction(new BuiltInFunction("adjust-dateTime-to-timezone", 1, 2, AdjustDateTimeToTimezoneFunction));
        RegisterFunction(new BuiltInFunction("adjust-date-to-timezone", 1, 2, AdjustDateToTimezoneFunction));
        RegisterFunction(new BuiltInFunction("adjust-time-to-timezone", 1, 2, AdjustTimeToTimezoneFunction));

        // Array functions
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "size", 1, ArraySizeFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "get", 2, ArrayGetFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "put", 3, ArrayPutFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "append", 2, ArrayAppendFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "subarray", 2, 3, ArraySubarrayFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "remove", 2, ArrayRemoveFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "insert-before", 3, ArrayInsertBeforeFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "head", 1, ArrayHeadFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "tail", 1, ArrayTailFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "reverse", 1, ArrayReverseFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "join", 1, ArrayJoinFunction));
        RegisterFunction(new BuiltInFunction(ArrayNamespace, "flatten", 1, ArrayFlattenFunction));

        // Map functions
        RegisterFunction(new BuiltInFunction(MapNamespace, "size", 1, MapSizeFunction));
        RegisterFunction(new BuiltInFunction(MapNamespace, "keys", 1, MapKeysFunction));
        RegisterFunction(new BuiltInFunction(MapNamespace, "contains", 2, MapContainsFunction));
        RegisterFunction(new BuiltInFunction(MapNamespace, "get", 2, MapGetFunction));
        RegisterFunction(new BuiltInFunction(MapNamespace, "put", 3, MapPutFunction));
        RegisterFunction(new BuiltInFunction(MapNamespace, "entry", 2, MapEntryFunction));
        RegisterFunction(new BuiltInFunction(MapNamespace, "remove", 2, MapRemoveFunction));
        RegisterFunction(new BuiltInFunction(MapNamespace, "merge", 1, 2, MapMergeFunction));

        // QName and node identity functions
        RegisterFunction(new BuiltInFunction("local-name", 0, 1, LocalNameFunction));
        RegisterFunction(new BuiltInFunction("name", 0, 1, NameFunction));
        RegisterFunction(new BuiltInFunction("namespace-uri", 0, 1, NamespaceUriFunction));
        RegisterFunction(new BuiltInFunction("QName", 2, QNameConstructorFunction));
        RegisterFunction(new BuiltInFunction("local-name-from-QName", 1, LocalNameFromQNameFunction));
        RegisterFunction(new BuiltInFunction("namespace-uri-from-QName", 1, NamespaceUriFromQNameFunction));
        RegisterFunction(new BuiltInFunction("prefix-from-QName", 1, PrefixFromQNameFunction));

        // URI functions
        RegisterFunction(new BuiltInFunction("resolve-uri", 1, 2, ResolveUriFunction));
        RegisterFunction(new BuiltInFunction("static-base-uri", 0, StaticBaseUriFunction));
        RegisterFunction(new BuiltInFunction("encode-for-uri", 1, EncodeForUriFunction));

        // Document functions
        RegisterFunction(new BuiltInFunction("doc", 1, DocFunction));
        RegisterFunction(new BuiltInFunction("doc-available", 1, DocAvailableFunction));

        // Node set functions
        RegisterFunction(new BuiltInFunction("innermost", 1, InnermostFunction));
        RegisterFunction(new BuiltInFunction("outermost", 1, OutermostFunction));

        // Metaschema-specific functions
        RegisterFunction(new BuiltInFunction(MetaschemaNamespace, "base64-encode", 1, Base64EncodeFunction));
        RegisterFunction(new BuiltInFunction(MetaschemaNamespace, "base64-decode", 1, Base64DecodeFunction));
        RegisterFunction(new BuiltInFunction(MetaschemaNamespace, "recurse-depth", 0, 1, RecurseDepthFunction));
    }

    // Built-in function implementations
    private static ISequence NotFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var ebv = args[0].GetEffectiveBooleanValue();
        return Sequence.Of(BooleanItem.Of(!ebv));
    }

    private static ISequence BooleanFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var ebv = args[0].GetEffectiveBooleanValue();
        return Sequence.Of(BooleanItem.Of(ebv));
    }

    private static ISequence EmptyFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        return Sequence.Of(BooleanItem.Of(args[0].IsEmpty));
    }

    private static ISequence ExistsFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        return Sequence.Of(BooleanItem.Of(!args[0].IsEmpty));
    }

    private static ISequence CountFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        return Sequence.Of(IntegerItem.Of(args[0].Count));
    }

    private static ISequence HeadFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var first = args[0].FirstOrDefault;
        return first is null ? Sequence.Empty : Sequence.Of(first);
    }

#pragma warning disable CA1859 // Return type must match delegate signature
    private static ISequence TailFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        return new Sequence(args[0].Skip(1));
    }
#pragma warning restore CA1859

    private static ISequence StringFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        if (args.Count == 0 || args[0].IsEmpty)
        {
            var contextItem = ctx.DynamicContext.ContextItem;
            return Sequence.Of(StringItem.Of(contextItem?.GetStringValue() ?? string.Empty));
        }
        var item = args[0].FirstOrDefault;
        return Sequence.Of(StringItem.Of(item?.GetStringValue() ?? string.Empty));
    }

    private static ISequence ConcatFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        if (args.Count == 0) return Sequence.Of(StringItem.Of(string.Empty));
        var result = string.Concat(args.Select(a => a.FirstOrDefault?.GetStringValue() ?? string.Empty));
        return Sequence.Of(StringItem.Of(result));
    }

    private static ISequence StringLengthFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        string str;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            str = ctx.DynamicContext.ContextItem?.GetStringValue() ?? string.Empty;
        }
        else
        {
            str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        }
        return Sequence.Of(IntegerItem.Of(str.Length));
    }

    private static ISequence NormalizeSpaceFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        string str;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            str = ctx.DynamicContext.ContextItem?.GetStringValue() ?? string.Empty;
        }
        else
        {
            str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        }
        str = string.Join(" ", str.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return Sequence.Of(StringItem.Of(str));
    }

    private static ISequence ContainsFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str1 = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var str2 = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;
        return Sequence.Of(BooleanItem.Of(str1.Contains(str2, StringComparison.Ordinal)));
    }

    private static ISequence StartsWithFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str1 = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var str2 = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;
        return Sequence.Of(BooleanItem.Of(str1.StartsWith(str2, StringComparison.Ordinal)));
    }

    private static ISequence EndsWithFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str1 = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var str2 = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;
        return Sequence.Of(BooleanItem.Of(str1.EndsWith(str2, StringComparison.Ordinal)));
    }

    private static ISequence SubstringFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var startItem = args[1].FirstOrDefault;
        var start = startItem is IAtomicItem { Value: long l } ? (int)l : 1;
        // XPath uses 1-based indexing
        var startIndex = Math.Max(0, start - 1);

        if (args.Count > 2)
        {
            var lengthItem = args[2].FirstOrDefault;
            var length = lengthItem is IAtomicItem { Value: long len } ? (int)len : str.Length;
            length = Math.Max(0, Math.Min(length, str.Length - startIndex));
            return Sequence.Of(StringItem.Of(str.Substring(startIndex, length)));
        }
        return Sequence.Of(StringItem.Of(startIndex < str.Length ? str[startIndex..] : string.Empty));
    }

    private static ISequence UpperCaseFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        return Sequence.Of(StringItem.Of(str.ToUpperInvariant()));
    }

    private static ISequence LowerCaseFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        return Sequence.Of(StringItem.Of(str.ToLowerInvariant()));
    }

    private static ISequence AbsFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            IntegerItem i => Sequence.Of(IntegerItem.Of(Math.Abs(i.Value))),
            DecimalItem d => Sequence.Of(DecimalItem.Of(Math.Abs(d.Value))),
            DoubleItem db => Sequence.Of(DoubleItem.Of(Math.Abs(db.Value))),
            _ => Sequence.Empty
        };
    }

    private static ISequence RoundFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            IntegerItem i => Sequence.Of(i),
            DecimalItem d => Sequence.Of(DecimalItem.Of(Math.Round(d.Value))),
            DoubleItem db => Sequence.Of(DoubleItem.Of(Math.Round(db.Value))),
            _ => Sequence.Empty
        };
    }

    private static ISequence FloorFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            IntegerItem i => Sequence.Of(i),
            DecimalItem d => Sequence.Of(DecimalItem.Of(Math.Floor(d.Value))),
            DoubleItem db => Sequence.Of(DoubleItem.Of(Math.Floor(db.Value))),
            _ => Sequence.Empty
        };
    }

    private static ISequence CeilingFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            IntegerItem i => Sequence.Of(i),
            DecimalItem d => Sequence.Of(DecimalItem.Of(Math.Ceiling(d.Value))),
            DoubleItem db => Sequence.Of(DoubleItem.Of(Math.Ceiling(db.Value))),
            _ => Sequence.Empty
        };
    }

    private static ISequence SumFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0];
        if (seq.IsEmpty) return Sequence.Of(IntegerItem.Zero);

        decimal sum = 0;
        foreach (var item in seq)
        {
            if (item is IntegerItem i) sum += i.Value;
            else if (item is DecimalItem d) sum += d.Value;
            else if (item is DoubleItem db) sum += (decimal)db.Value;
        }
        return Sequence.Of(DecimalItem.Of(sum));
    }

    private static ISequence AvgFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0];
        if (seq.IsEmpty) return Sequence.Empty;

        decimal sum = 0;
        var count = 0;
        foreach (var item in seq)
        {
            if (item is IntegerItem i) sum += i.Value;
            else if (item is DecimalItem d) sum += d.Value;
            else if (item is DoubleItem db) sum += (decimal)db.Value;
            count++;
        }
        return count > 0 ? Sequence.Of(DecimalItem.Of(sum / count)) : Sequence.Empty;
    }

    private static ISequence MinFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0];
        if (seq.IsEmpty) return Sequence.Empty;

        IItem? min = null;
        var minValue = decimal.MaxValue;

        foreach (var item in seq)
        {
            var value = item switch
            {
                IntegerItem i => i.Value,
                DecimalItem d => d.Value,
                DoubleItem db => (decimal)db.Value,
                _ => decimal.MaxValue
            };

            if (value < minValue)
            {
                minValue = value;
                min = item;
            }
        }

        return min is not null ? Sequence.Of(min) : Sequence.Empty;
    }

    private static ISequence MaxFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0];
        if (seq.IsEmpty) return Sequence.Empty;

        IItem? max = null;
        var maxValue = decimal.MinValue;

        foreach (var item in seq)
        {
            var value = item switch
            {
                IntegerItem i => i.Value,
                DecimalItem d => d.Value,
                DoubleItem db => (decimal)db.Value,
                _ => decimal.MinValue
            };

            if (value > maxValue)
            {
                maxValue = value;
                max = item;
            }
        }

        return max is not null ? Sequence.Of(max) : Sequence.Empty;
    }

    // Context functions
    private static ISequence PositionFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        return Sequence.Of(IntegerItem.Of(ctx.DynamicContext.ContextPosition));
    }

    private static ISequence LastFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        return Sequence.Of(IntegerItem.Of(ctx.DynamicContext.ContextSize));
    }

#pragma warning disable CA1859 // Return type must match delegate signature
    private static ISequence DataFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        ISequence source;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            var contextItem = ctx.DynamicContext.ContextItem;
            source = contextItem is not null ? Sequence.Of(contextItem) : Sequence.Empty;
        }
        else
        {
            source = args[0];
        }

        // Atomize the sequence - extract typed values
        var results = new List<IItem>();
        foreach (var item in source)
        {
            if (item is IAtomicItem)
            {
                results.Add(item);
            }
            else if (item is INodeItem node)
            {
                // Get the typed value of the node
                var stringValue = node.GetStringValue();
                results.Add(StringItem.Of(stringValue));
            }
        }
        return new Sequence(results);
    }
#pragma warning restore CA1859

    // Additional sequence functions
#pragma warning disable CA1859 // Return type must match delegate signature
    private static ISequence DistinctValuesFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var results = new List<IItem>();
        foreach (var item in args[0])
        {
            var key = item.GetStringValue();
            if (seen.Add(key))
            {
                results.Add(item);
            }
        }
        return new Sequence(results);
    }

    private static ISequence IndexOfFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0];
        var search = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var results = new List<IItem>();
        var index = 1;
        foreach (var item in seq)
        {
            if (string.Equals(item.GetStringValue(), search, StringComparison.Ordinal))
            {
                results.Add(IntegerItem.Of(index));
            }
            index++;
        }
        return new Sequence(results);
    }

    private static ISequence ReverseFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        return new Sequence(args[0].Reverse());
    }

    private static ISequence SubsequenceFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0].ToList();
        var startItem = args[1].FirstOrDefault;
        var start = startItem is IntegerItem si ? (int)si.Value : 1;
        // XPath uses 1-based indexing, convert to 0-based
        var startIndex = Math.Max(0, start - 1);

        if (args.Count > 2)
        {
            var lengthItem = args[2].FirstOrDefault;
            var length = lengthItem is IntegerItem li ? (int)li.Value : seq.Count;
            length = Math.Max(0, Math.Min(length, seq.Count - startIndex));
            return new Sequence(seq.Skip(startIndex).Take(length));
        }
        return new Sequence(seq.Skip(startIndex));
    }

    private static ISequence InsertBeforeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0].ToList();
        var posItem = args[1].FirstOrDefault;
        var position = posItem is IntegerItem pi ? (int)pi.Value : 1;
        var inserts = args[2].ToList();

        // Convert to 0-based, clamp to valid range
        var insertIndex = Math.Max(0, Math.Min(position - 1, seq.Count));

        var result = new List<IItem>(seq.Count + inserts.Count);
        result.AddRange(seq.Take(insertIndex));
        result.AddRange(inserts);
        result.AddRange(seq.Skip(insertIndex));
        return new Sequence(result);
    }

    private static ISequence RemoveFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0].ToList();
        var posItem = args[1].FirstOrDefault;
        var position = posItem is IntegerItem pi ? (int)pi.Value : 0;

        // Convert to 0-based
        var removeIndex = position - 1;
        if (removeIndex < 0 || removeIndex >= seq.Count)
        {
            return new Sequence(seq);
        }

        var result = new List<IItem>(seq.Count - 1);
        result.AddRange(seq.Take(removeIndex));
        result.AddRange(seq.Skip(removeIndex + 1));
        return new Sequence(result);
    }

    private static ISequence UnorderedFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        // unordered() is a hint to the processor - just return the sequence as-is
        return args[0];
    }
#pragma warning restore CA1859

    private static ISequence ZeroOrOneFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0];
        if (seq.Count > 1)
        {
            throw new MetapathException("zero-or-one() called with a sequence containing more than one item");
        }
        return seq;
    }

    private static ISequence OneOrMoreFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0];
        if (seq.IsEmpty)
        {
            throw new MetapathException("one-or-more() called with an empty sequence");
        }
        return seq;
    }

    private static ISequence ExactlyOneFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq = args[0];
        if (seq.Count != 1)
        {
            throw new MetapathException($"exactly-one() called with a sequence containing {seq.Count} items");
        }
        return seq;
    }

    private static ISequence DeepEqualFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var seq1 = args[0].ToList();
        var seq2 = args[1].ToList();

        if (seq1.Count != seq2.Count)
        {
            return Sequence.Of(BooleanItem.False);
        }

        for (var i = 0; i < seq1.Count; i++)
        {
            var val1 = seq1[i].GetStringValue();
            var val2 = seq2[i].GetStringValue();
            if (!string.Equals(val1, val2, StringComparison.Ordinal))
            {
                return Sequence.Of(BooleanItem.False);
            }
        }

        return Sequence.Of(BooleanItem.True);
    }

    // Additional string functions
#pragma warning disable CA1859
    private static ISequence StringJoinFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var separator = args.Count > 1 ? args[1].FirstOrDefault?.GetStringValue() ?? string.Empty : string.Empty;
        var strings = args[0].Select(item => item.GetStringValue());
        return Sequence.Of(StringItem.Of(string.Join(separator, strings)));
    }
#pragma warning restore CA1859

    private static ISequence SubstringBeforeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var search = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;

        if (string.IsNullOrEmpty(search))
        {
            return Sequence.Of(StringItem.Of(string.Empty));
        }

        var index = str.IndexOf(search, StringComparison.Ordinal);
        return Sequence.Of(StringItem.Of(index >= 0 ? str[..index] : string.Empty));
    }

    private static ISequence SubstringAfterFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var search = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;

        if (string.IsNullOrEmpty(search))
        {
            return Sequence.Of(StringItem.Of(str));
        }

        var index = str.IndexOf(search, StringComparison.Ordinal);
        return Sequence.Of(StringItem.Of(index >= 0 ? str[(index + search.Length)..] : string.Empty));
    }

    private static ISequence TranslateFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var mapString = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var transString = args[2].FirstOrDefault?.GetStringValue() ?? string.Empty;

        var result = new char[str.Length];
        var resultIndex = 0;

        foreach (var c in str)
        {
            var mapIndex = mapString.IndexOf(c);
            if (mapIndex < 0)
            {
                result[resultIndex++] = c;
            }
            else if (mapIndex < transString.Length)
            {
                result[resultIndex++] = transString[mapIndex];
            }
            // If mapIndex >= transString.Length, character is deleted
        }

        return Sequence.Of(StringItem.Of(new string(result, 0, resultIndex)));
    }

    private static ISequence CompareFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str1 = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var str2 = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var cmp = string.Compare(str1, str2, StringComparison.Ordinal);
        return Sequence.Of(IntegerItem.Of(cmp < 0 ? -1 : cmp > 0 ? 1 : 0));
    }

#pragma warning disable CA1859
    private static ISequence CodepointsToStringFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var codepoints = args[0]
            .Where(item => item is IntegerItem)
            .Select(item => (char)((IntegerItem)item).Value);
        return Sequence.Of(StringItem.Of(new string(codepoints.ToArray())));
    }

    private static ISequence StringToCodepointsFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var codepoints = str.Select(c => IntegerItem.Of(c)).Cast<IItem>();
        return new Sequence(codepoints);
    }
#pragma warning restore CA1859

    // Regex functions
    private static ISequence MatchesFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var input = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var pattern = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var flags = args.Count > 2 ? args[2].FirstOrDefault?.GetStringValue() ?? string.Empty : string.Empty;

        try
        {
            var options = ParseRegexFlags(flags);
            var match = System.Text.RegularExpressions.Regex.IsMatch(input, pattern, options);
            return Sequence.Of(BooleanItem.Of(match));
        }
        catch (ArgumentException)
        {
            throw new MetapathException($"Invalid regular expression: {pattern}");
        }
    }

    private static ISequence ReplaceFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var input = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var pattern = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var replacement = args[2].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var flags = args.Count > 3 ? args[3].FirstOrDefault?.GetStringValue() ?? string.Empty : string.Empty;

        try
        {
            var options = ParseRegexFlags(flags);
            var result = System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement, options);
            return Sequence.Of(StringItem.Of(result));
        }
        catch (ArgumentException)
        {
            throw new MetapathException($"Invalid regular expression: {pattern}");
        }
    }

#pragma warning disable CA1859
    private static ISequence TokenizeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var input = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var pattern = args.Count > 1 ? args[1].FirstOrDefault?.GetStringValue() ?? @"\s+" : @"\s+";
        var flags = args.Count > 2 ? args[2].FirstOrDefault?.GetStringValue() ?? string.Empty : string.Empty;

        try
        {
            var options = ParseRegexFlags(flags);
            var tokens = System.Text.RegularExpressions.Regex.Split(input, pattern, options)
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => StringItem.Of(s))
                .Cast<IItem>();
            return new Sequence(tokens);
        }
        catch (ArgumentException)
        {
            throw new MetapathException($"Invalid regular expression: {pattern}");
        }
    }
#pragma warning restore CA1859

    private static System.Text.RegularExpressions.RegexOptions ParseRegexFlags(string flags)
    {
        var options = System.Text.RegularExpressions.RegexOptions.None;
        foreach (var c in flags)
        {
            options |= c switch
            {
                'i' => System.Text.RegularExpressions.RegexOptions.IgnoreCase,
                'm' => System.Text.RegularExpressions.RegexOptions.Multiline,
                's' => System.Text.RegularExpressions.RegexOptions.Singleline,
                'x' => System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace,
                _ => System.Text.RegularExpressions.RegexOptions.None
            };
        }
        return options;
    }

    // Node functions
    private static ISequence PathFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        INodeItem? node;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            node = ctx.DynamicContext.ContextItem as INodeItem;
        }
        else
        {
            node = args[0].FirstOrDefault as INodeItem;
        }

        if (node is null)
        {
            return Sequence.Empty;
        }

        return Sequence.Of(StringItem.Of(node.GetPath()));
    }

    private static ISequence RootFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        INodeItem? node;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            node = ctx.DynamicContext.ContextItem as INodeItem;
        }
        else
        {
            node = args[0].FirstOrDefault as INodeItem;
        }

        if (node is null)
        {
            return Sequence.Empty;
        }

        // Navigate to root
        while (node.Parent is not null)
        {
            node = node.Parent;
        }

        return Sequence.Of(node);
    }

    private static ISequence BaseUriFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        INodeItem? node;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            node = ctx.DynamicContext.ContextItem as INodeItem;
        }
        else
        {
            node = args[0].FirstOrDefault as INodeItem;
        }

        if (node?.BaseUri is null)
        {
            return Sequence.Empty;
        }

        return Sequence.Of(StringItem.Of(node.BaseUri.ToString()));
    }

    private static ISequence DocumentUriFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        INodeItem? node;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            node = ctx.DynamicContext.ContextItem as INodeItem;
        }
        else
        {
            node = args[0].FirstOrDefault as INodeItem;
        }

        if (node?.DocumentUri is null)
        {
            return Sequence.Empty;
        }

        return Sequence.Of(StringItem.Of(node.DocumentUri.ToString()));
    }

    private static ISequence HasChildrenFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        INodeItem? node;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            node = ctx.DynamicContext.ContextItem as INodeItem;
        }
        else
        {
            node = args[0].FirstOrDefault as INodeItem;
        }

        if (node is null)
        {
            return Sequence.Of(BooleanItem.False);
        }

        return Sequence.Of(BooleanItem.Of(node.GetChildren().Any()));
    }

    // Type/Number functions
    private static ISequence NumberFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        string str;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            str = ctx.DynamicContext.ContextItem?.GetStringValue() ?? string.Empty;
        }
        else
        {
            str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        }

        if (decimal.TryParse(str, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            return Sequence.Of(DecimalItem.Of(value));
        }

        // XPath spec says return NaN for non-numeric strings
        return Sequence.Of(DoubleItem.Of(double.NaN));
    }

    // Date/Time functions
    private static ISequence CurrentDateFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var now = DateTimeOffset.Now;
        return Sequence.Of(DateItem.Of(DateOnly.FromDateTime(now.DateTime), now.Offset));
    }

    private static ISequence CurrentDateTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        return Sequence.Of(DateTimeItem.Of(DateTimeOffset.Now));
    }

    private static ISequence CurrentTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var now = DateTimeOffset.Now;
        return Sequence.Of(TimeItem.Of(TimeOnly.FromDateTime(now.DateTime), now.Offset));
    }

    private static ISequence DateTimeConstructorFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var dateArg = args[0].FirstOrDefault;
        var timeArg = args[1].FirstOrDefault;

        if (dateArg is DateItem date && timeArg is TimeItem time)
        {
            var tz = date.Timezone ?? time.Timezone ?? TimeSpan.Zero;
            var dt = new DateTimeOffset(
                date.Value.Year, date.Value.Month, date.Value.Day,
                time.Value.Hour, time.Value.Minute, time.Value.Second,
                tz);
            return Sequence.Of(DateTimeItem.Of(dt));
        }
        return Sequence.Empty;
    }

    private static ISequence YearFromDateFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DateItem d => Sequence.Of(IntegerItem.Of(d.Value.Year)),
            _ => Sequence.Empty
        };
    }

    private static ISequence YearFromDateTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DateTimeItem dt => Sequence.Of(IntegerItem.Of(dt.Value.Year)),
            _ => Sequence.Empty
        };
    }

    private static ISequence MonthFromDateFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DateItem d => Sequence.Of(IntegerItem.Of(d.Value.Month)),
            _ => Sequence.Empty
        };
    }

    private static ISequence MonthFromDateTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DateTimeItem dt => Sequence.Of(IntegerItem.Of(dt.Value.Month)),
            _ => Sequence.Empty
        };
    }

    private static ISequence DayFromDateFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DateItem d => Sequence.Of(IntegerItem.Of(d.Value.Day)),
            _ => Sequence.Empty
        };
    }

    private static ISequence DayFromDateTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DateTimeItem dt => Sequence.Of(IntegerItem.Of(dt.Value.Day)),
            _ => Sequence.Empty
        };
    }

    private static ISequence HoursFromDateTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DateTimeItem dt => Sequence.Of(IntegerItem.Of(dt.Value.Hour)),
            _ => Sequence.Empty
        };
    }

    private static ISequence HoursFromTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            TimeItem t => Sequence.Of(IntegerItem.Of(t.Value.Hour)),
            _ => Sequence.Empty
        };
    }

    private static ISequence MinutesFromDateTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DateTimeItem dt => Sequence.Of(IntegerItem.Of(dt.Value.Minute)),
            _ => Sequence.Empty
        };
    }

    private static ISequence MinutesFromTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            TimeItem t => Sequence.Of(IntegerItem.Of(t.Value.Minute)),
            _ => Sequence.Empty
        };
    }

    private static ISequence SecondsFromDateTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DateTimeItem dt => Sequence.Of(DecimalItem.Of(dt.Value.Second + dt.Value.Millisecond / 1000m)),
            _ => Sequence.Empty
        };
    }

    private static ISequence SecondsFromTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            TimeItem t => Sequence.Of(DecimalItem.Of(t.Value.Second + t.Value.Millisecond / 1000m)),
            _ => Sequence.Empty
        };
    }

    private static ISequence TimezoneFromDateFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is DateItem d && d.Timezone.HasValue)
        {
            return Sequence.Of(DayTimeDurationItem.Of(d.Timezone.Value));
        }
        return Sequence.Empty;
    }

    private static ISequence TimezoneFromDateTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is DateTimeItem dt)
        {
            return Sequence.Of(DayTimeDurationItem.Of(dt.Value.Offset));
        }
        return Sequence.Empty;
    }

    private static ISequence TimezoneFromTimeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is TimeItem t && t.Timezone.HasValue)
        {
            return Sequence.Of(DayTimeDurationItem.Of(t.Timezone.Value));
        }
        return Sequence.Empty;
    }

    private static ISequence YearsFromDurationFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DurationItem d => Sequence.Of(IntegerItem.Of(d.Months / 12)),
            _ => Sequence.Empty
        };
    }

    private static ISequence MonthsFromDurationFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DurationItem d => Sequence.Of(IntegerItem.Of(d.Months % 12)),
            _ => Sequence.Empty
        };
    }

    private static ISequence DaysFromDurationFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DurationItem d => Sequence.Of(IntegerItem.Of(d.Value.Days)),
            DayTimeDurationItem d => Sequence.Of(IntegerItem.Of(d.Value.Days)),
            _ => Sequence.Empty
        };
    }

    private static ISequence HoursFromDurationFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DurationItem d => Sequence.Of(IntegerItem.Of(d.Value.Hours)),
            DayTimeDurationItem d => Sequence.Of(IntegerItem.Of(d.Value.Hours)),
            _ => Sequence.Empty
        };
    }

    private static ISequence MinutesFromDurationFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DurationItem d => Sequence.Of(IntegerItem.Of(d.Value.Minutes)),
            DayTimeDurationItem d => Sequence.Of(IntegerItem.Of(d.Value.Minutes)),
            _ => Sequence.Empty
        };
    }

    private static ISequence SecondsFromDurationFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item switch
        {
            DurationItem d => Sequence.Of(DecimalItem.Of(d.Value.Seconds + d.Value.Milliseconds / 1000m)),
            DayTimeDurationItem d => Sequence.Of(DecimalItem.Of(d.Value.Seconds + d.Value.Milliseconds / 1000m)),
            _ => Sequence.Empty
        };
    }

    private static ISequence ImplicitTimezoneFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        return Sequence.Of(DayTimeDurationItem.Of(DateTimeOffset.Now.Offset));
    }

    private static ISequence AdjustDateTimeToTimezoneFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is not DateTimeItem dt) return Sequence.Empty;

        TimeSpan? tz;
        if (args.Count > 1 && !args[1].IsEmpty)
        {
            var tzItem = args[1].FirstOrDefault;
            tz = tzItem switch
            {
                DayTimeDurationItem d => d.Value,
                DurationItem d => d.Value,
                _ => null
            };
        }
        else
        {
            tz = DateTimeOffset.Now.Offset;
        }

        if (!tz.HasValue) return Sequence.Of(dt);

        var adjusted = dt.Value.ToOffset(tz.Value);
        return Sequence.Of(DateTimeItem.Of(adjusted));
    }

    private static ISequence AdjustDateToTimezoneFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is not DateItem d) return Sequence.Empty;

        TimeSpan? tz;
        if (args.Count > 1 && !args[1].IsEmpty)
        {
            var tzItem = args[1].FirstOrDefault;
            tz = tzItem switch
            {
                DayTimeDurationItem dur => dur.Value,
                DurationItem dur => dur.Value,
                _ => null
            };
        }
        else
        {
            tz = DateTimeOffset.Now.Offset;
        }

        return Sequence.Of(DateItem.Of(d.Value, tz));
    }

    private static ISequence AdjustTimeToTimezoneFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is not TimeItem t) return Sequence.Empty;

        TimeSpan? tz;
        if (args.Count > 1 && !args[1].IsEmpty)
        {
            var tzItem = args[1].FirstOrDefault;
            tz = tzItem switch
            {
                DayTimeDurationItem d => d.Value,
                DurationItem d => d.Value,
                _ => null
            };
        }
        else
        {
            tz = DateTimeOffset.Now.Offset;
        }

        return Sequence.Of(TimeItem.Of(t.Value, tz));
    }

    // Array functions
    private static ISequence ArraySizeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item is ArrayItem arr ? Sequence.Of(IntegerItem.Of(arr.Size)) : Sequence.Empty;
    }

    private static ISequence ArrayGetFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var arrItem = args[0].FirstOrDefault;
        var posItem = args[1].FirstOrDefault;
        if (arrItem is ArrayItem arr && posItem is IntegerItem pos)
        {
            return arr.Get((int)pos.Value);
        }
        return Sequence.Empty;
    }

#pragma warning disable CA1859
    private static ISequence ArrayPutFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var arrItem = args[0].FirstOrDefault;
        var posItem = args[1].FirstOrDefault;
        var value = args[2];
        if (arrItem is ArrayItem arr && posItem is IntegerItem pos)
        {
            var index = (int)pos.Value - 1;
            if (index >= 0 && index < arr.Size)
            {
                var newMembers = arr.Members.ToList();
                newMembers[index] = value;
                return Sequence.Of(ArrayItem.Of(newMembers));
            }
        }
        return Sequence.Empty;
    }

    private static ISequence ArrayAppendFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var arrItem = args[0].FirstOrDefault;
        var value = args[1];
        if (arrItem is ArrayItem arr)
        {
            var newMembers = arr.Members.ToList();
            newMembers.Add(value);
            return Sequence.Of(ArrayItem.Of(newMembers));
        }
        return Sequence.Empty;
    }

    private static ISequence ArraySubarrayFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var arrItem = args[0].FirstOrDefault;
        var startItem = args[1].FirstOrDefault;
        if (arrItem is not ArrayItem arr || startItem is not IntegerItem start) return Sequence.Empty;

        var startIndex = (int)start.Value - 1;
        var length = args.Count > 2 && args[2].FirstOrDefault is IntegerItem len
            ? (int)len.Value
            : arr.Size - startIndex;

        if (startIndex < 0 || startIndex >= arr.Size) return Sequence.Of(ArrayItem.Empty);

        length = Math.Min(length, arr.Size - startIndex);
        var newMembers = arr.Members.Skip(startIndex).Take(length).ToList();
        return Sequence.Of(ArrayItem.Of(newMembers));
    }

    private static ISequence ArrayRemoveFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var arrItem = args[0].FirstOrDefault;
        var posItem = args[1].FirstOrDefault;
        if (arrItem is ArrayItem arr && posItem is IntegerItem pos)
        {
            var index = (int)pos.Value - 1;
            if (index >= 0 && index < arr.Size)
            {
                var newMembers = arr.Members.Where((_, i) => i != index).ToList();
                return Sequence.Of(ArrayItem.Of(newMembers));
            }
            return Sequence.Of(arr);
        }
        return Sequence.Empty;
    }

    private static ISequence ArrayInsertBeforeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var arrItem = args[0].FirstOrDefault;
        var posItem = args[1].FirstOrDefault;
        var value = args[2];
        if (arrItem is ArrayItem arr && posItem is IntegerItem pos)
        {
            var index = Math.Max(0, Math.Min((int)pos.Value - 1, arr.Size));
            var newMembers = arr.Members.ToList();
            newMembers.Insert(index, value);
            return Sequence.Of(ArrayItem.Of(newMembers));
        }
        return Sequence.Empty;
    }
#pragma warning restore CA1859

    private static ISequence ArrayHeadFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item is ArrayItem arr && arr.Size > 0 ? arr.Get(1) : Sequence.Empty;
    }

#pragma warning disable CA1859
    private static ISequence ArrayTailFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is ArrayItem arr && arr.Size > 1)
        {
            return Sequence.Of(ArrayItem.Of(arr.Members.Skip(1).ToList()));
        }
        return Sequence.Of(ArrayItem.Empty);
    }

    private static ISequence ArrayReverseFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is ArrayItem arr)
        {
            return Sequence.Of(ArrayItem.Of(arr.Members.Reverse().ToList()));
        }
        return Sequence.Empty;
    }

    private static ISequence ArrayJoinFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var arrays = args[0]
            .Where(item => item is ArrayItem)
            .Cast<ArrayItem>()
            .SelectMany(a => a.Members)
            .ToList();
        return Sequence.Of(ArrayItem.Of(arrays));
    }

    private static ISequence ArrayFlattenFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        static IEnumerable<IItem> Flatten(ISequence seq)
        {
            foreach (var item in seq)
            {
                if (item is ArrayItem arr)
                {
                    foreach (var member in arr.Members)
                    {
                        foreach (var flattened in Flatten(member))
                        {
                            yield return flattened;
                        }
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }

        var items = Flatten(args[0]).ToList();
        return new Sequence(items);
    }
#pragma warning restore CA1859

    // Map functions
    private static ISequence MapSizeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        return item is MapItem map ? Sequence.Of(IntegerItem.Of(map.Size)) : Sequence.Empty;
    }

#pragma warning disable CA1859
    private static ISequence MapKeysFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is MapItem map)
        {
            return new Sequence(map.Keys.Cast<IItem>());
        }
        return Sequence.Empty;
    }
#pragma warning restore CA1859

    private static ISequence MapContainsFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var mapItem = args[0].FirstOrDefault;
        var keyItem = args[1].FirstOrDefault as IAtomicItem;
        if (mapItem is MapItem map && keyItem is not null)
        {
            return Sequence.Of(BooleanItem.Of(map.ContainsKey(keyItem)));
        }
        return Sequence.Of(BooleanItem.False);
    }

    private static ISequence MapGetFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var mapItem = args[0].FirstOrDefault;
        var keyItem = args[1].FirstOrDefault as IAtomicItem;
        if (mapItem is MapItem map && keyItem is not null)
        {
            return map.Get(keyItem);
        }
        return Sequence.Empty;
    }

#pragma warning disable CA1859
    private static ISequence MapPutFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var mapItem = args[0].FirstOrDefault;
        var keyItem = args[1].FirstOrDefault as IAtomicItem;
        var value = args[2];
        if (mapItem is MapItem map && keyItem is not null)
        {
            return Sequence.Of(map.Put(keyItem, value));
        }
        return Sequence.Empty;
    }

    private static ISequence MapEntryFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var keyItem = args[0].FirstOrDefault as IAtomicItem;
        var value = args[1];
        if (keyItem is not null)
        {
            var entries = new Dictionary<IAtomicItem, ISequence>(new AtomicItemComparer()) { [keyItem] = value };
            return Sequence.Of(MapItem.Of(entries));
        }
        return Sequence.Empty;
    }

    private static ISequence MapRemoveFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var mapItem = args[0].FirstOrDefault;
        var keyItem = args[1].FirstOrDefault as IAtomicItem;
        if (mapItem is MapItem map && keyItem is not null)
        {
            return Sequence.Of(map.Remove(keyItem));
        }
        return Sequence.Of(mapItem ?? MapItem.Empty);
    }

    private static ISequence MapMergeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var entries = new Dictionary<IAtomicItem, ISequence>(new AtomicItemComparer());
        foreach (var item in args[0])
        {
            if (item is MapItem map)
            {
                foreach (var kvp in map.Entries)
                {
                    entries[kvp.Key] = kvp.Value;
                }
            }
        }
        return Sequence.Of(MapItem.Of(entries));
    }
#pragma warning restore CA1859

    // QName and node identity functions
    private static ISequence LocalNameFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        INodeItem? node;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            node = ctx.DynamicContext.ContextItem as INodeItem;
        }
        else
        {
            node = args[0].FirstOrDefault as INodeItem;
        }

        if (node is null) return Sequence.Of(StringItem.Of(string.Empty));

        var name = node.Name ?? string.Empty;
        var localName = name.Contains(':') ? name[(name.IndexOf(':') + 1)..] : name;
        return Sequence.Of(StringItem.Of(localName));
    }

    private static ISequence NameFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        INodeItem? node;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            node = ctx.DynamicContext.ContextItem as INodeItem;
        }
        else
        {
            node = args[0].FirstOrDefault as INodeItem;
        }

        return Sequence.Of(StringItem.Of(node?.Name ?? string.Empty));
    }

    private static ISequence NamespaceUriFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        INodeItem? node;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            node = ctx.DynamicContext.ContextItem as INodeItem;
        }
        else
        {
            node = args[0].FirstOrDefault as INodeItem;
        }

        return Sequence.Of(StringItem.Of(node?.NamespaceUri ?? string.Empty));
    }

    private static ISequence QNameConstructorFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var nsUri = args[0].FirstOrDefault?.GetStringValue();
        var localName = args[1].FirstOrDefault?.GetStringValue() ?? string.Empty;

        // Parse prefix from local name if present
        string? prefix = null;
        if (localName.Contains(':'))
        {
            var parts = localName.Split(':');
            prefix = parts[0];
            localName = parts[1];
        }

        return Sequence.Of(QNameItem.Of(prefix, nsUri, localName));
    }

    private static ISequence LocalNameFromQNameFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is QNameItem qn)
        {
            return Sequence.Of(StringItem.Of(qn.LocalName));
        }
        return Sequence.Empty;
    }

    private static ISequence NamespaceUriFromQNameFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is QNameItem qn)
        {
            return Sequence.Of(StringItem.Of(qn.NamespaceUri ?? string.Empty));
        }
        return Sequence.Empty;
    }

    private static ISequence PrefixFromQNameFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var item = args[0].FirstOrDefault;
        if (item is QNameItem qn && !string.IsNullOrEmpty(qn.Prefix))
        {
            return Sequence.Of(StringItem.Of(qn.Prefix));
        }
        return Sequence.Empty;
    }

    // URI functions
    private static ISequence ResolveUriFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var relative = args[0].FirstOrDefault?.GetStringValue();
        if (string.IsNullOrEmpty(relative)) return Sequence.Empty;

        Uri? baseUri;
        if (args.Count > 1 && !args[1].IsEmpty)
        {
            var baseStr = args[1].FirstOrDefault?.GetStringValue();
            if (string.IsNullOrEmpty(baseStr) || !Uri.TryCreate(baseStr, UriKind.Absolute, out baseUri))
            {
                return Sequence.Empty;
            }
        }
        else
        {
            baseUri = ctx.StaticContext.BaseUri;
        }

        if (baseUri is null)
        {
            if (Uri.TryCreate(relative, UriKind.Absolute, out var absUri))
            {
                return Sequence.Of(UriItem.Of(absUri));
            }
            return Sequence.Empty;
        }

        if (Uri.TryCreate(baseUri, relative, out var resolved))
        {
            return Sequence.Of(UriItem.Of(resolved));
        }
        return Sequence.Empty;
    }

    private static ISequence StaticBaseUriFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var baseUri = ctx.StaticContext.BaseUri;
        return baseUri is not null ? Sequence.Of(UriItem.Of(baseUri)) : Sequence.Empty;
    }

    private static ISequence EncodeForUriFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        return Sequence.Of(StringItem.Of(Uri.EscapeDataString(str)));
    }

    // Document functions
    private static ISequence DocFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        // This is a placeholder - actual implementation would require document loading infrastructure
        var uri = args[0].FirstOrDefault?.GetStringValue();
        if (string.IsNullOrEmpty(uri)) return Sequence.Empty;

        // For now, return empty - full implementation would load and parse the document
        return Sequence.Empty;
    }

    private static ISequence DocAvailableFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        // This is a placeholder - actual implementation would check if document is accessible
        var uri = args[0].FirstOrDefault?.GetStringValue();
        if (string.IsNullOrEmpty(uri)) return Sequence.Of(BooleanItem.False);

        // For now, return false - full implementation would check document availability
        return Sequence.Of(BooleanItem.False);
    }

    // Node set functions
#pragma warning disable CA1859
    private static ISequence InnermostFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var nodes = args[0].OfType<INodeItem>().ToList();
        if (nodes.Count == 0) return Sequence.Empty;

        // Remove any node that is an ancestor of another node in the set
        var result = nodes.Where(n =>
            !nodes.Any(other => other != n && IsDescendant(other, n))).ToList();

        return new Sequence(result.Cast<IItem>());
    }

    private static ISequence OutermostFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var nodes = args[0].OfType<INodeItem>().ToList();
        if (nodes.Count == 0) return Sequence.Empty;

        // Remove any node that is a descendant of another node in the set
        var result = nodes.Where(n =>
            !nodes.Any(other => other != n && IsDescendant(n, other))).ToList();

        return new Sequence(result.Cast<IItem>());
    }
#pragma warning restore CA1859

    private static bool IsDescendant(INodeItem node, INodeItem ancestor)
    {
        var current = node.Parent;
        while (current is not null)
        {
            if (ReferenceEquals(current, ancestor)) return true;
            current = current.Parent;
        }
        return false;
    }

    // Metaschema-specific functions
    private static ISequence Base64EncodeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        var bytes = System.Text.Encoding.UTF8.GetBytes(str);
        return Sequence.Of(StringItem.Of(Convert.ToBase64String(bytes)));
    }

    private static ISequence Base64DecodeFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        var str = args[0].FirstOrDefault?.GetStringValue() ?? string.Empty;
        try
        {
            var bytes = Convert.FromBase64String(str);
            return Sequence.Of(StringItem.Of(System.Text.Encoding.UTF8.GetString(bytes)));
        }
        catch (FormatException)
        {
            throw new MetapathException($"Invalid base64 string: {str}");
        }
    }

    private static ISequence RecurseDepthFunction(IMetapathContext ctx, IReadOnlyList<ISequence> args)
    {
        INodeItem? node;
        if (args.Count == 0 || args[0].IsEmpty)
        {
            node = ctx.DynamicContext.ContextItem as INodeItem;
        }
        else
        {
            node = args[0].FirstOrDefault as INodeItem;
        }

        if (node is null) return Sequence.Of(IntegerItem.Zero);

        var depth = 0;
        var current = node;
        while (current.Parent is not null)
        {
            depth++;
            current = current.Parent;
        }
        return Sequence.Of(IntegerItem.Of(depth));
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

    /// <summary>
    /// Creates a function library with all built-in functions.
    /// </summary>
    /// <returns>The shared function library with built-in functions.</returns>
    [Obsolete("Use FunctionLibrary.Default instead for better performance.")]
    public static FunctionLibrary CreateWithBuiltIns() => Default;

    private readonly record struct FunctionKey(string? NamespaceUri, string LocalName, int Arity);
}

/// <summary>
/// A simple built-in function implementation.
/// </summary>
internal sealed class BuiltInFunction : IMetapathFunction
{
    private readonly Func<IMetapathContext, IReadOnlyList<ISequence>, ISequence> _implementation;

    /// <summary>
    /// Creates a function with fixed arity.
    /// </summary>
    public BuiltInFunction(string name, int arity, Func<IMetapathContext, IReadOnlyList<ISequence>, ISequence> implementation)
        : this(null, name, arity, arity, implementation)
    {
    }

    /// <summary>
    /// Creates a function with variable arity.
    /// </summary>
    public BuiltInFunction(string name, int minArity, int maxArity, Func<IMetapathContext, IReadOnlyList<ISequence>, ISequence> implementation)
        : this(null, name, minArity, maxArity, implementation)
    {
    }

    /// <summary>
    /// Creates a function with namespace and fixed arity.
    /// </summary>
    public BuiltInFunction(string? namespaceUri, string name, int arity, Func<IMetapathContext, IReadOnlyList<ISequence>, ISequence> implementation)
        : this(namespaceUri, name, arity, arity, implementation)
    {
    }

    /// <summary>
    /// Creates a function with namespace and variable arity.
    /// </summary>
    public BuiltInFunction(string? namespaceUri, string name, int minArity, int maxArity, Func<IMetapathContext, IReadOnlyList<ISequence>, ISequence> implementation)
    {
        NamespaceUri = namespaceUri;
        Name = name;
        MinArity = minArity;
        MaxArity = maxArity;
        Arity = minArity == maxArity ? minArity : -1;
        _implementation = implementation;
    }

    public string Name { get; }
    public string? NamespaceUri { get; }
    public int Arity { get; }
    public int MinArity { get; }
    public int MaxArity { get; }

    public ISequence Invoke(IMetapathContext context, IReadOnlyList<ISequence> arguments)
        => _implementation(context, arguments);
}
