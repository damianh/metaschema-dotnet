# Record-Based Code Generation with System.Text.Json

This demonstrates the new code generation approach using **C# records** with **System.Text.Json source generation** support.

## Key Features

✅ **Records** - Immutable data types with `init` properties  
✅ **System.Text.Json** - Zero-reflection JSON serialization  
✅ **No Attributes Needed** - Metadata stays in generator, not runtime  
✅ **AOT Compatible** - Works with Native AOT compilation  
✅ **Clean API** - Simple Load/Save methods  

## Generated Files

### 1. **Record Types** (e.g., `Catalog.g.cs`)

```csharp
public sealed record Catalog
{
    [JsonPropertyName("uuid")]
    public required Guid Uuid { get; init; }
    
    [JsonPropertyName("metadata")]
    public required Metadata Metadata { get; init; }
    
    [JsonPropertyName("groups")]
    public IReadOnlyList<Group> Groups { get; init; } = [];
}
```

**Features:**
- Immutable records with `init`-only properties
- `required` for mandatory properties
- `IReadOnlyList<T>` for collections
- Only `[JsonPropertyName]` attributes (STJ standard)
- No custom Metaschema attributes needed

### 2. **JsonSerializerContext** (e.g., `CatalogJsonContext.g.cs`)

```csharp
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Catalog))]
[JsonSerializable(typeof(Metadata))]
// ... all types
public partial class CatalogJsonContext : JsonSerializerContext
{
    // STJ source generator completes this at compile time
}
```

**Features:**
- Partial class for STJ source generator
- Configured for OSCAL/Metaschema conventions
- Zero reflection - all code generated at compile time

### 3. **Extension Methods** (e.g., `Extensions.g.cs`)

```csharp
public static class Extensions
{
    public static Catalog LoadFromJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize(json, CatalogJsonContext.Default.Catalog)
            ?? throw new InvalidOperationException("Failed to deserialize Catalog");
    }
    
    public static void SaveToJson(this Catalog catalog, string filePath)
    {
        var json = JsonSerializer.Serialize(catalog, CatalogJsonContext.Default.Catalog);
        File.WriteAllText(filePath, json);
    }
}
```

**Features:**
- Simple, clean API
- Uses generated context for zero-reflection serialization
- Easy to extend with XML/YAML support

## Usage

### Generate Code

```bash
metaschema generate oscal_catalog_metaschema.xml \
    --output ./Generated \
    --namespace Oscal.Catalog \
    --use-records
```

### Add to Project

```xml
<ItemGroup>
  <Compile Include="Generated\**\*.g.cs" />
</ItemGroup>
```

### Use in Code

```csharp
using Oscal.Catalog;

// Load with zero reflection
var catalog = Extensions.LoadFromJson("NIST_SP-800-53_rev5_catalog.json");

// Type-safe access
Console.WriteLine($"Catalog UUID: {catalog.Uuid}");
Console.WriteLine($"Title: {catalog.Metadata.Title}");

// Immutable updates
var updated = catalog with 
{ 
    Metadata = catalog.Metadata with { Version = "2.0" }
};

// Save
updated.SaveToJson("updated_catalog.json");
```

## Benefits Over Reflection Approach

| Aspect | This Approach | Reflection Approach |
|--------|---------------|-------------------|
| **Performance** | ✅ Fastest (no reflection) | ❌ Slower (runtime reflection) |
| **AOT Support** | ✅ Full Native AOT | ❌ Limited or broken |
| **Code Size** | ✅ Optimized by trimmer | ❌ Larger runtime overhead |
| **Startup Time** | ✅ Instant | ❌ Reflection warmup cost |
| **Debugging** | ✅ Easy (all code visible) | ❌ Harder (dynamic behavior) |
| **Modern .NET** | ✅ Idiomatic .NET 10 | ❌ Legacy approach |

## Implementation Status

✅ `RecordCodeGenerator.cs` - Complete  
✅ Record generation with `init` properties  
✅ `JsonSerializerContext` generation  
✅ Extension methods generation  
⏳ CLI integration (next step)  
⏳ XML serialization support (planned)  
⏳ YAML serialization support (planned)  

## See Also

- `RecordCodeGenerator.cs` - Main generator implementation
- `CodeGenerationOptions.cs` - Configuration options
- `RecordCodeGeneratorTests.cs` - Unit tests
