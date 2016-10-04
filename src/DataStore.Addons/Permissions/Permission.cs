namespace DataStore.Infrastructure.Impl.Permissions
{
    using System;

    public class Permission
    {
        public Permission(
            Guid id, 
            string permissionName, 
            Type[] permissionScopeObjectType, 
            Guid permissionRequiredToAdministerThisPermission, 
            int displayOrder = 99)
        {
            this.Id = id;
            this.PermissionName = permissionName;
            this.PermissionScopeObjectType = permissionScopeObjectType;
            this.PermissionRequiredToAdministerThisPermission = permissionRequiredToAdministerThisPermission;
            this.DisplayOrder = displayOrder;
        }

        public int DisplayOrder { get; set; }

        public Guid Id { get; set; }

        public string PermissionName { get; set; }

        public Guid PermissionRequiredToAdministerThisPermission { get; set; }

        public Type[] PermissionScopeObjectType { get; set; }

        // add this code to class ThreeDPoint as defined previously
        public static bool operator ==(Permission a, Permission b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(Permission a, Permission b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((Permission)obj);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        protected bool Equals(Permission other)
        {
            return this.Id.Equals(other.Id);
        }
    }
}