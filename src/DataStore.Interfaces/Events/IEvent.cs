namespace DataStore.Interfaces.Events
{
    using System;

    public interface IEvent
    {
        DateTime OccurredAt { get; set; }
    }
}