namespace DataStore.Interfaces.LowLevel
{
    #region

    using System;

    #endregion

    public interface IRememberWhenIWasCreated
    {
        DateTime Created { get; set; }

        double CreatedAsMillisecondsEpochTime { get; set; }
    }
}