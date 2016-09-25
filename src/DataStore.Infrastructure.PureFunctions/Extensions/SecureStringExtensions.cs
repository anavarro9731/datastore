// ReSharper disable once CheckNamespace
namespace Infrastructure.PureFunctions.Extensions
{
    using System.Security;

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
