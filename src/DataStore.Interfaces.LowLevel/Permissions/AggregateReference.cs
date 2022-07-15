namespace DataStore.Interfaces.LowLevel.Permissions
{
    #region

    using System;
    using Newtonsoft.Json;

    #endregion

    public class AggregateReference : IEquatable<AggregateReference>
    {
        public static bool operator ==(AggregateReference a, AggregateReference b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null) return false;

            // Return true if the fields match:
            return a.PropertiesAreEqual(b);
        }

        public static bool operator !=(AggregateReference a, AggregateReference b) => !(a == b);

        [JsonConstructor]
        internal AggregateReference()
        {
        }

        public AggregateReference(Guid idOfAggregate, string typeOfOwner = null, string debugId = null)
        {
            AggregateId = idOfAggregate;
            AggregateType = typeOfOwner;  //* here for debugging and logging use, would normally be the .net type
            DebugId = debugId; //* friendly id providing further granularity in debugging and logging, eg. "company xyz"
        }

        public string DebugId { get; set; }

        public Guid AggregateId { get; set; }

        public string AggregateType { get; set; }

        public override bool Equals(object obj)
        {
            //if the object passed is null return false;
            if (ReferenceEquals(null, obj)) return false;

            //if the objects are the same instance return true
            if (ReferenceEquals(this, obj)) return true;

            //if the objects are of different types return false
            if (obj.GetType() != GetType()) return false;

            //check on property equality
            return PropertiesAreEqual((AggregateReference)obj);
        }

        public override int GetHashCode()
        {
            var hash = 13;
            hash = hash * 7 + AggregateId.GetHashCode();
            //* don't include typename and debugId as their optional
            return hash;
        }

        bool IEquatable<AggregateReference>.Equals(AggregateReference other) => Equals(other);

        protected bool PropertiesAreEqual(AggregateReference other) => AggregateId.Equals(other.AggregateId);
    }
}
