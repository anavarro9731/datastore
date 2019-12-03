namespace DataStore.Tests.Models
{
    using System;
    using global::DataStore.Interfaces.LowLevel;

    public class CompanyOffice : Aggregate
    {
        [ScopeObjectReference(typeof(CompanyDivision))]
        public Guid CompanyDivisionId { get; set; }

        public string Name { get; set; }

        public CompanyOffice(string name, Guid myId, Guid companyDivisionId)
        {
            this.CompanyDivisionId = companyDivisionId;
            this.Name = name;
            id = myId;
        }

        public CompanyOffice()
        {
        }
    }
}