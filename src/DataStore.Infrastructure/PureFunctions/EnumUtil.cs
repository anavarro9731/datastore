namespace DataStore.Infrastructure.PureFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumUtil
    {
        //* returns an enums values as a collection of that enum's type
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof (T)).Cast<T>();
        }
    }
}