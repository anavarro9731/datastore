namespace DataStore.Interfaces.LowLevel.Permissions
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.Security;

    public interface IIdentityWithDatabasePermissions
    {
        // ReSharper disable once InconsistentNaming
        Guid id { get; set; } //lowercase to support entities persisted in cosmosdb, grr!

        string UserName { get; set; }

        bool HasPermission(IPermission permission);

        void AddPermission(IDatabasePermissionInstance permissionInstance);

        void RemovePermission(IDatabasePermissionInstance permissionInstance);

        IEnumerable<IDatabasePermissionInstance> PermissionInstances { get; }

    }
}