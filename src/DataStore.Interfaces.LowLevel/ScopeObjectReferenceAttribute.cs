namespace DataStore.Interfaces.LowLevel
{
    using System;

    public class ScopeObjectReferenceAttribute : Attribute
    {
        public string FullTypeName { get; }

        public ScopeObjectReferenceAttribute(Type type)
        {
            FullTypeName = type.FullName;
        }

        public ScopeObjectReferenceAttribute(string fullTypeName)
        {
            FullTypeName = fullTypeName;
        }
    }
}