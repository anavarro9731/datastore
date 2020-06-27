namespace DataStore.Tests.Models
{
    using System;
    using global::DataStore.Interfaces.LowLevel;

    public class Company : Aggregate
    {
        public Company(string name, Guid myId)
        {
            Name = name;
            id = myId;
        }

        public Company()
        {
        }

        public string Name { get; set; }
    }
}