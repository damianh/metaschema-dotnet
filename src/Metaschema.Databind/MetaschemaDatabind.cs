// Licensed under the MIT License.

namespace Metaschema.Databind;

/// <summary>
/// Placeholder class for the Metaschema.Databind library.
/// </summary>
public static class MetaschemaDatabind
{
    /// <summary>
    /// Gets the library version.
    /// </summary>
    public static string Version => typeof(MetaschemaDatabind).Assembly.GetName().Version?.ToString() ?? "0.0.0";
}
