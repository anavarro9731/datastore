namespace Infrastructure.Objects.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Interfaces;

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