namespace DataStore.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard.Permissions;

    public class User : IIdentityWithPermissions
    {
        public User(Guid id, string userName)
        {
            this.id = id;
            UserName = userName;
        }

        public void RemovePermission(IPermissionInstance permissionInstance)
        {
            this.Permissions.Remove(permissionInstance);
        }

        public Guid id { get; set; }

        private List<IPermissionInstance> Permissions { get; } = new List<IPermissionInstance>();

        public string UserName { get; set; }

        public IList<IPermissionInstance> PermissionInstances { get; }

        public bool HasPermission(IPermission permission)
        {
            var count = Permissions.Count(p => p.Id == permission.Id);
            if (count == 1)
            {
                return true;
            }

            if (count > 1)
            {
                throw new Exception("User has the same permission twice");
            }

            return false;
        }

        public void AddPermission(IPermissionInstance permissionInstance)
        {
            this.Permissions.Add(permissionInstance);
        }
    }

    public class PermissionInstance : Permission, IPermissionInstance
    {
        public PermissionInstance(Permission permission, List<ScopeReference> scopeReferences)
            : base(permission.DisplayOrder, permission.Id, permission.PermissionName, permission.PermissionRequiredToAdministerThisPermission)
        {
            ScopeReferences = scopeReferences;
        }

        public List<ScopeReference> ScopeReferences { get; set; }
    }

    public class Permission : IPermission
    {
        public Permission(Guid id, string permissionName)
        {
            Id = id;
            PermissionName = permissionName;
        }

        public Permission(int displayOrder, Guid id, string permissionName, Guid permissionRequiredToAdministerThisPermission)
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