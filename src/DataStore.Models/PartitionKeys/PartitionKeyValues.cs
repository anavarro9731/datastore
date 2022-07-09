namespace DataStore.Models.PartitionKeys
{
    using DataStore.Interfaces.LowLevel;

    public class PartitionKeyValues
    {

        public PartitionKeyValues(string partitionKey = null, Aggregate.HierarchicalPartitionKey partitionKeys = null)
        {
            PartitionKey = partitionKey;
            PartitionKeys = partitionKeys;
        }

        public string PartitionKey { get; }

        public Aggregate.HierarchicalPartitionKey PartitionKeys { get; }

        protected bool Equals(PartitionKeyValues other)
        {
            return PartitionKey == other.PartitionKey && Equals(PartitionKeys, other.PartitionKeys);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PartitionKeyValues)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((PartitionKey != null ? PartitionKey.GetHashCode() : 0) * 397) ^ (PartitionKeys != null ? PartitionKeys.GetHashCode() : 0);
            }
        }
    }
}