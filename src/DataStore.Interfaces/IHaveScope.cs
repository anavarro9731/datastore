namespace DataStore.DataAccess.Interfaces
{
    using System;
    using System.Collections.Generic;

    public interface IHaveScope
    {
        List<Guid> ScopeObjectIds { get; }

        void SetScope(params Guid[] scopeObjectIds);
    }
}