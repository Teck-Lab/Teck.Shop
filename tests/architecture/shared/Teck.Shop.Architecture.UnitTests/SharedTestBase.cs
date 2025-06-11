using System.Reflection;
using ArchUnitNET.Loader;
using ArchUnitNET.Domain;
using Assembly = System.Reflection.Assembly;

namespace Teck.Shop.Architectures.UnitTests;

public abstract class SharedTestBase
{
    // Load only assemblies relevant to shared rules (not a specific service)
    protected static readonly Assembly SharedKernelAssembly = typeof(SharedKernel.Core.Domain.IAggregateRoot).Assembly;
    protected static readonly Assembly PersistenceAssembly = typeof(SharedKernel.Persistence.Database.EFCore.GenericRepository<,>).Assembly;

    protected static readonly Architecture SharedArchitecture = new ArchLoader()
        .LoadAssemblies(
            SharedKernelAssembly,
            PersistenceAssembly
        )
        .Build();
}