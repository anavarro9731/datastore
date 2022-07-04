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
        public static PartitionKeyValues GetKeys<T>(T instance, PartitionKeySettings partitionKeySettings) where T : IAggregate
        {
            return new PartitionKeyValues
            {
                ParitionKey = partitionKeySettings.UseHierarchicalKeys ? null : GetSyntheticKey(GetNewHierarchicalKeys(instance)),
                PartitionKeys = partitionKeySettings.UseHierarchicalKeys ? GetNewHierarchicalKeys(instance) : null
            };

            string GetSyntheticKey(Aggregate.HierarchicalPartitionKey hierarchicalPartitionKey)
            {
                return hierarchicalPartitionKey.Key1 + hierarchicalPartitionKey.Key2 + hierarchicalPartitionKey.Key3;
            }
        }

        private static Aggregate.HierarchicalPartitionKey GetNewHierarchicalKeys<T>(T newInstance) where T : IAggregate
        {
            Guard.Against(newInstance.id == Guid.Empty, "Instance must have Id set before you can create Partition Keys");
            var keys = new Aggregate.HierarchicalPartitionKey
            {
                Key1 = typeof(T).FullName
            };

            var idPartitionKeyProperties = typeof(T).GetCustomAttributes<PartitionKey__Type_Id>();
            var tenantPartitionKeyProperties = typeof(T).GetCustomAttributes<PartitionKey__Type_ImmutableTenantId_Id>();
            var tenantAndTimePeriodPartitionKeyProperties = typeof(T).GetCustomAttributes<PartitionKey__Type_ImmutableTenantId_TimePeriod>();

            Guard.Against(
                idPartitionKeyProperties.Count() + tenantPartitionKeyProperties.Count() + tenantAndTimePeriodPartitionKeyProperties.Count() > 1,
                $"You cannot have more than one PartitionKey attribute on class {typeof(T).FullName}");

            if (idPartitionKeyProperties.Any())
            {
                keys.Key2 = newInstance.id.ToString();
            }
            else if (tenantPartitionKeyProperties.Any())
            {
                var property = typeof(T).GetProperty(tenantPartitionKeyProperties.Single().PropertyWithTenantId);
                Guard.Against(
                    property == null,
                    $"The PartitionKey property you specified for {nameof(PartitionKey__Type_ImmutableTenantId_Id.PropertyWithTenantId)} does not exist on this class");
                var propertyValue = property.GetValue(newInstance).ToString();
                Guard.Against(
                    string.IsNullOrWhiteSpace(propertyValue),
                    $"The PartitionKey property value you specified for {nameof(PartitionKey__Type_ImmutableTenantId_Id.PropertyWithTenantId)} is empty. Provide a value or use {nameof(PartitionKey__Type_Id)} instead.");

                keys.Key2 = propertyValue;
                keys.Key3 = newInstance.id.ToString();
            }
            else if (tenantAndTimePeriodPartitionKeyProperties.Any())
            {
                var property = typeof(T).GetProperty(tenantPartitionKeyProperties.Single().PropertyWithTenantId);

                var tenantPropertyValue = property == null || property.GetValue(newInstance) == default ? null : property.GetValue(newInstance).ToString();

                var timePeriodProperty = typeof(T).GetProperty(tenantAndTimePeriodPartitionKeyProperties.Single().PropertyWithTenantId);
                Guard.Against(
                    timePeriodProperty == null,
                    $"The PartitionKey property you specified for {nameof(PartitionKey__Type_ImmutableTenantId_TimePeriod.PropertyWithDateTime)} does not exist on this class");
                var timePeriodPropertyValue = property.GetValue(newInstance).Cast<DateTime?>();
                Guard.Against(
                    timePeriodPropertyValue.GetValueOrDefault() == default,
                    $"The PartitionKey property value you specified for {nameof(PartitionKey__Type_ImmutableTenantId_Id.PropertyWithTenantId)} is empty. Provide a value or use {nameof(PartitionKey__Type_Id)} instead.");
                var timePeriodPropertyValueNonNull = timePeriodPropertyValue.Value;
                var intervalType = tenantAndTimePeriodPartitionKeyProperties.Single().PartitionKeyTimeInterval;
                var timePeriodString = intervalType == PartitionKeyTimeIntervalEnum.Years
                                           ? timePeriodPropertyValueNonNull.Year.ToString()
                                           :
                                           intervalType == PartitionKeyTimeIntervalEnum.Months
                                               ?
                                               timePeriodPropertyValueNonNull.Year + timePeriodPropertyValueNonNull.Month.ToString()
                                               : intervalType == PartitionKeyTimeIntervalEnum.Days
                                                   ? timePeriodPropertyValueNonNull.Year + timePeriodPropertyValueNonNull.DayOfYear.ToString()
                                                   : intervalType == PartitionKeyTimeIntervalEnum.Seconds
                                                       ? new DateTimeOffset(timePeriodPropertyValueNonNull).ToUnixTimeSeconds().ToString()
                                                       : intervalType == PartitionKeyTimeIntervalEnum.Milliseconds
                                                           ? new DateTimeOffset(timePeriodPropertyValueNonNull).ToUnixTimeMilliseconds().ToString()
                                                           : throw new ArgumentOutOfRangeException();

                keys.Key2 = string.IsNullOrWhiteSpace(tenantPropertyValue) ? timePeriodString : tenantPropertyValue;
                keys.Key3 = string.IsNullOrWhiteSpace(tenantPropertyValue) ? newInstance.id.ToString() : timePeriodString;
            }
            else
            {
                keys.Key2 = newInstance.id.ToString();
            }

            return keys;
        }

        private static bool HasAttribute(this PropertyInfo prop, Type attributeType)
        {
            var att = prop.GetCustomAttributes(attributeType, true).FirstOrDefault();
            return att != null;
        }

        public class PartitionKeyValues
        {
            public string ParitionKey { get; internal set; }

            public Aggregate.HierarchicalPartitionKey PartitionKeys { get; internal set; }
        }
    }
}