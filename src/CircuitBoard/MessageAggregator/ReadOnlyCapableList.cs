using System.Collections.Generic;

namespace CircuitBoard.MessageAggregator
{
    /// <summary>
    ///     A .NET 4.0 compatible version of IReadOnlyList
    /// </summary>
    public class ReadOnlyCapableList<T> : List<T>, IReadOnlyList<T>
    {
    }
}