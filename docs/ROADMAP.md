# Metaschema .NET Toolchain Roadmap

## Overview

This project is a .NET implementation of the [Metaschema](https://pages.nist.gov/metaschema/) toolchain, providing equivalent functionality to the existing [metaschema-java](https://github.com/metaschema-framework/metaschema-java) and [metaschema-node](https://github.com/metaschema-framework/metaschema-node) implementations.

Metaschema is a framework for defining information models that can be used to generate schemas (XSD, JSON Schema), documentation, and code in multiple programming languages. It is the foundation for [OSCAL](https://pages.nist.gov/OSCAL/) (Open Security Controls Assessment Language).

- **Target Framework:** .NET 10
- **License:** MIT
- **Goal:** Full feature parity with the Java implementation

---

## Architecture Diagram

```
+------------------------------------------------------------------+
|                        Metaschema.Cli                            |
|              (Command-line tool for all operations)              |
+------------------------------------------------------------------+
         |              |              |              |
         v              v              v              v
+----------------+ +----------------+ +----------------+ +-----------------------+
| Metaschema.    | | Metaschema.    | | Metaschema.    | | Metaschema.           |
| Databind       | | Schemagen      | | SourceGenerator| | Testing               |
|                | |                | |                | |                       |
| - Serialization| | - XSD Gen      | | - MSBuild      | | - Test Utilities      |
| - Code Gen     | | - JSON Schema  | |   Integration  | | - Fixtures            |
| - Binding Ctx  | |   Gen          | | - Incremental  | | - Assertions          |
+----------------+ +----------------+ +----------------+ +-----------------------+
         |              |              |
         v              v              v
+------------------------------------------------------------------+
|                       Metaschema.Core                            |
|                                                                  |
|  +-------------+  +-------------+  +-------------+  +----------+ |
|  |    Model    |  |  Metapath   |  |  Datatypes  |  |Constraints||
|  |             |  |             |  |             |  |          | |
|  | - IModule   |  | - Parser    |  | - Adapters  |  | - Allowed| |
|  | - IDefinition| | - Evaluator |  | - Validation|  |   Values | |
|  | - Flags     |  | - Functions |  | - Built-in  |  | - Matches| |
|  | - Fields    |  |             |  |   Types     |  | - Expect | |
|  | - Assemblies|  |             |  |             |  | - Index  | |
|  +-------------+  +-------------+  +-------------+  +----------+ |
+------------------------------------------------------------------+
```

---

## Key .NET Libraries

The following table maps Java dependencies used in metaschema-java to their .NET equivalents:

| Purpose | Java Library | .NET Equivalent |
|---------|--------------|-----------------|
| XML Processing | Woodstox (StAX) | `System.Xml.Linq` / `XmlReader` |
| JSON Processing | Jackson | `System.Text.Json` |
| YAML Processing | jackson-dataformat-yaml | [YamlDotNet](https://github.com/aaubry/YamlDotNet) |
| Expression Parsing | ANTLR4 | [Antlr4.Runtime.Standard](https://www.nuget.org/packages/Antlr4.Runtime.Standard) |
| Code Generation | JavaPoet | Roslyn (`Microsoft.CodeAnalysis`) |
| CLI Framework | Commons CLI / Picocli | [System.CommandLine](https://github.com/dotnet/command-line-api) |
| Markdown Processing | Flexmark | [Markdig](https://github.com/xoofx/markdig) |
| Logging | SLF4J | `Microsoft.Extensions.Logging` |
| Testing | JUnit 5 | xUnit / NUnit |

---

## Phased Roadmap

### Phase 1: Project Foundation & Module Loading

**Objective:** Establish the core model and ability to load Metaschema modules from XML.

#### User Stories

1. **As a developer**, I can load a Metaschema module from an XML file so that I can work with the model programmatically.
2. **As a developer**, I can resolve module imports so that I can work with multi-module Metaschemas.
3. **As a developer**, I can access flag, field, and assembly definitions from a loaded module.

#### Acceptance Criteria

- Successfully parse the Metaschema XML format
- Resolve `<import>` elements to load dependent modules
- Build an in-memory model representing the complete module graph
- Handle circular import detection
- Support both file system and embedded resource loading

#### Key Interfaces

```
IModule
├── Name, Version, Namespace
├── Imports: IEnumerable<IModule>
├── GetFlagDefinitions(): IEnumerable<IFlagDefinition>
├── GetFieldDefinitions(): IEnumerable<IFieldDefinition>
├── GetAssemblyDefinitions(): IEnumerable<IAssemblyDefinition>
└── GetExportedDefinitions(): IEnumerable<IDefinition>

IDefinition
├── Name, FormalName, Description
├── Module: IModule
├── IsGlobal: bool
└── Remarks: MarkupMultiline

IFlagDefinition : IDefinition
├── DataType: IDataType
├── DefaultValue: string?
└── AllowedValues: IConstraint[]

IFieldDefinition : IDefinition
├── DataType: IDataType
├── Flags: IEnumerable<IFlagInstance>
└── IsCollapsible: bool

IAssemblyDefinition : IDefinition
├── Flags: IEnumerable<IFlagInstance>
├── Model: IModelContainer
└── RootName: string?
```

---

### Phase 2: Data Type System

**Objective:** Implement the complete Metaschema data type system with validation.

#### User Stories

1. **As a developer**, I can use built-in Metaschema data types so that values are properly typed.
2. **As a developer**, I can validate values against their declared data types so that I catch errors early.
3. **As a developer**, I can extend the type system with custom data type adapters.

#### Acceptance Criteria

- Implement all built-in data types from the Metaschema specification
- Provide validation for each data type
- Support custom data type registration
- Handle data type coercion where appropriate

#### Built-in Data Types

| Category | Data Types |
|----------|------------|
| String Types | `string`, `token`, `uri`, `uri-reference`, `uuid`, `email-address`, `hostname` |
| Numeric Types | `integer`, `non-negative-integer`, `positive-integer`, `decimal` |
| Boolean | `boolean` |
| Date/Time | `date`, `date-time`, `date-time-with-timezone`, `date-with-timezone` |
| Binary | `base64` |
| Markup | `markup-line`, `markup-multiline` |
| Special | `ncname`, `ip-v4-address`, `ip-v6-address` |

#### Key Interfaces

```
IDataTypeAdapter
├── TypeName: string
├── JsonValueType: Type
├── Parse(string value): object
├── Validate(string value): ValidationResult
└── Format(object value): string

IDataTypeProvider
├── GetAdapter(string typeName): IDataTypeAdapter
├── RegisterAdapter(IDataTypeAdapter adapter): void
└── GetAllAdapters(): IEnumerable<IDataTypeAdapter>
```

---

### Phase 3: Metapath Expression Language

**Objective:** Implement the Metapath query language for navigating and querying Metaschema-based data.

#### User Stories

1. **As a developer**, I can parse Metapath expressions so that I can build expression trees.
2. **As a developer**, I can evaluate Metapath expressions against a document node so that I can query data.
3. **As a developer**, I can use Metapath functions (string, numeric, boolean, sequence functions).

#### Acceptance Criteria

- Parse Metapath expressions using ANTLR4 grammar
- Evaluate path expressions against document instances
- Implement core function library
- Support variables in expressions
- Handle namespaces correctly

#### Implementation Notes

- The ANTLR4 grammar (`metapath10.g4`) should be ported from the Java project
- Expression evaluation should be lazy/streaming where possible
- Consider expression compilation for performance-critical paths

#### Metapath Function Categories

| Category | Example Functions |
|----------|-------------------|
| String | `concat()`, `substring()`, `contains()`, `starts-with()`, `ends-with()`, `string-length()`, `normalize-space()` |
| Numeric | `sum()`, `avg()`, `min()`, `max()`, `abs()`, `round()`, `floor()`, `ceiling()` |
| Boolean | `true()`, `false()`, `not()`, `boolean()` |
| Sequence | `count()`, `empty()`, `exists()`, `head()`, `tail()`, `distinct-values()` |
| Node | `name()`, `local-name()`, `path()` |
| Accessor | `data()`, `base-uri()`, `document-uri()` |

#### Key Interfaces

```
IMetapathExpression
├── Parse(string expression): IMetapathExpression
├── Evaluate(INodeItem context): ISequence
└── EvaluateSingle(INodeItem context): IItem?

IMetapathContext
├── StaticContext: IStaticContext
├── DynamicContext: IDynamicContext
└── Functions: IFunctionLibrary

INodeItem
├── NodeType: NodeType
├── Value: object
├── Parent: INodeItem?
└── Children: IEnumerable<INodeItem>
```

---

### Phase 4: Content Serialization (XML/JSON/YAML)

**Objective:** Implement bidirectional serialization between .NET objects and XML/JSON/YAML formats.

#### User Stories

1. **As a developer**, I can deserialize XML content into .NET objects based on a Metaschema.
2. **As a developer**, I can deserialize JSON content into .NET objects based on a Metaschema.
3. **As a developer**, I can deserialize YAML content into .NET objects based on a Metaschema.
4. **As a developer**, I can serialize .NET objects to XML/JSON/YAML.
5. **As a developer**, I can convert between formats losslessly.

#### Acceptance Criteria

- Support all three formats (XML, JSON, YAML)
- Preserve all information during round-trip conversion
- Handle format-specific nuances (XML namespaces, JSON discriminators)
- Support streaming for large documents
- Provide format detection from content

#### Key Interfaces

```
public enum Format
{
    Xml,
    Json,
    Yaml
}

ISerializer<T>
├── Serialize(T instance, Stream output, Format format): void
├── Serialize(T instance, TextWriter writer, Format format): void
└── SerializeToString(T instance, Format format): string

IDeserializer<T>
├── Deserialize(Stream input, Format format): T
├── Deserialize(TextReader reader, Format format): T
└── Deserialize(string content, Format format): T

IBoundLoader
├── DetectFormat(Stream input): Format
├── Load<T>(Stream input): T
├── Load<T>(string path): T
└── EnabledFormats: IEnumerable<Format>

IBindingContext
├── RegisterModule(IModule module): void
├── GetSerializer<T>(): ISerializer<T>
├── GetDeserializer<T>(): IDeserializer<T>
└── NewBoundLoader(): IBoundLoader
```

---

### Phase 5: Constraint Validation

**Objective:** Implement the full constraint validation system.

#### User Stories

1. **As a developer**, I can validate content against `allowed-values` constraints.
2. **As a developer**, I can validate content against `matches` (regex) constraints.
3. **As a developer**, I can validate content against `expect` (Metapath assertion) constraints.
4. **As a developer**, I can validate content against `index`/`unique` constraints.
5. **As a developer**, I can validate content against cardinality constraints.
6. **As a developer**, I receive structured validation results with locations and messages.

#### Acceptance Criteria

- Support all constraint types from the Metaschema specification
- Provide detailed error messages with document locations
- Support constraint severity levels (CRITICAL, ERROR, WARNING, INFORMATIONAL)
- Allow custom constraint validators
- Support constraint targeting via Metapath

#### Constraint Types

| Constraint | Description |
|------------|-------------|
| `allowed-values` | Restricts values to an enumerated set |
| `matches` | Validates against a regular expression |
| `expect` | Evaluates a Metapath expression that must return true |
| `index` | Creates a named index for cross-reference validation |
| `index-has-key` | Validates that a reference exists in an index |
| `is-unique` | Ensures uniqueness within a scope |
| `has-cardinality` | Validates min/max occurrence counts |

#### Key Interfaces

```
IConstraintValidator
├── Validate(INodeItem node, IConstraint constraint): IEnumerable<ValidationFinding>
└── ValidateAll(INodeItem root): ValidationResults

ValidationFinding
├── Severity: Severity { Critical, Error, Warning, Informational }
├── Location: IDocumentLocation
├── Constraint: IConstraint
├── Message: string
└── Node: INodeItem

ValidationResults
├── IsValid: bool
├── Findings: IEnumerable<ValidationFinding>
├── ErrorCount: int
└── WarningCount: int
```

---

### Phase 6: Code Generation

**Objective:** Generate C# classes from Metaschema modules using Roslyn.

#### User Stories

1. **As a developer**, I can generate C# classes from a Metaschema module.
2. **As a developer**, the generated classes have appropriate attributes for binding.
3. **As a developer**, I can use generated classes with the serialization APIs.
4. **As a developer**, I can customize code generation options (namespace, visibility, nullability, etc.).

#### Acceptance Criteria

- Generate idiomatic C# code with proper naming conventions
- Include XML documentation comments
- Support nullable reference types
- Generate immutable and mutable class variants
- Support inheritance for definition extensions
- Generate appropriate collection types

#### Binding Attributes

| Attribute | Purpose |
|-----------|---------|
| `[MetaschemaAssembly]` | Marks a class as a Metaschema assembly binding |
| `[MetaschemaField]` | Marks a class as a Metaschema field binding |
| `[BoundFlag]` | Binds a property to a flag instance |
| `[BoundField]` | Binds a property to a field instance |
| `[BoundAssembly]` | Binds a property to an assembly instance |
| `[JsonFieldValueKey]` | Specifies the JSON field value key name |
| `[XmlNamespace]` | Specifies the XML namespace |
| `[GroupAs]` | Specifies collection grouping behavior |

#### Code Generation Options

```
CodeGenerationOptions
├── Namespace: string
├── OutputPath: string
├── Visibility: Visibility { Public, Internal }
├── NullableAnnotations: bool
├── GenerateImmutableTypes: bool
├── GenerateBuilders: bool
└── FilePerType: bool
```

---

### Phase 7: Schema Generation

**Objective:** Generate XML Schema (XSD) and JSON Schema from Metaschema modules.

#### User Stories

1. **As a developer**, I can generate XML Schema (XSD) from a Metaschema module.
2. **As a developer**, I can generate JSON Schema from a Metaschema module.
3. **As a developer**, I can configure schema generation options.

#### Acceptance Criteria

- Generate valid XSD 1.1 schemas
- Generate valid JSON Schema (draft-07 or later)
- Include documentation annotations in generated schemas
- Handle all Metaschema constructs appropriately
- Support schema modularization options

#### Key Interfaces

```
ISchemaGenerator
├── GenerateXmlSchema(IModule module, SchemaGenerationOptions options): XDocument
├── GenerateJsonSchema(IModule module, SchemaGenerationOptions options): JsonDocument
└── SaveSchema(Stream output): void

SchemaGenerationOptions
├── InlineDefinitions: bool
├── IncludeDocumentation: bool
├── SchemaVersion: string
└── BaseUri: Uri?
```

---

### Phase 8: CLI Tool

**Objective:** Provide a command-line interface for all Metaschema operations.

#### User Stories

1. **As a user**, I can validate a Metaschema module definition using CLI.
2. **As a user**, I can validate content against a Metaschema using CLI.
3. **As a user**, I can generate schemas (XSD/JSON Schema) using CLI.
4. **As a user**, I can convert content between formats using CLI.
5. **As a user**, I can generate C# code from a Metaschema using CLI.

#### Acceptance Criteria

- Provide intuitive command structure
- Support standard CLI conventions (help, version, verbose)
- Return appropriate exit codes
- Support configuration files for complex options
- Provide machine-readable output options (JSON)

#### Commands

```
metaschema validate-module <metaschema-file>
    Validates a Metaschema module definition for correctness.
    
    Options:
      --constraint-set <file>   Additional constraint definitions
      --output <format>         Output format (text, json, sarif)

metaschema validate-content <content-file> --metaschema <metaschema-file>
    Validates content against a Metaschema definition.
    
    Options:
      --format <format>         Content format (xml, json, yaml, auto)
      --constraint-set <file>   Additional constraint definitions
      --output <format>         Output format (text, json, sarif)

metaschema generate-schema <metaschema-file> --type <schema-type>
    Generates XSD or JSON Schema from a Metaschema.
    
    Options:
      --type <type>             Schema type (xsd, json-schema)
      --output <file>           Output file path
      --inline-definitions      Inline all definitions

metaschema convert <input-file> --to <format>
    Converts content between XML, JSON, and YAML formats.
    
    Options:
      --metaschema <file>       Metaschema definition
      --to <format>             Target format (xml, json, yaml)
      --output <file>           Output file path

metaschema generate-code <metaschema-file>
    Generates C# code from a Metaschema module.
    
    Options:
      --namespace <ns>          Target namespace
      --output <directory>      Output directory
      --visibility <level>      public or internal
```

---

### Phase 9: Source Generator (MSBuild Integration)

**Objective:** Provide build-time code generation via MSBuild and .NET Source Generators.

#### User Stories

1. **As a developer**, I can add a NuGet package and have C# code generated at build time.
2. **As a developer**, I can specify Metaschema files in my `.csproj`.
3. **As a developer**, generated code is available for IntelliSense in my IDE.

#### Acceptance Criteria

- Implement as an incremental source generator
- Integrate seamlessly with MSBuild
- Support IDE integration (IntelliSense, go-to-definition)
- Provide helpful build-time diagnostics
- Support caching for incremental builds

#### MSBuild Integration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Metaschema.SourceGenerator" Version="1.0.0" />
  </ItemGroup>

  <ItemGroup>
    <MetaschemaFile Include="schemas/oscal_catalog.xml">
      <Namespace>MyApp.Oscal.Catalog</Namespace>
      <Visibility>Public</Visibility>
    </MetaschemaFile>
  </ItemGroup>
</Project>
```

#### Source Generator Architecture

```
MetaschemaSourceGenerator : IIncrementalGenerator
├── Initialize(IncrementalGeneratorInitializationContext context)
├── RegisterSourceOutput(...)
└── GenerateSource(MetaschemaFile file): SourceText
```

---

### Phase 10: Polish & Full Parity

**Objective:** Achieve full feature parity with metaschema-java and production readiness.

#### Focus Areas

- **Performance Optimization**
  - Profile and optimize hot paths
  - Implement caching strategies
  - Consider Span<T> and Memory<T> for reduced allocations
  - Benchmark against Java implementation

- **Documentation**
  - API reference documentation
  - Tutorials and guides
  - Sample applications
  - Migration guide from Java

- **Additional Metapath Functions**
  - Complete function library parity
  - OSCAL-specific extension functions

- **Edge Cases**
  - Handle all edge cases from Java implementation
  - Comprehensive error messages
  - Robust error recovery

- **OSCAL-Specific Testing**
  - Test against all OSCAL model versions
  - Validate against OSCAL content examples
  - Performance testing with large OSCAL documents

---

## Dependency Graph

The following diagram shows the dependencies between phases:

```
Phase 1: Project Foundation & Module Loading
    |
    +---> Phase 2: Data Type System
    |         |
    |         +---> Phase 4: Content Serialization
    |                   |
    |                   +---> Phase 5: Constraint Validation
    |                   |         |
    |                   |         +---> Phase 8: CLI Tool
    |                   |                   |
    |                   +---> Phase 6: Code Generation
    |                             |
    |                             +---> Phase 8: CLI Tool
    |                             |
    |                             +---> Phase 9: Source Generator
    |
    +---> Phase 3: Metapath Expression Language
    |         |
    |         +---> Phase 5: Constraint Validation
    |
    +---> Phase 7: Schema Generation
              |
              +---> Phase 8: CLI Tool

Phase 10: Polish & Full Parity (depends on all previous phases)
```

**Phase Dependency Summary:**

| Phase | Depends On |
|-------|------------|
| Phase 1 | (none) |
| Phase 2 | Phase 1 |
| Phase 3 | Phase 1 |
| Phase 4 | Phase 1, Phase 2 |
| Phase 5 | Phase 3, Phase 4 |
| Phase 6 | Phase 4 |
| Phase 7 | Phase 1 |
| Phase 8 | Phase 1-7 |
| Phase 9 | Phase 6 |
| Phase 10 | Phase 1-9 |

---

## Project Structure

```
metaschema-dotnet/
├── src/
│   ├── Metaschema.Core/
│   │   ├── Model/
│   │   │   ├── IModule.cs
│   │   │   ├── IDefinition.cs
│   │   │   ├── IFlagDefinition.cs
│   │   │   ├── IFieldDefinition.cs
│   │   │   └── IAssemblyDefinition.cs
│   │   ├── Metapath/
│   │   │   ├── Parser/
│   │   │   ├── Evaluator/
│   │   │   └── Functions/
│   │   ├── Datatypes/
│   │   │   ├── Adapters/
│   │   │   └── Validation/
│   │   └── Constraints/
│   │       ├── IConstraint.cs
│   │       └── Validators/
│   │
│   ├── Metaschema.Databind/
│   │   ├── Serialization/
│   │   │   ├── Xml/
│   │   │   ├── Json/
│   │   │   └── Yaml/
│   │   ├── CodeGeneration/
│   │   │   ├── CSharpGenerator.cs
│   │   │   └── Templates/
│   │   └── Binding/
│   │       ├── BindingContext.cs
│   │       └── Attributes/
│   │
│   ├── Metaschema.Schemagen/
│   │   ├── Xsd/
│   │   │   └── XsdGenerator.cs
│   │   └── JsonSchema/
│   │       └── JsonSchemaGenerator.cs
│   │
│   ├── Metaschema.Cli/
│   │   ├── Program.cs
│   │   └── Commands/
│   │       ├── ValidateModuleCommand.cs
│   │       ├── ValidateContentCommand.cs
│   │       ├── GenerateSchemaCommand.cs
│   │       ├── ConvertCommand.cs
│   │       └── GenerateCodeCommand.cs
│   │
│   ├── Metaschema.SourceGenerator/
│   │   ├── MetaschemaSourceGenerator.cs
│   │   └── build/
│   │       └── Metaschema.SourceGenerator.props
│   │
│   └── Metaschema.Testing/
│       ├── Fixtures/
│       ├── Assertions/
│       └── TestData/
│
├── tests/
│   ├── Metaschema.Core.Tests/
│   ├── Metaschema.Databind.Tests/
│   ├── Metaschema.Schemagen.Tests/
│   ├── Metaschema.Cli.Tests/
│   └── Metaschema.Integration.Tests/
│
├── samples/
│   ├── BasicUsage/
│   ├── OscalCatalog/
│   └── CustomBindings/
│
├── docs/
│   ├── ROADMAP.md
│   ├── api/
│   └── guides/
│
├── metaschema-dotnet.sln
├── Directory.Build.props
├── Directory.Packages.props
├── LICENSE
└── README.md
```

---

## References

- [Metaschema Specification](https://pages.nist.gov/metaschema/specification/)
- [metaschema-java Repository](https://github.com/metaschema-framework/metaschema-java)
- [metaschema-node Repository](https://github.com/metaschema-framework/metaschema-node)
- [OSCAL Project](https://pages.nist.gov/OSCAL/)
- [Metapath Specification](https://pages.nist.gov/metaschema/specification/metapath/)
