namespace DataStore.Models.PartitionKeys
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using CircuitBoard;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;
    using DataStore.Models.PureFunctions;
    using DataStore.Models.PureFunctions.Extensions;

    #endregion

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
        //* always requires full partition key in either mode
        public static HierarchicalPartitionKey GetKeysForExistingItemFromId<T>(bool useHierarchicalPartitionKeys, Guid id, IPartitionKeyOptionsLibrarySide partitionKeyOptions)
            where T : IAggregate
        {
            return GenerateHierarchicalKeys(id, partitionKeyOptions, useHierarchicalPartitionKeys);


            static HierarchicalPartitionKey GenerateHierarchicalKeys(Guid id, IPartitionKeyOptionsLibrarySide partitionKeyOptions, bool useHierarchicalPartitionKeys)
            {
                var keys = new HierarchicalPartitionKey();

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

                    Guard.Against(
                        partitionKeyOptions.HasSpecifiedAtLeastOneOption(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Shared)}."
                        + "This does not require any partition key Options, please do not provide them.");

                    keys.Key1 = "sh";
                    keys.Key2 = "ar";
                    keys.Key3 = "ed";
                }
                else if (idPartitionKeyAttributes.Any())
                {
                    Guard.Against(
                        partitionKeyOptions.HasSpecifiedAtLeastOneOption(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_Id)}."
                        + "This does not require any partition key Options, please do not provide them.");

                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
                    keys.Key2 = PartitionKeyPrefixes.IdOptional + id;
                    keys.Key3 = "_na";
                }
                else if (tenantPartitionKeyAttributes.Any())
                {
                    //* must always provide in the options when querying by id if the class requires it
                    Guard.Against(
                        !partitionKeyOptions.HasTenantIdOption(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Tenant Id but you have not provided one in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(tenantId) method on this query.");

                    Guard.Against(
                        partitionKeyOptions.HasTimePeriodOption(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_ImmutableTenantId_Id)}."
                        + "This does not require a time period partition key Option, please do not provide it.");

                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
                    keys.Key2 = PartitionKeyPrefixes.TenantId + partitionKeyOptions.PartitionKeyTenantId;
                    keys.Key3 = PartitionKeyPrefixes.IdOptional + id;
                }
                else if (timePeriodPartitionKeyAttributes.Any())
                {
                    //* must always provide in the options when querying by id if the class requires it
                    Guard.Against(
                        !partitionKeyOptions.HasTimePeriodOption(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Time Period but you have not provided one in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(timePeriod) method on this query.");

                    Guard.Against(
                        partitionKeyOptions.HasTenantIdOption(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_TimePeriod_Id)}."
                        + "This does not require a tenant partition key Option, please do not provide it.");

                    ValidateCorrectIntervalType(timePeriodPartitionKeyAttributes, partitionKeyOptions?.PartitionKeyTimeInterval);
                    
                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
                    keys.Key2 = PartitionKeyPrefixes.TimePeriod + partitionKeyOptions.PartitionKeyTimeInterval;
                    keys.Key3 = PartitionKeyPrefixes.IdOptional + id;
                }
                else if (tenantAndTimePeriodPartitionKeyAttributes.Any())
                {
                    //* must always provide in the options when querying by id if the class requires it
                    Guard.Against(
                        !partitionKeyOptions.HasSpecifiedBothOptions(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Tenant Id and a Time Period but you have not provided these in the query options. "
                        + "Please use the options => option.ProvidePartitionKeyValues(tenantId, timePeriod) method on this query.");
                    
                    ValidateCorrectIntervalType(tenantAndTimePeriodPartitionKeyAttributes, partitionKeyOptions?.PartitionKeyTimeInterval);
                    
                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
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

        public static Expression<Func<T, bool>> ToPredicate<T>(this HierarchicalPartitionKey key, bool useHierarchicalPartitionKeys) where T : IAggregate
        {
            Expression<Func<T, bool>> pred;
            if (!useHierarchicalPartitionKeys)
            {
                pred = t => t.PartitionKey == key.ToSyntheticKeyString();

                return pred;    
            }
            
            pred = t => t.PartitionKeys.Key1 == key.Key1;
                if (!string.IsNullOrWhiteSpace(key.Key2)) pred = pred.And(t => t.PartitionKeys.Key2 == key.Key2);
                if (!string.IsNullOrWhiteSpace(key.Key3)) pred = pred.And(t => t.PartitionKeys.Key3 == key.Key3);

                return pred;
        }
        
        public static HierarchicalPartitionKey GetKeysForLinqQuery<T>(bool useHierarchicalPartitionKeys, IPartitionKeyOptionsLibrarySide partitionKeyOptions) where T : IAggregate
        {
            return GenerateHierarchicalKeys(partitionKeyOptions, useHierarchicalPartitionKeys);

            static HierarchicalPartitionKey GenerateHierarchicalKeys(IPartitionKeyOptionsLibrarySide partitionKeyOptions, bool useHierarchicalPartitionKeys)
            {
                var keys = new HierarchicalPartitionKey();

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

                    Guard.Against(
                        partitionKeyOptions.HasSpecifiedAtLeastOneOption(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Shared)}."
                        + "This does not require any partition key Options, please do not provide them.");

                    keys.Key1 = "sh";
                    keys.Key2 = "ar";
                    keys.Key3 = "ed";
                }
                else if (idPartitionKeyAttributes.Any())
                {
                    Guard.Against(
                        partitionKeyOptions.HasSpecifiedAtLeastOneOption(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_Id)}."
                        + "This does not require any partition key Options, please do not provide them.");

                    if (useHierarchicalPartitionKeys)
                    {
                        /* if the class is partitioned by id and we are searching by type with LINQ predicates,
                            we will not have the L2 ID without parsing the expression tree which I am not going to do 
                            or adding a pointless id value to the ProvidePartitionKeyValues() method when the user can
                            just use ReadById or ReadManyById if they have that.*/
                        keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name; //* broaden search to L1
                    }
                    else
                    {
                        return keys; //* fallback to full fanout
                    }
                }
                else if (tenantPartitionKeyAttributes.Any())
                {
                    Guard.Against(
                        partitionKeyOptions.HasTimePeriodOption(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_ImmutableTenantId_Id)}."
                        + "This does not require a time period partition key Option, please do not provide it.");

                    if (useHierarchicalPartitionKeys)
                    {
                        keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
                        if (partitionKeyOptions?.PartitionKeyTenantId != null)
                        {
                            keys.Key2 = PartitionKeyPrefixes.TenantId + partitionKeyOptions.PartitionKeyTenantId; //* constrain search with L2
                        }
                        else if (partitionKeyOptions?.AcceptCrossPartitionQueryCost != true)
                        {
                            throw new CircuitException(
                                $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_ImmutableTenantId_Id)}."
                                + "You have not provided a Tenant Id which will result in a cross partition query across all Tenants. This will be a progressively expensive"
                                + "query as the system grows. Please acknowledge your acceptance of the query cost by setting the options => options.AcceptCrossPartitionQueryCost() parameter");
                        }
                    }
                    else
                    {
                        Guard.Against(
                            partitionKeyOptions.HasTenantIdOption(),
                            $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_ImmutableTenantId_Id)} "
                            + "without Hierarchical keys. Either use ReadById or ReadyByIds if you have them, alternative do not provide the TenantId and a fanout query will be used instead.");

                        /* if we are in synthetic mode, we can't compose the whole key we need to return nothing
                             this will result in an expensive full fan out query, but that is the problem with not using hierarchical keys
                             and you want to ensure limitless growth of logical partitions                          
                         */

                        return keys; //* reset to fan out
                    }
                }
                else if (timePeriodPartitionKeyAttributes.Any())
                {
                    Guard.Against(
                        partitionKeyOptions.HasTenantIdOption(),
                        $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_TimePeriod_Id)}."
                        + "This does not require a tenant partition key Option, please do not provide it.");
                    
                    if (useHierarchicalPartitionKeys)
                    {
                        keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
                        if (partitionKeyOptions?.PartitionKeyTimeInterval != null)
                        {
                            ValidateCorrectIntervalType(timePeriodPartitionKeyAttributes, partitionKeyOptions.PartitionKeyTimeInterval);
                            keys.Key2 = PartitionKeyPrefixes.TimePeriod + partitionKeyOptions.PartitionKeyTimeInterval; //* constrain to L2
                        }                       
                        else if (partitionKeyOptions?.AcceptCrossPartitionQueryCost != true)
                        {
                            throw new CircuitException(
                                $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_TimePeriod_Id)}."
                                + "You have not provided a Time Period which will result in a cross partition query across all Time Periods. This will be a progressively expensive"
                                + "query as the system grows. Please acknowledge your acceptance of the query cost by setting the options => options.AcceptCrossPartitionQueryCost() parameter");
                        }
                    }
                    else
                    {
                        Guard.Against(
                            partitionKeyOptions.HasTimePeriodOption(),
                            $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_TimePeriod_Id)} "
                            + "without Hierarchical keys. Either use ReadById or ReadyByIds if you have them, or do not provide the TimePeriod and a full cross partition query will be used.");

                        /* if we are in synthetic mode, we can't compose the whole key we need to return nothing
                             this will result in an expensive full fan out query, but that is the problem with not using hierarchical keys
                             and you want to ensure limitless growth of logical partitions */

                        return keys; //* broaden to fan out
                    }
                }
                else if (tenantAndTimePeriodPartitionKeyAttributes.Any())
                {
                    if (useHierarchicalPartitionKeys)
                    {
                        keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
                        if (partitionKeyOptions?.PartitionKeyTenantId != null)
                        {
                            keys.Key2 = PartitionKeyPrefixes.TenantId + partitionKeyOptions.PartitionKeyTenantId;
                            if (partitionKeyOptions.PartitionKeyTimeInterval != null)
                            {
                                ValidateCorrectIntervalType(tenantAndTimePeriodPartitionKeyAttributes, partitionKeyOptions?.PartitionKeyTimeInterval);

                                keys.Key3 = PartitionKeyPrefixes.TimePeriod + partitionKeyOptions.PartitionKeyTimeInterval; //* constrain to L3
                            } else if (partitionKeyOptions?.AcceptCrossPartitionQueryCost != true)
                            {
                                throw new CircuitException(
                                    $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_ImmutableTenantId_TimePeriod)}."
                                    + "You have not provided a Time Period which will result in a cross partition query across all Time Periods. This will be a progressively expensive"
                                    + "query as the system grows. Please acknowledge your acceptance of the query cost by setting the options => options.AcceptCrossPartitionQueryCost() parameter");
                            }
                        } else if (partitionKeyOptions?.AcceptCrossPartitionQueryCost != true)
                        {
                            throw new CircuitException(
                                $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute of type {nameof(PartitionKey__Type_ImmutableTenantId_TimePeriod)}."/**/
                                + "You have not provided a Tenant Id which will result in a cross partition query across all Tenants. This will be a progressively expensive"
                                + "query as the system grows. Please acknowledge your acceptance of the query cost by setting the options => options.AcceptCrossPartitionQueryCost() parameter");
                        }
                        
                    }
                    else
                    {
                        /* if we are in synthetic mode, and we can't compose the whole key we need to return nothing
                             this will result in an expensive full fan out query, but that is the problem with not using hierarchical keys
                             and you want to ensure limitless growth of logical partitions */
                        if (partitionKeyOptions.HasNotSpecifiedAnyOptions())
                        {
                            return keys; //* broaden to fan out
                        }
                        else if (partitionKeyOptions.HasSpecifiedBothOptions())
                        {
                            ValidateCorrectIntervalType(tenantAndTimePeriodPartitionKeyAttributes, partitionKeyOptions?.PartitionKeyTimeInterval);
                            
                            //* force exact key usage
                            keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
                            keys.Key2 = PartitionKeyPrefixes.TenantId + partitionKeyOptions.PartitionKeyTenantId;
                            keys.Key3 = PartitionKeyPrefixes.TimePeriod + partitionKeyOptions.PartitionKeyTimeInterval; //* constrain to L3
                        }
                        else
                        {
                            //* must always provide in the options when querying by id if the class requires it
                            throw new CircuitException(
                                $"You are querying a class type {typeof(T).Name} which has a Partition Key attribute requiring a Tenant Id and a Time Period but you have not provided these in the query options. "
                                + "Please use the options => option.ProvidePartitionKeyValues(tenantId, timePeriod) method on this query.");
                        }
                    }
                }
                else
                {
                    throw new CircuitException("There should always be a partition key attribute on every class, a guard has failed.");
                }

                return keys;
            }
        }

        public static HierarchicalPartitionKey GetKeysForNewModel<T>(T instance, bool useHierarchicalPartitionKeys) where T : IAggregate
        {
            return GenerateHierarchicalKeys(instance, useHierarchicalPartitionKeys);

            static HierarchicalPartitionKey GenerateHierarchicalKeys(T newInstance, bool useHierarchicalPartitionKeys)
            {
                Guard.Against(newInstance.id == Guid.Empty, "Instance must have Id set before you can create Partition Keys");

                var keys = new HierarchicalPartitionKey();

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

                    keys.Key1 = "sh";
                    keys.Key2 = "ar";
                    keys.Key3 = "ed";
                }
                else if (idPartitionKeyAttributes.Any())
                {
                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
                    keys.Key2 = PartitionKeyPrefixes.IdOptional + newInstance.id;
                    keys.Key3 = "_na";
                }
                else if (tenantPartitionKeyAttributes.Any())
                {
                    var attribute = tenantPartitionKeyAttributes.Single();
                    var tenantProperty = typeof(T).GetProperty(attribute.PropertyWithTenantId);
                    var tenantString = ValidateTenantAttributeExistsAndHasAValue(newInstance, tenantProperty);

                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
                    keys.Key2 = PartitionKeyPrefixes.TenantId + tenantString;
                    keys.Key3 = PartitionKeyPrefixes.IdOptional + newInstance.id;
                }
                else if (timePeriodPartitionKeyAttributes.Any())
                {
                    var attribute = timePeriodPartitionKeyAttributes.Single();
                    var timePeriodProperty = typeof(T).GetProperty(attribute.PropertyWithDateTime);
                    var timePeriodString = ValidateTimePeriodAttributeExistsAndHasAValue(newInstance, timePeriodProperty, attribute.PartitionKeyTimeInterval);

                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
                    keys.Key2 = PartitionKeyPrefixes.TimePeriod + timePeriodString;
                    keys.Key3 = PartitionKeyPrefixes.IdOptional + newInstance.id;
                }
                else if (tenantAndTimePeriodPartitionKeyAttributes.Any())
                {
                    var attribute = tenantAndTimePeriodPartitionKeyAttributes.Single();
                    var tenantProperty = typeof(T).GetProperty(attribute.PropertyWithTenantId);
                    var tenantString = ValidateTenantAttributeExistsAndHasAValue(newInstance, tenantProperty);

                    var timePeriodProperty = typeof(T).GetProperty(attribute.PropertyWithDateTime);
                    var timePeriodString = ValidateTimePeriodAttributeExistsAndHasAValue(newInstance, timePeriodProperty, attribute.PartitionKeyTimeInterval);

                    keys.Key1 = PartitionKeyPrefixes.Type + typeof(T).Name;
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

        private static bool HasNotSpecifiedAnyOptions(this IPartitionKeyOptionsLibrarySide options)
        {
            return !options.HasTenantIdOption() && !options.HasTimePeriodOption();
        }

        private static bool HasSpecifiedAtLeastOneOption(this IPartitionKeyOptionsLibrarySide options)
        {
            return options.HasTenantIdOption() || options.HasTimePeriodOption();
        }

        private static bool HasSpecifiedBothOptions(this IPartitionKeyOptionsLibrarySide options)
        {
            return options.HasTenantIdOption() && options.HasTimePeriodOption();
        }

        private static bool HasTenantIdOption(this IPartitionKeyOptionsLibrarySide options)
        {
            return options != null && !string.IsNullOrWhiteSpace(options.PartitionKeyTenantId);
        }

        private static bool HasTimePeriodOption(this IPartitionKeyOptionsLibrarySide options)
        {
            return options != null && !string.IsNullOrWhiteSpace(options.PartitionKeyTimeInterval);
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
        
        public static class ErrorCodes
        {
            public static readonly Guid UsedIncorrectTimeInterval = Guid.Parse("2EDA6D08-C034-4925-AD58-84404F851202");
        }

        private static void ValidateCorrectIntervalType(IEnumerable<Attribute> attributes, string intervalString)
        {
            var attribute = attributes.Single() as IPartitionKeyWithTimePeriod;
            if (attribute != null)
            {
                var intervalType = attribute.PartitionKeyTimeInterval;
                var warning =
                    $"You have provided an interval value {intervalString} that does not match that format. Please check Interval class used in the options for this query";
                 
                if (intervalType == PartitionKeyTimeIntervalEnum.Year)
                {
                    Guard.Against(
                        !YearInterval.IsValidString(intervalString),
                        $"The aggregate type you have used has a Partition Key with a Time Interval segment of type YEAR. {warning}", ErrorCodes.UsedIncorrectTimeInterval);
                }
                else if (intervalType == PartitionKeyTimeIntervalEnum.Month)
                {
                    Guard.Against(
                        !MonthInterval.IsValidString(intervalString),
                        $"The aggregate type you have used has a Partition Key with a Time Interval segment of type MONTH. {warning}",ErrorCodes.UsedIncorrectTimeInterval);
                }
                else if (intervalType == PartitionKeyTimeIntervalEnum.Week)
                {
                    Guard.Against(
                        !WeekInterval.IsValidString(intervalString),
                        $"The aggregate type you have used has a Partition Key with a Time Interval segment of type WEEK. {warning}",ErrorCodes.UsedIncorrectTimeInterval);
                }
                else if (intervalType == PartitionKeyTimeIntervalEnum.Day)
                {
                    Guard.Against(
                        !DayInterval.IsValidString(intervalString),
                        $"The aggregate type you have used has a Partition Key with a Time Interval segment of type DAY. {warning}",ErrorCodes.UsedIncorrectTimeInterval);
                }
                else if (intervalType == PartitionKeyTimeIntervalEnum.Hour)
                {
                    Guard.Against(
                        !HourInterval.IsValidString(intervalString),
                        $"The aggregate type you have used has a Partition Key with a Time Interval segment of type HOUR. {warning}",ErrorCodes.UsedIncorrectTimeInterval);
                }
                else if (intervalType == PartitionKeyTimeIntervalEnum.Minute)
                {
                    Guard.Against(
                        !MinuteInterval.IsValidString(intervalString),
                        $"The aggregate type you have used has a Partition Key with a Time Interval segment of type MINUTE. {warning}",ErrorCodes.UsedIncorrectTimeInterval);
                }
            }
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
                var timePeriodString = interval == PartitionKeyTimeIntervalEnum.Year ? YearInterval.FromDateTime(timePeriodPropertyValueNonNull).ToString() :
                                       interval == PartitionKeyTimeIntervalEnum.Month ? MonthInterval.FromDateTime(timePeriodPropertyValueNonNull).ToString() :
                                       interval == PartitionKeyTimeIntervalEnum.Week ? WeekInterval.FromDateTime(timePeriodPropertyValueNonNull).ToString() :
                                       interval == PartitionKeyTimeIntervalEnum.Day ? DayInterval.FromDateTime(timePeriodPropertyValueNonNull).ToString() :
                                       interval == PartitionKeyTimeIntervalEnum.Hour ? HourInterval.FromDateTime(timePeriodPropertyValueNonNull).ToString() :
                                       interval == PartitionKeyTimeIntervalEnum.Minute ? MinuteInterval.FromDateTime(timePeriodPropertyValueNonNull).ToString() :
                                       throw new ArgumentOutOfRangeException();
                return timePeriodString;
            }
        }
        
        public static string GetLongId(this IAggregate aggregate)
        {
            string Base64Encode(string plainText) {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }

            var longId = !aggregate.PartitionKeys.IsEmpty() ? Base64Encode(aggregate.PartitionKeys.ToSyntheticKeyString() + PartitionKeyPrefixes.IdRequired + aggregate.id) : Base64Encode(PartitionKeyPrefixes.IdRequired + aggregate.id);
            return longId;
        }

        public static Aggregate.PartitionedId DestructurePartitionedIdString(string longId)
        {
            string Base64Decode(string base64EncodedData)
            {
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }

            longId = Base64Decode(longId);
            if (longId.StartsWith("shared"))
            {
                return new Aggregate.PartitionedId()
                {
                    Id = Guid.Parse(longId.SubstringAfter("__"))
                };
            }
            var regex = new Regex(
                "^(?'type'_tp_[A-Za-z0-9]+)?(?'tenant'_tn_[A-Za-z0-9-]+)?(?'timeperiod'_tm_[A-Za-z0-9:]+)?(?'id'_id_[A-Za-z0-9-]+)?(_na)?(?'idrequired'__[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?)$");
            var result = regex.Match(longId);
            
            Guard.Against(result.Success == false || result.Groups["idrequired"].Success == false, "The longId value used has an invalid format.");

            var partitionedId = new Aggregate.PartitionedId();
            if (result.Groups["type"].Success) partitionedId.Type = result.Groups["type"].Value.SubstringAfter(PartitionKeyPrefixes.Type);
            if (result.Groups["tenant"].Success) partitionedId.TenantId = Guid.Parse(result.Groups["tenant"].Value.SubstringAfter(PartitionKeyPrefixes.TenantId));
            if (result.Groups["timeperiod"].Success)
                partitionedId.TimePeriod = TimeIntervalFromString(result.Groups["timeperiod"].Value.SubstringAfter(PartitionKeyPrefixes.TimePeriod));
            //* id rather than idrequired is used to increase cardinality in the partition key in some cases, but is always a duplicate of required 
            partitionedId.Id = Guid.Parse(result.Groups["idrequired"].Value.SubstringAfter(PartitionKeyPrefixes.IdRequired));

            return partitionedId;

            IPartitionKeyTimeInterval TimeIntervalFromString(string s)
            {
                if (MinuteInterval.IsValidString(s))
                {
                    return MinuteInterval.FromIntervalParts(IntervalParts.FromString(s));
                }

                if (HourInterval.IsValidString(s))
                {
                    return HourInterval.FromIntervalParts(IntervalParts.FromString(s));
                }

                if (DayInterval.IsValidString(s))
                {
                    return DayInterval.FromIntervalParts(IntervalParts.FromString(s));
                }

                if (WeekInterval.IsValidString(s))
                {
                    return WeekInterval.FromIntervalParts(IntervalParts.FromString(s));
                }

                if (MonthInterval.IsValidString(s))
                {
                    return MonthInterval.FromIntervalParts(IntervalParts.FromString(s));
                }

                if (YearInterval.IsValidString(s))
                {
                    return YearInterval.FromIntervalParts(IntervalParts.FromString(s));
                }

                throw new CircuitException("Time interval string does not match any known format");
            }
        }

        public static class PartitionKeyPrefixes
        {
            public const string IdOptional = "_id_";

            public const string TenantId = "_tn_";

            public const string TimePeriod = "_tm_";

            public const string Type = "_tp_";

            public const string IdRequired = "__";
        }
    }
}