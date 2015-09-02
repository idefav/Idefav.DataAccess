using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlTypeConverter : SqlVisitor
    {
        protected SqlFactory sql;

        internal SqlTypeConverter(SqlFactory sql)
        {
            this.sql = sql;
        }

        private bool StringConversionIsSafe(ProviderType oldSqlType, ProviderType newSqlType)
        {
            if (this.BothTypesAreStrings(oldSqlType, newSqlType) && newSqlType.HasSizeOrIsLarge)
                return this.OldWillFitInNew(oldSqlType, newSqlType);
            return true;
        }

        private bool StringConversionIsNeeded(ProviderType oldSqlType, ProviderType newSqlType)
        {
            if (!this.BothTypesAreStrings(oldSqlType, newSqlType))
                return true;
            bool flag = oldSqlType.IsFixedSize || newSqlType.IsFixedSize;
            if (!newSqlType.HasSizeOrIsLarge)
                return true;
            if (this.OldWillFitInNew(oldSqlType, newSqlType))
                return flag;
            return false;
        }

        private bool OldWillFitInNew(ProviderType oldSqlType, ProviderType newSqlType)
        {
            if (newSqlType.IsLargeType || !newSqlType.HasSizeOrIsLarge)
                return true;
            if (oldSqlType.IsLargeType || !oldSqlType.HasSizeOrIsLarge || !newSqlType.HasSizeOrIsLarge)
                return false;
            int? size1 = newSqlType.Size;
            int? size2 = oldSqlType.Size;
            if (size1.GetValueOrDefault() < size2.GetValueOrDefault())
                return false;
            return size1.HasValue & size2.HasValue;
        }

        private bool BothTypesAreStrings(ProviderType oldSqlType, ProviderType newSqlType)
        {
            if (oldSqlType.IsSameTypeFamily(this.sql.TypeProvider.From(typeof(string))))
                return newSqlType.IsSameTypeFamily(this.sql.TypeProvider.From(typeof(string)));
            return false;
        }

        internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
        {
            uo.Operand = this.VisitExpression(uo.Operand);
            if (uo.NodeType != SqlNodeType.Convert)
                return (SqlExpression)uo;
            ProviderType sqlType1 = uo.Operand.SqlType;
            ProviderType sqlType2 = uo.SqlType;
            Type nonNullableType1 = TypeSystem.GetNonNullableType(uo.Operand.ClrType);
            Type nonNullableType2 = TypeSystem.GetNonNullableType(uo.ClrType);
            if (nonNullableType2 == typeof(char))
            {
                if (nonNullableType1 == typeof(bool))
                    throw Error.ConvertToCharFromBoolNotSupported();
                if (sqlType1.IsNumeric)
                {
                    SqlFactory sqlFactory = this.sql;
                    Type clrType = uo.ClrType;
                    string name = "NCHAR";
                    SqlExpression[] sqlExpressionArray = new SqlExpression[1];
                    int index = 0;
                    SqlExpression operand = uo.Operand;
                    sqlExpressionArray[index] = operand;
                    Expression sourceExpression = uo.SourceExpression;
                    return (SqlExpression)sqlFactory.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, sourceExpression);
                }
                if (!this.StringConversionIsSafe(sqlType1, sqlType2))
                    throw Error.UnsafeStringConversion((object)sqlType1.ToQueryString(), (object)sqlType2.ToQueryString());
                if (this.StringConversionIsNeeded(sqlType1, sqlType2))
                    uo.SetSqlType(this.sql.TypeProvider.From(uo.ClrType, sqlType1.HasSizeOrIsLarge ? sqlType1.Size : new int?()));
            }
            else
            {
                if (nonNullableType1 == typeof(char) && (sqlType1.IsChar || sqlType1.IsString) && sqlType2.IsNumeric)
                {
                    SqlFactory sqlFactory = this.sql;
                    Type clrType = nonNullableType2;
                    ProviderType sqlType3 = this.sql.TypeProvider.From(typeof(int));
                    string name = "UNICODE";
                    SqlExpression[] sqlExpressionArray = new SqlExpression[1];
                    int index = 0;
                    SqlExpression operand = uo.Operand;
                    sqlExpressionArray[index] = operand;
                    Expression sourceExpression = uo.SourceExpression;
                    return (SqlExpression)sqlFactory.FunctionCall(clrType, sqlType3, name, (IEnumerable<SqlExpression>)sqlExpressionArray, sourceExpression);
                }
                if (nonNullableType2 == typeof(string))
                {
                    if (nonNullableType1 == typeof(double))
                        return this.ConvertDoubleToString(uo.Operand, uo.ClrType);
                    if (nonNullableType1 == typeof(bool))
                        return this.ConvertBitToString(uo.Operand, uo.ClrType);
                    if (!this.StringConversionIsSafe(sqlType1, sqlType2))
                        throw Error.UnsafeStringConversion((object)sqlType1.ToQueryString(), (object)sqlType2.ToQueryString());
                    if (this.StringConversionIsNeeded(sqlType1, sqlType2))
                        uo.SetSqlType(this.sql.TypeProvider.From(uo.ClrType, sqlType1.HasSizeOrIsLarge ? sqlType1.Size : new int?()));
                }
            }
            return (SqlExpression)uo;
        }

        private SqlExpression ConvertDoubleToString(SqlExpression expr, Type resultClrType)
        {
            SqlFactory sqlFactory1 = this.sql;
            Type clrType1 = typeof(void);
            string name1 = "NVARCHAR";
            SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
            int index1 = 0;
            SqlExpression sqlExpression1 = this.sql.ValueFromObject((object)30, false, expr.SourceExpression);
            sqlExpressionArray1[index1] = sqlExpression1;
            Expression sourceExpression1 = expr.SourceExpression;
            SqlExpression sqlExpression2 = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name1, (IEnumerable<SqlExpression>)sqlExpressionArray1, sourceExpression1);
            SqlFactory sqlFactory2 = this.sql;
            Type clrType2 = resultClrType;
            string name2 = "CONVERT";
            SqlExpression[] sqlExpressionArray2 = new SqlExpression[3];
            int index2 = 0;
            SqlExpression sqlExpression3 = sqlExpression2;
            sqlExpressionArray2[index2] = sqlExpression3;
            int index3 = 1;
            SqlExpression sqlExpression4 = expr;
            sqlExpressionArray2[index3] = sqlExpression4;
            int index4 = 2;
            SqlExpression sqlExpression5 = this.sql.ValueFromObject((object)2, false, expr.SourceExpression);
            sqlExpressionArray2[index4] = sqlExpression5;
            Expression sourceExpression2 = expr.SourceExpression;
            return (SqlExpression)sqlFactory2.FunctionCall(clrType2, name2, (IEnumerable<SqlExpression>)sqlExpressionArray2, sourceExpression2);
        }

        private SqlExpression ConvertBitToString(SqlExpression expr, Type resultClrType)
        {
            Type clrType = resultClrType;
            SqlWhen[] sqlWhenArray = new SqlWhen[1];
            int index = 0;
            SqlWhen sqlWhen = new SqlWhen(expr, this.sql.ValueFromObject((object)true.ToString(), false, expr.SourceExpression));
            sqlWhenArray[index] = sqlWhen;
            SqlExpression @else = this.sql.ValueFromObject((object)false.ToString(), false, expr.SourceExpression);
            Expression sourceExpression = expr.SourceExpression;
            return (SqlExpression)new SqlSearchedCase(clrType, (IEnumerable<SqlWhen>)sqlWhenArray, @else, sourceExpression);
        }
    }
}
