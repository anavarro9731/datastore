namespace DataStore.Tests.Models
{
    using System;
    using global::DataStore.Interfaces.LowLevel;

    public class CompanyOffice : Aggregate
    {
        public CompanyOffice(string name, Guid myId, Guid companyDivisionId)
        {
            CompanyDivisionId = companyDivisionId;
            Name = name;
            id = myId;
        }

        public CompanyOffice()
        {
        }

        [ScopeObjectReference(typeof(CompanyDivision))]
        public Guid CompanyDivisionId { get; set; }

        public string Name { get; set; }
    }
}