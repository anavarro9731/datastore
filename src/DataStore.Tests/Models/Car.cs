namespace DataStore.Tests.Models
{
    using global::DataStore.Interfaces.LowLevel;

    public class Car : Aggregate
    {
        public string FriendlyId { get; set; }

        public string Make { get; set; }

        public int Year { get; set; }
    }
}