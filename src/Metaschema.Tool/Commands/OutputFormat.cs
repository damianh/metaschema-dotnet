// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Tool.Commands;

/// <summary>
/// Output format for CLI results.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// Human-readable text output.
    /// </summary>
    Text,

    /// <summary>
    /// JSON format output.
    /// </summary>
    Json,

    /// <summary>
    /// SARIF format for static analysis results.
    /// </summary>
    Sarif
}
