// Licensed under the MIT License.

namespace Metaschema.Core.Model;

/// <summary>
/// Grouping configuration for collection instances.
/// </summary>
/// <param name="Name">The grouping name.</param>
/// <param name="InJson">JSON grouping behavior.</param>
/// <param name="InXml">XML grouping behavior.</param>
public record GroupAs(
    string Name,
    JsonGrouping InJson,
    XmlGrouping InXml
);
