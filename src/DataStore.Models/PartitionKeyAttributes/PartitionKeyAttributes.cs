namespace DataStore.Models.PartitionKeyAttributes
{
    using System;

    // public class PartitionKeyTimeInterval : IPartitionKey
    // {
    //     public PartitionKeyTimeInterval(TimeIntervalPartitionKey attribute)
    //     {
    //         var intervalType = attribute.IntervalType;
    //         
    //         Value = intervalType == PartitionKeyTimeIntervalEnum.Years ? DateTime.UtcNow.Year.ToString() :
    //                 intervalType == PartitionKeyTimeIntervalEnum.Months ? DateTime.UtcNow.Year + DateTime.UtcNow.Month.ToString() :
    //                 intervalType == PartitionKeyTimeIntervalEnum.Days ? DateTime.UtcNow.Year + DateTime.UtcNow.DayOfYear.ToString() :
    //                 intervalType == PartitionKeyTimeIntervalEnum.Seconds ? DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() :
    //                 intervalType == PartitionKeyTimeIntervalEnum.Milliseconds ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() :
    //                 throw new ArgumentOutOfRangeException();
    //     }
    //
    //     public string Value { get; internal set; }
    // }
    //
    // public class PartitionKeyTenant : IPartitionKey
    // {
    //     public PartitionKeyTenant()
    //     {
    //     }
    //
    //     public string Value { get; }
    // }
    //
    // public interface IPartitionKey
    // {
    //     string Value { get; }
    // }

    public enum PartitionKeyTimeIntervalEnum
    {
        Years,

        Months,

        Days,

        Seconds,

        Milliseconds
    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PartitionKey_Type_AggregateId : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PartitionKey_Type_ImmutableTenantId_AggregateId : Attribute
    {
        public string PropertyForTenantId { get; }

        public PartitionKey_Type_ImmutableTenantId_AggregateId(string propertyForTenantId)
        {
            PropertyForTenantId = propertyForTenantId;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PartitionKey_Type_ImmutableTimePeriod_AggregateId : Attribute
    {
        public string PropertyForTimePeriod { get; }

        public PartitionKeyTimeIntervalEnum PartitionKeyTimeInterval { get; }

        public PartitionKey_Type_ImmutableTimePeriod_AggregateId(string propertyForTimePeriod, PartitionKeyTimeIntervalEnum partitionKeyTimeInterval)
        {
            PropertyForTimePeriod = propertyForTimePeriod;
            PartitionKeyTimeInterval = partitionKeyTimeInterval;
        }
    }
    
}