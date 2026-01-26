// Licensed under the MIT License.

namespace Metaschema.Datatypes;

/// <summary>
/// Exception thrown when a value cannot be parsed as a specific data type.
/// </summary>
public class DataTypeParseException : MetaschemaException
{
    /// <summary>
    /// Gets the Metaschema type name that failed to parse.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets the value that failed to parse.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeParseException"/> class.
    /// </summary>
    /// <param name="typeName">The Metaschema type name.</param>
    /// <param name="value">The value that failed to parse.</param>
    /// <param name="message">The error message.</param>
    public DataTypeParseException(string typeName, string value, string message)
        : base(message)
    {
        TypeName = typeName;
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeParseException"/> class.
    /// </summary>
    /// <param name="typeName">The Metaschema type name.</param>
    /// <param name="value">The value that failed to parse.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DataTypeParseException(string typeName, string value, string message, Exception innerException)
        : base(message, innerException)
    {
        TypeName = typeName;
        Value = value;
    }

    /// <summary>
    /// Creates a parse exception for an invalid value.
    /// </summary>
    public static DataTypeParseException InvalidValue(string typeName, string value, string? reason = null)
    {
        var message = reason is not null
            ? $"Cannot parse '{value}' as {typeName}: {reason}"
            : $"Cannot parse '{value}' as {typeName}";
        return new DataTypeParseException(typeName, value, message);
    }
}
