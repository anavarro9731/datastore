namespace DataStore.Infrastructure.Configuration.Settings
{
    public class SerilogSettings
    {
        public SerilogSettings(string appName, Seq seq)
        {
            this.AppName = appName;
            this.Seq = seq;
        }

        public string AppName { get; }

        public Seq Seq { get; }
    }

    public class Seq
    {
        public Seq(string url, string apikey, string loglevel)
        {
            this.Url = url;
            this.Apikey = apikey;
            this.Loglevel = loglevel;
        }

        public string Apikey { get; }

        public string Loglevel { get; }

        public string Url { get; }
    }
}