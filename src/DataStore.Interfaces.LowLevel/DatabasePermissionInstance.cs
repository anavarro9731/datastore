namespace DataStore.Interfaces.LowLevel
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel.Permissions;

    public class DatabasePermissionInstance : DatabasePermission
    {
        public DatabasePermissionInstance(DatabasePermission databasePermission, List<DatabaseScopeReference> scopeReferences)
            : base(databasePermission.DisplayOrder, databasePermission.Id, databasePermission.PermissionName, databasePermission.PermissionRequiredToAdministerThisPermission)
        {
            ScopeReferences = scopeReferences;
        }

        public List<DatabaseScopeReference> ScopeReferences { get; set; }
    }
}