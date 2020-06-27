namespace DataStore.Interfaces.LowLevel.Permissions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;

    public interface IIdentityWithDatabasePermissions : IHaveAUniqueId
    {
        List<DatabasePermissionInstance> DatabasePermissions { get; set; }
    }

    public static class IdentityWithDatabasePermissionsExtensions
    {
        public static bool HasDatabasePermission(this IIdentityWithDatabasePermissions identity, DatabasePermission permission)
        {
            var count = identity.DatabasePermissions.Count(p => p.Id == permission.Id);

            if (count == 1)
            {
                return true;
            }

            if (count > 1)
            {
                throw new SecurityException("User has the same DatabasePermission twice instead the scopes should be merged");
            }

            return false;
        }
    }
}