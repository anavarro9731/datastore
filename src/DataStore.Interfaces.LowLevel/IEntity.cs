namespace DataStore.Interfaces.LowLevel
{
    public interface IEntity : IHaveAUniqueId, IRememberWhenIWasCreated, IHaveSchema
    {
        //required lowercase when a docdb partitionkey
        string schema { get; set; }
    }
}