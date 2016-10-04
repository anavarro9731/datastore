namespace DataStore.Infrastructure.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;

    public class AuthorizationHelper
    {
        public static void Authorize(IUserWithPermissions user, IApplicationPermission permission, Guid scopePermissionShouldHave)
        {
            var userPermission = user.Permissions.SingleOrDefault(x => x.Permission == permission);
            if (userPermission != null)
            {
                if (userPermission.PermissionScopeFilter.Contains(scopePermissionShouldHave))
                {
                    return;
                }
            }

            throw new SecurityException(
                "User not authorized to perform this action. You require the " + permission.PermissionName + " permission.");
        }

        public static void Authorize(IUserWithPermissions user, IApplicationPermission permission, IEnumerable<IHaveScope> dataBeingQueried)
        {
            var userPermission = user.Permissions.SingleOrDefault(x => x.Permission == permission);
            if (userPermission != null)
            {
                var scopedOnly = dataBeingQueried.Where(x => x?.ScopeObjectIds != null && x.ScopeObjectIds.Any()).ToList();
                if (scopedOnly.Count == 0
                    || userPermission.PermissionScopeFilter.Intersect(scopedOnly.SelectMany(x => x.ScopeObjectIds)).Any())
                {
                    return;
                }
            }

            throw new SecurityException(
                "User not authorized to perform this action. You require the " + permission.PermissionName + " permission.");
        }
    }
}