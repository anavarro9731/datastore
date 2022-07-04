namespace DataStore.Models.PureFunctions
{
    using System;
    using CircuitBoard;

    internal static class Guard
    {
        public static void Against(Func<bool> test, string errorMessage, Guid? code = null)
        {
            if (test()) throw new CircuitException(errorMessage + " " + code);
        }

        public static void Against(bool test, string errorMessage, Guid? code = null)
        {
            if (test) throw new CircuitException(errorMessage + " " + code);
        }
    }
}
