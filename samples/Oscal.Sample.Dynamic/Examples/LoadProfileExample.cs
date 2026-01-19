// Licensed under the MIT License.

using Metaschema.Core.Loading;
using Metaschema.Databind;
using Metaschema.Databind.Nodes;

namespace Oscal.Sample.Dynamic.Examples;

/// <summary>
/// Demonstrates loading and examining an OSCAL Profile document.
/// 
/// A Profile defines a baseline by selecting and tailoring controls from one
/// or more catalogs. The NIST 800-53 baselines (LOW, MODERATE, HIGH) are
/// common examples.
/// </summary>
public static class LoadProfileExample
{
    public static void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Example 2: Load OSCAL Profile                               ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Step 1: Load the OSCAL Profile Metaschema
        Console.WriteLine("Step 1: Loading OSCAL Profile Metaschema...");
        var loader = new ModuleLoader();
        var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", "oscal_profile_metaschema.xml");
        var module = loader.Load(metaschemaPath);
        Console.WriteLine($"  Loaded: {module.Name} (version {module.Version})");
        Console.WriteLine();

        // Step 2: Create binding context and load all three baselines
        Console.WriteLine("Step 2: Loading NIST 800-53 Baseline Profiles...");
        var context = new BindingContext();
        context.RegisterModule(module);

        var baselines = new[] { "LOW", "MODERATE", "HIGH" };
        var profiles = new Dictionary<string, DocumentNode>();

        foreach (var baseline in baselines)
        {
            var profilePath = Path.Combine(
                AppContext.BaseDirectory,
                "Content",
                "profile",
                $"NIST_SP-800-53_rev5_{baseline}-baseline_profile.json");

            if (File.Exists(profilePath))
            {
                var deserializer = context.GetDeserializer(Format.Json);
                var doc = deserializer.Deserialize(File.ReadAllText(profilePath));
                profiles[baseline] = doc;
                Console.WriteLine($"  Loaded {baseline} baseline profile");
            }
        }
        Console.WriteLine();

        // Step 3: Examine the MODERATE baseline in detail
        Console.WriteLine("Step 3: Examining MODERATE Baseline Profile...");
        if (!profiles.TryGetValue("MODERATE", out var moderateDoc) || moderateDoc.RootAssembly is null)
        {
            Console.WriteLine("  MODERATE profile not found");
            return;
        }

        var profile = moderateDoc.RootAssembly;

        // Get profile UUID
        if (profile.Flags.TryGetValue("uuid", out var uuidFlag))
        {
            Console.WriteLine($"  Profile UUID: {uuidFlag.RawValue}");
        }

        // Examine metadata
        var metadata = profile.ModelChildren.FirstOrDefault(c => c.Name == "metadata") as AssemblyNode;
        if (metadata is not null)
        {
            var title = metadata.ModelChildren.FirstOrDefault(c => c.Name == "title") as FieldNode;
            Console.WriteLine($"  Title: {title?.RawValue}");

            var version = metadata.ModelChildren.FirstOrDefault(c => c.Name == "version") as FieldNode;
            Console.WriteLine($"  Version: {version?.RawValue}");
        }
        Console.WriteLine();

        // Step 4: Examine imports (what catalogs this profile references)
        Console.WriteLine("Step 4: Profile Imports...");
        var imports = profile.ModelChildren.Where(c => c.Name == "import").ToList();
        Console.WriteLine($"  Number of imports: {imports.Count}");

        foreach (var import in imports.Cast<AssemblyNode>())
        {
            if (import.Flags.TryGetValue("href", out var hrefFlag))
            {
                Console.WriteLine($"  Import source: {hrefFlag.RawValue}");
            }

            // Count included controls
            var includeControls = import.ModelChildren
                .FirstOrDefault(c => c.Name == "include-controls") as AssemblyNode;

            if (includeControls is not null)
            {
                var withIds = includeControls.ModelChildren
                    .Where(c => c.Name == "with-id")
                    .ToList();

                Console.WriteLine($"  Controls explicitly included: {withIds.Count}");

                // Show first few control IDs
                var sampleIds = withIds.Take(5).Select(w => (w as FieldNode)?.RawValue).ToList();
                Console.WriteLine($"  Sample control IDs: {string.Join(", ", sampleIds)}...");
            }
        }
        Console.WriteLine();

        // Step 5: Compare baselines
        Console.WriteLine("Step 5: Comparing Baselines...");
        Console.WriteLine();
        Console.WriteLine("  Baseline        | Selected Controls");
        Console.WriteLine("  ----------------|-------------------");

        foreach (var (name, doc) in profiles.OrderBy(p => p.Key))
        {
            var controlCount = CountSelectedControls(doc.RootAssembly!);
            Console.WriteLine($"  {name,-15} | {controlCount}");
        }
        Console.WriteLine();

        // Step 6: Examine modifications (if any)
        Console.WriteLine("Step 6: Profile Modifications...");
        var modify = profile.ModelChildren.FirstOrDefault(c => c.Name == "modify") as AssemblyNode;
        if (modify is not null)
        {
            var setParameters = modify.ModelChildren.Where(c => c.Name == "set-parameter").ToList();
            var alterations = modify.ModelChildren.Where(c => c.Name == "alter").ToList();

            Console.WriteLine($"  Parameter settings: {setParameters.Count}");
            Console.WriteLine($"  Control alterations: {alterations.Count}");

            if (setParameters.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("  Sample parameter settings:");
                foreach (var param in setParameters.Take(3).Cast<AssemblyNode>())
                {
                    var paramId = param.Flags.TryGetValue("param-id", out var pidFlag) ? pidFlag.RawValue : "?";
                    Console.WriteLine($"    - {paramId}");
                }

                if (setParameters.Count > 3)
                {
                    Console.WriteLine($"    ... and {setParameters.Count - 3} more");
                }
            }
        }
        else
        {
            Console.WriteLine("  No modifications defined in this profile");
        }

        Console.WriteLine();
        Console.WriteLine("Profile loading example complete!");
    }

    private static int CountSelectedControls(AssemblyNode profile)
    {
        var count = 0;

        foreach (var import in profile.ModelChildren.Where(c => c.Name == "import").Cast<AssemblyNode>())
        {
            // Check include-all
            var includeAll = import.ModelChildren.FirstOrDefault(c => c.Name == "include-all");
            if (includeAll is not null)
            {
                // This means all controls are included - we'd need to resolve the catalog to count
                // For now, mark as -1 to indicate "all"
                return -1;
            }

            // Count with-id entries
            var includeControls = import.ModelChildren
                .FirstOrDefault(c => c.Name == "include-controls") as AssemblyNode;

            if (includeControls is not null)
            {
                count += includeControls.ModelChildren.Count(c => c.Name == "with-id");
            }
        }

        return count;
    }
}
