namespace DataStore.Interfaces.LowLevel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     This abstract class is here for convenience, so as not to clutter up
    ///     your classes with property implementations.
    ///     The interface is what is used by datastore because
    ///     the benefit of an interface over an abstract class is you can't sneak logic into it.
    ///     e.g. property which may not serialize reliably, or constructor logic which affects field values.
    ///     Furthermore, if you expose add any logic to the base class, even that which is
    ///     serialisation safe, if a client has models assemblies each with a different version
    ///     of this logic, your code could start producing unexpected results.
    ///     So, NO LOGIC of any kind in these abstract classes.
    /// </summary>
    public abstract class Aggregate : Entity, IAggregate
    {
        protected Aggregate()
        {
            //These defaults are here because alot of the tests depend on these defaults.
            //If we refactor all the tests are you can remove it. 
            //However, it is a significant convenience for it to be set correctly by default.
            schema = GetType().FullName;
            Active = true;
        }

        public bool Active { get; set; }

        public bool ReadOnly { get; set; }

        //required lowercase when a docdb partitionkey
        public string schema { get; set; }

        public List<IScopeReference> ScopeReferences { get; set; }
    }
}