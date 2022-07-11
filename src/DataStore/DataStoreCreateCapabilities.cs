using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DataStore.Tests")]

namespace DataStore
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.Operations;
    using global::DataStore.Interfaces.Options;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PartitionKeys;
    using global::DataStore.Models.PureFunctions;
    using global::DataStore.Models.PureFunctions.Extensions;

    //methods return the enriched object as it was added to the database

    internal class DataStoreCreateCapabilities
    {
        private readonly IncrementVersions incrementVersions;

        private readonly IMessageAggregator messageAggregator;

        private readonly IDataStoreOptions dataStoreOptions;

        public DataStoreCreateCapabilities(
            IDocumentRepository dataStoreConnection,
            IMessageAggregator messageAggregator,
            IDataStoreOptions dataStoreOptions,
            IncrementVersions incrementVersions)
        {
            this.messageAggregator = messageAggregator;
            this.dataStoreOptions = dataStoreOptions;
            this.incrementVersions = incrementVersions;
            DsConnection = dataStoreConnection;
        }

        private IDocumentRepository DsConnection { get; }

        public async Task<T> Create<T, O>(T model, O options, string methodName = null)
            where T : class, IAggregate, new() where O : CreateOptionsLibrarySide, new()
        {
            {
                //create a new one, we definitely don't want to use the instance passed in, in the event it changes after this call
                //and affects the commit and/or the resulting events
                var newObject = model.Clone();

                newObject.ForcefullySetMandatoryPropertyValues(options.SetReadonlyFlag, this.DsConnection.UseHierarchicalPartitionKeys);

                Guard.Against(
                    this.messageAggregator.AllMessages.OfType<IQueuedDataStoreWriteOperation<T>>()
                        .SingleOrDefault(e => !e.Committed && e.AggregateId == newObject.id) != null,
                    $"An item with the same ID of {newObject.id} is already queued to be created",
                    Guid.Parse("63328bcd-d58d-446a-bc85-fedfde43d2e2"));

                var existsAlreadyOptions = BuildReadOptionsLibrarySideFromModel(newObject, this.DsConnection.UseHierarchicalPartitionKeys);
                var existsAlready = (await DsConnection.GetItemAsync<T>(new AggregateQueriedByIdOperationOperation(methodName, model.id, existsAlreadyOptions))
                                                       .ConfigureAwait(false)) != null;
                Guard.Against(
                    existsAlready,
                    $"An item with the same ID of {newObject.id} already exists in this partition",
                    Guid.Parse("cfe3ebc2-4677-432b-9ded-0ef498b9f59d"));

                this.messageAggregator.Collect(new QueuedCreateOperation<T>(methodName, newObject, DsConnection, this.messageAggregator));

                var itemToReturnToCaller = newObject.Clone(); //* return clones otherwise its to easy to change the referenced object before committing    
                itemToReturnToCaller.Etag = "waiting to be committed";
                newObject.As<IEtagUpdated>().EtagUpdated += newTag => itemToReturnToCaller.Etag = newTag;

                await this.incrementVersions.IncrementAggregateVersionOfItemToBeQueued(newObject, methodName).ConfigureAwait(false);

                //for the same reason as the above we want a new object, but we want to return the enriched one, so we clone it,
                //essentially no external client should be able to get a reference to the instance we use internally
                return itemToReturnToCaller;
            }

            ReadOptionsLibrarySide BuildReadOptionsLibrarySideFromModel(IAggregate aggregate, bool dsConnectionUseHierarchicalPartitionKeys)
            {
                var existsAlreadyOptions = new ReadOptionsLibrarySide();
                
                CheckForTenantId(ref existsAlreadyOptions);
                CheckForTimePeriod(ref existsAlreadyOptions);

                void CheckForTenantId(ref ReadOptionsLibrarySide optionsToSet1)
                {
                    var tenantPrefix = PartitionKeyHelpers.PartitionKeyPrefixes.TenantId;
                    if (dsConnectionUseHierarchicalPartitionKeys)
                    {
                        var tenantId = aggregate.PartitionKeys.AsList().SingleOrDefault(p => p != null && p.StartsWith(tenantPrefix))?.SubstringAfter(tenantPrefix);
                        optionsToSet1.PartitionKeyTenantId = tenantId;
                    }
                    else
                    {
                        var tenantId = Regex.Match(aggregate.PartitionKey, $"(?<={tenantPrefix})[^_]*").Value;
                        optionsToSet1.PartitionKeyTenantId = tenantId;
                    }
                }

                 void CheckForTimePeriod(ref ReadOptionsLibrarySide optionsToSet1)
                {
                    var timePeriodPrefix = PartitionKeyHelpers.PartitionKeyPrefixes.TimePeriod;
                    if (dsConnectionUseHierarchicalPartitionKeys)
                    {
                        var timePeriod = aggregate.PartitionKeys.AsList().SingleOrDefault(p => p != null && p.StartsWith(timePeriodPrefix))?.SubstringAfter(timePeriodPrefix);
                        optionsToSet1.PartitionKeyTimeInterval = timePeriod;
                    }
                    else
                    {
                        var timePeriod = Regex.Match(aggregate.PartitionKey, $"(?<={timePeriodPrefix})[^_]*").Value;
                        optionsToSet1.PartitionKeyTimeInterval = timePeriod;
                    }
                }

                return existsAlreadyOptions;
            }
        }
    }
}
