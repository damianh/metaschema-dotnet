// Licensed under the MIT License.

using Metaschema.Core.Loading;
using Metaschema.Databind;
using Metaschema.Databind.Nodes;

namespace Oscal.Sample.Dynamic.Examples;

/// <summary>
/// Demonstrates loading and examining an OSCAL System Security Plan (SSP).
/// 
/// An SSP documents how a system implements security controls. It's the most
/// complex OSCAL model, containing system characteristics, control implementations,
/// and authorization information.
/// </summary>
public static class LoadSspExample
{
    public static void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Example 3: Load OSCAL System Security Plan (SSP)            ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Step 1: Load the OSCAL SSP Metaschema
        Console.WriteLine("Step 1: Loading OSCAL SSP Metaschema...");
        var loader = new ModuleLoader();
        var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", "oscal_ssp_metaschema.xml");
        var module = loader.Load(metaschemaPath);
        Console.WriteLine($"  Loaded: {module.Name} (version {module.Version})");
        Console.WriteLine($"  This module imports: {module.ImportedModules.Count} other modules");
        Console.WriteLine();

        // Step 2: Load the SSP example
        Console.WriteLine("Step 2: Loading SSP Example Document...");
        var context = new BindingContext();
        context.RegisterModule(module);

        var sspPath = Path.Combine(AppContext.BaseDirectory, "Content", "ssp", "ssp-example.json");
        var deserializer = context.GetDeserializer(Format.Json);
        var document = deserializer.Deserialize(File.ReadAllText(sspPath));
        Console.WriteLine($"  Loaded SSP document");
        Console.WriteLine();

        var ssp = document.RootAssembly;
        if (ssp is null)
        {
            Console.WriteLine("  Error: No root assembly found");
            return;
        }

        // Step 3: Examine SSP metadata
        Console.WriteLine("Step 3: SSP Metadata...");
        if (ssp.Flags.TryGetValue("uuid", out var uuidFlag))
        {
            Console.WriteLine($"  SSP UUID: {uuidFlag.RawValue}");
        }

        var metadata = ssp.ModelChildren.FirstOrDefault(c => c.Name == "metadata") as AssemblyNode;
        if (metadata is not null)
        {
            var title = metadata.ModelChildren.FirstOrDefault(c => c.Name == "title") as FieldNode;
            Console.WriteLine($"  Title: {title?.RawValue}");

            var version = metadata.ModelChildren.FirstOrDefault(c => c.Name == "version") as FieldNode;
            Console.WriteLine($"  Version: {version?.RawValue}");

            // Count roles and parties
            var roles = metadata.ModelChildren.Count(c => c.Name == "role");
            var parties = metadata.ModelChildren.Count(c => c.Name == "party");
            Console.WriteLine($"  Roles defined: {roles}");
            Console.WriteLine($"  Parties defined: {parties}");
        }
        Console.WriteLine();

        // Step 4: Examine import-profile (the baseline this SSP is based on)
        Console.WriteLine("Step 4: Imported Profile (Baseline)...");
        var importProfile = ssp.ModelChildren.FirstOrDefault(c => c.Name == "import-profile") as AssemblyNode;
        if (importProfile is not null)
        {
            if (importProfile.Flags.TryGetValue("href", out var hrefFlag))
            {
                Console.WriteLine($"  Profile reference: {hrefFlag.RawValue}");
            }
        }
        Console.WriteLine();

