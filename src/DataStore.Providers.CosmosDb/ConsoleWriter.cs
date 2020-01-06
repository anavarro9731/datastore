using System;
using System.Collections.Generic;
using System.Text;

namespace DataStore.Providers.CosmosDb
{
    using System.Diagnostics;
    using Xunit.Abstractions;

    public class ConsoleWriter : ITestOutputHelper
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(String.Format(format, args));
        }
    }
}
