namespace DataStore.Interfaces
{
    using System.Collections.Generic;

    public interface IHaveScope
    {
        List<IScopeReference> ScopeReferences { get; set; }
    }
}