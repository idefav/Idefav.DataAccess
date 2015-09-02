using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class Funcletizer
    {
        internal static Expression Funcletize(Expression expression)
        {
            return new Funcletizer.Localizer(new Funcletizer.LocalMapper().MapLocals(expression)).Localize(expression);
        }

        private class Localizer : ExpressionVisitor
        {
            private Dictionary<Expression, bool> locals;

            internal Localizer(Dictionary<Expression, bool> locals)
            {
                this.locals = locals;
            }

            internal Expression Localize(Expression expression)
            {
                return this.Visit(expression);
            }

            internal override Expression Visit(Expression exp)
            {
                if (exp == null)
                    return (Expression)null;
                if (this.locals.ContainsKey(exp))
                    return Funcletizer.Localizer.MakeLocal(exp);
                if (exp.NodeType == (ExpressionType)2000)
                    return exp;
                return base.Visit(exp);
            }

            private static Expression MakeLocal(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                    return e;
                if (e.NodeType == ExpressionType.Convert || e.NodeType == ExpressionType.ConvertChecked)
                {
                    UnaryExpression unaryExpression = (UnaryExpression)e;
                    if (unaryExpression.Type == typeof(object))
                    {
                        Expression expression = Funcletizer.Localizer.MakeLocal(unaryExpression.Operand);
                        if (e.NodeType != ExpressionType.Convert)
                            return (Expression)Expression.ConvertChecked(expression, e.Type);
                        return (Expression)Expression.Convert(expression, e.Type);
                    }
                    if (unaryExpression.Operand.NodeType == ExpressionType.Constant && ((ConstantExpression)unaryExpression.Operand).Value == null)
                        return (Expression)Expression.Constant((object)null, unaryExpression.Type);
                }
                return (Expression)Expression.Invoke((Expression)Expression.Constant((object)Expression.Lambda(e).Compile()));
            }
        }

        private class DependenceChecker : ExpressionVisitor
        {
            private HashSet<ParameterExpression> inScope = new HashSet<ParameterExpression>();
            private bool isIndependent = true;

            public static bool IsIndependent(Expression expression)
            {
                Funcletizer.DependenceChecker dependenceChecker = new Funcletizer.DependenceChecker();
                Expression exp = expression;
                dependenceChecker.Visit(exp);
                return dependenceChecker.isIndependent;
            }

            internal override Expression VisitLambda(LambdaExpression lambda)
            {
                foreach (ParameterExpression parameterExpression in lambda.Parameters)
                    this.inScope.Add(parameterExpression);
                return base.VisitLambda(lambda);
            }

            internal override Expression VisitParameter(ParameterExpression p)
            {
                this.isIndependent = this.isIndependent & this.inScope.Contains(p);
                return (Expression)p;
            }
        }

        private class LocalMapper : ExpressionVisitor
        {
            private bool isRemote;
            private Dictionary<Expression, bool> locals;

            internal Dictionary<Expression, bool> MapLocals(Expression expression)
            {
                this.locals = new Dictionary<Expression, bool>();
                this.isRemote = false;
                this.Visit(expression);
                return this.locals;
            }

            internal override Expression Visit(Expression expression)
            {
                if (expression == null)
                    return (Expression)null;
                bool flag = this.isRemote;
                switch (expression.NodeType)
                {
                    case ExpressionType.Constant:
                        if (typeof(ITable).IsAssignableFrom(expression.Type) || typeof(DataContext).IsAssignableFrom(expression.Type))
                            this.isRemote = true;
                        this.isRemote = this.isRemote | flag;
                        return expression;
                    case (ExpressionType)2000:
                        return expression;
                    default:
                        this.isRemote = false;
                        base.Visit(expression);
                        if (!this.isRemote && expression.NodeType != ExpressionType.Lambda && (expression.NodeType != ExpressionType.Quote && Funcletizer.DependenceChecker.IsIndependent(expression)))
                        {
                            this.locals[expression] = true;
                            goto case ExpressionType.Constant;
                        }
                        else
                            goto case ExpressionType.Constant;
                }
            }

            internal override Expression VisitMemberAccess(MemberExpression m)
            {
                base.VisitMemberAccess(m);
                this.isRemote = ((this.isRemote ? 1 : 0) | (m.Expression == null ? 0 : (typeof(ITable).IsAssignableFrom(m.Expression.Type) ? 1 : 0))) != 0;
                return (Expression)m;
            }

            internal override Expression VisitMethodCall(MethodCallExpression m)
            {
                base.VisitMethodCall(m);
                this.isRemote = ((this.isRemote ? 1 : 0) | (m.Method.DeclaringType == typeof(DataManipulation) ? 1 : (Attribute.IsDefined((MemberInfo)m.Method, typeof(FunctionAttribute)) ? 1 : 0))) != 0;
                return (Expression)m;
            }
        }
    }

    internal abstract class ExpressionVisitor
    {
        internal ExpressionVisitor()
        {
        }

        internal virtual Expression Visit(Expression exp)
        {
            if (exp == null)
                return exp;
            switch (exp.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                case ExpressionType.Invoke:
                    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.ListInit:
                    return this.VisitListInit((ListInitExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.UnaryPlus:
                    if (exp.Type == typeof(TimeSpan))
                        return this.VisitUnary((UnaryExpression)exp);
                    throw Error.UnhandledExpressionType((object)exp.NodeType);
                case ExpressionType.New:
                    return (Expression)this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.TypeIs:
                    return this.VisitTypeIs((TypeBinaryExpression)exp);
                default:
                    throw Error.UnhandledExpressionType((object)exp.NodeType);
            }
        }

        internal virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return (MemberBinding)this.VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return (MemberBinding)this.VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return (MemberBinding)this.VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw Error.UnhandledBindingType((object)binding.BindingType);
            }
        }

        internal virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> readOnlyCollection = this.VisitExpressionList(initializer.Arguments);
            if (readOnlyCollection != initializer.Arguments)
                return Expression.ElementInit(initializer.AddMethod, (IEnumerable<Expression>)readOnlyCollection);
            return initializer;
        }

        internal virtual Expression VisitUnary(UnaryExpression u)
        {
            Expression operand = this.Visit(u.Operand);
            if (operand != u.Operand)
                return (Expression)Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            return (Expression)u;
        }

        internal virtual Expression VisitBinary(BinaryExpression b)
        {
            Expression left = this.Visit(b.Left);
            Expression right = this.Visit(b.Right);
            if (left != b.Left || right != b.Right)
                return (Expression)Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
            return (Expression)b;
        }

        internal virtual Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression expression = this.Visit(b.Expression);
            if (expression != b.Expression)
                return (Expression)Expression.TypeIs(expression, b.TypeOperand);
            return (Expression)b;
        }

        internal virtual Expression VisitConstant(ConstantExpression c)
        {
            return (Expression)c;
        }

        internal virtual Expression VisitConditional(ConditionalExpression c)
        {
            Expression test = this.Visit(c.Test);
            Expression ifTrue = this.Visit(c.IfTrue);
            Expression ifFalse = this.Visit(c.IfFalse);
            if (test != c.Test || ifTrue != c.IfTrue || ifFalse != c.IfFalse)
                return (Expression)Expression.Condition(test, ifTrue, ifFalse);
            return (Expression)c;
        }

        internal virtual Expression VisitParameter(ParameterExpression p)
        {
            return (Expression)p;
        }

        internal virtual Expression VisitMemberAccess(MemberExpression m)
        {
            Expression expression = this.Visit(m.Expression);
            if (expression != m.Expression)
                return (Expression)Expression.MakeMemberAccess(expression, m.Member);
            return (Expression)m;
        }

        internal virtual Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression instance = this.Visit(m.Object);
            IEnumerable<Expression> arguments = (IEnumerable<Expression>)this.VisitExpressionList(m.Arguments);
            if (instance != m.Object || arguments != m.Arguments)
                return (Expression)Expression.Call(instance, m.Method, arguments);
            return (Expression)m;
        }

        internal virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = (List<Expression>)null;
            int index1 = 0;
            for (int count = original.Count; index1 < count; ++index1)
            {
                Expression expression = this.Visit(original[index1]);
                if (list != null)
                    list.Add(expression);
                else if (expression != original[index1])
                {
                    list = new List<Expression>(count);
                    for (int index2 = 0; index2 < index1; ++index2)
                        list.Add(original[index2]);
                    list.Add(expression);
                }
            }
            if (list != null)
                return new ReadOnlyCollection<Expression>((IList<Expression>)list);
            return original;
        }

        internal virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression expression = this.Visit(assignment.Expression);
            if (expression != assignment.Expression)
                return Expression.Bind(assignment.Member, expression);
            return assignment;
        }

        internal virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings);
            if (bindings != binding.Bindings)
                return Expression.MemberBind(binding.Member, bindings);
            return binding;
        }

        internal virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers);
            if (initializers != binding.Initializers)
                return Expression.ListBind(binding.Member, initializers);
            return binding;
        }

        internal virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = (List<MemberBinding>)null;
            int index1 = 0;
            for (int count = original.Count; index1 < count; ++index1)
            {
                MemberBinding memberBinding = this.VisitBinding(original[index1]);
                if (list != null)
                    list.Add(memberBinding);
                else if (memberBinding != original[index1])
                {
                    list = new List<MemberBinding>(count);
                    for (int index2 = 0; index2 < index1; ++index2)
                        list.Add(original[index2]);
                    list.Add(memberBinding);
                }
            }
            return (IEnumerable<MemberBinding>)list ?? (IEnumerable<MemberBinding>)original;
        }

        internal virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = (List<ElementInit>)null;
            int index1 = 0;
            for (int count = original.Count; index1 < count; ++index1)
            {
                ElementInit elementInit = this.VisitElementInitializer(original[index1]);
                if (list != null)
                    list.Add(elementInit);
                else if (elementInit != original[index1])
                {
                    list = new List<ElementInit>(count);
                    for (int index2 = 0; index2 < index1; ++index2)
                        list.Add(original[index2]);
                    list.Add(elementInit);
                }
            }
            return (IEnumerable<ElementInit>)list ?? (IEnumerable<ElementInit>)original;
        }

        internal virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = this.Visit(lambda.Body);
            if (body != lambda.Body)
                return (Expression)Expression.Lambda(lambda.Type, body, (IEnumerable<ParameterExpression>)lambda.Parameters);
            return (Expression)lambda;
        }

        internal virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> arguments = (IEnumerable<Expression>)this.VisitExpressionList(nex.Arguments);
            if (arguments == nex.Arguments)
                return nex;
            if (nex.Members != null)
                return Expression.New(nex.Constructor, arguments, (IEnumerable<MemberInfo>)nex.Members);
            return Expression.New(nex.Constructor, arguments);
        }

        internal virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression newExpression = this.VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(init.Bindings);
            if (newExpression != init.NewExpression || bindings != init.Bindings)
                return (Expression)Expression.MemberInit(newExpression, bindings);
            return (Expression)init;
        }

        internal virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression newExpression = this.VisitNew(init.NewExpression);
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(init.Initializers);
            if (newExpression != init.NewExpression || initializers != init.Initializers)
                return (Expression)Expression.ListInit(newExpression, initializers);
            return (Expression)init;
        }

        internal virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> enumerable = (IEnumerable<Expression>)this.VisitExpressionList(na.Expressions);
            if (enumerable == na.Expressions)
                return (Expression)na;
            if (na.NodeType == ExpressionType.NewArrayInit)
                return (Expression)Expression.NewArrayInit(na.Type.GetElementType(), enumerable);
            return (Expression)Expression.NewArrayBounds(na.Type.GetElementType(), enumerable);
        }

        internal virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> arguments = (IEnumerable<Expression>)this.VisitExpressionList(iv.Arguments);
            Expression expression = this.Visit(iv.Expression);
            if (arguments != iv.Arguments || expression != iv.Expression)
                return (Expression)Expression.Invoke(expression, arguments);
            return (Expression)iv;
        }
    }

    internal static class DataManipulation
    {
        public static TResult Insert<TEntity, TResult>(TEntity item, Func<TEntity, TResult> resultSelector)
        {
            throw new NotImplementedException();
        }

        public static int Insert<TEntity>(TEntity item)
        {
            throw new NotImplementedException();
        }

        public static TResult Update<TEntity, TResult>(TEntity item, Func<TEntity, bool> check, Func<TEntity, TResult> resultSelector)
        {
            throw new NotImplementedException();
        }

        public static TResult Update<TEntity, TResult>(TEntity item, Func<TEntity, TResult> resultSelector)
        {
            throw new NotImplementedException();
        }

        public static int Update<TEntity>(TEntity item, Func<TEntity, bool> check)
        {
            throw new NotImplementedException();
        }

        public static int Update<TEntity>(TEntity item)
        {
            throw new NotImplementedException();
        }

        public static int Delete<TEntity>(TEntity item, Func<TEntity, bool> check)
        {
            throw new NotImplementedException();
        }

        public static int Delete<TEntity>(TEntity item)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 将方法与数据库中的存储过程或用户定义的函数相关联。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class FunctionAttribute : Attribute
    {
        private string name;
        private bool isComposable;

        /// <summary>
        /// 获取或设置函数的名称。
        /// </summary>
        /// 
        /// <returns>
        /// 函数或存储过程的名称。
        /// </returns>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示将方法映射到函数还是映射到存储过程。
        /// </summary>
        /// 
        /// <returns>
        /// 如果映射到函数，则为 true；如果映射到存储过程，则为 false。
        /// </returns>
        public bool IsComposable
        {
            get
            {
                return this.isComposable;
            }
            set
            {
                this.isComposable = value;
            }
        }
    }
}
