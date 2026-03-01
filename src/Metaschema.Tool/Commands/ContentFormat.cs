// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

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
