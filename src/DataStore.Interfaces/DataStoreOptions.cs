namespace DataStore.Interfaces
{
    using System;

    public class DataStoreOptions
    {
        public Guid? UnitOfWorkId { get; set; }

        public bool UseVersionHistory { get; set; }
    }
}