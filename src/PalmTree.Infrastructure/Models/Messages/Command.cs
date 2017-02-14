using System;

namespace PalmTree.Infrastructure.Models.Messages
{
    public abstract class Command : Message
    {
        public Guid? ProcessId { get; set; } 
    }
}