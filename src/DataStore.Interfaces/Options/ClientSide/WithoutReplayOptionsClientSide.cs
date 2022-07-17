namespace DataStore.Interfaces.Options.ClientSide
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options.ClientSide.Interfaces;

    public class WithoutReplayOptionsClientSide<T> : WithoutReplayOptionsClientSideBase<T>, IWithoutReplayOptionsClientSide<T> where T : class, IAggregate, new()
    {
        public IWithoutReplayOptionsClientSide<T> ContinueFrom(ContinuationToken currentContinuationToken)
        {
            if (currentContinuationToken?.Value == null)
            {
                throw new Exception("The continuation token provided cannot be used since it's value is null");
            }

            LibrarySide.CurrentContinuationToken = currentContinuationToken;

            return this;
        }

        public IWithoutReplayOptionsClientSide<T> OrderBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            LibrarySide.OrderByProperty = Objects.GetPropertyName(propertyRefExpr);
            LibrarySide.OrderDescending = descending;
            return this;
        }

        public IWithoutReplayOptionsClientSide<T> Take(int take, ref ContinuationToken newContinuationToken)
        {
            LibrarySide.MaxTake = take;
            LibrarySide.NextContinuationToken = newContinuationToken ?? throw new Exception("ContinuationToken cannot be null");

            return this;
        }

        public IWithoutReplayOptionsClientSide<T> ThenBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            var propertyName = Objects.GetPropertyName(propertyRefExpr);
            LibrarySide.ThenByQueue.Enqueue((propertyName, descending));
            return this;
        }
    }

    public static class Objects
    {
        /// <summary>
        ///     get property name from current instance
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="type"></param>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        public static string GetPropertyName<TObject>(this TObject type, Expression<Func<TObject, object>> propertyRefExpr)
        {
            // usage: obj.GetPropertyName(o => o.Member)
            return GetPropertyNameCore(propertyRefExpr.Body);
        }

        /// <summary>
        ///     get property name from any class
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="propertyRefExpr"></param>
        /// <returns></returns>
        public static string GetPropertyName<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
        {
            // usage: Objects.GetPropertyName<SomeClass>(sc => sc.Member)
            return GetPropertyNameCore(propertyRefExpr.Body);
        }

        /// <summary>
        ///     get static property name from any class
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetStaticPropertyName<TResult>(Expression<Func<TResult>> expression)
        {
            // usage: Objects.GetStaticPropertyName(t => t.StaticProperty)
            return GetPropertyNameCore(expression);
        }

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
    }
}