namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class WithoutReplayOptions<T> : IWithoutReplayOptions<T> where T : class, IEntity, new()
    {
        private readonly Queue<(string, bool)> thenBys = new Queue<(string, bool)>();

        private bool orderByDirection;

        private string orderByProperty;

        private int skip;

        private int take;

        public WithoutReplayOptions<T> OrderBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            this.orderByProperty = Objects.GetPropertyName(propertyRefExpr);
            this.orderByDirection = descending;
            return this;
        }

        public WithoutReplayOptions<T> Skip(int skip)
        {
            this.skip = skip;
            return this;
        }

        public WithoutReplayOptions<T> Take(int take)
        {
            this.take = take;
            return this;
        }

        public WithoutReplayOptions<T> ThenBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            this.thenBys.Enqueue((Objects.GetPropertyName(propertyRefExpr), descending));
            return this;
        }

        IQueryable<T> IOrderBy<T>.AddOrderBy(IQueryable<T> queryable)
        {
            if (!string.IsNullOrEmpty(this.orderByProperty))
            {
                queryable = OrderBy(queryable, this.orderByProperty, this.orderByDirection);

                while (this.thenBys.Count > 0)
                {
                    var thenBy = this.thenBys.Dequeue();

                    queryable = ThenBy(queryable, thenBy.Item1, thenBy.Item2);
                }
            }

            return queryable;

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

            IOrderedQueryable<TEntity> ThenBy<TEntity>(IQueryable<TEntity> source, string orderByProperty, bool desc)
            {
                var command = desc ? "ThenByDescending" : "ThenBy";
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

        Queue<IQueryable<T>> ISkipAndTake<T>.AddSkipAndTake(IQueryable<T> queryable, int? maxTake,  out int skipped, out int took)
        {
            var result = new Queue<IQueryable<T>>();

            
            if (maxTake.HasValue && this.take > maxTake.Value)
            {
                var rounds = this.take / maxTake.Value;

                if (this.take % maxTake.Value != 0) rounds++; //add one more round to pickup the remainder

                for (var counter = 0; counter < rounds; counter++)
                {
                    var iteralSkip = this.skip + (counter * maxTake.Value);

                    var amountTaken = (counter * maxTake.Value);
                    var amountRemaining = this.take - amountTaken;
                    var iteralTake = amountRemaining > maxTake.Value ? maxTake.Value : amountRemaining;

                    result.Enqueue(queryable.Skip(iteralSkip).Take(iteralTake));
                }
            }
            else
            {
                queryable = queryable.Skip(this.skip);

                if (this.take > 0) queryable = queryable.Take(this.take);

                result.Enqueue(queryable);
            }

            skipped = this.skip;
            took = this.take;
            return result;
        }

        Queue<IQueryable<T>> ISkipAndTake<T>.AddSkipAndTake(IQueryable<T> queryable, out int skipped, out int took)
        {
            return (this as ISkipAndTake<T>).AddSkipAndTake(queryable, null, out skipped, out took);
        }
    }
}