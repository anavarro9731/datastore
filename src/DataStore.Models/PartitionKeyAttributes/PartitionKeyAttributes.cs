namespace DataStore.Models.PartitionKeyAttributes
{
    using System;
    using DataStore.Models.PureFunctions;

    public enum PartitionKeyTimeIntervalEnum
    {
        Years,

        Months,

        Days,

        Seconds,

        Milliseconds
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
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PartitionKey__Type_ImmutableTenantId_TimePeriod : Attribute
    {
        public string PropertyWithTenantId { get; }

        public string PropertyWithDateTime { get; }

        public PartitionKeyTimeIntervalEnum PartitionKeyTimeInterval { get; }

        public PartitionKey__Type_ImmutableTenantId_TimePeriod(string propertyWithTenantId, string propertyWithDateTime, PartitionKeyTimeIntervalEnum partitionKeyTimeInterval)
        {
            Guard.Against(string.IsNullOrWhiteSpace(propertyWithDateTime), $"You must provide a {propertyWithDateTime} to use this attribute. Use {nameof(PartitionKey__Type_ImmutableTenantId_Id)} or {nameof(PartitionKey__Type_Id)} instead if you don't have one.");
            PropertyWithTenantId = propertyWithTenantId;
            PropertyWithDateTime = propertyWithDateTime;
            PartitionKeyTimeInterval = partitionKeyTimeInterval;
        }
    }
    
}