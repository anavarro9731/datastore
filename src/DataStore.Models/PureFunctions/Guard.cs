namespace DataStore.Models.PureFunctions
{
    using System;

    public static class Guard
    {
        public static void Against(Func<bool> test, string errorMessage, string internalMessage = null)
        {
            if (test()) throw new ApplicationException(errorMessage, new Exception(internalMessage));
        }

        public static void AgainstInternal(Func<bool> test, string errorMessage)
        {
            if (test()) throw new Exception(errorMessage);
        }
    }
}