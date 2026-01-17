// Licensed under the MIT License.

namespace Metaschema.Core.Datatypes;

/// <summary>
/// Constants for Metaschema built-in data type names.
/// </summary>
public static class MetaschemaDataTypes
{
    // String types
    public const string StringType = "string";
    public const string Token = "token";
    public const string Uri = "uri";
    public const string UriReference = "uri-reference";
    public const string Uuid = "uuid";
    public const string EmailAddress = "email-address";
    public const string Hostname = "hostname";

    // Numeric types
    public const string IntegerType = "integer";
    public const string NonNegativeInteger = "non-negative-integer";
    public const string PositiveInteger = "positive-integer";
    public const string DecimalType = "decimal";

    // Boolean and binary
    public const string Boolean = "boolean";
    public const string Base64 = "base64";

    // Date/time types
    public const string Date = "date";
    public const string DateWithTimezone = "date-with-timezone";
    public const string DateTime = "date-time";
    public const string DateTimeWithTimezone = "date-time-with-timezone";
    public const string DayTimeDuration = "day-time-duration";
    public const string YearMonthDuration = "year-month-duration";

    // Network types
    public const string Ipv4Address = "ip-v4-address";
    public const string Ipv6Address = "ip-v6-address";

    // Markup types
    public const string MarkupLine = "markup-line";
    public const string MarkupMultiline = "markup-multiline";
}
