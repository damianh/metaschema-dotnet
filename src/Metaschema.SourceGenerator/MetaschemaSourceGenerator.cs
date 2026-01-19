// Licensed under the MIT License.

using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Metaschema.SourceGenerator;

/// <summary>
/// Incremental source generator for generating C# code from Metaschema modules at build time.
/// </summary>
/// <remarks>
/// <para>
/// This generator processes Metaschema XML files and generates C# types for all definitions.
/// It handles imports between metaschema files and generates types for all referenced definitions.
/// </para>
/// <para>
/// Usage: Add metaschema XML files as AdditionalFiles in your project:
/// <code>
/// &lt;ItemGroup&gt;
///   &lt;AdditionalFiles Include="metaschema/*.xml" /&gt;
/// &lt;/ItemGroup&gt;
/// </code>
/// </para>
/// <para>
/// Configuration options (in .csproj):
/// <code>
/// &lt;PropertyGroup&gt;
///   &lt;MetaschemaNamespace&gt;MyNamespace&lt;/MetaschemaNamespace&gt;
///   &lt;MetaschemaVisibility&gt;internal&lt;/MetaschemaVisibility&gt;
/// &lt;/PropertyGroup&gt;
/// </code>
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class MetaschemaSourceGenerator : IIncrementalGenerator
{
    private static readonly XNamespace MetaschemaNamespace = "http://csrc.nist.gov/ns/oscal/metaschema/1.0";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Collect ALL metaschema files at once so we can resolve imports between them
        var allMetaschemaFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, ct) =>
            {
                var text = file.GetText(ct);
                if (text is null) return (Path: file.Path, Content: (string?)null, IsMetaschema: false);

                var content = text.ToString();
                var isMetaschema = content.Contains("METASCHEMA") || content.Contains("metaschema");
                return (Path: file.Path, Content: content, IsMetaschema: isMetaschema);
            })
            .Where(static x => x.IsMetaschema && x.Content is not null)
            .Collect();

        // Get global options for namespace configuration
        var optionsProvider = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) =>
            {
                // Try both casing variants for the property name
                options.GlobalOptions.TryGetValue("build_property.MetaschemaNamespace", out var ns);
                if (ns is null)
                {
                    options.GlobalOptions.TryGetValue("build_property.metaschemanamespace", out ns);
                }
                options.GlobalOptions.TryGetValue("build_property.MetaschemaVisibility", out var visibility);
                if (visibility is null)
                {
                    options.GlobalOptions.TryGetValue("build_property.metaschemavisibility", out visibility);
                }
                return new GeneratorOptions(
                    ns ?? "Generated",
                    visibility?.Equals("internal", StringComparison.OrdinalIgnoreCase) == true
                        ? "internal"
                        : "public"
                );
            });

        // Combine all files with options
        var combined = allMetaschemaFiles.Combine(optionsProvider);

        // Register source output - process all files together
        context.RegisterSourceOutput(combined, static (spc, source) =>
        {
            var (files, options) = source;
            if (files.IsEmpty) return;

            try
            {
                GenerateFromAllFiles(spc, files, options);
            }
            catch (Exception ex)
            {
                var descriptor = new DiagnosticDescriptor(
                    "MTSGEN001",
                    "Metaschema Generation Error",
                    "Failed to generate code: {0}",
                    "MetaschemaSourceGenerator",
                    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);

                spc.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None, ex.Message));
            }
        });
    }

    private static void GenerateFromAllFiles(
        SourceProductionContext spc,
        ImmutableArray<(string Path, string? Content, bool IsMetaschema)> files,
        GeneratorOptions options)
    {
        // Step 1: Parse all files and build a module registry keyed by filename
        var moduleRegistry = new Dictionary<string, ModuleInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var (path, content, _) in files)
        {
            if (content is null) continue;

            XDocument doc;
            try
            {
                doc = XDocument.Parse(content);
            }
            catch
            {
                continue;
            }

            var root = doc.Root;
            if (root is null || root.Name.LocalName != "METASCHEMA") continue;

            var fileName = Path.GetFileName(path);
            var shortName = root.Element(MetaschemaNamespace + "short-name")?.Value ?? Path.GetFileNameWithoutExtension(path);
            var xmlNamespace = root.Element(MetaschemaNamespace + "namespace")?.Value ?? "http://example.com/ns";

            var imports = root.Elements(MetaschemaNamespace + "import")
                .Select(e => e.Attribute("href")?.Value)
                .Where(h => h is not null)
                .Select(h => Path.GetFileName(h!))
                .ToList();

            moduleRegistry[fileName] = new ModuleInfo(path, shortName, xmlNamespace, root, imports!);
        }

        if (moduleRegistry.Count == 0) return;

        // Step 2: Build a global definition registry by processing all modules
        var definitionRegistry = new DefinitionRegistry();

        foreach (var module in moduleRegistry.Values)
        {
            CollectDefinitions(module, definitionRegistry);
        }

        // Step 3: Find root modules (those not imported by any other module)
        var importedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in moduleRegistry.Values)
        {
            foreach (var import in module.Imports)
            {
                importedModules.Add(import);
            }
        }

        var rootModules = moduleRegistry.Keys
            .Where(fileName => !importedModules.Contains(fileName))
            .ToList();

        // Step 4: Generate code for each root module (includes its imports transitively)
        foreach (var rootFileName in rootModules)
        {
            if (!moduleRegistry.TryGetValue(rootFileName, out var rootModule)) continue;

            var generatedCode = GenerateCodeForModule(rootModule, moduleRegistry, definitionRegistry, options);
            if (generatedCode is not null)
            {
                var outputFileName = SanitizeFileName(rootModule.ShortName) + ".g.cs";
                spc.AddSource(outputFileName, SourceText.From(generatedCode, Encoding.UTF8));
            }
        }
    }

    private static void CollectDefinitions(ModuleInfo module, DefinitionRegistry registry)
    {
        var root = module.Root;

        // Collect TOP-LEVEL flag definitions
        foreach (var flagDef in root.Elements(MetaschemaNamespace + "define-flag"))
        {
            var name = flagDef.Attribute("name")?.Value;
            if (name is not null)
            {
                registry.AddFlag(name, new FlagDefinitionInfo(flagDef, module));
            }
        }

        // Collect TOP-LEVEL field definitions
        foreach (var fieldDef in root.Elements(MetaschemaNamespace + "define-field"))
        {
            var name = fieldDef.Attribute("name")?.Value;
            if (name is not null)
            {
                registry.AddField(name, new FieldDefinitionInfo(fieldDef, module));
            }
        }

        // Collect TOP-LEVEL assembly definitions and their INLINE definitions recursively
        foreach (var assemblyDef in root.Elements(MetaschemaNamespace + "define-assembly"))
        {
            var name = assemblyDef.Attribute("name")?.Value;
            if (name is not null)
            {
                registry.AddAssembly(name, new AssemblyDefinitionInfo(assemblyDef, module));
            }
            // Collect inline definitions from this assembly
            CollectInlineDefinitions(assemblyDef, module, registry);
        }
    }

    private static void CollectInlineDefinitions(XElement parent, ModuleInfo module, DefinitionRegistry registry)
    {
        // Look for model element containing inline definitions
        var model = parent.Element(MetaschemaNamespace + "model");
        if (model != null)
        {
            CollectInlineDefinitionsFromContainer(model, module, registry);
        }

        // Also check for inline flag definitions directly on the parent
        foreach (var flagDef in parent.Elements(MetaschemaNamespace + "define-flag"))
        {
            var name = flagDef.Attribute("name")?.Value;
            if (name is not null)
            {
                registry.AddFlag(name, new FlagDefinitionInfo(flagDef, module));
            }
        }
    }

    private static void CollectInlineDefinitionsFromContainer(XElement container, ModuleInfo module, DefinitionRegistry registry)
    {
        foreach (var element in container.Elements())
        {
            var localName = element.Name.LocalName;

            switch (localName)
            {
                case "define-flag":
                    {
                        var name = element.Attribute("name")?.Value;
                        if (name is not null)
                        {
                            registry.AddFlag(name, new FlagDefinitionInfo(element, module));
                        }
                        break;
                    }
                case "define-field":
                    {
                        var name = element.Attribute("name")?.Value;
                        if (name is not null)
                        {
                            registry.AddField(name, new FieldDefinitionInfo(element, module));
                        }
                        // Recurse for inline flags within the field
                        CollectInlineDefinitions(element, module, registry);
                        break;
                    }
                case "define-assembly":
                    {
                        var name = element.Attribute("name")?.Value;
                        if (name is not null)
                        {
                            registry.AddAssembly(name, new AssemblyDefinitionInfo(element, module));
                        }
                        // Recurse for nested inline definitions
                        CollectInlineDefinitions(element, module, registry);
                        break;
                    }
                case "choice":
                    // Recurse into choice groups
                    CollectInlineDefinitionsFromContainer(element, module, registry);
                    break;
            }
        }
    }

    private static string? GenerateCodeForModule(
        ModuleInfo rootModule,
        Dictionary<string, ModuleInfo> moduleRegistry,
        DefinitionRegistry definitionRegistry,
        GeneratorOptions options)
    {
        // Collect all modules to process (root + transitively imported)
        var modulesToProcess = new List<ModuleInfo>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CollectModulesTransitively(rootModule, moduleRegistry, modulesToProcess, visited);

        // Track generated types to avoid duplicates
        var generatedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// This file was generated by Metaschema.SourceGenerator.");
        sb.AppendLine("// Do not modify this file directly.");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine($"namespace {options.Namespace};");
        sb.AppendLine();

        // Generate types for all modules (in reverse order so dependencies come first)
        modulesToProcess.Reverse();

        // First, collect all assembly and field names across all modules to detect collisions
        var assemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var fieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in modulesToProcess)
        {
            foreach (var assemblyDef in module.Root.Descendants(MetaschemaNamespace + "define-assembly"))
            {
                var name = assemblyDef.Attribute("name")?.Value;
                if (name is not null) assemblyNames.Add(ToPascalCase(name));
            }
            foreach (var fieldDef in module.Root.Descendants(MetaschemaNamespace + "define-field"))
            {
                var name = fieldDef.Attribute("name")?.Value;
                if (name is not null) fieldNames.Add(ToPascalCase(name));
            }
        }

        foreach (var module in modulesToProcess)
        {
            // Generate TOP-LEVEL and INLINE flag definitions
            GenerateAllFlagsFromModule(sb, module.Root, options, generatedTypes, assemblyNames, fieldNames);

            // Generate TOP-LEVEL and INLINE field definitions
            GenerateAllFieldsFromModule(sb, module.Root, options, definitionRegistry, generatedTypes);

            // Generate TOP-LEVEL and INLINE assembly definitions
            GenerateAllAssembliesFromModule(sb, module.Root, options, definitionRegistry, generatedTypes);
        }

        return sb.ToString();
    }

    private static void GenerateAllFlagsFromModule(
        StringBuilder sb,
        XElement root,
        GeneratorOptions options,
        HashSet<string> generatedTypes,
        HashSet<string> assemblyNames,
        HashSet<string> fieldNames)
    {
        // Find ALL define-flag elements (both top-level and inline)
        foreach (var flagDef in root.Descendants(MetaschemaNamespace + "define-flag"))
        {
            var name = flagDef.Attribute("name")?.Value;
            if (name is null) continue;

            var className = ToPascalCase(name);
            
            // If an assembly or field has the same name, suffix the flag type
            if (assemblyNames.Contains(className) || fieldNames.Contains(className))
            {
                className = className + "Flag";
            }
            
            // Track by definition type to allow same name for flag/field/assembly
            var trackingKey = "flag:" + className;
            if (generatedTypes.Contains(trackingKey)) continue;
            generatedTypes.Add(trackingKey);

            GenerateFlagDefinition(sb, flagDef, options, className);
            sb.AppendLine();
        }
    }

    private static void GenerateAllFieldsFromModule(
        StringBuilder sb,
        XElement root,
        GeneratorOptions options,
        DefinitionRegistry registry,
        HashSet<string> generatedTypes)
    {
        // Find ALL define-field elements (both top-level and inline)
        foreach (var fieldDef in root.Descendants(MetaschemaNamespace + "define-field"))
        {
            var name = fieldDef.Attribute("name")?.Value;
            if (name is null) continue;

            var className = ToPascalCase(name);
            // Track by definition type to allow same name for flag/field/assembly
            var trackingKey = "field:" + className;
            if (generatedTypes.Contains(trackingKey)) continue;
            generatedTypes.Add(trackingKey);

            GenerateFieldDefinition(sb, fieldDef, options, registry);
            sb.AppendLine();
        }
    }

    private static void GenerateAllAssembliesFromModule(
        StringBuilder sb,
        XElement root,
        GeneratorOptions options,
        DefinitionRegistry registry,
        HashSet<string> generatedTypes)
    {
        // Find ALL define-assembly elements (both top-level and inline)
        foreach (var assemblyDef in root.Descendants(MetaschemaNamespace + "define-assembly"))
        {
            var name = assemblyDef.Attribute("name")?.Value;
            if (name is null) continue;

            var className = ToPascalCase(name);
            // Track by definition type to allow same name for flag/field/assembly
            var trackingKey = "assembly:" + className;
            if (generatedTypes.Contains(trackingKey)) continue;
            generatedTypes.Add(trackingKey);

            GenerateAssemblyDefinition(sb, assemblyDef, options, registry);
            sb.AppendLine();
        }
    }

    private static void CollectModulesTransitively(
        ModuleInfo module,
        Dictionary<string, ModuleInfo> registry,
        List<ModuleInfo> result,
        HashSet<string> visited)
    {
        var fileName = Path.GetFileName(module.Path);
        if (visited.Contains(fileName)) return;
        visited.Add(fileName);

        result.Add(module);

        foreach (var import in module.Imports)
        {
            if (registry.TryGetValue(import, out var importedModule))
            {
                CollectModulesTransitively(importedModule, registry, result, visited);
            }
        }
    }

    private static void GenerateFlagDefinition(StringBuilder sb, XElement flagDef, GeneratorOptions options, string className)
    {
        var name = flagDef.Attribute("name")?.Value ?? "Unknown";
        var formalName = flagDef.Element(MetaschemaNamespace + "formal-name")?.Value;
        var description = GetPlainTextDescription(flagDef.Element(MetaschemaNamespace + "description"));
        var dataType = flagDef.Attribute("as-type")?.Value ?? "string";
        var clrType = GetClrTypeName(dataType);
        
        // Use safe property name to avoid class/property name conflicts
        var valuePropertyName = GetSafePropertyName("Value", className);

        // XML documentation
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// {EscapeXml(formalName ?? name)}{(description != null ? " - " + EscapeXml(description) : "")}");
        sb.AppendLine("/// </summary>");

        // Struct declaration
        sb.AppendLine($"{options.Visibility} readonly partial struct {className} : IEquatable<{className}>");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>Gets the underlying value.</summary>");
        sb.AppendLine($"    public {clrType} {valuePropertyName} {{ get; }}");
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Initializes a new instance.</summary>");
        sb.AppendLine($"    public {className}({clrType} value) => {valuePropertyName} = value;");
        sb.AppendLine();
        sb.AppendLine($"    public static implicit operator {className}({clrType} value) => new {className}(value);");
        sb.AppendLine($"    public static implicit operator {clrType}({className} flag) => flag.{valuePropertyName};");
        sb.AppendLine($"    public bool Equals({className} other) => {valuePropertyName}.Equals(other.{valuePropertyName});");
        sb.AppendLine($"    public override bool Equals(object? obj) => obj is {className} other && Equals(other);");
        sb.AppendLine($"    public override int GetHashCode() => {valuePropertyName}.GetHashCode();");
        sb.AppendLine($"    public override string? ToString() => {valuePropertyName}.ToString();");
        sb.AppendLine($"    public static bool operator ==({className} left, {className} right) => left.Equals(right);");
        sb.AppendLine($"    public static bool operator !=({className} left, {className} right) => !left.Equals(right);");
        sb.AppendLine("}");
    }

    private static void GenerateFieldDefinition(
        StringBuilder sb,
        XElement fieldDef,
        GeneratorOptions options,
        DefinitionRegistry registry)
    {
        var name = fieldDef.Attribute("name")?.Value ?? "Unknown";
        var formalName = fieldDef.Element(MetaschemaNamespace + "formal-name")?.Value;
        var description = GetPlainTextDescription(fieldDef.Element(MetaschemaNamespace + "description"));
        var dataType = fieldDef.Attribute("as-type")?.Value ?? "string";
        var className = ToPascalCase(name);
        var clrType = GetClrTypeName(dataType);

        // Check if field has flags - if so, it's complex and can't have implicit conversions
        var hasFlags = fieldDef.Elements(MetaschemaNamespace + "flag").Any() ||
                       fieldDef.Elements(MetaschemaNamespace + "define-flag").Any();

        // XML documentation
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// {EscapeXml(formalName ?? name)}{(description != null ? " - " + EscapeXml(description) : "")}");
        sb.AppendLine("/// </summary>");

        // Class declaration
        sb.AppendLine($"{options.Visibility} partial class {className}");
        sb.AppendLine("{");
        
        // Use a safe property name for the Value property to avoid conflicts
        var valuePropertyName = GetSafePropertyName("Value", className);
        sb.AppendLine($"    /// <summary>Gets or sets the field value.</summary>");
        sb.AppendLine($"    public {clrType}? {valuePropertyName} {{ get; set; }}");

        // Add implicit conversion operators for simple fields (no flags)
        // Markup types (markup-line, markup-multiline) map to string and support implicit conversion
        if (!hasFlags)
        {
            sb.AppendLine();
            sb.AppendLine($"    /// <summary>Implicitly converts a {clrType} to a {className}.</summary>");
            sb.AppendLine($"    public static implicit operator {className}({clrType} value) => new {className} {{ {valuePropertyName} = value }};");
            sb.AppendLine($"    /// <summary>Implicitly converts a {className} to its underlying value.</summary>");
            sb.AppendLine($"    public static implicit operator {clrType}?({className}? field) => field?.{valuePropertyName};");
        }

        // Add ToString override for proper display
        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Returns the string representation of this field.</summary>");
        sb.AppendLine($"    public override string? ToString() => {valuePropertyName}?.ToString();");

        // Generate flag properties (both references and inline definitions)
        foreach (var flag in fieldDef.Elements(MetaschemaNamespace + "flag"))
        {
            GenerateFlagProperty(sb, flag, registry, className);
        }
        foreach (var flagDef2 in fieldDef.Elements(MetaschemaNamespace + "define-flag"))
        {
            GenerateInlineFlagProperty(sb, flagDef2, className);
        }

        sb.AppendLine("}");
    }

    private static void GenerateAssemblyDefinition(
        StringBuilder sb,
        XElement assemblyDef,
        GeneratorOptions options,
        DefinitionRegistry registry)
    {
        var name = assemblyDef.Attribute("name")?.Value ?? "Unknown";
        var formalName = assemblyDef.Element(MetaschemaNamespace + "formal-name")?.Value;
        var description = assemblyDef.Element(MetaschemaNamespace + "description")?.Value;
        var className = ToPascalCase(name);

        // XML documentation
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// {EscapeXml(formalName ?? name)}{(description != null ? " - " + EscapeXml(description) : "")}");
        sb.AppendLine("/// </summary>");

        // Class declaration
        sb.AppendLine($"{options.Visibility} partial class {className}");
        sb.AppendLine("{");

        // Generate flag properties (references and inline)
        foreach (var flag in assemblyDef.Elements(MetaschemaNamespace + "flag"))
        {
            GenerateFlagProperty(sb, flag, registry, className);
        }
        foreach (var flagDef in assemblyDef.Elements(MetaschemaNamespace + "define-flag"))
        {
            GenerateInlineFlagProperty(sb, flagDef, className);
        }

        // Generate model properties
        var model = assemblyDef.Element(MetaschemaNamespace + "model");
        if (model != null)
        {
            GenerateModelProperties(sb, model, registry);
        }

        sb.AppendLine("}");
    }

    private static void GenerateFlagProperty(StringBuilder sb, XElement flag, DefinitionRegistry registry, string className)
    {
        var refName = flag.Attribute("ref")?.Value;
        var inlineName = flag.Attribute("name")?.Value;
        var effectiveName = refName ?? inlineName;
        if (effectiveName is null) return;

        var propName = GetSafePropertyName(effectiveName, className);
        var required = flag.Attribute("required")?.Value == "yes";

        // Try to get the type from the registry
        var clrType = "string";
        if (refName is not null && registry.TryGetFlag(refName, out var flagInfo))
        {
            var dataType = flagInfo.Element.Attribute("as-type")?.Value ?? "string";
            clrType = GetClrTypeName(dataType);
        }

        // Reference types are always nullable since we don't generate constructors
        var nullable = required && IsValueType(clrType) ? "" : "?";

        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Gets or sets the {effectiveName} flag.</summary>");
        sb.AppendLine($"    public {clrType}{nullable} {propName} {{ get; set; }}");
    }

    private static void GenerateInlineFlagProperty(StringBuilder sb, XElement flagDef, string className)
    {
        var name = flagDef.Attribute("name")?.Value;
        if (name is null) return;

        var propName = GetSafePropertyName(name, className);
        var dataType = flagDef.Attribute("as-type")?.Value ?? "string";
        var clrType = GetClrTypeName(dataType);
        var required = flagDef.Attribute("required")?.Value == "yes";
        
        // Reference types are always nullable since we don't generate constructors
        var nullable = required && IsValueType(clrType) ? "" : "?";

        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Gets or sets the {name} flag.</summary>");
        sb.AppendLine($"    public {clrType}{nullable} {propName} {{ get; set; }}");
    }

    private static void GenerateModelProperties(StringBuilder sb, XElement model, DefinitionRegistry registry)
    {
        // Process direct children of model, handling choice groups correctly
        foreach (var element in model.Elements())
        {
            var localName = element.Name.LocalName;

            switch (localName)
            {
                case "field":
                    GenerateFieldInstanceProperty(sb, element, registry);
                    break;
                case "assembly":
                    GenerateAssemblyInstanceProperty(sb, element, registry);
                    break;
                case "define-field":
                    // Inline field definition - generate a property for it
                    GenerateInlineFieldProperty(sb, element);
                    break;
                case "define-assembly":
                    // Inline assembly definition - generate a property for it
                    GenerateInlineAssemblyProperty(sb, element);
                    break;
                case "choice":
                    // Process all elements within the choice group
                    foreach (var choiceElement in element.Elements())
                    {
                        var choiceLocalName = choiceElement.Name.LocalName;
                        switch (choiceLocalName)
                        {
                            case "field":
                                GenerateFieldInstanceProperty(sb, choiceElement, registry);
                                break;
                            case "assembly":
                                GenerateAssemblyInstanceProperty(sb, choiceElement, registry);
                                break;
                            case "define-field":
                                GenerateInlineFieldProperty(sb, choiceElement);
                                break;
                            case "define-assembly":
                                GenerateInlineAssemblyProperty(sb, choiceElement);
                                break;
                        }
                    }
                    break;
            }
        }
    }

    private static void GenerateFieldInstanceProperty(StringBuilder sb, XElement fieldRef, DefinitionRegistry registry)
    {
        var refName = fieldRef.Attribute("ref")?.Value;
        if (refName is null) return;

        var groupAs = fieldRef.Element(MetaschemaNamespace + "group-as");
        var propName = ToPascalCase(groupAs?.Attribute("name")?.Value ?? refName);
        var typeName = ToPascalCase(refName);
        var maxOccurs = fieldRef.Attribute("max-occurs")?.Value ?? "1";

        var isCollection = maxOccurs == "unbounded" || (int.TryParse(maxOccurs, out var max) && max > 1);

        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Gets or sets the {refName} field.</summary>");
        if (isCollection)
        {
            sb.AppendLine($"    public List<{typeName}> {propName} {{ get; set; }} = new List<{typeName}>();");
        }
        else
        {
            // Always nullable for reference types since we don't generate constructors
            sb.AppendLine($"    public {typeName}? {propName} {{ get; set; }}");
        }
    }

    private static void GenerateAssemblyInstanceProperty(StringBuilder sb, XElement assemblyRef, DefinitionRegistry registry)
    {
        var refName = assemblyRef.Attribute("ref")?.Value;
        if (refName is null) return;

        var groupAs = assemblyRef.Element(MetaschemaNamespace + "group-as");
        var propName = ToPascalCase(groupAs?.Attribute("name")?.Value ?? refName);
        var typeName = ToPascalCase(refName);
        var maxOccurs = assemblyRef.Attribute("max-occurs")?.Value ?? "1";

        var isCollection = maxOccurs == "unbounded" || (int.TryParse(maxOccurs, out var max) && max > 1);

        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Gets or sets the {refName} assembly.</summary>");
        if (isCollection)
        {
            sb.AppendLine($"    public List<{typeName}> {propName} {{ get; set; }} = new List<{typeName}>();");
        }
        else
        {
            // Always nullable for reference types since we don't generate constructors
            sb.AppendLine($"    public {typeName}? {propName} {{ get; set; }}");
        }
    }

    private static void GenerateInlineFieldProperty(StringBuilder sb, XElement fieldDef)
    {
        var name = fieldDef.Attribute("name")?.Value;
        if (name is null) return;

        var groupAs = fieldDef.Element(MetaschemaNamespace + "group-as");
        var propName = ToPascalCase(groupAs?.Attribute("name")?.Value ?? name);
        var typeName = ToPascalCase(name);
        var maxOccurs = fieldDef.Attribute("max-occurs")?.Value ?? "1";

        var isCollection = maxOccurs == "unbounded" || (int.TryParse(maxOccurs, out var max) && max > 1);

        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Gets or sets the {name} field.</summary>");
        if (isCollection)
        {
            sb.AppendLine($"    public List<{typeName}> {propName} {{ get; set; }} = new List<{typeName}>();");
        }
        else
        {
            // Always nullable for reference types since we don't generate constructors
            sb.AppendLine($"    public {typeName}? {propName} {{ get; set; }}");
        }
    }

    private static void GenerateInlineAssemblyProperty(StringBuilder sb, XElement assemblyDef)
    {
        var name = assemblyDef.Attribute("name")?.Value;
        if (name is null) return;

        var groupAs = assemblyDef.Element(MetaschemaNamespace + "group-as");
        var propName = ToPascalCase(groupAs?.Attribute("name")?.Value ?? name);
        var typeName = ToPascalCase(name);
        var maxOccurs = assemblyDef.Attribute("max-occurs")?.Value ?? "1";

        var isCollection = maxOccurs == "unbounded" || (int.TryParse(maxOccurs, out var max) && max > 1);

        sb.AppendLine();
        sb.AppendLine($"    /// <summary>Gets or sets the {name} assembly.</summary>");
        if (isCollection)
        {
            sb.AppendLine($"    public List<{typeName}> {propName} {{ get; set; }} = new List<{typeName}>();");
        }
        else
        {
            // Always nullable for reference types since we don't generate constructors
            sb.AppendLine($"    public {typeName}? {propName} {{ get; set; }}");
        }
    }

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;

        var sb = new StringBuilder();
        var capitalizeNext = true;

        foreach (var c in name)
        {
            if (c == '-' || c == '_' || c == '.')
            {
                capitalizeNext = true;
                continue;
            }

            if (capitalizeNext)
            {
                sb.Append(char.ToUpperInvariant(c));
                capitalizeNext = false;
            }
            else
            {
                sb.Append(c);
            }
        }

        if (sb.Length > 0)
        {
            sb[0] = char.ToUpperInvariant(sb[0]);
        }

        return sb.ToString();
    }

    private static string GetClrTypeName(string metaschemaTypeName)
    {
        switch (metaschemaTypeName)
        {
            case "string":
            case "token":
            case "email-address":
            case "hostname":
            case "ncname":
            case "ip-v4-address":
            case "ip-v6-address":
            case "markup-line":
            case "markup-multiline":
                return "string";
            case "uri":
            case "uri-reference":
                return "Uri";
            case "uuid":
                return "Guid";
            case "integer":
                return "long";
            case "non-negative-integer":
            case "positive-integer":
                return "ulong";
            case "decimal":
                return "decimal";
            case "boolean":
                return "bool";
            case "date":
                return "DateTime";
            case "date-time":
            case "date-with-timezone":
            case "date-time-with-timezone":
                return "DateTimeOffset";
            case "base64":
                return "byte[]";
            default:
                return "string";
        }
    }

    private static bool IsValueType(string clrTypeName)
    {
        switch (clrTypeName)
        {
            case "Guid":
            case "bool":
            case "long":
            case "ulong":
            case "decimal":
            case "DateTime":
            case "DateTimeOffset":
                return true;
            default:
                return false;
        }
    }

    private static string? GetPlainTextDescription(XElement? descriptionElement)
    {
        if (descriptionElement == null) return null;
        // Get the text content, stripping any markup elements
        return string.Join(" ", descriptionElement.DescendantNodes()
            .OfType<System.Xml.Linq.XText>()
            .Select(t => t.Value))
            .Trim();
    }

    private static string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        // First, normalize whitespace (replace newlines and multiple spaces with single space)
        var normalized = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        // Then escape XML entities
        return normalized
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private static string GetSafePropertyName(string name, string className)
    {
        var propName = ToPascalCase(name);
        // Avoid property name conflicting with class name
        if (propName == className)
        {
            propName = propName + "Value";
        }
        // Handle C# reserved keywords
        if (IsCSharpKeyword(propName))
        {
            propName = "@" + propName;
        }
        return propName;
    }

    private static bool IsCSharpKeyword(string name)
    {
        // Only true reserved keywords that require @ prefix
        // Note: "value" is only contextual in property setters, not a reserved keyword
        var keywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
            "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
            "void", "volatile", "while"
        };
        return keywords.Contains(name.ToLowerInvariant());
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder();
        foreach (var c in name)
        {
            sb.Append(invalid.Contains(c) ? '_' : c);
        }
        return sb.ToString();
    }

    // Helper types for tracking modules and definitions

    private readonly struct GeneratorOptions
    {
        public string Namespace { get; }
        public string Visibility { get; }

        public GeneratorOptions(string ns, string visibility)
        {
            Namespace = ns;
            Visibility = visibility;
        }
    }

    private sealed class ModuleInfo
    {
        public string Path { get; }
        public string ShortName { get; }
        public string XmlNamespace { get; }
        public XElement Root { get; }
        public List<string> Imports { get; }

        public ModuleInfo(string path, string shortName, string xmlNamespace, XElement root, List<string> imports)
        {
            Path = path;
            ShortName = shortName;
            XmlNamespace = xmlNamespace;
            Root = root;
            Imports = imports;
        }
    }

    private sealed class DefinitionRegistry
    {
        private readonly Dictionary<string, FlagDefinitionInfo> _flags = new Dictionary<string, FlagDefinitionInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, FieldDefinitionInfo> _fields = new Dictionary<string, FieldDefinitionInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AssemblyDefinitionInfo> _assemblies = new Dictionary<string, AssemblyDefinitionInfo>(StringComparer.OrdinalIgnoreCase);

        public void AddFlag(string name, FlagDefinitionInfo info)
        {
            if (!_flags.ContainsKey(name))
            {
                _flags[name] = info;
            }
        }

        public void AddField(string name, FieldDefinitionInfo info)
        {
            if (!_fields.ContainsKey(name))
            {
                _fields[name] = info;
            }
        }

        public void AddAssembly(string name, AssemblyDefinitionInfo info)
        {
            if (!_assemblies.ContainsKey(name))
            {
                _assemblies[name] = info;
            }
        }

#pragma warning disable CS8601 // Possible null reference assignment - caller handles null
        public bool TryGetFlag(string name, out FlagDefinitionInfo info) => _flags.TryGetValue(name, out info);
        public bool TryGetField(string name, out FieldDefinitionInfo info) => _fields.TryGetValue(name, out info);
        public bool TryGetAssembly(string name, out AssemblyDefinitionInfo info) => _assemblies.TryGetValue(name, out info);
#pragma warning restore CS8601
    }

    private sealed class FlagDefinitionInfo
    {
        public XElement Element { get; }
        public ModuleInfo Module { get; }

        public FlagDefinitionInfo(XElement element, ModuleInfo module)
        {
            Element = element;
            Module = module;
        }
    }

    private sealed class FieldDefinitionInfo
    {
        public XElement Element { get; }
        public ModuleInfo Module { get; }

        public FieldDefinitionInfo(XElement element, ModuleInfo module)
        {
            Element = element;
            Module = module;
        }
    }

    private sealed class AssemblyDefinitionInfo
    {
        public XElement Element { get; }
        public ModuleInfo Module { get; }

        public AssemblyDefinitionInfo(XElement element, ModuleInfo module)
        {
            Element = element;
            Module = module;
        }
    }
}
