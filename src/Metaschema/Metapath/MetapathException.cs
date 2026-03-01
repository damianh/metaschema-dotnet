// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Metapath;

/// <summary>
/// Exception thrown when a Metapath expression fails to parse or evaluate.
/// </summary>
public class MetapathException : MetaschemaException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetapathException"/> class.
    /// </summary>
    public MetapathException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetapathException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public MetapathException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetapathException"/> class with a specified error message
    /// and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MetapathException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
