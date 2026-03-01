// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Text;
using Metaschema.Model;

namespace Metaschema.CodeGeneration;

/// <summary>
/// Generates C# record types with System.Text.Json source generation support.
/// </summary>
public sealed class RecordCodeGenerator
{
    private readonly CodeGenerationOptions _options;
    private readonly List<string> _allTypeNames = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordCodeGenerator"/> class.
    /// </summary>
    /// <param name="options">The code generation options.</param>
    public RecordCodeGenerator(CodeGenerationOptions? options = null) => _options = options ?? new CodeGenerationOptions();

    /// <summary>
    /// Generates C# source code for all definitions in a module and its imports.
    /// </summary>
    /// <param name="module">The Metaschema module.</param>
    /// <returns>A dictionary of file names to source code.</returns>
    public Dictionary<string, string> Generate(MetaschemaModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        var files = new Dictionary<string, string>();
        _allTypeNames.Clear();

        // Collect all modules (main + imports)
        var allModules = new List<MetaschemaModule> { module };
        CollectImportedModules(module, allModules);

        // Collect all type names first for JsonSerializerContext
        foreach (var mod in allModules)
        {
            // For imported modules, include all definitions (global and local)
            // For the main module, only include global definitions
            var isMainModule = mod == module;

            foreach (var flag in mod.FlagDefinitions.Where(f => !isMainModule || f.Scope == Scope.Global))
            {
                var typeName = GetTypeName(flag.Name);
                if (!_allTypeNames.Contains(typeName))
                {
                    _allTypeNames.Add(typeName);
                }
            }
            foreach (var field in mod.FieldDefinitions.Where(f => !isMainModule || f.Scope == Scope.Global))
            {
                var typeName = GetTypeName(field.Name);
                if (!_allTypeNames.Contains(typeName))
                {
                    _allTypeNames.Add(typeName);
                }
            }
            foreach (var assembly in mod.AssemblyDefinitions.Where(a => !isMainModule || a.Scope == Scope.Global))
            {
                var typeName = GetTypeName(assembly.Name);
                if (!_allTypeNames.Contains(typeName))
                {
                    _allTypeNames.Add(typeName);
                }
            }
        }

        // Generate individual type files from all modules
        foreach (var mod in allModules)
        {
            var isMainModule = mod == module;

            foreach (var flag in mod.FlagDefinitions.Where(f => !isMainModule || f.Scope == Scope.Global))
            {
                var typeName = GetTypeName(flag.Name);
                if (!files.ContainsKey(typeName + ".g.cs"))
                {
                    var source = GenerateFlagRecord(flag);
                    files[typeName + ".g.cs"] = source;
                }
            }

            foreach (var field in mod.FieldDefinitions.Where(f => !isMainModule || f.Scope == Scope.Global))
            {
                var typeName = GetTypeName(field.Name);
                if (!files.ContainsKey(typeName + ".g.cs"))
                {
                    var source = GenerateFieldRecord(field);
                    files[typeName + ".g.cs"] = source;
                }
            }

            foreach (var assembly in mod.AssemblyDefinitions.Where(a => !isMainModule || a.Scope == Scope.Global))
            {
                var typeName = GetTypeName(assembly.Name);
                if (!files.ContainsKey(typeName + ".g.cs"))
                {
                    var source = GenerateAssemblyRecord(assembly);
                    files[typeName + ".g.cs"] = source;
                }
            }
        }

        // Generate JsonSerializerContext if requested
        if (_options.GenerateJsonContext)
        {
            var contextName = _options.JsonContextName ?? $"{_options.Namespace.Split('.').Last()}JsonContext";
            files[contextName + ".g.cs"] = GenerateJsonContext(contextName, module);
        }

        // Generate extension methods if requested
        if (_options.GenerateExtensionMethods)
        {
            var rootAssembly = module.AssemblyDefinitions.FirstOrDefault(a => a.RootName is not null);
            if (rootAssembly is not null)
            {
                files["Extensions.g.cs"] = GenerateExtensions(rootAssembly);
            }
        }

        return files;
    }

