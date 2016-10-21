using System;

namespace DataStore.DataAccess.Interfaces.Events
{
    public interface IEvent
    {
        DateTime OccurredAt { get; set; }
    }
}