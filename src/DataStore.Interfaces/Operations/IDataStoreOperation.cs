namespace DataStore.Interfaces.Operations
{
    #region

    using System;

    #endregion

    public interface IDataStoreOperation
    {
        DateTime Created { get; set; }

        string MethodCalled { get; set; }

        string TypeName { get; set; }
    }
}