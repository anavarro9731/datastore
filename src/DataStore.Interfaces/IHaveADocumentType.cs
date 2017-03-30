namespace DataStore.Interfaces
{
    //using Newtonsoft.Json;

    public interface IHaveSchema
    {
        //[JsonProperty(PropertyName = "schema")]
        string schema { get; }
    }
}