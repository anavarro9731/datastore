using System;

namespace DataStore.DataAccess.Models.Messages
{
    public abstract class Command : Message
    {
        public Guid SagaId { get; set; } 
    }
}