    private static void CollectImportedModules(MetaschemaModule module, List<MetaschemaModule> collected)
    {
        foreach (var imported in module.ImportedModules)
        {
            if (!collected.Contains(imported))
            {
                collected.Add(imported);
                CollectImportedModules(imported, collected);
            }
        }
    }

    private string GenerateFlagRecord(FlagDefinition flag)
    {
        var w = new CodeWriter();
        AppendFileHeader(w);
        AppendUsings(w, includeJson: true);
        w.Line();

        if (_options.FileScopedNamespaces)
        {
            w.Line($"namespace {_options.Namespace};");
            w.Line();
            GenerateFlagRecordContent(w, flag);
        }
        else
        {
            w.Line($"namespace {_options.Namespace}");
            w.Line("{");
            w.Indent();
            GenerateFlagRecordContent(w, flag);
            w.Outdent();
            w.Line("}");
        }

        return w.ToString();
    }

    private void GenerateFlagRecordContent(CodeWriter w, FlagDefinition flag)
    {
        var visibility = _options.Visibility == TypeVisibility.Public ? "public" : "internal";
        var typeName = GetTypeName(flag.Name);
        var dataType = GetClrTypeName(flag.DataTypeName);

        if (_options.IncludeDocumentation)
        {
            AppendXmlDoc(w, flag.FormalName ?? flag.Name, flag.Description?.ToString());
        }

        if (flag.DeprecatedVersion is not null)
        {
            w.Line($"[Obsolete(\"Deprecated since version {flag.DeprecatedVersion}\")]");
        }

        // Flag records are simple value wrappers
        w.Line($"{visibility} readonly record struct {typeName}(");
        w.Indent();
        if (_options.IncludeDocumentation)
        {
            w.Line("/// <summary>");
            w.Line("/// The underlying value.");
            w.Line("/// </summary>");
        }
        w.Line($"{dataType} Value);");
        w.Outdent();
    }

    private string GenerateFieldRecord(FieldDefinition field)
    {
        var w = new CodeWriter();
        AppendFileHeader(w);
        AppendUsings(w, includeJson: true);
        w.Line();

        if (_options.FileScopedNamespaces)
        {
            w.Line($"namespace {_options.Namespace};");
            w.Line();
            GenerateFieldRecordContent(w, field);
        }
        else
        {
            w.Line($"namespace {_options.Namespace}");
            w.Line("{");
            w.Indent();
            GenerateFieldRecordContent(w, field);
            w.Outdent();
            w.Line("}");
        }

        return w.ToString();
    }

    private void GenerateFieldRecordContent(CodeWriter w, FieldDefinition field)
    {
        var visibility = _options.Visibility == TypeVisibility.Public ? "public" : "internal";
        var typeName = GetTypeName(field.Name);
        var dataType = GetClrTypeName(field.DataTypeName);
        var jsonValueKey = field.JsonValueKeyName ?? "value";
        var isSimpleField = field.FlagInstances.Count == 0;

        if (_options.IncludeDocumentation)
        {
            AppendXmlDoc(w, field.FormalName ?? field.Name, field.Description?.ToString());
        }

        if (field.DeprecatedVersion is not null)
        {
            w.Line($"[Obsolete(\"Deprecated since version {field.DeprecatedVersion}\")]");
        }

        // Simple fields (no flags) need a custom converter to deserialize from JSON primitives
        if (isSimpleField)
        {
            w.Line($"[JsonConverter(typeof({typeName}JsonConverter))]");
        }

        w.Line($"{visibility} sealed record {typeName}");
        w.Line("{");
        w.Indent();

        // Field value property
        if (_options.IncludeDocumentation)
        {
            w.Line("/// <summary>");
            w.Line("/// Gets the field value.");
            w.Line("/// </summary>");
        }
        w.Line($"[JsonPropertyName(\"{jsonValueKey}\")]");
        w.Line($"public {dataType}? Value {{ get; init; }}");

        // Flag properties
        foreach (var flagInstance in field.FlagInstances)
        {
            w.Line();
            GenerateFlagProperty(w, flagInstance);
        }

        w.Outdent();
        w.Line("}");

        // Generate converter for simple fields
        if (isSimpleField)
        {
            w.Line();
            GenerateSimpleFieldConverter(w, typeName, dataType);
        }
    }

