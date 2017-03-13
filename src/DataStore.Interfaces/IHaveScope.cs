namespace DataStore.Interfaces
{
    using System.Collections.Generic;
    using ServiceApi.Interfaces.LowLevel;

    public interface IHaveScope
    {
        List<IScopeReference> ScopeReferences { get; set; }
    }
}