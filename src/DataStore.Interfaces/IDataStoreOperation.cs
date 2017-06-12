using System;

namespace DataStore.Interfaces
{
    public interface IDataStoreOperation
    {
        string TypeName { get; set; }
        string MethodCalled { get; set; }
        DateTime Created { get; set; }
    }
}