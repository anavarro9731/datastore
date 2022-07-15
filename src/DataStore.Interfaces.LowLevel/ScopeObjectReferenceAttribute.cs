namespace DataStore.Interfaces.LowLevel
{
    #region

    using System;

    #endregion

    /// <summary>
    ///     Only works on Guid or IEnumerable Guid property types
    /// </summary>
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