namespace DataStore.Interfaces.LowLevel.Permissions
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.Security;

    public interface IIdentityWithDatabasePermissions : IHaveAUniqueId
    {
        bool HasPermission(IPermission permission);

        void AddPermission(IDatabasePermissionInstance permissionInstance);

        void RemovePermission(IDatabasePermissionInstance permissionInstance);

        IEnumerable<IDatabasePermissionInstance> PermissionInstances { get; }

    }
}