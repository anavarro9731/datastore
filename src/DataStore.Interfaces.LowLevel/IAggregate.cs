namespace DataStore.Interfaces.LowLevel
{
    public interface IAggregate : IHaveScope, IEntity, IHaveSchema, IRememberWhenIWasModified
    {
        bool Active { get; set; }

        bool ReadOnly { get; set; }
    }
}