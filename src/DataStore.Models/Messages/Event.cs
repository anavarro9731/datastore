namespace DataStore.Models.Messages
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Interfaces.Events;

    public class Event<TModel> : Event
    {
        public Event(TModel model)
        {
            this.Model = model;

        }

        public TModel Model { get; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", 
        Justification = "Reviewed. Suppression is OK here.")]
    public class Event : Message, IEvent
    {
        public Event()
        {
            this.OccurredAt = this.Created;
        }

        public DateTime OccurredAt { get; set; }
    }
}