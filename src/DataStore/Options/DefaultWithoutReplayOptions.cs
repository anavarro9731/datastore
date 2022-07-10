namespace DataStore.Options
{
    using System;
    using System.Linq.Expressions;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class DefaultClientSideWithoutReplayOptions<T> : ClientSideWithoutReplayOptions<T> where T : class, IAggregate, new()
    {
        public DefaultClientSideWithoutReplayOptions()
            : base(new WithoutReplayOptionsLibrarySide<T>())
        {
        }

        public override void AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
        }

        public override ClientSideWithoutReplayOptions<T> ContinueFrom(ContinuationToken currentContinuationToken)
        {
            if (currentContinuationToken?.Value == null)
            {
                throw new Exception("The continuation token provided cannot be used since it's value is null");
            }

            LibrarySide.CurrentContinuationToken = currentContinuationToken;

            return this;
        }

        public override ClientSideWithoutReplayOptions<T> OrderBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            LibrarySide.OrderByProperty = Objects.GetPropertyName(propertyRefExpr);
            LibrarySide.OrderDescending = descending;
            return this;
        }

        public override ClientSideWithoutReplayOptions<T> Take(int take, ref ContinuationToken newContinuationToken)
        {
            LibrarySide.MaxTake = take;
            LibrarySide.NextContinuationToken = newContinuationToken ?? throw new Exception("ContinuationToken cannot be null");

            return this;
        }

        public override ClientSideWithoutReplayOptions<T> ThenBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
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
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
        }

        public override void ProvidePartitionKeyValues(PartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
        }

        public override void ProvidePartitionKeyValues(Guid tenantId, PartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
        }
    }
}