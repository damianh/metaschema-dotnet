// Licensed under the MIT License.

namespace Metaschema.Testing;

/// <summary>
/// Test utilities for Metaschema-based testing.
/// </summary>
public static class MetaschemaTesting
{
    /// <summary>
    /// Gets the library version.
    /// </summary>
    public static string Version => typeof(MetaschemaTesting).Assembly.GetName().Version?.ToString() ?? "0.0.0";
}
