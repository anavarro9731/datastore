namespace DataStore.Models.PureFunctions.Extensions
{
    #region

    using System.Security;

    #endregion

    internal static class SecureStringExtensions
    {
        public static SecureString ToSecureString(this string unsecuredString)
        {
            var results = new SecureString();
            foreach (var ch in unsecuredString) results.AppendChar(ch);

            return results;
        }
    }
}
