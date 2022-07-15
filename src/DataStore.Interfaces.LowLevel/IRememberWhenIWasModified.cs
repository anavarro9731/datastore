namespace DataStore.Interfaces.LowLevel
{
    #region

    using System;

    #endregion

    public interface IRememberWhenIWasModified
    {
        DateTime Modified { get; set; }

        double ModifiedAsMillisecondsEpochTime { get; set; }
    }
}