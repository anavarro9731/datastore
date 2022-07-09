namespace DataStore.Tests.Models
{
    using System;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;

    [PartitionKey__Type_ImmutableTenantId_Id(nameof(CompanyDivision.CompanyId))]
    public class CompanyDivision : Aggregate
    {
        public CompanyDivision(string name, Guid myId, Guid companyId)
        {
            Name = name;
            CompanyId = companyId;
            id = myId;
        }

        public CompanyDivision()
        {
        }

        [ScopeObjectReference(typeof(Company))]
        public Guid CompanyId { get; set; }

        public string Name { get; set; }
    }
}