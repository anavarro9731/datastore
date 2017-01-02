namespace DataStore.Models.Messages
{
    using System;

    public abstract class Command : Message
    {
        public Guid SagaId { get; set; } 
    }
}