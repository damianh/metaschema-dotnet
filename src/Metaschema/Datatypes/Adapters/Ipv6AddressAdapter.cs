// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Metaschema.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "ip-v6-address" data type.
/// An Internet Protocol version 6 address represented using the syntax defined in RFC3513.
/// </summary>
public sealed partial class Ipv6AddressAdapter : DataTypeAdapter<IPAddress>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.Ipv6Address;

    // Complex IPv6 pattern from the Metaschema specification
    [GeneratedRegex(@"^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|[fF][eE]80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::([fF]{4}(:0{1,4}){0,1}:){0,1}((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3,3}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9])\.){3,3}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9][0-9]|[0-9]))$",
        RegexOptions.Compiled)]
    private static partial Regex Ipv6Pattern();

    /// <inheritdoc />
    public override IPAddress Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value, "Value cannot be empty");
        }

        if (!Ipv6Pattern().IsMatch(trimmed))
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid IPv6 address");
        }

        if (!IPAddress.TryParse(trimmed, out var address) || address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            throw DataTypeParseException.InvalidValue(TypeName, value,
                "Value must be a valid IPv6 address");
        }

        return address;
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
        if (!Ipv6Pattern().IsMatch(trimmed))
        {
            result = null;
            return false;
        }

        if (!IPAddress.TryParse(trimmed, out result))
        {
            return false;
        }

        if (result.AddressFamily != AddressFamily.InterNetworkV6)
        {
            result = null;
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override string Format(IPAddress value) => value.ToString();
}
