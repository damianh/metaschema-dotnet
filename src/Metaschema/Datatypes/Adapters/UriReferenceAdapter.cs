// Licensed under the MIT License.

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "uri-reference" data type.
/// A URI Reference, either a URI or a relative-reference, formatted according to RFC3986.
/// </summary>
public sealed class UriReferenceAdapter : DataTypeAdapter<Uri>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.UriReference;

    /// <inheritdoc />
    public override Uri Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!System.Uri.TryCreate(trimmed, UriKind.RelativeOrAbsolute, out var uri))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value must be a valid URI reference");
        }

        return uri;
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out Uri? result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return false;
        }

        return System.Uri.TryCreate(value.Trim(), UriKind.RelativeOrAbsolute, out result);
    }

    /// <inheritdoc />
    public override string Format(Uri value) => value.ToString();
}
