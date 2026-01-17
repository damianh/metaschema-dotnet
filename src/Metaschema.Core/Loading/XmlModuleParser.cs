// Licensed under the MIT License.

using System.Xml;
using System.Xml.Linq;
using Metaschema.Core.Markup;
using Metaschema.Core.Model;

namespace Metaschema.Core.Loading;

/// <summary>
/// Parses Metaschema XML files into module objects.
/// </summary>
public sealed class XmlModuleParser
{
    /// <summary>
    /// The Metaschema XML namespace.
    /// </summary>
    public const string MetaschemaNamespace = "http://csrc.nist.gov/ns/oscal/metaschema/1.0";

    private static readonly XNamespace Ns = MetaschemaNamespace;

    private readonly Func<Uri, MetaschemaModule> _importLoader;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlModuleParser"/> class.
    /// </summary>
    /// <param name="importLoader">A function to load imported modules by URI.</param>
    public XmlModuleParser(Func<Uri, MetaschemaModule> importLoader)
    {
        _importLoader = importLoader ?? throw new ArgumentNullException(nameof(importLoader));
    }

    /// <summary>
    /// Parses a Metaschema module from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the Metaschema XML.</param>
    /// <param name="location">The URI of the module being parsed.</param>
    /// <returns>The parsed module.</returns>
    /// <exception cref="ModuleLoadException">Thrown when parsing fails.</exception>
    public MetaschemaModule Parse(Stream stream, Uri location)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Load(stream, LoadOptions.SetLineInfo);
        }
        catch (XmlException ex)
        {
            throw new ModuleLoadException($"Failed to parse XML: {ex.Message}", location, ex);
        }

        return Parse(doc, location);
    }

    /// <summary>
    /// Parses a Metaschema module from an XDocument.
    /// </summary>
    /// <param name="doc">The XDocument containing the Metaschema.</param>
    /// <param name="location">The URI of the module being parsed.</param>
    /// <returns>The parsed module.</returns>
    /// <exception cref="ModuleLoadException">Thrown when parsing fails.</exception>
    public MetaschemaModule Parse(XDocument doc, Uri location)
    {
        var root = doc.Root;
        if (root is null || root.Name != Ns + "METASCHEMA")
        {
            throw new ModuleLoadException(
                $"Expected root element 'METASCHEMA' in namespace '{MetaschemaNamespace}'",
                location);
        }

        // Parse required header elements
        var schemaName = GetRequiredElementValue(root, "schema-name", location);
        var shortName = GetRequiredElementValue(root, "short-name", location);
        var schemaVersion = GetRequiredElementValue(root, "schema-version", location);
        var namespaceUri = GetRequiredElementUri(root, "namespace", location);
        var jsonBaseUri = GetRequiredElementUri(root, "json-base-uri", location);

        // Parse optional remarks
        var remarksElement = root.Element(Ns + "remarks");
        MarkupMultiline? remarks = remarksElement is not null
            ? new MarkupMultiline(GetInnerXml(remarksElement))
            : (MarkupMultiline?)null;

        // Create the module (we need it before parsing definitions)
        var module = new MetaschemaModule
        {
            Name = schemaName,
            ShortName = shortName,
            Version = schemaVersion,
            XmlNamespace = namespaceUri,
            JsonBaseUri = jsonBaseUri,
            Remarks = remarks,
            Location = location
        };

        // Parse imports (must be done first so definitions can reference imported definitions)
        foreach (var importElement in root.Elements(Ns + "import"))
        {
            var href = importElement.Attribute("href")?.Value;
            if (string.IsNullOrEmpty(href))
            {
                throw new ModuleLoadException("Import element missing 'href' attribute", location);
            }

            var importUri = new Uri(location, href);
            var importedModule = _importLoader(importUri);
            module.AddImportedModule(importedModule);
        }

        // Parse define-flag elements
        foreach (var flagElement in root.Elements(Ns + "define-flag"))
        {
            var flag = ParseFlagDefinition(flagElement, module, location);
            module.AddFlagDefinition(flag);
        }

        // Parse define-field elements
        foreach (var fieldElement in root.Elements(Ns + "define-field"))
        {
            var field = ParseFieldDefinition(fieldElement, module, location);
            module.AddFieldDefinition(field);
        }

        // Parse define-assembly elements
        foreach (var assemblyElement in root.Elements(Ns + "define-assembly"))
        {
            var assembly = ParseAssemblyDefinition(assemblyElement, module, location);
            module.AddAssemblyDefinition(assembly);
        }

        // Resolve references in instances
        ResolveReferences(module);

        return module;
    }

    private static FlagDefinition ParseFlagDefinition(XElement element, MetaschemaModule module, Uri location)
    {
        var name = GetRequiredAttribute(element, "name", location);

        return new FlagDefinition
        {
            Name = name,
            UseName = element.Attribute("use-name")?.Value,
            FormalName = element.Element(Ns + "formal-name")?.Value,
            Description = ParseMarkupLine(element.Element(Ns + "description")),
            Scope = ParseScope(element.Attribute("scope")?.Value),
            DeprecatedVersion = element.Attribute("deprecated")?.Value,
            Remarks = ParseMarkupMultiline(element.Element(Ns + "remarks")),
            ContainingModule = module,
            DataTypeName = element.Attribute("as-type")?.Value ?? "string",
            DefaultValue = element.Attribute("default")?.Value
        };
    }

    private static FieldDefinition ParseFieldDefinition(XElement element, MetaschemaModule module, Uri location)
    {
        var name = GetRequiredAttribute(element, "name", location);
        var flagInstances = ParseFlagInstances(element);

        return new FieldDefinition
        {
            Name = name,
            UseName = element.Attribute("use-name")?.Value,
            FormalName = element.Element(Ns + "formal-name")?.Value,
            Description = ParseMarkupLine(element.Element(Ns + "description")),
            Scope = ParseScope(element.Attribute("scope")?.Value),
            DeprecatedVersion = element.Attribute("deprecated")?.Value,
            Remarks = ParseMarkupMultiline(element.Element(Ns + "remarks")),
            ContainingModule = module,
            DataTypeName = element.Attribute("as-type")?.Value ?? "string",
            DefaultValue = element.Attribute("default")?.Value,
            IsCollapsible = element.Attribute("collapsible")?.Value == "yes",
            JsonValueKeyName = element.Element(Ns + "json-value-key")?.Value,
            JsonKeyFlagRef = element.Element(Ns + "json-key")?.Attribute("flag-ref")?.Value,
            FlagInstances = flagInstances
        };
    }

    private static AssemblyDefinition ParseAssemblyDefinition(XElement element, MetaschemaModule module, Uri location)
    {
        var name = GetRequiredAttribute(element, "name", location);
        var flagInstances = ParseFlagInstances(element);

        // Parse root-name
        var rootNameElement = element.Element(Ns + "root-name");
        var rootName = rootNameElement?.Value;

        // Parse model
        var modelElement = element.Element(Ns + "model");
        var model = modelElement is not null
            ? ParseModelContainer(modelElement)
            : null;

        return new AssemblyDefinition
        {
            Name = name,
            UseName = element.Attribute("use-name")?.Value,
            FormalName = element.Element(Ns + "formal-name")?.Value,
            Description = ParseMarkupLine(element.Element(Ns + "description")),
            Scope = ParseScope(element.Attribute("scope")?.Value),
            DeprecatedVersion = element.Attribute("deprecated")?.Value,
            Remarks = ParseMarkupMultiline(element.Element(Ns + "remarks")),
            ContainingModule = module,
            RootName = rootName,
            JsonKeyFlagRef = element.Element(Ns + "json-key")?.Attribute("flag-ref")?.Value,
            FlagInstances = flagInstances,
            Model = model
        };
    }

    private static List<FlagInstance> ParseFlagInstances(XElement parentElement)
    {
        var instances = new List<FlagInstance>();

        foreach (var flagElement in parentElement.Elements(Ns + "flag"))
        {
            var refAttr = flagElement.Attribute("ref")?.Value;
            if (string.IsNullOrEmpty(refAttr))
            {
                continue;
            }

            instances.Add(new FlagInstance
            {
                Ref = refAttr,
                IsRequired = flagElement.Attribute("required")?.Value == "yes",
                FormalName = flagElement.Element(Ns + "formal-name")?.Value,
                Description = ParseMarkupLine(flagElement.Element(Ns + "description")),
                UseName = flagElement.Attribute("use-name")?.Value,
                Remarks = ParseMarkupMultiline(flagElement.Element(Ns + "remarks")),
                DeprecatedVersion = flagElement.Attribute("deprecated")?.Value
            });
        }

        return instances;
    }

    private static ModelContainer ParseModelContainer(XElement modelElement)
    {
        var elements = new List<ModelElement>();

        foreach (var child in modelElement.Elements())
        {
            var localName = child.Name.LocalName;

            switch (localName)
            {
                case "field":
                    elements.Add(ParseFieldInstance(child));
                    break;

                case "assembly":
                    elements.Add(ParseAssemblyInstance(child));
                    break;

                case "choice":
                    elements.Add(ParseChoiceGroup(child));
                    break;
            }
        }

        return new ModelContainer { Elements = elements };
    }

    private static FieldInstance ParseFieldInstance(XElement element)
    {
        var refAttr = element.Attribute("ref")?.Value ?? string.Empty;

        return new FieldInstance
        {
            Ref = refAttr,
            FormalName = element.Element(Ns + "formal-name")?.Value,
            Description = ParseMarkupLine(element.Element(Ns + "description")),
            UseName = element.Attribute("use-name")?.Value,
            Remarks = ParseMarkupMultiline(element.Element(Ns + "remarks")),
            DeprecatedVersion = element.Attribute("deprecated")?.Value,
            MinOccurs = ParseOccurs(element.Attribute("min-occurs")?.Value, 0),
            MaxOccurs = ParseMaxOccurs(element.Attribute("max-occurs")?.Value),
            GroupAs = ParseGroupAs(element.Element(Ns + "group-as")),
            InXml = ParseXmlWrapping(element.Attribute("in-xml")?.Value)
        };
    }

    private static AssemblyInstance ParseAssemblyInstance(XElement element)
    {
        var refAttr = element.Attribute("ref")?.Value ?? string.Empty;

        return new AssemblyInstance
        {
            Ref = refAttr,
            FormalName = element.Element(Ns + "formal-name")?.Value,
            Description = ParseMarkupLine(element.Element(Ns + "description")),
            UseName = element.Attribute("use-name")?.Value,
            Remarks = ParseMarkupMultiline(element.Element(Ns + "remarks")),
            DeprecatedVersion = element.Attribute("deprecated")?.Value,
            MinOccurs = ParseOccurs(element.Attribute("min-occurs")?.Value, 0),
            MaxOccurs = ParseMaxOccurs(element.Attribute("max-occurs")?.Value),
            GroupAs = ParseGroupAs(element.Element(Ns + "group-as"))
        };
    }

    private static ChoiceGroup ParseChoiceGroup(XElement choiceElement)
    {
        var elements = new List<ModelElement>();

        foreach (var child in choiceElement.Elements())
        {
            var localName = child.Name.LocalName;

            switch (localName)
            {
                case "field":
                    elements.Add(ParseFieldInstance(child));
                    break;

                case "assembly":
                    elements.Add(ParseAssemblyInstance(child));
                    break;
            }
        }

        return new ChoiceGroup { Choices = elements };
    }

    private static GroupAs? ParseGroupAs(XElement? element)
    {
        if (element is null)
        {
            return null;
        }

        var name = element.Attribute("name")?.Value ?? string.Empty;
        var inJson = element.Attribute("in-json")?.Value switch
        {
            "ARRAY" => JsonGrouping.Array,
            "SINGLETON_OR_ARRAY" => JsonGrouping.SingletonOrArray,
            "BY_KEY" => JsonGrouping.ByKey,
            _ => JsonGrouping.Array
        };
        var inXml = element.Attribute("in-xml")?.Value switch
        {
            "GROUPED" => XmlGrouping.Grouped,
            "UNGROUPED" => XmlGrouping.Ungrouped,
            _ => XmlGrouping.Ungrouped
        };

        return new GroupAs(name, inJson, inXml);
    }

    private static int ParseOccurs(string? value, int defaultValue)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    private static int? ParseMaxOccurs(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 1;
        }

        if (value == "unbounded")
        {
            return null;
        }

        return int.TryParse(value, out var result) ? result : 1;
    }

    private static XmlWrapping ParseXmlWrapping(string? value) =>
        value switch
        {
            "UNWRAPPED" => XmlWrapping.Unwrapped,
            _ => XmlWrapping.Wrapped
        };

    private static Scope ParseScope(string? value) =>
        value switch
        {
            "local" => Scope.Local,
            _ => Scope.Global
        };

    private static MarkupLine? ParseMarkupLine(XElement? element) =>
        element is not null ? new MarkupLine(GetInnerXml(element)) : (MarkupLine?)null;

    private static MarkupMultiline? ParseMarkupMultiline(XElement? element) =>
        element is not null ? new MarkupMultiline(GetInnerXml(element)) : (MarkupMultiline?)null;

    private static string GetInnerXml(XElement element)
    {
        using var reader = element.CreateReader();
        reader.MoveToContent();
        return reader.ReadInnerXml();
    }

    private static string GetRequiredAttribute(XElement element, string name, Uri location)
    {
        var value = element.Attribute(name)?.Value;
        if (string.IsNullOrEmpty(value))
        {
            throw new ModuleLoadException(
                $"Element '{element.Name.LocalName}' missing required attribute '{name}'",
                location);
        }

        return value;
    }

    private static string GetRequiredElementValue(XElement parent, string name, Uri location)
    {
        var element = parent.Element(Ns + name);
        if (element is null)
        {
            throw new ModuleLoadException($"Missing required element '{name}'", location);
        }

        return element.Value;
    }

    private static Uri GetRequiredElementUri(XElement parent, string name, Uri location)
    {
        var value = GetRequiredElementValue(parent, name, location);
        try
        {
            return new Uri(value, UriKind.Absolute);
        }
        catch (UriFormatException ex)
        {
            throw new ModuleLoadException($"Invalid URI in element '{name}': {value}", location, ex);
        }
    }

    /// <summary>
    /// Resolves definition references in all instances.
    /// </summary>
    private static void ResolveReferences(MetaschemaModule module)
    {
        // Resolve flag instances in field definitions
        foreach (var field in module.FieldDefinitions)
        {
            foreach (var flagInstance in field.FlagInstances)
            {
                flagInstance.ResolvedDefinition = module.GetFlagDefinition(flagInstance.Ref);
            }
        }

        // Resolve flag instances and model references in assembly definitions
        foreach (var assembly in module.AssemblyDefinitions)
        {
            foreach (var flagInstance in assembly.FlagInstances)
            {
                flagInstance.ResolvedDefinition = module.GetFlagDefinition(flagInstance.Ref);
            }

            if (assembly.Model is not null)
            {
                ResolveModelReferences(module, assembly.Model);
            }
        }
    }

    private static void ResolveModelReferences(MetaschemaModule module, ModelContainer model)
    {
        foreach (var element in model.Elements)
        {
            switch (element)
            {
                case FieldInstance fieldInstance:
                    fieldInstance.ResolvedDefinition = module.GetFieldDefinition(fieldInstance.Ref);
                    break;

                case AssemblyInstance assemblyInstance:
                    assemblyInstance.ResolvedDefinition = module.GetAssemblyDefinition(assemblyInstance.Ref);
                    break;

                case ChoiceGroup choiceGroup:
                    ResolveModelReferences(module, new ModelContainer { Elements = choiceGroup.Choices });
                    break;
            }
        }
    }
}
