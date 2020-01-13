namespace DataStore.Interfaces.LowLevel
{
    using System;

    public interface IHaveAnETag
    {
        string Etag { get; set; }
    }
}