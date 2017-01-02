namespace DataStore.Addons
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using global::DataStore.Interfaces;
    using Interfaces;

    public class AuthorizationHelper
    {
        // a user has permission scoped against some aggregate, and each aggregate is also scope against another aggregate
        // this checks to see that the user has a particular permission with a particular scope
        // which matches the scope of each aggregate in the list.
        // an aggregates scope is a collection of id of other objects and possibly itself in cases where
        // you want to control access to a single aggrgate on a per user basis as opposed to giving access via
        // another aggregate such as "Department, Customer, Role, etc).
        public static void Authorize(IUserWithPermissions user, IApplicationPermission permission, IEnumerable<IHaveScope> objectsBeingAuthorized)
        {
            var userPermission = user.Permissions.SingleOrDefault(x => x.Permission == permission);

            if (userPermission != null) //if the user has the permission in question
                foreach (var objectQueried in objectsBeingAuthorized)
                {
                    //if the object queried is not scope, return it, it is unsecured
                    if (objectQueried.ScopeReferences == null || objectQueried.ScopeReferences.Count == 0) continue;

                    //if the user's permission is scoped to the same scope as the object being queried allow it.
                    if (userPermission.PermissionScope.Intersect(objectQueried.ScopeReferences).Any()) continue;

                    throw new SecurityException(
                        "User not authorized to perform this action. You require the " + permission.PermissionName +
                        " permission scoped to one of the following which objects you do not have: "
                        + objectQueried.ScopeReferences.Select(s => $"{s.ScopeObjectType} - {s.ScopeObjectId}").Aggregate((a, b) => $"[{a}] [{b}]"));
                }
        }

        public static void Authorize(IUserWithPermissions user, IApplicationPermission permission, IHaveScope objectBeingAuthorized)
        {
            Authorize(user, permission, new IHaveScope[] {objectBeingAuthorized});
        }
    }
}