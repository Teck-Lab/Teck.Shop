using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Teck.Shop.Architectures.UnitTests.Rules;
using Teck.Shop.SharedKernel.Core.Domain;
using Xunit;

namespace Catalog.Arch.UnitTests.Domain;

public class AggregateRootTests : ArchUnitBaseTest
{
    [Fact]
    public void AggregateRoots_Should_FollowRules()
    {
        AggregateRootRules.AggregatesShouldInheritFromBaseEntity(Architecture);
        AggregateRootRules.AggregatesShouldResideInNamespace(Architecture, "Catalog.Domain");
        AggregateRootRules.AggregatesShouldOnlyExistInDomain(Architecture, "Catalog.Domain");
    }
}