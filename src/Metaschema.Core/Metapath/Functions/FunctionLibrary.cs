// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Metaschema.Core.Metapath.Item;

namespace Metaschema.Core.Metapath.Functions;

/// <summary>
/// Default implementation of <see cref="IFunctionLibrary"/>.
/// </summary>
public sealed class FunctionLibrary : IFunctionLibrary
{
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

    /// <summary>
    /// Creates a function library with all built-in functions.
    /// </summary>
    /// <returns>A new function library with built-in functions.</returns>
    public static FunctionLibrary CreateWithBuiltIns() => new(includeBuiltIn: true);

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
        : this(name, arity, arity, implementation)
    {
    }

    /// <summary>
    /// Creates a function with variable arity.
    /// </summary>
    public BuiltInFunction(string name, int minArity, int maxArity, Func<IMetapathContext, IReadOnlyList<ISequence>, ISequence> implementation)
    {
        Name = name;
        MinArity = minArity;
        MaxArity = maxArity;
        Arity = minArity == maxArity ? minArity : -1;
        _implementation = implementation;
    }

    public string Name { get; }
    public string? NamespaceUri => null;
    public int Arity { get; }
    public int MinArity { get; }
    public int MaxArity { get; }

    public ISequence Invoke(IMetapathContext context, IReadOnlyList<ISequence> arguments)
        => _implementation(context, arguments);
}
