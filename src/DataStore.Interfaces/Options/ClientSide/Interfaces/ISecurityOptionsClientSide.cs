namespace DataStore.Interfaces.Options.ClientSide.Interfaces
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface ISecurityOptionsClientSide<T> where T: ISecurityOptionsClientSide<T>
    {
        T AuthoriseFor(IIdentityWithDatabasePermissions identity);

        T BypassSecurity(string reason);
    }
}