// Licensed under the MIT License.

using Microsoft.CodeAnalysis;

namespace Metaschema.SourceGenerator;

/// <summary>
/// Incremental source generator for generating C# code from Metaschema modules at build time.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class MetaschemaSourceGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // TODO: Implement incremental source generation
        // 1. Register for MetaschemaFile additional files
        // 2. Parse Metaschema modules
        // 3. Generate C# source code
    }
}
