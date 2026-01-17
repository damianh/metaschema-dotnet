// Licensed under the MIT License.

namespace Metaschema.Databind;

/// <summary>
/// Entry point for the Metaschema.Databind library providing data binding
/// for serializing and deserializing Metaschema-based content.
/// </summary>
public static class MetaschemaDatabind
{
    /// <summary>
    /// Gets the library version.
    /// </summary>
    public static string Version => typeof(MetaschemaDatabind).Assembly.GetName().Version?.ToString() ?? "0.0.0";
}
