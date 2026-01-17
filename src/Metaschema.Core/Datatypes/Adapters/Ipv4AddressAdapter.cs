// Licensed under the MIT License.

using System.Net;
using System.Text.RegularExpressions;

namespace Metaschema.Core.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "ip-v4-address" data type.
/// An Internet Protocol version 4 address represented using dotted-quad syntax.
/// </summary>
public sealed partial class Ipv4AddressAdapter : DataTypeAdapter<IPAddress>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.Ipv4Address;

    // Pattern for IPv4: four octets 0-255 separated by dots
    [GeneratedRegex(@"^((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])$",
        RegexOptions.Compiled)]
    private static partial Regex Ipv4Pattern();

    /// <inheritdoc />
    public override IPAddress Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!Ipv4Pattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid IPv4 address in dotted-quad format (e.g., '192.168.1.1')");
        }

        return IPAddress.Parse(trimmed);
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out IPAddress? result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return false;
        }

        var trimmed = value.Trim();
        if (!Ipv4Pattern().IsMatch(trimmed))
        {
            result = null;
            return false;
        }

        return IPAddress.TryParse(trimmed, out result);
    }

    /// <inheritdoc />
    public override string Format(IPAddress value) => value.ToString();
}
