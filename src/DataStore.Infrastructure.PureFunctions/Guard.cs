namespace DataStore.Infrastructure.PureFunctions
{
    using System;

    public static class Guard
    {
        //simplest form of validation ever!
        public static void Against(Func<bool> test, string errorMessage)
        {
            if (test()) throw new ApplicationException(errorMessage);
        }


        public static bool Fails(Func<bool> test)
        {
            return test();
        }
    }
}