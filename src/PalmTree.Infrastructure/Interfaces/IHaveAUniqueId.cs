using System;

namespace PalmTree.Infrastructure.Interfaces
{
    public interface IHaveAUniqueId
    {
        Guid id { get; set; }
    }
}