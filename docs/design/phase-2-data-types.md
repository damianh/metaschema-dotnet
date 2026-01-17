# Phase 2 Design: Data Type System

## Overview

Phase 2 implements the complete Metaschema data type system with validation. This provides strongly-typed data elements with well-defined syntax and semantics.

## Data Type Categories

Metaschema defines two kinds of data types:

1. **Simple Data Types** - Basic data value primitives with specific syntax and semantics
2. **Markup Data Types** - Semantically formatted text for presentation

### Simple Data Types

| Category | Data Types |
|----------|------------|
| Numeric | `decimal`, `integer`, `non-negative-integer`, `positive-integer` |
| Temporal | `date`, `date-with-timezone`, `date-time`, `date-time-with-timezone`, `day-time-duration`, `year-month-duration` |
| Binary | `base64`, `boolean` |
| String | `string`, `token`, `email-address`, `hostname`, `uri`, `uri-reference`, `uuid`, `ncname` |
| Network | `ip-v4-address`, `ip-v6-address` |

### Markup Data Types

| Type | Description |
|------|-------------|
| `markup-line` | Single-line inline markup (emphasis, links, code, etc.) |
| `markup-multiline` | Multi-line block markup (paragraphs, headers, lists, tables, etc.) |

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            Data Type System                                  │
└─────────────────────────────────────────────────────────────────────────────┘

                          ┌──────────────────────┐
                          │  IDataTypeProvider   │  ← Registry/Factory
                          │  (singleton)         │
                          └──────────┬───────────┘
                                     │
                                     ▼
                 ┌───────────────────────────────────────┐
                 │         IDataTypeAdapter<T>           │
                 │  - TypeName: string                   │
                 │  - ClrType: Type                      │
                 │  - Parse(string): T                   │
                 │  - TryParse(string, out T): bool      │
                 │  - Validate(string): ValidationResult │
                 │  - Format(T): string                  │
                 └───────────────────────────────────────┘
                                     │
          ┌──────────────────────────┼──────────────────────────┐
          │                          │                          │
          ▼                          ▼                          ▼
  ┌───────────────┐        ┌───────────────┐        ┌───────────────┐
  │StringAdapter  │        │IntegerAdapter │        │DateTimeAdapter│
  │TokenAdapter   │        │DecimalAdapter │        │DateAdapter    │
  │UriAdapter     │        │PositiveInt... │        │DurationAdapter│
  │UuidAdapter    │        │NonNegative... │        │               │
  │EmailAdapter   │        │               │        │               │
  │etc.           │        │               │        │               │
  └───────────────┘        └───────────────┘        └───────────────┘
```

## Project Structure

```
src/Metaschema.Core/
├── Datatypes/
│   ├── IDataTypeAdapter.cs           # Core adapter interface
│   ├── IDataTypeProvider.cs          # Registry interface
│   ├── DataTypeProvider.cs           # Default implementation
│   ├── DataTypeValidationResult.cs   # Validation result
│   ├── DataTypeParseException.cs     # Parse exception
│   ├── Adapters/
│   │   ├── StringAdapter.cs          # string
│   │   ├── TokenAdapter.cs           # token (NCName-like)
│   │   ├── IntegerAdapter.cs         # integer
│   │   ├── NonNegativeIntegerAdapter.cs
│   │   ├── PositiveIntegerAdapter.cs
│   │   ├── DecimalAdapter.cs         # decimal
│   │   ├── BooleanAdapter.cs         # boolean
│   │   ├── Base64Adapter.cs          # base64
│   │   ├── DateAdapter.cs            # date (optional tz)
│   │   ├── DateWithTimezoneAdapter.cs
│   │   ├── DateTimeAdapter.cs        # date-time (optional tz)
│   │   ├── DateTimeWithTimezoneAdapter.cs
│   │   ├── DayTimeDurationAdapter.cs
│   │   ├── YearMonthDurationAdapter.cs
│   │   ├── UriAdapter.cs             # uri (absolute)
│   │   ├── UriReferenceAdapter.cs    # uri-reference
│   │   ├── UuidAdapter.cs            # uuid (v4/v5)
│   │   ├── EmailAddressAdapter.cs    # email-address
│   │   ├── HostnameAdapter.cs        # hostname
│   │   ├── Ipv4AddressAdapter.cs     # ip-v4-address
│   │   ├── Ipv6AddressAdapter.cs     # ip-v6-address
│   │   ├── MarkupLineAdapter.cs      # markup-line
│   │   └── MarkupMultilineAdapter.cs # markup-multiline
│   └── MetaschemaDataTypes.cs        # Static type name constants
```

## Core Interfaces

### IDataTypeAdapter

```csharp
namespace Metaschema.Core.Datatypes;

