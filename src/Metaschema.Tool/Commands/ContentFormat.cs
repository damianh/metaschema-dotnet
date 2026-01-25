// Licensed under the MIT License.

namespace Metaschema.Tool.Commands;

/// <summary>
/// Content format for input/output files.
/// </summary>
public enum ContentFormat
{
    /// <summary>
    /// Automatically detect format from file extension or content.
    /// </summary>
    Auto,

    /// <summary>
    /// XML format.
    /// </summary>
    Xml,

    /// <summary>
    /// JSON format.
    /// </summary>
    Json,

    /// <summary>
    /// YAML format.
    /// </summary>
    Yaml
}
