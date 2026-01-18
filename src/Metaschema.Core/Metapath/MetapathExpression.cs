// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Antlr4.Runtime;

using Metaschema.Core.Metapath.Context;
using Metaschema.Core.Metapath.Evaluator;
using Metaschema.Core.Metapath.Item;

namespace Metaschema.Core.Metapath;

/// <summary>
/// Represents a compiled Metapath expression that can be evaluated.
/// </summary>
public sealed class MetapathExpression : IMetapathExpression
{
    private readonly Metapath10.MetapathContext _parseTree;

    private MetapathExpression(string expression, Metapath10.MetapathContext parseTree)
    {
        Expression = expression;
        _parseTree = parseTree;
    }

    /// <inheritdoc/>
    public string Expression { get; }

    /// <summary>
    /// Compiles a Metapath expression string into an executable expression.
    /// </summary>
    /// <param name="expression">The expression string to compile.</param>
    /// <returns>The compiled expression.</returns>
    /// <exception cref="MetapathException">Thrown if the expression cannot be parsed.</exception>
    public static MetapathExpression Compile(string expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);

        var inputStream = new AntlrInputStream(expression);
        var lexer = new Metapath10Lexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new Metapath10(tokenStream);

        // Add error listener
        var errorListener = new MetapathErrorListener(expression);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);

        var tree = parser.metapath();

        if (errorListener.HasErrors)
        {
            throw new MetapathException(
                $"Failed to parse Metapath expression: {errorListener.GetErrorMessage()}");
        }

        return new MetapathExpression(expression, tree);
    }

    /// <summary>
    /// Tries to compile a Metapath expression string.
    /// </summary>
    /// <param name="expression">The expression string.</param>
    /// <param name="result">The compiled expression, if successful.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryCompile(string expression, out MetapathExpression? result)
    {
        try
        {
            result = Compile(expression);
            return true;
        }
        catch (MetapathException)
        {
            result = null;
            return false;
        }
    }

    /// <inheritdoc/>
    public ISequence Evaluate(IMetapathContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var evaluator = new MetapathEvaluator(context);
        return evaluator.Visit(_parseTree);
    }

    /// <inheritdoc/>
    public ISequence Evaluate(INodeItem contextItem)
    {
        ArgumentNullException.ThrowIfNull(contextItem);
        var context = MetapathContext.Create().WithContextItem(contextItem);
        return Evaluate(context);
    }

    /// <inheritdoc/>
    public IItem? EvaluateSingle(IMetapathContext context)
    {
        var result = Evaluate(context);
        if (result.Count > 1)
        {
            throw new MetapathException(
                $"Expected single result but got {result.Count} items.");
        }
        return result.FirstOrDefault;
    }

    /// <inheritdoc/>
    public bool EvaluateBoolean(IMetapathContext context)
    {
        var result = Evaluate(context);
        return result.GetEffectiveBooleanValue();
    }

    /// <inheritdoc/>
    public string? EvaluateString(IMetapathContext context)
    {
        var result = EvaluateSingle(context);
        return result?.GetStringValue();
    }

    /// <inheritdoc/>
    public override string ToString() => Expression;
}

/// <summary>
/// Error listener for collecting parse errors.
/// </summary>
internal sealed class MetapathErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
    private readonly string _expression;
    private readonly List<string> _errors = [];

    public MetapathErrorListener(string expression)
    {
        _expression = expression;
    }

    public bool HasErrors => _errors.Count > 0;

    public string GetErrorMessage() => string.Join("; ", _errors);

    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        _errors.Add($"Line {line}:{charPositionInLine} - {msg}");
    }

    public void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        int offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        _errors.Add($"Line {line}:{charPositionInLine} - {msg}");
    }
}
