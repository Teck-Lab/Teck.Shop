using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Teck.Shop.SharedKernel.Persistence.Database.EFCore
{
    internal static class ModelBuilderExtensions
    {
        public static ModelBuilder AppendGlobalQueryFilter<TInterface>(this ModelBuilder modelBuilder, Expression<Func<TInterface, bool>> filter)
        {
            // get a list of entities without a baseType that implement the interface TInterface
            IEnumerable<Type> entities = modelBuilder.Model.GetEntityTypes()
                .Where(entity => entity.BaseType is null && entity.ClrType.GetInterface(typeof(TInterface).Name) is not null)
                .Select(entity => entity.ClrType);

            foreach (Type? entity in entities)
            {
                ParameterExpression parameterType = Expression.Parameter(modelBuilder.Entity(entity).Metadata.ClrType);
                Expression filterBody = ReplacingExpressionVisitor.Replace(filter.Parameters[0], parameterType, filter.Body);

                // get the existing query filter
                if (modelBuilder.Entity(entity).Metadata.GetQueryFilter() is { } existingFilter)
                {
                    Expression existingFilterBody = ReplacingExpressionVisitor.Replace(existingFilter.Parameters[0], parameterType, existingFilter.Body);

                    // combine the existing query filter with the new query filter
                    filterBody = Expression.AndAlso(existingFilterBody, filterBody);
                }

                // apply the new query filter
                modelBuilder.Entity(entity).HasQueryFilter(Expression.Lambda(filterBody, parameterType));
            }

            return modelBuilder;
        }
    }
}
