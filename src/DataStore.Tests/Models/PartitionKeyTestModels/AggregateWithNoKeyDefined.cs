
namespace DataStore.Tests.Models.PartitionKeyTestModels
{
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;

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