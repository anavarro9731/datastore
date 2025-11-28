namespace DataStore.Providers.CosmosDb
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using DataStore.Interfaces.LowLevel;
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public sealed class CosmosJsonDotNetSerializer : CosmosSerializer
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        private readonly JsonSerializerSettings _settings =
        new JsonSerializerSettings() {
            ContractResolver = new CosmosMetadataPreservingResolver(),
            SerializationBinder = new TypeNameOnlyBinder()
        };

        public override T FromStream<T>(Stream stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                var js = typeof(T).Name == "ProcessState" ? JsonSerializer.CreateDefault() : JsonSerializer.CreateDefault(this._settings);
                return js.Deserialize<T>(jr);
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms, DefaultEncoding);
            var jw = new JsonTextWriter(sw)
            {
                Formatting = Formatting.None
            };

            var js = typeof(T).Name == "ProcessState" ? JsonSerializer.CreateDefault() : JsonSerializer.CreateDefault(this._settings);
            js.Serialize(jw, input);

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
                {
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
                }

                return contract;
            }

            public class ETagValueProvider : IValueProvider
            {
                public object GetValue(object target)
                {
                    return (target as IHaveAnETag)?.Etag;
                }

                public void SetValue(object target, object value)
                {
                    if (target is IHaveAnETag etagged) etagged.Etag = value?.ToString();
                }
            }
        }

        public class TypeNameOnlyBinder : ISerializationBinder
        {
            public void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                
                // Always use just the class name instead of full type name - this enables progressive data migration
                typeName = serializedType.Name;
            }

            public Type BindToType(string assemblyName, string typeName)
            {
                var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                                        .Where(x => x.GetName().Name.StartsWith("QCS.") || x.GetName().Name.StartsWith("Dapper") || x.GetName().Name.Equals("System.Data.SqlClient"))
                                        .SelectMany(a => a.GetTypes())
                                        .Where(t => typeof(DataStore.Interfaces.LowLevel.IEntity).IsAssignableFrom(t)) // Only allow IEntity types
                                        .ToList();

                // First, look for types whose actual class name matches
                var nameMatches = allTypes.Where(t => t.Name == typeName).ToList();
                
                // Then, look for types with SerialisedNames attribute matching the requested name
                var attributeMatches = allTypes.Where(t => 
                {
                    var attribute = t.GetCustomAttribute<SerialisedNamesAttribute>();
                    return attribute?.Names?.Contains(typeName) == true;
                }).ToList();

                var allMatches = nameMatches.Concat(attributeMatches).Distinct().ToList();
    
                if (allMatches.Count == 1) return allMatches[0];
    
                if (allMatches.Count == 0)
                    throw new JsonSerializationException($"Attempted to deserialize a type '{typeName}' that doesn't implement the {nameof(IEntity)} interface. An unconstrained $type property is a security vulnerability and may indicate malicious or corrupted data.");
    
                throw new JsonSerializationException(
                    $"Multiple IEntity types found with name '{typeName}': {string.Join(", ", allMatches.Select(t => t.FullName))}");
            }
        }
    }
}