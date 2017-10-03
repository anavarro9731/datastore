namespace DataStore.Impl.DocumentDb.Config
{
    using System;
    using DataStore.Interfaces.LowLevel;
    using Microsoft.Azure.Documents;

    public class DocDbCollectionSettings
    {
        private DocDbCollectionSettings(string collectionName, PartitionKeyTypeEnum partitionKey, bool enableCrossParitionQueries)
        {
            CollectionName = collectionName;
            PartitionKeyType = partitionKey;
            EnableCrossParitionQueries = enableCrossParitionQueries;
        }

        public enum PartitionKeyTypeEnum
        {
            ClassName,

            Id,

            None
        }

        public string CollectionName { get; }

        public bool EnableCrossParitionQueries { get; set; }

        public PartitionKeyTypeEnum PartitionKeyType { get; }

        public static DocDbCollectionSettings Create(string collectionName, PartitionKeyTypeEnum partitionKey = PartitionKeyTypeEnum.None)
        {
            return new DocDbCollectionSettings(collectionName, partitionKey, partitionKey != PartitionKeyTypeEnum.None);
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
                            "/" + nameof(Aggregate.schema).ToLower()
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

                case PartitionKeyTypeEnum.None: return null;

                default: throw new ArgumentOutOfRangeException(nameof(partitionKeyType), partitionKeyType.ToString(), null);
            }
        }
    }
}