namespace DataStore.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Interfaces;
    using PureFunctions.Extensions;
    using ServiceApi.Interfaces.LowLevel;

    public abstract class Aggregate : Entity, IAggregate
    {
        // .. high level filters
        public bool Active { get; set; } = true;

        public bool ReadOnly { get; set; } = false;

        // .. relationships
        public List<IScopeReference> ScopeReferences { get; set; } = new List<IScopeReference>();

        public void WalkGraphAndUpdateEntityMeta()
        {
            WalkGraphAndUpdateEntityMeta(this);
        }

        private void WalkGraphAndUpdateEntityMeta(object current)
        {
            if (current != null)
            {
                var t = current.GetType();

                foreach (var p in t.GetProperties())
                    if (p.Name == nameof(id))
                    {
                        //set an id for any entity in the tree if it doesn't have one
                        //regardless of whether it is the aggregate or a child entity
                        //in many cases this will already be done in the app code
                        if ((Guid) p.GetValue(current, null) == Guid.Empty)
                            p.SetValue(current, Guid.NewGuid(), null);
                    }
                    else if (p.Name == nameof(Created))
                    {
                        //set created datetime if this is the aggregate and it's null
                        if ((DateTime?) p.GetValue(current, null) == null)
                            p.SetValue(current, DateTime.Now, null);
                    }
                    else if (p.Name == nameof(CreatedNumber))
                    {
                        //set created datetime if this is the aggregate and it's null
                        if (p.GetValue(current, null) == null)
                            p.SetValue(current, DateTime.Now.ConvertToSecondsEpochTime(), null);
                    }
                    else if (p.Name == nameof(Modified))
                    {
                        //if this is the root model
                        if (current is Aggregate)
                            p.SetValue(current, DateTime.Now, null);
                    }
                    else if (!p.PropertyType.IsSystemType())
                    {
                        //one-to-one reference                    
                        WalkGraphAndUpdateEntityMeta(p.GetValue(current, null));
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                    {
                        var collection = p.GetValue(current, null);
                        if (collection != null)
                            foreach (var sub in (IEnumerable) p.GetValue(current, null))
                                WalkGraphAndUpdateEntityMeta(sub);
                    }
            }
        }
    }
}