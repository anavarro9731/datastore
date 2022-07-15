
namespace DataStore.Tests.Models.PartitionKeyTestModels
{
    #region

    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;

    #endregion

    public class AggregateWithNoKeyDefined : Aggregate
    {
    }
    
    [PartitionKey__Shared]
    [PartitionKey__Type_Id]
    public class AggregateWithTwoKeysDefined : Aggregate
    {
    }
    
    [PartitionKey__Type_ImmutableTenantId_Id("bad")]
    public class AggregateWithBadKeyDefined : Aggregate
    {
    }
}