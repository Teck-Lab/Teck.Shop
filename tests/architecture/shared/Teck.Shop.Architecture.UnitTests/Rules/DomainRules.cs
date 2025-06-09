using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using Teck.Shop.SharedKernel.Core.Domain;
using Teck.Shop.SharedKernel.Core.Events;

namespace Teck.Shop.Architectures.UnitTests.Rules;

public static class DomainRules
{
    public static void EntitiesShouldInheritBaseEntity(Architecture architecture)
    {
        var rule = ArchRuleDefinition.Classes()
            .That()
            .ImplementInterface(typeof(IBaseEntity))
            .Should()
            .BeAssignableTo(typeof(BaseEntity))
            .Because("entities should inherit from BaseEntity");

        rule.Check(architecture);
    }

    public static void AggregatesShouldTrackDomainEvents(Architecture architecture)
    {
        var rule = ArchRuleDefinition.Classes()
            .That()
            .ImplementInterface(typeof(IAggregateRoot))
            .Should()
            .HaveFieldMemberWithName("_domainEvents")
            .AndShould()
            .HaveMethodMemberWithName("AddDomainEvent")
            .AndShould()
            .HaveMethodMemberWithName("ClearDomainEvents")
            .AndShould()
            .HaveMethodMemberWithName("GetDomainEvents")
            .Because("aggregates should track domain events");

        rule.Check(architecture);
    }

    public static void EntitiesShouldHavePrivateSetters(Architecture architecture)
    {
        var rule = ArchRuleDefinition.Members()
            .That()
            .Are("set_*")
            .And()
            .AreDeclaredIn(ArchRuleDefinition.Classes().That().ImplementInterface(typeof(IBaseEntity)))
            .Should()
            .NotBePublic()
            .Because("entity properties should have private setters for encapsulation")
            .WithoutRequiringPositiveResults();

        rule.Check(architecture);
    }

    public static void AggregateRootsShouldBeInDomainLayer(Architecture architecture)
    {
        var rule = ArchRuleDefinition.Classes()
            .That()
            .ImplementInterface(typeof(IAggregateRoot))
            .Should()
            .ResideInNamespace("Domain.Entities", true)
            .Because("aggregate roots should be in the domain layer");

        rule.Check(architecture);
    }

    public static void EntityCreateMethodsShouldBeStatic(Architecture architecture)
    {
        var rule = ArchRuleDefinition.Members()
            .That()
            .Are("method")
            .And()
            .HaveNameStartingWith("Create")
            .And()
            .AreDeclaredIn(ArchRuleDefinition.Classes().That().ImplementInterface(typeof(IBaseEntity)))
            .Should()
            .BeStatic()
            .Because("entity factory methods should be static")
            .WithoutRequiringPositiveResults();

        rule.Check(architecture);
    }
}