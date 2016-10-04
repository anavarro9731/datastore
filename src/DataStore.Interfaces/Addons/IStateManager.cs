namespace DataStore.DataAccess.Interfaces.Addons
{
    using System;

    public interface IStateManager : IDisposable
    {
        Guid TransactionId { get; set; }

        void SubmitChanges();
    }
}