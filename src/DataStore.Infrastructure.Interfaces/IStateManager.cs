namespace Infrastructure.HandlerServiceInterfaces
{
    using System;

    public interface IStateManager : IDisposable
    {
        Guid TransactionId { get; set; }

        void SubmitChanges();
    }
}