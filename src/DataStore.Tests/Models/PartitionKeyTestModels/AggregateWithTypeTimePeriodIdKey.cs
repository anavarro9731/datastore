namespace DataStore.Tests.Models.PartitionKeyTestModels
{
    #region

    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;

    #endregion

    [PartitionKey__Type_TimePeriod_Id(nameof(Created), PartitionKeyTimeIntervalEnum.Day)]
    public class AggregateWithTypeTimePeriodIdKey : Aggregate
    {
        public int TestValue { get; set; }
    }
}