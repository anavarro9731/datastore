namespace DataStore.Interfaces
{
    using System;

    public interface IHaveAUniqueId
    {
        Guid id { get; set; }
    }
}