namespace DataStore.Models
{
    using System;
    using Interfaces;

    public class ScopeReference : IScopeReference, IEquatable<ScopeReference>
    {
        public ScopeReference(Guid idOfScope, string typeOfOwner, DateTime addedOn)
        {
            ScopeObjectId = idOfScope;
            ScopeObjectType = typeOfOwner;
            ScopeReferenceCreatedOn = addedOn;
        }

        public Guid ScopeObjectId { get; }
        public string ScopeObjectType { get; }
        public DateTime ScopeReferenceCreatedOn { get; }

        #region "Equality"

        bool IEquatable<ScopeReference>.Equals(ScopeReference other) => Equals(other);

        public static bool operator ==(ScopeReference a, ScopeReference b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null)
                return false;

            // Return true if the fields match:
            return a.PropertiesAreEqual(b);
        }

        public static bool operator !=(ScopeReference a, ScopeReference b)
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
            return PropertiesAreEqual((ScopeReference)obj);
        }

        public override int GetHashCode() 
        {
            int hash = 13;
            hash = (hash* 7) + ScopeObjectId.GetHashCode();
            hash = (hash* 7) + ScopeObjectType.GetHashCode();
            return hash;
        }

        protected bool PropertiesAreEqual(ScopeReference other) =>
           this.ScopeObjectId.Equals(other.ScopeObjectId) && ScopeObjectType.Equals(other.ScopeObjectType);
        
        #endregion
    }
}