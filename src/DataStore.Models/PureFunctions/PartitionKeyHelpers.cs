namespace DataStore.Models.PureFunctions.Extensions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.PartitionKeyAttributes;

    public static class PartitionKeyHelpers
    {
        public class PartitionKeyValues
        {
            public string ParitionKey { get; internal set; }
            public Aggregate.HierarchicalPartitionKey PartitionKeys { get; internal set; }
        }

        public static PartitionKeyValues GetKeys<T>(T instance, PartitionKeySettings partitionKeySettings) where T: IAggregate
        {
            return new PartitionKeyValues()
            {
                ParitionKey = partitionKeySettings.UseHierarchicalKeys ? null : GetSyntheticKey(GetHierarchicalKeys(instance)),
                PartitionKeys = partitionKeySettings.UseHierarchicalKeys ? GetHierarchicalKeys(instance) : null
            };

            string GetSyntheticKey(Aggregate.HierarchicalPartitionKey hierarchicalPartitionKey)
            {
                return hierarchicalPartitionKey.Key1 + hierarchicalPartitionKey.Key2 + hierarchicalPartitionKey.Key3;
            }
        }
        
        private static Aggregate.HierarchicalPartitionKey GetHierarchicalKeys<T>(T instance) where T: IAggregate
        {
            var keys = new Aggregate.HierarchicalPartitionKey
            {
                Key1 = typeof(T).FullName
            };

            var idPartitionKeyProperties = typeof(T).GetCustomAttributes<PartitionKey_Type_AggregateId>();
            var tenantPartitionKeyProperties = typeof(T).GetCustomAttributes<PartitionKey_Type_ImmutableTenantId_AggregateId>();
            var timePeriodPartitionKeyProperties = typeof(T).GetCustomAttributes<PartitionKey_Type_ImmutableTimePeriod_AggregateId>();
            
            Guard.Against(idPartitionKeyProperties.Count() + tenantPartitionKeyProperties.Count() + timePeriodPartitionKeyProperties.Count() > 1, $"You cannot have more than one PartitionKey attribute on class {typeof(T).FullName}");

            if (idPartitionKeyProperties.Any())
            {
                keys.Key2 = instance.id.ToString();
            }
            else if (tenantPartitionKeyProperties.Any())
            {
                
                keys.Key2 = tenantPartitionKeyProperties[0].GetValue(instance).ToString();
                keys.Key3 = instance.id.ToString();
            }
            else if (timeIntervalParitionKeys.Any())
            {
                Guard.Against(timeIntervalParitionKeys.Count() > 2, $"You cannot have more than 2 {nameof(ImmutableTimeIntervalPartitionKey)} attributes on a class");
                
                keys.Key2 = timeIntervalParitionKeys[0].GetValue(instance).ToString();
                keys.Key3 = timeIntervalParitionKeys.Count() == 2 ? timeIntervalParitionKeys[1].GetValue(instance).ToString() : instance.id.ToString();
            }
            else
            {
                keys.Key2 = instance.id.ToString();
            }

            return keys;
        }
        
        private static bool HasAttribute(this PropertyInfo prop, Type attributeType){
            var att = prop.GetCustomAttributes(attributeType, true).FirstOrDefault();
            return att != null;
        }
    }
}