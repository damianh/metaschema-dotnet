# Phase 7: Schema Generation - Design Document

## Overview

Phase 7 implements schema generation capabilities, allowing Metaschema modules to be converted into:
- **XML Schema (XSD 1.1)** - For XML validation
- **JSON Schema (draft-07)** - For JSON validation

## Architecture

```
ISchemaGenerator (interface)
├── GenerateXsd(module, options): XDocument
├── GenerateJsonSchema(module, options): JsonDocument
└── Save(document, stream): void

XsdGenerator : ISchemaGenerator
├── Uses System.Xml.Linq for XSD construction
├── Maps Metaschema types → XSD types
├── Handles namespaces and imports
└── Generates xs:complexType, xs:simpleType, xs:element

JsonSchemaGenerator : ISchemaGenerator
├── Uses System.Text.Json for JSON Schema construction
├── Maps Metaschema types → JSON Schema types
├── Generates $defs for reusable definitions
└── Handles oneOf/anyOf for choices
```

## Type Mapping

### XSD Type Mapping

| Metaschema Type | XSD Type |
|-----------------|----------|
| `string` | `xs:string` |
| `token` | `xs:NCName` |
| `integer` | `xs:integer` |
| `non-negative-integer` | `xs:nonNegativeInteger` |
| `positive-integer` | `xs:positiveInteger` |
| `decimal` | `xs:decimal` |
| `boolean` | `xs:boolean` |
| `date` | `xs:date` |
| `date-time` | `xs:dateTime` |
| `date-with-timezone` | `xs:date` (with pattern) |
| `date-time-with-timezone` | `xs:dateTime` (with pattern) |
| `uri` | `xs:anyURI` |
| `uri-reference` | `xs:anyURI` |
| `uuid` | `xs:string` (with pattern) |
| `email-address` | `xs:string` (with pattern) |
| `hostname` | `xs:string` |
| `ip-v4-address` | `xs:string` (with pattern) |
| `ip-v6-address` | `xs:string` (with pattern) |
| `base64` | `xs:base64Binary` |
| `day-time-duration` | `xs:duration` |
| `year-month-duration` | `xs:duration` |
| `markup-line` | `xs:string` (or inline markup elements) |
| `markup-multiline` | mixed content |

### JSON Schema Type Mapping

| Metaschema Type | JSON Schema |
|-----------------|-------------|
| `string` | `{ "type": "string" }` |
| `token` | `{ "type": "string", "pattern": "..." }` |
| `integer` | `{ "type": "integer" }` |
| `non-negative-integer` | `{ "type": "integer", "minimum": 0 }` |
| `positive-integer` | `{ "type": "integer", "minimum": 1 }` |
| `decimal` | `{ "type": "number" }` |
| `boolean` | `{ "type": "boolean" }` |
| `date` | `{ "type": "string", "format": "date" }` |
| `date-time` | `{ "type": "string", "format": "date-time" }` |
| `uri` | `{ "type": "string", "format": "uri" }` |
| `uri-reference` | `{ "type": "string", "format": "uri-reference" }` |
| `uuid` | `{ "type": "string", "format": "uuid" }` |
| `email-address` | `{ "type": "string", "format": "email" }` |
| `base64` | `{ "type": "string", "contentEncoding": "base64" }` |
| `markup-line` | `{ "type": "string" }` |
| `markup-multiline` | `{ "type": "string" }` |

## Key Interfaces

```csharp
public interface ISchemaGenerator
{
    XDocument GenerateXsd(MetaschemaModule module, SchemaGenerationOptions? options = null);
    JsonDocument GenerateJsonSchema(MetaschemaModule module, SchemaGenerationOptions? options = null);
}

public record SchemaGenerationOptions
{
    public bool InlineDefinitions { get; init; } = false;
    public bool IncludeDocumentation { get; init; } = true;
    public Uri? BaseUri { get; init; }
}
```

## XSD Generation Strategy

1. **Root Element**: Create `xs:schema` with target namespace
2. **Global Definitions**: Generate `xs:complexType` for each assembly/field definition
3. **Simple Types**: Generate `xs:simpleType` for custom data type restrictions
4. **Root Elements**: Generate `xs:element` for root assembly definitions
5. **Model Elements**: Generate `xs:sequence` or `xs:choice` for model containers

### XSD Structure Example

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
           targetNamespace="http://example.com/ns/test"
           xmlns:test="http://example.com/ns/test"
           elementFormDefault="qualified">
           
  <!-- Root element -->
  <xs:element name="catalog" type="test:catalog-type"/>
  
  <!-- Complex type for assembly -->
  <xs:complexType name="catalog-type">
    <xs:annotation>
      <xs:documentation>A catalog of items</xs:documentation>
    </xs:annotation>
    <xs:sequence>
      <xs:element name="title" type="xs:string"/>
      <xs:element name="item" type="test:item-type" minOccurs="0" maxOccurs="unbounded"/>
    </xs:sequence>
    <xs:attribute name="id" type="xs:string" use="required"/>
  </xs:complexType>
</xs:schema>
```

## JSON Schema Generation Strategy

1. **Root Schema**: Create schema with `$schema`, `$id`, and `type: "object"`
2. **$defs**: Generate reusable definitions for all global definitions
3. **Properties**: Map fields and assemblies to properties
4. **Required**: Track required fields/flags

### JSON Schema Structure Example

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "http://example.com/schema/test",
  "type": "object",
  "properties": {
    "catalog": { "$ref": "#/$defs/catalog" }
  },
  "$defs": {
    "catalog": {
      "type": "object",
      "description": "A catalog of items",
      "properties": {
        "id": { "type": "string" },
        "title": { "type": "string" },
        "items": {
          "type": "array",
          "items": { "$ref": "#/$defs/item" }
        }
      },
      "required": ["id", "title"]
    }
  }
}
```

## File Structure

```
src/Metaschema.Schemagen/
├── ISchemaGenerator.cs           # Main interface
├── SchemaGenerationOptions.cs    # Options record
├── SchemaGenerator.cs            # Default implementation
├── Xsd/
│   ├── XsdGenerator.cs           # XSD-specific logic
│   ├── XsdTypeMapper.cs          # Type mapping
│   └── XsdNamespaces.cs          # XSD namespace constants
└── JsonSchema/
    ├── JsonSchemaGenerator.cs    # JSON Schema-specific logic
    └── JsonSchemaTypeMapper.cs   # Type mapping
```

## Implementation Notes

1. **Namespace Handling**: XSD requires careful namespace management for imports
2. **Circular References**: Use named types with refs to handle circular definitions
3. **Documentation**: Include xs:annotation/xs:documentation for XSD, description for JSON Schema
4. **Cardinality**: Map minOccurs/maxOccurs to XSD attributes and JSON Schema minItems/maxItems
5. **Choices**: Use xs:choice for XSD, oneOf/anyOf for JSON Schema
