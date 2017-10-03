namespace DataStore.Interfaces.LowLevel
{
    using System;

    public interface IRememberWhenIWasCreated
    {
        DateTime? Created { get; set; }

        double? CreatedAsMillisecondsEpochTime { get; set; }
    }
}