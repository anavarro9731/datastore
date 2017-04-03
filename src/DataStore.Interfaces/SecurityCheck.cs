namespace DataStore.Interfaces
{
    using System.Collections.Generic;
    using LowLevel;

    public delegate void SecurityCheck(IEnumerable<IAggregate> objectsBeingAuthorized);
}