namespace DataStore.Models.PureFunctions.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.PartitionKeys;

    public static class IAggregateExt
    {


        internal static void ForcefullySetMandatoryPropertyValues<T>(this T newObject, bool readOnly, bool useHierarchicalPartitionKeys) where T : class, IAggregate, new()
        {
            
            newObject.Schema =
                typeof(T).FullName; //will be defaulted by Aggregate but needs to be forced as it is subject since the setter is accessible for serialisation purposes
            newObject.ReadOnly = readOnly;

            WalkGraphAndUpdateEntityMeta(newObject);
            
            var keys = PartitionKeyHelpers.GetKeysForNewModel(newObject, useHierarchicalPartitionKeys);
            newObject.PartitionKey = keys.PartitionKey;
            newObject.PartitionKeys = keys.PartitionKeys;
           
            newObject.VersionHistory = new List<Aggregate.AggregateVersionInfo>(); //-set again in commitchanges, still best not to allow any invalid state
            newObject.Modified = newObject.Created;
            newObject.ModifiedAsMillisecondsEpochTime = newObject.CreatedAsMillisecondsEpochTime;
        }

        internal static void WalkGraphAndUpdateEntityMeta(this object current)
        {
            if (current != null)
            {
                var t = current.GetType();

                foreach (var p in t.GetProperties())
                    if (p.Name == nameof(IEntity.id))
                    {
                        //set an id for any entity in the tree if it doesn't have one
                        //regardless of whether it is the aggregate or a child entity
                        //in many cases this will already be done in the app code
                        if ((Guid)p.GetValue(current, null) == default)
                        {
                            p.SetValue(current, Guid.NewGuid(), null);
                        }
                    }
                    else if (p.Name == nameof(IEntity.Created))
                    {
                        //set created datetime if this is null
                        if ((DateTime)p.GetValue(current, null) == default)
                        {
                            p.SetValue(current, DateTime.UtcNow, null);
                        }
                    }
                    else if (p.Name == nameof(IEntity.CreatedAsMillisecondsEpochTime))
                    {
                        //set created datetime if this is null
                        if (((double)p.GetValue(current, null)).Equals(default))
                        {
                            p.SetValue(current, DateTime.UtcNow.ConvertToMillisecondsEpochTime(), null);
                        }
                    }
                    else if (!p.PropertyType.IsSystemType())
                    {
                        //one-to-one reference                    
                        WalkGraphAndUpdateEntityMeta(p.GetValue(current, null));
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                    {
                        var collection = p.GetValue(current, null);
                        if (collection != null)
                        {
                            foreach (var sub in (IEnumerable)p.GetValue(current, null)) WalkGraphAndUpdateEntityMeta(sub);
                        }
                    }
            }
        }
    }
}