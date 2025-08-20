namespace DataStore.Tests.Models
{
    #region

    using System;
    using System.Collections.Generic;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Providers.CosmosDb;

    #endregion

    [PartitionKey__Type_Id]
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

        [SerialisedNames("Previous", "Names")]
        public class FancyWheel : Wheel
        {
            public string Coating { get; set; }
        }
     }
}