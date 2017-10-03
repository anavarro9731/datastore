namespace DataStore.Interfaces
{
    using System.Collections.Generic;
    using CircuitBoard.Permissions;
    using DataStore.Interfaces.LowLevel;

    public interface IDataPermission : IApplicationPermission
    {
        IReadOnlyList<IScopeReference> PermissionScope { get; set; }
    }
}