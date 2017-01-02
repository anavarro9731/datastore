namespace DataStore.Addons.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface ISecureDataStore : IDisposable
    {
        IUserWithPermissions SecuredAgainst { get; }

        Task CommitChanges();
    }
}