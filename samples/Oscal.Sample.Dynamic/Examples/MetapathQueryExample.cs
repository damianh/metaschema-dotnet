// Licensed under the MIT License.

using System.Globalization;

using Metaschema.Core.Loading;
using Metaschema.Core.Metapath;
using Metaschema.Core.Metapath.Context;
using Metaschema.Core.Metapath.Item;
using Metaschema.Databind;
using Metaschema.Databind.Nodes;
using Metaschema.Databind.Validation;

namespace Oscal.Sample.Dynamic.Examples;

/// <summary>
/// Demonstrates querying OSCAL documents using Metapath expressions.
/// 
/// Metapath is an expression language based on XPath 3.1, specifically designed
/// for querying Metaschema-based documents. It supports path navigation, filtering,
/// and a rich set of built-in functions.
/// </summary>
public static class MetapathQueryExample
{
    public static void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Example 5: Query with Metapath                              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Step 1: Load the OSCAL Catalog
        Console.WriteLine("Step 1: Loading OSCAL Catalog...");
        var loader = new ModuleLoader();
        var metaschemaPath = Path.Combine(AppContext.BaseDirectory, "Metaschema", "oscal_catalog_metaschema.xml");
        var module = loader.Load(metaschemaPath);

        var context = new BindingContext();
        context.RegisterModule(module);

        var catalogPath = Path.Combine(AppContext.BaseDirectory, "Content", "catalog", "NIST_SP-800-53_rev5_catalog.json");
        var deserializer = context.GetDeserializer(Format.Json);
        var document = deserializer.Deserialize(File.ReadAllText(catalogPath));
        Console.WriteLine($"  Loaded NIST 800-53 catalog");
        Console.WriteLine();

        // Create a node adapter for Metapath queries
        var rootNode = new DocumentNodeAdapter(document);

        // Step 2: Simple path expressions
        Console.WriteLine("Step 2: Simple Path Expressions...");
        Console.WriteLine();

        // Query: Get catalog title
        RunQuery("Get catalog title", "catalog/metadata/title", rootNode);

        // Query: Get OSCAL version
        RunQuery("Get OSCAL version", "catalog/metadata/oscal-version", rootNode);

        // Query: Count all control groups
        RunQuery("Count control groups", "count(catalog/group)", rootNode);

        // Step 3: Filtering with predicates
        Console.WriteLine("Step 3: Filtering with Predicates...");
        Console.WriteLine();

        // Query: Find control by ID
        RunQuery("Find control AC-1", "catalog//control[@id='ac-1']/title", rootNode);

        // Query: Find Access Control family
        RunQuery("Find Access Control family", "catalog/group[@id='ac']/title", rootNode);

        // Step 4: Aggregate functions
        Console.WriteLine("Step 4: Aggregate Functions...");
        Console.WriteLine();

        // Query: Count all controls
        RunQuery("Count all controls", "count(catalog//control)", rootNode);

        // Query: Count controls with enhancements
        RunQuery("Count control enhancements", "count(catalog//control/control)", rootNode);

        // Step 5: String functions
        Console.WriteLine("Step 5: String Functions...");
        Console.WriteLine();

        // Query: Check if control exists
        RunQuery("AC-1 exists?", "exists(catalog//control[@id='ac-1'])", rootNode);

        // Query: Control IDs starting with 'ac'
        RunQuery("Controls starting with 'ac-1'",
            "count(catalog//control[starts-with(@id, 'ac-1')])", rootNode);

        // Step 6: More complex queries
        Console.WriteLine("Step 6: Complex Queries...");
        Console.WriteLine();

        // Query: Get all group IDs
        RunQuery("All group IDs", "catalog/group/@id", rootNode, maxResults: 5);

        // Query: Controls with parameters
        RunQuery("Controls with parameters",
            "count(catalog//control[param])", rootNode);

        Console.WriteLine();
        Console.WriteLine("Metapath query example complete!");
    }

    private static void RunQuery(string description, string expression, INodeItem contextNode, int maxResults = 1)
    {
        Console.WriteLine($"  Query: {description}");
        Console.WriteLine($"  Expression: {expression}");

        try
        {
            var expr = MetapathExpression.Compile(expression);
            var metapathContext = MetapathContext.Create().ForNode(contextNode);
            var result = expr.Evaluate(metapathContext);

            if (result.IsEmpty)
            {
                Console.WriteLine($"  Result: (empty)");
            }
            else if (result.Count == 1)
            {
                var item = result.FirstOrDefault;
                Console.WriteLine($"  Result: {FormatItem(item)}");
            }
            else
            {
                Console.WriteLine($"  Results ({result.Count} items):");
                var items = result.Take(maxResults).ToList();
                foreach (var item in items)
                {
                    Console.WriteLine($"    - {FormatItem(item)}");
                }

                if (result.Count > maxResults)
                {
                    Console.WriteLine($"    ... and {result.Count - maxResults} more");
                }
            }
        }
        catch (MetapathException ex)
        {
            Console.WriteLine($"  Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    private static string FormatItem(IItem? item)
    {
        return item switch
        {
            null => "(null)",
            BooleanItem b => b.Value ? "true" : "false",
            IntegerItem i => i.Value.ToString(CultureInfo.InvariantCulture),
            DecimalItem d => d.Value.ToString(CultureInfo.InvariantCulture),
            StringItem s => $"\"{s.Value}\"",
            INodeItem node => node.GetStringValue(),
            _ => item.GetStringValue()
        };
    }
}
