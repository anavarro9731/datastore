namespace DataStore.DataAccess.Interfaces
{
    using System;

    public interface IDataStoreWriteOnlyScoped<T> :
        IDataStoreCreateCapabilitiesScoped<T>,
        IDataStoreDeleteCapabilitiesScoped<T>,
        IDataStoreUpdateCapabilitiesScoped<T>,
        IDisposable where T : IAggregate, new()

    {
    }
}