namespace DataStore.Interfaces
{
    public interface IAggregate : IHaveScope, IEntity
    {
        bool Active { get; set; }

        bool ReadOnly { get; set; }

        void WalkGraphAndUpdateEntityMeta();
    }
}