    private string GenerateAssemblyRecord(AssemblyDefinition assembly)
    {
        var w = new CodeWriter();
        AppendFileHeader(w);
        AppendUsings(w, includeJson: true);
        w.Line();

        if (_options.FileScopedNamespaces)
        {
            w.Line($"namespace {_options.Namespace};");
            w.Line();
            GenerateAssemblyRecordContent(w, assembly);
        }
        else
        {
            w.Line($"namespace {_options.Namespace}");
            w.Line("{");
            w.Indent();
            GenerateAssemblyRecordContent(w, assembly);
            w.Outdent();
            w.Line("}");
        }

        return w.ToString();
    }

    private void GenerateAssemblyRecordContent(CodeWriter w, AssemblyDefinition assembly)
    {
        var visibility = _options.Visibility == TypeVisibility.Public ? "public" : "internal";
        var typeName = GetTypeName(assembly.Name);

        if (_options.IncludeDocumentation)
        {
            AppendXmlDoc(w, assembly.FormalName ?? assembly.Name, assembly.Description?.ToString());
        }

        if (assembly.DeprecatedVersion is not null)
        {
            w.Line($"[Obsolete(\"Deprecated since version {assembly.DeprecatedVersion}\")]");
        }

        w.Line($"{visibility} sealed record {typeName}");
        w.Line("{");
        w.Indent();

        // Flag properties
        foreach (var flagInstance in assembly.FlagInstances)
        {
            GenerateFlagProperty(w, flagInstance);
            w.Line();
        }

        // Model properties
        if (assembly.Model is not null)
        {
            GenerateModelProperties(w, assembly.Model);
        }

        w.Outdent();
        w.Line("}");
    }

    private void GenerateFlagProperty(CodeWriter w, FlagInstance flagInstance)
    {
        var propName = GetPropertyName(flagInstance.EffectiveName);
        var dataType = flagInstance.ResolvedDefinition?.DataTypeName ?? "string";
        var clrType = GetClrTypeName(dataType);
        var isRequired = flagInstance.IsRequired;
        var jsonName = flagInstance.EffectiveName;

        if (_options.IncludeDocumentation)
        {
            var formalName = flagInstance.FormalName ?? flagInstance.ResolvedDefinition?.FormalName;
            var description = flagInstance.Description?.ToString() ?? flagInstance.ResolvedDefinition?.Description?.ToString();
            AppendXmlDoc(w, formalName ?? propName, description);
        }

        if (flagInstance.DeprecatedVersion is not null)
        {
            w.Line($"[Obsolete(\"Deprecated since version {flagInstance.DeprecatedVersion}\")]");
        }

        w.Line($"[JsonPropertyName(\"{jsonName}\")]");

        var nullable = !isRequired ? "?" : "";
        var required = isRequired ? "required " : "";
        w.Line($"public {required}{clrType}{nullable} {propName} {{ get; init; }}");
    }

