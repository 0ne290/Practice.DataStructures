using System.Reflection;
using Xunit;

namespace Practice.DataStructures.Tests.Infrastructure;

public sealed class CoreAssemblyTests
{
    [Fact]
    [Trait("Category", "Infrastructure")]
    public void CoreAssembly_LoadedFromProjectReference_HasNoEntryPoint()
    {
        // Arrange
        const string assemblyName = "Practice.DataStructures.Core";

        // Act
        var coreAssembly = Assembly.Load(assemblyName);

        // Assert
        Assert.Null(coreAssembly.EntryPoint);
    }
}
