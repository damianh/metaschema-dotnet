// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema;

/// <summary>
/// Base exception for all Metaschema-related errors.
/// </summary>
public class MetaschemaException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetaschemaException"/> class.
    /// </summary>
    public MetaschemaException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetaschemaException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public MetaschemaException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetaschemaException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MetaschemaException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
