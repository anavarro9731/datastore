namespace DataStore.Interfaces.Operations
{
    using System;

    public interface IDataStoreOperation
    {
        DateTime Created { get; set; }

        string MethodCalled { get; set; }

        string TypeName { get; set; }
    }
}