    private void GenerateSimpleFieldConverter(CodeWriter w, string typeName, string dataType)
    {
        var visibility = _options.Visibility == TypeVisibility.Public ? "public" : "internal";

        if (_options.IncludeDocumentation)
        {
            w.Line("/// <summary>");
            w.Line($"/// JSON converter for {typeName} that handles direct primitive values.");
            w.Line("/// </summary>");
        }

        w.Line($"{visibility} sealed class {typeName}JsonConverter : JsonConverter<{typeName}>");
        w.Line("{");
        w.Indent();

        // Read method
        w.Line($"public override {typeName}? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)");
        w.Line("{");
        w.Indent();
        w.Line("if (reader.TokenType == JsonTokenType.Null)");
        w.Line("{");
        w.Indent();
        w.Line("return null;");
        w.Outdent();
        w.Line("}");
        w.Line();

        // Handle direct primitive value
        GenerateConverterReadLogic(w, dataType, typeName);

        w.Outdent();
        w.Line("}");
        w.Line();

        // Write method
        w.Line($"public override void Write(Utf8JsonWriter writer, {typeName} value, JsonSerializerOptions options)");
        w.Line("{");
        w.Indent();
        w.Line("if (value.Value is null)");
        w.Line("{");
        w.Indent();
        w.Line("writer.WriteNullValue();");
        w.Outdent();
        w.Line("}");
        w.Line("else");
        w.Line("{");
        w.Indent();

        // Handle direct primitive value writing
        GenerateConverterWriteLogic(w, dataType);

        w.Outdent();
        w.Line("}");
        w.Outdent();
        w.Line("}");

        w.Outdent();
        w.Line("}");
    }

    private static void GenerateConverterReadLogic(CodeWriter w, string dataType, string typeName)
    {
        switch (dataType)
        {
            case "string":
                w.Line($"var value = reader.GetString();");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "long":
                w.Line($"var value = reader.GetInt64();");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "ulong":
                w.Line($"var value = reader.GetUInt64();");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "decimal":
                w.Line($"var value = reader.GetDecimal();");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "bool":
                w.Line($"var value = reader.GetBoolean();");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "DateTime":
                w.Line($"var value = reader.GetDateTime();");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "DateTimeOffset":
                w.Line($"var value = reader.GetDateTimeOffset();");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "DateOnly":
                w.Line($"var value = DateOnly.Parse(reader.GetString() ?? throw new JsonException());");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "Guid":
                w.Line($"var value = reader.GetGuid();");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "Uri":
                w.Line($"var str = reader.GetString();");
                w.Line($"var value = str is not null ? new Uri(str, UriKind.RelativeOrAbsolute) : null;");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "byte[]":
                w.Line($"var value = reader.GetBytesFromBase64();");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            case "TimeSpan":
                w.Line($"var str = reader.GetString();");
                w.Line($"var value = str is not null ? System.Xml.XmlConvert.ToTimeSpan(str) : default(TimeSpan);");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
            default:
                w.Line($"var value = reader.GetString();");
                w.Line($"return new {typeName} {{ Value = value }};");
                break;
        }
    }

    private static void GenerateConverterWriteLogic(CodeWriter w, string dataType)
    {
        switch (dataType)
        {
            case "string":
                w.Line("writer.WriteStringValue(value.Value);");
                break;
            case "long":
                w.Line("writer.WriteNumberValue(value.Value.Value);");
                break;
            case "ulong":
                w.Line("writer.WriteNumberValue(value.Value.Value);");
                break;
            case "decimal":
                w.Line("writer.WriteNumberValue(value.Value.Value);");
                break;
            case "bool":
                w.Line("writer.WriteBooleanValue(value.Value.Value);");
                break;
            case "DateTime":
                w.Line("writer.WriteStringValue(value.Value.Value);");
                break;
            case "DateTimeOffset":
                w.Line("writer.WriteStringValue(value.Value.Value);");
                break;
            case "DateOnly":
                w.Line("writer.WriteStringValue(value.Value.Value.ToString(\"O\"));");
                break;
            case "Guid":
                w.Line("writer.WriteStringValue(value.Value.Value);");
                break;
            case "Uri":
                w.Line("writer.WriteStringValue(value.Value.ToString());");
                break;
            case "byte[]":
                w.Line("writer.WriteBase64StringValue(value.Value);");
                break;
            case "TimeSpan":
                w.Line("writer.WriteStringValue(System.Xml.XmlConvert.ToString(value.Value.Value));");
                break;
            default:
                w.Line("writer.WriteStringValue(value.Value);");
                break;
        }
    }

