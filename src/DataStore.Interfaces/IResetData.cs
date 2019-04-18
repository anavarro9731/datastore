namespace DataStore.Interfaces
{
    using System.Threading.Tasks;

    public interface IResetData
    {
        Task NonTransactionalReset();
    }
}