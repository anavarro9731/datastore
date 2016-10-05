﻿namespace DataStore.DataAccess.Interfaces.Addons
{
    using System.Collections.Generic;

    /// <summary>
    /// used to remove dependencies on high-level permission classes
    /// </summary>
    public interface IPermissions
    {
        List<IApplicationPermission> ToList { get; }
    }
}