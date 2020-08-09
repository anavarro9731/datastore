namespace DataStore.Models.Messages
{
    using System;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;

    public interface ICanBeUpdatedWhileQueued<T> where T : class, IAggregate, new()
    {
        void UpdateModelWhileQueued(string methodCalled, Action<T> update);
    }
}