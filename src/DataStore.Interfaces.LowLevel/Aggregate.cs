namespace DataStore.Interfaces.LowLevel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DataStore.Interfaces.LowLevel.Permissions;
    using Newtonsoft.Json;

    /// <summary>
    ///     This abstract class is here for convenience, so as not to clutter up
    ///     your classes with property implementations.
    ///     The interface is what is used by datastore because
    ///     the benefit of an interface over an abstract class is you can't sneak logic into it.
    ///     e.g. property which may not serialize reliably, or constructor logic which affects field values.
    ///     Furthermore, if you expose add any logic to the base class, even that which is
    ///     serialisation safe, if a client has models assemblies each with a different version
    ///     of this logic, your code could start producing unexpected results.
    ///     So, no logic kind in these abstract classes.
    /// </summary>
    public abstract class Aggregate : Entity, IAggregate, IEtagUpdated
    {
        protected Aggregate()
        {
            /* Properties are set here (or as defaults) when they could be set in Create
             because if you set them there you can't determine on a boolean if they wanted
             them to be false unless we make them nullable which means they have to be accessed
             with .Value all the time. While it might seem odd to create an item that is not active
             tests in particular do this often.             
            */
            Active = true;
        }

        public bool Active { get; set; }

        public string Etag { get; set; }

        public DateTime Modified { get; set; }

        public double ModifiedAsMillisecondsEpochTime { get; set; }

        public string PartitionKey { get; set; }

        public HierarchicalPartitionKey PartitionKeys { get; set; }

        public bool ReadOnly { get; set; }

        public List<AggregateReference> ScopeReferences
        {
            get
            {
                var propertiesWithScope = GetType().GetProperties().Where(p => p.GetCustomAttribute<ScopeObjectReferenceAttribute>() != null);

                var scopeReferences = new List<AggregateReference>();
                foreach (var propertyInfo in propertiesWithScope)
                {
                    var attribute = propertyInfo.GetCustomAttribute<ScopeObjectReferenceAttribute>();
                    if (attribute != null && propertyInfo.GetValue(this) != null)
                    {
                        if (propertyInfo.PropertyType == typeof(Guid))
                        {
                            scopeReferences.Add(new AggregateReference((Guid)propertyInfo.GetValue(this), attribute.FullTypeName));
                        }
                        else if (propertyInfo.PropertyType == typeof(Guid?))
                        {
                            var nullableGuid = (Guid?)propertyInfo.GetValue(this);
                            if (nullableGuid.HasValue)
                            {
                                scopeReferences.Add(new AggregateReference(nullableGuid.Value, attribute.FullTypeName));
                            }
                        }
                        else if (typeof(IEnumerable<Guid>).IsAssignableFrom(propertyInfo.PropertyType))
                        {
                            var guids = (IEnumerable<Guid>)propertyInfo.GetValue(this);
                            foreach (var guid in guids) scopeReferences.Add(new AggregateReference(guid, attribute.FullTypeName));
                        }
                        else if (typeof(IEnumerable<Guid?>).IsAssignableFrom(propertyInfo.PropertyType))
                        {
                            var guids = (IEnumerable<Guid?>)propertyInfo.GetValue(this);
                            foreach (var guid in guids)
                                if (guid.HasValue)
                                {
                                    scopeReferences.Add(new AggregateReference(guid.Value, attribute.FullTypeName));
                                }
                        }
                    }

                    /* you always get yourself as a scope as well, so by default any document is scoped only by itself
                     and if you are using database security you would need to give a permissions specific to this document
                     or attach a wider scope to it in order for it to be accessible by a user */
                    scopeReferences.Add(new AggregateReference(id, Schema));
                }

                return scopeReferences;
            }
        }

        public List<AggregateVersionInfo> VersionHistory { get; set; } = new List<AggregateVersionInfo>();

        //* Json.NET ignores explicit implementations and we kind of want to hide this anyway
        Action<string> IEtagUpdated.EtagUpdated { get; set; }

        public string GetLongPartitionedId() => string.IsNullOrWhiteSpace(PartitionKey) ? PartitionKeys.ToSyntheticKey() : PartitionKey;
        
        public class AggregateVersionInfo
        {
            public Guid? AggegateHistoryItemId { get; set; }

            public int ChangeCount { get; set; }

            public int CommitBatch { get; set; }

            public DateTime Timestamp { get; set; }

            public string UnitOfWorkId { get; set; }
        }

        public class HierarchicalPartitionKey
        {
            public List<string> AsList() =>
                new List<string>()
                {
                    Key1, Key2, Key3
                };

            public string ToSyntheticKey()
            {
                return Key1 + Key2 + Key3;
            }
            
            
            
            public string Key1 { get; set; }

            public string Key2 { get; set; }

            public string Key3 { get; set; }

            protected bool Equals(HierarchicalPartitionKey other)
            {
                return Key1 == other.Key1 && Key2 == other.Key2 && Key3 == other.Key3;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((HierarchicalPartitionKey)obj);
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
}