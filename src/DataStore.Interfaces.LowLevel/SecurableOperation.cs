namespace DataStore.Interfaces.LowLevel
{
    public class SecurableOperation
    {
        public static bool operator ==(SecurableOperation a, SecurableOperation b)
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

        public static bool operator !=(SecurableOperation a, SecurableOperation b) => !(a == b);

        internal SecurableOperation()
        {
            
        }
        
        public SecurableOperation(string permissionName)
        {
            PermissionName = permissionName;
        }

        public string PermissionName { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SecurableOperation)obj);
        }

        public override int GetHashCode() => PermissionName.GetHashCode();

        protected bool Equals(SecurableOperation other) => PermissionName.Equals(other.PermissionName);
    }
}
