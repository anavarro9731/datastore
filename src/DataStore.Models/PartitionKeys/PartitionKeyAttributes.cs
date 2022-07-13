namespace DataStore.Models.PartitionKeys
{
    using System;
    using DataStore.Interfaces;
    using DataStore.Models.PureFunctions;

    /// <summary>
    /// When no tenantId exists
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PartitionKey__Shared : Attribute
    {
    }
    
    
    /// <summary>
    /// When no tenantId exists
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PartitionKey__Type_Id : Attribute
    {
    }
    
    /// <summary>
    /// When you have an immutable tenant id that you will have present at time of query 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PartitionKey__Type_ImmutableTenantId_Id : Attribute
    {
        public string PropertyWithTenantId { get; }

        public PartitionKey__Type_ImmutableTenantId_Id(string propertyWithTenantId)
        {
            Guard.Against(string.IsNullOrWhiteSpace(propertyWithTenantId), $"You must provide a {propertyWithTenantId} to use this attribute. Use {nameof(PartitionKey__Type_Id)} instead if you don't have one.");
            PropertyWithTenantId = propertyWithTenantId;
        }
    }

    public interface IPartitionKeyWithTimePeriod
    {
        string PropertyWithDateTime { get; }
        
        PartitionKeyTimeIntervalEnum PartitionKeyTimeInterval { get; }
    }

    /// <summary>
    /// When you want to query by time period and there is no possible tenant id on this aggregate (it is global) 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PartitionKey__Type_TimePeriod_Id : Attribute, IPartitionKeyWithTimePeriod
    {
        public string PropertyWithDateTime { get; }

        public PartitionKeyTimeIntervalEnum PartitionKeyTimeInterval { get; }

        public PartitionKey__Type_TimePeriod_Id( string propertyWithDateTime, PartitionKeyTimeIntervalEnum partitionKeyTimeIntervalType)
        {
            Guard.Against(string.IsNullOrWhiteSpace(propertyWithDateTime), $"You must provide a {propertyWithDateTime} to use this attribute. Use {nameof(PartitionKey__Type_Id)} instead if you don't have one.");
            PropertyWithDateTime = propertyWithDateTime;
            PartitionKeyTimeInterval = partitionKeyTimeIntervalType;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PartitionKey__Type_ImmutableTenantId_TimePeriod : Attribute, IPartitionKeyWithTimePeriod
    {
        public string PropertyWithTenantId { get; }

        public string PropertyWithDateTime { get; }

        public PartitionKeyTimeIntervalEnum PartitionKeyTimeInterval { get; }

        public PartitionKey__Type_ImmutableTenantId_TimePeriod(string propertyWithTenantId, string propertyWithDateTime, PartitionKeyTimeIntervalEnum partitionKeyTimeInterval)
        {
            Guard.Against(string.IsNullOrWhiteSpace(propertyWithDateTime), $"You must provide a {propertyWithDateTime} to use this attribute. Use {nameof(PartitionKey__Type_ImmutableTenantId_Id)} instead if you don't have one.");
            Guard.Against(string.IsNullOrWhiteSpace(propertyWithTenantId), $"You must provide a {propertyWithTenantId} to use this attribute. Use {nameof(PartitionKey__Type_TimePeriod_Id)} instead if you don't have one.");
            PropertyWithTenantId = propertyWithTenantId;
            PropertyWithDateTime = propertyWithDateTime;
            PartitionKeyTimeInterval = partitionKeyTimeInterval;        
        }
    }
    
}