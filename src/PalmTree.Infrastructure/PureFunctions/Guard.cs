using System;

namespace PalmTree.Infrastructure.PureFunctions
{
    public static class Guard
    {
        //simplest form of validation ever!
        public static void Against(Func<bool> test, string errorMessage)
        {
            if (test()) throw new ApplicationException(errorMessage);
        }
    }
}