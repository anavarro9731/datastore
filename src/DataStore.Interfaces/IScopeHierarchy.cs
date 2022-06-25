namespace DataStore.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface IScopeHierarchy
    {
        Task<IEnumerable<IHaveScope>> GetDataAndPermissionScopeIntersection(
            List<IHaveScope> dataWithScope,
            List<AggregateReference> userPermissionScopes,
            IDataStore dataStore);
    }
}