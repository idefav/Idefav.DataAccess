using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class QueryConverter
    {
        private IDataServices services;
        private Translator translator;
        private SqlFactory sql;
        private TypeSystemProvider typeProvider;
        private bool outerNode;
        private Dictionary<ParameterExpression, SqlExpression> map;
        private Dictionary<ParameterExpression, Expression> exprMap;
        private Dictionary<ParameterExpression, SqlNode> dupMap;
        private Dictionary<SqlNode, QueryConverter.GroupInfo> gmap;
        private Expression dominatingExpression;
        private bool allowDeferred;
        private ConverterStrategy converterStrategy;

        internal ConverterStrategy ConverterStrategy
        {
            get
            {
                return this.converterStrategy;
            }
            set
            {
                this.converterStrategy = value;
            }
        }

        internal QueryConverter(IDataServices services, TypeSystemProvider typeProvider, Translator translator, SqlFactory sql)
        {
            if (services == null)
                throw Error.ArgumentNull("services");
            if (sql == null)
                throw Error.ArgumentNull("sql");
            if (translator == null)
                throw Error.ArgumentNull("translator");
            if (typeProvider == null)
                throw Error.ArgumentNull("typeProvider");
            this.services = services;
            this.translator = translator;
            this.sql = sql;
            this.typeProvider = typeProvider;
            this.map = new Dictionary<ParameterExpression, SqlExpression>();
            this.exprMap = new Dictionary<ParameterExpression, Expression>();
            this.dupMap = new Dictionary<ParameterExpression, SqlNode>();
            this.gmap = new Dictionary<SqlNode, QueryConverter.GroupInfo>();
            this.allowDeferred = true;
        }

        private bool UseConverterStrategy(ConverterStrategy strategy)
        {
            return (this.converterStrategy & strategy) == strategy;
        }

        internal SqlNode ConvertOuter(Expression node)
        {
            this.dominatingExpression = node;
            this.outerNode = true;
            SqlNode child = !typeof(ITable).IsAssignableFrom(node.Type) ? this.VisitInner(node) : (SqlNode)this.VisitSequence(node);
            if (child.NodeType == SqlNodeType.MethodCall)
                throw Error.InvalidMethodExecution((object)((SqlMethodCall)child).Method.Name);
            SqlExpression selection = child as SqlExpression;
            if (selection != null)
                child = (SqlNode)new SqlSelect(selection, (SqlSource)null, this.dominatingExpression);
            return (SqlNode)new SqlIncludeScope(child, this.dominatingExpression);
        }

        internal SqlNode Visit(Expression node)
        {
            bool flag = this.outerNode;
            this.outerNode = false;
            SqlNode sqlNode = this.VisitInner(node);
            this.outerNode = flag;
            return sqlNode;
        }

        internal SqlNode ConvertInner(Expression node, Expression dominantExpression)
        {
            this.dominatingExpression = dominantExpression;
            bool flag = this.outerNode;
            this.outerNode = false;
            SqlNode sqlNode = this.VisitInner(node);
            this.outerNode = flag;
            return sqlNode;
        }

        private SqlNode VisitInner(Expression node)
        {
            if (node == null)
                return (SqlNode)null;
            Expression expression = this.dominatingExpression;
            this.dominatingExpression = QueryConverter.ChooseBestDominatingExpression(this.dominatingExpression, node);
            try
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Coalesce:
                    case ExpressionType.Divide:
                    case ExpressionType.Equal:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Modulo:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.Power:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        return this.VisitBinary((BinaryExpression)node);
                    case ExpressionType.ArrayLength:
                        return this.VisitArrayLength((UnaryExpression)node);
                    case ExpressionType.ArrayIndex:
                        return this.VisitArrayIndex((BinaryExpression)node);
                    case ExpressionType.Call:
                        return this.VisitMethodCall((MethodCallExpression)node);
                    case ExpressionType.Conditional:
                        return (SqlNode)this.VisitConditional((ConditionalExpression)node);
                    case ExpressionType.Constant:
                        return this.VisitConstant((ConstantExpression)node);
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        return this.VisitCast((UnaryExpression)node);
                    case ExpressionType.Invoke:
                        return this.VisitInvocation((InvocationExpression)node);
                    case ExpressionType.Lambda:
                        return this.VisitLambda((LambdaExpression)node);
                    case ExpressionType.LeftShift:
                    case ExpressionType.RightShift:
                        throw Error.UnsupportedNodeType((object)node.NodeType);
                    case ExpressionType.ListInit:
                        return (SqlNode)this.VisitListInit((ListInitExpression)node);
                    case ExpressionType.MemberAccess:
                        return this.VisitMemberAccess((MemberExpression)node);
                    case ExpressionType.MemberInit:
                        return (SqlNode)this.VisitMemberInit((MemberInitExpression)node);
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                        return this.VisitUnary((UnaryExpression)node);
                    case ExpressionType.UnaryPlus:
                        if (node.Type == typeof(TimeSpan))
                            return this.VisitUnary((UnaryExpression)node);
                        throw Error.UnrecognizedExpressionNode((object)node.NodeType);
                    case ExpressionType.New:
                        return (SqlNode)this.VisitNew((NewExpression)node);
                    case ExpressionType.NewArrayInit:
                        return (SqlNode)this.VisitNewArrayInit((NewArrayExpression)node);
                    case ExpressionType.Parameter:
                        return this.VisitParameter((ParameterExpression)node);
                    case ExpressionType.Quote:
                        return this.Visit(((UnaryExpression)node).Operand);
                    case ExpressionType.TypeAs:
                        return this.VisitAs((UnaryExpression)node);
                    case ExpressionType.TypeIs:
                        return this.VisitTypeBinary((TypeBinaryExpression)node);
                    case (ExpressionType)2000:
                        return ((KnownExpression)node).Node;
                    case (ExpressionType)2001:
                        return this.VisitLinkedTable((LinkedTableExpression)node);
                    default:
                        throw Error.UnrecognizedExpressionNode((object)node.NodeType);
                }
            }
            finally
            {
                this.dominatingExpression = expression;
            }
        }

        private static Expression ChooseBestDominatingExpression(Expression last, Expression next)
        {
            if (last == null || next != null && (next is MethodCallExpression || !(last is MethodCallExpression)))
                return next;
            return last;
        }

        private SqlSelect LockSelect(SqlSelect sel)
        {
            if (sel.Selection.NodeType == SqlNodeType.AliasRef && sel.Where == null && (sel.OrderBy.Count <= 0 && sel.GroupBy.Count <= 0) && (sel.Having == null && sel.Top == null && (sel.OrderingType == SqlOrderingType.Default && !sel.IsDistinct)))
                return sel;
            SqlAlias alias = new SqlAlias((SqlNode)sel);
            return new SqlSelect((SqlExpression)new SqlAliasRef(alias), (SqlSource)alias, this.dominatingExpression);
        }

        private SqlSelect VisitSequence(Expression exp)
        {
            return this.CoerceToSequence(this.Visit(exp));
        }

        private SqlSelect CoerceToSequence(SqlNode node)
        {
            SqlSelect sqlSelect = node as SqlSelect;
            if (sqlSelect != null)
                return sqlSelect;
            if (node.NodeType == SqlNodeType.Value)
            {
                SqlValue sqlValue = (SqlValue)node;
                ITable table = sqlValue.Value as ITable;
                if (table != null)
                    return this.CoerceToSequence(this.TranslateConstantTable(table, (SqlLink)null));
                IQueryable queryable = sqlValue.Value as IQueryable;
                if (queryable == null)
                    throw Error.CapturedValuesCannotBeSequences();
                Expression exp = Funcletizer.Funcletize(queryable.Expression);
                if (exp.NodeType != ExpressionType.Constant || ((ConstantExpression)exp).Value != queryable)
                    return this.VisitSequence(exp);
                throw Error.IQueryableCannotReturnSelfReferencingConstantExpression();
            }
            if (node.NodeType == SqlNodeType.Multiset || node.NodeType == SqlNodeType.Element)
                return ((SqlSubSelect)node).Select;
            if (node.NodeType == SqlNodeType.ClientArray)
                throw Error.ConstructedArraysNotSupported();
            if (node.NodeType == SqlNodeType.ClientParameter)
                throw Error.ParametersCannotBeSequences();
            SqlAlias alias = new SqlAlias(node);
            return new SqlSelect((SqlExpression)new SqlAliasRef(alias), (SqlSource)alias, this.dominatingExpression);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private SqlNode VisitInvocation(InvocationExpression invoke)
        {
            LambdaExpression lambdaExpression = invoke.Expression.NodeType == ExpressionType.Quote ? (LambdaExpression)((UnaryExpression)invoke.Expression).Operand : invoke.Expression as LambdaExpression;
            if (lambdaExpression != null)
            {
                int index = 0;
                for (int count = invoke.Arguments.Count; index < count; ++index)
                    this.exprMap[lambdaExpression.Parameters[index]] = invoke.Arguments[index];
                return this.VisitInner(lambdaExpression.Body);
            }
            SqlExpression sqlExpression1 = this.VisitExpression(invoke.Expression);
            if (sqlExpression1.NodeType == SqlNodeType.Value)
            {
                Delegate @delegate = ((SqlValue)sqlExpression1).Value as Delegate;
                if (@delegate != null)
                {
                    CompiledQuery compiledQuery = @delegate.Target as CompiledQuery;
                    if (compiledQuery != null)
                        return this.VisitInvocation(Expression.Invoke((Expression)compiledQuery.Expression, (IEnumerable<Expression>)invoke.Arguments));
                    if (invoke.Arguments.Count == 0)
                    {
                        object obj;
                        try
                        {
                            obj = @delegate.DynamicInvoke((object[])null);
                        }
                        catch (TargetInvocationException ex)
                        {
                            throw ex.InnerException;
                        }
                        return (SqlNode)this.sql.ValueFromObject(obj, invoke.Type, true, this.dominatingExpression);
                    }
                }
            }
            SqlExpression[] exprs = new SqlExpression[invoke.Arguments.Count];
            for (int index = 0; index < exprs.Length; ++index)
                exprs[index] = (SqlExpression)this.Visit(invoke.Arguments[index]);
            SqlClientArray sqlClientArray1 = new SqlClientArray(typeof(object[]), this.typeProvider.From(typeof(object[])), exprs, this.dominatingExpression);
            SqlFactory sqlFactory = this.sql;
            Type type = invoke.Type;
            MethodInfo method = typeof(Delegate).GetMethod("DynamicInvoke");
            SqlExpression sqlExpression2 = sqlExpression1;
            SqlExpression[] args = new SqlExpression[1];
            int index1 = 0;
            SqlClientArray sqlClientArray2 = sqlClientArray1;
            args[index1] = (SqlExpression)sqlClientArray2;
            Expression sourceExpression = this.dominatingExpression;
            return (SqlNode)sqlFactory.MethodCall(type, method, sqlExpression2, args, sourceExpression);
        }

        private SqlNode VisitLambda(LambdaExpression lambda)
        {
            int index1 = 0;
            for (int count = lambda.Parameters.Count; index1 < count; ++index1)
            {
                ParameterExpression index2 = lambda.Parameters[index1];
                if (index2.Type == typeof(Type))
                    throw Error.BadParameterType((object)index2.Type);
                ParameterExpression parameterExpression1 = Expression.Parameter(typeof(object[]), "args");
                Type type1 = typeof(Func<,>);
                Type[] typeArray = new Type[2];
                int index3 = 0;
                Type type2 = typeof(object[]);
                typeArray[index3] = type2;
                int index4 = 1;
                Type type3 = index2.Type;
                typeArray[index4] = type3;
                Type delegateType = type1.MakeGenericType(typeArray);
                UnaryExpression unaryExpression = Expression.Convert((Expression)Expression.ArrayIndex((Expression)parameterExpression1, (Expression)Expression.Constant((object)index1)), index2.Type);
                ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                int index5 = 0;
                ParameterExpression parameterExpression2 = parameterExpression1;
                parameterExpressionArray[index5] = parameterExpression2;
                LambdaExpression accessor = Expression.Lambda(delegateType, (Expression)unaryExpression, parameterExpressionArray);
                SqlClientParameter sqlClientParameter = new SqlClientParameter(index2.Type, this.typeProvider.From(index2.Type), accessor, this.dominatingExpression);
                this.dupMap[index2] = (SqlNode)sqlClientParameter;
            }
            return this.VisitInner(lambda.Body);
        }

        private SqlExpression VisitExpression(Expression exp)
        {
            SqlNode sqlNode = this.Visit(exp);
            if (sqlNode == null)
                return (SqlExpression)null;
            SqlExpression sqlExpression = sqlNode as SqlExpression;
            if (sqlExpression != null)
                return sqlExpression;
            SqlSelect select = sqlNode as SqlSelect;
            if (select != null)
                return (SqlExpression)this.sql.SubSelect(SqlNodeType.Multiset, select, exp.Type);
            throw Error.UnrecognizedExpressionNode((object)sqlNode);
        }

        private SqlSelect VisitSelect(Expression sequence, LambdaExpression selector)
        {
            SqlAlias alias1 = new SqlAlias((SqlNode)this.VisitSequence(sequence));
            SqlAliasRef sqlAliasRef1 = new SqlAliasRef(alias1);
            this.map[selector.Parameters[0]] = (SqlExpression)sqlAliasRef1;
            SqlNode sqlNode = this.Visit(selector.Body);
            SqlSelect select1 = sqlNode as SqlSelect;
            if (select1 != null)
                return new SqlSelect((SqlExpression)this.sql.SubSelect(SqlNodeType.Multiset, select1, selector.Body.Type), (SqlSource)alias1, this.dominatingExpression);
            if ((sqlNode.NodeType == SqlNodeType.Element || sqlNode.NodeType == SqlNodeType.ScalarSubSelect) && (this.converterStrategy & ConverterStrategy.CanUseOuterApply) != ConverterStrategy.Default)
            {
                SqlSelect select2 = ((SqlSubSelect)sqlNode).Select;
                SqlAlias alias2 = new SqlAlias((SqlNode)select2);
                SqlAliasRef sqlAliasRef2 = new SqlAliasRef(alias2);
                select2.Selection = sqlNode.NodeType != SqlNodeType.Element ? (SqlExpression)this.sql.Unary(SqlNodeType.OuterJoinedValue, select2.Selection) : (SqlExpression)new SqlOptionalValue((SqlExpression)new SqlColumn("test", (SqlExpression)this.sql.Unary(SqlNodeType.OuterJoinedValue, this.sql.Value(typeof(int?), this.typeProvider.From(typeof(int)), (object)1, false, this.dominatingExpression))), (SqlExpression)this.sql.Unary(SqlNodeType.OuterJoinedValue, select2.Selection));
                SqlJoin sqlJoin = new SqlJoin(SqlJoinType.OuterApply, (SqlSource)alias1, (SqlSource)alias2, (SqlExpression)null, this.dominatingExpression);
                Expression sourceExpression = this.dominatingExpression;
                return new SqlSelect((SqlExpression)sqlAliasRef2, (SqlSource)sqlJoin, sourceExpression);
            }
            SqlExpression selection = sqlNode as SqlExpression;
            if (selection != null)
                return new SqlSelect(selection, (SqlSource)alias1, this.dominatingExpression);
            throw Error.BadProjectionInSelect();
        }

        private SqlSelect VisitSelectMany(Expression sequence, LambdaExpression colSelector, LambdaExpression resultSelector)
        {
            SqlAlias alias1 = new SqlAlias((SqlNode)this.VisitSequence(sequence));
            SqlAliasRef sqlAliasRef1 = new SqlAliasRef(alias1);
            this.map[colSelector.Parameters[0]] = (SqlExpression)sqlAliasRef1;
            SqlAlias alias2 = new SqlAlias((SqlNode)this.VisitSequence(colSelector.Body));
            SqlAliasRef sqlAliasRef2 = new SqlAliasRef(alias2);
            SqlJoin sqlJoin = new SqlJoin(SqlJoinType.CrossApply, (SqlSource)alias1, (SqlSource)alias2, (SqlExpression)null, this.dominatingExpression);
            SqlExpression selection = (SqlExpression)sqlAliasRef2;
            if (resultSelector != null)
            {
                this.map[resultSelector.Parameters[0]] = (SqlExpression)sqlAliasRef1;
                this.map[resultSelector.Parameters[1]] = (SqlExpression)sqlAliasRef2;
                selection = this.VisitExpression(resultSelector.Body);
            }
            return new SqlSelect(selection, (SqlSource)sqlJoin, this.dominatingExpression);
        }

        private SqlSelect VisitJoin(Expression outerSequence, Expression innerSequence, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            SqlSelect sqlSelect1 = this.VisitSequence(outerSequence);
            SqlSelect sqlSelect2 = this.VisitSequence(innerSequence);
            SqlAlias alias1 = new SqlAlias((SqlNode)sqlSelect1);
            SqlAliasRef sqlAliasRef1 = new SqlAliasRef(alias1);
            SqlAlias alias2 = new SqlAlias((SqlNode)sqlSelect2);
            SqlAliasRef sqlAliasRef2 = new SqlAliasRef(alias2);
            this.map[outerKeySelector.Parameters[0]] = (SqlExpression)sqlAliasRef1;
            SqlExpression left = this.VisitExpression(outerKeySelector.Body);
            this.map[innerKeySelector.Parameters[0]] = (SqlExpression)sqlAliasRef2;
            SqlExpression right = this.VisitExpression(innerKeySelector.Body);
            this.map[resultSelector.Parameters[0]] = (SqlExpression)sqlAliasRef1;
            this.map[resultSelector.Parameters[1]] = (SqlExpression)sqlAliasRef2;
            SqlExpression selection = this.VisitExpression(resultSelector.Body);
            SqlExpression cond = (SqlExpression)this.sql.Binary(SqlNodeType.EQ, left, right);
            SqlSelect sqlSelect3;
            if ((this.converterStrategy & ConverterStrategy.CanUseJoinOn) != ConverterStrategy.Default)
            {
                SqlJoin sqlJoin = new SqlJoin(SqlJoinType.Inner, (SqlSource)alias1, (SqlSource)alias2, cond, this.dominatingExpression);
                sqlSelect3 = new SqlSelect(selection, (SqlSource)sqlJoin, this.dominatingExpression);
            }
            else
            {
                SqlJoin sqlJoin = new SqlJoin(SqlJoinType.Cross, (SqlSource)alias1, (SqlSource)alias2, (SqlExpression)null, this.dominatingExpression);
                sqlSelect3 = new SqlSelect(selection, (SqlSource)sqlJoin, this.dominatingExpression);
                sqlSelect3.Where = cond;
            }
            return sqlSelect3;
        }

        private SqlSelect VisitGroupJoin(Expression outerSequence, Expression innerSequence, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector, LambdaExpression resultSelector)
        {
            SqlSelect sqlSelect1 = this.VisitSequence(outerSequence);
            SqlSelect sqlSelect2 = this.VisitSequence(innerSequence);
            SqlAlias alias1 = new SqlAlias((SqlNode)sqlSelect1);
            SqlAliasRef sqlAliasRef1 = new SqlAliasRef(alias1);
            SqlAlias alias2 = new SqlAlias((SqlNode)sqlSelect2);
            SqlAliasRef sqlAliasRef2 = new SqlAliasRef(alias2);
            this.map[outerKeySelector.Parameters[0]] = (SqlExpression)sqlAliasRef1;
            SqlExpression left = this.VisitExpression(outerKeySelector.Body);
            this.map[innerKeySelector.Parameters[0]] = (SqlExpression)sqlAliasRef2;
            SqlExpression right = this.VisitExpression(innerKeySelector.Body);
            SqlExpression sqlExpression = (SqlExpression)this.sql.Binary(SqlNodeType.EQ, left, right);
            SqlSubSelect sqlSubSelect = this.sql.SubSelect(SqlNodeType.Multiset, new SqlSelect((SqlExpression)sqlAliasRef2, (SqlSource)alias2, this.dominatingExpression)
            {
                Where = sqlExpression
            });
            this.map[resultSelector.Parameters[0]] = (SqlExpression)sqlAliasRef1;
            this.dupMap[resultSelector.Parameters[1]] = (SqlNode)sqlSubSelect;
            return new SqlSelect(this.VisitExpression(resultSelector.Body), (SqlSource)alias1, this.dominatingExpression);
        }

        private SqlSelect VisitDefaultIfEmpty(Expression sequence)
        {
            SqlAlias alias1 = new SqlAlias((SqlNode)this.VisitSequence(sequence));
            SqlAlias alias2 = new SqlAlias((SqlNode)new SqlSelect((SqlExpression)new SqlOptionalValue((SqlExpression)new SqlColumn("test", (SqlExpression)this.sql.Unary(SqlNodeType.OuterJoinedValue, this.sql.Value(typeof(int?), this.typeProvider.From(typeof(int)), (object)1, false, this.dominatingExpression))), (SqlExpression)this.sql.Unary(SqlNodeType.OuterJoinedValue, (SqlExpression)new SqlAliasRef(alias1))), (SqlSource)alias1, this.dominatingExpression));
            return new SqlSelect((SqlExpression)new SqlAliasRef(alias2), (SqlSource)new SqlJoin(SqlJoinType.OuterApply, (SqlSource)new SqlAlias((SqlNode)new SqlSelect(this.sql.TypedLiteralNull(typeof(string), this.dominatingExpression), (SqlSource)null, this.dominatingExpression)), (SqlSource)alias2, (SqlExpression)null, this.dominatingExpression), this.dominatingExpression);
        }

        private SqlSelect VisitOfType(Expression sequence, Type ofType)
        {
            SqlSelect sel = this.LockSelect(this.VisitSequence(sequence));
            SqlAliasRef sqlAliasRef1 = (SqlAliasRef)sel.Selection;
            sel.Selection = (SqlExpression)new SqlUnary(SqlNodeType.Treat, ofType, this.typeProvider.From(ofType), (SqlExpression)sqlAliasRef1, this.dominatingExpression);
            SqlSelect sqlSelect = this.LockSelect(sel);
            SqlAliasRef sqlAliasRef2 = (SqlAliasRef)sqlSelect.Selection;
            sqlSelect.Where = this.sql.AndAccumulate(sqlSelect.Where, (SqlExpression)this.sql.Unary(SqlNodeType.IsNotNull, (SqlExpression)sqlAliasRef2, this.dominatingExpression));
            return sqlSelect;
        }

        private SqlNode VisitSequenceCast(Expression sequence, Type type)
        {
            Type elementType = TypeSystem.GetElementType(sequence.Type);
            ParameterExpression parameterExpression1 = Expression.Parameter(elementType, "pc");
            Type type1 = typeof(System.Linq.Enumerable);
            string methodName = "Select";
            Type[] typeArguments = new Type[2];
            int index1 = 0;
            Type type2 = elementType;
            typeArguments[index1] = type2;
            int index2 = 1;
            Type type3 = type;
            typeArguments[index2] = type3;
            Expression[] expressionArray = new Expression[2];
            int index3 = 0;
            Expression expression = sequence;
            expressionArray[index3] = expression;
            int index4 = 1;
            UnaryExpression unaryExpression = Expression.Convert((Expression)parameterExpression1, type);
            ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
            int index5 = 0;
            ParameterExpression parameterExpression2 = parameterExpression1;
            parameterExpressionArray[index5] = parameterExpression2;
            LambdaExpression lambdaExpression = Expression.Lambda((Expression)unaryExpression, parameterExpressionArray);
            expressionArray[index4] = (Expression)lambdaExpression;
            return this.Visit((Expression)Expression.Call(type1, methodName, typeArguments, expressionArray));
        }

        private SqlNode VisitTypeBinary(TypeBinaryExpression b)
        {
            SqlExpression expr = this.VisitExpression(b.Expression);
            if (b.NodeType != ExpressionType.TypeIs)
                throw Error.TypeBinaryOperatorNotRecognized();
            Type typeOperand = b.TypeOperand;
            return (SqlNode)this.sql.Unary(SqlNodeType.IsNotNull, (SqlExpression)new SqlUnary(SqlNodeType.Treat, typeOperand, this.typeProvider.From(typeOperand), expr, this.dominatingExpression), this.dominatingExpression);
        }

        private SqlSelect VisitWhere(Expression sequence, LambdaExpression predicate)
        {
            SqlSelect sqlSelect = this.LockSelect(this.VisitSequence(sequence));
            this.map[predicate.Parameters[0]] = sqlSelect.Selection;
            sqlSelect.Where = this.VisitExpression(predicate.Body);
            return sqlSelect;
        }

        private SqlNode VisitAs(UnaryExpression a)
        {
            SqlNode sqlNode = this.Visit(a.Operand);
            SqlExpression expr = sqlNode as SqlExpression;
            if (expr != null)
                return (SqlNode)new SqlUnary(SqlNodeType.Treat, a.Type, this.typeProvider.From(a.Type), expr, (Expression)a);
            SqlSelect select = sqlNode as SqlSelect;
            if (select == null)
                throw Error.DidNotExpectAs((object)a);
            SqlSubSelect sqlSubSelect = this.sql.SubSelect(SqlNodeType.Multiset, select);
            return (SqlNode)new SqlUnary(SqlNodeType.Treat, a.Type, this.typeProvider.From(a.Type), (SqlExpression)sqlSubSelect, (Expression)a);
        }

        private SqlNode VisitArrayLength(UnaryExpression c)
        {
            SqlExpression expr = this.VisitExpression(c.Operand);
            if (expr.SqlType.IsString || expr.SqlType.IsChar)
                return (SqlNode)this.sql.CLRLENGTH(expr);
            return (SqlNode)this.sql.DATALENGTH(expr);
        }

        private SqlNode VisitArrayIndex(BinaryExpression b)
        {
            SqlExpression sqlExpression1 = this.VisitExpression(b.Left);
            SqlExpression sqlExpression2 = this.VisitExpression(b.Right);
            if (sqlExpression1.NodeType != SqlNodeType.ClientParameter || sqlExpression2.NodeType != SqlNodeType.Value)
                throw Error.UnrecognizedExpressionNode((object)b.NodeType);
            SqlClientParameter sqlClientParameter = (SqlClientParameter)sqlExpression1;
            SqlValue sqlValue = (SqlValue)sqlExpression2;
            return (SqlNode)new SqlClientParameter(b.Type, this.sql.TypeProvider.From(b.Type), Expression.Lambda((Expression)Expression.ArrayIndex(sqlClientParameter.Accessor.Body, (Expression)Expression.Constant(sqlValue.Value, sqlValue.ClrType)), System.Linq.Enumerable.ToArray<ParameterExpression>((IEnumerable<ParameterExpression>)sqlClientParameter.Accessor.Parameters)), this.dominatingExpression);
        }

        private SqlNode VisitCast(UnaryExpression c)
        {
            if (!(c.Method != (MethodInfo)null))
                return this.VisitChangeType(c.Operand, c.Type);
            SqlExpression sqlExpression1 = this.VisitExpression(c.Operand);
            SqlFactory sqlFactory = this.sql;
            Type type = c.Type;
            MethodInfo method = c.Method;
            // ISSUE: variable of the null type
            
            SqlExpression[] args = new SqlExpression[1];
            int index = 0;
            SqlExpression sqlExpression2 = sqlExpression1;
            args[index] = sqlExpression2;
            Expression sourceExpression = this.dominatingExpression;
            return (SqlNode)sqlFactory.MethodCall(type, method, (SqlExpression)null, args, sourceExpression);
        }

        private SqlNode VisitChangeType(Expression expression, Type type)
        {
            return this.ChangeType(this.VisitExpression(expression), type);
        }

        private SqlNode ConvertDateToDateTime2(SqlExpression expr)
        {
            SqlExpression sqlExpression1 = (SqlExpression)new SqlVariable(expr.ClrType, expr.SqlType, "DATETIME2", expr.SourceExpression);
            SqlFactory sqlFactory = this.sql;
            Type clrType = typeof(DateTime);
            string name = "CONVERT";
            SqlExpression[] sqlExpressionArray = new SqlExpression[2];
            int index1 = 0;
            SqlExpression sqlExpression2 = sqlExpression1;
            sqlExpressionArray[index1] = sqlExpression2;
            int index2 = 1;
            SqlExpression sqlExpression3 = expr;
            sqlExpressionArray[index2] = sqlExpression3;
            Expression sourceExpression = expr.SourceExpression;
            return (SqlNode)sqlFactory.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, sourceExpression);
        }

        private SqlNode ChangeType(SqlExpression expr, Type type)
        {
            if (type == typeof(object))
                return (SqlNode)expr;
            if (expr.NodeType == SqlNodeType.Value && ((SqlValue)expr).Value == null)
                return (SqlNode)this.sql.TypedLiteralNull(type, expr.SourceExpression);
            if (expr.NodeType == SqlNodeType.ClientParameter)
            {
                SqlClientParameter sqlClientParameter = (SqlClientParameter)expr;
                return (SqlNode)new SqlClientParameter(type, this.sql.TypeProvider.From(type), Expression.Lambda((Expression)Expression.Convert(sqlClientParameter.Accessor.Body, type), System.Linq.Enumerable.ToArray<ParameterExpression>((IEnumerable<ParameterExpression>)sqlClientParameter.Accessor.Parameters)), sqlClientParameter.SourceExpression);
            }
            QueryConverter.ConversionMethod conversionMethod = this.ChooseConversionMethod(expr.ClrType, type);
            switch (conversionMethod)
            {
                case QueryConverter.ConversionMethod.Treat:
                    int num = 85;
                    Type clrType = type;
                    ProviderType sqlType = this.typeProvider.From(type);
                    SqlExpression expr1 = expr;
                    Expression sourceExpression1 = expr1.SourceExpression;
                    return (SqlNode)new SqlUnary((SqlNodeType)num, clrType, sqlType, expr1, sourceExpression1);
                case QueryConverter.ConversionMethod.Ignore:
                    if (SqlFactory.IsSqlDateType(expr))
                        return this.ConvertDateToDateTime2(expr);
                    return (SqlNode)expr;
                case QueryConverter.ConversionMethod.Convert:
                    SqlFactory sqlFactory = this.sql;
                    Type targetClrType = type;
                    ProviderType targetSqlType = this.typeProvider.From(type);
                    SqlExpression expression = expr;
                    Expression sourceExpression2 = expression.SourceExpression;
                    return (SqlNode)sqlFactory.UnaryConvert(targetClrType, targetSqlType, expression, sourceExpression2);
                case QueryConverter.ConversionMethod.Lift:
                    if (SqlFactory.IsSqlDateType(expr))
                        expr = (SqlExpression)this.ConvertDateToDateTime2(expr);
                    return (SqlNode)new SqlLift(type, expr, this.dominatingExpression);
                default:
                    throw Error.UnhandledExpressionType((object)conversionMethod);
            }
        }

        private QueryConverter.ConversionMethod ChooseConversionMethod(Type fromType, Type toType)
        {
            Type nonNullableType1 = TypeSystem.GetNonNullableType(fromType);
            Type nonNullableType2 = TypeSystem.GetNonNullableType(toType);
            if (fromType != toType && nonNullableType1 == nonNullableType2)
                return QueryConverter.ConversionMethod.Lift;
            if (TypeSystem.IsSequenceType(nonNullableType1) || TypeSystem.IsSequenceType(nonNullableType2))
                return QueryConverter.ConversionMethod.Ignore;
            ProviderType providerType1 = this.typeProvider.From(nonNullableType1);
            ProviderType providerType2 = this.typeProvider.From(nonNullableType2);
            if (providerType1.IsRuntimeOnlyType | providerType2.IsRuntimeOnlyType)
                return QueryConverter.ConversionMethod.Treat;
            return nonNullableType1 == nonNullableType2 || providerType1.IsString && providerType1.Equals((object)providerType2) || (nonNullableType1.IsEnum || nonNullableType2.IsEnum) ? QueryConverter.ConversionMethod.Ignore : QueryConverter.ConversionMethod.Convert;
        }

        private SqlNode TranslateConstantTable(ITable table, SqlLink link)
        {
            if (table.Context != this.services.Context)
                throw Error.WrongDataContext();
            return (SqlNode)this.translator.BuildDefaultQuery(this.services.Model.GetTable(table.ElementType).RowType, this.allowDeferred, link, this.dominatingExpression);
        }

        private SqlNode VisitLinkedTable(LinkedTableExpression linkedTable)
        {
            return this.TranslateConstantTable(linkedTable.Table, linkedTable.Link);
        }

        private SqlNode VisitConstant(ConstantExpression cons)
        {
            Type type = cons.Type;
            if (cons.Value == null)
                return (SqlNode)this.sql.TypedLiteralNull(type, this.dominatingExpression);
            if (type == typeof(object))
                type = cons.Value.GetType();
            return (SqlNode)this.sql.ValueFromObject(cons.Value, type, true, this.dominatingExpression);
        }

        private SqlExpression VisitConditional(ConditionalExpression cond)
        {
            List<SqlWhen> list = new List<SqlWhen>(1);
            list.Add(new SqlWhen(this.VisitExpression(cond.Test), this.VisitExpression(cond.IfTrue)));
            SqlExpression @else;
            SqlSearchedCase sqlSearchedCase;
            for (@else = this.VisitExpression(cond.IfFalse); @else.NodeType == SqlNodeType.SearchedCase; @else = sqlSearchedCase.Else)
            {
                sqlSearchedCase = (SqlSearchedCase)@else;
                list.AddRange((IEnumerable<SqlWhen>)sqlSearchedCase.Whens);
            }
            return (SqlExpression)this.sql.SearchedCase(list.ToArray(), @else, this.dominatingExpression);
        }

        private SqlExpression VisitNew(NewExpression qn)
        {
            if (TypeSystem.IsNullableType(qn.Type) && qn.Arguments.Count == 1 && TypeSystem.GetNonNullableType(qn.Type) == qn.Arguments[0].Type)
                return this.VisitCast(Expression.Convert(qn.Arguments[0], qn.Type)) as SqlExpression;
            if (qn.Type == typeof(Decimal) && qn.Arguments.Count == 1)
                return this.VisitCast(Expression.Convert(qn.Arguments[0], typeof(Decimal))) as SqlExpression;
            MetaType metaType = this.services.Model.GetMetaType(qn.Type);
            if (metaType.IsEntity)
                throw Error.CannotMaterializeEntityType((object)qn.Type);
            SqlExpression[] sqlExpressionArray = (SqlExpression[])null;
            if (qn.Arguments.Count > 0)
            {
                sqlExpressionArray = new SqlExpression[qn.Arguments.Count];
                int index = 0;
                for (int count = qn.Arguments.Count; index < count; ++index)
                    sqlExpressionArray[index] = this.VisitExpression(qn.Arguments[index]);
            }
            return (SqlExpression)this.sql.New(metaType, qn.Constructor, (IEnumerable<SqlExpression>)sqlExpressionArray, QueryConverter.PropertyOrFieldOf((IEnumerable<MemberInfo>)qn.Members), (IEnumerable<SqlMemberAssign>)null, this.dominatingExpression);
        }

        private SqlExpression VisitMemberInit(MemberInitExpression init)
        {
            MetaType metaType = this.services.Model.GetMetaType(init.Type);
            if (metaType.IsEntity)
                throw Error.CannotMaterializeEntityType((object)init.Type);
            SqlExpression[] sqlExpressionArray = (SqlExpression[])null;
            NewExpression newExpression = init.NewExpression;
            if (newExpression.Type == typeof(Decimal) && newExpression.Arguments.Count == 1)
                return this.VisitCast(Expression.Convert(newExpression.Arguments[0], typeof(Decimal))) as SqlExpression;
            if (newExpression.Arguments.Count > 0)
            {
                sqlExpressionArray = new SqlExpression[newExpression.Arguments.Count];
                int index = 0;
                for (int length = sqlExpressionArray.Length; index < length; ++index)
                    sqlExpressionArray[index] = this.VisitExpression(newExpression.Arguments[index]);
            }
            int count = init.Bindings.Count;
            SqlMemberAssign[] items = new SqlMemberAssign[count];
            int[] keys = new int[items.Length];
            for (int index = 0; index < count; ++index)
            {
                MemberAssignment memberAssignment = init.Bindings[index] as MemberAssignment;
                if (memberAssignment == null)
                    throw Error.UnhandledBindingType((object)init.Bindings[index].BindingType);
                SqlExpression expr = this.VisitExpression(memberAssignment.Expression);
                SqlMemberAssign sqlMemberAssign = new SqlMemberAssign(memberAssignment.Member, expr);
                items[index] = sqlMemberAssign;
                keys[index] = metaType.GetDataMember(memberAssignment.Member).Ordinal;
            }
            Array.Sort<int, SqlMemberAssign>(keys, items, 0, items.Length);
            return (SqlExpression)this.sql.New(metaType, newExpression.Constructor, (IEnumerable<SqlExpression>)sqlExpressionArray, QueryConverter.PropertyOrFieldOf((IEnumerable<MemberInfo>)newExpression.Members), (IEnumerable<SqlMemberAssign>)items, this.dominatingExpression);
        }

        private static IEnumerable<MemberInfo> PropertyOrFieldOf(IEnumerable<MemberInfo> members)
        {
            if (members == null)
                return (IEnumerable<MemberInfo>)null;
            List<MemberInfo> list = new List<MemberInfo>();
            foreach (MemberInfo memberInfo in members)
            {
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Field:
                    case MemberTypes.Property:
                        list.Add(memberInfo);
                        continue;
                    case MemberTypes.Method:
                        foreach (PropertyInfo propertyInfo in memberInfo.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            MethodInfo methodInfo = memberInfo as MethodInfo;
                            if (propertyInfo.CanRead && propertyInfo.GetGetMethod() == methodInfo)
                            {
                                list.Add((MemberInfo)propertyInfo);
                                break;
                            }
                        }
                        continue;
                    default:
                        throw Error.CouldNotConvertToPropertyOrField((object)memberInfo);
                }
            }
            return (IEnumerable<MemberInfo>)list;
        }

        private SqlSelect VisitDistinct(Expression sequence)
        {
            SqlSelect sqlSelect = this.LockSelect(this.VisitSequence(sequence));
            int num1 = 1;
            sqlSelect.IsDistinct = num1 != 0;
            int num2 = 2;
            sqlSelect.OrderingType = (SqlOrderingType)num2;
            return sqlSelect;
        }

        private SqlSelect VisitTake(Expression sequence, Expression count)
        {
            SqlExpression takeExp = this.VisitExpression(count);
            if (takeExp.NodeType == SqlNodeType.Value)
            {
                SqlValue sqlValue = (SqlValue)takeExp;
                if (typeof(int).IsAssignableFrom(sqlValue.Value.GetType()) && (int)sqlValue.Value < 0)
                    throw Error.ArgumentOutOfRange("takeCount");
            }
            MethodCallExpression mc = sequence as MethodCallExpression;
            if (mc == null || !this.IsSequenceOperatorCall(mc) || (!(mc.Method.Name == "Skip") || mc.Arguments.Count != 2))
                return this.GenerateSkipTake(this.VisitSequence(sequence), (SqlExpression)null, takeExp);
            SqlExpression skipExp = this.VisitExpression(mc.Arguments[1]);
            if (skipExp.NodeType == SqlNodeType.Value)
            {
                SqlValue sqlValue = (SqlValue)skipExp;
                if (typeof(int).IsAssignableFrom(sqlValue.Value.GetType()) && (int)sqlValue.Value < 0)
                    throw Error.ArgumentOutOfRange("skipCount");
            }
            return this.GenerateSkipTake(this.VisitSequence(mc.Arguments[0]), skipExp, takeExp);
        }

        private bool CanSkipOnSelection(SqlExpression selection)
        {
            if (this.IsGrouping(selection.ClrType) || this.services.Model.GetTable(selection.ClrType) != null)
                return true;
            if (TypeSystem.IsSequenceType(selection.ClrType) && !selection.SqlType.CanBeColumn)
                return false;
            switch (selection.NodeType)
            {
                case SqlNodeType.AliasRef:
                    SqlNode node = ((SqlAliasRef)selection).Alias.Node;
                    SqlSelect sqlSelect1 = node as SqlSelect;
                    if (sqlSelect1 != null)
                        return this.CanSkipOnSelection(sqlSelect1.Selection);
                    SqlUnion sqlUnion = node as SqlUnion;
                    if (sqlUnion == null)
                        return this.CanSkipOnSelection((SqlExpression)node);
                    bool flag1 = false;
                    bool flag2 = false;
                    SqlSelect sqlSelect2 = sqlUnion.Left as SqlSelect;
                    if (sqlSelect2 != null)
                        flag1 = this.CanSkipOnSelection(sqlSelect2.Selection);
                    SqlSelect sqlSelect3 = sqlUnion.Right as SqlSelect;
                    if (sqlSelect3 != null)
                        flag2 = this.CanSkipOnSelection(sqlSelect3.Selection);
                    return flag1 & flag2;
                case SqlNodeType.New:
                    SqlNew sqlNew = (SqlNew)selection;
                    foreach (SqlMemberAssign sqlMemberAssign in sqlNew.Members)
                    {
                        if (!this.CanSkipOnSelection(sqlMemberAssign.Expression))
                            return false;
                    }
                    if (sqlNew.ArgMembers != null)
                    {
                        int index = 0;
                        for (int count = sqlNew.ArgMembers.Count; index < count; ++index)
                        {
                            if (!this.CanSkipOnSelection(sqlNew.Args[index]))
                                return false;
                        }
                        break;
                    }
                    break;
            }
            return true;
        }

        private SqlSelect VisitSkip(Expression sequence, Expression skipCount)
        {
            SqlExpression skipExp = this.VisitExpression(skipCount);
            if (skipExp.NodeType == SqlNodeType.Value)
            {
                SqlValue sqlValue = (SqlValue)skipExp;
                if (typeof(int).IsAssignableFrom(sqlValue.Value.GetType()) && (int)sqlValue.Value < 0)
                    throw Error.ArgumentOutOfRange("skipCount");
            }
            return this.GenerateSkipTake(this.VisitSequence(sequence), skipExp, (SqlExpression)null);
        }

        private SqlSelect GenerateSkipTake(SqlSelect sequence, SqlExpression skipExp, SqlExpression takeExp)
        {
            SqlSelect sqlSelect1 = this.LockSelect(sequence);
            if (skipExp == null)
            {
                if (takeExp != null)
                    sqlSelect1.Top = takeExp;
                return sqlSelect1;
            }
            SqlAlias alias1 = new SqlAlias((SqlNode)sqlSelect1);
            SqlAliasRef sqlAliasRef1 = new SqlAliasRef(alias1);
            if (this.UseConverterStrategy(ConverterStrategy.SkipWithRowNumber))
            {
                SqlColumn col = new SqlColumn("ROW_NUMBER", (SqlExpression)this.sql.RowNumber(new List<SqlOrderExpression>(), this.dominatingExpression));
                SqlColumnRef sqlColumnRef = new SqlColumnRef(col);
                sqlSelect1.Row.Columns.Add(col);
                return new SqlSelect((SqlExpression)sqlAliasRef1, (SqlSource)alias1, this.dominatingExpression)
                {
                    Where = takeExp == null ? (SqlExpression)this.sql.Binary(SqlNodeType.GT, (SqlExpression)sqlColumnRef, skipExp) : (SqlExpression)this.sql.Between((SqlExpression)sqlColumnRef, this.sql.Add(skipExp, 1), (SqlExpression)this.sql.Binary(SqlNodeType.Add, (SqlExpression)SqlDuplicator.Copy((SqlNode)skipExp), takeExp), this.dominatingExpression)
                };
            }
            if (!this.CanSkipOnSelection(sqlSelect1.Selection))
                throw Error.SkipNotSupportedForSequenceTypes();
            SingleTableQueryVisitor tableQueryVisitor = new SingleTableQueryVisitor();
            SqlSelect sqlSelect2 = sqlSelect1;
            tableQueryVisitor.Visit((SqlNode)sqlSelect2);
            if (!tableQueryVisitor.IsValid)
                throw Error.SkipRequiresSingleTableQueryWithPKs();
            SqlSelect sqlSelect3 = (SqlSelect)SqlDuplicator.Copy((SqlNode)sqlSelect1);
            SqlExpression sqlExpression1 = skipExp;
            sqlSelect3.Top = sqlExpression1;
            SqlAlias alias2 = new SqlAlias((SqlNode)sqlSelect3);
            SqlAliasRef sqlAliasRef2 = new SqlAliasRef(alias2);
            SqlSubSelect sqlSubSelect = this.sql.SubSelect(SqlNodeType.Exists, new SqlSelect((SqlExpression)sqlAliasRef2, (SqlSource)alias2, this.dominatingExpression)
            {
                Where = (SqlExpression)this.sql.Binary(SqlNodeType.EQ2V, (SqlExpression)sqlAliasRef1, (SqlExpression)sqlAliasRef2)
            });
            SqlSelect sqlSelect4 = new SqlSelect((SqlExpression)sqlAliasRef1, (SqlSource)alias1, this.dominatingExpression);
            SqlUnary sqlUnary = this.sql.Unary(SqlNodeType.Not, (SqlExpression)sqlSubSelect, this.dominatingExpression);
            sqlSelect4.Where = (SqlExpression)sqlUnary;
            SqlExpression sqlExpression2 = takeExp;
            sqlSelect4.Top = sqlExpression2;
            return sqlSelect4;
        }

        private SqlNode VisitParameter(ParameterExpression p)
        {
            SqlExpression sqlExpression;
            if (this.map.TryGetValue(p, out sqlExpression))
                return (SqlNode)sqlExpression;
            Expression node1;
            if (this.exprMap.TryGetValue(p, out node1))
                return this.Visit(node1);
            SqlNode node2;
            if (this.dupMap.TryGetValue(p, out node2))
                return new SqlDuplicator(true).Duplicate(node2);
            throw Error.ParameterNotInScope((object)p.Name);
        }

        private SqlNode TranslateTableValuedFunction(MethodCallExpression mce, MetaFunction function)
        {
            List<SqlExpression> functionParameters = this.GetFunctionParameters(mce, function);
            SqlAlias alias = new SqlAlias((SqlNode)this.sql.TableValuedFunctionCall(function.ResultRowTypes[0].InheritanceRoot, mce.Method.ReturnType, function.MappedName, (IEnumerable<SqlExpression>)functionParameters, (Expression)mce));
            return (SqlNode)new SqlSelect(this.translator.BuildProjection((SqlExpression)new SqlAliasRef(alias), function.ResultRowTypes[0].InheritanceRoot, this.allowDeferred, (SqlLink)null, (Expression)mce), (SqlSource)alias, (Expression)mce);
        }

        private SqlNode TranslateStoredProcedureCall(MethodCallExpression mce, MetaFunction function)
        {
            if (!this.outerNode)
                throw Error.SprocsCannotBeComposed();
            List<SqlExpression> functionParameters = this.GetFunctionParameters(mce, function);
            SqlStoredProcedureCall storedProcedureCall = new SqlStoredProcedureCall(function, (SqlExpression)null, (IEnumerable<SqlExpression>)functionParameters, (Expression)mce);
            Type returnType = mce.Method.ReturnType;
            if (returnType.IsGenericType && (returnType.GetGenericTypeDefinition() == typeof(IEnumerable<>) || returnType.GetGenericTypeDefinition() == typeof(ISingleResult<>)))
            {
                MetaType inheritanceRoot = function.ResultRowTypes[0].InheritanceRoot;
                SqlUserRow sqlUserRow = new SqlUserRow(inheritanceRoot, this.typeProvider.GetApplicationType(0), (SqlUserQuery)storedProcedureCall, (Expression)mce);
                storedProcedureCall.Projection = this.translator.BuildProjection((SqlExpression)sqlUserRow, inheritanceRoot, this.allowDeferred, (SqlLink)null, (Expression)mce);
            }
            else if (!typeof(IMultipleResults).IsAssignableFrom(returnType) && !(returnType == typeof(int)) && !(returnType == typeof(int?)))
                throw Error.InvalidReturnFromSproc((object)returnType);
            return (SqlNode)storedProcedureCall;
        }

        private List<SqlExpression> GetFunctionParameters(MethodCallExpression mce, MetaFunction function)
        {
            List<SqlExpression> list = new List<SqlExpression>(mce.Arguments.Count);
            int index = 0;
            for (int count = mce.Arguments.Count; index < count; ++index)
            {
                SqlExpression sqlExpression = this.VisitExpression(mce.Arguments[index]);
                MetaParameter metaParameter = function.Parameters[index];
                if (!string.IsNullOrEmpty(metaParameter.DbType))
                {
                    SqlSimpleTypeExpression simpleTypeExpression = sqlExpression as SqlSimpleTypeExpression;
                    if (simpleTypeExpression != null)
                    {
                        ProviderType type = this.typeProvider.Parse(metaParameter.DbType);
                        simpleTypeExpression.SetSqlType(type);
                    }
                }
                list.Add(sqlExpression);
            }
            return list;
        }

        private SqlUserQuery VisitUserQuery(string query, Expression[] arguments, Type resultType)
        {
            SqlExpression[] sqlExpressionArray = new SqlExpression[arguments.Length];
            int index = 0;
            for (int length = sqlExpressionArray.Length; index < length; ++index)
                sqlExpressionArray[index] = this.VisitExpression(arguments[index]);
            SqlUserQuery query1 = new SqlUserQuery(query, (SqlExpression)null, (IEnumerable<SqlExpression>)sqlExpressionArray, this.dominatingExpression);
            if (resultType != typeof(void))
            {
                Type elementType = TypeSystem.GetElementType(resultType);
                MetaType metaType = this.services.Model.GetMetaType(elementType);
                if (TypeSystem.IsSimpleType(elementType))
                {
                    SqlUserColumn sqlUserColumn = new SqlUserColumn(elementType, this.typeProvider.From(elementType), query1, "", false, this.dominatingExpression);
                    query1.Columns.Add(sqlUserColumn);
                    query1.Projection = (SqlExpression)sqlUserColumn;
                }
                else
                {
                    SqlUserRow sqlUserRow = new SqlUserRow(metaType.InheritanceRoot, this.typeProvider.GetApplicationType(0), query1, this.dominatingExpression);
                    query1.Projection = this.translator.BuildProjection((SqlExpression)sqlUserRow, metaType, this.allowDeferred, (SqlLink)null, this.dominatingExpression);
                }
            }
            return query1;
        }

        private SqlNode VisitUnary(UnaryExpression u)
        {
            SqlExpression expression = this.VisitExpression(u.Operand);
            if (u.Method != (MethodInfo)null)
            {
                SqlFactory sqlFactory = this.sql;
                Type type = u.Type;
                MethodInfo method = u.Method;
                // ISSUE: variable of the null type
                //__Null local = null;
                SqlExpression[] args = new SqlExpression[1];
                int index = 0;
                SqlExpression sqlExpression = expression;
                args[index] = sqlExpression;
                Expression sourceExpression = this.dominatingExpression;
                return (SqlNode)sqlFactory.MethodCall(type, method, (SqlExpression)null, args, sourceExpression);
            }
            SqlExpression sqlExpression1 = (SqlExpression)null;
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sqlExpression1 = u.Operand.Type == typeof(bool) || u.Operand.Type == typeof(bool?) ? (SqlExpression)this.sql.Unary(SqlNodeType.Not, expression, this.dominatingExpression) : (SqlExpression)this.sql.Unary(SqlNodeType.BitNot, expression, this.dominatingExpression);
                    break;
                case ExpressionType.TypeAs:
                    sqlExpression1 = (SqlExpression)this.sql.Unary(SqlNodeType.Treat, expression, this.dominatingExpression);
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    sqlExpression1 = (SqlExpression)this.sql.Unary(SqlNodeType.Negate, expression, this.dominatingExpression);
                    break;
            }
            return (SqlNode)sqlExpression1;
        }

        private SqlNode VisitBinary(BinaryExpression b)
        {
            SqlExpression left = this.VisitExpression(b.Left);
            SqlExpression right = this.VisitExpression(b.Right);
            if (b.Method != (MethodInfo)null)
            {
                SqlFactory sqlFactory = this.sql;
                Type type = b.Type;
                MethodInfo method = b.Method;
                // ISSUE: variable of the null type
                //__Null local = null;
                SqlExpression[] args = new SqlExpression[2];
                int index1 = 0;
                SqlExpression sqlExpression1 = left;
                args[index1] = sqlExpression1;
                int index2 = 1;
                SqlExpression sqlExpression2 = right;
                args[index2] = sqlExpression2;
                Expression sourceExpression = this.dominatingExpression;
                return (SqlNode)sqlFactory.MethodCall(type, method, (SqlExpression)null, args, sourceExpression);
            }
            switch (b.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return (SqlNode)this.sql.Binary(SqlNodeType.Add, left, right, b.Type);
                case ExpressionType.And:
                    if (b.Left.Type == typeof(bool) || b.Left.Type == typeof(bool?))
                        return (SqlNode)this.sql.Binary(SqlNodeType.And, left, right, b.Type);
                    return (SqlNode)this.sql.Binary(SqlNodeType.BitAnd, left, right, b.Type);
                case ExpressionType.AndAlso:
                    return (SqlNode)this.sql.Binary(SqlNodeType.And, left, right, b.Type);
                case ExpressionType.Coalesce:
                    return (SqlNode)this.MakeCoalesce(left, right, b.Type);
                case ExpressionType.Divide:
                    return (SqlNode)this.sql.Binary(SqlNodeType.Div, left, right, b.Type);
                case ExpressionType.Equal:
                    return (SqlNode)this.sql.Binary(SqlNodeType.EQ, left, right, b.Type);
                case ExpressionType.ExclusiveOr:
                    return (SqlNode)this.sql.Binary(SqlNodeType.BitXor, left, right, b.Type);
                case ExpressionType.GreaterThan:
                    return (SqlNode)this.sql.Binary(SqlNodeType.GT, left, right, b.Type);
                case ExpressionType.GreaterThanOrEqual:
                    return (SqlNode)this.sql.Binary(SqlNodeType.GE, left, right, b.Type);
                case ExpressionType.LessThan:
                    return (SqlNode)this.sql.Binary(SqlNodeType.LT, left, right, b.Type);
                case ExpressionType.LessThanOrEqual:
                    return (SqlNode)this.sql.Binary(SqlNodeType.LE, left, right, b.Type);
                case ExpressionType.Modulo:
                    return (SqlNode)this.sql.Binary(SqlNodeType.Mod, left, right, b.Type);
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return (SqlNode)this.sql.Binary(SqlNodeType.Mul, left, right, b.Type);
                case ExpressionType.NotEqual:
                    return (SqlNode)this.sql.Binary(SqlNodeType.NE, left, right, b.Type);
                case ExpressionType.Or:
                    if (b.Left.Type == typeof(bool) || b.Left.Type == typeof(bool?))
                        return (SqlNode)this.sql.Binary(SqlNodeType.Or, left, right, b.Type);
                    return (SqlNode)this.sql.Binary(SqlNodeType.BitOr, left, right, b.Type);
                case ExpressionType.OrElse:
                    return (SqlNode)this.sql.Binary(SqlNodeType.Or, left, right, b.Type);
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return (SqlNode)this.sql.Binary(SqlNodeType.Sub, left, right, b.Type);
                default:
                    throw Error.BinaryOperatorNotRecognized((object)b.NodeType);
            }
        }

        private SqlExpression MakeCoalesce(SqlExpression left, SqlExpression right, Type resultType)
        {
            this.CompensateForLowerPrecedenceOfDateType(ref left, ref right);
            if (TypeSystem.IsSimpleType(resultType))
                return (SqlExpression)this.sql.Binary(SqlNodeType.Coalesce, left, right, resultType);
            List<SqlWhen> list1 = new List<SqlWhen>(1);
            List<SqlWhen> list2 = list1;
            SqlFactory sqlFactory = this.sql;
            int num = 37;
            SqlExpression expression = left;
            Expression sourceExpression = expression.SourceExpression;
            SqlWhen sqlWhen = new SqlWhen((SqlExpression)sqlFactory.Unary((SqlNodeType)num, expression, sourceExpression), right);
            list2.Add(sqlWhen);
            SqlDuplicator sqlDuplicator = new SqlDuplicator(true);
            return (SqlExpression)this.sql.SearchedCase(list1.ToArray(), (SqlExpression)sqlDuplicator.Duplicate((SqlNode)left), this.dominatingExpression);
        }

        private void CompensateForLowerPrecedenceOfDateType(ref SqlExpression left, ref SqlExpression right)
        {
            if (SqlFactory.IsSqlDateType(left) && SqlFactory.IsSqlDateTimeType(right))
            {
                right = (SqlExpression)this.ConvertDateToDateTime2(right);
            }
            else
            {
                if (!SqlFactory.IsSqlDateType(right) || !SqlFactory.IsSqlDateTimeType(left))
                    return;
                left = (SqlExpression)this.ConvertDateToDateTime2(left);
            }
        }

        private SqlNode VisitConcat(Expression source1, Expression source2)
        {
            SqlAlias alias = new SqlAlias((SqlNode)new SqlUnion((SqlNode)this.VisitSequence(source1), (SqlNode)this.VisitSequence(source2), true));
            SqlSelect sqlSelect = new SqlSelect((SqlExpression)new SqlAliasRef(alias), (SqlSource)alias, this.dominatingExpression);
            int num = 2;
            sqlSelect.OrderingType = (SqlOrderingType)num;
            return (SqlNode)sqlSelect;
        }

        private SqlNode VisitUnion(Expression source1, Expression source2)
        {
            SqlAlias alias = new SqlAlias((SqlNode)new SqlUnion((SqlNode)this.VisitSequence(source1), (SqlNode)this.VisitSequence(source2), false));
            SqlSelect sqlSelect = new SqlSelect((SqlExpression)new SqlAliasRef(alias), (SqlSource)alias, this.dominatingExpression);
            int num = 2;
            sqlSelect.OrderingType = (SqlOrderingType)num;
            return (SqlNode)sqlSelect;
        }

        private SqlNode VisitIntersect(Expression source1, Expression source2)
        {
            if (this.IsGrouping(TypeSystem.GetElementType(source1.Type)))
                throw Error.IntersectNotSupportedForHierarchicalTypes();
            SqlSelect sqlSelect1 = this.LockSelect(this.VisitSequence(source1));
            SqlSelect sqlSelect2 = this.VisitSequence(source2);
            SqlAlias alias1 = new SqlAlias((SqlNode)sqlSelect1);
            SqlAliasRef sqlAliasRef1 = new SqlAliasRef(alias1);
            SqlAlias alias2 = new SqlAlias((SqlNode)sqlSelect2);
            SqlAliasRef sqlAliasRef2 = new SqlAliasRef(alias2);
            SqlExpression sqlExpression = this.GenerateQuantifier(alias2, (SqlExpression)this.sql.Binary(SqlNodeType.EQ2V, (SqlExpression)sqlAliasRef1, (SqlExpression)sqlAliasRef2), true);
            SqlSelect sqlSelect3 = new SqlSelect((SqlExpression)sqlAliasRef1, (SqlSource)alias1, sqlSelect1.SourceExpression);
            sqlSelect3.Where = sqlExpression;
            int num1 = 1;
            sqlSelect3.IsDistinct = num1 != 0;
            int num2 = 2;
            sqlSelect3.OrderingType = (SqlOrderingType)num2;
            return (SqlNode)sqlSelect3;
        }

        private SqlNode VisitExcept(Expression source1, Expression source2)
        {
            if (this.IsGrouping(TypeSystem.GetElementType(source1.Type)))
                throw Error.ExceptNotSupportedForHierarchicalTypes();
            SqlSelect sqlSelect1 = this.LockSelect(this.VisitSequence(source1));
            SqlSelect sqlSelect2 = this.VisitSequence(source2);
            SqlAlias alias1 = new SqlAlias((SqlNode)sqlSelect1);
            SqlAliasRef sqlAliasRef1 = new SqlAliasRef(alias1);
            SqlAlias alias2 = new SqlAlias((SqlNode)sqlSelect2);
            SqlAliasRef sqlAliasRef2 = new SqlAliasRef(alias2);
            SqlExpression expression = this.GenerateQuantifier(alias2, (SqlExpression)this.sql.Binary(SqlNodeType.EQ2V, (SqlExpression)sqlAliasRef1, (SqlExpression)sqlAliasRef2), true);
            SqlSelect sqlSelect3 = new SqlSelect((SqlExpression)sqlAliasRef1, (SqlSource)alias1, sqlSelect1.SourceExpression);
            SqlUnary sqlUnary = this.sql.Unary(SqlNodeType.Not, expression);
            sqlSelect3.Where = (SqlExpression)sqlUnary;
            int num1 = 1;
            sqlSelect3.IsDistinct = num1 != 0;
            int num2 = 2;
            sqlSelect3.OrderingType = (SqlOrderingType)num2;
            return (SqlNode)sqlSelect3;
        }

        private bool IsGrouping(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IGrouping<,>);
        }

        private SqlSelect VisitOrderBy(Expression sequence, LambdaExpression expression, SqlOrderType orderType)
        {
            if (this.IsGrouping(expression.Body.Type))
                throw Error.GroupingNotSupportedAsOrderCriterion();
            if (!this.typeProvider.From(expression.Body.Type).IsOrderable)
                throw Error.TypeCannotBeOrdered((object)expression.Body.Type);
            SqlSelect sqlSelect = this.LockSelect(this.VisitSequence(sequence));
            if (sqlSelect.Selection.NodeType != SqlNodeType.AliasRef || sqlSelect.OrderBy.Count > 0)
            {
                SqlAlias alias = new SqlAlias((SqlNode)sqlSelect);
                sqlSelect = new SqlSelect((SqlExpression)new SqlAliasRef(alias), (SqlSource)alias, this.dominatingExpression);
            }
            this.map[expression.Parameters[0]] = sqlSelect.Selection;
            SqlExpression expr = this.VisitExpression(expression.Body);
            sqlSelect.OrderBy.Add(new SqlOrderExpression(orderType, expr));
            return sqlSelect;
        }

        private SqlSelect VisitThenBy(Expression sequence, LambdaExpression expression, SqlOrderType orderType)
        {
            if (this.IsGrouping(expression.Body.Type))
                throw Error.GroupingNotSupportedAsOrderCriterion();
            if (!this.typeProvider.From(expression.Body.Type).IsOrderable)
                throw Error.TypeCannotBeOrdered((object)expression.Body.Type);
            SqlSelect sqlSelect = this.VisitSequence(sequence);
            this.map[expression.Parameters[0]] = sqlSelect.Selection;
            SqlExpression expr = this.VisitExpression(expression.Body);
            sqlSelect.OrderBy.Add(new SqlOrderExpression(orderType, expr));
            return sqlSelect;
        }

        private SqlNode VisitGroupBy(Expression sequence, LambdaExpression keyLambda, LambdaExpression elemLambda, LambdaExpression resultSelector)
        {
            SqlSelect sqlSelect1 = this.LockSelect(this.VisitSequence(sequence));
            SqlAlias alias1 = new SqlAlias((SqlNode)sqlSelect1);
            SqlAliasRef sqlAliasRef1 = new SqlAliasRef(alias1);
            this.map[keyLambda.Parameters[0]] = (SqlExpression)sqlAliasRef1;
            SqlExpression expr1 = this.VisitExpression(keyLambda.Body);
            SqlAlias alias2 = new SqlAlias(new SqlDuplicator().Duplicate((SqlNode)sqlSelect1));
            SqlAliasRef sqlAliasRef2 = new SqlAliasRef(alias2);
            this.map[keyLambda.Parameters[0]] = (SqlExpression)sqlAliasRef2;
            SqlExpression right = this.VisitExpression(keyLambda.Body);
            SqlExpression selection1;
            SqlExpression sqlExpression1;
            if (elemLambda != null)
            {
                this.map[elemLambda.Parameters[0]] = (SqlExpression)sqlAliasRef2;
                selection1 = this.VisitExpression(elemLambda.Body);
                this.map[elemLambda.Parameters[0]] = (SqlExpression)sqlAliasRef1;
                sqlExpression1 = this.VisitExpression(elemLambda.Body);
            }
            else
            {
                selection1 = (SqlExpression)sqlAliasRef2;
                sqlExpression1 = (SqlExpression)sqlAliasRef1;
            }
            SqlSharedExpression expr2 = new SqlSharedExpression(expr1);
            SqlExpression sqlExpression2 = (SqlExpression)new SqlSharedExpressionRef(expr2);
            SqlSubSelect sqlSubSelect = this.sql.SubSelect(SqlNodeType.Multiset, new SqlSelect(selection1, (SqlSource)alias2, this.dominatingExpression)
            {
                Where = (SqlExpression)this.sql.Binary(SqlNodeType.EQ2V, sqlExpression2, right)
            });
            SqlSelect sqlSelect2 = new SqlSelect((SqlExpression)new SqlSharedExpressionRef(expr2), (SqlSource)alias1, this.dominatingExpression);
            sqlSelect2.GroupBy.Add((SqlExpression)expr2);
            SqlAlias sqlAlias = new SqlAlias((SqlNode)sqlSelect2);
            SqlSelect sqlSelect3;
            if (resultSelector != null)
            {
                Type type1 = typeof(IGrouping<,>);
                Type[] typeArray = new Type[2];
                int index1 = 0;
                Type clrType1 = sqlExpression2.ClrType;
                typeArray[index1] = clrType1;
                int index2 = 1;
                Type clrType2 = selection1.ClrType;
                typeArray[index2] = clrType2;
                Type type2 = type1.MakeGenericType(typeArray);
                SqlAlias alias3 = new SqlAlias((SqlNode)new SqlSelect((SqlExpression)new SqlGrouping(type2, this.typeProvider.From(type2), sqlExpression2, (SqlExpression)sqlSubSelect, this.dominatingExpression), (SqlSource)sqlAlias, this.dominatingExpression));
                SqlAliasRef sqlAliasRef3 = new SqlAliasRef(alias3);
                this.map[resultSelector.Parameters[0]] = (SqlExpression)this.sql.Member((SqlExpression)sqlAliasRef3, (MemberInfo)type2.GetProperty("Key"));
                this.map[resultSelector.Parameters[1]] = (SqlExpression)sqlAliasRef3;
                this.gmap[(SqlNode)sqlAliasRef3] = new QueryConverter.GroupInfo()
                {
                    SelectWithGroup = sqlSelect2,
                    ElementOnGroupSource = sqlExpression1
                };
                SqlExpression selection2 = this.VisitExpression(resultSelector.Body);
                sqlSelect3 = new SqlSelect(selection2, (SqlSource)alias3, this.dominatingExpression);
                this.gmap[(SqlNode)selection2] = new QueryConverter.GroupInfo()
                {
                    SelectWithGroup = sqlSelect2,
                    ElementOnGroupSource = sqlExpression1
                };
            }
            else
            {
                Type type1 = typeof(IGrouping<,>);
                Type[] typeArray = new Type[2];
                int index1 = 0;
                Type clrType1 = sqlExpression2.ClrType;
                typeArray[index1] = clrType1;
                int index2 = 1;
                Type clrType2 = selection1.ClrType;
                typeArray[index2] = clrType2;
                Type type2 = type1.MakeGenericType(typeArray);
                SqlExpression selection2 = (SqlExpression)new SqlGrouping(type2, this.typeProvider.From(type2), sqlExpression2, (SqlExpression)sqlSubSelect, this.dominatingExpression);
                sqlSelect3 = new SqlSelect(selection2, (SqlSource)sqlAlias, this.dominatingExpression);
                this.gmap[(SqlNode)selection2] = new QueryConverter.GroupInfo()
                {
                    SelectWithGroup = sqlSelect2,
                    ElementOnGroupSource = sqlExpression1
                };
            }
            return (SqlNode)sqlSelect3;
        }

        private SqlNode VisitAggregate(Expression sequence, LambdaExpression lambda, SqlNodeType aggType, Type returnType)
        {
            bool flag = aggType == SqlNodeType.Count || aggType == SqlNodeType.LongCount;
            SqlNode sqlNode = this.Visit(sequence);
            SqlSelect sqlSelect1 = this.CoerceToSequence(sqlNode);
            SqlAlias alias = new SqlAlias((SqlNode)sqlSelect1);
            SqlAliasRef sqlAliasRef = new SqlAliasRef(alias);
            MethodCallExpression mc = sequence as MethodCallExpression;
            if (!this.outerNode && !flag && (lambda == null || lambda.Parameters.Count == 1 && lambda.Parameters[0] == lambda.Body) && (mc != null && this.IsSequenceOperatorCall(mc, "Select") && sqlSelect1.From is SqlAlias))
            {
                LambdaExpression lambda1 = this.GetLambda(mc.Arguments[1]);
                lambda = Expression.Lambda(lambda1.Type, lambda1.Body, (IEnumerable<ParameterExpression>)lambda1.Parameters);
                alias = (SqlAlias)sqlSelect1.From;
                sqlAliasRef = new SqlAliasRef(alias);
            }
            if (lambda != null && !TypeSystem.IsSimpleType(lambda.Body.Type))
                throw Error.CannotAggregateType((object)lambda.Body.Type);
            if (sqlSelect1.Selection.SqlType.IsRuntimeOnlyType && !this.IsGrouping(sequence.Type) && (!flag && lambda == null))
                throw Error.NonCountAggregateFunctionsAreNotValidOnProjections((object)aggType);
            if (lambda != null)
                this.map[lambda.Parameters[0]] = (SqlExpression)sqlAliasRef;
            if (this.outerNode)
            {
                SqlExpression sqlExpression1 = lambda != null ? this.VisitExpression(lambda.Body) : (SqlExpression)null;
                SqlExpression sqlExpression2 = (SqlExpression)null;
                if (flag && sqlExpression1 != null)
                {
                    sqlExpression2 = sqlExpression1;
                    sqlExpression1 = (SqlExpression)null;
                }
                else if (sqlExpression1 == null && !flag)
                    sqlExpression1 = (SqlExpression)sqlAliasRef;
                if (sqlExpression1 != null)
                    sqlExpression1 = (SqlExpression)new SqlSimpleExpression(sqlExpression1);
                SqlSelect sqlSelect2 = new SqlSelect(this.GetAggregate(aggType, returnType, sqlExpression1), (SqlSource)alias, this.dominatingExpression);
                sqlSelect2.Where = sqlExpression2;
                int num = 1;
                sqlSelect2.OrderingType = (SqlOrderingType)num;
                return (SqlNode)sqlSelect2;
            }
            if (!flag || lambda == null)
            {
                QueryConverter.GroupInfo groupInfo = this.FindGroupInfo(sqlNode);
                if (groupInfo != null)
                {
                    SqlExpression sqlExpression = (SqlExpression)null;
                    if (lambda != null)
                    {
                        this.map[lambda.Parameters[0]] = (SqlExpression)SqlDuplicator.Copy((SqlNode)groupInfo.ElementOnGroupSource);
                        sqlExpression = this.VisitExpression(lambda.Body);
                    }
                    else if (!flag)
                        sqlExpression = groupInfo.ElementOnGroupSource;
                    if (sqlExpression != null)
                        sqlExpression = (SqlExpression)new SqlSimpleExpression(sqlExpression);
                    SqlExpression aggregate = this.GetAggregate(aggType, returnType, sqlExpression);
                    SqlColumn col = new SqlColumn(aggregate.ClrType, aggregate.SqlType, (string)null, (MetaDataMember)null, aggregate, this.dominatingExpression);
                    groupInfo.SelectWithGroup.Row.Columns.Add(col);
                    return (SqlNode)new SqlColumnRef(col);
                }
            }
            SqlExpression expr = lambda != null ? this.VisitExpression(lambda.Body) : (SqlExpression)null;
            if (expr != null)
                expr = (SqlExpression)new SqlSimpleExpression(expr);
            return (SqlNode)this.sql.SubSelect(SqlNodeType.ScalarSubSelect, new SqlSelect(this.GetAggregate(aggType, returnType, flag ? (SqlExpression)null : (lambda == null ? (SqlExpression)sqlAliasRef : expr)), (SqlSource)alias, this.dominatingExpression)
            {
                Where = flag ? expr : (SqlExpression)null
            });
        }

        private QueryConverter.GroupInfo FindGroupInfo(SqlNode source)
        {
            QueryConverter.GroupInfo groupInfo = (QueryConverter.GroupInfo)null;
            this.gmap.TryGetValue(source, out groupInfo);
            if (groupInfo != null)
                return groupInfo;
            SqlAlias sqlAlias = source as SqlAlias;
            if (sqlAlias != null)
            {
                SqlSelect sqlSelect = sqlAlias.Node as SqlSelect;
                if (sqlSelect != null)
                    return this.FindGroupInfo((SqlNode)sqlSelect.Selection);
                source = sqlAlias.Node;
            }
            SqlExpression sqlExpression = source as SqlExpression;
            if (sqlExpression == null)
                return (QueryConverter.GroupInfo)null;
            switch (sqlExpression.NodeType)
            {
                case SqlNodeType.AliasRef:
                    return this.FindGroupInfo((SqlNode)((SqlAliasRef)sqlExpression).Alias);
                case SqlNodeType.Member:
                    return this.FindGroupInfo((SqlNode)((SqlMember)sqlExpression).Expression);
                default:
                    this.gmap.TryGetValue((SqlNode)sqlExpression, out groupInfo);
                    return groupInfo;
            }
        }

        private SqlExpression GetAggregate(SqlNodeType aggType, Type clrType, SqlExpression exp)
        {
            ProviderType sqlType = this.typeProvider.From(clrType);
            return (SqlExpression)new SqlUnary(aggType, clrType, sqlType, exp, this.dominatingExpression);
        }

        private SqlNode VisitContains(Expression sequence, Expression value)
        {
            Type elemType = TypeSystem.GetElementType(sequence.Type);
            SqlNode node = this.Visit(sequence);
            if (node.NodeType == SqlNodeType.ClientArray)
            {
                SqlClientArray sqlClientArray = (SqlClientArray)node;
                return (SqlNode)this.GenerateInExpression(this.VisitExpression(value), sqlClientArray.Expressions);
            }
            if (node.NodeType == SqlNodeType.Value)
            {
                IEnumerable source = ((SqlValue)node).Value as IEnumerable;
                IQueryable queryable = source as IQueryable;
                if (queryable == null)
                    return (SqlNode)this.GenerateInExpression(this.VisitExpression(value), System.Linq.Enumerable.ToList<SqlExpression>(System.Linq.Enumerable.Select<object, SqlExpression>(System.Linq.Enumerable.OfType<object>(source), (Func<object, SqlExpression>)(v => this.sql.ValueFromObject(v, elemType, true, this.dominatingExpression)))));
                node = this.Visit(queryable.Expression);
            }
            ParameterExpression parameterExpression1 = Expression.Parameter(value.Type, "p");
            BinaryExpression binaryExpression = Expression.Equal((Expression)parameterExpression1, value);
            ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
            int index = 0;
            ParameterExpression parameterExpression2 = parameterExpression1;
            parameterExpressionArray[index] = parameterExpression2;
            LambdaExpression lambda = Expression.Lambda((Expression)binaryExpression, parameterExpressionArray);
            return this.VisitQuantifier(this.CoerceToSequence(node), lambda, true);
        }

        private SqlExpression GenerateInExpression(SqlExpression expr, List<SqlExpression> list)
        {
            if (list.Count == 0)
                return this.sql.ValueFromObject((object)false, this.dominatingExpression);
            if (list[0].SqlType.CanBeColumn)
                return (SqlExpression)this.sql.In(expr, (IEnumerable<SqlExpression>)list, this.dominatingExpression);
            SqlExpression left = (SqlExpression)this.sql.Binary(SqlNodeType.EQ, expr, list[0]);
            int index = 1;
            for (int count = list.Count; index < count; ++index)
                left = (SqlExpression)this.sql.Binary(SqlNodeType.Or, left, (SqlExpression)this.sql.Binary(SqlNodeType.EQ, (SqlExpression)SqlDuplicator.Copy((SqlNode)expr), list[index]));
            return left;
        }

        private SqlNode VisitQuantifier(Expression sequence, LambdaExpression lambda, bool isAny)
        {
            return this.VisitQuantifier(this.VisitSequence(sequence), lambda, isAny);
        }

        private SqlNode VisitQuantifier(SqlSelect select, LambdaExpression lambda, bool isAny)
        {
            SqlAlias alias = new SqlAlias((SqlNode)select);
            SqlAliasRef sqlAliasRef = new SqlAliasRef(alias);
            if (lambda != null)
                this.map[lambda.Parameters[0]] = (SqlExpression)sqlAliasRef;
            SqlExpression cond = lambda != null ? this.VisitExpression(lambda.Body) : (SqlExpression)null;
            return (SqlNode)this.GenerateQuantifier(alias, cond, isAny);
        }

        private SqlExpression GenerateQuantifier(SqlAlias alias, SqlExpression cond, bool isAny)
        {
            SqlAliasRef sqlAliasRef = new SqlAliasRef(alias);
            if (isAny)
                return (SqlExpression)this.sql.SubSelect(SqlNodeType.Exists, new SqlSelect((SqlExpression)sqlAliasRef, (SqlSource)alias, this.dominatingExpression)
                {
                    Where = cond,
                    OrderingType = SqlOrderingType.Never
                });
            SqlSelect select = new SqlSelect((SqlExpression)sqlAliasRef, (SqlSource)alias, this.dominatingExpression);
            SqlSubSelect sqlSubSelect = this.sql.SubSelect(SqlNodeType.Exists, select);
            select.Where = (SqlExpression)this.sql.Unary(SqlNodeType.Not2V, cond, this.dominatingExpression);
            return (SqlExpression)this.sql.Unary(SqlNodeType.Not, (SqlExpression)sqlSubSelect, this.dominatingExpression);
        }

        private void CheckContext(SqlExpression expr)
        {
            SqlValue sqlValue = expr as SqlValue;
            if (sqlValue == null)
                return;
            DataContext dataContext = sqlValue.Value as DataContext;
            if (dataContext != null && dataContext != this.services.Context)
                throw Error.WrongDataContext();
        }

        private SqlNode VisitMemberAccess(MemberExpression ma)
        {
            Type memberType = TypeSystem.GetMemberType(ma.Member);
            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Table<>))
            {
                Type type = memberType.GetGenericArguments()[0];
                this.CheckContext(this.VisitExpression(ma.Expression));
                ITable table = this.services.Context.GetTable(type);
                if (table != null)
                    return this.Visit((Expression)Expression.Constant((object)table));
            }
            if (ma.Member.Name == "Count" && TypeSystem.IsSequenceType(ma.Expression.Type))
                return this.VisitAggregate(ma.Expression, (LambdaExpression)null, SqlNodeType.Count, typeof(int));
            return (SqlNode)this.sql.Member(this.VisitExpression(ma.Expression), ma.Member);
        }

        private SqlNode VisitMethodCall(MethodCallExpression mc)
        {
            Type declaringType = mc.Method.DeclaringType;
            if (mc.Method.IsStatic)
            {
                if (this.IsSequenceOperatorCall(mc))
                    return this.VisitSequenceOperatorCall(mc);
                if (QueryConverter.IsDataManipulationCall(mc))
                    return this.VisitDataManipulationCall(mc);
                if ((declaringType == typeof(DBConvert) || declaringType == typeof(Convert)) && mc.Method.Name == "ChangeType")
                {
                    SqlNode sqlNode = (SqlNode)null;
                    if (mc.Arguments.Count == 2)
                    {
                        object obj = this.GetValue(mc.Arguments[1], "ChangeType");
                        if (obj != null && typeof(Type).IsAssignableFrom(obj.GetType()))
                            sqlNode = this.VisitChangeType(mc.Arguments[0], (Type)obj);
                    }
                    if (sqlNode == null)
                        throw Error.MethodFormHasNoSupportConversionToSql((object)mc.Method.Name, (object)mc.Method);
                    return sqlNode;
                }
            }
            else if (typeof(DataContext).IsAssignableFrom(mc.Method.DeclaringType))
            {
                string name = mc.Method.Name;
                if (!(name == "GetTable"))
                {
                    if (name == "ExecuteCommand" || name == "ExecuteQuery")
                        return (SqlNode)this.VisitUserQuery((string)this.GetValue(mc.Arguments[0], mc.Method.Name), QueryConverter.GetArray(mc.Arguments[1]), mc.Type);
                }
                else if (mc.Method.IsGenericMethod)
                {
                    Type[] genericArguments = mc.Method.GetGenericArguments();
                    if (genericArguments.Length == 1 && mc.Method.GetParameters().Length == 0)
                    {
                        this.CheckContext(this.VisitExpression(mc.Object));
                        ITable table = this.services.Context.GetTable(genericArguments[0]);
                        if (table != null)
                            return this.Visit((Expression)Expression.Constant((object)table));
                    }
                }
                if (this.IsMappedFunctionCall(mc))
                    return this.VisitMappedFunctionCall(mc);
            }
            else if (mc.Method.DeclaringType != typeof(string) && mc.Method.Name == "Contains" && (!mc.Method.IsStatic && typeof(IList).IsAssignableFrom(mc.Method.DeclaringType)) && (mc.Type == typeof(bool) && mc.Arguments.Count == 1 && TypeSystem.GetElementType(mc.Method.DeclaringType).IsAssignableFrom(mc.Arguments[0].Type)))
                return this.VisitContains(mc.Object, mc.Arguments[0]);
            SqlExpression sqlExpression = this.VisitExpression(mc.Object);
            SqlExpression[] args = new SqlExpression[mc.Arguments.Count];
            int index = 0;
            for (int length = args.Length; index < length; ++index)
                args[index] = this.VisitExpression(mc.Arguments[index]);
            return (SqlNode)this.sql.MethodCall(mc.Method, sqlExpression, args, this.dominatingExpression);
        }

        private object GetValue(Expression expression, string operation)
        {
            SqlExpression sqlExpression = this.VisitExpression(expression);
            if (sqlExpression.NodeType == SqlNodeType.Value)
                return ((SqlValue)sqlExpression).Value;
            throw Error.NonConstantExpressionsNotSupportedFor((object)operation);
        }

        private static Expression[] GetArray(Expression array)
        {
            NewArrayExpression newArrayExpression = array as NewArrayExpression;
            if (newArrayExpression != null)
                return System.Linq.Enumerable.ToArray<Expression>((IEnumerable<Expression>)newArrayExpression.Expressions);
            ConstantExpression constantExpression = array as ConstantExpression;
            if (constantExpression != null)
            {
                object[] objArray = constantExpression.Value as object[];
                if (objArray != null)
                {
                    Type elemType = TypeSystem.GetElementType(constantExpression.Type);
                    return (Expression[])System.Linq.Enumerable.ToArray<ConstantExpression>(System.Linq.Enumerable.Select<object, ConstantExpression>((IEnumerable<object>)objArray, (Func<object, ConstantExpression>)(o => Expression.Constant(o, elemType))));
                }
            }
            return new Expression[0];
        }

        private Expression RemoveQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
                expression = ((UnaryExpression)expression).Operand;
            return expression;
        }

        private bool IsLambda(Expression expression)
        {
            return this.RemoveQuotes(expression).NodeType == ExpressionType.Lambda;
        }

        private LambdaExpression GetLambda(Expression expression)
        {
            return this.RemoveQuotes(expression) as LambdaExpression;
        }

        private bool IsMappedFunctionCall(MethodCallExpression mc)
        {
            return this.services.Model.GetFunction(mc.Method) != null;
        }

        private SqlNode VisitMappedFunctionCall(MethodCallExpression mc)
        {
            MetaFunction function = this.services.Model.GetFunction(mc.Method);
            this.CheckContext(this.VisitExpression(mc.Object));
            if (!function.IsComposable)
                return this.TranslateStoredProcedureCall(mc, function);
            if (function.ResultRowTypes.Count > 0)
                return this.TranslateTableValuedFunction(mc, function);
            ProviderType sqlType = function.ReturnParameter == null || string.IsNullOrEmpty(function.ReturnParameter.DbType) ? this.typeProvider.From(mc.Method.ReturnType) : this.typeProvider.Parse(function.ReturnParameter.DbType);
            List<SqlExpression> functionParameters = this.GetFunctionParameters(mc, function);
            return (SqlNode)this.sql.FunctionCall(mc.Method.ReturnType, sqlType, function.MappedName, (IEnumerable<SqlExpression>)functionParameters, (Expression)mc);
        }

        private bool IsSequenceOperatorCall(MethodCallExpression mc)
        {
            Type declaringType = mc.Method.DeclaringType;
            return declaringType == typeof(System.Linq.Enumerable) || declaringType == typeof(Queryable);
        }

        private bool IsSequenceOperatorCall(MethodCallExpression mc, string methodName)
        {
            return this.IsSequenceOperatorCall(mc) && mc.Method.Name == methodName;
        }

        private SqlNode VisitSequenceOperatorCall(MethodCallExpression mc)
        {
            Type declaringType = mc.Method.DeclaringType;
            bool flag = false;
            if (!this.IsSequenceOperatorCall(mc))
                throw Error.InvalidSequenceOperatorCall((object)declaringType);
            string name = mc.Method.Name;
            // ISSUE: reference to a compiler-generated method
            uint stringHash = 0;
            if (stringHash <= 1721518424U)
            {
                if (stringHash <= 805458841U)
                {
                    if (stringHash <= 520910365U)
                    {
                        if (stringHash <= 360869050U)
                        {
                            if ((int)stringHash != 353010018)
                            {
                                if ((int)stringHash == 360869050 && name == "SelectMany")
                                {
                                    flag = true;
                                    if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                        return (SqlNode)this.VisitSelectMany(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), (LambdaExpression)null);
                                    if (mc.Arguments.Count == 3 && this.IsLambda(mc.Arguments[1]) && (this.GetLambda(mc.Arguments[1]).Parameters.Count == 1 && this.IsLambda(mc.Arguments[2])) && this.GetLambda(mc.Arguments[2]).Parameters.Count == 2)
                                        return (SqlNode)this.VisitSelectMany(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), this.GetLambda(mc.Arguments[2]));
                                    goto label_162;
                                }
                                else
                                    goto label_162;
                            }
                            else if (name == "OrderBy")
                            {
                                flag = true;
                                if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                    return (SqlNode)this.VisitOrderBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Ascending);
                                goto label_162;
                            }
                            else
                                goto label_162;
                        }
                        else if ((int)stringHash != 507488866)
                        {
                            if ((int)stringHash != 520910365 || !(name == "SingleOrDefault"))
                                goto label_162;
                            else
                                goto label_94;
                        }
                        else if (name == "DefaultIfEmpty")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 1)
                                return (SqlNode)this.VisitDefaultIfEmpty(mc.Arguments[0]);
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if (stringHash <= 638126352U)
                    {
                        if ((int)stringHash != 626204920)
                        {
                            if ((int)stringHash == 638126352 && name == "Except")
                            {
                                flag = true;
                                if (mc.Arguments.Count == 2)
                                    return this.VisitExcept(mc.Arguments[0], mc.Arguments[1]);
                                goto label_162;
                            }
                            else
                                goto label_162;
                        }
                        else if (name == "OrderByDescending")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                return (SqlNode)this.VisitOrderBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Descending);
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if ((int)stringHash != 709991534)
                    {
                        if ((int)stringHash != 781469175)
                        {
                            if ((int)stringHash == 805458841 && name == "Join")
                            {
                                flag = true;
                                if (mc.Arguments.Count == 5 && this.IsLambda(mc.Arguments[2]) && (this.GetLambda(mc.Arguments[2]).Parameters.Count == 1 && this.IsLambda(mc.Arguments[3])) && (this.GetLambda(mc.Arguments[3]).Parameters.Count == 1 && this.IsLambda(mc.Arguments[4]) && this.GetLambda(mc.Arguments[4]).Parameters.Count == 2))
                                    return (SqlNode)this.VisitJoin(mc.Arguments[0], mc.Arguments[1], this.GetLambda(mc.Arguments[2]), this.GetLambda(mc.Arguments[3]), this.GetLambda(mc.Arguments[4]));
                                goto label_162;
                            }
                            else
                                goto label_162;
                        }
                        else if (name == "Min")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 1)
                                return this.VisitAggregate(mc.Arguments[0], (LambdaExpression)null, SqlNodeType.Min, mc.Type);
                            if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Min, mc.Type);
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if (name == "Cast")
                    {
                        flag = true;
                        if (mc.Arguments.Count == 1)
                        {
                            Type type = mc.Method.GetGenericArguments()[0];
                            return this.VisitSequenceCast(mc.Arguments[0], type);
                        }
                        goto label_162;
                    }
                    else
                        goto label_162;
                }
                else if (stringHash <= 1049176909U)
                {
                    if (stringHash <= 1017635769U)
                    {
                        if ((int)stringHash != 969681439)
                        {
                            if ((int)stringHash == 1017635769 && name == "Max")
                            {
                                flag = true;
                                if (mc.Arguments.Count == 1)
                                    return this.VisitAggregate(mc.Arguments[0], (LambdaExpression)null, SqlNodeType.Max, mc.Type);
                                if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                    return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Max, mc.Type);
                                goto label_162;
                            }
                            else
                                goto label_162;
                        }
                        else if (name == "ThenBy")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                return (SqlNode)this.VisitThenBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Ascending);
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if ((int)stringHash != 1046310813)
                    {
                        if ((int)stringHash == 1049176909 && name == "Select")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                return (SqlNode)this.VisitSelect(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if (name == "Concat")
                    {
                        flag = true;
                        if (mc.Arguments.Count == 2)
                            return this.VisitConcat(mc.Arguments[0], mc.Arguments[1]);
                        goto label_162;
                    }
                    else
                        goto label_162;
                }
                else if (stringHash <= 1208454600U)
                {
                    if ((int)stringHash != 1071174216)
                    {
                        if ((int)stringHash == 1208454600 && name == "Average")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 1)
                                return this.VisitAggregate(mc.Arguments[0], (LambdaExpression)null, SqlNodeType.Avg, mc.Type);
                            if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Avg, mc.Type);
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if (name == "Sum")
                    {
                        flag = true;
                        if (mc.Arguments.Count == 1)
                            return this.VisitAggregate(mc.Arguments[0], (LambdaExpression)null, SqlNodeType.Sum, mc.Type);
                        if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                            return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Sum, mc.Type);
                        goto label_162;
                    }
                    else
                        goto label_162;
                }
                else if ((int)stringHash != 1503328741)
                {
                    if ((int)stringHash != 1628731858)
                    {
                        if ((int)stringHash == 1721518424 && name == "Contains")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 2)
                                return this.VisitContains(mc.Arguments[0], mc.Arguments[1]);
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if (name == "LongCount")
                    {
                        flag = true;
                        if (mc.Arguments.Count == 1)
                            return this.VisitAggregate(mc.Arguments[0], (LambdaExpression)null, SqlNodeType.LongCount, mc.Type);
                        if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                            return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.LongCount, mc.Type);
                        goto label_162;
                    }
                    else
                        goto label_162;
                }
                else if (!(name == "FirstOrDefault"))
                    goto label_162;
            }
            else
            {
                if (stringHash <= 3120673548U)
                {
                    if (stringHash <= 2174371988U)
                    {
                        if (stringHash <= 1890307873U)
                        {
                            if ((int)stringHash != 1736436137)
                            {
                                if ((int)stringHash == 1890307873 && name == "ThenByDescending")
                                {
                                    flag = true;
                                    if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                        return (SqlNode)this.VisitThenBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlOrderType.Descending);
                                    goto label_162;
                                }
                                else
                                    goto label_162;
                            }
                            else if (name == "GroupBy")
                            {
                                flag = true;
                                if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                    return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), (LambdaExpression)null, (LambdaExpression)null);
                                if (mc.Arguments.Count == 3 && this.IsLambda(mc.Arguments[1]) && (this.GetLambda(mc.Arguments[1]).Parameters.Count == 1 && this.IsLambda(mc.Arguments[2])) && this.GetLambda(mc.Arguments[2]).Parameters.Count == 1)
                                    return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), this.GetLambda(mc.Arguments[2]), (LambdaExpression)null);
                                if (mc.Arguments.Count == 3 && this.IsLambda(mc.Arguments[1]) && (this.GetLambda(mc.Arguments[1]).Parameters.Count == 1 && this.IsLambda(mc.Arguments[2])) && this.GetLambda(mc.Arguments[2]).Parameters.Count == 2)
                                    return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), (LambdaExpression)null, this.GetLambda(mc.Arguments[2]));
                                if (mc.Arguments.Count == 4 && this.IsLambda(mc.Arguments[1]) && (this.GetLambda(mc.Arguments[1]).Parameters.Count == 1 && this.IsLambda(mc.Arguments[2])) && (this.GetLambda(mc.Arguments[2]).Parameters.Count == 1 && this.IsLambda(mc.Arguments[3]) && this.GetLambda(mc.Arguments[3]).Parameters.Count == 2))
                                    return this.VisitGroupBy(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), this.GetLambda(mc.Arguments[2]), this.GetLambda(mc.Arguments[3]));
                                goto label_162;
                            }
                            else
                                goto label_162;
                        }
                        else if ((int)stringHash != 1974461284)
                        {
                            if ((int)stringHash == -2120595308 && name == "Union")
                            {
                                flag = true;
                                if (mc.Arguments.Count == 2)
                                    return this.VisitUnion(mc.Arguments[0], mc.Arguments[1]);
                                goto label_162;
                            }
                            else
                                goto label_162;
                        }
                        else if (name == "All")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                return this.VisitQuantifier(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), false);
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if (stringHash <= 2394299117U)
                    {
                        if ((int)stringHash != -1931749258)
                        {
                            if ((int)stringHash == -1900668179 && name == "Any")
                            {
                                flag = true;
                                if (mc.Arguments.Count == 1)
                                    return this.VisitQuantifier(mc.Arguments[0], (LambdaExpression)null, true);
                                if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                                    return this.VisitQuantifier(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), true);
                                goto label_162;
                            }
                            else
                                goto label_162;
                        }
                        else if (name == "Take")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 2)
                                return (SqlNode)this.VisitTake(mc.Arguments[0], mc.Arguments[1]);
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if ((int)stringHash != -1847930632)
                    {
                        if ((int)stringHash != -1198753408)
                        {
                            if ((int)stringHash != -1174293748 || !(name == "ToList"))
                                goto label_162;
                        }
                        else if (name == "OfType")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 1)
                            {
                                Type ofType = mc.Method.GetGenericArguments()[0];
                                return (SqlNode)this.VisitOfType(mc.Arguments[0], ofType);
                            }
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if (name == "Where")
                    {
                        flag = true;
                        if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                            return (SqlNode)this.VisitWhere(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                        goto label_162;
                    }
                    else
                        goto label_162;
                }
                else if (stringHash <= 3531690413U)
                {
                    if (stringHash <= 3347606073U)
                    {
                        if ((int)stringHash != -1047017502)
                        {
                            if ((int)stringHash != -947361223 || !(name == "ToArray"))
                                goto label_162;
                        }
                        else if (name == "Skip")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 2)
                                return (SqlNode)this.VisitSkip(mc.Arguments[0], mc.Arguments[1]);
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if ((int)stringHash != -819871866)
                    {
                        if ((int)stringHash == -763276883 && name == "Distinct")
                        {
                            flag = true;
                            if (mc.Arguments.Count == 1)
                                return (SqlNode)this.VisitDistinct(mc.Arguments[0]);
                            goto label_162;
                        }
                        else
                            goto label_162;
                    }
                    else if (name == "Intersect")
                    {
                        flag = true;
                        if (mc.Arguments.Count == 2)
                            return this.VisitIntersect(mc.Arguments[0], mc.Arguments[1]);
                        goto label_162;
                    }
                    else
                        goto label_162;
                }
                else if (stringHash <= 3885445163U)
                {
                    if ((int)stringHash != -504907628)
                    {
                        if ((int)stringHash != -409522133 || !(name == "AsEnumerable"))
                            goto label_162;
                    }
                    else if (name == "Count")
                    {
                        flag = true;
                        if (mc.Arguments.Count == 1)
                            return this.VisitAggregate(mc.Arguments[0], (LambdaExpression)null, SqlNodeType.Count, mc.Type);
                        if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                            return this.VisitAggregate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), SqlNodeType.Count, mc.Type);
                        goto label_162;
                    }
                    else
                        goto label_162;
                }
                else if ((int)stringHash != -335737780)
                {
                    if ((int)stringHash != -297973279)
                    {
                        if ((int)stringHash != -243833591 || !(name == "Single"))
                            goto label_162;
                        else
                            goto label_94;
                    }
                    else if (name == "First")
                        goto label_90;
                    else
                        goto label_162;
                }
                else if (name == "GroupJoin")
                {
                    flag = true;
                    if (mc.Arguments.Count == 5 && this.IsLambda(mc.Arguments[2]) && (this.GetLambda(mc.Arguments[2]).Parameters.Count == 1 && this.IsLambda(mc.Arguments[3])) && (this.GetLambda(mc.Arguments[3]).Parameters.Count == 1 && this.IsLambda(mc.Arguments[4]) && this.GetLambda(mc.Arguments[4]).Parameters.Count == 2))
                        return (SqlNode)this.VisitGroupJoin(mc.Arguments[0], mc.Arguments[1], this.GetLambda(mc.Arguments[2]), this.GetLambda(mc.Arguments[3]), this.GetLambda(mc.Arguments[4]));
                    goto label_162;
                }
                else
                    goto label_162;
                flag = true;
                if (mc.Arguments.Count == 1)
                    return this.Visit(mc.Arguments[0]);
                goto label_162;
            }
            label_90:
            flag = true;
            if (mc.Arguments.Count == 1)
                return this.VisitFirst(mc.Arguments[0], (LambdaExpression)null, true);
            if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                return this.VisitFirst(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), true);
            goto label_162;
            label_94:
            flag = true;
            if (mc.Arguments.Count == 1)
                return this.VisitFirst(mc.Arguments[0], (LambdaExpression)null, false);
            if (mc.Arguments.Count == 2 && this.IsLambda(mc.Arguments[1]) && this.GetLambda(mc.Arguments[1]).Parameters.Count == 1)
                return this.VisitFirst(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), false);
            label_162:
            if (flag)
                throw Error.QueryOperatorOverloadNotSupported((object)mc.Method.Name);
            throw Error.QueryOperatorNotSupported((object)mc.Method.Name);
        }

        private static bool IsDataManipulationCall(MethodCallExpression mc)
        {
            if (mc.Method.IsStatic)
                return mc.Method.DeclaringType == typeof(DataManipulation);
            return false;
        }

        private SqlNode VisitDataManipulationCall(MethodCallExpression mc)
        {
            if (!QueryConverter.IsDataManipulationCall(mc))
                throw Error.InvalidSequenceOperatorCall((object)mc.Method.Name);
            bool flag = false;
            string name = mc.Method.Name;
            if (!(name == "Insert"))
            {
                if (!(name == "Update"))
                {
                    if (name == "Delete")
                    {
                        flag = true;
                        if (mc.Arguments.Count == 2)
                            return (SqlNode)this.VisitDelete(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                        if (mc.Arguments.Count == 1)
                            return (SqlNode)this.VisitDelete(mc.Arguments[0], (LambdaExpression)null);
                    }
                }
                else
                {
                    flag = true;
                    if (mc.Arguments.Count == 3)
                        return (SqlNode)this.VisitUpdate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), this.GetLambda(mc.Arguments[2]));
                    if (mc.Arguments.Count == 2)
                    {
                        if (mc.Method.GetGenericArguments().Length == 1)
                            return (SqlNode)this.VisitUpdate(mc.Arguments[0], this.GetLambda(mc.Arguments[1]), (LambdaExpression)null);
                        return (SqlNode)this.VisitUpdate(mc.Arguments[0], (LambdaExpression)null, this.GetLambda(mc.Arguments[1]));
                    }
                    if (mc.Arguments.Count == 1)
                        return (SqlNode)this.VisitUpdate(mc.Arguments[0], (LambdaExpression)null, (LambdaExpression)null);
                }
            }
            else
            {
                flag = true;
                if (mc.Arguments.Count == 2)
                    return (SqlNode)this.VisitInsert(mc.Arguments[0], this.GetLambda(mc.Arguments[1]));
                if (mc.Arguments.Count == 1)
                    return (SqlNode)this.VisitInsert(mc.Arguments[0], (LambdaExpression)null);
            }
            if (flag)
                throw Error.QueryOperatorOverloadNotSupported((object)mc.Method.Name);
            throw Error.QueryOperatorNotSupported((object)mc.Method.Name);
        }

        private SqlNode VisitFirst(Expression sequence, LambdaExpression lambda, bool isFirst)
        {
            SqlSelect select = this.LockSelect(this.VisitSequence(sequence));
            if (lambda != null)
            {
                this.map[lambda.Parameters[0]] = select.Selection;
                select.Where = this.VisitExpression(lambda.Body);
            }
            if (isFirst)
                select.Top = this.sql.ValueFromObject((object)1, false, this.dominatingExpression);
            if (this.outerNode)
                return (SqlNode)select;
            return (SqlNode)this.sql.SubSelect(this.typeProvider.From(select.Selection.ClrType).CanBeColumn ? SqlNodeType.ScalarSubSelect : SqlNodeType.Element, select, sequence.Type);
        }

        private SqlStatement VisitInsert(Expression item, LambdaExpression resultSelector)
        {
            if (item == null)
                throw Error.ArgumentNull("item");
            this.dominatingExpression = item;
            MetaTable table1 = this.services.Model.GetTable(item.Type);
            Expression expression = this.services.Context.GetTable(table1.RowType.Type).Expression;
            ConstantExpression constantExpression = item as ConstantExpression;
            if (constantExpression == null)
                throw Error.InsertItemMustBeConstant();
            if (constantExpression.Value == null)
                throw Error.ArgumentNull("item");
            List<SqlMemberAssign> list = new List<SqlMemberAssign>();
            MetaType inheritanceType = table1.RowType.GetInheritanceType(constantExpression.Value.GetType());
            SqlExpression expr1 = this.sql.ValueFromObject(constantExpression.Value, true, expression);
            foreach (MetaDataMember metaDataMember in inheritanceType.PersistentDataMembers)
            {
                if (!metaDataMember.IsAssociation && !metaDataMember.IsDbGenerated && !metaDataMember.IsVersion)
                    list.Add(new SqlMemberAssign(metaDataMember.Member, (SqlExpression)this.sql.Member(expr1, metaDataMember.Member)));
            }
            ConstructorInfo constructor = inheritanceType.Type.GetConstructor(Type.EmptyTypes);
            SqlNew sqlNew = this.sql.New(inheritanceType, constructor, (IEnumerable<SqlExpression>)null, (IEnumerable<MemberInfo>)null, (IEnumerable<SqlMemberAssign>)list, item);
            SqlFactory sqlFactory = this.sql;
            MetaTable table2 = table1;
            MetaType rowType = table2.RowType;
            Expression sourceExpression = this.dominatingExpression;
            SqlTable table3 = sqlFactory.Table(table2, rowType, sourceExpression);
            SqlInsert sqlInsert = new SqlInsert(table3, (SqlExpression)sqlNew, item);
            if (resultSelector == null)
                return (SqlStatement)sqlInsert;
            MetaDataMember generatedIdentityMember = inheritanceType.DBGeneratedIdentityMember;
            bool flag = false;
            if (generatedIdentityMember != null)
            {
                flag = this.IsDbGeneratedKeyProjectionOnly(resultSelector.Body, generatedIdentityMember);
                if (generatedIdentityMember.Type == typeof(Guid) && (this.converterStrategy & ConverterStrategy.CanOutputFromInsert) != ConverterStrategy.Default)
                {
                    sqlInsert.OutputKey = new SqlColumn(generatedIdentityMember.Type, this.sql.Default(generatedIdentityMember), generatedIdentityMember.Name, generatedIdentityMember, (SqlExpression)null, this.dominatingExpression);
                    if (!flag)
                        sqlInsert.OutputToLocal = true;
                }
            }
            SqlSelect sqlSelect1 = (SqlSelect)null;
            SqlAlias alias = new SqlAlias((SqlNode)table3);
            SqlAliasRef sqlAliasRef = new SqlAliasRef(alias);
            this.map.Add(resultSelector.Parameters[0], (SqlExpression)sqlAliasRef);
            SqlExpression selection = this.VisitExpression(resultSelector.Body);
            SqlExpression sqlExpression;
            if (generatedIdentityMember != null)
            {
                sqlExpression = (SqlExpression)this.sql.Binary(SqlNodeType.EQ, (SqlExpression)this.sql.Member((SqlExpression)sqlAliasRef, generatedIdentityMember.Member), this.GetIdentityExpression(generatedIdentityMember, sqlInsert.OutputKey != null));
            }
            else
            {
                SqlExpression right = this.VisitExpression(item);
                sqlExpression = (SqlExpression)this.sql.Binary(SqlNodeType.EQ2V, (SqlExpression)sqlAliasRef, right);
            }
            SqlAlias sqlAlias = alias;
            LambdaExpression lambdaExpression1 = resultSelector;
            SqlSelect sqlSelect2 = new SqlSelect(selection, (SqlSource)sqlAlias, (Expression)lambdaExpression1);
            sqlSelect2.Where = sqlExpression;
            if (generatedIdentityMember != null & flag)
            {
                if (sqlInsert.OutputKey == null)
                {
                    SqlExpression expr2 = this.GetIdentityExpression(generatedIdentityMember, false);
                    if (expr2.ClrType != generatedIdentityMember.Type)
                    {
                        ProviderType sqlType = this.sql.Default(generatedIdentityMember);
                        expr2 = this.sql.ConvertTo(generatedIdentityMember.Type, sqlType, expr2);
                    }
                    ParameterExpression key = Expression.Parameter(generatedIdentityMember.Type, "p");
                    Expression[] expressionArray = new Expression[1];
                    int index1 = 0;
                    UnaryExpression unaryExpression = Expression.Convert((Expression)key, typeof(object));
                    expressionArray[index1] = (Expression)unaryExpression;
                    NewArrayExpression newArrayExpression = Expression.NewArrayInit(typeof(object), expressionArray);
                    ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                    int index2 = 0;
                    ParameterExpression parameterExpression = key;
                    parameterExpressionArray[index2] = parameterExpression;
                    LambdaExpression lambdaExpression2 = Expression.Lambda((Expression)newArrayExpression, parameterExpressionArray);
                    this.map.Add(key, expr2);
                    sqlSelect1 = new SqlSelect(this.VisitExpression(lambdaExpression2.Body), (SqlSource)null, (Expression)lambdaExpression2);
                }
                sqlSelect2.DoNotOutput = true;
            }
            SqlBlock sqlBlock = new SqlBlock(this.dominatingExpression);
            sqlBlock.Statements.Add((SqlStatement)sqlInsert);
            if (sqlSelect1 != null)
                sqlBlock.Statements.Add((SqlStatement)sqlSelect1);
            sqlBlock.Statements.Add((SqlStatement)sqlSelect2);
            return (SqlStatement)sqlBlock;
        }

        private bool IsDbGeneratedKeyProjectionOnly(Expression projection, MetaDataMember keyMember)
        {
            NewArrayExpression newArrayExpression = projection as NewArrayExpression;
            if (newArrayExpression != null && newArrayExpression.Expressions.Count == 1)
            {
                Expression expression = newArrayExpression.Expressions[0];
                while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
                    expression = ((UnaryExpression)expression).Operand;
                MemberExpression memberExpression = expression as MemberExpression;
                if (memberExpression != null && memberExpression.Member == keyMember.Member)
                    return true;
            }
            return false;
        }

        private SqlExpression GetIdentityExpression(MetaDataMember id, bool isOutputFromInsert)
        {
            if (isOutputFromInsert)
                return (SqlExpression)new SqlVariable(id.Type, this.sql.Default(id), "@id", this.dominatingExpression);
            ProviderType providerType = this.sql.Default(id);
            if (!QueryConverter.IsLegalIdentityType(providerType.GetClosestRuntimeType()))
                throw Error.InvalidDbGeneratedType((object)providerType.ToQueryString());
            if ((this.converterStrategy & ConverterStrategy.CanUseScopeIdentity) != ConverterStrategy.Default)
                return (SqlExpression)new SqlVariable(typeof(Decimal), this.typeProvider.From(typeof(Decimal)), "SCOPE_IDENTITY()", this.dominatingExpression);
            return (SqlExpression)new SqlVariable(typeof(Decimal), this.typeProvider.From(typeof(Decimal)), "@@IDENTITY", this.dominatingExpression);
        }

        private static bool IsLegalIdentityType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        private SqlExpression GetRowCountExpression()
        {
            if ((this.converterStrategy & ConverterStrategy.CanUseRowStatus) != ConverterStrategy.Default)
                return (SqlExpression)new SqlVariable(typeof(Decimal), this.typeProvider.From(typeof(Decimal)), "@@ROWCOUNT", this.dominatingExpression);
            return (SqlExpression)new SqlVariable(typeof(Decimal), this.typeProvider.From(typeof(Decimal)), "@ROWCOUNT", this.dominatingExpression);
        }

        private SqlStatement VisitUpdate(Expression item, LambdaExpression check, LambdaExpression resultSelector)
        {
            if (item == null)
                throw Error.ArgumentNull("item");
            MetaTable table = this.services.Model.GetTable(item.Type);
            Expression expression1 = this.services.Context.GetTable(table.RowType.Type).Expression;
            Type type1 = table.RowType.Type;
            bool flag = this.allowDeferred;
            this.allowDeferred = false;
            try
            {
                Expression expression2 = expression1;
                ParameterExpression parameterExpression1 = Expression.Parameter(type1, "p");
                BinaryExpression binaryExpression1 = Expression.Equal((Expression)parameterExpression1, item);
                ParameterExpression[] parameterExpressionArray1 = new ParameterExpression[1];
                int index1 = 0;
                ParameterExpression parameterExpression2 = parameterExpression1;
                parameterExpressionArray1[index1] = parameterExpression2;
                LambdaExpression lambdaExpression1 = Expression.Lambda((Expression)binaryExpression1, parameterExpressionArray1);
                LambdaExpression lambdaExpression2 = lambdaExpression1;
                if (check != null)
                {
                    LambdaExpression lambdaExpression3 = lambdaExpression2;
                    Expression[] expressionArray1 = new Expression[1];
                    int index2 = 0;
                    ParameterExpression parameterExpression3 = parameterExpression1;
                    expressionArray1[index2] = (Expression)parameterExpression3;
                    InvocationExpression invocationExpression1 = Expression.Invoke((Expression)lambdaExpression3, expressionArray1);
                    LambdaExpression lambdaExpression4 = check;
                    Expression[] expressionArray2 = new Expression[1];
                    int index3 = 0;
                    ParameterExpression parameterExpression4 = parameterExpression1;
                    expressionArray2[index3] = (Expression)parameterExpression4;
                    InvocationExpression invocationExpression2 = Expression.Invoke((Expression)lambdaExpression4, expressionArray2);
                    BinaryExpression binaryExpression2 = Expression.And((Expression)invocationExpression1, (Expression)invocationExpression2);
                    ParameterExpression[] parameterExpressionArray2 = new ParameterExpression[1];
                    int index4 = 0;
                    ParameterExpression parameterExpression5 = parameterExpression1;
                    parameterExpressionArray2[index4] = parameterExpression5;
                    lambdaExpression2 = Expression.Lambda((Expression)binaryExpression2, parameterExpressionArray2);
                }
                Type type2 = typeof(System.Linq.Enumerable);
                string methodName1 = "Where";
                Type[] typeArguments1 = new Type[1];
                int index5 = 0;
                Type type3 = type1;
                typeArguments1[index5] = type3;
                Expression[] expressionArray3 = new Expression[2];
                int index6 = 0;
                Expression expression3 = expression2;
                expressionArray3[index6] = expression3;
                int index7 = 1;
                LambdaExpression lambdaExpression5 = lambdaExpression2;
                expressionArray3[index7] = (Expression)lambdaExpression5;
                SqlSelect select = new QueryConverter.RetypeCheckClause().VisitSelect(this.VisitSequence((Expression)Expression.Call(type2, methodName1, typeArguments1, expressionArray3)));
                List<SqlAssign> list = new List<SqlAssign>();
                ConstantExpression constantExpression = item as ConstantExpression;
                if (constantExpression == null)
                    throw Error.UpdateItemMustBeConstant();
                if (constantExpression.Value == null)
                    throw Error.ArgumentNull("item");
                MetaType metaType = this.services.Model.GetMetaType(constantExpression.Value.GetType());
                foreach (ModifiedMemberInfo modifiedMemberInfo in this.services.Context.GetTable(metaType.InheritanceRoot.Type).GetModifiedMembers(constantExpression.Value))
                {
                    MetaDataMember dataMember = metaType.GetDataMember(modifiedMemberInfo.Member);
                    list.Add(new SqlAssign((SqlExpression)this.sql.Member(select.Selection, modifiedMemberInfo.Member), (SqlExpression)new SqlValue(dataMember.Type, this.typeProvider.From(dataMember.Type), modifiedMemberInfo.CurrentValue, true, expression1), expression1));
                }
                SqlUpdate sqlUpdate = new SqlUpdate(select, (IEnumerable<SqlAssign>)list, expression1);
                if (resultSelector == null)
                    return (SqlStatement)sqlUpdate;
                Expression expression4 = expression1;
                Type type4 = typeof(System.Linq.Enumerable);
                string methodName2 = "Where";
                Type[] typeArguments2 = new Type[1];
                int index8 = 0;
                Type type5 = type1;
                typeArguments2[index8] = type5;
                Expression[] expressionArray4 = new Expression[2];
                int index9 = 0;
                Expression expression5 = expression4;
                expressionArray4[index9] = expression5;
                int index10 = 1;
                LambdaExpression lambdaExpression6 = lambdaExpression1;
                expressionArray4[index10] = (Expression)lambdaExpression6;
                Expression expression6 = (Expression)Expression.Call(type4, methodName2, typeArguments2, expressionArray4);
                Type type6 = typeof(System.Linq.Enumerable);
                string methodName3 = "Select";
                Type[] typeArguments3 = new Type[2];
                int index11 = 0;
                Type type7 = type1;
                typeArguments3[index11] = type7;
                int index12 = 1;
                Type type8 = resultSelector.Body.Type;
                typeArguments3[index12] = type8;
                Expression[] expressionArray5 = new Expression[2];
                int index13 = 0;
                Expression expression7 = expression6;
                expressionArray5[index13] = expression7;
                int index14 = 1;
                LambdaExpression lambdaExpression7 = resultSelector;
                expressionArray5[index14] = (Expression)lambdaExpression7;
                SqlSelect sqlSelect = this.VisitSequence((Expression)Expression.Call(type6, methodName3, typeArguments3, expressionArray5));
                sqlSelect.Where = this.sql.AndAccumulate((SqlExpression)this.sql.Binary(SqlNodeType.GT, this.GetRowCountExpression(), this.sql.ValueFromObject((object)0, false, this.dominatingExpression)), sqlSelect.Where);
                return (SqlStatement)new SqlBlock(expression1)
                {
                    Statements = {
            (SqlStatement) sqlUpdate,
            (SqlStatement) sqlSelect
          }
                };
            }
            finally
            {
                this.allowDeferred = flag;
            }
        }

        private SqlStatement VisitDelete(Expression item, LambdaExpression check)
        {
            if (item == null)
                throw Error.ArgumentNull("item");
            bool flag = this.allowDeferred;
            this.allowDeferred = false;
            try
            {
                MetaTable table = this.services.Model.GetTable(item.Type);
                Expression expression1 = this.services.Context.GetTable(table.RowType.Type).Expression;
                Type type1 = table.RowType.Type;
                ParameterExpression parameterExpression1 = Expression.Parameter(type1, "p");
                BinaryExpression binaryExpression1 = Expression.Equal((Expression)parameterExpression1, item);
                ParameterExpression[] parameterExpressionArray1 = new ParameterExpression[1];
                int index1 = 0;
                ParameterExpression parameterExpression2 = parameterExpression1;
                parameterExpressionArray1[index1] = parameterExpression2;
                LambdaExpression lambdaExpression1 = Expression.Lambda((Expression)binaryExpression1, parameterExpressionArray1);
                if (check != null)
                {
                    LambdaExpression lambdaExpression2 = lambdaExpression1;
                    Expression[] expressionArray1 = new Expression[1];
                    int index2 = 0;
                    ParameterExpression parameterExpression3 = parameterExpression1;
                    expressionArray1[index2] = (Expression)parameterExpression3;
                    InvocationExpression invocationExpression1 = Expression.Invoke((Expression)lambdaExpression2, expressionArray1);
                    LambdaExpression lambdaExpression3 = check;
                    Expression[] expressionArray2 = new Expression[1];
                    int index3 = 0;
                    ParameterExpression parameterExpression4 = parameterExpression1;
                    expressionArray2[index3] = (Expression)parameterExpression4;
                    InvocationExpression invocationExpression2 = Expression.Invoke((Expression)lambdaExpression3, expressionArray2);
                    BinaryExpression binaryExpression2 = Expression.And((Expression)invocationExpression1, (Expression)invocationExpression2);
                    ParameterExpression[] parameterExpressionArray2 = new ParameterExpression[1];
                    int index4 = 0;
                    ParameterExpression parameterExpression5 = parameterExpression1;
                    parameterExpressionArray2[index4] = parameterExpression5;
                    lambdaExpression1 = Expression.Lambda((Expression)binaryExpression2, parameterExpressionArray2);
                }
                Type type2 = typeof(System.Linq.Enumerable);
                string methodName = "Where";
                Type[] typeArguments = new Type[1];
                int index5 = 0;
                Type type3 = type1;
                typeArguments[index5] = type3;
                Expression[] expressionArray = new Expression[2];
                int index6 = 0;
                Expression expression2 = expression1;
                expressionArray[index6] = expression2;
                int index7 = 1;
                LambdaExpression lambdaExpression4 = lambdaExpression1;
                expressionArray[index7] = (Expression)lambdaExpression4;
                SqlSelect select = new QueryConverter.RetypeCheckClause().VisitSelect(this.VisitSequence((Expression)Expression.Call(type2, methodName, typeArguments, expressionArray)));
                this.allowDeferred = flag;
                Expression sourceExpression = expression1;
                return (SqlStatement)new SqlDelete(select, sourceExpression);
            }
            finally
            {
                this.allowDeferred = flag;
            }
        }

        private SqlExpression VisitNewArrayInit(NewArrayExpression arr)
        {
            SqlExpression[] exprs = new SqlExpression[arr.Expressions.Count];
            int index = 0;
            for (int length = exprs.Length; index < length; ++index)
                exprs[index] = this.VisitExpression(arr.Expressions[index]);
            return (SqlExpression)new SqlClientArray(arr.Type, this.typeProvider.From(arr.Type), exprs, this.dominatingExpression);
        }

        private SqlExpression VisitListInit(ListInitExpression list)
        {
            if ((ConstructorInfo)null != list.NewExpression.Constructor && list.NewExpression.Arguments.Count != 0)
                throw Error.UnrecognizedExpressionNode((object)list.NodeType);
            SqlExpression[] exprs = new SqlExpression[list.Initializers.Count];
            int index = 0;
            for (int length = exprs.Length; index < length; ++index)
            {
                if (1 != list.Initializers[index].Arguments.Count)
                    throw Error.UnrecognizedExpressionNode((object)list.NodeType);
                exprs[index] = this.VisitExpression(System.Linq.Enumerable.Single<Expression>((IEnumerable<Expression>)list.Initializers[index].Arguments));
            }
            return (SqlExpression)new SqlClientArray(list.Type, this.typeProvider.From(list.Type), exprs, this.dominatingExpression);
        }

        private class GroupInfo
        {
            internal SqlSelect SelectWithGroup;
            internal SqlExpression ElementOnGroupSource;
        }

        private enum ConversionMethod
        {
            Treat,
            Ignore,
            Convert,
            Lift,
        }

        private class RetypeCheckClause : SqlVisitor
        {
            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                if (mc.Arguments.Count == 2 && mc.Method.Name == "op_Equality")
                {
                    SqlExpression sqlExpression = mc.Arguments[1];
                    if (sqlExpression.NodeType == SqlNodeType.Value)
                        ((SqlSimpleTypeExpression)sqlExpression).SetSqlType(mc.Arguments[0].SqlType);
                }
                return base.VisitMethodCall(mc);
            }
        }
    }

    /// <summary>
    /// 表示具有单个返回序列的映射函数的结果。
    /// </summary>
    /// <typeparam name="T">返回序列中的元素的类型。</typeparam>
    public interface ISingleResult<T> : IEnumerable<T>, IEnumerable, IFunctionResult, IDisposable
    {
    }

    internal sealed class LinkedTableExpression : InternalExpression
    {
        private SqlLink link;
        private ITable table;

        internal SqlLink Link
        {
            get
            {
                return this.link;
            }
        }

        internal ITable Table
        {
            get
            {
                return this.table;
            }
        }

        internal LinkedTableExpression(SqlLink link, ITable table, Type type)
          : base(InternalExpressionType.LinkedTable, type)
        {
            this.link = link;
            this.table = table;
        }
    }

    internal sealed class KnownExpression : InternalExpression
    {
        private SqlNode node;

        internal SqlNode Node
        {
            get
            {
                return this.node;
            }
        }

        internal KnownExpression(SqlNode node, Type type)
          : base(InternalExpressionType.Known, type)
        {
            this.node = node;
        }
    }

    internal enum InternalExpressionType
    {
        Known = 2000,
        LinkedTable = 2001,
    }

    internal abstract class InternalExpression : Expression
    {
        internal InternalExpression(InternalExpressionType nt, Type type)
          : base((ExpressionType)nt, type)
        {
        }

        internal static KnownExpression Known(SqlExpression expr)
        {
            return new KnownExpression((SqlNode)expr, expr.ClrType);
        }

        internal static KnownExpression Known(SqlNode node, Type type)
        {
            return new KnownExpression(node, type);
        }
    }   

    [Flags]
    internal enum ConverterStrategy
    {
        Default = 0,
        SkipWithRowNumber = 1,
        CanUseScopeIdentity = 2,
        CanUseOuterApply = 4,
        CanUseRowStatus = 8,
        CanUseJoinOn = 16,
        CanOutputFromInsert = 32,
    }
}
