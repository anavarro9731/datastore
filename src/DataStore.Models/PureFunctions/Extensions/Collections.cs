namespace DataStore.Models.PureFunctions.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Collections
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }
    }
}
