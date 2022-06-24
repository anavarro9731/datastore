namespace DataStore.Interfaces.LowLevel
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel.Permissions;

    public class DatabasePermission : SecurableOperation
    {
        public DatabasePermission(SecurableOperation securableOperation, List<AggregateReference> scopeReferences)
            : base(securableOperation.PermissionName)
        {
            ScopeReferences = scopeReferences;
        }

        public List<AggregateReference> ScopeReferences { get; set; }
    }
}
