using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlFactory
    {
        private TypeSystemProvider typeProvider;
        private MetaModel model;

        internal TypeSystemProvider TypeProvider
        {
            get
            {
                return this.typeProvider;
            }
        }

        internal SqlFactory(TypeSystemProvider typeProvider, MetaModel model)
        {
            this.typeProvider = typeProvider;
            this.model = model;
        }

        internal SqlExpression ConvertTo(Type clrType, ProviderType sqlType, SqlExpression expr)
        {
            Type targetClrType = clrType;
            ProviderType targetSqlType = sqlType;
            SqlExpression expression = expr;
            Expression sourceExpression = expression.SourceExpression;
            return (SqlExpression)this.UnaryConvert(targetClrType, targetSqlType, expression, sourceExpression);
        }

        internal SqlExpression ConvertTo(Type clrType, SqlExpression expr)
        {
            if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>))
                clrType = clrType.GetGenericArguments()[0];
            bool flag = clrType == typeof(TimeSpan);
            if (SqlFactory.IsSqlTimeType(expr))
            {
                if (flag)
                    return expr;
                expr = this.ConvertToDateTime(expr);
            }
            Type targetClrType = clrType;
            ProviderType targetSqlType = this.typeProvider.From(clrType);
            SqlExpression expression = expr;
            Expression sourceExpression = expression.SourceExpression;
            return (SqlExpression)this.UnaryConvert(targetClrType, targetSqlType, expression, sourceExpression);
        }

        internal SqlExpression ConvertToBigint(SqlExpression expr)
        {
            return this.ConvertTo(typeof(long), expr);
        }

        internal SqlExpression ConvertToInt(SqlExpression expr)
        {
            return this.ConvertTo(typeof(int), expr);
        }

        internal SqlExpression ConvertToDouble(SqlExpression expr)
        {
            return this.ConvertTo(typeof(double), expr);
        }

        internal SqlExpression ConvertTimeToDouble(SqlExpression exp)
        {
            if (!SqlFactory.IsSqlTimeType(exp))
                return exp;
            return this.ConvertToDouble(exp);
        }

        internal SqlExpression ConvertToBool(SqlExpression expr)
        {
            return this.ConvertTo(typeof(bool), expr);
        }

        internal SqlExpression ConvertToDateTime(SqlExpression expr)
        {
            Type targetClrType = typeof(DateTime);
            ProviderType targetSqlType = this.typeProvider.From(typeof(DateTime));
            SqlExpression expression = expr;
            Expression sourceExpression = expression.SourceExpression;
            return (SqlExpression)this.UnaryConvert(targetClrType, targetSqlType, expression, sourceExpression);
        }

        internal SqlExpression AndAccumulate(SqlExpression left, SqlExpression right)
        {
            if (left == null)
                return right;
            if (right == null)
                return left;
            return (SqlExpression)this.Binary(SqlNodeType.And, left, right);
        }

        internal SqlExpression OrAccumulate(SqlExpression left, SqlExpression right)
        {
            if (left == null)
                return right;
            if (right == null)
                return left;
            return (SqlExpression)this.Binary(SqlNodeType.Or, left, right);
        }

        internal SqlExpression Concat(params SqlExpression[] expressions)
        {
            SqlExpression[] sqlExpressionArray = expressions;
            int index1 = sqlExpressionArray.Length - 1;
            SqlExpression right = sqlExpressionArray[index1];
            for (int index2 = expressions.Length - 2; index2 >= 0; --index2)
                right = (SqlExpression)this.Binary(SqlNodeType.Concat, expressions[index2], right);
            return right;
        }

        internal SqlExpression Add(params SqlExpression[] expressions)
        {
            SqlExpression[] sqlExpressionArray = expressions;
            int index1 = sqlExpressionArray.Length - 1;
            SqlExpression right = sqlExpressionArray[index1];
            for (int index2 = expressions.Length - 2; index2 >= 0; --index2)
                right = (SqlExpression)this.Binary(SqlNodeType.Add, expressions[index2], right);
            return right;
        }

        internal SqlExpression Subtract(SqlExpression first, SqlExpression second)
        {
            return (SqlExpression)this.Binary(SqlNodeType.Sub, first, second);
        }

        internal SqlExpression Multiply(params SqlExpression[] expressions)
        {
            SqlExpression[] sqlExpressionArray = expressions;
            int index1 = sqlExpressionArray.Length - 1;
            SqlExpression right = sqlExpressionArray[index1];
            for (int index2 = expressions.Length - 2; index2 >= 0; --index2)
                right = (SqlExpression)this.Binary(SqlNodeType.Mul, expressions[index2], right);
            return right;
        }

        internal SqlExpression Divide(SqlExpression first, SqlExpression second)
        {
            return (SqlExpression)this.Binary(SqlNodeType.Div, first, second);
        }

        internal SqlExpression Add(SqlExpression expr, int second)
        {
            return (SqlExpression)this.Binary(SqlNodeType.Add, expr, this.ValueFromObject((object)second, false, expr.SourceExpression));
        }

        internal SqlExpression Subtract(SqlExpression expr, int second)
        {
            return (SqlExpression)this.Binary(SqlNodeType.Sub, expr, this.ValueFromObject((object)second, false, expr.SourceExpression));
        }

        internal SqlExpression Multiply(SqlExpression expr, long second)
        {
            return (SqlExpression)this.Binary(SqlNodeType.Mul, expr, this.ValueFromObject((object)second, false, expr.SourceExpression));
        }

        internal SqlExpression Divide(SqlExpression expr, long second)
        {
            return (SqlExpression)this.Binary(SqlNodeType.Div, expr, this.ValueFromObject((object)second, false, expr.SourceExpression));
        }

        internal SqlExpression Mod(SqlExpression expr, long second)
        {
            return (SqlExpression)this.Binary(SqlNodeType.Mod, expr, this.ValueFromObject((object)second, false, expr.SourceExpression));
        }

        internal SqlExpression LEN(SqlExpression expr)
        {
            Type clrType = typeof(int);
            string name = "LEN";
            SqlExpression[] sqlExpressionArray = new SqlExpression[1];
            int index = 0;
            SqlExpression sqlExpression = expr;
            sqlExpressionArray[index] = sqlExpression;
            Expression sourceExpression = expr.SourceExpression;
            return (SqlExpression)this.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, sourceExpression);
        }

        internal SqlExpression DATALENGTH(SqlExpression expr)
        {
            Type clrType = typeof(int);
            string name = "DATALENGTH";
            SqlExpression[] sqlExpressionArray = new SqlExpression[1];
            int index = 0;
            SqlExpression sqlExpression = expr;
            sqlExpressionArray[index] = sqlExpression;
            Expression sourceExpression = expr.SourceExpression;
            return (SqlExpression)this.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, sourceExpression);
        }

        internal SqlExpression CLRLENGTH(SqlExpression expr)
        {
            return (SqlExpression)this.Unary(SqlNodeType.ClrLength, expr);
        }

        internal SqlExpression DATEPART(string partName, SqlExpression expr)
        {
            Type clrType = typeof(int);
            string name = "DATEPART";
            SqlExpression[] sqlExpressionArray = new SqlExpression[2];
            int index1 = 0;
            SqlVariable sqlVariable = new SqlVariable(typeof(void), (ProviderType)null, partName, expr.SourceExpression);
            sqlExpressionArray[index1] = (SqlExpression)sqlVariable;
            int index2 = 1;
            SqlExpression sqlExpression = expr;
            sqlExpressionArray[index2] = sqlExpression;
            Expression sourceExpression = expr.SourceExpression;
            return (SqlExpression)this.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, sourceExpression);
        }

        internal SqlExpression DATEADD(string partName, SqlExpression value, SqlExpression expr)
        {
            string partName1 = partName;
            SqlExpression sqlExpression = value;
            SqlExpression expr1 = expr;
            Expression sourceExpression = expr1.SourceExpression;
            int num = 0;
            return this.DATEADD(partName1, sqlExpression, expr1, sourceExpression, num != 0);
        }

        internal SqlExpression DATEADD(string partName, SqlExpression value, SqlExpression expr, Expression sourceExpression, bool asNullable)
        {
            Type clrType = asNullable ? typeof(DateTime?) : typeof(DateTime);
            string name = "DATEADD";
            SqlExpression[] sqlExpressionArray = new SqlExpression[3];
            int index1 = 0;
            SqlVariable sqlVariable = new SqlVariable(typeof(void), (ProviderType)null, partName, sourceExpression);
            sqlExpressionArray[index1] = (SqlExpression)sqlVariable;
            int index2 = 1;
            SqlExpression sqlExpression1 = value;
            sqlExpressionArray[index2] = sqlExpression1;
            int index3 = 2;
            SqlExpression sqlExpression2 = expr;
            sqlExpressionArray[index3] = sqlExpression2;
            Expression source = sourceExpression;
            return (SqlExpression)this.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
        }

        internal SqlExpression DATETIMEOFFSETADD(string partName, SqlExpression value, SqlExpression expr)
        {
            string partName1 = partName;
            SqlExpression sqlExpression = value;
            SqlExpression expr1 = expr;
            Expression sourceExpression = expr1.SourceExpression;
            int num = 0;
            return this.DATETIMEOFFSETADD(partName1, sqlExpression, expr1, sourceExpression, num != 0);
        }

        internal SqlExpression DATETIMEOFFSETADD(string partName, SqlExpression value, SqlExpression expr, Expression sourceExpression, bool asNullable)
        {
            Type clrType = asNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);
            string name = "DATEADD";
            SqlExpression[] sqlExpressionArray = new SqlExpression[3];
            int index1 = 0;
            SqlVariable sqlVariable = new SqlVariable(typeof(void), (ProviderType)null, partName, sourceExpression);
            sqlExpressionArray[index1] = (SqlExpression)sqlVariable;
            int index2 = 1;
            SqlExpression sqlExpression1 = value;
            sqlExpressionArray[index2] = sqlExpression1;
            int index3 = 2;
            SqlExpression sqlExpression2 = expr;
            sqlExpressionArray[index3] = sqlExpression2;
            Expression source = sourceExpression;
            return (SqlExpression)this.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
        }

        internal SqlExpression AddTimeSpan(SqlExpression dateTime, SqlExpression timeSpan)
        {
            return this.AddTimeSpan(dateTime, timeSpan, false);
        }

        internal SqlExpression AddTimeSpan(SqlExpression dateTime, SqlExpression timeSpan, bool asNullable)
        {
            SqlExpression sqlExpression1 = this.DATEPART("NANOSECOND", timeSpan);
            SqlExpression sqlExpression2 = this.DATEPART("MILLISECOND", timeSpan);
            SqlExpression sqlExpression3 = this.DATEPART("SECOND", timeSpan);
            SqlExpression sqlExpression4 = this.DATEPART("MINUTE", timeSpan);
            SqlExpression sqlExpression5 = this.DATEPART("HOUR", timeSpan);
            SqlExpression expr1 = dateTime;
            SqlExpression expr2 = !SqlFactory.IsSqlHighPrecisionDateTimeType(dateTime) ? this.DATEADD("MILLISECOND", sqlExpression2, expr1, dateTime.SourceExpression, asNullable) : this.DATEADD("NANOSECOND", sqlExpression1, expr1, dateTime.SourceExpression, asNullable);
            SqlExpression expr3 = this.DATEADD("SECOND", sqlExpression3, expr2, dateTime.SourceExpression, asNullable);
            SqlExpression expr4 = this.DATEADD("MINUTE", sqlExpression4, expr3, dateTime.SourceExpression, asNullable);
            SqlExpression expr5 = this.DATEADD("HOUR", sqlExpression5, expr4, dateTime.SourceExpression, asNullable);
            if (SqlFactory.IsSqlDateTimeOffsetType(dateTime))
                return this.ConvertTo(typeof(DateTimeOffset), expr5);
            return expr5;
        }

        internal static bool IsSqlDateTimeType(SqlExpression exp)
        {
            SqlDbType sqlDbType = ((SqlTypeSystem.SqlType)exp.SqlType).SqlDbType;
            if (sqlDbType != SqlDbType.DateTime)
                return sqlDbType == SqlDbType.SmallDateTime;
            return true;
        }

        internal static bool IsSqlDateType(SqlExpression exp)
        {
            return ((SqlTypeSystem.SqlType)exp.SqlType).SqlDbType == SqlDbType.Date;
        }

        internal static bool IsSqlTimeType(SqlExpression exp)
        {
            return ((SqlTypeSystem.SqlType)exp.SqlType).SqlDbType == SqlDbType.Time;
        }

        internal static bool IsSqlDateTimeOffsetType(SqlExpression exp)
        {
            return ((SqlTypeSystem.SqlType)exp.SqlType).SqlDbType == SqlDbType.DateTimeOffset;
        }

        internal static bool IsSqlHighPrecisionDateTimeType(SqlExpression exp)
        {
            SqlDbType sqlDbType = ((SqlTypeSystem.SqlType)exp.SqlType).SqlDbType;
            switch (sqlDbType)
            {
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                    return true;
                default:
                    return sqlDbType == SqlDbType.DateTimeOffset;
            }
        }

        internal SqlExpression Value(Type clrType, ProviderType sqlType, object value, bool isClientSpecified, Expression sourceExpression)
        {
            if (typeof(Type).IsAssignableFrom(clrType) && value != null)
                return this.StaticType(this.model.GetMetaType((Type)value), sourceExpression);
            return (SqlExpression)new SqlValue(clrType, sqlType, value, isClientSpecified, sourceExpression);
        }

        internal SqlExpression StaticType(MetaType typeOf, Expression sourceExpression)
        {
            if (typeOf == null)
                throw Error.ArgumentNull("typeOf");
            if (typeOf.InheritanceCode == null)
                return (SqlExpression)new SqlValue(typeof(Type), this.typeProvider.From(typeof(Type)), (object)typeOf.Type, false, sourceExpression);
            Type type = typeOf.InheritanceCode.GetType();
            return this.DiscriminatedType((SqlExpression)new SqlValue(type, this.typeProvider.From(type), typeOf.InheritanceCode, true, sourceExpression), typeOf);
        }

        internal SqlExpression DiscriminatedType(SqlExpression discriminator, MetaType targetType)
        {
            return (SqlExpression)new SqlDiscriminatedType(this.typeProvider.From(typeof(Type)), discriminator, targetType, discriminator.SourceExpression);
        }

        internal SqlTable Table(MetaTable table, MetaType rowType, Expression sourceExpression)
        {
            return new SqlTable(table, rowType, this.typeProvider.GetApplicationType(0), sourceExpression);
        }

        internal SqlUnary Unary(SqlNodeType nodeType, SqlExpression expression)
        {
            int num = (int)nodeType;
            SqlExpression expression1 = expression;
            Expression sourceExpression = expression1.SourceExpression;
            return this.Unary((SqlNodeType)num, expression1, sourceExpression);
        }

        internal SqlRowNumber RowNumber(List<SqlOrderExpression> orderBy, Expression sourceExpression)
        {
            return new SqlRowNumber(typeof(long), this.typeProvider.From(typeof(long)), orderBy, sourceExpression);
        }

        internal SqlUnary Unary(SqlNodeType nodeType, SqlExpression expression, Expression sourceExpression)
        {
            return this.Unary(nodeType, expression, (MethodInfo)null, sourceExpression);
        }

        internal SqlUnary Unary(SqlNodeType nodeType, SqlExpression expression, MethodInfo method, Expression sourceExpression)
        {
            Type clrType;
            ProviderType sqlType;
            if (nodeType == SqlNodeType.Count)
            {
                clrType = typeof(int);
                sqlType = this.typeProvider.From(typeof(int));
            }
            else if (nodeType == SqlNodeType.LongCount)
            {
                clrType = typeof(long);
                sqlType = this.typeProvider.From(typeof(long));
            }
            else if (nodeType == SqlNodeType.ClrLength)
            {
                clrType = typeof(int);
                sqlType = this.typeProvider.From(typeof(int));
            }
            else
            {
                clrType = !SqlNodeTypeOperators.IsPredicateUnaryOperator(nodeType) ? expression.ClrType : (expression.ClrType.Equals(typeof(bool?)) ? typeof(bool?) : typeof(bool));
                sqlType = this.typeProvider.PredictTypeForUnary(nodeType, expression.SqlType);
            }
            return new SqlUnary(nodeType, clrType, sqlType, expression, method, sourceExpression);
        }

        internal SqlUnary UnaryConvert(Type targetClrType, ProviderType targetSqlType, SqlExpression expression, Expression sourceExpression)
        {
            return new SqlUnary(SqlNodeType.Convert, targetClrType, targetSqlType, expression, (MethodInfo)null, sourceExpression);
        }

        internal SqlUnary UnaryValueOf(SqlExpression expression, Expression sourceExpression)
        {
            return new SqlUnary(SqlNodeType.ValueOf, TypeSystem.GetNonNullableType(expression.ClrType), expression.SqlType, expression, (MethodInfo)null, sourceExpression);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right)
        {
            return this.Binary(nodeType, left, right, (MethodInfo)null, (Type)null);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right, MethodInfo method)
        {
            return this.Binary(nodeType, left, right, method, (Type)null);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right, Type clrType)
        {
            return this.Binary(nodeType, left, right, (MethodInfo)null, clrType);
        }

        internal SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right, MethodInfo method, Type clrType)
        {
            ProviderType sqlType;
            if (SqlNodeTypeOperators.IsPredicateBinaryOperator(nodeType))
            {
                if (clrType == (Type)null)
                    clrType = typeof(bool);
                sqlType = this.typeProvider.From(clrType);
            }
            else
            {
                ProviderType providerType = this.typeProvider.PredictTypeForBinary(nodeType, left.SqlType, right.SqlType);
                if (providerType == right.SqlType)
                {
                    if (clrType == (Type)null)
                        clrType = right.ClrType;
                    sqlType = right.SqlType;
                }
                else if (providerType == left.SqlType)
                {
                    if (clrType == (Type)null)
                        clrType = left.ClrType;
                    sqlType = left.SqlType;
                }
                else
                {
                    sqlType = providerType;
                    if (clrType == (Type)null)
                        clrType = providerType.GetClosestRuntimeType();
                }
            }
            return new SqlBinary(nodeType, clrType, sqlType, left, right, method);
        }

        internal SqlBetween Between(SqlExpression expr, SqlExpression start, SqlExpression end, Expression source)
        {
            return new SqlBetween(typeof(bool), this.typeProvider.From(typeof(bool)), expr, start, end, source);
        }

        internal SqlIn In(SqlExpression expr, IEnumerable<SqlExpression> values, Expression source)
        {
            return new SqlIn(typeof(bool), this.typeProvider.From(typeof(bool)), expr, values, source);
        }

        internal SqlLike Like(SqlExpression expr, SqlExpression pattern, SqlExpression escape, Expression source)
        {
            return new SqlLike(typeof(bool), this.typeProvider.From(typeof(bool)), expr, pattern, escape, source);
        }

        internal SqlSearchedCase SearchedCase(SqlWhen[] whens, SqlExpression @else, Expression sourceExpression)
        {
            return new SqlSearchedCase(whens[0].Value.ClrType, (IEnumerable<SqlWhen>)whens, @else, sourceExpression);
        }

        internal SqlExpression Case(Type clrType, SqlExpression discriminator, List<SqlExpression> matches, List<SqlExpression> values, Expression sourceExpression)
        {
            if (values.Count == 0)
                throw Error.EmptyCaseNotSupported();
            bool flag = false;
            foreach (SqlExpression expr in values)
                flag |= SqlNodeTypeOperators.IsClientAidedExpression(expr);
            if (flag)
            {
                List<SqlClientWhen> list = new List<SqlClientWhen>();
                int index = 0;
                for (int count = matches.Count; index < count; ++index)
                    list.Add(new SqlClientWhen(matches[index], values[index]));
                return (SqlExpression)new SqlClientCase(clrType, discriminator, (IEnumerable<SqlClientWhen>)list, sourceExpression);
            }
            List<SqlWhen> list1 = new List<SqlWhen>();
            int index1 = 0;
            for (int count = matches.Count; index1 < count; ++index1)
                list1.Add(new SqlWhen(matches[index1], values[index1]));
            return (SqlExpression)new SqlSimpleCase(clrType, discriminator, (IEnumerable<SqlWhen>)list1, sourceExpression);
        }

        internal SqlExpression Parameter(object value, Expression source)
        {
            return this.Value(value.GetType(), this.typeProvider.From(value), value, true, source);
        }

        internal SqlExpression ValueFromObject(object value, Expression sourceExpression)
        {
            return this.ValueFromObject(value, false, sourceExpression);
        }

        internal SqlExpression ValueFromObject(object value, bool isClientSpecified, Expression sourceExpression)
        {
            if (value == null)
                throw Error.ArgumentNull("value");
            Type type = value.GetType();
            return this.ValueFromObject(value, type, isClientSpecified, sourceExpression);
        }

        internal SqlExpression ValueFromObject(object value, Type clrType, bool isClientSpecified, Expression sourceExpression)
        {
            if (clrType == (Type)null)
                throw Error.ArgumentNull("clrType");
            ProviderType sqlType = value == null ? this.typeProvider.From(clrType) : this.typeProvider.From(value);
            return this.Value(clrType, sqlType, value, isClientSpecified, sourceExpression);
        }

        public SqlExpression TypedLiteralNull(Type type, Expression sourceExpression)
        {
            return this.ValueFromObject((object)null, type, false, sourceExpression);
        }

        internal SqlMember Member(SqlExpression expr, MetaDataMember member)
        {
            return new SqlMember(member.Type, this.Default(member), expr, member.Member);
        }

        internal SqlMember Member(SqlExpression expr, MemberInfo member)
        {
            Type memberType = TypeSystem.GetMemberType(member);
            MetaType metaType = this.model.GetMetaType(member.DeclaringType);
            MemberInfo member1 = member;
            MetaDataMember dataMember = metaType.GetDataMember(member1);
            if (metaType != null && dataMember != null)
                return new SqlMember(memberType, this.Default(dataMember), expr, member);
            return new SqlMember(memberType, this.Default(memberType), expr, member);
        }

        internal SqlExpression TypeCase(Type clrType, MetaType rowType, SqlExpression discriminator, IEnumerable<SqlTypeCaseWhen> whens, Expression sourceExpression)
        {
            return (SqlExpression)new SqlTypeCase(clrType, this.typeProvider.From(clrType), rowType, discriminator, whens, sourceExpression);
        }

        internal SqlNew New(MetaType type, ConstructorInfo cons, IEnumerable<SqlExpression> args, IEnumerable<MemberInfo> argMembers, IEnumerable<SqlMemberAssign> bindings, Expression sourceExpression)
        {
            return new SqlNew(type, this.typeProvider.From(type.Type), cons, args, argMembers, bindings, sourceExpression);
        }

        internal SqlMethodCall MethodCall(MethodInfo method, SqlExpression obj, SqlExpression[] args, Expression sourceExpression)
        {
            return new SqlMethodCall(method.ReturnType, this.Default(method.ReturnType), method, obj, (IEnumerable<SqlExpression>)args, sourceExpression);
        }

        internal SqlMethodCall MethodCall(Type returnType, MethodInfo method, SqlExpression obj, SqlExpression[] args, Expression sourceExpression)
        {
            return new SqlMethodCall(returnType, this.Default(returnType), method, obj, (IEnumerable<SqlExpression>)args, sourceExpression);
        }

        internal SqlExprSet ExprSet(SqlExpression[] exprs, Expression sourceExpression)
        {
            return new SqlExprSet(exprs[0].ClrType, (IEnumerable<SqlExpression>)exprs, sourceExpression);
        }

        internal SqlSubSelect SubSelect(SqlNodeType nt, SqlSelect select)
        {
            return this.SubSelect(nt, select, (Type)null);
        }

        internal SqlSubSelect SubSelect(SqlNodeType nt, SqlSelect select, Type clrType)
        {
            ProviderType sqlType = (ProviderType)null;
            if (nt <= SqlNodeType.Exists)
            {
                if (nt != SqlNodeType.Element)
                {
                    if (nt == SqlNodeType.Exists)
                    {
                        clrType = typeof(bool);
                        sqlType = this.typeProvider.From(typeof(bool));
                        goto label_10;
                    }
                    else
                        goto label_10;
                }
            }
            else if (nt != SqlNodeType.Multiset)
            {
                if (nt != SqlNodeType.ScalarSubSelect)
                    goto label_10;
            }
            else
            {
                if (clrType == (Type)null)
                {
                    Type type = typeof(List<>);
                    Type[] typeArray = new Type[1];
                    int index = 0;
                    Type clrType1 = select.Selection.ClrType;
                    typeArray[index] = clrType1;
                    clrType = type.MakeGenericType(typeArray);
                }
                sqlType = this.typeProvider.GetApplicationType(1);
                goto label_10;
            }
            clrType = select.Selection.ClrType;
            sqlType = select.Selection.SqlType;
            label_10:
            return new SqlSubSelect(nt, clrType, sqlType, select);
        }

        internal SqlDoNotVisitExpression DoNotVisitExpression(SqlExpression expr)
        {
            return new SqlDoNotVisitExpression(expr);
        }

        internal SqlFunctionCall FunctionCall(Type clrType, string name, IEnumerable<SqlExpression> args, Expression source)
        {
            return new SqlFunctionCall(clrType, this.Default(clrType), name, args, source);
        }

        internal SqlFunctionCall FunctionCall(Type clrType, ProviderType sqlType, string name, IEnumerable<SqlExpression> args, Expression source)
        {
            return new SqlFunctionCall(clrType, sqlType, name, args, source);
        }

        internal SqlTableValuedFunctionCall TableValuedFunctionCall(MetaType rowType, Type clrType, string name, IEnumerable<SqlExpression> args, Expression source)
        {
            return new SqlTableValuedFunctionCall(rowType, clrType, this.Default(clrType), name, args, source);
        }

        internal ProviderType Default(Type clrType)
        {
            return this.typeProvider.From(clrType);
        }

        internal ProviderType Default(MetaDataMember member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (member.DbType != null)
                return this.typeProvider.Parse(member.DbType);
            return this.typeProvider.From(member.Type);
        }

        internal SqlJoin MakeJoin(SqlJoinType joinType, SqlSource location, SqlAlias alias, SqlExpression condition, Expression source)
        {
            if (joinType == SqlJoinType.LeftOuter)
            {
                SqlSelect sqlSelect = alias.Node as SqlSelect;
                //if (sqlSelect != null && sqlSelect.Selection != null && sqlSelect.Selection.NodeType != SqlNodeType.OptionalValue)
                //    sqlSelect.Selection = (SqlExpression)new SqlOptionalValue((SqlExpression)new SqlColumn("test", (SqlExpression)this.Unary(SqlNodeType.OuterJoinedValue, this.Value(typeof(int?), this.typeProvider.From(typeof(int)), (object)1, false, source))), sqlSelect.Selection);
            }
            return new SqlJoin(joinType, location, (SqlSource)alias, condition, source);
        }
    }

    internal static class SqlNodeTypeOperators
    {
        internal static bool IsPredicateUnaryOperator(this SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.Sum:
                case SqlNodeType.ValueOf:
                case SqlNodeType.OuterJoinedValue:
                case SqlNodeType.Stddev:
                case SqlNodeType.Min:
                case SqlNodeType.Negate:
                case SqlNodeType.LongCount:
                case SqlNodeType.Max:
                case SqlNodeType.Convert:
                case SqlNodeType.Count:
                case SqlNodeType.Avg:
                case SqlNodeType.BitNot:
                case SqlNodeType.ClrLength:
                    return false;
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.IsNull:
                    return true;
                default:
                    throw Error.UnexpectedNode((object)nodeType);
            }
        }

        internal static bool IsUnaryOperatorExpectingPredicateOperand(this SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.Sum:
                case SqlNodeType.ValueOf:
                case SqlNodeType.OuterJoinedValue:
                case SqlNodeType.Stddev:
                case SqlNodeType.Min:
                case SqlNodeType.Negate:
                case SqlNodeType.LongCount:
                case SqlNodeType.Max:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.IsNull:
                case SqlNodeType.Convert:
                case SqlNodeType.Count:
                case SqlNodeType.Avg:
                case SqlNodeType.BitNot:
                case SqlNodeType.ClrLength:
                    return false;
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                    return true;
                default:
                    throw Error.UnexpectedNode((object)nodeType);
            }
        }

        internal static bool IsPredicateBinaryOperator(this SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.Or:
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.LE:
                case SqlNodeType.And:
                    return true;
                case SqlNodeType.Sub:
                case SqlNodeType.Mod:
                case SqlNodeType.Mul:
                case SqlNodeType.Coalesce:
                case SqlNodeType.Concat:
                case SqlNodeType.Div:
                case SqlNodeType.Add:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                    return false;
                default:
                    throw Error.UnexpectedNode((object)nodeType);
            }
        }

        internal static bool IsComparisonOperator(this SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.Or:
                case SqlNodeType.Sub:
                case SqlNodeType.Mod:
                case SqlNodeType.Mul:
                case SqlNodeType.Coalesce:
                case SqlNodeType.Concat:
                case SqlNodeType.Div:
                case SqlNodeType.Add:
                case SqlNodeType.And:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                    return false;
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.LE:
                    return true;
                default:
                    throw Error.UnexpectedNode((object)nodeType);
            }
        }

        internal static bool IsBinaryOperatorExpectingPredicateOperands(this SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.Or:
                case SqlNodeType.And:
                    return true;
                case SqlNodeType.Sub:
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.Mod:
                case SqlNodeType.Mul:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.LE:
                case SqlNodeType.Coalesce:
                case SqlNodeType.Concat:
                case SqlNodeType.Div:
                case SqlNodeType.Add:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                    return false;
                default:
                    throw Error.UnexpectedNode((object)nodeType);
            }
        }

        internal static bool IsClientAidedExpression(this SqlExpression expr)
        {
            switch (expr.NodeType)
            {
                case SqlNodeType.Multiset:
                case SqlNodeType.New:
                case SqlNodeType.TypeCase:
                case SqlNodeType.ClientQuery:
                case SqlNodeType.Element:
                case SqlNodeType.Link:
                    return true;
                default:
                    return false;
            }
        }
    }

    internal abstract class TypeSystemProvider
    {
        internal abstract ProviderType PredictTypeForUnary(SqlNodeType unaryOp, ProviderType operandType);

        internal abstract ProviderType PredictTypeForBinary(SqlNodeType binaryOp, ProviderType leftType, ProviderType rightType);

        internal abstract ProviderType From(Type runtimeType);

        internal abstract ProviderType From(object o);

        internal abstract ProviderType From(Type type, int? size);

        internal abstract ProviderType Parse(string text);

        internal abstract ProviderType GetApplicationType(int index);

        internal abstract ProviderType MostPreciseTypeInFamily(ProviderType type);

        internal abstract ProviderType GetBestLargeType(ProviderType type);

        internal abstract ProviderType GetBestType(ProviderType typeA, ProviderType typeB);

        internal abstract ProviderType ReturnTypeOfFunction(SqlFunctionCall functionCall);

        internal abstract ProviderType ChangeTypeFamilyTo(ProviderType type, ProviderType typeWithFamily);

        internal abstract void InitializeParameter(ProviderType type, DbParameter parameter, object value);
    }
}
