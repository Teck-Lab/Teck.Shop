using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Catalog.Arch.UnitTests;
using Teck.Shop.Architectures.UnitTests.Rules;
using Teck.Shop.SharedKernel.Core.CQRS;
using Teck.Shop.SharedKernel.Core.Events;

namespace Catalog.Arch.UnitTests.Application
{
    public class ApplicationTests : ArchUnitBaseTest
    {
    [Fact]
    public void CommandHandlers_Should_FollowRules()
    {
        CommandHandlerRules.CommandHandlersShouldBeSealed(Architecture);
        CommandHandlerRules.CommandHandlersShouldResideInFeaturesNamespace(Architecture, "Catalog.Application.Features");
        CommandHandlerRules.CommandHandlersShouldHaveCorrectName(Architecture);
        CommandHandlerRules.CommandHandlersShouldNotBePublic(Architecture);
        CommandHandlerRules.CommandsShouldBeImmutable(Architecture);
    }

    [Fact]
    public void QueryHandlers_Should_FollowRules()
    {
        QueryHandlerRules.QueryHandlersShouldHaveCorrectName(Architecture);
        QueryHandlerRules.QueryHandlersShouldNotBePublic(Architecture);
    }
    }
}
