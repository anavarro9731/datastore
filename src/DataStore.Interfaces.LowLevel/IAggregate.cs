namespace DataStore.Interfaces.LowLevel
{
    using CircuitBoard.Permissions;

    public interface IAggregate : IHaveScope, IEntity, IRememberWhenIWasModified
    {
        bool Active { get; set; }

        bool ReadOnly { get; set; }
    }
}