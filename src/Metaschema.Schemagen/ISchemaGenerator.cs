// Licensed under the MIT License.

using System.Text.Json;
using System.Xml.Linq;
using Metaschema.Core.Model;

namespace Metaschema.Schemagen;

/// <summary>
/// Generates XML Schema (XSD) and JSON Schema from Metaschema modules.
/// </summary>
public interface ISchemaGenerator
{
    /// <summary>
    /// Generates an XML Schema (XSD) document from a Metaschema module.
    /// </summary>
    /// <param name="metaschemaModule">The Metaschema module to generate schema from.</param>
    /// <param name="options">Optional generation options.</param>
    /// <returns>The generated XSD document.</returns>
    XDocument GenerateXsd(MetaschemaModule metaschemaModule, SchemaGenerationOptions? options = null);

    /// <summary>
    /// Generates a JSON Schema document from a Metaschema module.
    /// </summary>
    /// <param name="metaschemaModule">The Metaschema module to generate schema from.</param>
    /// <param name="options">Optional generation options.</param>
    /// <returns>The generated JSON Schema document.</returns>
    JsonDocument GenerateJsonSchema(MetaschemaModule metaschemaModule, SchemaGenerationOptions? options = null);
}
