// Licensed under the MIT License.

using System.Xml.Linq;
using Metaschema.Model;

namespace Metaschema.SchemaGeneration.Xsd;

/// <summary>
/// Generates XML Schema (XSD) documents from Metaschema modules.
/// </summary>
public sealed class XsdGenerator
{
    private static readonly XNamespace Xs = XsdNamespaces.Xs;

    private readonly SchemaGenerationOptions _options;
    private readonly HashSet<string> _generatedTypes = new(StringComparer.Ordinal);
    private XNamespace _targetNs = null!;
    private string _targetPrefix = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="XsdGenerator"/> class.
    /// </summary>
    /// <param name="options">The generation options.</param>
    public XsdGenerator(SchemaGenerationOptions? options = null)
    {
        _options = options ?? SchemaGenerationOptions.Default;
    }

    /// <summary>
    /// Generates an XSD document from the specified module.
    /// </summary>
    /// <param name="module">The Metaschema module.</param>
    /// <returns>The generated XSD document.</returns>
    public XDocument Generate(MetaschemaModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        _targetNs = module.XmlNamespace.ToString();
        _targetPrefix = module.ShortName;
        _generatedTypes.Clear();

        var schema = new XElement(XsdNamespaces.Schema,
            new XAttribute("targetNamespace", _targetNs.ToString()),
            new XAttribute(XNamespace.Xmlns + "xs", Xs.ToString()),
            new XAttribute(XNamespace.Xmlns + _targetPrefix, _targetNs.ToString()),
            new XAttribute("elementFormDefault", "qualified"),
            new XAttribute("attributeFormDefault", "unqualified"));

        // Add documentation
        if (_options.IncludeDocumentation)
        {
            schema.Add(CreateAnnotation($"Schema generated from Metaschema module: {module.Name} v{module.Version}"));
        }

        // Generate custom simple types for special data types
        GenerateCustomSimpleTypes(schema, module);

        // Generate root elements for root assemblies
        foreach (var rootAssembly in module.RootAssemblyDefinitions)
        {
            schema.Add(CreateRootElement(rootAssembly));
        }

        // Generate complex types for all assembly definitions
        foreach (var assembly in GetAllAssemblyDefinitions(module))
        {
            if (_generatedTypes.Add(assembly.Name))
            {
                schema.Add(GenerateAssemblyComplexType(assembly, module));
            }
        }

        // Generate complex types for field definitions with flags
        foreach (var field in GetAllFieldDefinitions(module))
        {
            if (field.FlagInstances.Count > 0 && _generatedTypes.Add(field.Name))
            {
                schema.Add(GenerateFieldComplexType(field, module));
            }
        }

        return new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            schema);
    }

    private static void GenerateCustomSimpleTypes(XElement schema, MetaschemaModule module)
    {
        var usedTypes = new HashSet<string>(StringComparer.Ordinal);

        // Collect all data types used in the module
        foreach (var flag in GetAllFlagDefinitions(module))
        {
            usedTypes.Add(flag.DataTypeName);
        }
        foreach (var field in GetAllFieldDefinitions(module))
        {
            usedTypes.Add(field.DataTypeName);
        }

        // Generate custom types for those that need them
        foreach (var typeName in usedTypes)
        {
            if (XsdTypeMapper.NeedsCustomType(typeName))
            {
                var pattern = XsdTypeMapper.GetPattern(typeName);
                if (pattern is not null)
                {
                    schema.Add(XsdTypeMapper.CreatePatternRestrictedType(
                        GetLocalTypeName(typeName),
                        XsdTypeMapper.GetXsdTypeName(typeName),
                        pattern));
                }
            }
        }
    }

    private XElement CreateRootElement(AssemblyDefinition assembly)
    {
        var element = new XElement(XsdNamespaces.Element,
            new XAttribute("name", assembly.RootName ?? assembly.EffectiveName),
            new XAttribute("type", $"{_targetPrefix}:{assembly.Name}-type"));

        if (_options.IncludeDocumentation && assembly.Description is not null)
        {
            element.AddFirst(CreateAnnotation(assembly.Description.Value));
        }

        return element;
    }

    private XElement GenerateAssemblyComplexType(AssemblyDefinition assembly, MetaschemaModule module)
    {
        var complexType = new XElement(XsdNamespaces.ComplexType,
            new XAttribute("name", $"{assembly.Name}-type"));

        if (_options.IncludeDocumentation && assembly.Description is not null)
        {
            complexType.Add(CreateAnnotation(assembly.Description.Value));
        }

        // Generate sequence for model content
        if (assembly.Model?.Elements.Count > 0)
        {
            var sequence = new XElement(XsdNamespaces.Sequence);
            foreach (var element in assembly.Model.Elements)
            {
                var childElement = GenerateModelElement(element, module);
                if (childElement is not null)
                {
                    sequence.Add(childElement);
                }
            }
            if (sequence.HasElements)
            {
                complexType.Add(sequence);
            }
        }

        // Generate attributes for flags
        foreach (var flag in assembly.FlagInstances)
        {
            complexType.Add(GenerateFlagAttribute(flag, module));
        }

        return complexType;
    }

    private XElement GenerateFieldComplexType(FieldDefinition field, MetaschemaModule module)
    {
        var complexType = new XElement(XsdNamespaces.ComplexType,
            new XAttribute("name", $"{field.Name}-type"));

        if (_options.IncludeDocumentation && field.Description is not null)
        {
            complexType.Add(CreateAnnotation(field.Description.Value));
        }

        // For fields with flags, use simpleContent extension
        var xsdType = GetXsdTypeReference(field.DataTypeName);
        complexType.Add(
            new XElement(XsdNamespaces.SimpleContent,
                new XElement(XsdNamespaces.Extension,
                    new XAttribute("base", xsdType),
                    field.FlagInstances.Select(f => GenerateFlagAttribute(f, module)))));

        return complexType;
    }

    private XElement? GenerateModelElement(ModelElement element, MetaschemaModule module)
    {
        return element switch
        {
            FieldInstance field => GenerateFieldElement(field, module),
            AssemblyInstance assembly => GenerateAssemblyElement(assembly, module),
            ChoiceGroup choice => GenerateChoiceElement(choice, module),
            _ => null
        };
    }

    private XElement GenerateFieldElement(FieldInstance field, MetaschemaModule module)
    {
        var definition = field.ResolvedDefinition ?? module.GetFieldDefinition(field.Ref);
        var hasFlags = definition?.FlagInstances.Count > 0;

        var element = new XElement(XsdNamespaces.Element,
            new XAttribute("name", field.EffectiveName));

        // Use complex type reference if field has flags, otherwise simple type
        if (hasFlags && definition is not null)
        {
            element.Add(new XAttribute("type", $"{_targetPrefix}:{definition.Name}-type"));
        }
        else
        {
            var xsdType = GetXsdTypeReference(definition?.DataTypeName ?? "string");
            element.Add(new XAttribute("type", xsdType));
        }

        // Add cardinality
        AddCardinality(element, field.MinOccurs, field.MaxOccurs);

        return element;
    }

    private XElement GenerateAssemblyElement(AssemblyInstance assembly, MetaschemaModule module)
    {
        var element = new XElement(XsdNamespaces.Element,
            new XAttribute("name", assembly.EffectiveName),
            new XAttribute("type", $"{_targetPrefix}:{assembly.Ref}-type"));

        AddCardinality(element, assembly.MinOccurs, assembly.MaxOccurs);

        return element;
    }

    private XElement? GenerateChoiceElement(ChoiceGroup choice, MetaschemaModule module)
    {
        if (choice.Choices.Count == 0)
        {
            return null;
        }

        var choiceElement = new XElement(XsdNamespaces.Choice);
        foreach (var item in choice.Choices)
        {
            var childElement = GenerateModelElement(item, module);
            if (childElement is not null)
            {
                choiceElement.Add(childElement);
            }
        }

        return choiceElement.HasElements ? choiceElement : null;
    }

    private XElement GenerateFlagAttribute(FlagInstance flag, MetaschemaModule module)
    {
        var definition = flag.ResolvedDefinition ?? module.GetFlagDefinition(flag.Ref);
        var xsdType = GetXsdTypeReference(definition?.DataTypeName ?? "string");

        var attribute = new XElement(XsdNamespaces.Attribute,
            new XAttribute("name", flag.EffectiveName),
            new XAttribute("type", xsdType));

        if (flag.IsRequired)
        {
            attribute.Add(new XAttribute("use", "required"));
        }

        if (definition?.DefaultValue is not null && !flag.IsRequired)
        {
            attribute.Add(new XAttribute("default", definition.DefaultValue));
        }

        return attribute;
    }

    private string GetXsdTypeReference(string metaschemaTypeName)
    {
        if (XsdTypeMapper.NeedsCustomType(metaschemaTypeName))
        {
            return $"{_targetPrefix}:{GetLocalTypeName(metaschemaTypeName)}";
        }
        return XsdTypeMapper.GetXsdTypeName(metaschemaTypeName);
    }

    private static string GetLocalTypeName(string metaschemaTypeName)
    {
        // Convert metaschema type name to a valid XSD type name
        return metaschemaTypeName.Replace("-", "") + "-type";
    }

    private static void AddCardinality(XElement element, int minOccurs, int? maxOccurs)
    {
        if (minOccurs != 1)
        {
            element.Add(new XAttribute("minOccurs", minOccurs));
        }

        if (maxOccurs != 1)
        {
            element.Add(new XAttribute("maxOccurs",
                maxOccurs.HasValue ? maxOccurs.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "unbounded"));
        }
    }

    private static XElement CreateAnnotation(string documentation)
    {
        return new XElement(XsdNamespaces.Annotation,
            new XElement(XsdNamespaces.Documentation, documentation));
    }

    private static IEnumerable<FlagDefinition> GetAllFlagDefinitions(MetaschemaModule module)
    {
        foreach (var flag in module.FlagDefinitions)
        {
            yield return flag;
        }
        foreach (var import in module.ImportedModules)
        {
            foreach (var flag in GetAllFlagDefinitions(import))
            {
                yield return flag;
            }
        }
    }

    private static IEnumerable<FieldDefinition> GetAllFieldDefinitions(MetaschemaModule module)
    {
        foreach (var field in module.FieldDefinitions)
        {
            yield return field;
        }
        foreach (var import in module.ImportedModules)
        {
            foreach (var field in GetAllFieldDefinitions(import))
            {
                yield return field;
            }
        }
    }

    private static IEnumerable<AssemblyDefinition> GetAllAssemblyDefinitions(MetaschemaModule module)
    {
        foreach (var assembly in module.AssemblyDefinitions)
        {
            yield return assembly;
        }
        foreach (var import in module.ImportedModules)
        {
            foreach (var assembly in GetAllAssemblyDefinitions(import))
            {
                yield return assembly;
            }
        }
    }
}
