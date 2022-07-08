namespace DataStore.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeyAttributes;

    
    public class Car : Aggregate
    {
        public string FriendlyId { get; set; }

        public string Make { get; set; }

        [ScopeObjectReference(typeof(CompanyOffice))]
        public Guid? OfficeId { get; set; }

        public List<Wheel> Wheels { get; set; } = new List<Wheel>();

        public int Year { get; set; }

        public class Wheel : Entity
        {
            public string FriendlyId { get; set; }

            public int RimSize { get; set; } = 15;
        }
    }
}