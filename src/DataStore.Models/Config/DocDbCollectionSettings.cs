using System;
using Microsoft.Azure.Documents;

namespace DataStore.Models.Config
{
    public class DocDbCollectionSettings
    {
        public enum PartitionKeyTypeEnum
        {
            ClassName,
            Id,
            None
        }

        private DocDbCollectionSettings(string collectionName, PartitionKeyTypeEnum partitionKey,
            bool enableCrossParitionQueries)
        {
            CollectionName = collectionName;
            PartitionKeyType = partitionKey;
            EnableCrossParitionQueries = enableCrossParitionQueries;
        }

        public string CollectionName { get; }

        public PartitionKeyTypeEnum PartitionKeyType { get; }
        public bool EnableCrossParitionQueries { get; set; }

        public static DocDbCollectionSettings Create(string collectionName,
            PartitionKeyTypeEnum partitionKey = PartitionKeyTypeEnum.None)
        {
            return new DocDbCollectionSettings(collectionName, partitionKey, partitionKey != PartitionKeyTypeEnum.None);
        }

        public PartitionKeyDefinition ToPrivateKeyDefinition()
        {
            var partitionKeyType = PartitionKeyType;
            switch (partitionKeyType)
            {
                case PartitionKeyTypeEnum.ClassName:
                    return new PartitionKeyDefinition
                    {
                        Paths =
                        {
                            "/" + nameof(Aggregate.Schema).ToLower()
                        }
                    };
                case PartitionKeyTypeEnum.Id:

                    return new PartitionKeyDefinition
                    {
                        Paths =
                        {
                            "/" + nameof(Aggregate.id)
                        }
                    };

                case PartitionKeyTypeEnum.None:

                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(partitionKeyType), partitionKeyType.ToString(), null);
            }
        }

        public DocumentCollection ToDocumentCollection()
        {
            var documentCollection = new DocumentCollection
            {
                Id = CollectionName
            };

            //the partitionKey property creates a default value on calls to the getter and default values will fail
            //so make sure not to call it. Bad MS!
            if (ToPrivateKeyDefinition() != null) documentCollection.PartitionKey = ToPrivateKeyDefinition();

            return documentCollection;
        }
    }
}