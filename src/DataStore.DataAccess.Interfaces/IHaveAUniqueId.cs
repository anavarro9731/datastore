namespace DataAccess.Interfaces
{
    using System;

    public interface IHaveAUniqueId
    {
        Guid id { get; set; }
    }
}