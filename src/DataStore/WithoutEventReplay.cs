namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Options;

    public class WithoutEventReplay : IWithoutEventReplay
    {
        private readonly ControlFunctions controlFunctions;

        private readonly IDocumentRepository dataStoreConnection;

        private readonly IDataStoreOptions dataStoreOptions;

        private readonly IMessageAggregator messageAggregator;

        public WithoutEventReplay(
            IDocumentRepository dataStoreConnection,
            IMessageAggregator messageAggregator,
            ControlFunctions controlFunctions,
            IDataStoreOptions dataStoreOptions)
        {
            this.dataStoreConnection = dataStoreConnection;
            this.messageAggregator = messageAggregator;
            this.controlFunctions = controlFunctions;
            this.dataStoreOptions = dataStoreOptions;
        }

        //* Count (no options)
        public async Task<int> Count<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new()
        {
            var result = await this.messageAggregator.CollectAndForward(new AggregateCountedOperation<T>(nameof(Count), predicate))
                                   .To(this.dataStoreConnection.CountAsync).ConfigureAwait(false);

            return result;
        }

        //* CountActive (no options)
        public async Task<int> CountActive<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new()
        {
            predicate = predicate == null ? a => a.Active : predicate.And(a => a.Active);

            var result = await this.messageAggregator.CollectAndForward(new AggregateCountedOperation<T>(nameof(CountActive), predicate))
                                   .To(this.dataStoreConnection.CountAsync).ConfigureAwait(false);

            return result;
        }

        //* Read
        public Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate = null, Action<WithoutReplayOptionsClientSide<T>> setOptions = null)
            where T : class, IAggregate, new()
        {
            return Read<T, DefaultWithoutReplayOptions<T>, T>(null, predicate, setOptions);
        }

        public Task<IEnumerable<R>> Read<T, R>(
            Expression<Func<T, R>> map,
            Expression<Func<T, bool>> predicate = null,
            Action<WithoutReplayOptionsClientSide<R>> setOptions = null) where T : class, IAggregate, new() where R : class, IAggregate, new()
        {
            return Read<T, DefaultWithoutReplayOptions<R>, R>(map, predicate, setOptions);
        }

        public async Task<IEnumerable<R>> Read<T, O, R>(Expression<Func<T, R>> map = null, Expression<Func<T, bool>> predicate = null, Action<O> setOptions = null)
            where T : class, IAggregate, new() where O : WithoutReplayOptionsClientSide<R>, new() where R : class, IAggregate, new()
        {
            /* T is Aggregate, R is a possible Projection */
            WithoutReplayOptionsLibrarySide<R> options;
            if (setOptions == null)
            {
                options = new DefaultWithoutReplayOptions<R>();
            }
            else
            {
                options = new O().Op(setOptions);
            }

            if (map != null)
            {
                map = Expressions.Combine(
                    map,
                    aggregate => new R
                    {
                        Active = aggregate.Active,
                        id = aggregate.id,
                        Created = aggregate.Created,
                        CreatedAsMillisecondsEpochTime = aggregate.CreatedAsMillisecondsEpochTime,
                        Modified = aggregate.Modified,
                        ModifiedAsMillisecondsEpochTime = aggregate.ModifiedAsMillisecondsEpochTime,
                        ReadOnly = aggregate.ReadOnly,
                        VersionHistory = aggregate.VersionHistory,
                        Schema = aggregate.Schema
                    });
            }

            IQueryable<R> queryableR = null;
            IQueryable<T> queryableT = null;
            if (predicate == null)
            {
                queryableR = map == null ? this.dataStoreConnection.CreateQueryable<R>(options) : this.dataStoreConnection.CreateQueryable<T>(options).Select(map);
            }
            else if (map == null)
            {
                queryableT = this.dataStoreConnection.CreateQueryable<T>(options).Where(predicate);
            }
            else
            {
                queryableR = this.dataStoreConnection.CreateQueryable<T>(options).Where(predicate).Select(map);
            }

            if (queryableR != null)
            {
                var resultsR = await ExecuteQuery(queryableR);
                resultsR = await AuthoriseData(resultsR);
                return resultsR;
            }

            var resultsT = await ExecuteQuery(queryableT);
            resultsT = await AuthoriseData(resultsT);
            return resultsT.Cast<R>(); //* T is R when R not supplied

            async Task<IEnumerable<T2>> AuthoriseData<T2>(IEnumerable<T2> results) where T2 : class, IAggregate, new()
            {
                var applySecurity = this.dataStoreOptions.Security != null && options.Identity != null;
                var bypassSecurityEnabledForThisAggregate = typeof(T).GetCustomAttributes(false).ToList().Exists(x => x.GetType() == typeof(BypassSecurity));
                var bypassSecurityEnabledForThisCall = options.BypassSecurity;
                
                if (applySecurity && !bypassSecurityEnabledForThisAggregate && !bypassSecurityEnabledForThisCall)
                {
                    var hasPii = typeof(T).GetProperties().Any(x => x.GetCustomAttribute(typeof(PIIAttribute), false) != null);
                    if (hasPii)
                    {
                        return await this.controlFunctions.AuthoriseData(results, SecurableOperations.READPII, options.Identity).ConfigureAwait(false);
                    }

                    return await this.controlFunctions.AuthoriseData(results, SecurableOperations.READ, options.Identity).ConfigureAwait(false);
                }

                return results;
            }

            async Task<IEnumerable<T1>> ExecuteQuery<T1>(IQueryable<T1> query) where T1 : class, IAggregate, new()
            {
                return await this.messageAggregator.CollectAndForward(new AggregatesQueriedOperation<T1>(nameof(Read), query, options))
                                 .To(this.dataStoreConnection.ExecuteQuery).ConfigureAwait(false);
            }
        }

        //* ReadActive
        public Task<IEnumerable<T>> ReadActive<T>(Expression<Func<T, bool>> predicate = null, Action<WithoutReplayOptionsClientSide<T>> setOptions = null)
            where T : class, IAggregate, new()
        {
            return Read(predicate == null ? a => a.Active : predicate.And(a => a.Active), setOptions);
        }

        public Task<IEnumerable<R>> ReadActive<T, R>(
            Expression<Func<T, R>> map,
            Expression<Func<T, bool>> predicate = null,
            Action<WithoutReplayOptionsClientSide<R>> setOptions = null) where T : class, IAggregate, new() where R : class, IAggregate, new()
        {
            return Read(map, predicate == null ? a => a.Active : predicate.And(a => a.Active), setOptions);
        }

        public Task<IEnumerable<R>> ReadActive<T, O, R>(Expression<Func<T, R>> map, Expression<Func<T, bool>> predicate, Action<O> setOptions)
            where T : class, IAggregate, new() where O : WithoutReplayOptionsClientSide<R>, new() where R : class, IAggregate, new()
        {
            return Read(map, predicate == null ? a => a.Active : predicate.And(a => a.Active), setOptions);
        }

        //* ReadById (no map)
        public async Task<T> ReadById<T, O>(Guid modelId, Action<O> setOptions = null)
            where T : class, IAggregate, new() where O : WithoutReplayOptionsClientSide<T>, new()
        {
            WithoutReplayOptionsLibrarySide<T> options = setOptions == null ? new O() : new O().Op(setOptions);

            var result = await this.messageAggregator.CollectAndForward(new AggregateQueriedByIdOperation(nameof(ReadById), modelId))
                                   .To(this.dataStoreConnection.GetItemAsync<T>).ConfigureAwait(false);

            var applySecurity = this.dataStoreOptions.Security != null && options.Identity != null;
            var bypassSecurityEnabledForThisAggregate = typeof(T).GetCustomAttributes(false).ToList().Exists(x => x.GetType() == typeof(BypassSecurity));
            var bypassSecurityEnabledForThisCall = options.BypassSecurity;
                
            if (applySecurity && !bypassSecurityEnabledForThisAggregate && !bypassSecurityEnabledForThisCall)
            {
                result = await this.controlFunctions.AuthoriseDatum(result, SecurableOperations.READ, options.Identity).ConfigureAwait(false);
            }

            return result;
        }

        public Task<T> ReadById<T>(Guid modelId, Action<WithoutReplayOptionsClientSide<T>> setOptions = null) where T : class, IAggregate, new()
        {
            return ReadById<T, DefaultWithoutReplayOptions<T>>(modelId, setOptions);
        }
    }

    internal static class Expressions
    {
        internal static Expression<Func<TSource, TDestination>> Combine<TSource, TDestination>(params Expression<Func<TSource, TDestination>>[] selectors)
        {
            var param = Expression.Parameter(typeof(TSource), "x");
            return Expression.Lambda<Func<TSource, TDestination>>(
                Expression.MemberInit(
                    Expression.New(typeof(TDestination).GetConstructor(Type.EmptyTypes)),
                    from selector in selectors
                    let replace = new ParameterReplaceVisitor(selector.Parameters[0], param)
                    from binding in ((MemberInitExpression)selector.Body).Bindings.OfType<MemberAssignment>()
                    select Expression.Bind(binding.Member, replace.VisitAndConvert(binding.Expression, "Combine"))),
                param);
        }

        internal class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression from, to;

            public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
            {
                this.from = from;
                this.to = to;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == this.from ? this.to : base.VisitParameter(node);
            }
        }
    }
}