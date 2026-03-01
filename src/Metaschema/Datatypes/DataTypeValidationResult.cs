// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Datatypes;

/// <summary>
/// Result of validating a value against a data type.
/// </summary>
public readonly record struct DataTypeValidationResult
{
    /// <summary>
    /// Gets whether the validation succeeded.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static DataTypeValidationResult Valid() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="message">The error message.</param>
    public static DataTypeValidationResult Invalid(string message) =>
        new() { IsValid = false, ErrorMessage = message };
}
