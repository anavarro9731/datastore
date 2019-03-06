namespace DataStore
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class DefaultQueryOptions<T> : IQueryOptions, ISkipAndTake<T>, IOrderBy<T> where T : class, IAggregate, new()
    {
        private bool descending;

        private string orderByProperty;

        private int skip;

        private int take;

        public DefaultQueryOptions<T> OrderBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            this.orderByProperty = Objects.GetPropertyName(propertyRefExpr);
            this.descending = descending;
            return this;
        }

        public DefaultQueryOptions<T> Skip(int skip)
        {
            this.skip = skip;
            return this;
        }

        public DefaultQueryOptions<T> Take(int take)
        {
            this.take = take;
            return this;
        }

        IQueryable<T> IOrderBy<T>.AddOrderBy(IQueryable<T> queryable)
        {
            {
                return !string.IsNullOrEmpty(this.orderByProperty) ? OrderBy(queryable, this.orderByProperty, this.descending) : queryable;
            }

            IOrderedQueryable<TEntity> OrderBy<TEntity>(IQueryable<TEntity> source, string orderByProperty, bool desc)
            {
                var command = desc ? "OrderByDescending" : "OrderBy";
                var type = typeof(TEntity);
                var property = type.GetProperty(orderByProperty);
                var parameter = Expression.Parameter(type, "p");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExpression = Expression.Lambda(propertyAccess, parameter);
                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    command,
                    new[]
                    {
                        type,
                        property.PropertyType
                    },
                    source.Expression,
                    Expression.Quote(orderByExpression));
                return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(resultExpression);
            }
        }

        IQueryable<T> ISkipAndTake<T>.AddSkip(IQueryable<T> queryable)
        {
            return this.skip > 0 ? queryable.Skip(this.skip) : queryable;
        }

        IQueryable<T> ISkipAndTake<T>.AddTake(IQueryable<T> queryable)
        {
            return this.take > 0 ? queryable.Take(this.take) : queryable;
        }
    }
}