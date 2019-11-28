namespace DataStore.Tests.Models
{
    using System;
    using global::DataStore.Interfaces.LowLevel;

    public class Company : Aggregate
    {
        public string Name { get; set; }

        public Company(string name, Guid myId)
        {
            this.Name = name;
            id = myId;
        }

        public Company()
        {
        }
    }
}