// Licensed under the MIT License.

using Oscal.Sample.Dynamic.Examples;

namespace Oscal.Sample.Dynamic;

/// <summary>
/// OSCAL Sample - Dynamic Document API
/// 
/// This sample demonstrates how to work with OSCAL documents using the
/// Metaschema .NET dynamic document model. This approach loads documents
/// into a generic tree structure that can be navigated and queried without
/// requiring generated types.
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         OSCAL Sample - Dynamic Document API                    ║");
        Console.WriteLine("║         Using Metaschema .NET                                  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var examples = new Dictionary<string, Func<Task>>
        {
            ["1"] = () => { LoadCatalogExample.Run(); return Task.CompletedTask; },
            ["2"] = () => { LoadProfileExample.Run(); return Task.CompletedTask; },
            ["3"] = () => { LoadSspExample.Run(); return Task.CompletedTask; },
            ["4"] = () => { ValidateContentExample.Run(); return Task.CompletedTask; },
            ["5"] = () => { MetapathQueryExample.Run(); return Task.CompletedTask; },
            ["6"] = () => { ConvertFormatExample.Run(); return Task.CompletedTask; },
            ["7"] = () => { GenerateSchemaExample.Run(); return Task.CompletedTask; },
            ["all"] = RunAllExamples
        };

        if (args.Length > 0 && examples.TryGetValue(args[0].ToLowerInvariant(), out var example))
        {
            await example();
            return;
        }

        while (true)
        {
            Console.WriteLine("Select an example to run:");
            Console.WriteLine("  [1] Load Catalog      - Load and navigate NIST 800-53 catalog");
            Console.WriteLine("  [2] Load Profile      - Load and examine a security baseline profile");
            Console.WriteLine("  [3] Load SSP          - Load a System Security Plan");
            Console.WriteLine("  [4] Validate Content  - Validate OSCAL content against constraints");
            Console.WriteLine("  [5] Metapath Query    - Query documents using Metapath expressions");
            Console.WriteLine("  [6] Convert Format    - Convert between JSON, XML, and YAML");
            Console.WriteLine("  [7] Generate Schema   - Generate JSON Schema and XSD from Metaschema");
            Console.WriteLine("  [all] Run all examples");
            Console.WriteLine("  [q] Quit");
            Console.WriteLine();
            Console.Write("Enter choice: ");

            var input = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(input) || input == "q")
            {
                break;
            }

            Console.WriteLine();

            if (examples.TryGetValue(input, out example))
            {
                try
                {
                    await example();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Invalid choice. Please try again.");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            Console.Clear();
        }
    }

    private static async Task RunAllExamples()
    {
        Console.WriteLine("Running all examples...");
        Console.WriteLine(new string('=', 70));

        LoadCatalogExample.Run();
        Console.WriteLine(new string('-', 70));

        LoadProfileExample.Run();
        Console.WriteLine(new string('-', 70));

        LoadSspExample.Run();
        Console.WriteLine(new string('-', 70));

        ValidateContentExample.Run();
        Console.WriteLine(new string('-', 70));

        MetapathQueryExample.Run();
        Console.WriteLine(new string('-', 70));

        ConvertFormatExample.Run();
        Console.WriteLine(new string('-', 70));

        GenerateSchemaExample.Run();
        Console.WriteLine(new string('=', 70));

        Console.WriteLine("All examples completed.");
        await Task.CompletedTask;
    }
}
