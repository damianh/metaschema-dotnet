// Licensed under the MIT License.

namespace Metaschema.Core.Datatypes;

/// <summary>
/// Base class for data type adapters providing common functionality.
/// </summary>
/// <typeparam name="T">The CLR type this adapter produces.</typeparam>
public abstract class DataTypeAdapter<T> : IDataTypeAdapter<T>
{
    /// <inheritdoc />
    public abstract string TypeName { get; }

    /// <inheritdoc />
    public Type ClrType => typeof(T);

    /// <inheritdoc />
    public abstract T Parse(string value);

    /// <inheritdoc />
    public virtual bool TryParse(string value, out T? result)
    {
        try
        {
            result = Parse(value);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc />
    public virtual DataTypeValidationResult Validate(string value)
    {
        if (TryParse(value, out _))
        {
            return DataTypeValidationResult.Valid();
        }

        return DataTypeValidationResult.Invalid($"Invalid {TypeName} value: '{value}'");
    }

    /// <inheritdoc />
    public abstract string Format(T value);

    // Explicit IDataTypeAdapter implementation
    object IDataTypeAdapter.Parse(string value) => Parse(value)!;

    bool IDataTypeAdapter.TryParse(string value, out object? result)
    {
        var success = TryParse(value, out var typed);
        result = typed;
        return success;
    }

    string IDataTypeAdapter.Format(object value) => Format((T)value);
}
