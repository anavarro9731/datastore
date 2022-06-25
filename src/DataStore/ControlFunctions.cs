namespace DataStore
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.LowLevel.Permissions;

    public class ControlFunctions
    {
        private readonly DataStore dataStore;

        public ControlFunctions(DataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public async Task<IEnumerable<T>> AuthoriseData<T>(
            IEnumerable<T> data,
            SecurableOperation requiredPermission,
            IIdentityWithDatabasePermissions identity) where T : class, IAggregate, new()
        {
            if (identity == null)
            {
                //* this should be checked by caller first
                throw new SecurityException(
                    "Data authorisation enabled but no identity has been provided. Please set the .AuthoriseFor(identity) option when calling your DataStore operation");
            }
            var result = await Authorise(identity, requiredPermission, data.Cast<IHaveScope>().ToList()).ConfigureAwait(false);

            return result.Cast<T>();
        }

        public async Task<T> AuthoriseDatum<T>(T data, SecurableOperation requiredPermissionWithScopeToData, IIdentityWithDatabasePermissions identity)
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
            SecurableOperation requiredPermission,
            List<IHaveScope> dataBeingQueried)
        {
            {
                if (user.HasDatabasePermission(new SecurableOperation("*"))) return dataBeingQueried; 
                /* wildcard means all permissions and ALSO all possible scopes, its an escape hatch for service accounts
                 or test accounts, if you want all permissions with limited scopes you need to add all the permissions separately */ 
                
                if (user.HasDatabasePermission(requiredPermission)) 
                    /* do they have the read, write, delete, etc capability regardless of scope
                     when using full rbac in soap, by default they will have all permissions, unless a specific restriction has been made */
                {
                    DatabasePermission
                        permissionInstance = user.DatabasePermissions.Single(x => x.PermissionName == requiredPermission.PermissionName);
                    
                    //* check that the scope of the data intersects the scope of the user for this operation
                    var authorised = await CheckScope(permissionInstance, this.dataStore.DataStoreOptions.Security, this.dataStore).ConfigureAwait(false);
                    if (authorised) return dataBeingQueried;
                }

                /* One consideration was whether to filter the data rather than throw an exception if there is a mismatch between the data
                    requested and the data authorised. However this would hide problems and make debugging hard and violate the principle of
                    least astonishment. 
                    If your queries are architect-ed correctly then there won't be any mismatch and therefore no performance hit and no security failure.                
                    */
                
                throw new SecurityException(
                    "User not authorized to perform this action. " + $"You require the {requiredPermission.PermissionName} permission with the appropriate scope.");
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
}