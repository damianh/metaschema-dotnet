// Licensed under the MIT License.

namespace Metaschema.SchemaGeneration;

/// <summary>
/// Placeholder class for the Metaschema.Schemagen library.
/// </summary>
public static class MetaschemaSchemagen
{
    /// <summary>
    /// Gets the library version.
    /// </summary>
    public static string Version => typeof(MetaschemaSchemagen).Assembly.GetName().Version?.ToString() ?? "0.0.0";
}
