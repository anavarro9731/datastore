namespace DataStore.Interfaces.LowLevel.Permissions
{
    #region

    using System.Collections.Generic;

    #endregion

    public interface IHaveScope
    {
        List<AggregateReference> ScopeReferences { get; }
    }
}