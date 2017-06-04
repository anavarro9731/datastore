namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.LowLevel;
    using ServiceApi.Interfaces.LowLevel.Messages;

    public interface ITestHarness
    {
        DataStore DataStore { get; }

        List<IMessage> AllMessages { get; }

        //add to the underlying db directly (i.e. not via datastore)
        void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new();

        //query the underlying db directly (i.e. not via datastore)
        IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null)
            where T : class, IAggregate, new();
    }
}