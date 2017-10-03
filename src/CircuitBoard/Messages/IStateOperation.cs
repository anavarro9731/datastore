using System;

namespace CircuitBoard.Messages
{
    public interface IStateOperation
    {
        double StateOperationCost { get; set; }

        long StateOperationStartTimestamp { get; set; }

        long? StateOperationStopTimestamp { get; set; }

        TimeSpan? StateOperationDuration { get; set; }
    }
}