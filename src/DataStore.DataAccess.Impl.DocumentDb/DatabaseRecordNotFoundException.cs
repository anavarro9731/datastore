namespace Finygo.DocumentDb
{
    public class DatabaseRecordNotFoundException : DatabaseException
    {
        public DatabaseRecordNotFoundException(string id)
            : base($"Unable to find item with id {id}")
        {
        }
    }
}