namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using CircuitBoard.Permissions;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;

    public class ScopeHierarchy
    {
        private readonly Dictionary<Guid, EntityWithChildren> scopeEntityLookup = new Dictionary<Guid, EntityWithChildren>();

        private readonly List<IScopeLevel> scopeLevels = new List<IScopeLevel>();

        public static ScopeHierarchy Create()
        {
            return new ScopeHierarchy();
        }

        private ScopeHierarchy()
        {
        }

        private interface IScopeLevel
        {
            Task<List<EntityWithChildren>> HydrateScopeLevel(IDataStore dataStore);

            string EntityTypeName { get; }
        }

        
        public async Task<IEnumerable<IHaveScope>> ExtrapolatedIntersection(List<IHaveScope> dataWithScope, List<ScopeReference> userPermissionScopes, IDataStore dataStore)
        {
            {
                if (this.scopeEntityLookup.Count == 0) await HydrateHierarchy(dataStore).ConfigureAwait(false); ;

                var extrapolatedScopes = new List<ScopeReference>(userPermissionScopes);

                foreach (var userPermissionScope in userPermissionScopes)
                    if (this.scopeEntityLookup.ContainsKey(userPermissionScope.ScopeObjectObjectId))
                    {
                        var currentScopeReferencedEntity = this.scopeEntityLookup[userPermissionScope.ScopeObjectObjectId];
                        RecurseAndFindNewScopeReferences(currentScopeReferencedEntity, ref extrapolatedScopes);
                    }

                return dataWithScope.Where(sd => sd.ScopeReferences.Intersect(extrapolatedScopes).Any());
            }

            void RecurseAndFindNewScopeReferences(EntityWithChildren referencedEntity, ref List<ScopeReference> extrapolatedScopesBuffer)
            {
                if (referencedEntity.Children.Any())
                {
                    extrapolatedScopesBuffer.AddRange(referencedEntity.Children.Select(c => new ScopeReference(c.id, c.EntityTypeName, referenceId: c.ScopeReferenceId)));
                    foreach (var referencedEntityChild in referencedEntity.Children)
                        RecurseAndFindNewScopeReferences(referencedEntityChild, ref extrapolatedScopesBuffer);
                }
            }
        }

        public ScopeHierarchy WithScopeLevel<T>(Func<T, Guid> parentIdSelector) where T : class, IAggregate, new()
        {
            this.scopeLevels.Add(
                new ScopeLevel<T>
                {
                    ParentIdSelector = parentIdSelector
                });

            return this;
        }

        private async Task HydrateHierarchy(IDataStore dataStore)
        {
            foreach (var scopeLevel in this.scopeLevels)
            {
                var scopeEntities = await scopeLevel.HydrateScopeLevel(dataStore).ConfigureAwait(false); ;

                if (TheUserIsAddingAScopeLevelWhoseEntitiesCantBeTracedToAParent())
                {
                    throw new Exception($"Some of the entities in scope-level {scopeLevel.EntityTypeName} do not have links to a parent. This is only allowed for the first level.");
                }

                foreach (var entity in scopeEntities)
                {
                    this.scopeEntityLookup.Add(entity.id, entity);

                    if (this.scopeEntityLookup.ContainsKey(entity.ParentId))
                    {
                        this.scopeEntityLookup[entity.ParentId].Children.Add(entity);
                    }
                }

                bool AllEntitiesHaveParents()
                {
                    return scopeEntities.Any(e => e.ParentId != Guid.Empty);
                }

                bool TheUserIsAddingAScopeLevelWhoseEntitiesCantBeTracedToAParent()
                {
                    return this.scopeEntityLookup.Count > 0 && AllEntitiesHaveParents() == false;
                }
            }

        }

        private class EntityWithChildren
        {
            public readonly Guid ParentId;

            private readonly IAggregate aggregateImplementation;

            public EntityWithChildren(IAggregate aggregateImplementation, Guid parentId)
            {
                this.aggregateImplementation = aggregateImplementation;
                this.ParentId = parentId;
            }

            public string ScopeReferenceId
            {
                get
                {
                    try
                    {
                        return (this.aggregateImplementation as dynamic).Name;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            public List<EntityWithChildren> Children { get; } = new List<EntityWithChildren>();

            public string EntityTypeName => this.aggregateImplementation.GetType().FullName;

            public Guid id { get => this.aggregateImplementation.id; set => this.aggregateImplementation.id = value; }
        }

        private class ScopeLevel<T> : IScopeLevel where T : class, IAggregate, new()
        {
            public Func<T, Guid> ParentIdSelector;

            public async Task<List<EntityWithChildren>> HydrateScopeLevel(IDataStore dataStore)
            {
                var aggregates = await dataStore.Read<T>().ConfigureAwait(false);
                var projection = aggregates.Select(x =>
                    {
                        return new EntityWithChildren(x, this.ParentIdSelector(x));
                    }).ToList();
                return projection;
            }

            public string EntityTypeName => typeof(T).FullName;
        }
    }
}