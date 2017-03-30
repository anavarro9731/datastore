using System;

namespace DataStore.Interfaces
{
    public interface IRememberWhenIWasModified
    {
        DateTime? Modified { get; set; }
    }    
}