namespace DataStore.Interfaces
{
    using ServiceApi.Interfaces.LowLevel;

    public interface IEntity : IHaveAUniqueId, IRememberWhenIWasCreated, IRememberWhenIWasModified, IHaveSchema
    {
        dynamic More { get; set; }

        string Type { get; set; }

        void UpdateFromAnotherObject<T>(T source, params string[] exclude);
    }
}