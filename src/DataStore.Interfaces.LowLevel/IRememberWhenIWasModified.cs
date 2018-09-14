namespace DataStore.Interfaces.LowLevel
{
    using System;

    public interface IRememberWhenIWasModified
    {
        DateTime Modified { get; set; }

        double ModifiedAsMillisecondsEpochTime { get; set; }
    }
}