
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using Teck.Shop.Architectures.UnitTests.Rules;
using Teck.Shop.SharedKernel.Core.Database;
using Teck.Shop.SharedKernel.Core.Domain;
using Teck.Shop.SharedKernel.Persistence.Database.EFCore;

namespace Catalog.Arch.UnitTests.Infrastructure
{
    public class RepositoryTests : ArchUnitBaseTest
    {
[Fact]
public void Repositories_Should_ImplementCorrectInterface()
{
    var rule = ArchRuleDefinition
        .Classes()
        .That()
        .HaveNameEndingWith("Repository")
        .Should()
        .BeAssignableTo(typeof(GenericRepository<,>))
        .AndShould()
        .ImplementInterface(typeof(IGenericRepository<,>))
        .Because("repositories should inherit from GenericRepository and implement their specific interface");

    rule.Check(Architecture);
}
[Fact]
public void Repositories_Should_BeSealed()
{
    ArchRuleDefinition
        .Classes()
        .That()
        .HaveNameEndingWith("Repository")
        .Should()
        .BeSealed()
        .Because("repositories should be sealed to prevent inheritance")
        .Check(Architecture);
}
    }
}