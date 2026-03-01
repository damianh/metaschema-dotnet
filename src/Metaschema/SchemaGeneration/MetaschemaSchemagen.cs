// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

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
