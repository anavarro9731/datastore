namespace DataStore.Providers.CosmosDb
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class SerialisedNamesAttribute : Attribute
    {
        public string[] Names { get; }

        public SerialisedNamesAttribute(params string[] names)
        {
            if (names == null || names.Length == 0)
                throw new ArgumentException("At least one name must be provided", nameof(names));
            
            Names = names;
        }
        
        // Backwards compatibility - keep the single Name property for existing code
        public string Name => Names[0];
    }
}