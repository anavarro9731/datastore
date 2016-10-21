using DataStore.DataAccess.Models;

namespace DataStore.Tests.Models
{
    public class Car : Aggregate
    {
        public string Make { get; set; }
    }
}