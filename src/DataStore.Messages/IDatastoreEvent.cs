namespace DataStore.Messages
{
    using System;

    public interface IDataStoreEvent
    {
        double QueryCost { get; set; }
        TimeSpan QueryDuration { get; set; }
    }
}