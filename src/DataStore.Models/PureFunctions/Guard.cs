namespace DataStore.Models.PureFunctions
{
    using System;

    public static class Guard
    {
        public static void Against(Func<bool> test, string errorMessage, Guid? code = null)
        {
            if (test()) throw new Exception(errorMessage + " " + code);
        }

        public static void Against(bool test, string errorMessage, Guid? code = null)
        {
            if (test) throw new Exception(errorMessage + " " + code);
        }
    }
}