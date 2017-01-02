namespace DataStore.Addons.Permissions
{
    using System;

    public class Permission : IEquatable<Permission>
    {
        public Permission(
            Guid id,
            string permissionName,
            Guid permissionRequiredToAdministerThisPermission,
            int displayOrder = 99)
        {
            Id = id;
            PermissionName = permissionName;
            PermissionRequiredToAdministerThisPermission = permissionRequiredToAdministerThisPermission;
            DisplayOrder = displayOrder;
        }

        public int DisplayOrder { get; set; }

        public Guid Id { get; set; }

        public string PermissionName { get; set; }

        public Guid PermissionRequiredToAdministerThisPermission { get; set; }

        #region "Equality"

        bool IEquatable<Permission>.Equals(Permission other) => Equals(other);

        public static bool operator ==(Permission a, Permission b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if ((object) a == null || (object) b == null)
                return false;

            // Return true if the fields match:
            return a.PropertiesAreEqual(b);
        }

        public static bool operator !=(Permission a, Permission b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            //if the object passed is null return false;
            if (ReferenceEquals(null, obj))
                return false;

            //if the objects are the same instance return true
            if (ReferenceEquals(this, obj))
                return true;

            //if the objects are of different types return false
            if (obj.GetType() != GetType())
                return false;

            //check on property equality
            return PropertiesAreEqual((Permission) obj);
        }

        public override int GetHashCode() => Id.GetHashCode();

        protected bool PropertiesAreEqual(Permission other) => Id.Equals(other.Id);

        #endregion
    }
}