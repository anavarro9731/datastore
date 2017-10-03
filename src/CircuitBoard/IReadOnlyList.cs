using System.Collections;
using System.Collections.Generic;

namespace CircuitBoard
{
    /// <summary>
    ///     A .NET 4.0 compatible version of IReadOnlyList
    ///     Legacy code might need it
    ///     see ReadOnlyCapableList
    /// </summary>
    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>,
        IEnumerable<T>, IEnumerable
    {
    }
}