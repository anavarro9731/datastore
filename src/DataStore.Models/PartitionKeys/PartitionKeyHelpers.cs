namespace DataStore.Models.PartitionKeys
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CircuitBoard;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options;
    using DataStore.Models.PureFunctions;
    using DataStore.Models.PureFunctions.Extensions;

    
    /*     
     * in the methods below i take an approach where when using synthetic keys, which i do not expect
     * to be the default going forward as hierarchical is a more progressive approach and i dont see
     * the feature not surviving, i have added the id property as the 3rd level key, with the one
     * exception where we already fill all levels with other data.
     * this means that in synthetic key mode you always have to have the id or do a full fan out query
     * which is expensive. there is an option of revising the code to offer less cardinality when using synthetics
     * e.g. making the the key "{Type}" only basically staying only with the data you have but
     * while that would work often its not really safe because if you have a type with over 20GB
     * data then you are going to have a big problem. when you have the full cardinality you are
     * taking this risk away.
     */

    public static class PartitionKeyHelpers
    {
        public static class PartitionKeyPrefixes
        {
            public const string Type = "TP:";
            public const string TenantId = "TN:";
            public const string TimePeriod = "TM:";
            public const string AggregateId = "ID:";
        }
        
        //* always requires full partition key in either mode
        public static PartitionKeyValues GetKeysForExistingItemFromId<T>(bool useHierarchicalPartitionKeys, Guid id, IPartitionKeyOptions partitionKeyOptions)
            where T : IAggregate
        {
            return new PartitionKeyValues(
                useHierarchicalPartitionKeys ? null : GenerateHierarchicalKeys(id, partitionKeyOptions, false)?.ToSyntheticKey(),
                useHierarchicalPartitionKeys ? GenerateHierarchicalKeys(id, partitionKeyOptions, true) : null);

            static Aggregate.HierarchicalPartitionKey GenerateHierarchicalKeys(Guid id, IPartitionKeyOptions partitionKeyOptions, bool useHierarchicalPartitionKeys)
            {
                var keys = new Aggregate.HierarchicalPartitionKey();
                
                PartitionKeyTypeIds<T>(
                    out var sharedPartitionKeyAttributes,
                    out var idPartitionKeyAttributes,
                    out var tenantPartitionKeyAttributes,
                    out var timePeriodPartitionKeyAttributes,
                    out var tenantAndTimePeriodPartitionKeyAttributes);

                if (sharedPartitionKeyAttributes.Any())
                {
                    //* support legacy versions of DataStore before the PartitionKeys feature through use of "SharedPartitionKeyAttribute"
                    if (useHierarchicalPartitionKeys)
                    {
                        throw new CircuitException(
                            $"If you have aggregates using the legacy {nameof(PartitionKey__Shared)} attribute you cannot use Hierarchical Partition Keys. "
                            + "Please ensure the UseHierarchicalPartitionKeys property on the DocumentRepository resolves to false. This is usually set through a"
                            + " parameter on the database settings class that is used to create the repository");
                    } 

                    keys.Key1 = "shared"; 
                }
                else if (idPartitionKeyAttributes.Any())
                {
                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                    keys.Key2 = PartitionKeyPrefixes.AggregateId + id;
                }
                else if (tenantPartitionKeyAttributes.Any())
                {
                    //* must always provide in the options when querying by id if the class requires it
                    Guard.Against(
                        partitionKeyOptions?.PartitionKeyTenantId == null,
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Tenant Id but you have not provided one in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(tenantId) method on this query.");
                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                    keys.Key2 = PartitionKeyPrefixes.TenantId + partitionKeyOptions.PartitionKeyTenantId;
                    keys.Key3 = PartitionKeyPrefixes.AggregateId + id;
                }
                else if (timePeriodPartitionKeyAttributes.Any())
                {
                    //* must always provide in the options when querying by id if the class requires it
                    Guard.Against(
                        partitionKeyOptions?.PartitionKeyTimeInterval == null,
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Time Period but you have not provided one in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(timePeriod) method on this query.");
                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                    keys.Key2 = PartitionKeyPrefixes.TimePeriod + partitionKeyOptions.PartitionKeyTimeInterval;
                    keys.Key3 = PartitionKeyPrefixes.AggregateId + id;
                }
                else if (tenantAndTimePeriodPartitionKeyAttributes.Any())
                {
                    //* must always provide in the options when querying by id if the class requires it
                    Guard.Against(
                        partitionKeyOptions?.PartitionKeyTimeInterval == null || partitionKeyOptions?.PartitionKeyTenantId == null,
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Tenant Id and a Time Period but you have not provided these in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(tenantId, timePeriod) method on this query.");
                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                    keys.Key2 = PartitionKeyPrefixes.TenantId + partitionKeyOptions.PartitionKeyTenantId;
                    keys.Key3 = PartitionKeyPrefixes.TimePeriod + partitionKeyOptions.PartitionKeyTimeInterval;
                }
                else
                {
                    throw new CircuitException("There should always be a partition key attribute on every class, a guard has failed.");
                }

                return keys;
            }
        }

        public static PartitionKeyValues GetKeysForLinqQuery<T>(bool useHierarchicalPartitionKeys, IPartitionKeyOptions partitionKeyOptions)
            where T : IAggregate
        {
            return new PartitionKeyValues(
                useHierarchicalPartitionKeys ? null : GenerateHierarchicalKeys(partitionKeyOptions, false)?.ToSyntheticKey(),
                useHierarchicalPartitionKeys ? GenerateHierarchicalKeys(partitionKeyOptions, true) : null);

            static Aggregate.HierarchicalPartitionKey GenerateHierarchicalKeys(IPartitionKeyOptions partitionKeyOptions, bool useHierarchicalPartitionKeys)
            {
                var keys = new Aggregate.HierarchicalPartitionKey();

                PartitionKeyTypeIds<T>(
                    out var sharedPartitionKeyAttributes,
                    out var idPartitionKeyAttributes,
                    out var tenantPartitionKeyAttributes,
                    out var timePeriodPartitionKeyAttributes,
                    out var tenantAndTimePeriodPartitionKeyAttributes);

                if (sharedPartitionKeyAttributes.Any())
                {
                    //* support legacy versions of DataStore before the PartitionKeys feature through use of "SharedPartitionKeyAttribute"
                    if (useHierarchicalPartitionKeys)
                    {
                        throw new CircuitException(
                            $"If you have aggregates using the legacy {nameof(PartitionKey__Shared)} attribute you cannot use Hierarchical Partition Keys. "
                            + "Please ensure the UseHierarchicalPartitionKeys property on the DocumentRepository resolves to false. This is usually set through a"
                            + " parameter on the database settings class that is used to create the repository");
                    }

                    keys.Key1 = "shared";
                }
                else if (idPartitionKeyAttributes.Any())
                {
                    if (useHierarchicalPartitionKeys)
                    {
                        /* if the class is partitioned by id and we are searching by type with LINQ predicates,
                            we will not have the L2 ID without parsing the expression tree which I am not going to do 
                            or adding a pointless id value to the ProvidePartitionKeyValues() method when the user can
                            just use ReadById or ReadManyById if they have that.*/
                        keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName; //* broaden search to L1
                    }
                    else
                    {
                        keys = null; //* fallback to full fanout
                    }
                }
                else if (tenantPartitionKeyAttributes.Any())
                {
                    if (useHierarchicalPartitionKeys)
                    {
                        keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                        if (partitionKeyOptions?.PartitionKeyTenantId != null) keys.Key2 = PartitionKeyPrefixes.TenantId + partitionKeyOptions.PartitionKeyTenantId; //* constrain search with L2
                    }
                    else
                    {
                        /* if we are in synthetic mode, we can't compose the whole key we need to return nothing
                             this will result in an expensive full fan out query, but that is the problem with not using hierarchical keys
                             and you want to ensure limitless growth of logical partitions                          
                         */

                            keys = null; //* reset to fan out
                    }
                }
                else if (timePeriodPartitionKeyAttributes.Any())
                {
                    if (useHierarchicalPartitionKeys)
                    {
                        keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                        if (partitionKeyOptions?.PartitionKeyTimeInterval != null) keys.Key2 = PartitionKeyPrefixes.TimePeriod + partitionKeyOptions.PartitionKeyTimeInterval; //* constrain to L2
                    }
                    else
                    {
                        /* if we are in synthetic mode, we can't compose the whole key we need to return nothing
                             this will result in an expensive full fan out query, but that is the problem with not using hierarchical keys
                             and you want to ensure limitless growth of logical partitions */
                            
                            keys = null; //* broaden to fan out
                    }
                }
                else if (tenantAndTimePeriodPartitionKeyAttributes.Any())
                {
                    if (useHierarchicalPartitionKeys)
                    {
                        keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                        if (partitionKeyOptions?.PartitionKeyTenantId != null) keys.Key2 = PartitionKeyPrefixes.TenantId + partitionKeyOptions.PartitionKeyTenantId;
                        if (partitionKeyOptions?.PartitionKeyTimeInterval != null) keys.Key3 = PartitionKeyPrefixes.TimePeriod + partitionKeyOptions.PartitionKeyTimeInterval; //* constrain to L3
                    }
                    else
                    {
                            /* if we are in synthetic mode, we can't compose the whole key we need to return nothing
                                 this will result in an expensive full fan out query, but that is the problem with not using hierarchical keys
                                 and you want to ensure limitless growth of logical partitions */
                            
                            keys = null; //* broaden to fan out
                    }
                }
                else
                {
                    throw new CircuitException("There should always be a partition key attribute on every class, a guard has failed.");
                }

                return keys;
            }
        }

        public static PartitionKeyValues GetKeysForNewModel<T>(T instance, bool useHierarchicalPartitionKeys) where T : IAggregate
        {
            return new PartitionKeyValues(
                useHierarchicalPartitionKeys ? null : GenerateHierarchicalKeys(instance, false).ToSyntheticKey(),
                useHierarchicalPartitionKeys ? GenerateHierarchicalKeys(instance, true) : null);

            static Aggregate.HierarchicalPartitionKey GenerateHierarchicalKeys(T newInstance, bool useHierarchicalPartitionKeys)
            {
                Guard.Against(newInstance.id == Guid.Empty, "Instance must have Id set before you can create Partition Keys");
                
                var keys = new Aggregate.HierarchicalPartitionKey();

                PartitionKeyTypeIds<T>(
                    out var sharedPartitionKeyAttributes,
                    out var idPartitionKeyAttributes,
                    out var tenantPartitionKeyAttributes,
                    out var timePeriodPartitionKeyAttributes,
                    out var tenantAndTimePeriodPartitionKeyAttributes);

                if (sharedPartitionKeyAttributes.Any())
                {
                    //* support legacy versions of DataStore before the PartitionKeys feature through use of "SharedPartitionKeyAttribute"
                    if (useHierarchicalPartitionKeys)
                    {
                        throw new CircuitException(
                            $"If you have aggregates using the legacy {nameof(PartitionKey__Shared)} attribute you cannot use Hierarchical Partition Keys. "
                            + "Please ensure the UseHierarchicalPartitionKeys property on the DocumentRepository resolves to false. This is usually set through a"
                            + " parameter on the database settings class that is used to create the repository");
                    }

                    keys.Key1 = "shared";
                }
                else if (idPartitionKeyAttributes.Any())
                {
                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                    keys.Key2 = PartitionKeyPrefixes.AggregateId + newInstance.id;
                }
                else if (tenantPartitionKeyAttributes.Any())
                {
                    var attribute = tenantPartitionKeyAttributes.Single();
                    var tenantProperty = typeof(T).GetProperty(attribute.PropertyWithTenantId);
                    var tenantString = ValidateTenantAttributeExistsAndHasAValue(newInstance, tenantProperty);

                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                    keys.Key2 = PartitionKeyPrefixes.TenantId + tenantString;
                    keys.Key3 = PartitionKeyPrefixes.AggregateId + newInstance.id;
                }
                else if (timePeriodPartitionKeyAttributes.Any())
                {
                    var attribute = timePeriodPartitionKeyAttributes.Single();
                    var timePeriodProperty = typeof(T).GetProperty(attribute.PropertyWithDateTime);
                    var timePeriodString = ValidateTimePeriodAttributeExistsAndHasAValue(newInstance, timePeriodProperty, attribute.PartitionKeyTimeInterval);

                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                    keys.Key2 = PartitionKeyPrefixes.TimePeriod + timePeriodString;
                    keys.Key3 = PartitionKeyPrefixes.AggregateId + newInstance.id;
                }
                else if (tenantAndTimePeriodPartitionKeyAttributes.Any())
                {
                    var attribute = tenantAndTimePeriodPartitionKeyAttributes.Single();
                    var tenantProperty = typeof(T).GetProperty(attribute.PropertyWithTenantId);
                    var tenantString = ValidateTenantAttributeExistsAndHasAValue(newInstance, tenantProperty);

                    var timePeriodProperty = typeof(T).GetProperty(attribute.PropertyWithDateTime);
                    var timePeriodString = ValidateTimePeriodAttributeExistsAndHasAValue(newInstance, timePeriodProperty, attribute.PartitionKeyTimeInterval);

                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).FullName;
                    keys.Key2 = PartitionKeyPrefixes.TenantId + tenantString;
                    keys.Key3 = PartitionKeyPrefixes.TimePeriod + timePeriodString;
                }
                else
                {
                    throw new CircuitException("There should always be a partition key attribute on every class, a guard has failed.");
                }

                return keys;
            }
        }

        private static void PartitionKeyTypeIds<T>(
            out List<PartitionKey__Shared> sharedPartitionKeyAttributes,
            out List<PartitionKey__Type_Id> idPartitionKeyAttributes,
            out List<PartitionKey__Type_ImmutableTenantId_Id> tenantPartitionKeyAttributes,
            out List<PartitionKey__Type_TimePeriod_Id> timePeriodPartitionKeyAttributes,
            out List<PartitionKey__Type_ImmutableTenantId_TimePeriod> tenantAndTimePeriodPartitionKeyAttributes) where T : IAggregate
        {
            sharedPartitionKeyAttributes = typeof(T).GetCustomAttributes<PartitionKey__Shared>().ToList();
            idPartitionKeyAttributes = typeof(T).GetCustomAttributes<PartitionKey__Type_Id>().ToList();
            tenantPartitionKeyAttributes = typeof(T).GetCustomAttributes<PartitionKey__Type_ImmutableTenantId_Id>().ToList();
            timePeriodPartitionKeyAttributes = typeof(T).GetCustomAttributes<PartitionKey__Type_TimePeriod_Id>().ToList();
            tenantAndTimePeriodPartitionKeyAttributes = typeof(T).GetCustomAttributes<PartitionKey__Type_ImmutableTenantId_TimePeriod>().ToList();

            Guard.Against(
                idPartitionKeyAttributes.Count() + tenantPartitionKeyAttributes.Count() + timePeriodPartitionKeyAttributes.Count()
                + tenantAndTimePeriodPartitionKeyAttributes.Count() + sharedPartitionKeyAttributes.Count() > 1,
                $"You cannot have more than one PartitionKey attribute on class {typeof(T).FullName}");

            Guard.Against(
                idPartitionKeyAttributes.Count() + tenantPartitionKeyAttributes.Count() + timePeriodPartitionKeyAttributes.Count()
                + tenantAndTimePeriodPartitionKeyAttributes.Count() + sharedPartitionKeyAttributes.Count() == 0,
                $"You must have at least one PartitionKey attribute on class {typeof(T).FullName}");
        }

        private static string ToSyntheticKey(this Aggregate.HierarchicalPartitionKey hierarchicalPartitionKey)
        {
            //* return first and second and optional third level as a string
            return
                $"{hierarchicalPartitionKey.Key1}" + 
                $"{(string.IsNullOrWhiteSpace(hierarchicalPartitionKey.Key2) ? string.Empty : '_' + hierarchicalPartitionKey.Key2)}" + 
                $"{(string.IsNullOrWhiteSpace(hierarchicalPartitionKey.Key3) ? string.Empty : '_' + hierarchicalPartitionKey.Key3)}";
        }

        private static string ValidateTenantAttributeExistsAndHasAValue<T>(T newInstance, PropertyInfo property) where T : IAggregate
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

        private static string ValidateTimePeriodAttributeExistsAndHasAValue<T>(T newInstance, PropertyInfo property, PartitionKeyTimeIntervalEnum timeInterval)
            where T : IAggregate
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
    }
}