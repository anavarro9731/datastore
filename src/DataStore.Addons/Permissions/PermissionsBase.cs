namespace DataStore.Infrastructure.Impl.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using DataAccess.Interfaces.Addons;

    public abstract class PermissionsBase : IPermissions
    {
        #region IPermissions Members

        public List<IApplicationPermission> ToList
        {
            get
            {
                var list =
                    GetType()
                        .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                        .Select(field => (Permission) field.GetValue(null))
                        .OrderBy(x => x.DisplayOrder)
                        .ToList();

                return list.Cast<IApplicationPermission>().ToList();
            }
        }

        #endregion

        public IApplicationPermission GetById(Guid permissionId)
        {
            return ToList.Single(x => x.Id == permissionId);
        }
    }
}