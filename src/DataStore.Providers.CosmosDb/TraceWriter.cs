using System;
using System.Collections.Generic;
using System.Text;

namespace DataStore.Providers.CosmosDb
{
    using System.Diagnostics;
    using Xunit.Abstractions;

    public class TraceWriter : ITestOutputHelper
    {
        public void WriteLine(string message)
        {
            Trace.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            Trace.WriteLine(String.Format(format, args));
        }
    }
}
