namespace DataStore.Options
{
    using System;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Interfaces.Options;

    public class DefaultClientSideCreateOptions : ClientSideCreateOptions
    {
        public DefaultClientSideCreateOptions()
            : base(new CreateOptionsLibrarySide())
        {
            /* use constructors on derived classes to input a more advanced library side
             which we could then cast to in the additional interface methods below to 
            set its advanced properties */
        }

        public override void AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
        }

        public override void CreateReadonly()
        {
            LibrarySide.SetReadonlyFlag = true;
        }

        public override void BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
        }
    }
}