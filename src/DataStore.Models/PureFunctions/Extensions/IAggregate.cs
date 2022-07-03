namespace DataStore.Models.PureFunctions.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.PartitionKeyAttributes;

    public static class IAggregateExt
    {


        internal static void ForcefullySetMandatoryPropertyValues<T>(this T newObject, bool readOnly, PartitionKeySettings partitionKeySettings) where T : class, IAggregate, new()
        {
            
            newObject.Schema =
                typeof(T).FullName; //will be defaulted by Aggregate but needs to be forced as it is open to change because of serialisation opening the property setter
            newObject.ReadOnly = readOnly;

            newObject.PartitonKey = 
            newObject.PartitionKeys = partitionKeySettings.GetKey<T>(newObject.id);
            /*
           even though we use the id having a separate field here will allow us to more easily pivot later to allow other values as the partition key and not have to change existing data
           If we pivot away from id we need to consider the following things:
               1. For the GetItemAsync call in cosmosdb we won't have the partition key need to make the call and we
               will end up needing to change that code to do a cross-partition query to find the item unless the partition key is the type which we can't guarantee will
               stay below 20GB limit over time. Really not using Id or Type would only work if you were storing data that at time of query you know the partition key
               value as well as the id when you want to retrieve a single document and you know the key wont result in more than 20GB of data over time. 
               Seems an odd scenario. If it goes to a cross partition query for a single document its still ok
               until you have multiple physical partitions this is where indexes wont help and you'd end up making a xpartition query where you wouldn't with id.
               There might be some scenario's where you could imagine that the readbyid xpartition query is worth accepting for a gain in other areas due
               to a specific read query that dominates a particular app or datastore container connection, but i imagine these would be hard to come by.
               2. you would have to tale that value in the datastoreoptions and then set it here with something like this
                           if (!string.IsNullOrWhiteSpace(options.CustomPartitionKey))
                           {
                               var prop = newObject.GetType().GetProperty(options.CustomPartitionKey);
                               Guard.Against(prop == null, "The property specified as a PartitionKey does not exist");
                               var propValue = prop.GetValue(newObject).ToString();
                               Guard.Against(string.IsNullOrWhiteSpace(propValue), "The property specified as a PartitionKey contains an empty value");
                               newObject.PartitionKeyValue = propValue;
                           }
               3. you have to extend the ReadAggregateByIdOperation to hold the partitionkey and transfer it to the cosmosrepository GetItemAsync method
               which will require it.  
           */
            newObject.VersionHistory = new List<Aggregate.AggregateVersionInfo>(); //-set again in commitchanges, still best not to allow any invalid state

            WalkGraphAndUpdateEntityMeta(newObject);

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