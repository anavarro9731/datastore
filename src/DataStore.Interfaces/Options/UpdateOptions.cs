namespace DataStore.Interfaces.Options
{
    using DataStore.Interfaces.LowLevel.Permissions;

    public abstract class UpdateOptionsClientSide
    {
        public static implicit operator UpdateOptionsLibrarySide(UpdateOptionsClientSide options) => options.LibrarySide;

        protected UpdateOptionsClientSide(UpdateOptionsLibrarySide librarySide)
        {
            LibrarySide = librarySide;
        }

        protected UpdateOptionsLibrarySide LibrarySide { get; }

        public abstract void AuthoriseFor(IIdentityWithDatabasePermissions identity);

        public abstract void DisableOptimisticConcurrency();

        public abstract void OverwriteReadonly();
    }

    public class UpdateOptionsLibrarySide
    {
        public bool AllowReadonlyOverwriting { get; set; }

        public IIdentityWithDatabasePermissions Identity { get; set; }

        public bool OptimisticConcurrency { get; set; } = true;
    }
}