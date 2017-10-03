namespace DataStore.Impl.DocumentDb
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Impl.DocumentDb.Config;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    public interface IDocumentDbInitialiser
    {
        Tuple<string, string> Initialise();
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

        public Tuple<string, string> Initialise()
        {
            var db = RetrieveOrCreateDatabaseAsync(this.documentClient).Result;

            var collection = RetrieveOrCreateCollectionAsync(this.settings.CollectionSettings, db).Result;

            return new Tuple<string, string>(db.SelfLink, collection.SelfLink);
        }

        private async Task<DocumentCollection> RetrieveOrCreateCollectionAsync(DocDbCollectionSettings collectionSettings, Database db)
        {
            var collection = this.documentClient.CreateDocumentCollectionQuery(db.SelfLink).Where(d => d.Id == collectionSettings.CollectionName).AsEnumerable()
                                 .FirstOrDefault();

            if (collection == null)
            {
                var documentCollection = collectionSettings.ToDocumentCollection();

                //required to use partitioned collections
                var requestOptions = new RequestOptions();

                //do not check documentCollection.PartitionKey property because
                //the partitionKey property creates a default value on calls to the getter and default values will fail
                //so make sure not to call it. Bad MS!
                if (collectionSettings.PartitionKeyType != DocDbCollectionSettings.PartitionKeyTypeEnum.None)
                {
                    requestOptions.OfferThroughput = 2600;
                }

                collection = await this.documentClient.CreateDocumentCollectionAsync(db.SelfLink, documentCollection, requestOptions).ConfigureAwait(false);
            }

            return collection;
        }

        private async Task<Database> RetrieveOrCreateDatabaseAsync(DocumentClient client)
        {
            var existingDatabase = client.CreateDatabaseQuery().Where(d => d.Id == this.settings.DatabaseName).AsEnumerable().SingleOrDefault();

            if (existingDatabase != null)
            {
                return existingDatabase;
            }

            return await client.CreateDatabaseAsync(
                       new Database
                       {
                           Id = this.settings.DatabaseName
                       }).ConfigureAwait(false);
        }
    }
}