/// <summary>
/// Adapts string values to and from a specific Metaschema data type.
/// </summary>
public interface IDataTypeAdapter
{
    /// <summary>
    /// Gets the Metaschema type name (e.g., "string", "integer", "uuid").
    /// </summary>
    string TypeName { get; }
    
    /// <summary>
    /// Gets the CLR type that this adapter produces.
    /// </summary>
    Type ClrType { get; }
    
    /// <summary>
    /// Parses a string value into the target type.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="DataTypeParseException">If parsing fails.</exception>
    object Parse(string value);
    
    /// <summary>
    /// Attempts to parse a string value into the target type.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">The parsed value if successful.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    bool TryParse(string value, out object? result);
    
    /// <summary>
    /// Validates a string value against this data type's constraints.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <returns>The validation result.</returns>
    DataTypeValidationResult Validate(string value);
    
    /// <summary>
    /// Formats a value back to its canonical string representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The canonical string representation.</returns>
    string Format(object value);
}

/// <summary>
/// Generic version of the data type adapter interface.
/// </summary>
/// <typeparam name="T">The CLR type this adapter produces.</typeparam>
public interface IDataTypeAdapter<T> : IDataTypeAdapter
{
    /// <summary>
    /// Parses a string value into the target type.
    /// </summary>
    new T Parse(string value);
    
    /// <summary>
    /// Attempts to parse a string value into the target type.
    /// </summary>
    new bool TryParse(string value, out T? result);
    
    /// <summary>
    /// Formats a value back to its canonical string representation.
    /// </summary>
    string Format(T value);
}
```

### IDataTypeProvider

```csharp
namespace Metaschema.Core.Datatypes;

/// <summary>
/// Provides access to data type adapters.
/// </summary>
public interface IDataTypeProvider
{
    /// <summary>
    /// Gets an adapter by Metaschema type name.
    /// </summary>
    /// <param name="typeName">The Metaschema type name.</param>
    /// <returns>The adapter, or null if not found.</returns>
    IDataTypeAdapter? GetAdapter(string typeName);
    
    /// <summary>
    /// Gets a typed adapter by Metaschema type name.
    /// </summary>
    IDataTypeAdapter<T>? GetAdapter<T>(string typeName);
    
    /// <summary>
    /// Gets all registered adapters.
    /// </summary>
    IEnumerable<IDataTypeAdapter> GetAllAdapters();
    
    /// <summary>
    /// Registers a custom adapter.
    /// </summary>
    void RegisterAdapter(IDataTypeAdapter adapter);
}
```

### DataTypeValidationResult

```csharp
namespace Metaschema.Core.Datatypes;

/// <summary>
/// Result of validating a value against a data type.
/// </summary>
public readonly record struct DataTypeValidationResult
{
    /// <summary>
    /// Gets whether the validation succeeded.
    /// </summary>
    public bool IsValid { get; init; }
    
    /// <summary>
    /// Gets the error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static DataTypeValidationResult Valid() => new() { IsValid = true };
    
    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static DataTypeValidationResult Invalid(string message) => 
        new() { IsValid = false, ErrorMessage = message };
}
```

## CLR Type Mappings

| Metaschema Type | CLR Type | Notes |
|-----------------|----------|-------|
| `string` | `string` | Non-empty, trimmed |
| `token` | `string` | NCName-like pattern |
| `integer` | `long` | Whole numbers |
| `non-negative-integer` | `ulong` | >= 0 |
| `positive-integer` | `ulong` | > 0 |
| `decimal` | `decimal` | Real numbers |
| `boolean` | `bool` | |
| `base64` | `byte[]` | |
| `date` | `DateOnly` | Optional timezone stored separately |
| `date-with-timezone` | `DateTimeOffset` | Date portion only |
| `date-time` | `DateTime` | Optional timezone |
| `date-time-with-timezone` | `DateTimeOffset` | |
| `day-time-duration` | `TimeSpan` | |
| `year-month-duration` | `(int Years, int Months)` | Custom struct |
| `uri` | `Uri` | Absolute URI |
| `uri-reference` | `Uri` | Relative or absolute |
| `uuid` | `Guid` | V4 or V5 only |
| `email-address` | `string` | Validated format |
| `hostname` | `string` | IDN hostname |
| `ip-v4-address` | `System.Net.IPAddress` | |
| `ip-v6-address` | `System.Net.IPAddress` | |
| `markup-line` | `MarkupLine` | Custom type |
| `markup-multiline` | `MarkupMultiline` | Custom type |

## Validation Patterns

Each data type uses regex patterns from the Metaschema specification for validation:

### String Types

```csharp
// string - non-empty, trimmed
private static readonly Regex StringPattern = new(@"^\S(.*\S)?$");

