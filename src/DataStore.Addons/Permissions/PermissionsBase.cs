namespace DataStore.Infrastructure.Objects.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using DataAccess.Interfaces.Addons;

    public abstract class PermissionsBase : IPermissions
    {
        public List<IApplicationPermission> ToList
        {
            get
            {
                var list =
                    this.GetType()
                        .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                        .Select(field => (Permission)field.GetValue(null))
                        .OrderBy(x => x.DisplayOrder)
                        .ToList();

                return list.Cast<IApplicationPermission>().ToList();
            }
        }

        public IApplicationPermission GetById(Guid permissionId)
        {
            return this.ToList.Single(x => x.Id == permissionId);
        }
    }
}