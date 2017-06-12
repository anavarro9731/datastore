using DataStore.Interfaces.LowLevel;

namespace DataStore.Tests.Models
{
    public class Car : Aggregate
    {
        public string Make { get; set; }
    }
}