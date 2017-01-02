namespace DataStore.Addons.Interfaces
{
    using System.Collections.Generic;
    using global::DataStore.Interfaces;

    public interface IUserWithPermissions : IHaveAUniqueId
    {
        List<IUserPermission> Permissions { get; set; }
    }
}