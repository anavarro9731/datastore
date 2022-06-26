namespace DataStore.Interfaces.LowLevel.Permissions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;

    public interface IIdentityWithDatabasePermissions 
    {
        List<DatabasePermission> DatabasePermissions { get; set; }
    }


}
