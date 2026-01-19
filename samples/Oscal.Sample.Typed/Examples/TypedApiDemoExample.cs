// Licensed under the MIT License.

namespace Oscal.Sample.Typed.Examples;

/// <summary>
/// Demonstrates a hand-crafted typed API that shows what generated types would look like.
/// This example creates simple POCO classes representing OSCAL Catalog structures.
/// </summary>
public static class TypedApiDemoExample
{
    public static void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Example 1: Hand-Crafted Typed API Demo                      ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        Console.WriteLine("This example demonstrates what a typed API for OSCAL looks like.");
        Console.WriteLine("These types are manually crafted to show the API design.");
        Console.WriteLine();

        // Step 1: Create a catalog programmatically
        Console.WriteLine("Step 1: Creating a Catalog with Strongly-Typed Classes...");
        Console.WriteLine();

        var catalog = new Catalog
        {
            Uuid = Guid.NewGuid(),
            Metadata = new Metadata
            {
                Title = "Example Security Controls Catalog",
                Version = "1.0.0",
                OscalVersion = "1.1.1",
                LastModified = DateTimeOffset.UtcNow
            },
            Groups =
            [
                new Group
                {
                    Id = "ac",
                    Title = "Access Control",
                    Controls =
                    [
                        new Control
                        {
                            Id = "ac-1",
                            Title = "Policy and Procedures",
                            Class = "SP800-53",
                            Parts =
                            [
                                new Part
                                {
                                    Id = "ac-1_stmt",
                                    Name = "statement",
                                    Prose = "The organization develops, documents, and disseminates..."
                                }
                            ],
                            Props =
                            [
                                new OscalProperty { Name = "label", Value = "AC-1" },
                                new OscalProperty { Name = "sort-id", Value = "ac-01" }
                            ]
                        },
                        new Control
                        {
                            Id = "ac-2",
                            Title = "Account Management",
                            Class = "SP800-53",
                            Params =
                            [
                                new Parameter
                                {
                                    Id = "ac-2_prm_1",
                                    Label = "organization-defined account types"
                                }
                            ]
                        }
                    ]
                },
                new Group
                {
                    Id = "au",
                    Title = "Audit and Accountability",
                    Controls =
                    [
                        new Control
                        {
                            Id = "au-1",
                            Title = "Policy and Procedures",
                            Class = "SP800-53"
                        }
                    ]
                }
            ]
        };

        Console.WriteLine($"  Created catalog: {catalog.Metadata?.Title}");
        Console.WriteLine($"  UUID: {catalog.Uuid}");
        Console.WriteLine($"  Groups: {catalog.Groups.Count}");
        Console.WriteLine($"  Total Controls: {catalog.Groups.Sum(g => g.Controls.Count)}");
        Console.WriteLine();

        // Step 2: Navigate with IntelliSense support
        Console.WriteLine("Step 2: Type-Safe Navigation...");
        Console.WriteLine();

        // This is what typed navigation looks like - full IntelliSense support
        var acGroup = catalog.Groups.FirstOrDefault(g => g.Id == "ac");
        if (acGroup != null)
        {
            Console.WriteLine($"  Access Control Family:");
            Console.WriteLine($"    Title: {acGroup.Title}");
            Console.WriteLine($"    Controls: {acGroup.Controls.Count}");

            foreach (var control in acGroup.Controls)
            {
                Console.WriteLine($"      - [{control.Id}] {control.Title}");
                Console.WriteLine($"        Class: {control.Class}");
                Console.WriteLine($"        Parts: {control.Parts.Count}");
                Console.WriteLine($"        Parameters: {control.Params.Count}");
            }
        }

        Console.WriteLine();

        // Step 3: Show compile-time safety
        Console.WriteLine("Step 3: Benefits of Compile-Time Safety...");
        Console.WriteLine();

        Console.WriteLine("  With typed APIs, you get:");
        Console.WriteLine("    - IntelliSense autocomplete for all properties");
        Console.WriteLine("    - Compile-time checking for typos in property names");
        Console.WriteLine("    - Refactoring support (rename works across codebase)");
        Console.WriteLine("    - Strongly-typed collections (List<Control>, not List<object>)");
        Console.WriteLine("    - Nullable reference type warnings");
        Console.WriteLine();

        Console.WriteLine("  Example code:");
        Console.WriteLine(@"    var title = catalog.Metadata?.Title;  // string?
    var controls = catalog.Groups
        .SelectMany(g => g.Controls)
        .Where(c => c.Class == ""SP800-53"");  // IEnumerable<Control>");
        Console.WriteLine();

        Console.WriteLine("Typed API demo complete!");
    }
}

// === Hand-crafted OSCAL Catalog types ===
// These represent what the source generator would produce

public class Catalog
{
    public Guid Uuid { get; set; }
    public Metadata? Metadata { get; set; }
    public List<Group> Groups { get; set; } = [];
    public BackMatter? BackMatter { get; set; }
}

public class Metadata
{
    public string? Title { get; set; }
    public DateTimeOffset? Published { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? Version { get; set; }
    public string? OscalVersion { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public List<Role> Roles { get; set; } = [];
    public List<Party> Parties { get; set; } = [];
    public string? Remarks { get; set; }
}

public class Group
{
    public string? Id { get; set; }
    public string? Class { get; set; }
    public string? Title { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public List<Parameter> Params { get; set; } = [];
    public List<Part> Parts { get; set; } = [];
    public List<Control> Controls { get; set; } = [];
    public List<Group> NestedGroups { get; set; } = [];  // Renamed to avoid conflict
}

public class Control
{
    public string? Id { get; set; }
    public string? Class { get; set; }
    public string? Title { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public List<Parameter> Params { get; set; } = [];
    public List<Part> Parts { get; set; } = [];
    public List<Control> Enhancements { get; set; } = [];  // Renamed to avoid conflict
}

public class Parameter
{
    public string? Id { get; set; }
    public string? Class { get; set; }
    public string? Label { get; set; }
    public string? Usage { get; set; }
    public List<string> Values { get; set; } = [];
    public string? Select { get; set; }
    public string? Remarks { get; set; }
}

public class Part
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Ns { get; set; }
    public string? Class { get; set; }
    public string? Title { get; set; }
    public string? Prose { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public List<Part> SubParts { get; set; } = [];  // Renamed to avoid conflict
}

public class OscalProperty
{
    public string? Name { get; set; }
    public Guid? Uuid { get; set; }
    public string? Ns { get; set; }
    public string? Value { get; set; }
    public string? Class { get; set; }
    public string? Remarks { get; set; }
}

public class Link
{
    public Uri? Href { get; set; }
    public string? Rel { get; set; }
    public string? MediaType { get; set; }
    public string? Text { get; set; }
}

public class Role
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public string? Remarks { get; set; }
}

public class Party
{
    public Guid Uuid { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? ShortName { get; set; }
    public List<string> EmailAddresses { get; set; } = [];
    public List<string> TelephoneNumbers { get; set; } = [];
    public string? Remarks { get; set; }
}

public class BackMatter
{
    public List<Resource> Resources { get; set; } = [];
}

public class Resource
{
    public Guid Uuid { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public string? Remarks { get; set; }
}
