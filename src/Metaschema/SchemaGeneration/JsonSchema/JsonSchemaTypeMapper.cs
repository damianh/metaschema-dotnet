// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Text.Json.Nodes;
using Metaschema.Datatypes;

namespace Metaschema.SchemaGeneration.JsonSchema;

/// <summary>
/// Maps Metaschema data types to JSON Schema types.
/// </summary>
internal static class JsonSchemaTypeMapper
{
    /// <summary>
    /// Gets the JSON Schema type object for a Metaschema data type.
    /// </summary>
    /// <param name="metaschemaTypeName">The Metaschema type name.</param>
    /// <returns>A JSON object representing the type schema.</returns>
    public static JsonObject GetTypeSchema(string metaschemaTypeName) => metaschemaTypeName switch
    {
        MetaschemaDataTypes.StringType => new JsonObject { ["type"] = "string" },
        MetaschemaDataTypes.Token => new JsonObject
        {
            ["type"] = "string",
            ["pattern"] = @"^(\p{L}|_)(\p{L}|\p{N}|[.\-_])*$"
        },
        MetaschemaDataTypes.IntegerType => new JsonObject { ["type"] = "integer" },
        MetaschemaDataTypes.NonNegativeInteger => new JsonObject
        {
            ["type"] = "integer",
            ["minimum"] = 0
        },
        MetaschemaDataTypes.PositiveInteger => new JsonObject
        {
            ["type"] = "integer",
            ["minimum"] = 1
        },
        MetaschemaDataTypes.DecimalType => new JsonObject { ["type"] = "number" },
        MetaschemaDataTypes.Boolean => new JsonObject { ["type"] = "boolean" },
        MetaschemaDataTypes.Date => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "date"
        },
        MetaschemaDataTypes.DateWithTimezone => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "date",
            ["pattern"] = @"^\d{4}-\d{2}-\d{2}(Z|[+-]\d{2}:\d{2})$"
        },
        MetaschemaDataTypes.DateTime => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "date-time"
        },
        MetaschemaDataTypes.DateTimeWithTimezone => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "date-time"
        },
        MetaschemaDataTypes.Uri => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "uri"
        },
        MetaschemaDataTypes.UriReference => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "uri-reference"
        },
        MetaschemaDataTypes.Uuid => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "uuid"
        },
        MetaschemaDataTypes.EmailAddress => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "email"
        },
        MetaschemaDataTypes.Hostname => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "hostname"
        },
        MetaschemaDataTypes.Ipv4Address => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "ipv4"
        },
        MetaschemaDataTypes.Ipv6Address => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "ipv6"
        },
        MetaschemaDataTypes.Base64 => new JsonObject
        {
            ["type"] = "string",
            ["contentEncoding"] = "base64"
        },
        MetaschemaDataTypes.DayTimeDuration => new JsonObject
        {
            ["type"] = "string",
            ["format"] = "duration"
        },
        MetaschemaDataTypes.YearMonthDuration => new JsonObject
        {
            ["type"] = "string",
            ["pattern"] = @"^-?P(\d+Y)?(\d+M)?$"
        },
        MetaschemaDataTypes.MarkupLine => new JsonObject { ["type"] = "string" },
        MetaschemaDataTypes.MarkupMultiline => new JsonObject { ["type"] = "string" },
        _ => new JsonObject { ["type"] = "string" }
    };
}
