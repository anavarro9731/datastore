using PalmTree.Infrastructure.PureFunctions.Extensions;

namespace DataStore.Impl.DocumentDb
{
    using System;
    using Microsoft.Azure.Documents.Client;
    using Models.Config;

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
            var client = new DocumentClient(
                new Uri(config.EndpointUrl),
                config.AuthorizationKey.ToSecureString());
            
            return client;
        }
    }
}