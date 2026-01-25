// Licensed under the MIT License.

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
