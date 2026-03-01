// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using System.Globalization;
using System.Text;
using Metaschema.Model;

namespace Metaschema.CodeGeneration;

/// <summary>
/// Generates C# source code from Metaschema modules.
/// </summary>
public sealed class CSharpCodeGenerator
{
    private readonly CodeGenerationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpCodeGenerator"/> class.
    /// </summary>
    /// <param name="options">The code generation options.</param>
    public CSharpCodeGenerator(CodeGenerationOptions? options = null) => _options = options ?? new CodeGenerationOptions();

    /// <summary>
    /// Generates C# source code for all definitions in a module.
    /// </summary>
    /// <param name="module">The Metaschema module.</param>
    /// <returns>A dictionary of file names to source code.</returns>
    public Dictionary<string, string> Generate(MetaschemaModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        var files = new Dictionary<string, string>();

        if (_options.FilePerType)
        {
            foreach (var flag in module.FlagDefinitions.Where(f => f.Scope == Scope.Global))
            {
                var source = GenerateFlagClass(flag);
                files[GetClassName(flag.Name) + ".g.cs"] = source;
            }

            foreach (var field in module.FieldDefinitions.Where(f => f.Scope == Scope.Global))
            {
                var source = GenerateFieldClass(field);
                files[GetClassName(field.Name) + ".g.cs"] = source;
            }

            foreach (var assembly in module.AssemblyDefinitions.Where(a => a.Scope == Scope.Global))
            {
                var source = GenerateAssemblyClass(assembly);
                files[GetClassName(assembly.Name) + ".g.cs"] = source;
            }
        }
        else
        {
            var source = GenerateAllTypes(module);
            var fileName = SanitizeFileName(module.ShortName ?? module.Name) + ".g.cs";
            files[fileName] = source;
        }

        return files;
    }

    private string GenerateAllTypes(MetaschemaModule module)
    {
        var w = new CodeWriter();

        AppendFileHeader(w);
        AppendUsings(w);
        w.Line();

        if (_options.FileScopedNamespaces)
        {
            w.Line("namespace " + _options.Namespace + ";");
            w.Line();

            foreach (var flag in module.FlagDefinitions.Where(f => f.Scope == Scope.Global))
            {
                GenerateFlagTypeContent(w, flag);
                w.Line();
            }

            foreach (var field in module.FieldDefinitions.Where(f => f.Scope == Scope.Global))
            {
                GenerateFieldTypeContent(w, field);
                w.Line();
            }

            foreach (var assembly in module.AssemblyDefinitions.Where(a => a.Scope == Scope.Global))
            {
                GenerateAssemblyTypeContent(w, assembly);
                w.Line();
            }
        }
        else
        {
            w.Line("namespace " + _options.Namespace);
            w.Line("{");
            w.Indent();

            foreach (var flag in module.FlagDefinitions.Where(f => f.Scope == Scope.Global))
            {
                GenerateFlagTypeContent(w, flag);
                w.Line();
            }

            foreach (var field in module.FieldDefinitions.Where(f => f.Scope == Scope.Global))
            {
                GenerateFieldTypeContent(w, field);
                w.Line();
            }

            foreach (var assembly in module.AssemblyDefinitions.Where(a => a.Scope == Scope.Global))
            {
                GenerateAssemblyTypeContent(w, assembly);
                w.Line();
            }

            w.Outdent();
            w.Line("}");
        }

        return w.ToString();
    }

    private string GenerateFlagClass(FlagDefinition flag)
    {
        var w = new CodeWriter();

        AppendFileHeader(w);
        AppendUsings(w);
        w.Line();

        if (_options.FileScopedNamespaces)
        {
            w.Line("namespace " + _options.Namespace + ";");
            w.Line();
            GenerateFlagTypeContent(w, flag);
        }
        else
        {
            w.Line("namespace " + _options.Namespace);
            w.Line("{");
            w.Indent();
            GenerateFlagTypeContent(w, flag);
            w.Outdent();
            w.Line("}");
        }

        return w.ToString();
    }

