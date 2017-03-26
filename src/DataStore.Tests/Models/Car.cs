namespace DataStore.Tests.Models
{
    using global::DataStore.Models;

    public class Car : Aggregate
    {
        public string Make { get; set; }        
    }
}