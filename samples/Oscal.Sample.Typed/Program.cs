// Licensed under the MIT License.

using Oscal.Sample.Typed.Examples;

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║         OSCAL Sample - Strongly Typed API                      ║");
Console.WriteLine("║         Demonstrating Type Generation Concepts                 ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// If command line arg provided, run that specific example
if (args.Length > 0 && int.TryParse(args[0], out var exampleNum))
{
    RunExample(exampleNum);
    return;
}

// Otherwise show menu
while (true)
{
    Console.WriteLine("Available Examples:");
    Console.WriteLine("  1. Hand-Crafted Typed API Demo");
    Console.WriteLine("  2. CLI Code Generation Demo");
    Console.WriteLine("  0. Exit");
    Console.WriteLine();
    Console.Write("Select an example (0-2): ");

    var input = Console.ReadLine();
    if (!int.TryParse(input, out var choice))
    {
        Console.WriteLine("Invalid input. Please enter a number.");
        continue;
    }

    if (choice == 0) break;

    Console.WriteLine();
    RunExample(choice);
    Console.WriteLine();
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey(true);
    Console.Clear();
}

void RunExample(int number)
{
    switch (number)
    {
        case 1:
            TypedApiDemoExample.Run();
            break;
        case 2:
            CliCodeGenExample.Run();
            break;
        default:
            Console.WriteLine($"Unknown example: {number}");
            break;
    }
}
