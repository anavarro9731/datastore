namespace DataStore.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using CircuitBoard.Security;
    using global::DataStore.Interfaces.LowLevel.Permissions;

    public class User : IIdentityWithDatabasePermissions
    {
        public User(Guid id, string userName)
        {
            this.id = id;
            UserName = userName;
        }

        public void RemovePermission(IDatabasePermissionInstance permissionInstance)
        {
            this.Permissions.Remove(permissionInstance);
        }

        public Guid id { get; set; }

        private List<IDatabasePermissionInstance> Permissions { get; } = new List<IDatabasePermissionInstance>();

        public string UserName { get; set; }

        public IEnumerable<IDatabasePermissionInstance> PermissionInstances => Permissions;

        public bool HasPermission(IPermission permission)
        {
            var count = Permissions.Count(p => p.Id == permission.Id);
            if (count == 1)
            {
                return true;
            }

            if (count > 1)
            {
                throw new Exception("User has the same databasePermission twice");
            }

            return false;
        }

        public void AddPermission(IDatabasePermissionInstance permissionInstance)
        {
            this.Permissions.Add(permissionInstance);
        }
    }

    public class DatabasePermissionInstance : DatabasePermission, IDatabasePermissionInstance
    {
        public DatabasePermissionInstance(DatabasePermission databasePermission, List<DatabaseScopeReference> scopeReferences)
            : base(databasePermission.DisplayOrder, databasePermission.Id, databasePermission.PermissionName, databasePermission.PermissionRequiredToAdministerThisPermission)
        {
            ScopeReferences = scopeReferences;
        }

        public List<DatabaseScopeReference> ScopeReferences { get; set; }
    }

    public class DatabasePermission : IPermission
    {
        public DatabasePermission(Guid id, string permissionName)
        {
            Id = id;
            PermissionName = permissionName;
        }

        public DatabasePermission(int displayOrder, Guid id, string permissionName, Guid permissionRequiredToAdministerThisPermission)
        {
            DisplayOrder = displayOrder;
            Id = id;
            PermissionName = permissionName;
            PermissionRequiredToAdministerThisPermission = permissionRequiredToAdministerThisPermission;
        }

        public int DisplayOrder { get; set; }

        public Guid Id { get; set; }

        public string PermissionName { get; set; }

        public Guid PermissionRequiredToAdministerThisPermission { get; set; }
    }
}