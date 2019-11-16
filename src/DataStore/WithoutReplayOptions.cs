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

        private readonly List<(string, bool)> orderByParameterList = new List<(string, bool)>();

        private readonly Queue<(string, bool)> thenByQueue = new Queue<(string, bool)>();

        private ContinuationToken currentContinuationToken;

        private ContinuationToken nextContinuationToken;

        private int? maxTake;

        private string orderByProperty;

        private bool orderDescending;

        public WithoutReplayOptions<T> ContinueFrom(ContinuationToken currentContinuationToken)
        {
            if (currentContinuationToken?.Value == null)
            {
                throw new Exception("The continuation token provided cannot be used since it's value is null");
            }

            this.currentContinuationToken = currentContinuationToken;

            return this;
        }

        public WithoutReplayOptions<T> Take(int take, ref ContinuationToken newContinuationToken)
        {
            this.maxTake = take;
            this.nextContinuationToken = newContinuationToken ?? throw new Exception("ContinuationToken cannot be null");

            return this;
        }

        public WithoutReplayOptions<T> OrderBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            this.orderByProperty = Objects.GetPropertyName(propertyRefExpr);
            this.orderDescending = descending;
            return this;
        }

        public WithoutReplayOptions<T> ThenBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            var propertyName = Objects.GetPropertyName(propertyRefExpr);
            this.thenByQueue.Enqueue((propertyName, descending));
            return this;
        }

        ContinuationToken IContinueAndTake<T>.CurrentContinuationToken => this.currentContinuationToken;

        int? IContinueAndTake<T>.MaxTake => this.maxTake;

        ContinuationToken IContinueAndTake<T>.NextContinuationToken { set => this.nextContinuationToken.Value = value.Value; }

        List<(string, bool)> IOrderBy<T>.OrderByParameters => this.orderByParameterList;

        IQueryable<T> IOrderBy<T>.AddOrderBy(IQueryable<T> queryable)
        {
            if (!string.IsNullOrEmpty(this.orderByProperty))
            {
                queryable = OrderBy(queryable, this.orderByProperty, this.orderDescending);

                this.orderByParameterList.Add((this.orderByProperty, this.orderDescending));

                while (this.thenByQueue.Count > 0)
                {
                    var thenBy = this.thenByQueue.Dequeue();

                    queryable = ThenBy(queryable, thenBy.Item1, thenBy.Item2);

                    this.orderByParameterList.Add(thenBy);
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
                        type, property.PropertyType
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
                        type, property.PropertyType
                    },
                    source.Expression,
                    Expression.Quote(orderByExpression));
                return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(resultExpression);
            }
        }
    }
}