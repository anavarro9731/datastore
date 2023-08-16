namespace DataStore.Interfaces.Options.LibrarySide
{
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;

    public class CreateOptionsLibrarySide : ISecurityOptionsLibrarySide, IOptionsLibrarySide
    {
        public bool BypassSecurity { get; set; }

        public IIdentityWithDatabasePermissions Identity { get; set; }

        public bool SetReadonlyFlag { get; set; }
        
    }
}