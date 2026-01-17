// Licensed under the MIT License.

namespace Metaschema.Core;

/// <summary>
/// Placeholder class for the Metaschema.Core library.
/// </summary>
public static class MetaschemaCore
{
    /// <summary>
    /// Gets the library version.
    /// </summary>
    public static string Version => typeof(MetaschemaCore).Assembly.GetName().Version?.ToString() ?? "0.0.0";
}
