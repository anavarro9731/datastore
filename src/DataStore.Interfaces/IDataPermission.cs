namespace DataStore.Interfaces
{
    using System.Collections.Generic;
    using ServiceApi.Interfaces.LowLevel.Permissions;

    public interface IDataPermission : IApplicationPermission
    {
        IReadOnlyList<IScopeReference> PermissionScope { get; set; }
    }
}