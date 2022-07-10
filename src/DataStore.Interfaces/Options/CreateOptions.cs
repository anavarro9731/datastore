namespace DataStore.Interfaces.Options
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public class CreateOptionsLibrarySide : ISecurityOptions, IQueryOptions
    {
        public bool BypassSecurity { get; set; }

        public IIdentityWithDatabasePermissions Identity { get; set; }

        public bool SetReadonlyFlag { get; set; }
    }

    public abstract class ClientSideCreateOptions
    {
        protected ClientSideCreateOptions(CreateOptionsLibrarySide librarySide)
        {
            LibrarySide = librarySide;
        }

        protected CreateOptionsLibrarySide LibrarySide { get; }

        public static implicit operator CreateOptionsLibrarySide(ClientSideCreateOptions options)
        {
            return options.LibrarySide;
        }

        //* visible members
        public abstract void AuthoriseFor(IIdentityWithDatabasePermissions identity);

        public abstract void BypassSecurity(string reason);

        public abstract void CreateReadonly();
    }
}