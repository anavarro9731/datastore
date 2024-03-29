﻿namespace DataStore.Models.Messages
{
    #region

    using System;
    using System.Linq.Expressions;
    using DataStore.Interfaces.Operations;
    using DataStore.Interfaces.Options;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;

    #endregion

    public class AggregateCountedOperation<T> : IDataStoreCountFromQueryable<T>
    {
        public AggregateCountedOperation(string methodCalled, Expression<Func<T, bool>> predicate = null, IOptionsLibrarySide queryOptions = null)
        {
            MethodCalled = methodCalled;
            TypeName = typeof(T).FullName;
            Predicate = predicate;
            QueryOptions = queryOptions;
            Created = DateTime.UtcNow;
        }

        public DateTime Created { get; set; }

        public string MethodCalled { get; set; }

        public Expression<Func<T, bool>> Predicate { get; set; }

        public IOptionsLibrarySide QueryOptions { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }

        public string TypeName { get; set; }
    }
}