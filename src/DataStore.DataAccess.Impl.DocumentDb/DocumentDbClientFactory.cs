namespace Finygo.DocumentDb
{
    using System;

    using Infrastructure.Configuration.Settings;
    using Infrastructure.PureFunctions.Extensions;

    using Microsoft.Azure.Documents.Client;

    using Serilog;

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
            try
            {
                this.InitialiseIfRequired();

                var client = this.CreateDocumentClient();

                return client;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to get documentclient: {error}", e.Message);
                throw;
            }
        }

        private DocumentClient CreateDocumentClient()
        {
            var client = new DocumentClient(
                new Uri(this.config.EndpointUrl), 
                this.config.AuthorizationKey.ToSecureString());
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

                        try
                        {
                            this.initialiser.Initialise();
                        }
                        catch (Exception e)
                        {
                            Log.Logger.Fatal(e, "Failed to initialise the database: {err}", e.Message);
                            throw;
                        }

                        initialised = true;
                    }
                }
            }
        }
    }
}