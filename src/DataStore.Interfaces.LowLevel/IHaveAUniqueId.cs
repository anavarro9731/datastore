namespace DataStore.Interfaces.LowLevel
{
    using System;
    
    public interface IHaveAUniqueId
    {
        Guid Id { get; set; }
    }
}