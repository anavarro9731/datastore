namespace DataStore.Interfaces.Options
{
    #region

    using DataStore.Interfaces.LowLevel.Permissions;

    #endregion

    public interface ISecurityOptions
    {
        IIdentityWithDatabasePermissions Identity { get; set; }

        bool BypassSecurity { get; set; }
    }
}