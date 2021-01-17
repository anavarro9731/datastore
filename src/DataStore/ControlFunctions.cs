namespace DataStore
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Options;

    public class ControlFunctions
    {
        private readonly DataStore dataStore;

        private static async Task<List<IHaveScope>> Authorise(
            IIdentityWithDatabasePermissions user,
            DatabasePermission requiredPermission,
            List<IHaveScope> dataBeingQueried,
            DataStoreOptions.SecuritySettings settings,
            DataStore dataStore)
        {
            if (user.HasDatabasePermission(requiredPermission))
            {
                //all data will be returned if the scoped data passes or if there is no scoped data, see below

                var scopedData = dataBeingQueried.Where( /* if an underlying provider should
                    return null for an empty list you would need to add x.ScopeReferences != null || */
                    x => x.ScopeReferences.Any()).ToList();

                if (scopedData.Count == 0) return dataBeingQueried;

                /*
                we could add a .Where clause to the query itself but this would mask the filter and make it harder during
                debugging to work out what the results looked like before the security was applied which I think is a distinct advantage on balance
                and with scopeHierarchies the Where clause could be ginormous. 
                If queries are architect-ed correctly then there won't be any mismatch and therefore no performance hit.
                
                Secure DataStore should enforce that all data has scope, even if by a default scope, but older data may not, 
                so if none of the data being queried has any scope we will authorize the request because scope-less data = unsecured data.
                in other words data with no scope specified is intended to pass all authorization checks.
                However, if even one piece of data is scoped the entire request will succeed or fail based on the success/failure 
                of the scoped piece. This makes sense since the request is an atomic operation and we are 
                not going to return only a sub-set of the data the user requested since that will mean lots of support questions as to 
                why things are not appearing.
                */

                var userPermission = user.DatabasePermissions.Single(x => x.PermissionName == requiredPermission.PermissionName);
                if (scopedData.All(sd => sd.ScopeReferences.Intersect(userPermission.ScopeReferences).Any())) return dataBeingQueried;

                /*
                If there is not a direct intersection between all of the data and the users' permissions there may be an indirect match
                via a scope hierarchy. Extrapolate all indirect scopes and check against this. A bit more costly which is why we don't default
                to this approach.
                 */

                if ((await settings.ScopeHierarchy.Expanded(scopedData, userPermission.ScopeReferences, dataStore).ConfigureAwait(false))
                    .Count() == dataBeingQueried.Count)
                {
                    return dataBeingQueried;
                }
            }

            throw new SecurityException(
                "User not authorized to perform this action. "
                + $"You require the {requiredPermission.PermissionName} permission with the appropriate scope.");
        }

        public ControlFunctions(DataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public async Task<IEnumerable<T>> AuthoriseData<T>(
            IEnumerable<T> data,
            DatabasePermission requiredPermissionWithScopeToData,
            IIdentityWithDatabasePermissions identity) where T : class, IAggregate, new()
        {
            var result = await Authorise(identity, requiredPermissionWithScopeToData, data.Cast<IHaveScope>().ToList())
                             .ConfigureAwait(false);

            return result.Cast<T>();
        }

        public async Task<T> AuthoriseDatum<T>(
            T data,
            DatabasePermission requiredPermissionWithScopeToData,
            IIdentityWithDatabasePermissions identity) where T : class, IAggregate, new()
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

        private Task<List<IHaveScope>> Authorise(
            IIdentityWithDatabasePermissions user,
            DatabasePermission requiredPermission,
            List<IHaveScope> dataBeingQueried) =>
            Authorise(user, requiredPermission, dataBeingQueried, this.dataStore.DataStoreOptions.Security, this.dataStore);
    }
}
