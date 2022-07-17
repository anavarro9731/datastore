namespace DataStore.Interfaces.Options.ClientSide.Interfaces
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface ISecurityOptionsClientSide
    {
        void AuthoriseFor(IIdentityWithDatabasePermissions identity);

        void BypassSecurity(string reason);
    }
}