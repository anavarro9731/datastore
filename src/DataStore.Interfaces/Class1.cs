using System;
using System.Collections.Generic;
using System.Text;

namespace DataStore.Interfaces
{
    public class DataStoreOptions
    {
        public Guid? UnitOfWorkId { get; set; }

        public bool UseVersionHistory { get; set; }
    }
}
