namespace DataStore.Models.Messages
{
    #region

    using System;
    using DataStore.Interfaces.LowLevel;

    #endregion

    public interface ICanBeUpdatedWhileQueued<T> where T : class, IAggregate, new()
    {
        void UpdateModelWhileQueued(string methodCalled, Action<T> update);
    }
}