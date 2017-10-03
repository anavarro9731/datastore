using System;
using System.Threading.Tasks;

namespace CircuitBoard.Messages
{
    public interface IQueuedStateChange : IMessage
    {
        Func<Task> CommitClosure { get; set; }

        bool Committed { get; set; }
    }
}