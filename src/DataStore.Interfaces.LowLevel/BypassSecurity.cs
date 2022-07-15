namespace DataStore.Interfaces.LowLevel
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;

    #endregion

    [AttributeUsage(AttributeTargets.Class)]
    //* permission still required but no scope match is required
    public class BypassSecurity : Attribute
    {
        public List<SecurableOperation> ForTheseOperations { get; }

        public BypassSecurity(string reason, params string[] forTheseOperations)
        {
            //* reason is for readability of source code only
            ForTheseOperations = forTheseOperations.Select(x => new SecurableOperation(x)).ToList();
        }
    }
}