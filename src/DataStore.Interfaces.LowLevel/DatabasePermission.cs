namespace DataStore.Interfaces.LowLevel
{
    public class DatabasePermission
    {
        public static bool operator ==(DatabasePermission a, DatabasePermission b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(DatabasePermission a, DatabasePermission b) => !(a == b);

        public DatabasePermission(string permissionName)
        {
            PermissionName = permissionName;
        }

        public string PermissionName { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DatabasePermission)obj);
        }

        public override int GetHashCode() => PermissionName.GetHashCode();

        protected bool Equals(DatabasePermission other) => PermissionName.Equals(other.PermissionName);
    }
}
