namespace DataStore.DataAccess.Impl.DocumentDb
{
    using System.Collections.Generic;
    using Infrastructure.Configuration.Settings;
    using Microsoft.Azure.Documents.Client;

    public class SimplePartitionResolver : IPartitionResolver
    {
        private readonly DocumentDbSettings config;

        public SimplePartitionResolver(DocumentDbSettings config)
        {
            this.config = config;
        }

        public object GetPartitionKey(object document)
        {
            return GetPartitionKeyFromId(document as dynamic);
        }

        public string ResolveForCreate(object partitionKey)
        {
            return
                UriFactory.CreateDocumentCollectionUri(this.config.DatabaseName, this.config.DefaultCollectionName)
                    .ToString();
        }

        public IEnumerable<string> ResolveForRead(object partitionKey)
        {
            return new[]
                       {
                           UriFactory.CreateDocumentCollectionUri(
                               this.config.DatabaseName, 
                               this.config.DefaultCollectionName).ToString()
                       };
        }

        internal static object GetPartitionKeyFromId(object id)
        {
            return null;
        }
    }
}