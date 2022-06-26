namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;
    using CircuitBoard;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Options;

    public class ControlFunctions
    {
        private readonly DataStore dataStore;

        public ControlFunctions(DataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public async Task<IEnumerable<T>> AuthoriseData<T>(
            IEnumerable<T> data,
            string requiredPermission,
            IIdentityWithDatabasePermissions identity) where T : class, IAggregate, new()
        {
            if (identity == null)
            {
                //* this should be checked by caller first
                throw new CircuitException(ErrorCodes.IdentityMissingWhenApplyingAuthorisation);
            }
            var result = await Authorise(identity, requiredPermission, data.Cast<IHaveScope>().ToList()).ConfigureAwait(false);

            return result.Cast<T>();
        }

        public async Task<T> AuthoriseDatum<T>(T data, string requiredPermissionWithScopeToData, IIdentityWithDatabasePermissions identity)
            where T : class, IAggregate, new()
        {
            var result = await AuthoriseData(
                             new[]
                             {
                                 data
                             },
                             requiredPermissionWithScopeToData,
                             identity).ConfigureAwait(false);

            return result.SingleOrDefault();
        }

        private async Task<List<IHaveScope>> Authorise(
            IIdentityWithDatabasePermissions user,
            string requiredPermission,
            List<IHaveScope> dataBeingQueried)
        {
            {
                if (user.HasDatabasePermission("*")) return dataBeingQueried; 
                /* wildcard means all permissions and ALSO all possible scopes, its an escape hatch for service accounts
                 or test accounts, if you want all permissions with limited scopes you need to add all the permissions separately */ 
                
                if (user.HasDatabasePermission(requiredPermission)) 
                    /* do they have the read, write, delete, etc capability regardless of scope
                     when using full rbac in soap, by default they will have all permissions, unless a specific restriction has been made */
                {
                    DatabasePermission
                        permissionInstance = user.DatabasePermissions.Single(x => x.PermissionName == requiredPermission);
                    
                    //* check that the scope of the data intersects the scope of the user for this operation
                    var authorised = await CheckScope(permissionInstance, this.dataStore.DataStoreOptions.Security, this.dataStore).ConfigureAwait(false);
                    if (authorised) return dataBeingQueried;
                }

                /* One consideration was whether to filter the data rather than throw an exception if there is a mismatch between the data
                    requested and the data authorised. However this would hide problems and make debugging hard and violate the principle of
                    least astonishment. 
                    If your queries are architect-ed correctly then there won't be any mismatch and therefore no performance hit and no security failure.                
                    */
                
                throw new CircuitException($"You require the {requiredPermission} permission with the appropriate scope.", ErrorCodes.MissingDbPermissions);
            }

            async Task<bool> CheckScope(DatabasePermission databasePermissionInstance, SecuritySettings settings, DataStore dataStore)
            {
                if (dataBeingQueried.Count == 0) return true;

                /*  
                DataStore with security on will enforce that all data that is not PublicScope has been scoped.                 
                */

                //* every datum needs to find an intersecting scope object with those of the users' permissions
                if (dataBeingQueried.All(data => data.ScopeReferences.Intersect(databasePermissionInstance.ScopeReferences).Any())) return true;

                /*
                If there is not a direct intersection between all of the data and the users' permissions there may be an indirect match
                via a scope hierarchy. Extrapolate all indirect scopes and check against this. A bit more costly which is why we don't default
                to this approach.
                 */

                if (settings.ScopeHierarchy != null && 
                    (await settings.ScopeHierarchy.GetDataAndPermissionScopeIntersection(dataBeingQueried, databasePermissionInstance.ScopeReferences, dataStore).ConfigureAwait(false)).Count() == dataBeingQueried.Count)
                {
                    return true;
                }

                return false;
            }
        }
    }
    public static class IdentityWithDatabasePermissionsExtensions
    {
        public static bool HasDatabasePermission(this IIdentityWithDatabasePermissions identity, string permission)
        {
            var count = identity.DatabasePermissions.Count(p => p.PermissionName == permission);

            if (count == 1)
            {
                return true;
            }

            if (count > 1)
            {
                
                throw new CircuitException($"The permission that is duplicated is {permission}.", ErrorCodes.DuplicateDbPermissions);
            }

            return false;
        }
    }
}