namespace DataStore.Interfaces.LowLevel
{
    public static class DatabasePermissions
    {
        public static DatabasePermission CREATE = new DatabasePermission(nameof(CREATE));

        public static DatabasePermission DELETE = new DatabasePermission(nameof(DELETE));

        public static DatabasePermission READ = new DatabasePermission(nameof(READ));

        public static DatabasePermission UPDATE = new DatabasePermission(nameof(UPDATE));
    }
}
