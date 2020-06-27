namespace DataStore.Interfaces.LowLevel
{
    using System;

    public static class DatabasePermissions
    {
        public static DatabasePermission CREATE = new DatabasePermission(
            Guid.Parse("3cfc323f-f123-4d5d-94e9-6705cd3fcac5"),
            nameof(CREATE));

        public static DatabasePermission DELETE = new DatabasePermission(
            Guid.Parse("655372ab-9775-40c3-9f80-ffb9c6df5fcc"),
            nameof(DELETE));

        public static DatabasePermission READ = new DatabasePermission(Guid.Parse("880b180c-fee8-4084-8c18-e66cfb39c72a"), nameof(READ));

        public static DatabasePermission UPDATE = new DatabasePermission(
            Guid.Parse("71b249d4-99a6-4b4c-b561-3f27ac6b43bb"),
            nameof(UPDATE));
    }
}