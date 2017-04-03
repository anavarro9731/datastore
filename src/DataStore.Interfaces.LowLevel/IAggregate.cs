namespace DataStore.Interfaces.LowLevel
{
    public interface IAggregate : IHaveScope, IEntity, IHaveSchema
    {
        bool Active { get; set; }

        bool ReadOnly { get; set; }
    }
}