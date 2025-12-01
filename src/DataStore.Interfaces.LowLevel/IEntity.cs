namespace DataStore.Interfaces.LowLevel
{
    public interface IEntity : IHaveAUniqueId, IRememberWhenIWasCreated, IHaveSchema, IDatastoreSerializable
    {
    }

    public interface IDatastoreSerializable
    {
    }
}