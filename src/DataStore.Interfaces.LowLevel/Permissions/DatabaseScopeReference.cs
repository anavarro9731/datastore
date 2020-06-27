namespace DataStore.Interfaces.LowLevel.Permissions
{
    using System;

    public class DatabaseScopeReference : IEquatable<DatabaseScopeReference>
    {
        public static bool operator ==(DatabaseScopeReference a, DatabaseScopeReference b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null) return false;

            // Return true if the fields match:
            return a.PropertiesAreEqual(b);
        }

        public static bool operator !=(DatabaseScopeReference a, DatabaseScopeReference b) => !(a == b);

        public DatabaseScopeReference()
        {
            //serialiser
        }

        public DatabaseScopeReference(Guid idOfScopeObject, string typeOfOwner, string scopeObjectDebugId = null)
        {
            ScopeObjectId = idOfScopeObject;
            ScopeObjectType = typeOfOwner;
            ScopeObjectDebugId = scopeObjectDebugId;
        }

        public string ScopeObjectDebugId { get; set; }

        public Guid ScopeObjectId { get; set; }

        public string ScopeObjectType { get; set; }

        public override bool Equals(object obj)
        {
            //if the object passed is null return false;
            if (ReferenceEquals(null, obj)) return false;

            //if the objects are the same instance return true
            if (ReferenceEquals(this, obj)) return true;

            //if the objects are of different types return false
            if (obj.GetType() != GetType()) return false;

            //check on property equality
            return PropertiesAreEqual((DatabaseScopeReference)obj);
        }

        public override int GetHashCode()
        {
            var hash = 13;
            hash = hash * 7 + ScopeObjectId.GetHashCode();
            hash = hash * 7 + ScopeObjectType.GetHashCode();
            return hash;
        }

        bool IEquatable<DatabaseScopeReference>.Equals(DatabaseScopeReference other) => Equals(other);

        protected bool PropertiesAreEqual(DatabaseScopeReference other) =>
            ScopeObjectId.Equals(other.ScopeObjectId) && ScopeObjectType.Equals(other.ScopeObjectType);
    }
}