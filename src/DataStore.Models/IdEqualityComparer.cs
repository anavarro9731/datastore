namespace DataStore.Models
{
    #region

    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;

    #endregion

    public class IdEqualityComparer : IEqualityComparer<IHaveAUniqueId>
    {
        public bool Equals(IHaveAUniqueId x, IHaveAUniqueId y) => x.id == y.id;

        public int GetHashCode(IHaveAUniqueId obj) => obj.id.GetHashCode();
    }
}