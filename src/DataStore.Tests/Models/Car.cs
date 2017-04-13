namespace DataStore.Tests.Models
{
    using global::DataStore.Models;
    using Interfaces.LowLevel;

    
    public class Car : Aggregate
    {
        public string Make { get; set; }        
    }
}