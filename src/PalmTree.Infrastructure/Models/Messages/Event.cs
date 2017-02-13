﻿using System;
using System.Diagnostics.CodeAnalysis;
using PalmTree.Infrastructure.EventAggregator;

namespace PalmTree.Infrastructure.Models.Messages
{
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