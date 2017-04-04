namespace DataStore.Models.PureFunctions
{
    using System;

    public static class Guard
    {
        public static void Against(Func<bool> test, string errorMessage)
        {
            if (test()) throw new Exception(errorMessage);
        }

        public static void Against(bool test, string errorMessage)
        {
            if (test) throw new Exception(errorMessage);
        }
    }
}