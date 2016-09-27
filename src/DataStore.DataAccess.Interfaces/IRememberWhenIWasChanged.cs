namespace DataStore.DataAccess.Interfaces
{
    using System;

    public interface IRememberWhenIWasChanged
    {
        DateTime? Created { get; set; }

        DateTime? Modified { get; set; }
    }
}