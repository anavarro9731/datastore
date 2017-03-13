namespace DataStore.Interfaces
{
    using ServiceApi.Interfaces.LowLevel;

    public interface IEntity : IHaveAUniqueId, IRememberWhenIWasChanged, IHaveSchema
    {
        dynamic More { get; set; }

        string Type { get; set; }

        void UpdateFromAnotherObject<T>(T source, params string[] exclude);
    }
}