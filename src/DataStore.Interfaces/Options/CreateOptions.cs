namespace DataStore.Interfaces.Options
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public class CreateOptionsLibrarySide
    {
        public IIdentityWithDatabasePermissions Identity { get; set; }

        public bool SetReadonlyFlag { get; set; }
    }

    public abstract class CreateOptionsClientSide
    {
        public static implicit operator CreateOptionsLibrarySide(CreateOptionsClientSide options) => options.LibrarySide;

        protected CreateOptionsClientSide(CreateOptionsLibrarySide librarySide)
        {
            LibrarySide = librarySide;
        }

        protected CreateOptionsLibrarySide LibrarySide { get; }

        //* visible members
        public abstract void AuthoriseFor(IIdentityWithDatabasePermissions identity);

        public abstract void CreateReadonly();
    }
}