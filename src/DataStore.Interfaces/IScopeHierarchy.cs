namespace DataStore.Interfaces
{
    #region

    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel.Permissions;

    #endregion

    public interface IScopeHierarchy
    {
        Task<IEnumerable<IHaveScope>> GetDataAndPermissionScopeIntersection(
            List<IHaveScope> dataWithScope,
            List<AggregateReference> userPermissionScopes,
            IDataStore dataStore);
    }
}