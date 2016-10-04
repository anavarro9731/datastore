namespace DataStore.DataAccess.Impl.DocumentDb
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using System.Linq;
    using Microsoft.Azure.Documents.Linq;
    using Models.Config;

    public interface IDocumentDbInitialiser
    {
        Tuple<string, List<string>> Initialise();
    }

    public class DocumentDbInitialiser : IDocumentDbInitialiser
    {
        private readonly DocumentClient documentClient;

        private readonly DocumentDbSettings settings;

        public DocumentDbInitialiser(DocumentDbSettings settings)
        {
            this.settings = settings;
            this.documentClient = new DocumentClient(new Uri(settings.EndpointUrl), settings.AuthorizationKey);
        }

        public Tuple<string, List<string>> Initialise()
        {
            var db = this.RetrieveOrCreateDatabaseAsync(this.documentClient).Result;

            var collections =
                this.RetrieveOrCreateCollectionsAsync(new List<string>() { this.settings.DefaultCollectionName }, db)
                    .Result;

            return new Tuple<string, List<string>>(db.SelfLink, collections.Select(c => c.SelfLink).ToList());
        }

        private async Task<IEnumerable<DocumentCollection>> RetrieveOrCreateCollectionsAsync(
            List<string> collectionNames, 
            Database db)
        {
            var collections = new List<DocumentCollection>();

            foreach (var collectionName in collectionNames)
            {
                var coll =
                    this.documentClient.CreateDocumentCollectionQuery(db.SelfLink)
                        .Where(d => d.Id == collectionName)
                        .AsEnumerable()
                        .FirstOrDefault()
                    ?? await
                       this.documentClient.CreateDocumentCollectionAsync(
                           db.SelfLink, 
                           new DocumentCollection { Id = collectionName }).ConfigureAwait(false);

                collections.Add(coll);
            }

            return collections;
        }

        private async Task<Database> RetrieveOrCreateDatabaseAsync(DocumentClient client)
        {
            var existingDatabase =
                client.CreateDatabaseQuery()
                    .Where(d => d.Id == this.settings.DatabaseName)
                    .AsEnumerable()
                    .SingleOrDefault();

            if (existingDatabase != null)
            {
                return existingDatabase;
            }

            return
                await client.CreateDatabaseAsync(new Database { Id = this.settings.DatabaseName }).ConfigureAwait(false);
        }
    }
}