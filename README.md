# Metaschema .NET

[![CI](https://github.com/damianh/metaschema-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/damianh/metaschema-dotnet/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![GitHub Stars](https://img.shields.io/github/stars/damianh/metaschema-dotnet.svg)](https://github.com/damianh/metaschema-dotnet/stargazers)

A .NET implementation of the [Metaschema](https://pages.nist.gov/metaschema/) toolchain, providing equivalent functionality to [metaschema-java](https://github.com/metaschema-framework/metaschema-java) and [metaschema-node](https://github.com/metaschema-framework/metaschema-node).

## Overview

Metaschema is a framework for defining information models that can be used to generate schemas (XSD, JSON Schema), documentation, and code in multiple programming languages. It is the foundation for [OSCAL](https://pages.nist.gov/OSCAL/) (Open Security Controls Assessment Language).

## Features

- **Module Loading** - Parse Metaschema XML modules with import resolution and caching
- **Data Types** - Full implementation of all 23 built-in Metaschema data types
- **Metapath** - Complete Metapath expression language with 80+ functions
- **Serialization** - Read and write XML, JSON, and YAML content
- **Constraint Validation** - Validate content against allowed-values, matches, expect, and cardinality constraints
- **Code Generation** - Generate C# classes from Metaschema modules
- **Schema Generation** - Generate XSD and JSON Schema from Metaschema modules
- **CLI Tool** - Command-line interface for all operations

## Packages

| Package | Description | NuGet | Downloads |
|---------|-------------|-------|-----------|
| **DamianH.Metaschema** | Comprehensive library for working with NIST Metaschema including model loading, validation, serialization, code generation, and schema generation | [![NuGet](https://img.shields.io/nuget/v/DamianH.Metaschema.svg)](https://www.nuget.org/packages/DamianH.Metaschema/) | [![Downloads](https://img.shields.io/nuget/dt/DamianH.Metaschema.svg)](https://www.nuget.org/packages/DamianH.Metaschema/) |
| **DamianH.Metaschema.Tool** | Command-line tool for validation, schema generation, code generation, and format conversion | [![NuGet](https://img.shields.io/nuget/v/DamianH.Metaschema.Tool.svg)](https://www.nuget.org/packages/DamianH.Metaschema.Tool/) | [![Downloads](https://img.shields.io/nuget/dt/DamianH.Metaschema.Tool.svg)](https://www.nuget.org/packages/DamianH.Metaschema.Tool/) |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Installation

```bash
# Install the CLI tool globally
dotnet tool install -g DamianH.Metaschema.Tool

# Add the core library to your project
dotnet add package DamianH.Metaschema.Core

# Add serialization support
dotnet add package DamianH.Metaschema.Databind
```

## Quick Start

### Loading a Metaschema Module

```csharp
using Metaschema.Core.Loading;

// Load a Metaschema module from file
var loader = new ModuleLoader();
var module = loader.Load("path/to/metaschema.xml");

// Access definitions
foreach (var assembly in module.AssemblyDefinitions)
{
    Console.WriteLine($"Assembly: {assembly.Name}");
}

foreach (var field in module.FieldDefinitions)
{
    Console.WriteLine($"Field: {field.Name} ({field.DataTypeName})");
}
```

### Deserializing Content

```csharp
using Metaschema.Core.Loading;
using Metaschema.Databind;

// Create a binding context with your module
var loader = new ModuleLoader();
var module = loader.Load("path/to/metaschema.xml");
var context = new BindingContext(module);

// Deserialize JSON content
var deserializer = context.GetDeserializer(Format.Json);
var document = deserializer.Deserialize(jsonContent);

// Access the document structure
var rootAssembly = document.RootAssembly;
Console.WriteLine($"Root: {rootAssembly.Name}");

// Access flags (attributes)
foreach (var flag in rootAssembly.Flags)
{
    Console.WriteLine($"  {flag.Key}: {flag.Value.RawValue}");
}
```

### Serializing Content

```csharp
using Metaschema.Databind;

// Serialize a document to different formats
var serializer = context.GetSerializer(Format.Json);
var json = serializer.Serialize(document);

var xmlSerializer = context.GetSerializer(Format.Xml);
var xml = xmlSerializer.Serialize(document);

var yamlSerializer = context.GetSerializer(Format.Yaml);
var yaml = yamlSerializer.Serialize(document);
```

### Evaluating Metapath Expressions

```csharp
using Metaschema.Core.Metapath;

// Parse and evaluate a Metapath expression
var expression = MetapathExpression.Parse("//control[@id='ac-1']");
var results = expression.Evaluate(documentNode);

foreach (var item in results)
{
    Console.WriteLine(item);
}

// Use built-in functions
var countExpr = MetapathExpression.Parse("count(//control)");
var count = countExpr.EvaluateSingle(documentNode);
```

### Constraint Validation

```csharp
using Metaschema.Core.Constraints;

// Validate content against constraints
var validator = new ConstraintValidator(module);
var results = validator.Validate(documentNode);

if (!results.IsValid)
{
    foreach (var finding in results.Findings)
    {
        Console.WriteLine($"[{finding.Severity}] {finding.Message}");
    }
}
```

### Generating Code

```csharp
using Metaschema.Databind.CodeGeneration;

// Generate C# classes from a module
var generator = new CSharpCodeGenerator();
var options = new CodeGenerationOptions
{
    Namespace = "MyApp.Models",
    OutputPath = "Generated",
    Visibility = Visibility.Public
};

generator.Generate(module, options);
```

### Generating Schemas

```csharp
using Metaschema.Schemagen;

// Generate JSON Schema
var jsonSchemaGenerator = new JsonSchemaGenerator();
var jsonSchema = jsonSchemaGenerator.Generate(module);

// Generate XSD
var xsdGenerator = new XsdGenerator();
var xsd = xsdGenerator.Generate(module);
```

## CLI Usage

```bash
# Validate a Metaschema module
metaschema validate-module path/to/metaschema.xml

# Validate content against a Metaschema
metaschema validate-content document.json --metaschema path/to/metaschema.xml

# Generate JSON Schema
metaschema generate-schema path/to/metaschema.xml --type json-schema --output schema.json

# Generate XSD
metaschema generate-schema path/to/metaschema.xml --type xsd --output schema.xsd

# Generate C# code
metaschema generate-code path/to/metaschema.xml --namespace MyApp.Models --output Generated

# Convert between formats
metaschema convert document.xml --to json --output document.json
metaschema convert document.json --to yaml --output document.yaml
```

## Metapath Functions

The Metapath implementation includes 80+ functions across these categories:

| Category | Functions |
|----------|-----------|
| **String** | `concat`, `substring`, `contains`, `starts-with`, `ends-with`, `string-length`, `normalize-space`, `upper-case`, `lower-case`, `translate`, `replace`, `tokenize` |
| **Numeric** | `sum`, `avg`, `min`, `max`, `abs`, `round`, `floor`, `ceiling`, `number` |
| **Boolean** | `true`, `false`, `not`, `boolean` |
| **Sequence** | `count`, `empty`, `exists`, `head`, `tail`, `distinct-values`, `reverse`, `subsequence` |
| **Comparison** | `compare`, `deep-equal` |
| **Date/Time** | `current-date`, `current-dateTime`, `current-time`, `year-from-dateTime`, `month-from-dateTime`, etc. |
| **Array** | `array:size`, `array:get`, `array:put`, `array:append`, `array:head`, `array:tail`, etc. |
| **Map** | `map:size`, `map:keys`, `map:contains`, `map:get`, `map:put`, `map:merge`, etc. |
| **Node** | `name`, `local-name`, `namespace-uri`, `path` |
| **URI** | `resolve-uri`, `encode-for-uri` |
| **Metaschema** | `mp:base64-encode`, `mp:base64-decode`, `mp:recurse-depth` |

## Data Types

All 23 built-in Metaschema data types are supported:

| Category | Types |
|----------|-------|
| **String** | `string`, `token`, `uri`, `uri-reference`, `uuid`, `email-address`, `hostname` |
| **Numeric** | `integer`, `non-negative-integer`, `positive-integer`, `decimal` |
| **Boolean** | `boolean` |
| **Date/Time** | `date`, `date-time`, `date-time-with-timezone`, `date-with-timezone` |
| **Binary** | `base64` |
| **Markup** | `markup-line`, `markup-multiline` |
| **Special** | `ncname`, `ip-v4-address`, `ip-v6-address` |

## Building from Source

```bash
# Clone the repository
git clone https://github.com/damianh/metaschema-dotnet.git
cd metaschema-dotnet

# Build
dotnet build metaschema-dotnet.slnx

# Run tests
dotnet test metaschema-dotnet.slnx
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## References

- [Metaschema Specification](https://pages.nist.gov/metaschema/specification/)
- [Metapath Specification](https://pages.nist.gov/metaschema/specification/metapath/)
- [metaschema-java](https://github.com/metaschema-framework/metaschema-java) - Java implementation
- [metaschema-node](https://github.com/metaschema-framework/metaschema-node) - Node.js/TypeScript implementation
- [OSCAL](https://pages.nist.gov/OSCAL/) - Open Security Controls Assessment Language
