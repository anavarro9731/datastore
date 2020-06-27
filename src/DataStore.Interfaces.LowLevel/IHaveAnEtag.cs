namespace DataStore.Interfaces.LowLevel
{
    public interface IHaveAnETag
    {
        string Etag { get; set; }
    }
}