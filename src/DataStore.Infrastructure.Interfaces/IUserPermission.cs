namespace Infrastructure.Interfaces
{
    using System;
    using System.Collections.Generic;

    public interface IUserPermission
    {
        IApplicationPermission Permission { get; set; }

        List<Guid> PermissionScopeFilter { get; set; }
    }
}