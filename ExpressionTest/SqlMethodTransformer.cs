using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlMethodTransformer : SqlVisitor
    {
        protected SqlFactory sql;

        internal SqlMethodTransformer(SqlFactory sql)
        {
            this.sql = sql;
        }

        internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
        {
            SqlExpression first = base.VisitFunctionCall(fc);
            if (first is SqlFunctionCall)
            {
                SqlFunctionCall sqlFunctionCall = (SqlFunctionCall)first;
                if (sqlFunctionCall.Name == "LEN")
                {
                    SqlExpression expr = sqlFunctionCall.Arguments[0];
                    if (expr.SqlType.IsLargeType && !expr.SqlType.SupportsLength)
                    {
                        first = this.sql.DATALENGTH(expr);
                        if (expr.SqlType.IsUnicodeType)
                            first = this.sql.ConvertToInt(this.sql.Divide(first, this.sql.ValueFromObject((object)2, expr.SourceExpression)));
                    }
                }
                Type closestRuntimeType = sqlFunctionCall.SqlType.GetClosestRuntimeType();
                bool flag = SqlMethodTransformer.SkipConversionForDateAdd(sqlFunctionCall.Name, sqlFunctionCall.ClrType, closestRuntimeType);
                if (sqlFunctionCall.ClrType != closestRuntimeType && !flag)
                    first = this.sql.ConvertTo(sqlFunctionCall.ClrType, (SqlExpression)sqlFunctionCall);
            }
            return first;
        }

        internal override SqlExpression VisitUnaryOperator(SqlUnary fc)
        {
            SqlExpression sqlExpression1 = base.VisitUnaryOperator(fc);
            if (sqlExpression1 is SqlUnary)
            {
                SqlUnary sqlUnary = (SqlUnary)sqlExpression1;
                if (sqlUnary.NodeType == SqlNodeType.ClrLength)
                {
                    SqlExpression operand = sqlUnary.Operand;
                    SqlExpression sqlExpression2 = this.sql.DATALENGTH(operand);
                    if (operand.SqlType.IsUnicodeType)
                        sqlExpression2 = this.sql.Divide(sqlExpression2, this.sql.ValueFromObject((object)2, operand.SourceExpression));
                    sqlExpression1 = this.sql.ConvertToInt(sqlExpression2);
                }
            }
            return sqlExpression1;
        }

        private static bool SkipConversionForDateAdd(string functionName, Type expected, Type actual)
        {
            if (string.Compare(functionName, "DATEADD", StringComparison.OrdinalIgnoreCase) != 0 || !(expected == typeof(DateTime)))
                return false;
            return actual == typeof(DateTimeOffset);
        }
    }
}
