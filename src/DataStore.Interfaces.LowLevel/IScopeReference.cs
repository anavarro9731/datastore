namespace DataStore.Interfaces.LowLevel
{
    using System;

    public interface IScopeReference
    {
        Guid ScopeObjectId { get; }
        string ScopeObjectType { get; }
        DateTime ScopeReferenceCreatedOn { get; }
    }
}