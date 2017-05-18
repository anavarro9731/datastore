namespace DataStore.Interfaces.Events
{
    using System;

    public interface IDataStoreOperation
    {
        string TypeName { get; set; }
        string MethodCalled { get; set; }
        DateTime Created { get; set; }
    }
}