    private string GenerateFieldClass(FieldDefinition field)
    {
        var w = new CodeWriter();

        AppendFileHeader(w);
        AppendUsings(w);
        w.Line();

        if (_options.FileScopedNamespaces)
        {
            w.Line("namespace " + _options.Namespace + ";");
            w.Line();
            GenerateFieldTypeContent(w, field);
        }
        else
        {
            w.Line("namespace " + _options.Namespace);
            w.Line("{");
            w.Indent();
            GenerateFieldTypeContent(w, field);
            w.Outdent();
            w.Line("}");
        }

        return w.ToString();
    }

    private string GenerateAssemblyClass(AssemblyDefinition assembly)
    {
        var w = new CodeWriter();

        AppendFileHeader(w);
        AppendUsings(w);
        w.Line();

        if (_options.FileScopedNamespaces)
        {
            w.Line("namespace " + _options.Namespace + ";");
            w.Line();
            GenerateAssemblyTypeContent(w, assembly);
        }
        else
        {
            w.Line("namespace " + _options.Namespace);
            w.Line("{");
            w.Indent();
            GenerateAssemblyTypeContent(w, assembly);
            w.Outdent();
            w.Line("}");
        }

        return w.ToString();
    }

    private void GenerateFlagTypeContent(CodeWriter w, FlagDefinition flag)
    {
        var visibility = _options.Visibility == TypeVisibility.Public ? "public" : "internal";
        var className = GetClassName(flag.Name);
        var dataType = GetClrTypeName(flag.DataTypeName);

        if (_options.IncludeDocumentation)
        {
            AppendXmlDoc(w, flag.FormalName ?? flag.Name, flag.Description?.ToString());
        }

        if (flag.DeprecatedVersion is not null)
        {
            w.Line("[Obsolete(\"Deprecated since version " + flag.DeprecatedVersion + "\")]");
        }

        w.Line("[MetaschemaField(Name = \"" + flag.Name + "\", DataType = \"" + flag.DataTypeName + "\")]");
        w.Line(visibility + " readonly partial struct " + className + " : IEquatable<" + className + ">");
        w.Line("{");
        w.Indent();

        if (_options.IncludeDocumentation)
        {
            w.Line("/// <summary>");
            w.Line("/// Gets the underlying value.");
            w.Line("/// </summary>");
        }
        w.Line("public " + dataType + " Value { get; }");
        w.Line();

        if (_options.IncludeDocumentation)
        {
            w.Line("/// <summary>");
            w.Line("/// Initializes a new instance of the <see cref=\"" + className + "\"/> struct.");
            w.Line("/// </summary>");
            w.Line("/// <param name=\"value\">The value.</param>");
        }
        w.Line("public " + className + "(" + dataType + " value) => Value = value;");
        w.Line();

        w.Line("public static implicit operator " + className + "(" + dataType + " value) => new(value);");
        w.Line();
        w.Line("public static implicit operator " + dataType + "(" + className + " flag) => flag.Value;");
        w.Line();
        w.Line("public bool Equals(" + className + " other) => Value.Equals(other.Value);");
        w.Line();
        w.Line("public override bool Equals(object? obj) => obj is " + className + " other && Equals(other);");
        w.Line();
        w.Line("public override int GetHashCode() => Value.GetHashCode();");
        w.Line();
        w.Line("public override string ToString() => Value.ToString();");
        w.Line();
        w.Line("public static bool operator ==(" + className + " left, " + className + " right) => left.Equals(right);");
        w.Line();
        w.Line("public static bool operator !=(" + className + " left, " + className + " right) => !left.Equals(right);");

        w.Outdent();
        w.Line("}");
    }

    private void GenerateFieldTypeContent(CodeWriter w, FieldDefinition field)
    {
        var visibility = _options.Visibility == TypeVisibility.Public ? "public" : "internal";
        var className = GetClassName(field.Name);
        var dataType = GetClrTypeName(field.DataTypeName);
        var ns = field.ContainingModule.XmlNamespace.ToString();

        if (_options.IncludeDocumentation)
        {
            AppendXmlDoc(w, field.FormalName ?? field.Name, field.Description?.ToString());
        }

        if (field.DeprecatedVersion is not null)
        {
            w.Line("[Obsolete(\"Deprecated since version " + field.DeprecatedVersion + "\")]");
        }

        w.Line("[MetaschemaField(Name = \"" + field.Name + "\", DataType = \"" + field.DataTypeName + "\")]");
        w.Line("[XmlNamespace(\"" + ns + "\")]");
        if (field.JsonValueKeyName is not null)
        {
            w.Line("[JsonFieldValueKey(\"" + field.JsonValueKeyName + "\")]");
        }

        w.Line(visibility + " partial class " + className);
        w.Line("{");
        w.Indent();

        if (_options.IncludeDocumentation)
        {
            w.Line("/// <summary>");
            w.Line("/// Gets or sets the field value.");
            w.Line("/// </summary>");
        }
        w.Line("public " + dataType + GetNullableSuffix(true) + " Value { get; set; }");

        foreach (var flagInstance in field.FlagInstances)
        {
            w.Line();
            GenerateFlagProperty(w, flagInstance);
        }

        w.Outdent();
        w.Line("}");
    }

