namespace DataStore.Options
{
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Interfaces.Options;

    public class DefaultCreateOptions : CreateOptionsClientSide
    {
        public DefaultCreateOptions()
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
    }
}