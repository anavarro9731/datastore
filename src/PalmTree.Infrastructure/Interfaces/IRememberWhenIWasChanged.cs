using System;

namespace PalmTree.Infrastructure.Interfaces
{
    public interface IRememberWhenIWasChanged
    {
        DateTime? Created { get; set; }

        DateTime? Modified { get; set; }
    }
}