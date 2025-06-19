using System;
using ArchUnitNET.Domain;
using Teck.Shop.SharedKernel.Core.Domain;
using Xunit;

namespace Teck.Shop.Architectures.UnitTests.Rules;

    public static class RepositoryRules
    {
        public static void Repositories_Should_UseEntitiesImplementingIAggregateRoot(Architecture architecture)
        {
            // Find repository classes in the loaded architecture
            var repoClasses = architecture.Classes
                .Where(c => c.Name.EndsWith("Repository"))
                .ToList();

            foreach (var repoClass in repoClasses)
            {
                // Get the System.Type for the repo class
                var systemType = Type.GetType(repoClass.FullName);
                if (systemType == null)
                {
                    throw new Exception($"Type not found for {repoClass.FullName}");
                }

                var baseType = systemType.BaseType;
                if (baseType == null || !baseType.IsGenericType)
                {
                    // Could skip or fail depending on your conventions
                    continue;
                }

                if (!baseType.GetGenericTypeDefinition().Name.StartsWith("GenericRepository"))
                {
                    continue;
                }

                var entityType = baseType.GetGenericArguments()[0];
                bool implementsAggregateRoot = typeof(IAggregateRoot).IsAssignableFrom(entityType);

                Assert.True(implementsAggregateRoot,
                    $"{systemType.Name} uses entity {entityType.Name} which does not implement IAggregateRoot");
            }
        }
    }