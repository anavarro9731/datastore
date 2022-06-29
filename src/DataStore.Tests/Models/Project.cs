
namespace DataStore.Tests.Models
{
    using System;
    using global::DataStore.Interfaces.LowLevel;

    /// <summary>
    /// $SUMMARY$
    /// </summary>
    public class Project : Aggregate
    {
        public Project(string name, Guid myId, Guid? companyDivisionId)
        {
            CompanyDivisionId = companyDivisionId;
            Name = name;
            id = myId;
        }

        public Project()
        {
        }

        [ScopeObjectReference(typeof(CompanyDivision))]
        public Guid? CompanyDivisionId { get; set; }

        public string Name { get; set; }
    }
}