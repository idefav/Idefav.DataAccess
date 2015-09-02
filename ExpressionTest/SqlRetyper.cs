using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlRetyper
    {
        private SqlRetyper.Visitor visitor;

        internal SqlRetyper(TypeSystemProvider typeProvider, MetaModel model)
        {
            this.visitor = new SqlRetyper.Visitor(typeProvider, model);
        }

        internal SqlNode Retype(SqlNode node)
        {
            return this.visitor.Visit(node);
        }

        private class Visitor : SqlVisitor
        {
            private TypeSystemProvider typeProvider;
            private SqlFactory sql;

            internal Visitor(TypeSystemProvider typeProvider, MetaModel model)
            {
                this.sql = new SqlFactory(typeProvider, model);
                this.typeProvider = typeProvider;
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                return base.VisitColumn(col);
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                base.VisitUnaryOperator(uo);
                if (uo.NodeType != SqlNodeType.Convert && uo.Operand != null && uo.Operand.SqlType != (ProviderType)null)
                    uo.SetSqlType(this.typeProvider.PredictTypeForUnary(uo.NodeType, uo.Operand.SqlType));
                return (SqlExpression)uo;
            }

            private static bool CanDbConvert(Type from, Type to)
            {
                from = TypeSystem.GetNonNullableType(from);
                to = TypeSystem.GetNonNullableType(to);
                if (from == to || to.IsAssignableFrom(from))
                    return true;
                TypeCode typeCode1 = Type.GetTypeCode(to);
                TypeCode typeCode2 = Type.GetTypeCode(from);
                switch (typeCode1)
                {
                    case TypeCode.Int16:
                        if (typeCode2 != TypeCode.Byte)
                            return typeCode2 == TypeCode.SByte;
                        return true;
                    case TypeCode.UInt16:
                        if (typeCode2 != TypeCode.Byte)
                            return typeCode2 == TypeCode.SByte;
                        return true;
                    case TypeCode.Int32:
                        if (typeCode2 != TypeCode.Byte && typeCode2 != TypeCode.SByte && typeCode2 != TypeCode.Int16)
                            return typeCode2 == TypeCode.UInt16;
                        return true;
                    case TypeCode.UInt32:
                        if (typeCode2 != TypeCode.Byte && typeCode2 != TypeCode.SByte && typeCode2 != TypeCode.Int16)
                            return typeCode2 == TypeCode.UInt16;
                        return true;
                    case TypeCode.Int64:
                        if (typeCode2 != TypeCode.Byte && typeCode2 != TypeCode.SByte && (typeCode2 != TypeCode.Int16 && typeCode2 != TypeCode.UInt16) && typeCode2 != TypeCode.Int32)
                            return typeCode2 == TypeCode.UInt32;
                        return true;
                    case TypeCode.UInt64:
                        if (typeCode2 != TypeCode.Byte && typeCode2 != TypeCode.SByte && (typeCode2 != TypeCode.Int16 && typeCode2 != TypeCode.UInt16) && typeCode2 != TypeCode.Int32)
                            return typeCode2 == TypeCode.UInt32;
                        return true;
                    case TypeCode.Double:
                        return typeCode2 == TypeCode.Single;
                    case TypeCode.Decimal:
                        if (typeCode2 != TypeCode.Single)
                            return typeCode2 == TypeCode.Double;
                        return true;
                    default:
                        return false;
                }
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                base.VisitBinaryOperator(bo);
                if (SqlNodeTypeOperators.IsComparisonOperator(bo.NodeType) && bo.Left.ClrType != typeof(bool) && bo.Right.ClrType != typeof(bool))
                {
                    if (bo.Left.NodeType == SqlNodeType.Convert)
                    {
                        SqlUnary sqlUnary = (SqlUnary)bo.Left;
                        if (SqlRetyper.Visitor.CanDbConvert(sqlUnary.Operand.ClrType, bo.Right.ClrType) && sqlUnary.Operand.SqlType.ComparePrecedenceTo(bo.Right.SqlType) != 1)
                            return this.VisitBinaryOperator(new SqlBinary(bo.NodeType, bo.ClrType, bo.SqlType, sqlUnary.Operand, bo.Right));
                    }
                    if (bo.Right.NodeType == SqlNodeType.Convert)
                    {
                        SqlUnary sqlUnary = (SqlUnary)bo.Right;
                        if (SqlRetyper.Visitor.CanDbConvert(sqlUnary.Operand.ClrType, bo.Left.ClrType) && sqlUnary.Operand.SqlType.ComparePrecedenceTo(bo.Left.SqlType) != 1)
                            return this.VisitBinaryOperator(new SqlBinary(bo.NodeType, bo.ClrType, bo.SqlType, bo.Left, sqlUnary.Operand));
                    }
                }
                if (bo.Right != null && bo.NodeType != SqlNodeType.Concat)
                {
                    SqlExpression left = bo.Left;
                    SqlExpression right = bo.Right;
                    this.CoerceBinaryArgs(ref left, ref right);
                    if (bo.Left != left || bo.Right != right)
                        bo = this.sql.Binary(bo.NodeType, left, right);
                    bo.SetSqlType(this.typeProvider.PredictTypeForBinary(bo.NodeType, left.SqlType, right.SqlType));
                }
                if (SqlNodeTypeOperators.IsComparisonOperator(bo.NodeType))
                {
                    Func<SqlExpression, SqlExpression, bool> func = (Func<SqlExpression, SqlExpression, bool>)((expr, val) =>
                    {
                        if ((val.NodeType == SqlNodeType.Value || val.NodeType == SqlNodeType.ClientParameter) && (expr.NodeType != SqlNodeType.Value && expr.NodeType != SqlNodeType.ClientParameter) && val.SqlType.IsUnicodeType)
                            return !expr.SqlType.IsUnicodeType;
                        return false;
                    });
                    SqlSimpleTypeExpression simpleTypeExpression1 = (SqlSimpleTypeExpression)null;
                    if (func(bo.Left, bo.Right))
                        simpleTypeExpression1 = (SqlSimpleTypeExpression)bo.Right;
                    else if (func(bo.Right, bo.Left))
                        simpleTypeExpression1 = (SqlSimpleTypeExpression)bo.Left;
                    if (simpleTypeExpression1 != null)
                    {
                        SqlSimpleTypeExpression simpleTypeExpression2 = simpleTypeExpression1;
                        ProviderType unicodeEquivalent = simpleTypeExpression2.SqlType.GetNonUnicodeEquivalent();
                        simpleTypeExpression2.SetSqlType(unicodeEquivalent);
                    }
                }
                return (SqlExpression)bo;
            }

            internal override SqlExpression VisitIn(SqlIn sin)
            {
                SqlExpression expression = sin.Expression;
                bool flag = false;
                List<SqlExpression> list = new List<SqlExpression>(sin.Values.Count);
                ProviderType rightType = (ProviderType)null;
                int index = 0;
                for (int count = sin.Values.Count; index < count; ++index)
                {
                    SqlExpression sqlExpression = sin.Values[index];
                    this.CoerceBinaryArgs(ref expression, ref sqlExpression);
                    if (sqlExpression != sin.Values[index])
                    {
                        rightType = (ProviderType)null == rightType ? sqlExpression.SqlType : this.typeProvider.PredictTypeForBinary(SqlNodeType.EQ, sqlExpression.SqlType, rightType);
                        flag = true;
                    }
                    list.Add(sqlExpression);
                }
                if (expression != sin.Expression)
                    flag = true;
                if (flag)
                {
                    ProviderType sqlType = this.typeProvider.PredictTypeForBinary(SqlNodeType.EQ, expression.SqlType, rightType);
                    sin = new SqlIn(sin.ClrType, sqlType, expression, (IEnumerable<SqlExpression>)list, sin.SourceExpression);
                }
                return (SqlExpression)sin;
            }

            internal override SqlExpression VisitLike(SqlLike like)
            {
                base.VisitLike(like);
                if (!like.Expression.SqlType.IsUnicodeType && like.Pattern.SqlType.IsUnicodeType && (like.Pattern.NodeType == SqlNodeType.Value || like.Pattern.NodeType == SqlNodeType.ClientParameter))
                {
                    SqlSimpleTypeExpression simpleTypeExpression = (SqlSimpleTypeExpression)like.Pattern;
                    ProviderType unicodeEquivalent = simpleTypeExpression.SqlType.GetNonUnicodeEquivalent();
                    simpleTypeExpression.SetSqlType(unicodeEquivalent);
                }
                return (SqlExpression)like;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                base.VisitScalarSubSelect(ss);
                SqlSubSelect sqlSubSelect = ss;
                ProviderType sqlType = sqlSubSelect.Select.Selection.SqlType;
                sqlSubSelect.SetSqlType(sqlType);
                return (SqlExpression)ss;
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
            {
                base.VisitSearchedCase(c);
                ProviderType type = c.Whens[0].Value.SqlType;
                for (int index = 1; index < c.Whens.Count; ++index)
                {
                    ProviderType sqlType = c.Whens[index].Value.SqlType;
                    type = this.typeProvider.GetBestType(type, sqlType);
                }
                if (c.Else != null)
                {
                    ProviderType sqlType = c.Else.SqlType;
                    type = this.typeProvider.GetBestType(type, sqlType);
                }
                List<SqlWhen> whens = c.Whens;
                foreach (SqlWhen sqlWhen in System.Linq.Enumerable.Where<SqlWhen>((IEnumerable<SqlWhen>)whens, (Func<SqlWhen, bool>)(w =>
                {
                    if (w.Value.SqlType != type)
                        return !w.Value.SqlType.IsRuntimeOnlyType;
                    return false;
                })))
                    sqlWhen.Value = (SqlExpression)this.sql.UnaryConvert(sqlWhen.Value.ClrType, type, sqlWhen.Value, sqlWhen.Value.SourceExpression);
                if (c.Else != null && c.Else.SqlType != type && !c.Else.SqlType.IsRuntimeOnlyType)
                    c.Else = (SqlExpression)this.sql.UnaryConvert(c.Else.ClrType, type, c.Else, c.Else.SourceExpression);
                return (SqlExpression)c;
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                base.VisitSimpleCase(c);
                ProviderType type = c.Whens[0].Value.SqlType;
                for (int index = 1; index < c.Whens.Count; ++index)
                {
                    ProviderType sqlType = c.Whens[index].Value.SqlType;
                    type = this.typeProvider.GetBestType(type, sqlType);
                }
                List<SqlWhen> whens = c.Whens;
                foreach (SqlWhen sqlWhen in System.Linq.Enumerable.Where<SqlWhen>((IEnumerable<SqlWhen>)whens, (Func<SqlWhen, bool>)(w =>
                {
                    if (w.Value.SqlType != type)
                        return !w.Value.SqlType.IsRuntimeOnlyType;
                    return false;
                })))
                    sqlWhen.Value = (SqlExpression)this.sql.UnaryConvert(sqlWhen.Value.ClrType, type, sqlWhen.Value, sqlWhen.Value.SourceExpression);
                return (SqlExpression)c;
            }

            internal override SqlStatement VisitAssign(SqlAssign sa)
            {
                base.VisitAssign(sa);
                SqlExpression rvalue = sa.RValue;
                this.CoerceToFirst(sa.LValue, ref rvalue);
                sa.RValue = rvalue;
                return (SqlStatement)sa;
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                int index = 0;
                for (int count = fc.Arguments.Count; index < count; ++index)
                    fc.Arguments[index] = this.VisitExpression(fc.Arguments[index]);
                if (fc.Arguments.Count > 0 && fc.Arguments[0].SqlType != (ProviderType)null)
                {
                    ProviderType type = this.typeProvider.ReturnTypeOfFunction(fc);
                    if (type != (ProviderType)null)
                        fc.SetSqlType(type);
                }
                return (SqlExpression)fc;
            }

            private void CoerceToFirst(SqlExpression arg1, ref SqlExpression arg2)
            {
                if (!(arg1.SqlType != (ProviderType)null) || !(arg2.SqlType != (ProviderType)null))
                    return;
                if (arg2.NodeType == SqlNodeType.Value)
                {
                    SqlValue sqlValue = (SqlValue)arg2;
                    arg2 = this.sql.Value(arg1.ClrType, arg1.SqlType, DBConvert.ChangeType(sqlValue.Value, arg1.ClrType), sqlValue.IsClientSpecified, arg2.SourceExpression);
                }
                else if (arg2.NodeType == SqlNodeType.ClientParameter && arg2.SqlType != arg1.SqlType)
                {
                    ((SqlSimpleTypeExpression)arg2).SetSqlType(arg1.SqlType);
                }
                else
                {
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    //SqlExpression & local = @arg2;
                    SqlFactory sqlFactory = this.sql;
                    Type clrType = arg1.ClrType;
                    ProviderType sqlType = arg1.SqlType;
                    SqlExpression expression = arg2;
                    Expression sourceExpression = expression.SourceExpression;
                    SqlUnary sqlUnary = sqlFactory.UnaryConvert(clrType, sqlType, expression, sourceExpression);
          // ISSUE: explicit reference operation
          //^ local = (SqlExpression)sqlUnary;
                }
            }

            private void CoerceBinaryArgs(ref SqlExpression arg1, ref SqlExpression arg2)
            {
                if (arg1.SqlType == (ProviderType)null || arg2.SqlType == (ProviderType)null)
                    return;
                if (arg1.SqlType.IsSameTypeFamily(arg2.SqlType))
                {
                    this.CoerceTypeFamily(arg1, arg2);
                }
                else
                {
                    if (!(arg1.ClrType != typeof(bool)) || !(arg2.ClrType != typeof(bool)))
                        return;
                    this.CoerceTypes(ref arg1, ref arg2);
                }
            }

            private void CoerceTypeFamily(SqlExpression arg1, SqlExpression arg2)
            {
                if (arg1.SqlType.HasPrecisionAndScale && arg2.SqlType.HasPrecisionAndScale && arg1.SqlType != arg2.SqlType || (SqlFactory.IsSqlHighPrecisionDateTimeType(arg1) || SqlFactory.IsSqlHighPrecisionDateTimeType(arg2)))
                {
                    ProviderType bestType = this.typeProvider.GetBestType(arg1.SqlType, arg2.SqlType);
                    SqlRetyper.Visitor.SetSqlTypeIfSimpleExpression(arg1, bestType);
                    SqlRetyper.Visitor.SetSqlTypeIfSimpleExpression(arg2, bestType);
                }
                else if (SqlFactory.IsSqlDateType(arg1) && !SqlFactory.IsSqlHighPrecisionDateTimeType(arg2))
                {
                    SqlRetyper.Visitor.SetSqlTypeIfSimpleExpression(arg2, arg1.SqlType);
                }
                else
                {
                    if (!SqlFactory.IsSqlDateType(arg2) || SqlFactory.IsSqlHighPrecisionDateTimeType(arg1))
                        return;
                    SqlRetyper.Visitor.SetSqlTypeIfSimpleExpression(arg1, arg2.SqlType);
                }
            }

            private static void SetSqlTypeIfSimpleExpression(SqlExpression expression, ProviderType sqlType)
            {
                SqlSimpleTypeExpression simpleTypeExpression = expression as SqlSimpleTypeExpression;
                if (simpleTypeExpression == null)
                    return;
                simpleTypeExpression.SetSqlType(sqlType);
            }

            private void CoerceTypes(ref SqlExpression arg1, ref SqlExpression arg2)
            {
                if (arg2.NodeType == SqlNodeType.Value)
                    arg2 = this.CoerceValueForExpression((SqlValue)arg2, arg1);
                else if (arg1.NodeType == SqlNodeType.Value)
                    arg1 = this.CoerceValueForExpression((SqlValue)arg1, arg2);
                else if (arg2.NodeType == SqlNodeType.ClientParameter && arg2.SqlType != arg1.SqlType)
                    ((SqlSimpleTypeExpression)arg2).SetSqlType(arg1.SqlType);
                else if (arg1.NodeType == SqlNodeType.ClientParameter && arg1.SqlType != arg2.SqlType)
                {
                    ((SqlSimpleTypeExpression)arg1).SetSqlType(arg2.SqlType);
                }
                else
                {
                    int num = arg1.SqlType.ComparePrecedenceTo(arg2.SqlType);
                    if (num > 0)
                    {
                        // ISSUE: explicit reference operation
                        // ISSUE: variable of a reference type
                        //SqlExpression & local = @arg2;
                        SqlFactory sqlFactory = this.sql;
                        Type clrType = arg1.ClrType;
                        ProviderType sqlType = arg1.SqlType;
                        SqlExpression expression = arg2;
                        Expression sourceExpression = expression.SourceExpression;
                        SqlUnary sqlUnary = sqlFactory.UnaryConvert(clrType, sqlType, expression, sourceExpression);
            // ISSUE: explicit reference operation
            //^ local = (SqlExpression)sqlUnary;
                    }
                    else
                    {
                        if (num >= 0)
                            return;
                        // ISSUE: explicit reference operation
                        // ISSUE: variable of a reference type
                       // SqlExpression & local = @arg1;
                        SqlFactory sqlFactory = this.sql;
                        Type clrType = arg2.ClrType;
                        ProviderType sqlType = arg2.SqlType;
                        SqlExpression expression = arg1;
                        Expression sourceExpression = expression.SourceExpression;
                        SqlUnary sqlUnary = sqlFactory.UnaryConvert(clrType, sqlType, expression, sourceExpression);
            // ISSUE: explicit reference operation
            //^ local = (SqlExpression)sqlUnary;
                    }
                }
            }

            private SqlExpression CoerceValueForExpression(SqlValue value, SqlExpression expression)
            {
                object obj = value.Value;
                if (!value.ClrType.IsAssignableFrom(expression.ClrType))
                    obj = DBConvert.ChangeType(obj, expression.ClrType);
                ProviderType sqlType = this.typeProvider.ChangeTypeFamilyTo(value.SqlType, expression.SqlType);
                return this.sql.Value(expression.ClrType, sqlType, obj, value.IsClientSpecified, value.SourceExpression);
            }
        }
    }
}
