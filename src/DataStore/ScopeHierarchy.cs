namespace DataStore
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Models.PureFunctions.Extensions;

    public interface IScopeHierarchyStore
    {
        ConcurrentDictionary<Guid, ScopeHierarchy.EntityWithChildren> Instance { get; }
    }

    public class ScopeHierarchy : IScopeHierarchy
    {
        private readonly ConcurrentDictionary<Guid, EntityWithChildren> scopeEntityLookup;

        private readonly List<IScopeLevel> scopeLevels = new List<IScopeLevel>();

        private ScopeHierarchy(IScopeHierarchyStore scopeHierarchyStore)
        {
            this.scopeEntityLookup = scopeHierarchyStore.Instance;
        }

        private interface IScopeLevel
        {
            string EntityTypeName { get; }

            Task<List<EntityWithChildren>> HydrateScopeLevel(IDataStore dataStore);
        }

        public static ScopeHierarchy Create(IScopeHierarchyStore store)
        {
            return new ScopeHierarchy(store);
        }

        public async Task<IEnumerable<IHaveScope>> GetDataAndPermissionScopeIntersection(
            List<IHaveScope> dataWithScope,
            List<AggregateReference> userPermissionScopes,
            IDataStore dataStore)
        {
            //* if the hierarchy hasn't been hydrated yet
            if (this.scopeEntityLookup.Count == 0) await HydrateHierarchy(dataStore).ConfigureAwait(false);

            //* full list of scopes to check against, start by adding the direct ones
            var extrapolatedScopes = new List<AggregateReference>(userPermissionScopes);

            //* check all users permissions scopes and go through all their children and add their childrens scopes
            foreach (var userPermissionScope in userPermissionScopes)
                if (this.scopeEntityLookup.ContainsKey(userPermissionScope.AggregateId))
                {
                    var currentScopeReferencedEntity = this.scopeEntityLookup[userPermissionScope.AggregateId];
                    RecurseAndFindNewScopeReferences(currentScopeReferencedEntity, ref extrapolatedScopes);
                }

            //* return the intersection
            return dataWithScope.Where(usersData => usersData.ScopeReferences.Intersect(extrapolatedScopes).Any());
        }

        public ScopeHierarchy WithScopeLevel<T>() where T : class, IAggregate, new()
        {
            this.scopeLevels.Add(new ScopeLevel<T>());

            return this;
        }

        private async Task HydrateHierarchy(IDataStore dataStore)
        {
            foreach (var scopeLevel in this.scopeLevels)
            {
                var scopeEntities = await scopeLevel.HydrateScopeLevel(dataStore).ConfigureAwait(false);

                foreach (var entity in scopeEntities)
                {
                    //* add the level item

                    this.scopeEntityLookup.TryAdd(entity.id, entity);

                    //* if you can find it's parent, then add it to the parent's collection of children as well
                    foreach (var parentId in entity.ParentIds)
                        if (this.scopeEntityLookup.ContainsKey(parentId))
                        {
                            this.scopeEntityLookup[parentId].Children.Add(entity);
                        }
                }
            }
        }

        private void RecurseAndFindNewScopeReferences(EntityWithChildren referencedEntity, ref List<AggregateReference> extrapolatedScopesBuffer)
        {
            if (referencedEntity.Children.Any())
            {
                extrapolatedScopesBuffer.AddRange(referencedEntity.Children.Select(c => new AggregateReference(c.id, c.EntityTypeName, c.DebugId)));
                foreach (var referencedEntityChild in referencedEntity.Children)
                    RecurseAndFindNewScopeReferences(referencedEntityChild, ref extrapolatedScopesBuffer);
            }
        }

        public class AggregateImpl : Aggregate
        {
            public List<AggregateReference> SettableScopeReferences { get; set; }
        }

        public class EntityWithChildren
        {
            public readonly List<Guid> ParentIds;

            private readonly AggregateImpl aggregateImpl;

            public EntityWithChildren(AggregateImpl aggregateImpl, IEnumerable<Guid> parentIds, string entityTypeName)
            {
                EntityTypeName = entityTypeName;
                this.aggregateImpl = aggregateImpl;
                this.ParentIds = parentIds.ToList();
            }

            public List<EntityWithChildren> Children { get; } = new List<EntityWithChildren>();

            public string DebugId { get; set; }

            public string EntityTypeName { get; }

            public Guid id { get => this.aggregateImpl.id; set => this.aggregateImpl.id = value; }
        }

        public class InstanceStore : IScopeHierarchyStore
        {
            public ConcurrentDictionary<Guid, EntityWithChildren> Instance { get; } = new ConcurrentDictionary<Guid, EntityWithChildren>();
        }

        public class StaticStore : IScopeHierarchyStore
        {
            private static readonly ConcurrentDictionary<Guid, EntityWithChildren> sharedStore = new ConcurrentDictionary<Guid, EntityWithChildren>();

            public ConcurrentDictionary<Guid, EntityWithChildren> Instance => sharedStore;
        }

        private class ScopeLevel<T> : IScopeLevel where T : class, IAggregate, new()
        {
            public string EntityTypeName => typeof(T).FullName;

            public async Task<List<EntityWithChildren>> HydrateScopeLevel(IDataStore dataStore)
            {
                var stopWatch = new Stopwatch().Op(s => s.Start());

                var aggregates = await dataStore.WithoutEventReplay.Read<T, AggregateImpl>(
                                     x => new AggregateImpl
                                     {
                                         SettableScopeReferences = x.ScopeReferences /* this mapping will take place server side 
                                         with the LINQ just translating this to cosmos SQL API, and since newtonsoft serialises
                                         calculated properties, you can get back the refs this way*/
                                     }, setOptions:o => o.AcceptCrossPartitionQueryCost()).ConfigureAwait(false); //* this is expensive, x-partition, and may fanout

                Debug.WriteLine($"Hydrating scope level {typeof(T).FullName} cost {stopWatch.ElapsedMilliseconds} milliseconds");

                var projection = aggregates.Select(
                    x => new EntityWithChildren(
                        x,
                        x.SettableScopeReferences.Where(y => /*not equal to yourself */ y.AggregateId != x.id).Select(a => a.AggregateId),
                        EntityTypeName)).ToList();

                return projection;
            }
        }
    }
}