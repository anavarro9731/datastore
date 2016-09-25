namespace DataAccess.Impl.DocumentDb
{
    using System;
    using Infrastructure.Configuration.Settings;
    using Infrastructure.PureFunctions.Extensions;
    using Microsoft.Azure.Documents.Client;

    public class DocumentDbClientFactory
    {
        private static readonly object Locker = new object();

        private static volatile bool initialised;

        private readonly DocumentDbSettings config;

        private readonly IDocumentDbInitialiser initialiser;

        private readonly SimplePartitionResolver partitionResolver;

        public DocumentDbClientFactory(DocumentDbSettings config)
        {
            this.initialiser = new DocumentDbInitialiser(config);
            this.partitionResolver = new SimplePartitionResolver(config);
            this.config = config;
        }

        public DocumentClient GetDocumentClient()
        {
            this.InitialiseIfRequired();

            var client = this.CreateDocumentClient();

            return client;
        }

        private DocumentClient CreateDocumentClient()
        {
            var client = new DocumentClient(
                new Uri(this.config.EndpointUrl),
                this.config.AuthorizationKey.ToSecureString(),
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp });
            client.PartitionResolvers[this.config.DatabaseSelfLink()] = this.partitionResolver;
            return client;
        }

        private void InitialiseIfRequired()
        {
            if (!initialised)
            {
                lock (Locker)
                {
                    if (!initialised)
                    {
                        this.CreateDocumentClient();

                        this.initialiser.Initialise();

                        initialised = true;
                    }
                }
            }
        }
    }
}