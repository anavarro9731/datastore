namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataStore.Interfaces.LowLevel;

    public interface ITestHarness
    {
        IDataStore DataStore { get; }

        //add to the underlying db directly (i.e. not via datastore)
        void AddItemDirectlyToUnderlyingDb<T>(T aggregate) where T : class, IAggregate, new();

        //query the underlying db directly (i.e. not via datastore)
        List<T> QueryUnderlyingDbDirectly<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new();
    }
}