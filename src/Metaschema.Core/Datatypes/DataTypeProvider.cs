// Licensed under the MIT License.

using Metaschema.Core.Datatypes.Adapters;

namespace Metaschema.Core.Datatypes;

/// <summary>
/// Default implementation of <see cref="IDataTypeProvider"/> that provides
/// all built-in Metaschema data type adapters.
/// </summary>
public sealed class DataTypeProvider : IDataTypeProvider
{
    private readonly Dictionary<string, IDataTypeAdapter> _adapters = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the default provider instance with all built-in adapters registered.
    /// </summary>
    public static DataTypeProvider Default { get; } = CreateDefault();

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeProvider"/> class.
    /// </summary>
    public DataTypeProvider()
    {
    }

    private static DataTypeProvider CreateDefault()
    {
        var provider = new DataTypeProvider();

        // String types
        provider.RegisterAdapter(new StringAdapter());
        provider.RegisterAdapter(new TokenAdapter());
        provider.RegisterAdapter(new UriAdapter());
        provider.RegisterAdapter(new UriReferenceAdapter());
        provider.RegisterAdapter(new UuidAdapter());
        provider.RegisterAdapter(new EmailAddressAdapter());
        provider.RegisterAdapter(new HostnameAdapter());

        // Numeric types
        provider.RegisterAdapter(new IntegerAdapter());
        provider.RegisterAdapter(new NonNegativeIntegerAdapter());
        provider.RegisterAdapter(new PositiveIntegerAdapter());
        provider.RegisterAdapter(new DecimalAdapter());

        // Boolean and binary
        provider.RegisterAdapter(new BooleanAdapter());
        provider.RegisterAdapter(new Base64Adapter());

        // Date/time types
        provider.RegisterAdapter(new DateAdapter());
        provider.RegisterAdapter(new DateWithTimezoneAdapter());
        provider.RegisterAdapter(new DateTimeAdapter());
        provider.RegisterAdapter(new DateTimeWithTimezoneAdapter());
        provider.RegisterAdapter(new DayTimeDurationAdapter());
        provider.RegisterAdapter(new YearMonthDurationAdapter());

        // Network types
        provider.RegisterAdapter(new Ipv4AddressAdapter());
        provider.RegisterAdapter(new Ipv6AddressAdapter());

        // Markup types
        provider.RegisterAdapter(new MarkupLineAdapter());
        provider.RegisterAdapter(new MarkupMultilineAdapter());

        return provider;
    }

    /// <inheritdoc />
    public IDataTypeAdapter? GetAdapter(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);
        return _adapters.GetValueOrDefault(typeName);
    }

    /// <inheritdoc />
    public IDataTypeAdapter<T>? GetAdapter<T>(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);
        if (_adapters.TryGetValue(typeName, out var adapter) && adapter is IDataTypeAdapter<T> typed)
        {
            return typed;
        }
        return null;
    }

    /// <inheritdoc />
    public IEnumerable<IDataTypeAdapter> GetAllAdapters() => _adapters.Values;

    /// <inheritdoc />
    public void RegisterAdapter(IDataTypeAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);
        _adapters[adapter.TypeName] = adapter;
    }
}
