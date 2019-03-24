namespace DataStore.Tests.Models
{
    using global::DataStore.Providers.CosmosDb;

    public class Car : CosmosAggregate
    {
        public string Make { get; set; }

        public int Year { get; set; }
    }
}