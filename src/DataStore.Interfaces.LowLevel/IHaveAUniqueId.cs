namespace DataStore.Interfaces.LowLevel
{
    using System;

    //this must remain as lowercase until such a time as documentdb linq provider
    //can translate Id to id for its DocDb SQL-esque query syntax which requires the
    //query to be written using id to match the mandatory id field
    public interface IHaveAUniqueId
    {
        Guid id { get; set; }
    }
}