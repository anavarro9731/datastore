namespace DataAccess.Models
{
    using System.Collections.Generic;

    using DataAccess.Interfaces;

    public class IdEqualityComparer : IEqualityComparer<IHaveAUniqueId>
    {
        public bool Equals(IHaveAUniqueId x, IHaveAUniqueId y)
        {
            return x.id == y.id;
        }

        public int GetHashCode(IHaveAUniqueId obj)
        {
            return obj.id.GetHashCode();
        }
    }
}