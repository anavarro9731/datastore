namespace DataStore.Models.PureFunctions
{
    #region

    using System;
    using CircuitBoard;

    #endregion

    internal static class Guard
    {
        public static void Against(Func<bool> test, string errorMessage, Guid? code = null)
        {
            if (test()) throw new CircuitException(errorMessage + " " + code, ErrorCode.Create(code.GetValueOrDefault(), errorMessage));
        }

        public static void Against(bool test, string errorMessage, Guid? code = null)
        {
            if (test) throw new CircuitException(errorMessage + " " + code, ErrorCode.Create(code.GetValueOrDefault(), errorMessage));
        }
    }
}
