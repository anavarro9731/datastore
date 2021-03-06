﻿namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class ScopeHierarchy
    {
        private readonly Dictionary<Guid, EntityWithChildren> scopeEntityLookup = new Dictionary<Guid, EntityWithChildren>();

        private readonly List<IScopeLevel> scopeLevels = new List<IScopeLevel>();

        public static ScopeHierarchy Create() => new ScopeHierarchy();

        private ScopeHierarchy()
        {
        }

        private interface IScopeLevel
        {
            string EntityTypeName { get; }

            Task<List<EntityWithChildren>> HydrateScopeLevel(DataStore dataStore);
        }

        public async Task<bool> ExpandedPermissionContains(
            DatabasePermissionInstance permissionInstance,
            DataStore dataStore,
            DatabaseScopeReference scopeToMatch)
        {
            if (this.scopeEntityLookup.Count == 0) await HydrateHierarchy(dataStore).ConfigureAwait(false);

            var extrapolatedScopes = new List<DatabaseScopeReference>();
            foreach (var userPermissionScope in permissionInstance.ScopeReferences)
                if (this.scopeEntityLookup.ContainsKey(userPermissionScope.ScopeObjectId))
                {
                    var currentScopeReferencedEntity = this.scopeEntityLookup[userPermissionScope.ScopeObjectId];
                    RecurseAndFindNewScopeReferences(currentScopeReferencedEntity, ref extrapolatedScopes);
                }

            var result =
                permissionInstance.ScopeReferences.Contains(scopeToMatch) /*better check the required scope is not already present*/
                || extrapolatedScopes.Contains(scopeToMatch);

            return result;
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

        internal async Task<IEnumerable<IHaveScope>> Expanded(
            List<IHaveScope> dataWithScope,
            List<DatabaseScopeReference> userPermissionScopes,
            DataStore dataStore)
        {
            if (this.scopeEntityLookup.Count == 0) await HydrateHierarchy(dataStore).ConfigureAwait(false);

            var extrapolatedScopes = new List<DatabaseScopeReference>(userPermissionScopes);

            foreach (var userPermissionScope in userPermissionScopes)
                if (this.scopeEntityLookup.ContainsKey(userPermissionScope.ScopeObjectId))
                {
                    var currentScopeReferencedEntity = this.scopeEntityLookup[userPermissionScope.ScopeObjectId];
                    RecurseAndFindNewScopeReferences(currentScopeReferencedEntity, ref extrapolatedScopes);
                }

            return dataWithScope.Where(sd => sd.ScopeReferences.Intersect(extrapolatedScopes).Any());
        }

        private async Task HydrateHierarchy(DataStore dataStore)
        {
            foreach (var scopeLevel in this.scopeLevels)
            {
                var scopeEntities = await scopeLevel.HydrateScopeLevel(dataStore).ConfigureAwait(false);

                if (scopeEntities.Any() && TheUserIsAddingAScopeLevelWhoseEntitiesCantBeTracedToAParent())
                {
                    throw new Exception(
                        $"Some of the entities in scope-level {scopeLevel.EntityTypeName} do not have links to a parent. This is only allowed for the first level.");
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

                bool TheUserIsAddingAScopeLevelWhoseEntitiesCantBeTracedToAParent() =>
                    this.scopeEntityLookup.Count > 0 && AllEntitiesHaveParents() == false;
            }
        }

        private void RecurseAndFindNewScopeReferences(
            EntityWithChildren referencedEntity,
            ref List<DatabaseScopeReference> extrapolatedScopesBuffer)
        {
            if (referencedEntity.Children.Any())
            {
                extrapolatedScopesBuffer.AddRange(
                    referencedEntity.Children.Select(c => new DatabaseScopeReference(c.id, c.EntityTypeName, c.ScopeReferenceId)));
                foreach (var referencedEntityChild in referencedEntity.Children)
                    RecurseAndFindNewScopeReferences(referencedEntityChild, ref extrapolatedScopesBuffer);
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

            public List<EntityWithChildren> Children { get; } = new List<EntityWithChildren>();

            public string EntityTypeName => this.aggregateImplementation.GetType().FullName;

            public Guid id { get => this.aggregateImplementation.id; set => this.aggregateImplementation.id = value; }

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
        }

        private class ScopeLevel<T> : IScopeLevel where T : class, IAggregate, new()
        {
            public Func<T, Guid> ParentIdSelector;

            public string EntityTypeName => typeof(T).FullName;

            public async Task<List<EntityWithChildren>> HydrateScopeLevel(DataStore dataStore)
            {
                var stopWatch = new Stopwatch().Op(s => s.Start());

                var aggregates = await dataStore.Read<T>().ConfigureAwait(false);

                Debug.WriteLine($"Hydrating scope level {typeof(T).FullName} cost {stopWatch.ElapsedMilliseconds} milliseconds");

                var projection = aggregates.Select(x => new EntityWithChildren(x, this.ParentIdSelector(x))).ToList();

                return projection;
            }
        }
    }
}