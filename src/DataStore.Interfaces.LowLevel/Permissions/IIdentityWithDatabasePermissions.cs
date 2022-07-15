namespace DataStore.Interfaces.LowLevel.Permissions
{
    #region

    using System.Collections.Generic;

    #endregion

    public interface IIdentityWithDatabasePermissions 
    {
        List<DatabasePermission> DatabasePermissions { get; set; }
    }


}
