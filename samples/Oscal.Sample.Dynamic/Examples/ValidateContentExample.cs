// Licensed under the MIT License.

using Metaschema.Core.Constraints;
using Metaschema.Core.Loading;
using Metaschema.Databind;
using Metaschema.Databind.Validation;

namespace Oscal.Sample.Dynamic.Examples;

/// <summary>
/// Demonstrates validating OSCAL content against Metaschema constraints.
/// 
/// Metaschema defines constraints that content must satisfy, including:
/// - Allowed values for specific fields
/// - Pattern matching for string formats
/// - Cardinality requirements
/// - Cross-reference validation
/// </summary>
public static class ValidateContentExample
{
    public static void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Example 4: Validate OSCAL Content                           ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Step 1: Load the OSCAL Catalog Metaschema
        Console.WriteLine("Step 1: Loading OSCAL Catalog Metaschema...");
        var loader = new ModuleLoader();
        var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", "oscal_catalog_metaschema.xml");
        var module = loader.Load(metaschemaPath);
        Console.WriteLine($"  Loaded: {module.Name}");
        Console.WriteLine();

        // Step 2: Load the catalog content
        Console.WriteLine("Step 2: Loading NIST SP 800-53 Catalog...");
        var context = new BindingContext();
        context.RegisterModule(module);

        var catalogPath = Path.Combine(AppContext.BaseDirectory, "Content", "catalog", "NIST_SP-800-53_rev5_catalog.json");
        var deserializer = context.GetDeserializer(Format.Json);
        var document = deserializer.Deserialize(File.ReadAllText(catalogPath));
        Console.WriteLine($"  Loaded catalog document");
        Console.WriteLine();

        // Step 3: Collect and analyze constraints from the module
        Console.WriteLine("Step 3: Analyzing Constraints in Metaschema...");

        var constraintCounts = new Dictionary<string, int>
        {
            ["allowed-values"] = 0,
            ["matches"] = 0,
            ["expect"] = 0,
            ["index"] = 0,
            ["index-has-key"] = 0,
            ["is-unique"] = 0,
            ["has-cardinality"] = 0
        };

        // Collect all constraints from the module
        var allConstraints = new List<IConstraint>();

        foreach (var assembly in module.AssemblyDefinitions)
        {
            allConstraints.AddRange(assembly.Constraints);
            CountConstraints(assembly.Constraints, constraintCounts);
        }
        foreach (var field in module.FieldDefinitions)
        {
            allConstraints.AddRange(field.Constraints);
            CountConstraints(field.Constraints, constraintCounts);
        }
        foreach (var flag in module.FlagDefinitions)
        {
            allConstraints.AddRange(flag.Constraints);
            CountConstraints(flag.Constraints, constraintCounts);
        }

        Console.WriteLine("  Constraint types in catalog metaschema:");
        foreach (var (type, count) in constraintCounts.Where(c => c.Value > 0))
        {
            Console.WriteLine($"    - {type}: {count}");
        }
        Console.WriteLine($"  Total constraints: {allConstraints.Count}");
        Console.WriteLine();

        // Step 4: Create validator and validate the document
        Console.WriteLine("Step 4: Validating Document...");

        var validator = new ConstraintValidator();
        var adapter = new DocumentNodeAdapter(document);
        var results = validator.ValidateAll(adapter, allConstraints);

        Console.WriteLine($"  Validation completed");
        Console.WriteLine($"  Is valid: {results.IsValid}");
        Console.WriteLine($"  Total findings: {results.Count}");
        Console.WriteLine();

        // Step 5: Show validation findings summary
        Console.WriteLine("Step 5: Validation Findings...");

        if (results.IsValid && results.Count == 0)
        {
            Console.WriteLine("  No validation issues found - document is fully compliant!");
        }
        else
        {
            // Show summary by severity
            Console.WriteLine($"  Critical: {results.CriticalCount}");
            Console.WriteLine($"  Errors: {results.ErrorCount}");
            Console.WriteLine($"  Warnings: {results.WarningCount}");
            Console.WriteLine($"  Informational: {results.InformationalCount}");
            Console.WriteLine();

            // Show sample findings
            var sampleFindings = results.Findings.Take(5).ToList();
            if (sampleFindings.Count > 0)
            {
                Console.WriteLine("  Sample findings:");
                foreach (var finding in sampleFindings)
                {
                    Console.WriteLine($"    [{finding.Severity}] {finding.Message}");
                    if (finding.Location is not null)
                    {
                        Console.WriteLine($"             at: {finding.Location}");
                    }
                }

                if (results.Count > 5)
                {
                    Console.WriteLine($"    ... and {results.Count - 5} more findings");
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine("Validation example complete!");
    }

    private static void CountConstraints(IReadOnlyList<IConstraint> constraints, Dictionary<string, int> counts)
    {
        foreach (var constraint in constraints)
        {
            switch (constraint)
            {
                case IAllowedValuesConstraint:
                    counts["allowed-values"]++;
                    break;
                case IMatchesConstraint:
                    counts["matches"]++;
                    break;
                case IExpectConstraint:
                    counts["expect"]++;
                    break;
                case IIndexConstraint:
                    counts["index"]++;
                    break;
                case IIndexHasKeyConstraint:
                    counts["index-has-key"]++;
                    break;
                case IUniqueConstraint:
                    counts["is-unique"]++;
                    break;
                case ICardinalityConstraint:
                    counts["has-cardinality"]++;
                    break;
            }
        }
    }
}
