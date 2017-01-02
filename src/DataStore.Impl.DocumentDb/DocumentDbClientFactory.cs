namespace DataStore.Impl.DocumentDb
{
    using System;
    using Infrastructure.PureFunctions.Extensions;
    using Microsoft.Azure.Documents.Client;
    using Models.Config;

    public class DocumentDbClientFactory
    {
        private readonly DocumentDbSettings _config;

        public DocumentDbClientFactory(DocumentDbSettings config)
        {
            this._config = config;
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
                new Uri(_config.EndpointUrl),
                _config.AuthorizationKey.ToSecureString());
            
            return client;
        }
    }
}