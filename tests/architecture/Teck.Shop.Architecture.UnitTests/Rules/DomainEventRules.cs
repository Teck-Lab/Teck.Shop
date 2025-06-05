using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Teck.Shop.SharedKernel.Core.Events;

namespace Teck.Shop.Architectures.UnitTests.Rules;

public static class DomainEventRules
{
    public static void DomainEventsShouldBeSealed(Architecture architecture)
    {
        var rule = ArchRuleDefinition
            .Classes()
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .BeSealed()
            .Because("domain events should be sealed to prevent inheritance");

        rule.Check(architecture);
    }

    public static void DomainEventsShouldHaveCorrectName(Architecture architecture)
    {
        var rule = ArchRuleDefinition
            .Classes()
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .Because("domain events should follow naming convention");

        rule.Check(architecture);
    }
}