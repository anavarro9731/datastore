namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IAdvancedCapabilities
    {
        Task<IEnumerable<T2>> ReadCommitted<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension) where T : IAggregate;
        Task<IEnumerable<T2>> ReadActiveCommitted<T, T2>(Func<IQueryable<T>, IQueryable<T2>> queryableExtension) where T : IAggregate;
        Task<dynamic> ReadCommittedById(Guid modelId);
    }
}