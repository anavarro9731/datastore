namespace DataStore.Providers.CosmosDb
{
    using System;
    using System.IO;
    using System.Text;
    using DataStore.Interfaces.LowLevel;
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public sealed class CosmosJsonDotNetSerializer : CosmosSerializer
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CosmosMetadataPreservingResolver()
        };

        public override T FromStream<T>(Stream stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
                return (T)(object)stream;

            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                var js = JsonSerializer.Create(this._settings);
                return js.Deserialize<T>(jr);
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms, DefaultEncoding);
            var jw = new JsonTextWriter(sw) { Formatting = Formatting.None };

            JsonSerializer.Create(this._settings).Serialize(jw, input);

            jw.Flush();
            sw.Flush();

            ms.Position = 0;
            return ms; // caller disposes this stream
        }


        public class CosmosMetadataPreservingResolver : DefaultContractResolver
        {
            protected override JsonContract CreateContract(Type objectType)
            {
                var contract = base.CreateContract(objectType);

                // If this type has an ETag property
                if (typeof(IHaveAnETag).IsAssignableFrom(objectType))
                    if (contract is JsonObjectContract objectContract)
                    {
                        // Add a mapping: JSON "_etag" ↔ C# "Etag" property
                        var etagProperty = new JsonProperty
                        {
                            PropertyName = "_etag", // JSON field name from Cosmos
                            PropertyType = typeof(string), // Type of the property
                            Readable = true, // Can read from JSON
                            Writable = true, // Can write to JSON
                            ValueProvider = new ETagValueProvider() // Custom logic
                        };

                        objectContract.Properties.Add(etagProperty);
                    }

                return contract;
            }

            public class ETagValueProvider : IValueProvider
            {
                public void SetValue(object target, object value)
                {
                    if (target is IHaveAnETag etagged) etagged.Etag = value?.ToString();
                }

                public object GetValue(object target)
                {
                    return (target as IHaveAnETag)?.Etag;
                }
            }
        }
    }
}