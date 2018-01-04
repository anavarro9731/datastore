namespace DataStore.Impl.RavenDb
{
    using Newtonsoft.Json;

    public class RavenSettings
    {
        [JsonConstructor]
        public RavenSettings(string url, string database, string userId, string password)
        {
            Url = url;
            Database = database;
            UserId = userId;
            Password = password;
        }

        public string Url { get; }

        public string Database { get; set; }

        public string UserId { get; }

        public string Password { get; set; }
    }
}
