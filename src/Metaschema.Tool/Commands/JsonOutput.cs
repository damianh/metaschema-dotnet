// Licensed under the MIT License.

using System.Text.Json;

namespace Metaschema.Tool.Commands;

/// <summary>
/// Shared utilities for CLI commands.
/// </summary>
internal static class JsonOutput
{
    /// <summary>
    /// Shared JSON serializer options for CLI output.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes an object to JSON and writes it to the console.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    public static void Write<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, Options);
        Console.WriteLine(json);
    }
}
