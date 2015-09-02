using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlBooleanizer
    {
        internal static SqlNode Rationalize(SqlNode node, TypeSystemProvider typeProvider, MetaModel model)
        {
            return new SqlBooleanizer.Booleanizer(typeProvider, model).Visit(node);
        }

        private class Booleanizer : SqlBooleanMismatchVisitor
        {
            private SqlFactory sql;

            internal Booleanizer(TypeSystemProvider typeProvider, MetaModel model)
            {
                this.sql = new SqlFactory(typeProvider, model);
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                if (select.Where != null && select.Where.NodeType == SqlNodeType.Coalesce)
                {
                    SqlBinary sqlBinary = (SqlBinary)select.Where;
                    if (sqlBinary.Right.NodeType == SqlNodeType.Value)
                    {
                        SqlValue sqlValue = (SqlValue)sqlBinary.Right;
                        if (sqlValue.Value != null && sqlValue.Value.GetType() == typeof(bool) && !(bool)sqlValue.Value)
                            select.Where = sqlBinary.Left;
                    }
                }
                return base.VisitSelect(select);
            }

            internal override SqlExpression ConvertValueToPredicate(SqlExpression valueExpression)
            {
                return (SqlExpression)new SqlBinary(SqlNodeType.EQ, valueExpression.ClrType, this.sql.TypeProvider.From(typeof(bool)), valueExpression, this.sql.Value(typeof(bool), valueExpression.SqlType, (object)true, false, valueExpression.SourceExpression));
            }

            internal override SqlExpression ConvertPredicateToValue(SqlExpression predicateExpression)
            {
                SqlExpression sqlExpression1 = this.sql.ValueFromObject((object)true, false, predicateExpression.SourceExpression);
                SqlExpression sqlExpression2 = this.sql.ValueFromObject((object)false, false, predicateExpression.SourceExpression);
                bool? nullable = SqlExpressionNullability.CanBeNull(predicateExpression);
                bool flag = false;
                if ((nullable.GetValueOrDefault() == flag ? (!nullable.HasValue ? 1 : 0) : 1) != 0)
                {
                    SqlExpression sqlExpression3 = this.sql.Value(sqlExpression1.ClrType, sqlExpression1.SqlType, (object)null, false, predicateExpression.SourceExpression);
                    Type clrType1 = predicateExpression.ClrType;
                    SqlWhen[] sqlWhenArray = new SqlWhen[2];
                    int index1 = 0;
                    SqlWhen sqlWhen1 = new SqlWhen(predicateExpression, sqlExpression1);
                    sqlWhenArray[index1] = sqlWhen1;
                    int index2 = 1;
                    int num = 62;
                    Type clrType2 = predicateExpression.ClrType;
                    ProviderType sqlType = predicateExpression.SqlType;
                    SqlExpression expr = predicateExpression;
                    Expression sourceExpression1 = expr.SourceExpression;
                    SqlWhen sqlWhen2 = new SqlWhen((SqlExpression)new SqlUnary((SqlNodeType)num, clrType2, sqlType, expr, sourceExpression1), sqlExpression2);
                    sqlWhenArray[index2] = sqlWhen2;
                    SqlExpression @else = sqlExpression3;
                    Expression sourceExpression2 = predicateExpression.SourceExpression;
                    return (SqlExpression)new SqlSearchedCase(clrType1, (IEnumerable<SqlWhen>)sqlWhenArray, @else, sourceExpression2);
                }
                Type clrType = predicateExpression.ClrType;
                SqlWhen[] sqlWhenArray1 = new SqlWhen[1];
                int index = 0;
                SqlWhen sqlWhen = new SqlWhen(predicateExpression, sqlExpression1);
                sqlWhenArray1[index] = sqlWhen;
                SqlExpression else1 = sqlExpression2;
                Expression sourceExpression = predicateExpression.SourceExpression;
                return (SqlExpression)new SqlSearchedCase(clrType, (IEnumerable<SqlWhen>)sqlWhenArray1, else1, sourceExpression);
            }
        }
    }
}
