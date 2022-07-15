namespace DataStore.Interfaces.LowLevel
{
    #region

    using System;

    #endregion

    public interface IHaveAnETag
    {
        string Etag { get; set; }
    }

    public interface IEtagUpdated
    {
        Action<string> EtagUpdated { get; set; }
    }
}