// token - NCName-like
private static readonly Regex TokenPattern = new(@"^(\p{L}|_)(\p{L}|\p{N}|[.\-_])*$");

// uuid - version 4 or 5
private static readonly Regex UuidPattern = new(
    @"^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[45][0-9A-Fa-f]{3}-[89ABab][0-9A-Fa-f]{3}-[0-9A-Fa-f]{12}$");
```

### Numeric Types

```csharp
// integer - any whole number
// non-negative-integer - >= 0
// positive-integer - > 0
// decimal - whole and optional fractional part
private static readonly Regex DecimalPattern = new(@"^(\+|-)?([0-9]+(\.[0-9]*)?|\.[0-9]+)$");
```

### Date/Time Types

Date patterns are complex to handle leap years correctly. We'll use the patterns from the specification.

## Implementation Strategy

### Base Adapter Class

```csharp
public abstract class DataTypeAdapter<T> : IDataTypeAdapter<T>
{
    public abstract string TypeName { get; }
    public Type ClrType => typeof(T);
    
    public abstract T Parse(string value);
    
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
    
    public virtual DataTypeValidationResult Validate(string value)
    {
        return TryParse(value, out _) 
            ? DataTypeValidationResult.Valid() 
            : DataTypeValidationResult.Invalid($"Invalid {TypeName} value: {value}");
    }
    
    public abstract string Format(T value);
    
    // IDataTypeAdapter implementation
    object IDataTypeAdapter.Parse(string value) => Parse(value)!;
    bool IDataTypeAdapter.TryParse(string value, out object? result)
    {
        var success = TryParse(value, out var typed);
        result = typed;
        return success;
    }
    string IDataTypeAdapter.Format(object value) => Format((T)value);
}
```

### Integration with Model

The `FlagDefinition` and `FieldDefinition` currently store `DataTypeName` as a string. We can add:

```csharp
public class FlagDefinition
{
    public string DataTypeName { get; init; } = "string";
    
    /// <summary>
    /// Gets the data type adapter for this definition.
    /// </summary>
    public IDataTypeAdapter? GetDataTypeAdapter(IDataTypeProvider provider) =>
        provider.GetAdapter(DataTypeName);
}
```

## Test Strategy

```
test/Metaschema.Core.Tests/
├── Datatypes/
│   ├── StringAdapterTests.cs
│   ├── TokenAdapterTests.cs
│   ├── IntegerAdapterTests.cs
│   ├── DecimalAdapterTests.cs
│   ├── BooleanAdapterTests.cs
│   ├── DateTimeAdapterTests.cs
│   ├── UriAdapterTests.cs
│   ├── UuidAdapterTests.cs
│   ├── IpAddressAdapterTests.cs
│   └── DataTypeProviderTests.cs
```

Each adapter test should cover:
1. Valid inputs parse correctly
2. Invalid inputs throw/return false
3. Validation returns correct results
4. Formatting produces canonical output
5. Round-trip (parse then format) preserves value

## Implementation Order

1. **Infrastructure** - Interfaces, base class, provider, exceptions
2. **String Types** - `string`, `token` (simplest)
3. **Numeric Types** - `integer`, `decimal`, etc.
4. **Boolean & Base64** - Simple conversions
5. **URI Types** - `uri`, `uri-reference`
6. **UUID** - Pattern-based validation
7. **Network Types** - IP addresses using System.Net
8. **Date/Time Types** - Most complex patterns
9. **Duration Types** - ISO 8601 durations
10. **Markup Types** - Integrate with existing MarkupLine/MarkupMultiline

## Dependencies

- Phase 1 (Module Loading) - complete
- No external dependencies beyond .NET BCL
- Markup types will be enhanced in later phases with Markdig parsing
