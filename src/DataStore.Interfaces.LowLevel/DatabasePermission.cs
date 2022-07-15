namespace DataStore.Interfaces.LowLevel
{
    #region

    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel.Permissions;
    using Newtonsoft.Json;

    #endregion

    public class DatabasePermission : SecurableOperation
    {

        [JsonConstructor]
        internal DatabasePermission()
        {
            
        }
        
        public DatabasePermission(string securableOperation, List<AggregateReference> scopeReferences)
            : base(securableOperation)
        {
            if (scopeReferences != null) ScopeReferences = scopeReferences;
        }

        public List<AggregateReference> ScopeReferences { get; private set; } = new List<AggregateReference>();
    }
}