using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    public sealed class DataLoadOptions
    {
        private Dictionary<MetaPosition, MemberInfo> includes = new Dictionary<MetaPosition, MemberInfo>();
        private Dictionary<MetaPosition, LambdaExpression> subqueries = new Dictionary<MetaPosition, LambdaExpression>();
        private bool frozen;

        internal bool IsEmpty
        {
            get
            {
                if (this.includes.Count == 0)
                    return this.subqueries.Count == 0;
                return false;
            }
        }

        /// <summary>
        /// 指定在为 T 类型的对象提交查询时要检索的子对象。
        /// </summary>
        /// <param name="expression">标识要检索的字段或属性。如果该表达式不标识表示一对一关系或一对多关系的字段或属性，则会引发异常。</param><typeparam name="T">要查询的类型。如果未映射此类型，则会引发异常。</typeparam>
        public void LoadWith<T>(Expression<Func<T, object>> expression)
        {
            if (expression == null)
                throw Error.ArgumentNull("expression");
            this.Preload(DataLoadOptions.GetLoadWithMemberInfo((LambdaExpression)expression));
        }

        /// <summary>
        /// 通过使用 lambda 表达式检索与主目标相关的指定数据。
        /// </summary>
        /// <param name="expression">标识相关内容的 lambda 表达式。</param><filterpriority>2</filterpriority>
        public void LoadWith(LambdaExpression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull("expression");
            this.Preload(DataLoadOptions.GetLoadWithMemberInfo(expression));
        }

        /// <summary>
        /// 筛选针对特定关系检索的对象。
        /// </summary>
        /// <param name="expression">标识要对特定一对多字段或属性使用的查询。注意下列事项：如果该表达式不是以表示一对多关系的字段或属性开头，则会引发异常。如果无效运算符出现在表达式中，则会引发异常。有效运算符包括：WhereOrderByThenByOrderByDescendingThenByDescendingTake</param><typeparam name="T">要查询的类型。如果未映射该类型，则会引发异常。</typeparam>
        public void AssociateWith<T>(Expression<Func<T, object>> expression)
        {
            if (expression == null)
                throw Error.ArgumentNull("expression");
            this.AssociateWithInternal((LambdaExpression)expression);
        }

        /// <summary>
        /// 筛选针对特定关系检索的对象。
        /// </summary>
        /// <param name="expression">标识要对特定一对多字段或属性使用的查询。注意下列事项：如果该表达式不是以表示一对多关系的字段或属性开头，则会引发异常。如果无效运算符出现在表达式中，则会引发异常。有效运算符包括：WhereOrderByThenByOrderByDescendingThenByDescendingTake</param><filterpriority>2</filterpriority>
        public void AssociateWith(LambdaExpression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull("expression");
            this.AssociateWithInternal(expression);
        }

        private void AssociateWithInternal(LambdaExpression expression)
        {
            Expression body = expression.Body;
            while (body.NodeType == ExpressionType.Convert || body.NodeType == ExpressionType.ConvertChecked)
                body = ((UnaryExpression)body).Operand;
            LambdaExpression lambdaExpression = Expression.Lambda(body, System.Linq.Enumerable.ToArray<ParameterExpression>((IEnumerable<ParameterExpression>)expression.Parameters));
            this.Subquery(DataLoadOptions.Searcher.MemberInfoOf(lambdaExpression), lambdaExpression);
        }

        internal bool IsPreloaded(MemberInfo member)
        {
            if (member == (MemberInfo)null)
                throw Error.ArgumentNull("member");
            return this.includes.ContainsKey(new MetaPosition(member));
        }

        internal static bool ShapesAreEquivalent(DataLoadOptions ds1, DataLoadOptions ds2)
        {
            if ((ds1 == ds2 ? 1 : (ds1 == null || ds1.IsEmpty ? (ds2 == null ? 1 : (ds2.IsEmpty ? 1 : 0)) : 0)) == 0)
            {
                if (ds1 == null || ds2 == null || ds1.includes.Count != ds2.includes.Count)
                    return false;
                foreach (MetaPosition key in ds2.includes.Keys)
                {
                    if (!ds1.includes.ContainsKey(key))
                        return false;
                }
            }
            return true;
        }

        internal LambdaExpression GetAssociationSubquery(MemberInfo member)
        {
            if (member == (MemberInfo)null)
                throw Error.ArgumentNull("member");
            LambdaExpression lambdaExpression = (LambdaExpression)null;
            this.subqueries.TryGetValue(new MetaPosition(member), out lambdaExpression);
            return lambdaExpression;
        }

        internal void Freeze()
        {
            this.frozen = true;
        }

        internal void Preload(MemberInfo association)
        {
            if (association == (MemberInfo)null)
                throw Error.ArgumentNull("association");
            if (this.frozen)
                throw Error.IncludeNotAllowedAfterFreeze();
            this.includes.Add(new MetaPosition(association), association);
            this.ValidateTypeGraphAcyclic();
        }

        private void Subquery(MemberInfo association, LambdaExpression subquery)
        {
            if (this.frozen)
                throw Error.SubqueryNotAllowedAfterFreeze();
            subquery = (LambdaExpression)Funcletizer.Funcletize((Expression)subquery);
            DataLoadOptions.ValidateSubqueryMember(association);
            DataLoadOptions.ValidateSubqueryExpression(subquery);
            this.subqueries[new MetaPosition(association)] = subquery;
        }

        private static MemberInfo GetLoadWithMemberInfo(LambdaExpression lambda)
        {
            Expression expression = lambda.Body;
            if (expression != null && (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked))
                expression = ((UnaryExpression)expression).Operand;
            MemberExpression memberExpression = expression as MemberExpression;
            if (memberExpression != null && memberExpression.Expression.NodeType == ExpressionType.Parameter)
                return memberExpression.Member;
            throw Error.InvalidLoadOptionsLoadMemberSpecification();
        }

        private void ValidateTypeGraphAcyclic()
        {
            IEnumerable<MemberInfo> enumerable = (IEnumerable<MemberInfo>)this.includes.Values;
            int num = 0;
            for (int index = 0; index < this.includes.Count; ++index)
            {
                HashSet<Type> hashSet = new HashSet<Type>();
                foreach (MemberInfo mi in enumerable)
                    hashSet.Add(DataLoadOptions.GetIncludeTarget(mi));
                List<MemberInfo> list = new List<MemberInfo>();
                bool flag = false;
                foreach (MemberInfo memberInfo in enumerable)
                {
                    MemberInfo edge = memberInfo;
                    if (System.Linq.Enumerable.Any<Type>(System.Linq.Enumerable.Where<Type>((IEnumerable<Type>)hashSet, (Func<Type, bool>)(et =>
                    {
                        if (!et.IsAssignableFrom(edge.DeclaringType))
                            return edge.DeclaringType.IsAssignableFrom(et);
                        return true;
                    }))))
                    {
                        list.Add(edge);
                    }
                    else
                    {
                        ++num;
                        flag = true;
                        if (num == this.includes.Count)
                            return;
                    }
                }
                if (!flag)
                    throw Error.IncludeCycleNotAllowed();
                enumerable = (IEnumerable<MemberInfo>)list;
            }
            throw new InvalidOperationException("Bug in ValidateTypeGraphAcyclic");
        }

        private static Type GetIncludeTarget(MemberInfo mi)
        {
            Type memberType = TypeSystem.GetMemberType(mi);
            if (memberType.IsGenericType)
                return memberType.GetGenericArguments()[0];
            return memberType;
        }

        private static void ValidateSubqueryMember(MemberInfo mi)
        {
            Type memberType = TypeSystem.GetMemberType(mi);
            if (memberType == (Type)null)
                throw Error.SubqueryNotSupportedOn((object)mi);
            if (!typeof(IEnumerable).IsAssignableFrom(memberType))
                throw Error.SubqueryNotSupportedOnType((object)mi.Name, (object)mi.DeclaringType);
        }

        private static void ValidateSubqueryExpression(LambdaExpression subquery)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(subquery.Body.Type))
                throw Error.SubqueryMustBeSequence();
            new DataLoadOptions.SubqueryValidator().VisitLambda(subquery);
        }

        private static class Searcher
        {
            internal static MemberInfo MemberInfoOf(LambdaExpression lambda)
            {
                DataLoadOptions.Searcher.Visitor visitor = new DataLoadOptions.Searcher.Visitor();
                LambdaExpression lambda1 = lambda;
                visitor.VisitLambda(lambda1);
                return visitor.MemberInfo;
            }

            private class Visitor : ExpressionVisitor
            {
                internal MemberInfo MemberInfo;

                internal override Expression VisitMemberAccess(MemberExpression m)
                {
                    this.MemberInfo = m.Member;
                    return base.VisitMemberAccess(m);
                }

                internal override Expression VisitMethodCall(MethodCallExpression m)
                {
                    this.Visit(m.Object);
                    using (IEnumerator<Expression> enumerator = m.Arguments.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                            this.Visit(enumerator.Current);
                    }
                    return (Expression)m;
                }
            }
        }

        private class SubqueryValidator : ExpressionVisitor
        {
            private bool isTopLevel = true;

            internal override Expression VisitMethodCall(MethodCallExpression m)
            {
                bool flag = this.isTopLevel;
                try
                {
                    if (this.isTopLevel && !SubqueryRules.IsSupportedTopLevelMethod(m.Method))
                        throw Error.SubqueryDoesNotSupportOperator((object)m.Method.Name);
                    this.isTopLevel = false;
                    return base.VisitMethodCall(m);
                }
                finally
                {
                    this.isTopLevel = flag;
                }
            }
        }
    }

    internal static class SubqueryRules
    {
        internal static bool IsSupportedTopLevelMethod(MethodInfo mi)
        {
            if (!SubqueryRules.IsSequenceOperatorCall(mi))
                return false;
            string name = mi.Name;
            return name == "Where" || name == "OrderBy" || (name == "OrderByDescending" || name == "ThenBy") || (name == "ThenByDescending" || name == "Take");
        }

        private static bool IsSequenceOperatorCall(MethodInfo mi)
        {
            Type declaringType = mi.DeclaringType;
            return declaringType == typeof(System.Linq.Enumerable) || declaringType == typeof(Queryable);
        }
    }
}