    private void GenerateModelProperties(CodeWriter w, ModelContainer model)
    {
        foreach (var element in model.Elements)
        {
            switch (element)
            {
                case FieldInstance fieldInstance:
                    GenerateFieldInstanceProperty(w, fieldInstance);
                    w.Line();
                    break;
                case AssemblyInstance assemblyInstance:
                    GenerateAssemblyInstanceProperty(w, assemblyInstance);
                    w.Line();
                    break;
                case ChoiceGroup choiceGroup:
                    foreach (var choice in choiceGroup.Choices)
                    {
                        if (choice is FieldInstance fi)
                        {
                            GenerateFieldInstanceProperty(w, fi);
                            w.Line();
                        }
                        else if (choice is AssemblyInstance ai)
                        {
                            GenerateAssemblyInstanceProperty(w, ai);
                            w.Line();
                        }
                    }
                    break;
            }
        }
    }

    private void GenerateFieldInstanceProperty(CodeWriter w, FieldInstance fieldInstance)
    {
        var propName = GetPropertyName(fieldInstance.GroupAs?.Name ?? fieldInstance.EffectiveName);
        var typeName = GetTypeName(fieldInstance.ResolvedDefinition?.Name ?? fieldInstance.Ref);
        var isCollection = fieldInstance.MaxOccurs is null || fieldInstance.MaxOccurs > 1;
        var jsonName = fieldInstance.GroupAs?.Name ?? fieldInstance.EffectiveName;

        if (_options.IncludeDocumentation)
        {
            var formalName = fieldInstance.FormalName ?? fieldInstance.ResolvedDefinition?.FormalName;
            var description = fieldInstance.Description?.ToString() ?? fieldInstance.ResolvedDefinition?.Description?.ToString();
            AppendXmlDoc(w, formalName ?? propName, description);
        }

        if (fieldInstance.DeprecatedVersion is not null)
        {
            w.Line($"[Obsolete(\"Deprecated since version {fieldInstance.DeprecatedVersion}\")]");
        }

        w.Line($"[JsonPropertyName(\"{jsonName}\")]");

        if (isCollection)
        {
            w.Line($"public IReadOnlyList<{typeName}> {propName} {{ get; init; }} = [];");
        }
        else
        {
            var nullable = fieldInstance.MinOccurs == 0 ? "?" : "";
            var required = fieldInstance.MinOccurs > 0 ? "required " : "";
            w.Line($"public {required}{typeName}{nullable} {propName} {{ get; init; }}");
        }
    }

    private void GenerateAssemblyInstanceProperty(CodeWriter w, AssemblyInstance assemblyInstance)
    {
        var propName = GetPropertyName(assemblyInstance.GroupAs?.Name ?? assemblyInstance.EffectiveName);
        var typeName = GetTypeName(assemblyInstance.ResolvedDefinition?.Name ?? assemblyInstance.Ref);
        var isCollection = assemblyInstance.MaxOccurs is null || assemblyInstance.MaxOccurs > 1;
        var jsonName = assemblyInstance.GroupAs?.Name ?? assemblyInstance.EffectiveName;

        if (_options.IncludeDocumentation)
        {
            var formalName = assemblyInstance.FormalName ?? assemblyInstance.ResolvedDefinition?.FormalName;
            var description = assemblyInstance.Description?.ToString() ?? assemblyInstance.ResolvedDefinition?.Description?.ToString();
            AppendXmlDoc(w, formalName ?? propName, description);
        }

        if (assemblyInstance.DeprecatedVersion is not null)
        {
            w.Line($"[Obsolete(\"Deprecated since version {assemblyInstance.DeprecatedVersion}\")]");
        }

        w.Line($"[JsonPropertyName(\"{jsonName}\")]");

        if (isCollection)
        {
            w.Line($"public IReadOnlyList<{typeName}> {propName} {{ get; init; }} = [];");
        }
        else
        {
            var nullable = assemblyInstance.MinOccurs == 0 ? "?" : "";
            var required = assemblyInstance.MinOccurs > 0 ? "required " : "";
            w.Line($"public {required}{typeName}{nullable} {propName} {{ get; init; }}");
        }
    }

