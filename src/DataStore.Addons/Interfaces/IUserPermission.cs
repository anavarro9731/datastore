namespace DataStore.Addons.Interfaces
{
    using System.Collections.Generic;
    using global::DataStore.Interfaces;

    public interface IUserPermission
    {
        IApplicationPermission Permission { get; set; }

        List<IScopeReference> PermissionScope { get; set; }
    }
}