namespace DataStore.Models
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;

    public class IdEqualityComparer : IEqualityComparer<IHaveAUniqueId>
    {
        public bool Equals(IHaveAUniqueId x, IHaveAUniqueId y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(IHaveAUniqueId obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}