    private string GenerateJsonContext(string contextName, MetaschemaModule module)
    {
        var w = new CodeWriter();
        AppendFileHeader(w);
        w.Line("using System.Text.Json;");
        w.Line("using System.Text.Json.Serialization;");
        w.Line();

        if (_options.FileScopedNamespaces)
        {
            w.Line($"namespace {_options.Namespace};");
            w.Line();
        }
        else
        {
            w.Line($"namespace {_options.Namespace}");
            w.Line("{");
            w.Indent();
        }

        if (_options.IncludeDocumentation)
        {
            w.Line("/// <summary>");
            w.Line($"/// JSON serialization context for {module.Name}.");
            w.Line("/// The System.Text.Json source generator will complete this partial class.");
            w.Line("/// </summary>");
        }

        w.Line("[JsonSourceGenerationOptions(");
        w.Indent();
        w.Line("WriteIndented = true,");
        w.Line("PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower,");
        w.Line("DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,");
        w.Line("GenerationMode = JsonSourceGenerationMode.Default)]");
        w.Outdent();

        // Add [JsonSerializable] attribute for each type
        foreach (var typeName in _allTypeNames)
        {
            w.Line($"[JsonSerializable(typeof({typeName}))]");
        }

        w.Line($"public partial class {contextName} : JsonSerializerContext");
        w.Line("{");
        w.Line("}");

        if (!_options.FileScopedNamespaces)
        {
            w.Outdent();
            w.Line("}");
        }

        return w.ToString();
    }

    private string GenerateExtensions(AssemblyDefinition rootAssembly)
    {
        var w = new CodeWriter();
        AppendFileHeader(w);
        w.Line("using System;");
        w.Line("using System.IO;");
        w.Line("using System.Text.Json;");
        w.Line();

        if (_options.FileScopedNamespaces)
        {
            w.Line($"namespace {_options.Namespace};");
            w.Line();
        }
        else
        {
            w.Line($"namespace {_options.Namespace}");
            w.Line("{");
            w.Indent();
        }

        var rootTypeName = GetTypeName(rootAssembly.Name);
        var contextName = _options.JsonContextName ?? $"{_options.Namespace.Split('.').Last()}JsonContext";

        if (_options.IncludeDocumentation)
        {
            w.Line("/// <summary>");
            w.Line($"/// Extension methods for working with {rootTypeName}.");
            w.Line("/// </summary>");
        }

        w.Line("public static class Extensions");
        w.Line("{");
        w.Indent();

        // LoadFromJson
        if (_options.IncludeDocumentation)
        {
            w.Line("/// <summary>");
            w.Line($"/// Loads a {rootTypeName} from a JSON file.");
            w.Line("/// </summary>");
            w.Line("/// <param name=\"filePath\">The path to the JSON file.</param>");
            w.Line($"/// <returns>The deserialized {rootTypeName}.</returns>");
        }
        w.Line($"public static {rootTypeName} LoadFromJson(string filePath)");
        w.Line("{");
        w.Indent();
        w.Line("var json = File.ReadAllText(filePath);");
        w.Line($"return JsonSerializer.Deserialize(json, {contextName}.Default.{rootTypeName})");
        w.Indent();
        w.Line($"?? throw new InvalidOperationException(\"Failed to deserialize {rootTypeName}\");");
        w.Outdent();
        w.Outdent();
        w.Line("}");
        w.Line();

        // SaveToJson
        if (_options.IncludeDocumentation)
        {
            w.Line("/// <summary>");
            w.Line($"/// Saves a {rootTypeName} to a JSON file.");
            w.Line("/// </summary>");
            w.Line($"/// <param name=\"{ToCamelCase(rootTypeName)}\">The {rootTypeName} to save.</param>");
            w.Line("/// <param name=\"filePath\">The path to the output JSON file.</param>");
        }
        w.Line($"public static void SaveToJson(this {rootTypeName} {ToCamelCase(rootTypeName)}, string filePath)");
        w.Line("{");
        w.Indent();
        w.Line($"var json = JsonSerializer.Serialize({ToCamelCase(rootTypeName)}, {contextName}.Default.{rootTypeName});");
        w.Line("File.WriteAllText(filePath, json);");
        w.Outdent();
        w.Line("}");

        w.Outdent();
        w.Line("}");

        if (!_options.FileScopedNamespaces)
        {
            w.Outdent();
            w.Line("}");
        }

        return w.ToString();
    }

