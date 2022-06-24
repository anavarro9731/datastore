namespace DataStore.Interfaces.LowLevel
{
    public static class SecurableOperations
    {
        public static SecurableOperation CREATE = new SecurableOperation("CREATE");

        public static SecurableOperation DELETE = new SecurableOperation("DELETE");

        public static SecurableOperation READ = new SecurableOperation("READ");
        
        public static SecurableOperation READPII = new SecurableOperation("READ-PII");

        public static SecurableOperation UPDATE = new SecurableOperation("UPDATE");
        
        
    }
}
