using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.Tests.Tests.RavenOnly
{
    using Newtonsoft.Json;
    using System.IO;
    using Xunit;

    public class Cleanup
    {
        [Fact(DisplayName = "RavenDB Cleanup Task")]
        public void CleanUpTestDatabases()
        {
            var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RavenSettings.json");

            var ravenSettings = JsonConvert.DeserializeObject<Impl.RavenDb.RavenSettings>(File.ReadAllText(location));
            ravenSettings.Database = "";

            Impl.RavenDb.RavenRepository repository = new Impl.RavenDb.RavenRepository(ravenSettings);
            repository.DropAllDatabases();
        }
    }
}
