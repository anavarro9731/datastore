namespace DataStore.Interfaces.LowLevel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DataStore.Interfaces.LowLevel.Permissions;

    /// <summary>
    ///     This abstract class is here for convenience, so as not to clutter up
    ///     your classes with property implementations.
    ///     The interface is what is used by datastore because
    ///     the benefit of an interface over an abstract class is you can't sneak logic into it.
    ///     e.g. property which may not serialize reliably, or constructor logic which affects field values.
    ///     Furthermore, if you expose add any logic to the base class, even that which is
    ///     serialisation safe, if a client has models assemblies each with a different version
    ///     of this logic, your code could start producing unexpected results.
    ///     So, NO LOGIC of any kind in these abstract classes.
    /// </summary>
    public abstract class Aggregate : Entity, IAggregate, IEtagUpdated
    {
        public const string PartitionKeyValue = "shared";

        protected Aggregate()
        {
            //Properties are set here (or as defaults) when they could be set in Create because a lot of the tests which
            //create classes without create() depend on these defaults and it is a significant convenience for it
            //to be set correctly by default.
            Active = true;
        }

        public bool Active { get; set; }

        public string Etag { get; set; }

        public DateTime Modified { get; set; }

        public double ModifiedAsMillisecondsEpochTime { get; set; }

        public string PartitionKey { get; set; } = PartitionKeyValue;

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

        public class AggregateVersionInfo
        {
            public Guid? AggegateHistoryItemId { get; set; }

            public int ChangeCount { get; set; }

            public int CommitBatch { get; set; }

            public DateTime Timestamp { get; set; }

            public string UnitOfWorkId { get; set; }
        }
    }
}