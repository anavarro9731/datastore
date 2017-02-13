namespace DataStore.Impl.DocumentDb
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Models.Config;

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
            documentClient = new DocumentClient(new Uri(settings.EndpointUrl), settings.AuthorizationKey);
        }

        public Tuple<string, string> Initialise()
        {
            var db = RetrieveOrCreateDatabaseAsync(documentClient).Result;

            var collection = RetrieveOrCreateCollectionAsync(settings.CollectionSettings, db).Result;

            return new Tuple<string, string>(db.SelfLink, collection.SelfLink);
        }

        private async Task<DocumentCollection> RetrieveOrCreateCollectionAsync(
            DocDbCollectionSettings collectionSettings,
            Database db)
        {
            var collection = documentClient.CreateDocumentCollectionQuery(db.SelfLink)
                .Where(d => d.Id == collectionSettings.CollectionName)
                .AsEnumerable()
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
                    //set this over 10000 to cause docdb to create a partitioned collection
                    requestOptions.OfferThroughput = 10100;
                }

                collection = await documentClient.CreateDocumentCollectionAsync(
                    db.SelfLink,
                    documentCollection, requestOptions).ConfigureAwait(false);
            }


            return collection;
        }

        private async Task<Database> RetrieveOrCreateDatabaseAsync(DocumentClient client)
        {
            var existingDatabase =
                client.CreateDatabaseQuery()
                    .Where(d => d.Id == settings.DatabaseName)
                    .AsEnumerable()
                    .SingleOrDefault();

            if (existingDatabase != null)
                return existingDatabase;

            return
                await client.CreateDatabaseAsync(new Database { Id = settings.DatabaseName }).ConfigureAwait(false);
        }
    }
}