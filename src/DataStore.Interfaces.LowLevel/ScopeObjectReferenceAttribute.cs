namespace DataStore.Interfaces.LowLevel
{
    using System;

    public class ScopeObjectReferenceAttribute : Attribute
    {
        public ScopeObjectReferenceAttribute(Type type)
        {
            FullTypeName = type.FullName;
        }

        public ScopeObjectReferenceAttribute(string fullTypeName)
        {
            FullTypeName = fullTypeName;
        }

        public string FullTypeName { get; }
    }
}