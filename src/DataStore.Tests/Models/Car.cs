namespace DataStore.Tests.Models
{
    using global::DataStore.Interfaces.LowLevel;

    public class Car : Aggregate
    {
        public string Make { get; set; }
    }
}