    private void AppendFileHeader(CodeWriter w)
    {
        w.Line("// <auto-generated/>");
        w.Line("// This file was generated by the Metaschema code generator.");
        w.Line("// Do not modify this file directly.");
        w.Line();
        if (_options.NullableAnnotations)
        {
            w.Line("#nullable enable");
            w.Line();
        }
    }

    private static void AppendUsings(CodeWriter w, bool includeJson = false)
    {
        w.Line("using System;");
        w.Line("using System.Collections.Generic;");
        if (includeJson)
        {
            w.Line("using System.Text.Json;");
            w.Line("using System.Text.Json.Serialization;");
        }
    }

    private static void AppendXmlDoc(CodeWriter w, string? summary, string? description)
    {
        w.Line("/// <summary>");
        var text = NormalizeWhitespace(EscapeXmlDoc(summary ?? ""));
        if (!string.IsNullOrEmpty(description))
        {
            text += " - " + NormalizeWhitespace(EscapeXmlDoc(description));
        }
        w.Line("/// " + text);
        w.Line("/// </summary>");
    }

    private string GetTypeName(string name)
    {
        var typeName = ToPascalCase(name);
        if (_options.ClassPrefix is not null)
        {
            typeName = _options.ClassPrefix + typeName;
        }
        if (_options.ClassSuffix is not null)
        {
            typeName += _options.ClassSuffix;
        }
        return typeName;
    }

    private static string GetPropertyName(string name) => ToPascalCase(name);

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

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

    private static string ToCamelCase(string name)
    {
        var pascal = ToPascalCase(name);
        if (string.IsNullOrEmpty(pascal))
        {
            return pascal;
        }

        return char.ToLowerInvariant(pascal[0]) + pascal[1..];
    }

    private static string GetClrTypeName(string metaschemaTypeName) => metaschemaTypeName switch
    {
        "string" => "string",
        "token" => "string",
        "uri" => "Uri",
        "uri-reference" => "Uri",
        "uuid" => "Guid",
        "email-address" => "string",
        "hostname" => "string",
        "ncname" => "string",
        "integer" => "long",
        "non-negative-integer" => "ulong",
        "positive-integer" => "ulong",
        "decimal" => "decimal",
        "boolean" => "bool",
        "date" => "DateOnly",
        "date-time" => "DateTime",
        "date-with-timezone" => "DateTimeOffset",
        "date-time-with-timezone" => "DateTimeOffset",
        "day-time-duration" => "TimeSpan",
        "year-month-duration" => "TimeSpan",
        "base64" => "byte[]",
        "ip-v4-address" => "string",
        "ip-v6-address" => "string",
        "markup-line" => "string",
        "markup-multiline" => "string",
        _ => "string"
    };

    private static string EscapeXmlDoc(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }
        // Replace newlines and multiple spaces with a single space
        return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
    }

    /// <summary>
    /// Helper class for generating code with proper indentation.
    /// </summary>
    private sealed class CodeWriter
    {
        private readonly StringBuilder _sb = new();
        private int _indent;
        private const string IndentString = "    ";

        public void Indent() => _indent++;
        public void Outdent() => _indent = Math.Max(0, _indent - 1);

        public void Line() => _sb.AppendLine();

        public void Line(string text)
        {
            for (var i = 0; i < _indent; i++)
            {
                _sb.Append(IndentString);
            }
            _sb.AppendLine(text);
        }

        public override string ToString() => _sb.ToString();
    }
}
