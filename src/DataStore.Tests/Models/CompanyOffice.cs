namespace DataStore.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using global::DataStore.Interfaces.LowLevel;

    public class CompanyOffice : Aggregate
    {
        public CompanyOffice(string name, Guid myId, Guid companyDivisionId)
        {
            CompanyDivisionIds.Add(companyDivisionId);
            Name = name;
            id = myId;
        }
        
        public CompanyOffice(string name, Guid myId, List<Guid> companyDivisionIds)
        {
            CompanyDivisionIds.AddRange(companyDivisionIds);
            Name = name;
            id = myId;
        }

        public CompanyOffice()
        {
        }

        [ScopeObjectReference(typeof(CompanyDivision))]
        public List<Guid> CompanyDivisionIds { get; set; } = new List<Guid>();

        public string Name { get; set; }
    }
}