// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

using Metaschema.Core.Metapath.Functions;
using Metaschema.Core.Metapath.Item;

namespace Metaschema.Core.Metapath.Evaluator;

/// <summary>
/// Evaluates Metapath expressions by visiting the parse tree.
/// </summary>
public sealed class MetapathEvaluator : Metapath10BaseVisitor<ISequence>
{
    private readonly IMetapathContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetapathEvaluator"/> class.
    /// </summary>
    /// <param name="context">The evaluation context.</param>
    public MetapathEvaluator(IMetapathContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    protected override ISequence DefaultResult => Sequence.Empty;

    /// <inheritdoc/>
    public override ISequence VisitMetapath(Metapath10.MetapathContext context)
    {
        return Visit(context.expr());
    }

    /// <inheritdoc/>
    public override ISequence VisitExpr(Metapath10.ExprContext context)
    {
        var expressions = context.exprsingle();
        if (expressions.Length == 1)
        {
            return Visit(expressions[0]);
        }

        // Multiple expressions form a sequence
        var items = new List<IItem>();
        foreach (var expr in expressions)
        {
            var result = Visit(expr);
            foreach (var item in result)
            {
                items.Add(item);
            }
        }
        return new Sequence(items);
    }

    /// <inheritdoc/>
    public override ISequence VisitOrexpr(Metapath10.OrexprContext context)
    {
        var andExprs = context.andexpr();
        if (andExprs.Length == 1)
        {
            return Visit(andExprs[0]);
        }

        // Short-circuit OR evaluation
        foreach (var andExpr in andExprs)
        {
            var result = Visit(andExpr);
            if (result.GetEffectiveBooleanValue())
            {
                return Sequence.Of(BooleanItem.True);
            }
        }
        return Sequence.Of(BooleanItem.False);
    }

    /// <inheritdoc/>
    public override ISequence VisitAndexpr(Metapath10.AndexprContext context)
    {
        var compExprs = context.comparisonexpr();
        if (compExprs.Length == 1)
        {
            return Visit(compExprs[0]);
        }

        // Short-circuit AND evaluation
        foreach (var compExpr in compExprs)
        {
            var result = Visit(compExpr);
            if (!result.GetEffectiveBooleanValue())
            {
                return Sequence.Of(BooleanItem.False);
            }
        }
        return Sequence.Of(BooleanItem.True);
    }

    /// <inheritdoc/>
    public override ISequence VisitComparisonexpr(Metapath10.ComparisonexprContext context)
    {
        var concatExprs = context.stringconcatexpr();
        if (concatExprs.Length == 1)
        {
            return Visit(concatExprs[0]);
        }

        // We have a comparison
        var left = Visit(concatExprs[0]);
        var right = Visit(concatExprs[1]);

        // Determine comparison type
        var generalComp = context.generalcomp();
        var valueComp = context.valuecomp();

        bool result;
        if (generalComp is not null)
        {
            result = EvaluateGeneralComparison(left, right, generalComp);
        }
        else if (valueComp is not null)
        {
            result = EvaluateValueComparison(left, right, valueComp);
        }
        else
        {
            throw new MetapathException("Unknown comparison type");
        }

        return Sequence.Of(BooleanItem.Of(result));
    }

    private static bool EvaluateGeneralComparison(ISequence left, ISequence right, Metapath10.GeneralcompContext comp)
    {
        // General comparison: existential semantics
        foreach (var leftItem in left)
        {
            foreach (var rightItem in right)
            {
                if (CompareItems(leftItem, rightItem, comp))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool EvaluateValueComparison(ISequence left, ISequence right, Metapath10.ValuecompContext comp)
    {
        // Value comparison: single item semantics
        if (left.Count != 1 || right.Count != 1)
        {
            return false;
        }

        var leftItem = left.FirstOrDefault!;
        var rightItem = right.FirstOrDefault!;

        return CompareItemsValue(leftItem, rightItem, comp);
    }

    private static bool CompareItems(IItem left, IItem right, Metapath10.GeneralcompContext comp)
    {
        var leftStr = left.GetStringValue();
        var rightStr = right.GetStringValue();

        // Try numeric comparison first
        if (TryGetNumericValue(left, out var leftNum) && TryGetNumericValue(right, out var rightNum))
        {
            if (comp.EQ() is not null) return leftNum == rightNum;
            if (comp.NE() is not null) return leftNum != rightNum;
            if (comp.LT() is not null) return leftNum < rightNum;
            if (comp.LE() is not null) return leftNum <= rightNum;
            if (comp.GT() is not null) return leftNum > rightNum;
            if (comp.GE() is not null) return leftNum >= rightNum;
        }

        // String comparison
        var cmp = string.Compare(leftStr, rightStr, StringComparison.Ordinal);
        if (comp.EQ() is not null) return cmp == 0;
        if (comp.NE() is not null) return cmp != 0;
        if (comp.LT() is not null) return cmp < 0;
        if (comp.LE() is not null) return cmp <= 0;
        if (comp.GT() is not null) return cmp > 0;
        if (comp.GE() is not null) return cmp >= 0;

        return false;
    }

    private static bool CompareItemsValue(IItem left, IItem right, Metapath10.ValuecompContext comp)
    {
        var leftStr = left.GetStringValue();
        var rightStr = right.GetStringValue();

        // Try numeric comparison first
        if (TryGetNumericValue(left, out var leftNum) && TryGetNumericValue(right, out var rightNum))
        {
            if (comp.KW_EQ() is not null) return leftNum == rightNum;
            if (comp.KW_NE() is not null) return leftNum != rightNum;
            if (comp.KW_LT() is not null) return leftNum < rightNum;
            if (comp.KW_LE() is not null) return leftNum <= rightNum;
            if (comp.KW_GT() is not null) return leftNum > rightNum;
            if (comp.KW_GE() is not null) return leftNum >= rightNum;
        }

        // String comparison
        var cmp = string.Compare(leftStr, rightStr, StringComparison.Ordinal);
        if (comp.KW_EQ() is not null) return cmp == 0;
        if (comp.KW_NE() is not null) return cmp != 0;
        if (comp.KW_LT() is not null) return cmp < 0;
        if (comp.KW_LE() is not null) return cmp <= 0;
        if (comp.KW_GT() is not null) return cmp > 0;
        if (comp.KW_GE() is not null) return cmp >= 0;

        return false;
    }

    private static bool TryGetNumericValue(IItem item, out decimal value)
    {
        switch (item)
        {
            case IntegerItem i:
                value = i.Value;
                return true;
            case DecimalItem d:
                value = d.Value;
                return true;
            case DoubleItem db:
                value = (decimal)db.Value;
                return true;
            default:
                if (decimal.TryParse(item.GetStringValue(), CultureInfo.InvariantCulture, out value))
                {
                    return true;
                }
                value = 0;
                return false;
        }
    }

    /// <inheritdoc/>
    public override ISequence VisitStringconcatexpr(Metapath10.StringconcatexprContext context)
    {
        var rangeExprs = context.rangeexpr();
        if (rangeExprs.Length == 1)
        {
            return Visit(rangeExprs[0]);
        }

        // String concatenation with ||
        var result = string.Concat(rangeExprs.Select(e => Visit(e).FirstOrDefault?.GetStringValue() ?? string.Empty));
        return Sequence.Of(StringItem.Of(result));
    }

    /// <inheritdoc/>
    public override ISequence VisitRangeexpr(Metapath10.RangeexprContext context)
    {
        var additiveExprs = context.additiveexpr();
        if (additiveExprs.Length == 1)
        {
            return Visit(additiveExprs[0]);
        }

        // Range expression (1 to 10)
        var startSeq = Visit(additiveExprs[0]);
        var endSeq = Visit(additiveExprs[1]);

        if (startSeq.IsEmpty || endSeq.IsEmpty)
        {
            return Sequence.Empty;
        }

        var startItem = startSeq.FirstOrDefault;
        var endItem = endSeq.FirstOrDefault;

        if (!TryGetNumericValue(startItem!, out var startVal) || !TryGetNumericValue(endItem!, out var endVal))
        {
            return Sequence.Empty;
        }

        var start = (long)startVal;
        var end = (long)endVal;

        if (start > end)
        {
            return Sequence.Empty;
        }

        var items = new List<IItem>();
        for (var i = start; i <= end; i++)
        {
            items.Add(IntegerItem.Of(i));
        }
        return new Sequence(items);
    }

    /// <inheritdoc/>
    public override ISequence VisitAdditiveexpr(Metapath10.AdditiveexprContext context)
    {
        var multExprs = context.multiplicativeexpr();
        var operators = context.children?
            .Where(c => c.GetText() == "+" || c.GetText() == "-")
            .Select(c => c.GetText())
            .ToList() ?? [];

        if (multExprs.Length == 1)
        {
            return Visit(multExprs[0]);
        }

        var result = Visit(multExprs[0]);
        if (!TryGetNumericValue(result.FirstOrDefault!, out var value))
        {
            return Sequence.Empty;
        }

        for (var i = 1; i < multExprs.Length; i++)
        {
            var nextResult = Visit(multExprs[i]);
            if (!TryGetNumericValue(nextResult.FirstOrDefault!, out var nextValue))
            {
                return Sequence.Empty;
            }

            if (operators[i - 1] == "+")
            {
                value += nextValue;
            }
            else
            {
                value -= nextValue;
            }
        }

        return Sequence.Of(DecimalItem.Of(value));
    }

    /// <inheritdoc/>
    public override ISequence VisitMultiplicativeexpr(Metapath10.MultiplicativeexprContext context)
    {
        var unionExprs = context.unionexpr();

        if (unionExprs.Length == 1)
        {
            return Visit(unionExprs[0]);
        }

        var result = Visit(unionExprs[0]);
        if (!TryGetNumericValue(result.FirstOrDefault!, out var value))
        {
            return Sequence.Empty;
        }

        var childIndex = 1;
        for (var i = 1; i < unionExprs.Length; i++)
        {
            // Find the operator between union expressions
            var op = "*";
            while (childIndex < context.ChildCount)
            {
                var child = context.GetChild(childIndex);
                var text = child.GetText();
                if (text == "*" || text == "div" || text == "idiv" || text == "mod")
                {
                    op = text;
                    childIndex++;
                    break;
                }
                childIndex++;
            }
            childIndex++; // Skip to next expression

            var nextResult = Visit(unionExprs[i]);
            if (!TryGetNumericValue(nextResult.FirstOrDefault!, out var nextValue))
            {
                return Sequence.Empty;
            }

            value = op switch
            {
                "*" => value * nextValue,
                "div" => value / nextValue,
                "idiv" => Math.Truncate(value / nextValue),
                "mod" => value % nextValue,
                _ => value
            };
        }

        return Sequence.Of(DecimalItem.Of(value));
    }

    /// <inheritdoc/>
    public override ISequence VisitUnaryexpr(Metapath10.UnaryexprContext context)
    {
        var result = Visit(context.valueexpr());

        // Count minus signs
        var minusCount = context.children?.Count(c => c.GetText() == "-") ?? 0;

        if (minusCount % 2 == 1 && result.FirstOrDefault is not null)
        {
            // Negate the value
            var item = result.FirstOrDefault;
            return item switch
            {
                IntegerItem i => Sequence.Of(IntegerItem.Of(-i.Value)),
                DecimalItem d => Sequence.Of(DecimalItem.Of(-d.Value)),
                DoubleItem db => Sequence.Of(DoubleItem.Of(-db.Value)),
                _ => result
            };
        }

        return result;
    }

    /// <inheritdoc/>
    public override ISequence VisitLiteral(Metapath10.LiteralContext context)
    {
        var numericLiteral = context.numericliteral();
        if (numericLiteral is not null)
        {
            return Visit(numericLiteral);
        }

        var stringLiteral = context.StringLiteral();
        if (stringLiteral is not null)
        {
            // Remove quotes and handle escape sequences
            var text = stringLiteral.GetText();
            text = text[1..^1]; // Remove surrounding quotes
            text = text.Replace("''", "'").Replace("\"\"", "\"");
            return Sequence.Of(StringItem.Of(text));
        }

        return Sequence.Empty;
    }

    /// <inheritdoc/>
    public override ISequence VisitNumericliteral(Metapath10.NumericliteralContext context)
    {
        var intLiteral = context.IntegerLiteral();
        if (intLiteral is not null)
        {
            if (long.TryParse(intLiteral.GetText(), CultureInfo.InvariantCulture, out var value))
            {
                return Sequence.Of(IntegerItem.Of(value));
            }
        }

        var decLiteral = context.DecimalLiteral();
        if (decLiteral is not null)
        {
            if (decimal.TryParse(decLiteral.GetText(), CultureInfo.InvariantCulture, out var value))
            {
                return Sequence.Of(DecimalItem.Of(value));
            }
        }

        var dblLiteral = context.DoubleLiteral();
        if (dblLiteral is not null)
        {
            if (double.TryParse(dblLiteral.GetText(), CultureInfo.InvariantCulture, out var value))
            {
                return Sequence.Of(DoubleItem.Of(value));
            }
        }

        return Sequence.Empty;
    }

    /// <inheritdoc/>
    public override ISequence VisitContextitemexpr(Metapath10.ContextitemexprContext context)
    {
        var contextItem = _context.DynamicContext.ContextItem;
        return contextItem is not null ? Sequence.Of(contextItem) : Sequence.Empty;
    }

    /// <inheritdoc/>
    public override ISequence VisitVarref(Metapath10.VarrefContext context)
    {
        var varName = context.varname().GetText();
        return _context.DynamicContext.GetVariable(varName);
    }

    /// <inheritdoc/>
    public override ISequence VisitParenthesizedexpr(Metapath10.ParenthesizedexprContext context)
    {
        var expr = context.expr();
        return expr is not null ? Visit(expr) : Sequence.Empty;
    }

    /// <inheritdoc/>
    public override ISequence VisitFunctioncall(Metapath10.FunctioncallContext context)
    {
        var name = context.eqname().GetText();
        var args = context.argumentlist().argument();

        // Evaluate arguments
        var evaluatedArgs = new List<ISequence>();
        foreach (var arg in args)
        {
            evaluatedArgs.Add(Visit(arg.exprsingle()));
        }

        // Look up function
        var function = _context.StaticContext.FunctionLibrary.GetFunction(name, evaluatedArgs.Count);
        if (function is null)
        {
            throw new MetapathException($"Unknown function: {name} with {evaluatedArgs.Count} arguments");
        }

        return function.Invoke(_context, evaluatedArgs);
    }

    /// <inheritdoc/>
    public override ISequence VisitIfexpr(Metapath10.IfexprContext context)
    {
        var condition = Visit(context.expr());
        if (condition.GetEffectiveBooleanValue())
        {
            return Visit(context.exprsingle(0));
        }
        else
        {
            return Visit(context.exprsingle(1));
        }
    }

    /// <inheritdoc/>
    public override ISequence VisitForexpr(Metapath10.ForexprContext context)
    {
        return EvaluateForClause(context.simpleforclause(), context.exprsingle(), 0);
    }

    private ISequence EvaluateForClause(Metapath10.SimpleforclauseContext clause, Metapath10.ExprsingleContext returnExpr, int bindingIndex)
    {
        var bindings = clause.simpleforbinding();
        if (bindingIndex >= bindings.Length)
        {
            return Visit(returnExpr);
        }

        var binding = bindings[bindingIndex];
        var varName = binding.varname().GetText();
        var inExpr = Visit(binding.exprsingle());

        var results = new List<IItem>();
        foreach (var item in inExpr)
        {
            var newContext = _context.WithVariable(varName, Sequence.Of(item));
            var evaluator = new MetapathEvaluator(newContext);
            var result = evaluator.EvaluateForClause(clause, returnExpr, bindingIndex + 1);
            foreach (var r in result)
            {
                results.Add(r);
            }
        }

        return new Sequence(results);
    }

    /// <inheritdoc/>
    public override ISequence VisitLetexpr(Metapath10.LetexprContext context)
    {
        return EvaluateLetClause(context.simpleletclause(), context.exprsingle(), 0);
    }

    private ISequence EvaluateLetClause(Metapath10.SimpleletclauseContext clause, Metapath10.ExprsingleContext returnExpr, int bindingIndex)
    {
        var bindings = clause.simpleletbinding();
        if (bindingIndex >= bindings.Length)
        {
            return Visit(returnExpr);
        }

        var binding = bindings[bindingIndex];
        var varName = binding.varname().GetText();
        var valueExpr = Visit(binding.exprsingle());

        var newContext = _context.WithVariable(varName, valueExpr);
        var evaluator = new MetapathEvaluator(newContext);
        return evaluator.EvaluateLetClause(clause, returnExpr, bindingIndex + 1);
    }

    /// <inheritdoc/>
    public override ISequence VisitQuantifiedexpr(Metapath10.QuantifiedexprContext context)
    {
        var isSome = context.KW_SOME() is not null;
        var satisfiesExpr = context.exprsingle().Last();
        var varNames = context.varname();
        var inExprs = context.exprsingle().Take(context.exprsingle().Length - 1).ToArray();

        return EvaluateQuantified(isSome, varNames.Select(v => v.GetText()).ToArray(), inExprs, satisfiesExpr, 0);
    }

    private ISequence EvaluateQuantified(bool isSome, string[] varNames, Metapath10.ExprsingleContext[] inExprs,
        Metapath10.ExprsingleContext satisfiesExpr, int index)
    {
        if (index >= varNames.Length)
        {
            var result = Visit(satisfiesExpr);
            return Sequence.Of(BooleanItem.Of(result.GetEffectiveBooleanValue()));
        }

        var varName = varNames[index];
        var inSeq = Visit(inExprs[index]);

        foreach (var item in inSeq)
        {
            var newContext = _context.WithVariable(varName, Sequence.Of(item));
            var evaluator = new MetapathEvaluator(newContext);
            var result = evaluator.EvaluateQuantified(isSome, varNames, inExprs, satisfiesExpr, index + 1);

            if (isSome && result.GetEffectiveBooleanValue())
            {
                return Sequence.Of(BooleanItem.True);
            }
            if (!isSome && !result.GetEffectiveBooleanValue())
            {
                return Sequence.Of(BooleanItem.False);
            }
        }

        return Sequence.Of(BooleanItem.Of(!isSome));
    }

    // Path expressions will be implemented in phase 3 continuation
    // For now, basic path support
    /// <inheritdoc/>
    public override ISequence VisitPathexpr(Metapath10.PathexprContext context)
    {
        var relativePath = context.relativepathexpr();
        if (relativePath is null)
        {
            // Just '/' - return document root
            var contextItem = _context.DynamicContext.ContextItem;
            if (contextItem is INodeItem node)
            {
                // Navigate to root
                while (node.Parent is not null)
                {
                    node = node.Parent;
                }
                return Sequence.Of(node);
            }
            return Sequence.Empty;
        }

        // Check for absolute path (starts with /)
        if (context.SLASH() is not null || context.SS() is not null)
        {
            // Start from document root
            var contextItem = _context.DynamicContext.ContextItem;
            if (contextItem is INodeItem node)
            {
                while (node.Parent is not null)
                {
                    node = node.Parent;
                }
                var rootContext = _context.WithContextItem(node);
                var evaluator = new MetapathEvaluator(rootContext);
                return evaluator.Visit(relativePath);
            }
            return Sequence.Empty;
        }

        return Visit(relativePath);
    }

    /// <inheritdoc/>
    public override ISequence VisitRelativepathexpr(Metapath10.RelativepathexprContext context)
    {
        var steps = context.stepexpr();
        if (steps.Length == 0)
        {
            return Sequence.Empty;
        }

        var currentSequence = Visit(steps[0]);

        for (var i = 1; i < steps.Length; i++)
        {
            var nextItems = new List<IItem>();
            foreach (var item in currentSequence)
            {
                var stepContext = _context.WithContextItem(item);
                var evaluator = new MetapathEvaluator(stepContext);
                var stepResult = evaluator.Visit(steps[i]);
                foreach (var resultItem in stepResult)
                {
                    nextItems.Add(resultItem);
                }
            }
            currentSequence = new Sequence(nextItems);
        }

        return currentSequence;
    }

    /// <inheritdoc/>
    public override ISequence VisitUnionexpr(Metapath10.UnionexprContext context)
    {
        var exprs = context.intersectexceptexpr();
        if (exprs.Length == 1)
        {
            return Visit(exprs[0]);
        }

        // Union of all results
        var items = new List<IItem>();
        foreach (var expr in exprs)
        {
            var result = Visit(expr);
            foreach (var item in result)
            {
                if (!items.Contains(item))
                {
                    items.Add(item);
                }
            }
        }
        return new Sequence(items);
    }

    /// <inheritdoc/>
    public override ISequence VisitIntersectexceptexpr(Metapath10.IntersectexceptexprContext context)
    {
        var exprs = context.instanceofexpr();
        if (exprs.Length == 1)
        {
            return Visit(exprs[0]);
        }

        // Get the first sequence as the base
        var result = Visit(exprs[0]).ToList();

        // Process each subsequent expression with its operator
        var childIndex = 1;
        for (var i = 1; i < exprs.Length; i++)
        {
            // Find the operator (intersect or except)
            var op = "intersect";
            while (childIndex < context.ChildCount)
            {
                var child = context.GetChild(childIndex);
                var text = child.GetText();
                if (text == "intersect" || text == "except")
                {
                    op = text;
                    childIndex++;
                    break;
                }
                childIndex++;
            }
            childIndex++;

            var other = Visit(exprs[i]).ToList();
            var otherSet = new HashSet<string>(other.Select(item => item.GetStringValue()), StringComparer.Ordinal);

            if (op == "intersect")
            {
                // Keep only items that are in both sequences
                result = result.Where(item => otherSet.Contains(item.GetStringValue())).ToList();
            }
            else // except
            {
                // Keep only items that are NOT in the other sequence
                result = result.Where(item => !otherSet.Contains(item.GetStringValue())).ToList();
            }
        }

        return new Sequence(result);
    }

    /// <inheritdoc/>
    public override ISequence VisitInstanceofexpr(Metapath10.InstanceofexprContext context)
    {
        var treatExpr = context.treatexpr();
        var result = Visit(treatExpr);

        // If no "instance of" clause, just return the result
        if (context.KW_INSTANCE() is null)
        {
            return result;
        }

        // Type checking would go here
        // For now, just return the result
        return result;
    }

    /// <inheritdoc/>
    public override ISequence VisitTreatexpr(Metapath10.TreatexprContext context)
    {
        return Visit(context.castableexpr());
    }

    /// <inheritdoc/>
    public override ISequence VisitCastableexpr(Metapath10.CastableexprContext context)
    {
        return Visit(context.castexpr());
    }

    /// <inheritdoc/>
    public override ISequence VisitCastexpr(Metapath10.CastexprContext context)
    {
        return Visit(context.arrowexpr());
    }

    /// <inheritdoc/>
    public override ISequence VisitArrowexpr(Metapath10.ArrowexprContext context)
    {
        var result = Visit(context.unaryexpr());

        var arrowFuncs = context.arrowfunctionspecifier();
        if (arrowFuncs.Length == 0)
        {
            return result;
        }

        // Arrow function calls: expr => func(args)
        foreach (var arrowFunc in arrowFuncs)
        {
            var funcName = arrowFunc.eqname()?.GetText();
            if (funcName is null) continue;

            var argList = context.argumentlist().FirstOrDefault();
            var args = new List<ISequence> { result };

            if (argList is not null)
            {
                foreach (var arg in argList.argument())
                {
                    args.Add(Visit(arg.exprsingle()));
                }
            }

            var function = _context.StaticContext.FunctionLibrary.GetFunction(funcName, args.Count);
            if (function is null)
            {
                throw new MetapathException($"Unknown function: {funcName}");
            }

            result = function.Invoke(_context, args);
        }

        return result;
    }

    /// <inheritdoc/>
    public override ISequence VisitValueexpr(Metapath10.ValueexprContext context)
    {
        return Visit(context.simplemapexpr());
    }

    /// <inheritdoc/>
    public override ISequence VisitSimplemapexpr(Metapath10.SimplemapexprContext context)
    {
        var pathExprs = context.pathexpr();
        if (pathExprs.Length == 1)
        {
            return Visit(pathExprs[0]);
        }

        // Simple map: left ! right
        var leftResult = Visit(pathExprs[0]);
        var results = new List<IItem>();

        foreach (var leftItem in leftResult)
        {
            var mapContext = _context.WithContextItem(leftItem);
            var evaluator = new MetapathEvaluator(mapContext);
            for (var i = 1; i < pathExprs.Length; i++)
            {
                var rightResult = evaluator.Visit(pathExprs[i]);
                foreach (var item in rightResult)
                {
                    results.Add(item);
                }
            }
        }

        return new Sequence(results);
    }

    /// <inheritdoc/>
    public override ISequence VisitStepexpr(Metapath10.StepexprContext context)
    {
        var postfixExpr = context.postfixexpr();
        if (postfixExpr is not null)
        {
            return Visit(postfixExpr);
        }

        var axisStep = context.axisstep();
        if (axisStep is not null)
        {
            return Visit(axisStep);
        }

        return Sequence.Empty;
    }

    /// <inheritdoc/>
    public override ISequence VisitPostfixexpr(Metapath10.PostfixexprContext context)
    {
        var primaryResult = Visit(context.primaryexpr());

        // Apply predicates
        var predicates = context.predicate();
        foreach (var predicate in predicates)
        {
            primaryResult = ApplyPredicate(primaryResult, predicate);
        }

        return primaryResult;
    }

    private Sequence ApplyPredicate(ISequence sequence, Metapath10.PredicateContext predicate)
    {
        var results = new List<IItem>();
        var position = 1;
        var size = sequence.Count;

        foreach (var item in sequence)
        {
            var predicateContext = _context.WithContextItem(item);
            // Would also need to set position and size in dynamic context
            var evaluator = new MetapathEvaluator(predicateContext);
            var predicateResult = evaluator.Visit(predicate.expr());

            // Numeric predicate: position check
            if (predicateResult.FirstOrDefault is IntegerItem posItem)
            {
                if (posItem.Value == position)
                {
                    results.Add(item);
                }
            }
            else if (predicateResult.GetEffectiveBooleanValue())
            {
                results.Add(item);
            }

            position++;
        }

        return new Sequence(results);
    }

    /// <inheritdoc/>
    public override ISequence VisitPrimaryexpr(Metapath10.PrimaryexprContext context)
    {
        // Try each primary expression type
        var literal = context.literal();
        if (literal is not null) return Visit(literal);

        var varref = context.varref();
        if (varref is not null) return Visit(varref);

        var parenExpr = context.parenthesizedexpr();
        if (parenExpr is not null) return Visit(parenExpr);

        var contextItem = context.contextitemexpr();
        if (contextItem is not null) return Visit(contextItem);

        var funcCall = context.functioncall();
        if (funcCall is not null) return Visit(funcCall);

        var arrayConstr = context.arrayconstructor();
        if (arrayConstr is not null) return Visit(arrayConstr);

        var mapConstr = context.mapconstructor();
        if (mapConstr is not null) return Visit(mapConstr);

        return Sequence.Empty;
    }

    /// <inheritdoc/>
    public override ISequence VisitAxisstep(Metapath10.AxisstepContext context)
    {
        var contextItem = _context.DynamicContext.ContextItem;
        if (contextItem is not INodeItem node)
        {
            return Sequence.Empty;
        }

        // Get nodes from the axis
        IEnumerable<INodeItem> axisNodes;

        var forwardStep = context.forwardstep();
        var reverseStep = context.reversestep();

        if (forwardStep is not null)
        {
            axisNodes = GetForwardAxisNodes(node, forwardStep);
        }
        else if (reverseStep is not null)
        {
            axisNodes = GetReverseAxisNodes(node, reverseStep);
        }
        else
        {
            return Sequence.Empty;
        }

        // Apply predicates
        ISequence result = new Sequence(axisNodes.Cast<IItem>());
        var predicates = context.predicatelist()?.predicate() ?? [];
        foreach (var predicate in predicates)
        {
            result = ApplyPredicate(result, predicate);
        }

        return result;
    }

    private static IEnumerable<INodeItem> GetForwardAxisNodes(INodeItem node, Metapath10.ForwardstepContext forwardStep)
    {
        var axis = forwardStep.forwardaxis();
        var abbrev = forwardStep.abbrevforwardstep();
        var nodeTest = forwardStep.nodetest() ?? abbrev?.nodetest();

        if (abbrev is not null)
        {
            // Abbreviated step: name or @name
            var isAttribute = abbrev.AT() is not null;
            if (isAttribute)
            {
                // Get flags
                foreach (var flag in node.GetFlags())
                {
                    if (MatchesNodeTest(flag, nodeTest))
                    {
                        yield return flag;
                    }
                }
            }
            else
            {
                // Get children
                foreach (var child in node.GetChildren())
                {
                    if (MatchesNodeTest(child, nodeTest))
                    {
                        yield return child;
                    }
                }
            }
        }
        else if (axis is not null)
        {
            // Full axis specification
            if (axis.KW_CHILD() is not null)
            {
                foreach (var child in node.GetChildren())
                {
                    if (MatchesNodeTest(child, nodeTest))
                    {
                        yield return child;
                    }
                }
            }
            else if (axis.KW_SELF() is not null)
            {
                if (MatchesNodeTest(node, nodeTest))
                {
                    yield return node;
                }
            }
            else if (axis.KW_DESCENDANT() is not null || axis.KW_DESCENDANT_OR_SELF() is not null)
            {
                if (axis.KW_DESCENDANT_OR_SELF() is not null && MatchesNodeTest(node, nodeTest))
                {
                    yield return node;
                }
                foreach (var desc in GetDescendants(node))
                {
                    if (MatchesNodeTest(desc, nodeTest))
                    {
                        yield return desc;
                    }
                }
            }
            else if (axis.KW_FLAG() is not null)
            {
                foreach (var flag in node.GetFlags())
                {
                    if (MatchesNodeTest(flag, nodeTest))
                    {
                        yield return flag;
                    }
                }
            }
        }
    }

    private static IEnumerable<INodeItem> GetReverseAxisNodes(INodeItem node, Metapath10.ReversestepContext reverseStep)
    {
        var axis = reverseStep.reverseaxis();
        var abbrev = reverseStep.abbrevreversestep();

        if (abbrev is not null)
        {
            // '..' - parent
            if (node.Parent is not null)
            {
                yield return node.Parent;
            }
        }
        else if (axis is not null)
        {
            var nodeTest = reverseStep.nodetest();

            if (axis.KW_PARENT() is not null)
            {
                if (node.Parent is not null && MatchesNodeTest(node.Parent, nodeTest))
                {
                    yield return node.Parent;
                }
            }
            else if (axis.KW_ANCESTOR() is not null || axis.KW_ANCESTOR_OR_SELF() is not null)
            {
                if (axis.KW_ANCESTOR_OR_SELF() is not null && MatchesNodeTest(node, nodeTest))
                {
                    yield return node;
                }
                var current = node.Parent;
                while (current is not null)
                {
                    if (MatchesNodeTest(current, nodeTest))
                    {
                        yield return current;
                    }
                    current = current.Parent;
                }
            }
        }
    }

    private static IEnumerable<INodeItem> GetDescendants(INodeItem node)
    {
        foreach (var child in node.GetChildren())
        {
            yield return child;
            foreach (var desc in GetDescendants(child))
            {
                yield return desc;
            }
        }
    }

    private static bool MatchesNodeTest(INodeItem node, Metapath10.NodetestContext? nodeTest)
    {
        if (nodeTest is null)
        {
            return true;
        }

        var kindTest = nodeTest.kindtest();
        if (kindTest is not null)
        {
            return MatchesKindTest(node, kindTest);
        }

        var nameTest = nodeTest.nametest();
        if (nameTest is not null)
        {
            return MatchesNameTest(node, nameTest);
        }

        return true;
    }

    private static bool MatchesKindTest(INodeItem node, Metapath10.KindtestContext kindTest)
    {
        if (kindTest.anykindtest() is not null)
        {
            return true;
        }
        if (kindTest.documenttest() is not null)
        {
            return node.NodeType == NodeType.Document;
        }
        if (kindTest.fieldtest() is not null)
        {
            return node.NodeType == NodeType.Field;
        }
        if (kindTest.assemblytest() is not null)
        {
            return node.NodeType == NodeType.Assembly;
        }
        if (kindTest.flagtest() is not null)
        {
            return node.NodeType == NodeType.Flag;
        }
        return true;
    }

    private static bool MatchesNameTest(INodeItem node, Metapath10.NametestContext nameTest)
    {
        var wildcard = nameTest.wildcard();
        if (wildcard is not null)
        {
            if (wildcard.STAR() is not null && wildcard.ChildCount == 1)
            {
                return true; // Match any name
            }
            // Prefix or local name wildcard - simplified implementation
            return true;
        }

        var eqname = nameTest.eqname();
        if (eqname is not null)
        {
            var testName = eqname.GetText();
            return string.Equals(node.Name, testName, StringComparison.Ordinal);
        }

        return true;
    }
}
