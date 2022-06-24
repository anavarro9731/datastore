namespace DataStore.Interfaces.LowLevel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

    //* permission still required but no scope match is required
    [AttributeUsage(AttributeTargets.Class)]
    public class BypassSecurity : Attribute
    {
        public List<SecurableOperation> ForTheseOperations { get; }

        public BypassSecurity(params SecurableOperation[] forTheseOperations)
        {
            ForTheseOperations = forTheseOperations.ToList();
        }
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class ContainsPIIAttribute : Attribute
    {
        
    }
}