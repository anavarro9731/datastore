namespace DataStore.Interfaces
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface IDataStoreOptions
    {
        bool OptimisticConcurrency { get; }

        SecuritySettings Security { get; }

        string UnitOfWorkId { get; }

        VersionHistorySettings VersionHistory { get; }
        
    }
    
    public class SecuritySettings 
    {
        public IScopeHierarchy ScopeHierarchy { get; set; }
        public IIdentityWithDatabasePermissions SecuredFor { get; set; }
    }

    public class VersionHistorySettings 
    {
        public VersioningStyle Style { get; set; }
            
        public enum VersioningStyle
        {
            AggregateHeaderInfoOnly,

            CompleteCopyOfAllAggregateVersions
        }
    }
}