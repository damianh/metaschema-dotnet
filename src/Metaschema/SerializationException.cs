// Licensed under the MIT License.

namespace Metaschema;

/// <summary>
/// Exception thrown when serialization or deserialization fails.
/// </summary>
public class SerializationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SerializationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public SerializationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
