namespace DataStore.Addons.Interfaces
{
    using System;

    public interface IApplicationPermission
    {
        int DisplayOrder { get; set; }

        Guid Id { get; set; }

        string PermissionName { get; set; }

        Guid PermissionRequiredToAdministerThisPermission { get; set; }
    }
}