namespace DataStore.Interfaces
{
    public interface IDatabaseSettings
    {
        IDocumentRepository CreateRepository(); 
    }
}