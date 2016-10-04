namespace DataStore.DataAccess.Interfaces
{
    using Newtonsoft.Json;

    public interface IHaveSchema
    {
        [JsonProperty(PropertyName = "schema")]
        string Schema { get; }
    }
}