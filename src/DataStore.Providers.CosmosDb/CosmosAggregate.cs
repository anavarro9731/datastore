namespace DataStore.Providers.CosmosDb
{
    using System;
    using Cosmonaut;
    using Cosmonaut.Attributes;
    using DataStore.Interfaces.LowLevel;
    using Newtonsoft.Json;

    [SharedCosmosCollection("globalcollection", true)]
    public abstract class CosmosAggregate : Aggregate, ISharedCosmosEntity
    {
        protected CosmosAggregate()
        {
            CosmosEntityName = Schema;
        }

        /* this field will be ignored by cosmosbecause you have set useEntityFullName
         useEntityFullName uses GetType().FullName and this is important because 
         it needs to match what our Entity.Schema property uses so there is complete alignment
         between datastore features and cosmos features in such a way that the values of the
         properties could be interchangeable should they need to be*/
        public string CosmosEntityName { get; set; }

        [CosmosPartitionKey]        
        public override Guid id { get; set; }
    }
}