namespace DataStore.DataAccess.Interfaces
{
    public interface IAggregate : IHaveScope, IEntity
    {
        bool Active { get; set; }

        bool Hidden { get; set; }

        bool ReadOnly { get; set; }

        int ScopeObjectIdCount { get; }

        void SoftDelete();

        void WalkGraphAndUpdateEntityMeta();
    }
}