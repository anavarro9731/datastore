namespace DataStore.Interfaces.LowLevel
{
    public interface IAggregate : IHaveScope, IEntity, IRememberWhenIWasModified
    {
        bool Active { get; set; }

        bool ReadOnly { get; set; }
    }
}