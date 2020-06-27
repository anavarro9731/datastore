namespace DataStore.Models
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;

    public class IdEqualityComparer : IEqualityComparer<IHaveAUniqueId>
    {
        public bool Equals(IHaveAUniqueId x, IHaveAUniqueId y) => x.id == y.id;

        public int GetHashCode(IHaveAUniqueId obj) => obj.id.GetHashCode();
    }
}