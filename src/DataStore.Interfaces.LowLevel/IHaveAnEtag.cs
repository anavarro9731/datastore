namespace DataStore.Interfaces.LowLevel
{
    using System;

    public interface IHaveAnETag
    {
        string Etag { get; set; }
    }

    public interface IEtagUpdated
    {
        Action<string> EtagUpdated { get; set; }
    }
}