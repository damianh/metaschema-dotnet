// Licensed under the MIT License.

namespace Metaschema.Datatypes;

/// <summary>
/// Provides access to data type adapters.
/// </summary>
public interface IDataTypeProvider
{
    /// <summary>
    /// Gets an adapter by Metaschema type name.
    /// </summary>
    /// <param name="typeName">The Metaschema type name.</param>
    /// <returns>The adapter, or null if not found.</returns>
    IDataTypeAdapter? GetAdapter(string typeName);

    /// <summary>
    /// Gets a typed adapter by Metaschema type name.
    /// </summary>
    /// <typeparam name="T">The expected CLR type.</typeparam>
    /// <param name="typeName">The Metaschema type name.</param>
    /// <returns>The typed adapter, or null if not found or type doesn't match.</returns>
    IDataTypeAdapter<T>? GetAdapter<T>(string typeName);

    /// <summary>
    /// Gets all registered adapters.
    /// </summary>
    /// <returns>An enumerable of all adapters.</returns>
    IEnumerable<IDataTypeAdapter> GetAllAdapters();

    /// <summary>
    /// Registers a custom adapter.
    /// </summary>
    /// <param name="adapter">The adapter to register.</param>
    void RegisterAdapter(IDataTypeAdapter adapter);
}
