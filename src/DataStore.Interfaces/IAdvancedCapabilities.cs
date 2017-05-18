namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using LowLevel;

    public interface IAdvancedCapabilities
    {
        Task<IEnumerable<T2>> ReadCommitted<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension) where T : class, IAggregate, new();

        Task<IEnumerable<T2>> ReadActiveCommitted<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension)
            where T : class, IAggregate, new();

        Task<dynamic> ReadCommittedById(Guid modelId);
    }
}