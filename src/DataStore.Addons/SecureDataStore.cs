namespace DataStore.Addons
{
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using Interfaces;

    public class SecureDataStore : ISecureDataStore
    {
        public SecureDataStore(IDataStore dataStore, IUserWithPermissions user)
        {
            DataStore = dataStore;
            SecuredAgainst = user;
        }

        public IDataStore DataStore { get; }

        public void Dispose()
        {
            DataStore.Dispose();
        }

        public IUserWithPermissions SecuredAgainst { get; }

        public async Task CommitChanges()
        {
            await DataStore.CommitChanges();
        }
    }
}