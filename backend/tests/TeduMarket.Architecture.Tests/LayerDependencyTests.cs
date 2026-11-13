using NetArchTest.Rules;
using TeduMarket.Application;
using TeduMarket.Domain;
using TeduMarket.Infrastructure;

namespace TeduMarket.Architecture.Tests;

[Trait("Category", "Unit")]
public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_ReferencesOtherLayers_ReturnsFailure()
    {
        // Arrange
        var assembly = typeof(DomainAssemblyMarker).Assembly;
        var forbiddenNamespaces = new[] { "TeduMarket.Application", "TeduMarket.Infrastructure", "TeduMarket.Api" };

        // Act
        var result = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(forbiddenNamespaces).GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_ReferencesInfrastructureOrApi_ReturnsFailure()
    {
        // Arrange
        var assembly = typeof(ApplicationAssemblyMarker).Assembly;
        var forbiddenNamespaces = new[] { "TeduMarket.Infrastructure", "TeduMarket.Api" };

        // Act
        var result = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(forbiddenNamespaces).GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Infrastructure_ReferencesApi_ReturnsFailure()
    {
        // Arrange
        var assembly = typeof(InfrastructureAssemblyMarker).Assembly;
        var forbiddenNamespaces = new[] { "TeduMarket.Api" };

        // Act
        var result = Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(forbiddenNamespaces).GetResult();

        // Assert
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void DomainAndApplication_ReferenceEfCoreOrDapper_ReturnsFailure()
    {
        // Arrange
        var assemblies = new[] { typeof(DomainAssemblyMarker).Assembly, typeof(ApplicationAssemblyMarker).Assembly };
        var forbiddenNamespaces = new[] { "Microsoft.EntityFrameworkCore", "Dapper", "Npgsql" };

        // Act
        var results = assemblies.Select(assembly =>
            Types.InAssembly(assembly).ShouldNot().HaveDependencyOnAny(forbiddenNamespaces).GetResult());

        // Assert
        Assert.All(results, result => Assert.True(result.IsSuccessful));
    }
}
