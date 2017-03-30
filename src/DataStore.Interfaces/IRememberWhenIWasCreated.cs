using System;

namespace DataStore.Interfaces
{
    public interface IRememberWhenIWasCreated
    {
        DateTime? Created { get; set; }
    }    
}