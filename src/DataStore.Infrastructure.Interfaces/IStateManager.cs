namespace Infrastructure.Interfaces
{
    using System;

    public interface IStateManager : IDisposable
    {
        Guid TransactionId { get; set; }

        void SubmitChanges();
    }
}