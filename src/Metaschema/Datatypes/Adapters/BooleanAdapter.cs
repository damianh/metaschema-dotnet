// Licensed under the MIT License.

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "boolean" data type.
/// A boolean value that can be "true", "false", "1", or "0".
/// </summary>
public sealed class BooleanAdapter : DataTypeAdapter<bool>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.Boolean;

    /// <inheritdoc />
    public override bool Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Trim().ToLowerInvariant() switch
        {
            "true" or "1" => true,
            "false" or "0" => false,
            _ => throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be 'true', 'false', '1', or '0'")
        };
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out bool result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = false;
            return false;
        }

        switch (value.Trim().ToLowerInvariant())
        {
            case "true" or "1":
                result = true;
                return true;
            case "false" or "0":
                result = false;
                return true;
            default:
                result = false;
                return false;
        }
    }

    /// <inheritdoc />
    public override string Format(bool value) => value ? "true" : "false";
}
