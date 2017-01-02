namespace DataStore.Addons.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Interfaces;

    public abstract class PermissionsBase : IPermissions
    {
        #region IPermissions Members

        //get the list of permissions in a derived class using this base method
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