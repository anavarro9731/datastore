namespace DataStore.Tests.Models
{
    using System;

    public class ScopeObjectReferenceAttribute : Attribute
    {
        public Type Type { get; }

        public ScopeObjectReferenceAttribute(Type type)
        {
            Type = type;
        }
    }
}