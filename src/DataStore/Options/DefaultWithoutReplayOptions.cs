namespace DataStore.Options
{
    using System;
    using System.Linq.Expressions;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class DefaultWithoutReplayOptions<T> : WithoutReplayOptionsClientSide<T> where T : class, IAggregate, new()
    {
        public DefaultWithoutReplayOptions()
            : base(new WithoutReplayOptionsLibrarySide<T>())
        {
        }

        public override void AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
        }

        public override WithoutReplayOptionsClientSide<T> ContinueFrom(ContinuationToken currentContinuationToken)
        {
            if (currentContinuationToken?.Value == null)
            {
                throw new Exception("The continuation token provided cannot be used since it's value is null");
            }

            LibrarySide.CurrentContinuationToken = currentContinuationToken;

            return this;
        }

        public override WithoutReplayOptionsClientSide<T> OrderBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            LibrarySide.OrderByProperty = Objects.GetPropertyName(propertyRefExpr);
            LibrarySide.OrderDescending = descending;
            return this;
        }

        public override WithoutReplayOptionsClientSide<T> Take(int take, ref ContinuationToken newContinuationToken)
        {
            LibrarySide.MaxTake = take;
            LibrarySide.NextContinuationToken = newContinuationToken ?? throw new Exception("ContinuationToken cannot be null");

            return this;
        }

        public override WithoutReplayOptionsClientSide<T> ThenBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            var propertyName = Objects.GetPropertyName(propertyRefExpr);
            LibrarySide.ThenByQueue.Enqueue((propertyName, descending));
            return this;
        }
        
                
        public override void BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
        }

        public override void ProvidePartitionKeyValues(Guid tenantId)
        {
            LibrarySide.PartitionKeyTenantId = tenantId;
        }

        public override void ProvidePartitionKeyValues(PartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTimeInterval = timeInterval;
        }

        public override void ProvidePartitionKeyValues(Guid tenantId, PartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTenantId = tenantId;
            LibrarySide.PartitionKeyTimeInterval = timeInterval;
        }
    }
}