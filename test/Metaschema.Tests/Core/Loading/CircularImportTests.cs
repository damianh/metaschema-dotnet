// Licensed under the MIT License.

using Shouldly;
using Xunit;

namespace Metaschema.Loading;

public class CircularImportTests
{
    private static string GetTestDataPath(string relativePath) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", relativePath);

    [Fact]
    public void Load_CircularImport_ShouldThrowCircularImportException()
    {
        // Arrange
        var loader = new ModuleLoader();
        var path = GetTestDataPath("circular-import/a.xml");

        // Act & Assert
        var exception = Should.Throw<CircularImportException>(() => loader.Load(path));

        // The exception should contain the import chain
        exception.ImportChain.ShouldNotBeNull();
        exception.ImportChain.Count.ShouldBeGreaterThan(1);
        exception.Message.ShouldContain("Circular import detected");
    }
}
