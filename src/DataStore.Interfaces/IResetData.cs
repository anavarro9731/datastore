namespace DataStore.Interfaces
{
    #region

    using System.Threading.Tasks;

    #endregion

    public interface IResetData
    {
        Task NonTransactionalReset();
    }
}