namespace DataStore.DataAccess.Interfaces
{
    using System;

    public interface IHaveAUniqueId
    {
        Guid id { get; set; }
    }
}