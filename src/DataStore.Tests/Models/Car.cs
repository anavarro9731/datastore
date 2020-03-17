namespace DataStore.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::DataStore.Interfaces.LowLevel;

    public class Car : Aggregate
    {
        [ScopeObjectReference(typeof(Company))]
        public Guid? CompanyId { get; set; }

        [ScopeObjectReference(typeof(CompanyDivision))]
        public Guid? DivisionId { get; set; }

        public string FriendlyId { get; set; }

        public string Make { get; set; }

        [ScopeObjectReference(typeof(CompanyOffice))]
        public Guid? OfficeId { get; set; }

        public List<Wheel> Wheels { get; set; } = new List<Wheel>();

        public int Year { get; set; }

        public class Wheel : Entity
        {
            public int RimSize { get; set; } = 15;
            public string FriendlyId { get; set; }
        }
    }
}