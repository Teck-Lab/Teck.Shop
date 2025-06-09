using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using Teck.Shop.SharedKernel.Core.CQRS;

namespace Teck.Shop.Architectures.UnitTests.Rules;

public static class CommandHandlerRules
{
    public static void CommandHandlersShouldBeSealed(Architecture architecture)
    {
        var rule = ArchRuleDefinition
            .Classes()
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Should()
            .BeSealed()
            .Because("command handlers should be sealed to prevent inheritance");

        rule.Check(architecture);
    }

    public static void CommandHandlersShouldResideInFeaturesNamespace(Architecture architecture, string featuresNamespace)
    {
        var rule = ArchRuleDefinition
            .Classes()
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Should()
            .ResideInNamespace(featuresNamespace, true)
            .Because("command handlers should be organized in feature folders");

        rule.Check(architecture);
    }

public static void CommandsShouldBeImmutable(Architecture architecture)
{
    var rule = ArchRuleDefinition
        .Classes()
        .That()
        .AreAssignableTo(typeof(ICommand<>))
        .Should()
        .BeImmutable()
        .Because("commands should be immutable to prevent state changes");

    rule.Check(architecture);
}
        public static void CommandHandlersShouldHaveCorrectName(Architecture architecture)
    {
        var rule = ArchRuleDefinition
            .Classes()
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .Because("command handlers should follow naming convention");

        rule.Check(architecture);
    }

    public static void CommandHandlersShouldNotBePublic(Architecture architecture)
    {
        var rule = ArchRuleDefinition
            .Classes()
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Should()
            .NotBePublic()
            .Because("command handlers should be internal for better encapsulation");

        rule.Check(architecture);
    }
}