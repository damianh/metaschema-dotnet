# Metaschema .NET

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A .NET implementation of the [Metaschema](https://pages.nist.gov/metaschema/) toolchain, providing equivalent functionality to [metaschema-java](https://github.com/usnistgov/metaschema-java) and [metaschema-node](https://github.com/usnistgov/metaschema-node).

## Overview

Metaschema is a framework for defining information models that can be used to generate schemas (XSD, JSON Schema), documentation, and code in multiple programming languages. It is the foundation for [OSCAL](https://pages.nist.gov/OSCAL/) (Open Security Controls Assessment Language).

This project provides:

- **Metaschema.Core** - Core library for loading and working with Metaschema models, including the Metapath expression language, data types, and constraint validation
- **Metaschema.Databind** - Data binding library for serializing/deserializing Metaschema-based content in XML, JSON, and YAML formats
- **Metaschema.Schemagen** - Schema generation library for producing XML Schema (XSD) and JSON Schema
- **Metaschema.Cli** - Command-line tool for validation, schema generation, and format conversion
- **Metaschema.SourceGenerator** - MSBuild source generator for generating C# code from Metaschema modules at build time
- **Metaschema.Testing** - Test utilities and fixtures for Metaschema-based testing

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview)

## Building

```bash
dotnet build metaschema-dotnet.slnx
```

## Testing

```bash
dotnet test metaschema-dotnet.slnx
```

## Project Status

This project is in early development. See the [Roadmap](docs/ROADMAP.md) for planned features and phases.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## References

- [Metaschema Specification](https://pages.nist.gov/metaschema/specification/)
- [metaschema-java](https://github.com/usnistgov/metaschema-java) - Java implementation
- [metaschema-node](https://github.com/usnistgov/metaschema-node) - Node.js/TypeScript implementation
- [OSCAL](https://pages.nist.gov/OSCAL/) - Open Security Controls Assessment Language
