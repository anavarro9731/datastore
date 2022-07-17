namespace DataStore.Interfaces.Options.ClientSide
{
    using System;
    using DataStore.Interfaces.LowLevel.Permissions;

    public class CreateOptionsClientSide : CreateOptionsClientSideBase
    {
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

        public void CreateReadonly()
        {
            LibrarySide.SetReadonlyFlag = true;
        }
    }
}