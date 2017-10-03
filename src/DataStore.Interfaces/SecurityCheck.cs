namespace DataStore.Interfaces
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;

    public delegate void SecurityCheck(IEnumerable<IAggregate> objectsBeingAuthorized);
}