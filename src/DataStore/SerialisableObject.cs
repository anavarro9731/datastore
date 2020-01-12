namespace DataStore
{
    using System;
    using System.Text.Json;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class SerialisableObject
    {
        public SerialisableObject(object x)
        {
            ObjectData = JsonSerializer.Serialize(x);
            TypeName = x.GetType().AssemblyQualifiedName;
        }

        public string ObjectData { get; internal set; }

        public string TypeName { get; internal set; }

        public T Deserialise<T>() where T : class
        {
            T obj = JsonSerializer.Deserialize(ObjectData, Type.GetType(TypeName)).As<T>();
            return obj;
        }

    }
}