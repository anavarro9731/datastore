namespace DataStore.Messages
{
    using System;

    public abstract class Message
    {
        protected Message()
        {
            this.MessageId = Guid.NewGuid();
            this.Created = DateTime.UtcNow;
        }

        public DateTime Created { get; }

        public Guid MessageId { get; }

        public Guid TransactionId { get; set; }

    }
}