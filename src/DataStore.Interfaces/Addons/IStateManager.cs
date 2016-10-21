namespace DataStore.DataAccess.Interfaces.Addons
{
    using System;
    using System.Threading.Tasks;

    public interface IStateManager
    {
        Guid TransactionId { get; set; }

        Task CommitChanges();
    }
}