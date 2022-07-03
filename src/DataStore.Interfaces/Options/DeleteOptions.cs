namespace DataStore.Interfaces.Options
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public abstract class DeleteOptionsClientSide
    {
        public static implicit operator DeleteOptionsLibrarySide(DeleteOptionsClientSide options) => options.LibrarySide;

        protected DeleteOptionsClientSide(DeleteOptionsLibrarySide librarySide)
        {
            LibrarySide = librarySide;
        }

        protected DeleteOptionsLibrarySide LibrarySide { get; }

        //* visible members

        public abstract void AuthoriseFor(IIdentityWithDatabasePermissions identity);

        public abstract void BypassSecurity(string reason);

        public abstract void Permanently();
    }

    public class DeleteOptionsLibrarySide  : ISecurityOptions
    {
        public IIdentityWithDatabasePermissions Identity { get; set; }

        public bool IsHardDelete { get; set; }

        public bool BypassSecurity { get; set; }
    }
}