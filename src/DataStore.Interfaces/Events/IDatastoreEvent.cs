namespace DataStore.Interfaces.Events
{
    using System;

    public interface IDataStoreEvent
    {
        string TypeName { get; set; }
        string MethodCalled { get; set; }
        double QueryCost { get; set; }
        TimeSpan QueryDuration { get; set; }
    }
}