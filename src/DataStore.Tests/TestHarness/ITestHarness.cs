using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataStore.Tests.TestHarness
{
    using Interfaces;
    using Interfaces.Events;

    public interface ITestHarness
    {
        DataStore DataStore { get; }

        List<IDataStoreEvent> Events { get; }

        //add to the underlying db directly (i.e. not via datastore)
        Task AddToDatabase<T>(T aggregate) where T : IAggregate;

        //query the underlying db directly (i.e. not via datastore)
        Task<IEnumerable<T>> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : IHaveSchema, IHaveAUniqueId;
    }
}