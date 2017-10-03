namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.Messages;
    using global::DataStore.Interfaces.LowLevel;

    public interface ITestHarness
    {
        List<IMessage> AllMessages { get; }

        DataStore DataStore { get; }

        //add to the underlying db directly (i.e. not via datastore)
        void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new();

        //query the underlying db directly (i.e. not via datastore)
        IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new();
    }
}