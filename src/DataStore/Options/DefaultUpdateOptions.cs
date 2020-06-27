﻿namespace DataStore.Options
{
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Interfaces.Options;

    public class DefaultUpdateOptions : UpdateOptionsClientSide
    {
        public DefaultUpdateOptions()
            : base(new UpdateOptionsLibrarySide())
        {
            /* use constructors on derived classes to input a more advanced library side
             which we could then cast to in the additional interface methods below to 
            set its advanced properties */
        }

        public override void AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
        }

        public override void DisableOptimisticConcurrency()
        {
            LibrarySide.OptimisticConcurrency = false;
        }

        public override void OverwriteReadonly()
        {
            LibrarySide.AllowReadonlyOverwriting = true;
        }
    }
}