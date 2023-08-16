namespace DataStore.Interfaces.Options.LibrarySide.Interfaces
{
    #region

    using DataStore.Interfaces.LowLevel.Permissions;

    #endregion

    public interface ISecurityOptionsLibrarySide
    {
        IIdentityWithDatabasePermissions Identity { get; set; }

        bool BypassSecurity { get; set; }
        
    }
}