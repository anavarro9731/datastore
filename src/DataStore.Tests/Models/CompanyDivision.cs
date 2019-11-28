namespace DataStore.Tests.Models
{
    using System;
    using global::DataStore.Interfaces.LowLevel;

    public class CompanyDivision : Aggregate
    {
        public Guid CompanyId { get; set; }

        public string Name { get; set; }

        public CompanyDivision(string name, Guid myId, Guid companyId)
        {
            this.Name = name;
            this.CompanyId = companyId;
            id = myId;
        }

        public CompanyDivision()
        {
        }
    }
}