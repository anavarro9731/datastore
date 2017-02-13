using System;

namespace PalmTree.Infrastructure.EventAggregator
{
    public interface IEvent
    {
        DateTime OccurredAt { get; set; }
    }
}