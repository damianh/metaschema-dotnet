// Licensed under the MIT License.

using System.Xml.Linq;
using Metaschema.Core.Datatypes;

namespace Metaschema.Schemagen.Xsd;

/// <summary>
/// Maps Metaschema data types to XSD types.
/// </summary>
internal static class XsdTypeMapper
{
    private static readonly XNamespace Xs = XsdNamespaces.Xs;

    /// <summary>
    /// Gets the XSD type name for a Metaschema data type.
    /// </summary>
    /// <param name="metaschemaTypeName">The Metaschema type name.</param>
    /// <returns>The XSD type name (e.g., "xs:string").</returns>
    public static string GetXsdTypeName(string metaschemaTypeName)
    {
        return metaschemaTypeName switch
        {
            MetaschemaDataTypes.StringType => "xs:string",
            MetaschemaDataTypes.Token => "xs:NCName",
            MetaschemaDataTypes.IntegerType => "xs:integer",
            MetaschemaDataTypes.NonNegativeInteger => "xs:nonNegativeInteger",
            MetaschemaDataTypes.PositiveInteger => "xs:positiveInteger",
            MetaschemaDataTypes.DecimalType => "xs:decimal",
            MetaschemaDataTypes.Boolean => "xs:boolean",
            MetaschemaDataTypes.Date => "xs:date",
            MetaschemaDataTypes.DateWithTimezone => "xs:date",
            MetaschemaDataTypes.DateTime => "xs:dateTime",
            MetaschemaDataTypes.DateTimeWithTimezone => "xs:dateTime",
            MetaschemaDataTypes.Uri => "xs:anyURI",
            MetaschemaDataTypes.UriReference => "xs:anyURI",
            MetaschemaDataTypes.Base64 => "xs:base64Binary",
            MetaschemaDataTypes.DayTimeDuration => "xs:duration",
            MetaschemaDataTypes.YearMonthDuration => "xs:duration",
            // Types that need custom patterns - use xs:string with restrictions
            MetaschemaDataTypes.Uuid => "xs:string",
            MetaschemaDataTypes.EmailAddress => "xs:string",
            MetaschemaDataTypes.Hostname => "xs:string",
            MetaschemaDataTypes.Ipv4Address => "xs:string",
            MetaschemaDataTypes.Ipv6Address => "xs:string",
            // Markup types - use xs:string for simple representation
            MetaschemaDataTypes.MarkupLine => "xs:string",
            MetaschemaDataTypes.MarkupMultiline => "xs:string",
            _ => "xs:string"
        };
    }

    /// <summary>
    /// Determines if the type needs a custom simple type with pattern restriction.
    /// </summary>
    /// <param name="metaschemaTypeName">The Metaschema type name.</param>
    /// <returns>True if a custom type is needed.</returns>
    public static bool NeedsCustomType(string metaschemaTypeName)
    {
        return metaschemaTypeName is
            MetaschemaDataTypes.Uuid or
            MetaschemaDataTypes.EmailAddress or
            MetaschemaDataTypes.Ipv4Address or
            MetaschemaDataTypes.Ipv6Address or
            MetaschemaDataTypes.DateWithTimezone or
            MetaschemaDataTypes.DateTimeWithTimezone;
    }

    /// <summary>
    /// Gets the regex pattern for types that need pattern restrictions.
    /// </summary>
    /// <param name="metaschemaTypeName">The Metaschema type name.</param>
    /// <returns>The regex pattern, or null if no pattern needed.</returns>
    public static string? GetPattern(string metaschemaTypeName)
    {
        return metaschemaTypeName switch
        {
            MetaschemaDataTypes.Uuid =>
                @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
            MetaschemaDataTypes.EmailAddress =>
                @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
            MetaschemaDataTypes.Ipv4Address =>
                @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)",
            MetaschemaDataTypes.DateWithTimezone =>
                @"\d{4}-\d{2}-\d{2}(Z|[+-]\d{2}:\d{2})",
            MetaschemaDataTypes.DateTimeWithTimezone =>
                @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})",
            _ => null
        };
    }

    /// <summary>
    /// Creates a simple type element with pattern restriction.
    /// </summary>
    /// <param name="typeName">The name for the simple type.</param>
    /// <param name="baseType">The base XSD type.</param>
    /// <param name="pattern">The pattern restriction.</param>
    /// <returns>The xs:simpleType element.</returns>
    public static XElement CreatePatternRestrictedType(string typeName, string baseType, string pattern)
    {
        return new XElement(XsdNamespaces.SimpleType,
            new XAttribute("name", typeName),
            new XElement(XsdNamespaces.Restriction,
                new XAttribute("base", baseType),
                new XElement(XsdNamespaces.Pattern,
                    new XAttribute("value", pattern))));
    }
}
