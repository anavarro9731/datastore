namespace DataStore.Interfaces.LowLevel
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
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