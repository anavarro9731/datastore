namespace DataStore.Models.PureFunctions.Extensions
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.Json;

    public static class Objects
    {
        private static readonly char[] SystemTypeChars =
        {
            '<', '>', '+'
        };

        /// <summary>
        ///     a simpler cast
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T As<T>(this object obj) where T : class => obj as T;

        public static T Cast<T>(this object o) => (T)o;

        public static T Clone<T>(this T source) => source.ToJsonString().FromJsonString<T>();

        /// <summary>
        ///     copies the values of matching properties from one object to another regardless of type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="exclude"></param>
        public static void CopyPropertiesTo(this object source, object destination, params string[] exclude)
        {
            // If any this null throw an exception
            if (source == null || destination == null) throw new Exception("Source or/and Destination Objects are null");

            // Getting the Types of the objects
            var typeDest = destination.GetType();
            var typeSrc = source.GetType();

            // Collect all the valid properties to map
            var results = from srcProp in typeSrc.GetProperties()
                          let targetProperty = typeDest.GetProperty(srcProp.Name)
                          where srcProp.CanRead && targetProperty != null && targetProperty.GetSetMethod(true) != null
                                && !targetProperty.GetSetMethod(true).IsPrivate
                                && (targetProperty.GetSetMethod(true).Attributes & MethodAttributes.Static) == 0
                                && targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)
                                && !exclude.Contains(targetProperty.Name)
                          select new
                          {
                              sourceProperty = srcProp, targetProperty
                          };

            // map the properties
            foreach (var props in results) props.targetProperty.SetValue(destination, props.sourceProperty.GetValue(source, null), null);
        }

        public static T FromJsonString<T>(this string source) => source == null ? default : JsonSerializer.Deserialize<T>(source);

        /// <summary>
        ///     get property name from current instance
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="type"></param>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        public static string GetPropertyName<TObject>(this TObject type, Expression<Func<TObject, object>> propertyRefExpr) =>
            // usage: obj.GetPropertyName(o => o.Member)
            GetPropertyNameCore(propertyRefExpr.Body);

        /// <summary>
        ///     get property name from any class
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        public static string GetPropertyName<TObject>(Expression<Func<TObject, object>> propertyRefExpr) =>
            // usage: Objects.GetPropertyName<SomeClass>(sc => sc.Member)
            GetPropertyNameCore(propertyRefExpr.Body);

        /// <summary>
        ///     get static property name from any class
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetStaticPropertyName<TResult>(Expression<Func<TResult>> expression) =>
            // usage: Objects.GetStaticPropertyName(t => t.StaticProperty)
            GetPropertyNameCore(expression);

        /// <summary>
        ///     checks if a class inherits from or implements a base class/interface.
        ///     Superbly supports generic interfaces and types!
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool InheritsOrImplements(this Type child, Type parent)
        {
            parent = ResolveGenericTypeDefinition(parent);

            var currentChild = child.IsGenericType ? child.GetGenericTypeDefinition() : child;

            while (currentChild != typeof(object))
            {
                if (parent == currentChild || //this get a direct match 
                    parent == currentChild.BaseType || //this gets a specific generic impl BaseType<SomeType>
                    HasAnyInterfaces(parent, currentChild))
                    //this child implements any parent interfaces (not sure about specific impl like BaseType<SomeType> requires a test
                {
                    return true;
                }

                currentChild = currentChild.BaseType != null && currentChild.BaseType.IsGenericType
                                   ? currentChild.BaseType.GetGenericTypeDefinition() //this gets a generic impl BaseType<>
                                   : currentChild.BaseType; //this just sets up the next child type

                if (currentChild == null) return false;
            }

            return false;
        }

        public static bool Is(this object child, Type t) => child.GetType().InheritsOrImplements(t);

        public static bool IsAnonymousType(this Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        public static bool IsSystemType(this Type type) =>
            type.Namespace == null || type.Namespace.StartsWith("System") || type.Name.IndexOfAny(SystemTypeChars) >= 0;

        /// <summary>
        ///     perform an operation on any class inline, (e.g. new Object().Op(o => someoperationon(o));
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        public static T Op<T>(this T obj, Action<T> op)
        {
            op(obj);
            return obj;
        }

        public static string ToJsonString(this object source) => source == null ? null : JsonSerializer.Serialize(source);

        private static string GetPropertyNameCore(Expression propertyRefExpr)
        {
            if (propertyRefExpr == null) throw new ArgumentNullException(nameof(propertyRefExpr), "propertyRefExpr is null.");

            var memberExpr = propertyRefExpr as MemberExpression;
            if (memberExpr == null)
            {
                var unaryExpr = propertyRefExpr as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                {
                    memberExpr = unaryExpr.Operand as MemberExpression;
                }
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property) return memberExpr.Member.Name;

            throw new ArgumentException("No property reference expression was found.", nameof(propertyRefExpr));
        }

        private static bool HasAnyInterfaces(Type parent, Type child)
        {
            return child.GetInterfaces().Any(
                childInterface =>
                    {
                    var currentInterface = childInterface.IsGenericType
                                               ? childInterface.GetGenericTypeDefinition()
                                               : childInterface;

                    return currentInterface == parent;
                    });
        }

        private static Type ResolveGenericTypeDefinition(Type parent)
        {
            var shouldUseGenericType = true;
            if (parent.IsGenericType && parent.GetGenericTypeDefinition() != parent) shouldUseGenericType = false;

            if (parent.IsGenericType && shouldUseGenericType) parent = parent.GetGenericTypeDefinition();

            return parent;
        }
    }
}