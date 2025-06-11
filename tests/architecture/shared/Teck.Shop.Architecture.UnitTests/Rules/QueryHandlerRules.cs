using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Teck.Shop.SharedKernel.Core.CQRS;

namespace Teck.Shop.Architectures.UnitTests.Rules;

public static class QueryHandlerRules
{
    public static void QueryHandlersShouldBeSealed(Architecture architecture)
    {
        var rule = Classes()
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .BeSealed()
            .Because("query handlers should be sealed to prevent inheritance");

        rule.Check(architecture);
    }

    public static void QueryHandlersShouldBeReadOnly(Architecture architecture)
    {
        var rule = Classes()
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .BeImmutable()
            .Because("query handlers should not modify state");

        rule.Check(architecture);
    }

    public static void QueriesShouldNotModifyState(Architecture architecture)
    {
        var rule = Classes()
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .BeImmutable()
            .Because("queries should be immutable");

        rule.Check(architecture);
    }
        public static void QueryHandlersShouldHaveCorrectName(Architecture architecture)
    {
        var rule = ArchRuleDefinition
            .Classes()
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .Because("query handlers should follow naming convention");

        rule.Check(architecture);
    }

    public static void QueryHandlersShouldNotBePublic(Architecture architecture)
    {
        var rule = ArchRuleDefinition
            .Classes()
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .NotBePublic()
            .Because("query handlers should be internal for better encapsulation");

        rule.Check(architecture);
    }
}