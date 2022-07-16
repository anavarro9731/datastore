﻿namespace DataStore.Options
{
    #region

    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Interfaces.Options;

    #endregion

    public class DefaultClientSideDeleteOptions : ClientSideDeleteOptions
    {
        public DefaultClientSideDeleteOptions()
            : base(new DeleteOptionsLibrarySide())
        {
            /* use constructors on derived classes to input a more advanced library side
             which we could then cast to in the additional interface methods below to 
            set its advanced properties */
        }

        public override void AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
        }

        public override void Permanently()
        {
            LibrarySide.IsHardDelete = true;
        }
        
        public override void BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
        }
        
        public override void ProvidePartitionKeyValues(Guid tenantId)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
        }

        public override void ProvidePartitionKeyValues(PartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
        }

        public override void ProvidePartitionKeyValues(Guid tenantId, PartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
        }

        public override void AcceptCrossPartitionQueryCost()
        {
            LibrarySide.AcceptCrossPartitionQueryCost = true;
        }
    }
}