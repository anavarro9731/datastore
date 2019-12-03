namespace DataStore.Tests.Models
{
    using System;
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

        public int Year { get; set; }
    }
}