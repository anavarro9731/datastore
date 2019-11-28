namespace DataStore.Interfaces.LowLevel
{
    using System;

    public interface IHaveAUniqueId
    {
        //using lowercase "id" exactly as spelled because CosmosDb must have it this way
        //persistence can be overcome with a JsonProperty("id") but this does nothing for LINQ translation
        Guid id { get; set; }
    }
}