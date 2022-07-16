namespace DataStore.Interfaces.LowLevel
{
    using System.Collections.Generic;

    public sealed class HierarchicalPartitionKey
    {
        public List<string> AsList() =>
            new List<string>()
            {
                Key1, Key2, Key3
            };

        public bool IsEmpty()
        {
            return this == new HierarchicalPartitionKey();
        }

        public string ToSyntheticKeyString()
        {
            
            return  Key1 + Key2 + Key3;
        }

        public string Key1 { get; set; } 

        public string Key2 { get; set; } 

        public string Key3 { get; set; }

        private bool PropertiesAreEqual(HierarchicalPartitionKey other)
        {
            return Key1 == other.Key1 && Key2 == other.Key2 && Key3 == other.Key3;
        }
            
        public static bool operator ==(HierarchicalPartitionKey left, HierarchicalPartitionKey right)
        {
            if (left is null)
            {
                return right is null;
            }
            return Equals(left, right);
        }

        public static bool operator !=(HierarchicalPartitionKey left, HierarchicalPartitionKey right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return PropertiesAreEqual((HierarchicalPartitionKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Key1 != null ? Key1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Key2 != null ? Key2.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Key3 != null ? Key3.GetHashCode() : 0);
                return hashCode;
            }
        }


    }
}