namespace DataStore.Interfaces
{
    using System.Collections.Generic;

    public delegate void SecurityCheck(IEnumerable<IAggregate> objectsBeingAuthorized);
}