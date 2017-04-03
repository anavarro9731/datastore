namespace DataStore.Interfaces
{
    using System.Collections.Generic;
    using LowLevel;
    using ServiceApi.Interfaces.LowLevel.Permissions;

    public interface IDataPermission : IApplicationPermission
    {
        IReadOnlyList<IScopeReference> PermissionScope { get; set; }
    }
}