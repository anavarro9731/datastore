namespace DataStore.Interfaces.Options
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface ISecurityOptions
    {
        IIdentityWithDatabasePermissions Identity { get; set; }

        bool BypassSecurity { get; set; }
    }
}