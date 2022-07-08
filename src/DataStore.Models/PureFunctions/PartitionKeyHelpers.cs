namespace DataStore.Models.PureFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options;
    using DataStore.Models.PartitionKeyAttributes;
    using DataStore.Models.PureFunctions.Extensions;

    public static class PartitionKeyHelpers
    {
        public static PartitionKeyValues GetKeysForExistingItemFromId<T>(bool useHierarchicalPartitionKeys, Guid id, IPartitionKeyOptions partitionKeyOptions)
            where T : IAggregate
        {
            return new PartitionKeyValues
            {
                PartitionKey = useHierarchicalPartitionKeys ? null : GenerateSyntheticKey(GenerateHierarchicalKeys(id, partitionKeyOptions)),
                PartitionKeys = useHierarchicalPartitionKeys ? GenerateHierarchicalKeys(id, partitionKeyOptions) : null
            };

            static Aggregate.HierarchicalPartitionKey GenerateHierarchicalKeys(Guid id, IPartitionKeyOptions partitionKeyOptions)
            {
                var keys = new Aggregate.HierarchicalPartitionKey
                {
                    Key1 = typeof(T).FullName
                };

                PartitionKeyTypeIds<T>(
                    out var idPartitionKeyAttributes,
                    out var tenantPartitionKeyAttributes,
                    out var timePeriodPartitionKeyAttributes,
                    out var tenantAndTimePeriodPartitionKeyAttributes);

                if (idPartitionKeyAttributes.Any())
                {
                    //* even though this is the default we have an explicit attribute for readability of the code if devs want to use
                    keys.Key2 = id.ToString();
                }
                else if (tenantPartitionKeyAttributes.Any())
                {
                    Guard.Against(
                        !partitionKeyOptions.PartitionKeyTenantId.HasValue,
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Tenant Id but you have not provided one in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(tenantId) method on this query.");
                    keys.Key2 = partitionKeyOptions.PartitionKeyTenantId.ToString();
                    keys.Key3 = id.ToString();
                }
                else if (timePeriodPartitionKeyAttributes.Any())
                {
                    Guard.Against(
                        partitionKeyOptions.PartitionKeyTimeInterval == null,
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Time Period but you have not provided one in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(timePeriod) method on this query.");
                    keys.Key2 = partitionKeyOptions.PartitionKeyTimeInterval.ToString();
                    keys.Key3 = id.ToString();
                }
                else if (tenantAndTimePeriodPartitionKeyAttributes.Any())
                {
                    Guard.Against(
                        partitionKeyOptions.PartitionKeyTimeInterval == null,
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Tenant Id and a Time Period but you have not provided these in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(tenantId, timePeriod) method on this query.");
                    keys.Key2 = partitionKeyOptions.PartitionKeyTenantId.ToString();
                    keys.Key2 = partitionKeyOptions.PartitionKeyTimeInterval.ToString();
                }
                else
                {
                    //* default
                    keys.Key2 = id.ToString();
                }

                return keys;
            }
        }

        //TODO consider when you query for groups and you dont provide all values, and you are using synthetic key, it should return nothing in that case
        public static PartitionKeyValues GetKeysForExistingItemFromType<T>(bool useHierarchicalPartitionKeys, IPartitionKeyOptions partitionKeyOptions)
            where T : IAggregate
        {
            return new PartitionKeyValues
            {
                PartitionKey = useHierarchicalPartitionKeys ? null : GenerateSyntheticKey(GenerateHierarchicalKeys(partitionKeyOptions)),
                PartitionKeys = useHierarchicalPartitionKeys ? GenerateHierarchicalKeys(partitionKeyOptions) : null
            };

            static Aggregate.HierarchicalPartitionKey GenerateHierarchicalKeys(IPartitionKeyOptions partitionKeyOptions)
            {
                var keys = new Aggregate.HierarchicalPartitionKey
                {
                    Key1 = typeof(T).FullName
                };

                PartitionKeyTypeIds<T>(
                    out var idPartitionKeyAttributes,
                    out var tenantPartitionKeyAttributes,
                    out var timePeriodPartitionKeyAttributes,
                    out var tenantAndTimePeriodPartitionKeyAttributes);

                if (idPartitionKeyAttributes.Any())
                {
                    //* even though this is the default we have an explicit attribute for readability of the code if devs want to use
                }
                else if (tenantPartitionKeyAttributes.Any())
                {
                    Guard.Against(
                        !partitionKeyOptions.PartitionKeyTenantId.HasValue,
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Tenant Id but you have not provided one in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(tenantId) method on this query.");
                    keys.Key2 = partitionKeyOptions.PartitionKeyTenantId.ToString();
                }
                else if (timePeriodPartitionKeyAttributes.Any())
                {
                    Guard.Against(
                        partitionKeyOptions.PartitionKeyTimeInterval == null,
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Time Period but you have not provided one in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(timePeriod) method on this query.");
                    keys.Key2 = partitionKeyOptions.PartitionKeyTimeInterval.ToString();
                }
                else if (tenantAndTimePeriodPartitionKeyAttributes.Any())
                {
                    Guard.Against(
                        partitionKeyOptions.PartitionKeyTimeInterval == null,
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Tenant Id and a Time Period but you have not provided these in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(tenantId, timePeriod) method on this query.");
                    keys.Key2 = partitionKeyOptions.PartitionKeyTenantId.ToString();
                    keys.Key2 = partitionKeyOptions.PartitionKeyTimeInterval.ToString();
                }

                return keys;
            }
        }

        public static PartitionKeyValues GetKeysForNewInstance<T>(T instance, bool useHierarchicalPartitionKeys) where T : IAggregate
        {
            return new PartitionKeyValues
            {
                PartitionKey = useHierarchicalPartitionKeys ? null : GenerateSyntheticKey(GenerateHierarchicalKeys(instance)),
                PartitionKeys = useHierarchicalPartitionKeys ? GenerateHierarchicalKeys(instance) : null
            };

            static Aggregate.HierarchicalPartitionKey GenerateHierarchicalKeys(T newInstance)
            {
                Guard.Against(newInstance.id == Guid.Empty, "Instance must have Id set before you can create Partition Keys");
                var keys = new Aggregate.HierarchicalPartitionKey
                {
                    Key1 = typeof(T).FullName
                };

                PartitionKeyTypeIds<T>(
                    out var idPartitionKeyAttributes,
                    out var tenantPartitionKeyAttributes,
                    out var timePeriodPartitionKeyAttributes,
                    out var tenantAndTimePeriodPartitionKeyAttributes);

                if (idPartitionKeyAttributes.Any())
                {
                    keys.Key2 = newInstance.id.ToString();
                }
                else if (tenantPartitionKeyAttributes.Any())
                {
                    var attribute = tenantPartitionKeyAttributes.Single();
                    var tenantProperty = typeof(T).GetProperty(attribute.PropertyWithTenantId);
                    var tenantString = ValidateTenantAttribute(newInstance, tenantProperty);

                    keys.Key2 = tenantString;
                    keys.Key3 = newInstance.id.ToString();
                }
                else if (timePeriodPartitionKeyAttributes.Any())
                {
                    var attribute = timePeriodPartitionKeyAttributes.Single();
                    var timePeriodProperty = typeof(T).GetProperty(attribute.PropertyWithDateTime);
                    var timePeriodString = ValidateTimePeriodAttribute(newInstance, timePeriodProperty, attribute.PartitionKeyTimeInterval);

                    keys.Key2 = timePeriodString;
                    keys.Key3 = newInstance.id.ToString();
                }
                else if (tenantAndTimePeriodPartitionKeyAttributes.Any())
                {
                    var attribute = tenantAndTimePeriodPartitionKeyAttributes.Single();
                    var tenantProperty = typeof(T).GetProperty(attribute.PropertyWithTenantId);
                    var tenantString = ValidateTenantAttribute(newInstance, tenantProperty);

                    var timePeriodProperty = typeof(T).GetProperty(attribute.PropertyWithDateTime);
                    var timePeriodString = ValidateTimePeriodAttribute(newInstance, timePeriodProperty, attribute.PartitionKeyTimeInterval);

                    keys.Key2 = tenantString;
                    keys.Key3 = timePeriodString;
                }
                else
                {
                    keys.Key2 = newInstance.id.ToString();
                }

                return keys;
            }
        }

        private static string GenerateSyntheticKey(Aggregate.HierarchicalPartitionKey hierarchicalPartitionKey)
        {
            return $"{hierarchicalPartitionKey.Key1}_{hierarchicalPartitionKey.Key2}{(string.IsNullOrWhiteSpace(hierarchicalPartitionKey.Key3) ? string.Empty : '_' + hierarchicalPartitionKey.Key3)}";
        }

        private static void PartitionKeyTypeIds<T>(
            out List<PartitionKey__Type_Id> idPartitionKeyAttributes,
            out List<PartitionKey__Type_ImmutableTenantId_Id> tenantPartitionKeyAttributes,
            out List<PartitionKey__Type_TimePeriod_Id> timePeriodPartitionKeyAttributes,
            out List<PartitionKey__Type_ImmutableTenantId_TimePeriod> tenantAndTimePeriodPartitionKeyAttributes) where T : IAggregate
        {
            idPartitionKeyAttributes = typeof(T).GetCustomAttributes<PartitionKey__Type_Id>().ToList();
            tenantPartitionKeyAttributes = typeof(T).GetCustomAttributes<PartitionKey__Type_ImmutableTenantId_Id>().ToList();
            timePeriodPartitionKeyAttributes = typeof(T).GetCustomAttributes<PartitionKey__Type_TimePeriod_Id>().ToList();
            tenantAndTimePeriodPartitionKeyAttributes = typeof(T).GetCustomAttributes<PartitionKey__Type_ImmutableTenantId_TimePeriod>().ToList();

            Guard.Against(
                idPartitionKeyAttributes.Count() + tenantPartitionKeyAttributes.Count() + timePeriodPartitionKeyAttributes.Count()
                + tenantAndTimePeriodPartitionKeyAttributes.Count() > 1,
                $"You cannot have more than one PartitionKey attribute on class {typeof(T).FullName}");
        }

        private static string ValidateTenantAttribute<T>(T newInstance, PropertyInfo property) where T : IAggregate
        {
            Guard.Against(
                property == null,
                $"The PartitionKey property you specified for {nameof(PartitionKey__Type_ImmutableTenantId_Id.PropertyWithTenantId)} does not exist on this class");
            var propertyValue = property.GetValue(newInstance).ToString(); //* expect guid
            Guard.Against(
                string.IsNullOrWhiteSpace(propertyValue),
                $"The PartitionKey property value you specified for {nameof(PartitionKey__Type_ImmutableTenantId_Id.PropertyWithTenantId)} is empty. Provide a value or use {nameof(PartitionKey__Type_Id)} instead.");
            return propertyValue;
        }

        private static string ValidateTimePeriodAttribute<T>(T newInstance, PropertyInfo property, PartitionKeyTimeIntervalEnum timeInterval) where T : IAggregate
        {
            Guard.Against(
                property == null,
                $"The PartitionKey property you specified for {nameof(PartitionKey__Type_TimePeriod_Id.PropertyWithDateTime)} does not exist on this class");
            var timePeriodPropertyValue = property.GetValue(newInstance).Cast<DateTime?>();

            Guard.Against(
                timePeriodPropertyValue.GetValueOrDefault() == default,
                $"The PartitionKey property value you specified for {nameof(PartitionKey__Type_TimePeriod_Id.PropertyWithDateTime)} is empty. Provide a value or use {nameof(PartitionKey__Type_Id)} instead.");
            var timePeriodString = GetTimePeriodString(timePeriodPropertyValue, timeInterval);
            return timePeriodString;

            static string GetTimePeriodString(DateTime? timePeriodPropertyValue, PartitionKeyTimeIntervalEnum interval)
            {
                var timePeriodPropertyValueNonNull = timePeriodPropertyValue.Value;
                var timePeriodString = interval == PartitionKeyTimeIntervalEnum.Year
                                           ? new YearInterval(timePeriodPropertyValueNonNull.Year).ToString()
                                           :
                                           interval == PartitionKeyTimeIntervalEnum.Month
                                               ?
                                               new MonthInterval(timePeriodPropertyValueNonNull.Year, timePeriodPropertyValueNonNull.Month).ToString()
                                               : interval == PartitionKeyTimeIntervalEnum.Day
                                                   ? new DayInterval(
                                                       timePeriodPropertyValueNonNull.Year,
                                                       timePeriodPropertyValueNonNull.Month,
                                                       timePeriodPropertyValueNonNull.Day).ToString()
                                                   : interval == PartitionKeyTimeIntervalEnum.Hour
                                                       ? new HourInterval(
                                                           timePeriodPropertyValueNonNull.Year,
                                                           timePeriodPropertyValueNonNull.Month,
                                                           timePeriodPropertyValueNonNull.Day,
                                                           timePeriodPropertyValueNonNull.Hour).ToString()
                                                       : interval == PartitionKeyTimeIntervalEnum.Minute
                                                           ? new MinuteInterval(
                                                               timePeriodPropertyValueNonNull.Year,
                                                               timePeriodPropertyValueNonNull.Month,
                                                               timePeriodPropertyValueNonNull.Day,
                                                               timePeriodPropertyValueNonNull.Hour,
                                                               timePeriodPropertyValueNonNull.Minute).ToString()
                                                           : throw new ArgumentOutOfRangeException();
                return timePeriodString;
            }
        }

        public class PartitionKeyValues
        {
            public string PartitionKey { get; internal set; }

            public Aggregate.HierarchicalPartitionKey PartitionKeys { get; internal set; }
        }
    }
}