    private void GenerateAssemblyTypeContent(CodeWriter w, AssemblyDefinition assembly)
    {
        var visibility = _options.Visibility == TypeVisibility.Public ? "public" : "internal";
        var className = GetClassName(assembly.Name);
        var ns = assembly.ContainingModule.XmlNamespace.ToString();

        if (_options.IncludeDocumentation)
        {
            AppendXmlDoc(w, assembly.FormalName ?? assembly.Name, assembly.Description?.ToString());
        }

        if (assembly.DeprecatedVersion is not null)
        {
            w.Line("[Obsolete(\"Deprecated since version " + assembly.DeprecatedVersion + "\")]");
        }

        var rootAttr = assembly.RootName is not null ? ", RootName = \"" + assembly.RootName + "\"" : "";
        w.Line("[MetaschemaAssembly(Name = \"" + assembly.Name + "\"" + rootAttr + ")]");
        w.Line("[XmlNamespace(\"" + ns + "\")]");

        w.Line(visibility + " partial class " + className);
        w.Line("{");
        w.Indent();

        foreach (var flagInstance in assembly.FlagInstances)
        {
            GenerateFlagProperty(w, flagInstance);
            w.Line();
        }

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

        if (_options.IncludeDocumentation)
        {
            var formalName = flagInstance.FormalName ?? flagInstance.ResolvedDefinition?.FormalName;
            var description = flagInstance.Description?.ToString() ?? flagInstance.ResolvedDefinition?.Description?.ToString();
            AppendXmlDoc(w, formalName ?? propName, description);
        }

        if (flagInstance.DeprecatedVersion is not null)
        {
            w.Line("[Obsolete(\"Deprecated since version " + flagInstance.DeprecatedVersion + "\")]");
        }

        var requiredStr = isRequired ? "true" : "false";
        w.Line("[BoundFlag(\"" + flagInstance.EffectiveName + "\", IsRequired = " + requiredStr + ", DataType = \"" + dataType + "\")]");

        var nullable = !isRequired ? GetNullableSuffix(true) : "";
        w.Line("public " + clrType + nullable + " " + propName + " { get; set; }");
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
        var typeName = GetClassName(fieldInstance.ResolvedDefinition?.Name ?? fieldInstance.Ref);
        var isCollection = fieldInstance.MaxOccurs is null || fieldInstance.MaxOccurs > 1;

        if (_options.IncludeDocumentation)
        {
            var formalName = fieldInstance.FormalName ?? fieldInstance.ResolvedDefinition?.FormalName;
            var description = fieldInstance.Description?.ToString() ?? fieldInstance.ResolvedDefinition?.Description?.ToString();
            AppendXmlDoc(w, formalName ?? propName, description);
        }

        if (fieldInstance.DeprecatedVersion is not null)
        {
            w.Line("[Obsolete(\"Deprecated since version " + fieldInstance.DeprecatedVersion + "\")]");
        }

        var minOccurs = fieldInstance.MinOccurs.ToString(CultureInfo.InvariantCulture);
        var maxOccurs = (fieldInstance.MaxOccurs ?? -1).ToString(CultureInfo.InvariantCulture);
        w.Line("[BoundField(\"" + fieldInstance.EffectiveName + "\", MinOccurs = " + minOccurs + ", MaxOccurs = " + maxOccurs + ")]");

        if (fieldInstance.GroupAs is not null)
        {
            var jsonBehavior = fieldInstance.GroupAs.InJson switch
            {
                JsonGrouping.Array => "JsonBehavior.List",
                JsonGrouping.SingletonOrArray => "JsonBehavior.SingletonOrList",
                JsonGrouping.ByKey => "JsonBehavior.ByKey",
                _ => "JsonBehavior.List"
            };
            w.Line("[GroupAs(\"" + fieldInstance.GroupAs.Name + "\", JsonBehavior = " + jsonBehavior + ")]");
        }

        if (isCollection)
        {
            w.Line("public List<" + typeName + "> " + propName + " { get; set; } = [];");
        }
        else
        {
            var nullable = fieldInstance.MinOccurs == 0 ? GetNullableSuffix(true) : "";
            w.Line("public " + typeName + nullable + " " + propName + " { get; set; }");
        }
    }

