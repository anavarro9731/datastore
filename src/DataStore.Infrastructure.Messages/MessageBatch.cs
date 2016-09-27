namespace DataStore.Infrastructure.Messages
{
    using System;

    public class MessageBatch
    {
        public MessageBatch(params Message[] messages)
        {
            this.BatchId = Guid.NewGuid();
            foreach (var message in messages)
            {
                message.TransactionId = this.BatchId;
            }

            this.Messages = messages;
        }

        public Guid BatchId { get; }

        public Message[] Messages { get; }
    }
}