namespace Infrastructure.Interfaces
{
    using System;
    using System.Collections.Generic;

    public interface IUserWithPermissions
    {
        List<IUserPermission> Permissions { get; set; }

        int ScopeObjectIdCount { get; }

        List<Guid> ScopeObjectIds { get; }

        Guid id { get; set; }
    }
}