namespace DataStore.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Models.PartitionKeys;

    [PartitionKey__Shared]
    public class User : IIdentityWithDatabasePermissions
    {
        public User(Guid id, string userName)
        {
            this.id = id;
            UserName = userName;
        }

        public List<DatabasePermission> DatabasePermissions { get; set; } = new List<DatabasePermission>();

        public Guid id { get; set; }

        public string UserName { get; set; }
    }
}