// Licensed under the MIT License.

namespace Metaschema.Datatypes;

/// <summary>
/// Adapts string values to and from a specific Metaschema data type.
/// </summary>
public interface IDataTypeAdapter
{
    /// <summary>
    /// Gets the Metaschema type name (e.g., "string", "integer", "uuid").
    /// </summary>
    string TypeName { get; }

    /// <summary>
    /// Gets the CLR type that this adapter produces.
    /// </summary>
    Type ClrType { get; }

    /// <summary>
    /// Parses a string value into the target type.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="DataTypeParseException">If parsing fails.</exception>
    object Parse(string value);

    /// <summary>
    /// Attempts to parse a string value into the target type.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">The parsed value if successful.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    bool TryParse(string value, out object? result);

    /// <summary>
    /// Validates a string value against this data type's constraints.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <returns>The validation result.</returns>
    DataTypeValidationResult Validate(string value);

    /// <summary>
    /// Formats a value back to its canonical string representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The canonical string representation.</returns>
    string Format(object value);
}

/// <summary>
/// Generic version of the data type adapter interface.
/// </summary>
/// <typeparam name="T">The CLR type this adapter produces.</typeparam>
public interface IDataTypeAdapter<T> : IDataTypeAdapter
{
    /// <summary>
    /// Parses a string value into the target type.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="DataTypeParseException">If parsing fails.</exception>
    new T Parse(string value);

    /// <summary>
    /// Attempts to parse a string value into the target type.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">The parsed value if successful.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    bool TryParse(string value, out T? result);

    /// <summary>
    /// Formats a value back to its canonical string representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The canonical string representation.</returns>
    string Format(T value);
}
