namespace DataStore.Impl.DocumentDb
{
    using System;
    using DataStore.Impl.DocumentDb.Config;
    using DataStore.Models.PureFunctions.Extensions;
    using Microsoft.Azure.Documents.Client;

    public class DocumentDbClientFactory
    {
        private readonly DocumentDbSettings config;

        public DocumentDbClientFactory(DocumentDbSettings config)
        {
            this.config = config;
            new DocumentDbInitialiser(config).Initialise();
        }

        public DocumentClient GetDocumentClient()
        {
            var client = CreateDocumentClient();

            return client;
        }

        private DocumentClient CreateDocumentClient()
        {
            var client = new DocumentClient(new Uri(this.config.EndpointUrl), this.config.AuthorizationKey.ToSecureString());

            return client;
        }
    }
}