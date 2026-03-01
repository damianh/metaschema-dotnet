// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Antlr4.Runtime;

namespace Metaschema.Metapath.Parser;

/// <summary>
/// Base class for the Metapath10 parser providing helper methods for semantic predicates.
/// </summary>
public abstract class Metapath10ParserBase : Antlr4.Runtime.Parser
{
    /// <summary>
    /// Reserved function names that cannot be used as function calls without parentheses.
    /// These are keywords that have special meaning in the grammar.
    /// </summary>
    private static readonly HashSet<string> ReservedFunctionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "array",
        "attribute",
        "comment",
        "document-node",
        "element",
        "empty-sequence",
        "function",
        "if",
        "item",
        "map",
        "namespace-node",
        "node",
        "processing-instruction",
        "schema-attribute",
        "schema-element",
        "switch",
        "text",
        "typeswitch",
        // Metaschema-specific
        "assembly",
        "field",
        "flag"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="Metapath10ParserBase"/> class.
    /// </summary>
    /// <param name="input">The token stream input.</param>
    protected Metapath10ParserBase(ITokenStream input) : base(input)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Metapath10ParserBase"/> class.
    /// </summary>
    /// <param name="input">The token stream input.</param>
    /// <param name="output">The text writer for output.</param>
    /// <param name="errorOutput">The text writer for error output.</param>
    protected Metapath10ParserBase(ITokenStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
    {
    }

    /// <summary>
    /// Determines whether the current position represents a function call.
    /// This is used as a semantic predicate to disambiguate function calls from
    /// other constructs like axis steps or type tests.
    /// </summary>
    /// <returns><c>true</c> if the current position is a function call; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// A function call is identified by a name followed by an opening parenthesis,
    /// where the name is not a reserved function name that would indicate a type test
    /// or axis step instead.
    /// </remarks>
    public bool IsFuncCall()
    {
        // Look ahead to see if there's an opening parenthesis
        var nextTokenType = CurrentToken.Type;

        // Get the function name from the current context
        // We need to check if the next token is an opening parenthesis
        var lookAhead = TokenStream.Get(TokenStream.Index);

        // Check for NCName or QName followed by '('
        if (lookAhead == null)
        {
            return false;
        }

        // Get the token text to check against reserved names
        var tokenText = lookAhead.Text ?? string.Empty;

        // Check if this is a reserved function name
        // Reserved function names followed by '(' are NOT function calls,
        // they are type tests or other constructs
        if (ReservedFunctionNames.Contains(tokenText))
        {
            return false;
        }

        // Look ahead for the opening parenthesis
        var index = TokenStream.Index + 1;
        var nextToken = index < TokenStream.Size ? TokenStream.Get(index) : null;

        // If the next non-whitespace token is '(', this is a function call
        return nextToken?.Type == Metapath10Lexer.OP;
    }
}
