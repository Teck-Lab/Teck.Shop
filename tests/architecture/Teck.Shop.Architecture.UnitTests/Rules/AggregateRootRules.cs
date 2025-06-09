using System;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using Teck.Shop.SharedKernel.Core.Domain;

namespace Teck.Shop.Architectures.UnitTests.Rules;

public static class AggregateRootRules
{
    public static void AggregatesShouldInheritFromBaseEntity(Architecture architecture)
    {
        var rule = ArchRuleDefinition
            .Classes()
            .That().ImplementInterface(typeof(IAggregateRoot))
            .Should().BeAssignableTo(typeof(BaseEntity))
            .Because("all aggregate roots must inherit from BaseEntity");

        rule.Check(architecture);
    }

    public static void AggregatesShouldResideInNamespace(Architecture architecture, string expectedNamespace)
    {
        var rule = ArchRuleDefinition
            .Classes()
            .That().ImplementInterface(typeof(IAggregateRoot))
            .Should().ResideInNamespace(expectedNamespace, true)
            .Because("aggregate roots should live in the domain layer");

        rule.Check(architecture);
    }
    public static void AggregatesShouldOnlyExistInDomain(Architecture architecture, string domainNamespace)
{
    var rule = ArchRuleDefinition
        .Classes()
        .That()
        .ImplementInterface(typeof(IAggregateRoot))
        .Should()
        .ResideInNamespace(domainNamespace, true)
        .Because("aggregate roots should only exist in domain layer");

    rule.Check(architecture);
}
}