    private void GenerateAssemblyInstanceProperty(CodeWriter w, AssemblyInstance assemblyInstance)
    {
        var propName = GetPropertyName(assemblyInstance.GroupAs?.Name ?? assemblyInstance.EffectiveName);
        var typeName = GetClassName(assemblyInstance.ResolvedDefinition?.Name ?? assemblyInstance.Ref);
        var isCollection = assemblyInstance.MaxOccurs is null || assemblyInstance.MaxOccurs > 1;

        if (_options.IncludeDocumentation)
        {
            var formalName = assemblyInstance.FormalName ?? assemblyInstance.ResolvedDefinition?.FormalName;
            var description = assemblyInstance.Description?.ToString() ?? assemblyInstance.ResolvedDefinition?.Description?.ToString();
            AppendXmlDoc(w, formalName ?? propName, description);
        }

        if (assemblyInstance.DeprecatedVersion is not null)
        {
            w.Line("[Obsolete(\"Deprecated since version " + assemblyInstance.DeprecatedVersion + "\")]");
        }

        var minOccurs = assemblyInstance.MinOccurs.ToString(CultureInfo.InvariantCulture);
        var maxOccurs = (assemblyInstance.MaxOccurs ?? -1).ToString(CultureInfo.InvariantCulture);
        w.Line("[BoundAssembly(\"" + assemblyInstance.EffectiveName + "\", MinOccurs = " + minOccurs + ", MaxOccurs = " + maxOccurs + ")]");

        if (assemblyInstance.GroupAs is not null)
        {
            var jsonBehavior = assemblyInstance.GroupAs.InJson switch
            {
                JsonGrouping.Array => "JsonBehavior.List",
                JsonGrouping.SingletonOrArray => "JsonBehavior.SingletonOrList",
                JsonGrouping.ByKey => "JsonBehavior.ByKey",
                _ => "JsonBehavior.List"
            };
            w.Line("[GroupAs(\"" + assemblyInstance.GroupAs.Name + "\", JsonBehavior = " + jsonBehavior + ")]");
        }

        if (isCollection)
        {
            w.Line("public List<" + typeName + "> " + propName + " { get; set; } = [];");
        }
        else
        {
            var nullable = assemblyInstance.MinOccurs == 0 ? GetNullableSuffix(true) : "";
            w.Line("public " + typeName + nullable + " " + propName + " { get; set; }");
        }
    }

    private void AppendFileHeader(CodeWriter w)
    {
        w.Line("// <auto-generated/>");
        w.Line("// This file was generated by Metaschema.Databind code generator.");
        w.Line("// Do not modify this file directly.");
        w.Line();
        if (_options.NullableAnnotations)
        {
            w.Line("#nullable enable");
            w.Line();
        }
    }

    private static void AppendUsings(CodeWriter w)
    {
        w.Line("using System;");
        w.Line("using System.Collections.Generic;");
        w.Line("using Metaschema.Binding.Attributes;");
    }

    private static void AppendXmlDoc(CodeWriter w, string? summary, string? description)
    {
        w.Line("/// <summary>");
        var text = EscapeXmlDoc(summary ?? "") + (description is not null ? " - " + EscapeXmlDoc(description) : "");
        w.Line("/// " + text);
        w.Line("/// </summary>");
    }

    private string GetClassName(string name)
    {
        var className = ToPascalCase(name);
        if (_options.ClassPrefix is not null)
        {
            className = _options.ClassPrefix + className;
        }
        if (_options.ClassSuffix is not null)
        {
            className += _options.ClassSuffix;
        }
        return className;
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

    private string GetNullableSuffix(bool isNullable) =>
        _options.NullableAnnotations && isNullable ? "?" : "";

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
