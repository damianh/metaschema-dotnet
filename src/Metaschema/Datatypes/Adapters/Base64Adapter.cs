// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "base64" data type.
/// A string representing arbitrary binary data encoded using Base64.
/// </summary>
public sealed partial class Base64Adapter : DataTypeAdapter<byte[]>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.Base64;

    // Pattern for valid base64 (trimmed, proper padding)
    [GeneratedRegex(@"^[0-9A-Za-z+/]+={0,2}$", RegexOptions.Compiled)]
    private static partial Regex Base64Pattern();

    /// <inheritdoc />
    public override byte[] Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!Base64Pattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be valid Base64 encoded data");
        }

        try
        {
            return Convert.FromBase64String(trimmed);
        }
        catch (FormatException ex)
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, ex.Message);
        }
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out byte[]? result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return false;
        }

        var trimmed = value.Trim();
        if (!Base64Pattern().IsMatch(trimmed))
        {
            result = null;
            return false;
        }

        try
        {
            result = Convert.FromBase64String(trimmed);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <inheritdoc />
    public override string Format(byte[] value) => Convert.ToBase64String(value);
}
