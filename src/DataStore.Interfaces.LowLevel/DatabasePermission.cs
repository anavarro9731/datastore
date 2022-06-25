namespace DataStore.Interfaces.LowLevel
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel.Permissions;
    using Newtonsoft.Json;

    public class DatabasePermission : SecurableOperation
    {

        [JsonConstructor]
        internal DatabasePermission()
        {
            
        }
        
        public DatabasePermission(SecurableOperation securableOperation, List<AggregateReference> scopeReferences)
            : base(securableOperation.PermissionName)
        {
            if (scopeReferences != null) ScopeReferences = scopeReferences;
        }

        public List<AggregateReference> ScopeReferences { get; private set; } = new List<AggregateReference>();
    }
}