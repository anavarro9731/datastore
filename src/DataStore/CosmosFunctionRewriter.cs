namespace DataStore
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    #endregion

    /// <summary>
    ///     Cosmos LINQ extensions such as IsDefined() and IsNull() are only markers that the Cosmos provider translates into SQL;
    ///     calling them directly throws. This visitor replaces them with in-memory equivalents so the same predicates can run
    ///     against the InMemoryDocumentRepository.
    ///     In memory there is no difference between "missing" and "null", so IsDefined() means "is not null" and IsNull() means "is null".
    ///     Non-nullable value types (int, bool, Guid, DateTime, enums) are always considered defined, even at their default value, since a CLR object cannot be missing a field.
    ///     This differs from Cosmos for documents saved before a property was added; use a nullable property to simulate those.
    ///     Where the argument is reached through a member chain (x.Child.Prop) a null parent makes both return false rather than throw.
    ///     Methods are matched by name so the core library does not need a reference to the Cosmos SDK.
    /// </summary>
    public class CosmosFunctionRewriter : ExpressionVisitor
    {
        private const string CosmosExtensionsTypeName = "CosmosLinqExtensions";

        public bool Changed { get; private set; }

        public static Expression<Func<T, bool>> Rewrite<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
            {
                return null;
            }

            var visitor = new CosmosFunctionRewriter();
            var result = (Expression<Func<T, bool>>)visitor.Visit(predicate);
            return visitor.Changed ? result : predicate;
        }

        public static IQueryable<T> Rewrite<T>(IQueryable<T> query)
        {
            if (query == null)
            {
                return null;
            }

            var visitor = new CosmosFunctionRewriter();
            var expression = visitor.Visit(query.Expression);
            return visitor.Changed ? query.Provider.CreateQuery<T>(expression) : query;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType?.Name == CosmosExtensionsTypeName && node.Arguments.Count == 1 &&
                (node.Method.Name == "IsDefined" || node.Method.Name == "IsNull"))
            {
                Changed = true;

                // visit first so nested rewrites are applied
                var argument = StripConversions(Visit(node.Arguments[0]));
                return node.Method.Name == "IsDefined" ? BuildIsDefined(argument) : BuildIsNull(argument);
            }

            return base.VisitMethodCall(node);
        }

        private static Expression BuildIsDefined(Expression argument)
        {
            if (!CanBeNull(argument.Type))
            {
                return And(NullGuards(argument), Expression.Constant(true));
            }

            return And(NullGuards(argument), Expression.NotEqual(argument, Expression.Constant(null, argument.Type)));
        }

        private static Expression BuildIsNull(Expression argument)
        {
            if (!CanBeNull(argument.Type))
            {
                return Expression.Constant(false);
            }

            return And(NullGuards(argument), Expression.Equal(argument, Expression.Constant(null, argument.Type)));
        }

        private static Expression And(IEnumerable<Expression> guards, Expression final)
        {
            return guards.Reverse().Aggregate(final, (acc, guard) => Expression.AndAlso(guard, acc));
        }

        /// <summary>For x.A.B.C returns [x.A != null, x.A.B != null], outermost first.</summary>
        private static List<Expression> NullGuards(Expression argument)
        {
            var guards = new List<Expression>();
            var current = (argument as MemberExpression)?.Expression;
            while (current is MemberExpression member)
            {
                if (CanBeNull(member.Type))
                {
                    guards.Insert(0, Expression.NotEqual(member, Expression.Constant(null, member.Type)));
                }

                current = member.Expression;
            }

            return guards;
        }

        private static bool CanBeNull(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        private static Expression StripConversions(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked ||
                   expression.NodeType == ExpressionType.TypeAs)
            {
                // IsDefined(this object) boxes value types; unwrap to inspect the real type
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }
    }
}
