using System;
using DataStore.DataAccess.Models.Config;
using DataStore.Infrastructure.PureFunctions.PureFunctions.Extensions;
using Microsoft.Azure.Documents.Client;

namespace DataStore.DataAccess.Impl.DocumentDb
{
    public class DocumentDbClientFactory
    {
        private readonly DocumentDbSettings config;

        private readonly SimplePartitionResolver partitionResolver;

        public DocumentDbClientFactory(DocumentDbSettings config)
        {
            partitionResolver = new SimplePartitionResolver(config);
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
            client.PartitionResolvers[config.DatabaseSelfLink()] = partitionResolver;
            return client;
        }
    }
}