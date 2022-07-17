namespace DataStore.Interfaces.Options.ClientSide
{
    using System;
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.ClientSide.Interfaces;
    using DataStore.Interfaces.Options.LibrarySide;

    public abstract class UpdateOptionsClientSideBase : ISecurityOptionsClientSide
    {
        protected UpdateOptionsClientSideBase()
        {
            LibrarySide = new UpdateOptionsLibrarySide();
        }
        protected UpdateOptionsLibrarySide LibrarySide { get; }

        public static implicit operator UpdateOptionsLibrarySide(UpdateOptionsClientSideBase options)
        {
            return options.LibrarySide;
        }

        public void AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
        }

        public void BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            }

            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
        }

        public void DisableOptimisticConcurrency()
        {
            LibrarySide.OptimisticConcurrency = false;
        }

        public void OverwriteReadonly()
        {
            LibrarySide.AllowReadonlyOverwriting = true;
        }
        
    }
}