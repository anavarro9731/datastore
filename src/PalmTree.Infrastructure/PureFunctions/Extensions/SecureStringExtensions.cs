using System.Security;

namespace PalmTree.Infrastructure.PureFunctions.Extensions
{
    public static class SecureStringExtensions
    {
        public static SecureString ToSecureString(this string unsecuredString)
        {
            var results = new SecureString();
            foreach (var ch in unsecuredString)
            {
                results.AppendChar(ch);
            }

            return results;
        }
    }
}
