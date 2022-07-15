namespace DataStore.Tests.Models
{
    #region

    using System;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;

    #endregion

    [PartitionKey__Type_Id]
    public class Company : Aggregate
    {
        public Company(string name, Guid myId)
        {
            Name = name;
            id = myId;
        }

        public Company()
        {
        }

        public string Name { get; set; }
    }
}