// Licensed under the MIT License.
// Demonstrates the new record-based code generation with STJ support.

using System.Text;
using Metaschema.Core.Model;
using Metaschema.Databind.CodeGeneration;

// Create a simple test metaschema module programmatically
var module = CreateTestModule();

// Generate with the new RecordCodeGenerator
var generator = new RecordCodeGenerator(new CodeGenerationOptions
{
    Namespace = "Example.Generated",
    UseRecords = true,
    GenerateJsonContext = true,
    GenerateExtensionMethods = true,
    IncludeDocumentation = true,
    FilePerType = true
});

var files = generator.Generate(module);

Console.WriteLine($"Generated {files.Count} files:");
Console.WriteLine();

foreach (var (fileName, content) in files.OrderBy(f => f.Key))
{
    Console.WriteLine($"=== {fileName} ===");
    Console.WriteLine(TruncateContent(content, 40));
    Console.WriteLine();
}

static MetaschemaModule CreateTestModule()
{
    // This is a placeholder - in real usage, this would come from ModuleLoader
    return new MetaschemaModule
    {
        Name = "test-module",
        ShortName = "test",
        Version = "1.0",
        XmlNamespace = new Uri("http://example.com/ns/test"),
        JsonBaseUri = new Uri("http://example.com/ns/test")
    };
}

static string TruncateContent(string content, int maxLines)
{
    var lines = content.Split('\n');
    if (lines.Length <= maxLines)
    {
        return content;
    }
    
    var sb = new StringBuilder();
    for (int i = 0; i < maxLines; i++)
    {
        sb.AppendLine(lines[i]);
    }
    sb.AppendLine($"... ({lines.Length - maxLines} more lines)");
    return sb.ToString();
}
