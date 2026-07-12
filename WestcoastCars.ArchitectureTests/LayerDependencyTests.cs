using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace WestcoastCars.ArchitectureTests;

/// <summary>
/// Enforces the clean-architecture dependency rule: Api/Web → Application → Domain,
/// Infrastructure implements Application interfaces, Contracts is a standalone DTO layer,
/// and Web talks to the Api over HTTP only (never to inner layers directly).
/// Exclude with: dotnet test --filter "Category!=Architecture"
/// </summary>
[Trait("Category", "Architecture")]
public class LayerDependencyTests
{
    private const string Domain = "WestcoastCars.Domain";
    private const string Contracts = "WestcoastCars.Contracts";
    private const string Application = "WestcoastCars.Application";
    private const string Infrastructure = "WestcoastCars.Infrastructure";
    private const string Api = "WestcoastCars.Api";
    private const string Web = "WestcoastCars.Web";
    private const string EntityFrameworkCore = "Microsoft.EntityFrameworkCore";

    private static Assembly Load(string name) => Assembly.Load(name);

    private static void AssertNoDependency(string assembly, params string[] forbidden)
    {
        var result = Types.InAssembly(Load(assembly))
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"{assembly} must not depend on [{string.Join(", ", forbidden)}], " +
            $"but these types do: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainDependsOnNothing()
        => AssertNoDependency(Domain, Contracts, Application, Infrastructure, Api, Web, EntityFrameworkCore);

    [Fact]
    public void ContractsIsAStandaloneDtoLayer()
        => AssertNoDependency(Contracts, Domain, Application, Infrastructure, Api, Web);

    [Fact]
    public void ApplicationNeverReachesOutward()
        => AssertNoDependency(Application, Infrastructure, Api, Web, EntityFrameworkCore);

    [Fact]
    public void InfrastructureNeverDependsOnPresentation()
        => AssertNoDependency(Infrastructure, Api, Web);

    [Fact]
    public void WebOnlyUsesContractsAndTalksToTheApiOverHttp()
        => AssertNoDependency(Web, Domain, Application, Infrastructure, Api);
}
