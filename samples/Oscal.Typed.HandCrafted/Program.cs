// Licensed under the MIT License.
// Demonstrates hand-crafted OSCAL POCO types showing what a strongly-typed API looks like.

using Oscal.Typed.HandCrafted;

// Create a catalog programmatically with strongly-typed classes
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

Console.WriteLine($"Created catalog: {catalog.Metadata?.Title}");
Console.WriteLine($"  UUID: {catalog.Uuid}");
Console.WriteLine($"  Groups: {catalog.Groups.Count}");
Console.WriteLine($"  Total Controls: {catalog.Groups.Sum(g => g.Controls.Count)}");
Console.WriteLine();

// Type-safe navigation with IntelliSense support
var acGroup = catalog.Groups.FirstOrDefault(g => g.Id == "ac");
if (acGroup != null)
{
    Console.WriteLine($"Access Control Family:");
    Console.WriteLine($"  Title: {acGroup.Title}");
    Console.WriteLine($"  Controls: {acGroup.Controls.Count}");

    foreach (var control in acGroup.Controls)
    {
        Console.WriteLine($"    [{control.Id}] {control.Title}");
        Console.WriteLine($"      Class: {control.Class}");
        Console.WriteLine($"      Parts: {control.Parts.Count}, Parameters: {control.Params.Count}");
    }
}

Console.WriteLine();
Console.WriteLine("Benefits of strongly-typed APIs:");
Console.WriteLine("  - IntelliSense autocomplete for all properties");
Console.WriteLine("  - Compile-time checking for typos");
Console.WriteLine("  - Refactoring support across codebase");
Console.WriteLine("  - Strongly-typed collections (List<Control>, not List<object>)");