        // Step 5: Examine system-characteristics
        Console.WriteLine("Step 5: System Characteristics...");
        var sysChars = ssp.ModelChildren.FirstOrDefault(c => c.Name == "system-characteristics") as AssemblyNode;
        if (sysChars is not null)
        {
            var systemName = sysChars.ModelChildren.FirstOrDefault(c => c.Name == "system-name") as FieldNode;
            Console.WriteLine($"  System Name: {systemName?.RawValue}");

            var description = sysChars.ModelChildren.FirstOrDefault(c => c.Name == "description") as FieldNode;
            if (description?.RawValue is not null)
            {
                var desc = description.RawValue.Length > 100
                    ? description.RawValue[..100] + "..."
                    : description.RawValue;
                Console.WriteLine($"  Description: {desc}");
            }

            // System IDs
            var systemIds = sysChars.ModelChildren.Where(c => c.Name == "system-id").ToList();
            foreach (var sysId in systemIds.Cast<FieldNode>())
            {
                var scheme = sysId.Flags.TryGetValue("identifier-type", out var schemeFlag)
                    ? schemeFlag.RawValue
                    : "unknown";
                Console.WriteLine($"  System ID ({scheme}): {sysId.RawValue}");
            }

            // Security sensitivity level
            var secSensitivity = sysChars.ModelChildren
                .FirstOrDefault(c => c.Name == "security-sensitivity-level") as FieldNode;
            if (secSensitivity is not null)
            {
                Console.WriteLine($"  Security Sensitivity Level: {secSensitivity.RawValue}");
            }

            // Authorization boundary
            var authBoundary = sysChars.ModelChildren.FirstOrDefault(c => c.Name == "authorization-boundary");
            if (authBoundary is not null)
            {
                Console.WriteLine($"  Authorization boundary: defined");
            }
        }
        Console.WriteLine();

        // Step 6: Examine system-implementation
        Console.WriteLine("Step 6: System Implementation...");
        var sysImpl = ssp.ModelChildren.FirstOrDefault(c => c.Name == "system-implementation") as AssemblyNode;
        if (sysImpl is not null)
        {
            var users = sysImpl.ModelChildren.Count(c => c.Name == "user");
            var components = sysImpl.ModelChildren.Count(c => c.Name == "component");
            var inventoryItems = sysImpl.ModelChildren.Count(c => c.Name == "inventory-item");

            Console.WriteLine($"  Users: {users}");
            Console.WriteLine($"  Components: {components}");
            Console.WriteLine($"  Inventory Items: {inventoryItems}");

            // List component types
            if (components > 0)
            {
                Console.WriteLine();
                Console.WriteLine("  Components:");
                foreach (var component in sysImpl.ModelChildren.Where(c => c.Name == "component").Take(5).Cast<AssemblyNode>())
                {
                    var compType = component.Flags.TryGetValue("type", out var typeFlag) ? typeFlag.RawValue : "unknown";
                    var compTitle = component.ModelChildren.FirstOrDefault(c => c.Name == "title") as FieldNode;
                    Console.WriteLine($"    - [{compType}] {compTitle?.RawValue}");
                }

                if (components > 5)
                {
                    Console.WriteLine($"    ... and {components - 5} more");
                }
            }
        }
        Console.WriteLine();

        // Step 7: Examine control-implementation
        Console.WriteLine("Step 7: Control Implementation...");
        var controlImpl = ssp.ModelChildren.FirstOrDefault(c => c.Name == "control-implementation") as AssemblyNode;
        if (controlImpl is not null)
        {
            var implementedReqs = controlImpl.ModelChildren
                .Where(c => c.Name == "implemented-requirement")
                .ToList();

            Console.WriteLine($"  Implemented requirements: {implementedReqs.Count}");

            // Show sample implementations
            Console.WriteLine();
            Console.WriteLine("  Sample Control Implementations:");
            foreach (var impl in implementedReqs.Take(3).Cast<AssemblyNode>())
            {
                var controlId = impl.Flags.TryGetValue("control-id", out var cidFlag) ? cidFlag.RawValue : "?";
                var stmtCount = impl.ModelChildren.Count(c => c.Name == "statement");

                Console.WriteLine($"    - {controlId}: {stmtCount} statement(s)");
            }

            if (implementedReqs.Count > 3)
            {
                Console.WriteLine($"    ... and {implementedReqs.Count - 3} more");
            }
        }

        Console.WriteLine();
        Console.WriteLine("SSP loading example complete!");
    }
}
