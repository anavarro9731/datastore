
namespace DataStore.Tests.Models
{
    #region

    using System;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PartitionKeys;

    #endregion

    [PartitionKey__Type_Id]
    public class ProjectTask : Aggregate
    {
        public ProjectTask(string name, Guid myId, Guid projectId)
        {
            ProjectId = projectId;
            Name = name;
            id = myId;
        }

        public ProjectTask()
        {
        }

        [ScopeObjectReference(typeof(Project))]
        public Guid ProjectId { get; set; }

        public string Name { get; set; }
    }
}