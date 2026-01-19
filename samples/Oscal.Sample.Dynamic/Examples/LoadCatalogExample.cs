// Licensed under the MIT License.

using Metaschema.Core.Loading;
using Metaschema.Databind;
using Metaschema.Databind.Nodes;

namespace Oscal.Sample.Dynamic.Examples;

/// <summary>
/// Demonstrates loading and navigating an OSCAL Catalog document.
/// 
/// A Catalog contains security controls organized in groups. The NIST SP 800-53
/// catalog is the most commonly used catalog in the OSCAL ecosystem.
/// </summary>
public static class LoadCatalogExample
{
    public static void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Example 1: Load OSCAL Catalog                               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Step 1: Load the OSCAL Catalog Metaschema
        Console.WriteLine("Step 1: Loading OSCAL Catalog Metaschema...");
        var loader = new ModuleLoader();
        var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", "oscal_catalog_metaschema.xml");
        var module = loader.Load(metaschemaPath);
        Console.WriteLine($"  Loaded: {module.Name} (version {module.Version})");
        Console.WriteLine($"  Namespace: {module.XmlNamespace}");
        Console.WriteLine($"  Assembly definitions: {module.AssemblyDefinitions.Count()}");
        Console.WriteLine($"  Field definitions: {module.FieldDefinitions.Count()}");
        Console.WriteLine();

        // Step 2: Create a binding context and load the catalog JSON
        Console.WriteLine("Step 2: Loading NIST SP 800-53 Rev 5 Catalog...");
        var context = new BindingContext();
        context.RegisterModule(module);

        var catalogPath = Path.Combine(AppContext.BaseDirectory, "Content", "catalog", "NIST_SP-800-53_rev5_catalog.json");
        var catalogJson = File.ReadAllText(catalogPath);

        var deserializer = context.GetDeserializer(Format.Json);
        var document = deserializer.Deserialize(catalogJson);
        Console.WriteLine($"  Loaded catalog document");
        Console.WriteLine();

        // Step 3: Navigate the catalog structure
        Console.WriteLine("Step 3: Exploring Catalog Structure...");
        var catalog = document.RootAssembly;
        if (catalog is null)
        {
            Console.WriteLine("  Error: No root assembly found");
            return;
        }

        Console.WriteLine($"  Root element: {catalog.Name}");

        // Get catalog UUID
        if (catalog.Flags.TryGetValue("uuid", out var uuidFlag))
        {
            Console.WriteLine($"  Catalog UUID: {uuidFlag.RawValue}");
        }

        Console.WriteLine();

        // Step 4: Examine metadata
        Console.WriteLine("Step 4: Catalog Metadata...");
        var metadata = catalog.ModelChildren.FirstOrDefault(c => c.Name == "metadata");
        if (metadata is AssemblyNode metadataAssembly)
        {
            var title = metadataAssembly.ModelChildren.FirstOrDefault(c => c.Name == "title");
            if (title is FieldNode titleField)
            {
                Console.WriteLine($"  Title: {titleField.RawValue}");
            }

            var version = metadataAssembly.ModelChildren.FirstOrDefault(c => c.Name == "version");
            if (version is FieldNode versionField)
            {
                Console.WriteLine($"  Version: {versionField.RawValue}");
            }

            var lastModified = metadataAssembly.ModelChildren.FirstOrDefault(c => c.Name == "last-modified");
            if (lastModified is FieldNode lastModifiedField)
            {
                Console.WriteLine($"  Last Modified: {lastModifiedField.RawValue}");
            }

            var oscalVersion = metadataAssembly.ModelChildren.FirstOrDefault(c => c.Name == "oscal-version");
            if (oscalVersion is FieldNode oscalVersionField)
            {
                Console.WriteLine($"  OSCAL Version: {oscalVersionField.RawValue}");
            }
        }
        Console.WriteLine();

        // Step 5: Count and list control families (groups)
        Console.WriteLine("Step 5: Control Families (Groups)...");
        var groups = catalog.ModelChildren.Where(c => c.Name == "group").ToList();
        Console.WriteLine($"  Total control families: {groups.Count}");
        Console.WriteLine();
        Console.WriteLine("  Control Families:");

        foreach (var group in groups.Take(5).Cast<AssemblyNode>())
        {
            var groupId = group.Flags.TryGetValue("id", out var idFlag) ? idFlag.RawValue : "unknown";
            var groupTitle = group.ModelChildren.FirstOrDefault(c => c.Name == "title") as FieldNode;
            var controlCount = group.ModelChildren.Count(c => c.Name == "control");

            Console.WriteLine($"    [{groupId}] {groupTitle?.RawValue} ({controlCount} controls)");
        }

        if (groups.Count > 5)
        {
            Console.WriteLine($"    ... and {groups.Count - 5} more families");
        }
        Console.WriteLine();

        // Step 6: Count total controls
        Console.WriteLine("Step 6: Control Statistics...");
        var totalControls = CountControls(catalog);
        Console.WriteLine($"  Total controls (including enhancements): {totalControls}");

        // Show a sample control
        Console.WriteLine();
        Console.WriteLine("Step 7: Sample Control (AC-1)...");
        var ac1 = FindControl(catalog, "ac-1");
        if (ac1 is not null)
        {
            PrintControl(ac1, indent: 2);
        }

        Console.WriteLine();
        Console.WriteLine("Catalog loading example complete!");
    }

    private static int CountControls(AssemblyNode parent)
    {
        var count = 0;

        foreach (var child in parent.ModelChildren)
        {
            if (child is AssemblyNode assembly)
            {
                if (child.Name == "control")
                {
                    count++;
                    count += CountControls(assembly);
                }
                else if (child.Name == "group")
                {
                    count += CountControls(assembly);
                }
            }
        }

        return count;
    }

    private static AssemblyNode? FindControl(AssemblyNode parent, string controlId)
    {
        foreach (var child in parent.ModelChildren)
        {
            if (child is AssemblyNode assembly)
            {
                if (child.Name == "control")
                {
                    if (assembly.Flags.TryGetValue("id", out var idFlag) &&
                        string.Equals(idFlag.RawValue, controlId, StringComparison.OrdinalIgnoreCase))
                    {
                        return assembly;
                    }

                    var nested = FindControl(assembly, controlId);
                    if (nested is not null) return nested;
                }
                else if (child.Name == "group")
                {
                    var nested = FindControl(assembly, controlId);
                    if (nested is not null) return nested;
                }
            }
        }

        return null;
    }

    private static void PrintControl(AssemblyNode control, int indent)
    {
        var prefix = new string(' ', indent);

        var id = control.Flags.TryGetValue("id", out var idFlag) ? idFlag.RawValue : "unknown";
        var classAttr = control.Flags.TryGetValue("class", out var classFlag) ? classFlag.RawValue : null;

        Console.WriteLine($"{prefix}Control ID: {id}");
        if (classAttr is not null)
        {
            Console.WriteLine($"{prefix}Class: {classAttr}");
        }

        var title = control.ModelChildren.FirstOrDefault(c => c.Name == "title") as FieldNode;
        if (title is not null)
        {
            Console.WriteLine($"{prefix}Title: {title.RawValue}");
        }

        // Count parts and enhancements
        var parts = control.ModelChildren.Count(c => c.Name == "part");
        var enhancements = control.ModelChildren.Count(c => c.Name == "control");
        var params_ = control.ModelChildren.Count(c => c.Name == "param");

        Console.WriteLine($"{prefix}Parts: {parts}");
        Console.WriteLine($"{prefix}Parameters: {params_}");
        Console.WriteLine($"{prefix}Control Enhancements: {enhancements}");
    }
}
