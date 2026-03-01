#:package Bullseye
#:package SimpleExec

using Bullseye;
using static Bullseye.Targets;
using static SimpleExec.Command;

// Find repository root by looking for .git directory
var repoRoot = Directory.GetCurrentDirectory();
while (!Directory.Exists(Path.Combine(repoRoot, ".git")))
{
    repoRoot = Directory.GetParent(repoRoot) is { } parent
        ? parent.FullName
        : throw new InvalidOperationException("Could not find repository root (no .git directory found)");
}

const string Clean = "clean";
const string DebugBuild = "debug-build";
const string Default = "default";
const string Pack = "pack";
const string ReleaseBuild = "release-build";
const string Restore = "restore";
const string Test = "test";
const string SolutionFile = "metaschema-dotnet.slnx";

Target(Clean, () =>
    RunAsync("dotnet", $"clean {SolutionFile}", workingDirectory: repoRoot));

Target(Restore, () =>
    RunAsync("dotnet", $"restore {SolutionFile}", workingDirectory: repoRoot));

Target(DebugBuild, dependsOn: [Restore], () =>
    RunAsync("dotnet", $"build {SolutionFile} --no-restore -c Debug", workingDirectory: repoRoot));

Target(ReleaseBuild, dependsOn: [Restore], () =>
    RunAsync("dotnet", $"build {SolutionFile} --no-restore -c Release", workingDirectory: repoRoot));

Target(Test, dependsOn: [ReleaseBuild], async () =>
{
    await RunAsync("dotnet", "run --project test/Metaschema.Tests --no-build -c Release", workingDirectory: repoRoot);
});

Target(Pack, dependsOn: [ReleaseBuild], () =>
    RunAsync("dotnet", $"pack {SolutionFile} --no-build -c Release -o artifacts/packages", workingDirectory: repoRoot));

Target(Default, [Clean, ReleaseBuild, Test]);

await RunTargetsAndExitAsync(args);
