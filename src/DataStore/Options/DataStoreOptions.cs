namespace DataStore
{
    using System;

    public class DataStoreOptions 
    {
        public static DataStoreOptions Create()
        {
            return new DataStoreOptions();
        }

        private DataStoreOptions() { }

        public SecuritySettings Security { get; private set; }

        public VersionHistorySettings VersionHistory { get; private set; }

        public bool OptimisticConcurrency { get; private set; } = true;

        public DataStoreOptions WithSecurity(ScopeHierarchy scopeHierarchy)
        {
            Security = new SecuritySettings
            {
                ScopeHierarchy = scopeHierarchy
            };

            return this;
        }

        public DataStoreOptions DisableOptimisticConcurrency()
        {
            OptimisticConcurrency = false;
            return this;
        }

        public DataStoreOptions WithVersionHistory(Guid? unitOfWorkId)
        {
            VersionHistory = new VersionHistorySettings
            {
                UnitOfWorkId = unitOfWorkId
            };

            return this;
        }

        public class SecuritySettings
        {
            public ScopeHierarchy ScopeHierarchy { get; set; }
        }

        public class VersionHistorySettings
        {
            public Guid? UnitOfWorkId { get; set; }
        }
    }


}