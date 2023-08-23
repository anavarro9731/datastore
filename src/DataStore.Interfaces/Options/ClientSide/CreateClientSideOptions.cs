namespace DataStore.Interfaces.Options.ClientSide
{
    using System;
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.ClientSide.Interfaces;
    using DataStore.Interfaces.Options.LibrarySide;

    public class CreateClientSideOptions : ISecurityOptionsClientSide<CreateClientSideOptions>
    {
        public CreateClientSideOptions()
        {
            LibrarySide = new CreateOptionsLibrarySide();
        }

        protected CreateOptionsLibrarySide LibrarySide { get; }

        public static implicit operator CreateOptionsLibrarySide(CreateClientSideOptions options)
        {
            return options.LibrarySide;
        }

        public CreateClientSideOptions AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
            return this;
        }

        public CreateClientSideOptions BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            }

            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
            return this;
        }

        public CreateClientSideOptions CreateReadonly()
        {
            LibrarySide.SetReadonlyFlag = true;
            return this;
        }
    }
}