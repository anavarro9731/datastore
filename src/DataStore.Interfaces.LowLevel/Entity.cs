namespace DataStore.Interfaces.LowLevel
{
    using System;

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
    public abstract class Entity : IEntity
    {
        // apart from keeping an audit trail this is used for sorting 
        public DateTime? Created { get; set; }

        //here for easier comparison in some systems such as docdb
        public double? CreatedAsMillisecondsEpochTime { get; set; }

        // this is here to give references which are stored in a models json a unique id which is necessary during updates to determines 
        // what changes have occurred. It can either be implemented as-is or the getter can be overridden to select another existing property as the key
        public Guid id { get; set; }
        
    }
}