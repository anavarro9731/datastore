namespace DataStore.Interfaces.LowLevel.Permissions
{
    using System.Collections.Generic;

    public interface IHaveScope
    {
        List<AggregateReference> ScopeReferences { get; }
    }
}