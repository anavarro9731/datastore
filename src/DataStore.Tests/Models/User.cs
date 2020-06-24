namespace DataStore.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.LowLevel.Permissions;

    public class User : IIdentityWithDatabasePermissions
    {
        public User(Guid id, string userName)
        {
            this.id = id;
            UserName = userName;
        }

        public List<DatabasePermissionInstance> DatabasePermissions { get; set; } = new List<DatabasePermissionInstance>();

        public Guid id { get; set; }

        public string UserName { get; set; }
    }
}