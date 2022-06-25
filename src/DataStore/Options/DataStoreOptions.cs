namespace DataStore.Options
{
    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Models.PureFunctions;

    public class DataStoreOptions : IDataStoreOptions
    {
        public static DataStoreOptions Create() => new DataStoreOptions();

        private DataStoreOptions()
        {
        }

        public bool OptimisticConcurrency { get; private set; } = true;

        public SecuritySettings Security { get; private set; }

        public string UnitOfWorkId { get; internal set; } = DateTime.UtcNow.Ticks.ToString();

        public VersionHistorySettings VersionHistory { get; } = new VersionHistorySettings
        {
            Style = VersionHistorySettings.VersioningStyle.AggregateHeaderInfoOnly
        };

        public DataStoreOptions DisableOptimisticConcurrency()
        {
            OptimisticConcurrency = false;
            return this;
        }

        public DataStoreOptions EnableFullVersionHistory()
        {
            VersionHistory.Style = VersionHistorySettings.VersioningStyle.CompleteCopyOfAllAggregateVersions;
            return this;
        }

        public DataStoreOptions SpecifyUnitOfWorkId(Guid unitOfWorkId)
        {
            Guard.Against(unitOfWorkId == Guid.Empty, "If you specify a unit-of-work ID it cannot be empty");
            UnitOfWorkId = unitOfWorkId.ToString();
            return this;
        }

        public DataStoreOptions WithSecurity(ScopeHierarchy scopeHierarchy = null)
        {
            Security = new SecuritySettings
            {
                ScopeHierarchy = scopeHierarchy
            };

            return this;
        }

        

    }
}