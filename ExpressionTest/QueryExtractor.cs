using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class QueryExtractor
    {
        internal static SqlClientQuery Extract(SqlSubSelect subquery, IEnumerable<SqlParameter> parentParameters)
        {
            SqlClientQuery sqlClientQuery = new SqlClientQuery(subquery);
            if (parentParameters != null)
                sqlClientQuery.Parameters.AddRange(parentParameters);
            QueryExtractor.Visitor visitor = new QueryExtractor.Visitor(sqlClientQuery.Arguments, sqlClientQuery.Parameters);
            sqlClientQuery.Query = (SqlSubSelect)visitor.Visit((SqlNode)subquery);
            return sqlClientQuery;
        }

        private class Visitor : SqlDuplicator.DuplicatingVisitor
        {
            private List<SqlExpression> externals;
            private List<SqlParameter> parameters;

            internal Visitor(List<SqlExpression> externals, List<SqlParameter> parameters)
              : base(true)
            {
                this.externals = externals;
                this.parameters = parameters;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                SqlExpression expr = base.VisitColumnRef(cref);
                if (expr == cref)
                    return this.ExtractParameter(expr);
                return expr;
            }

            internal override SqlExpression VisitUserColumn(SqlUserColumn suc)
            {
                SqlExpression expr = base.VisitUserColumn(suc);
                if (expr == suc)
                    return this.ExtractParameter(expr);
                return expr;
            }

            private SqlExpression ExtractParameter(SqlExpression expr)
            {
                Type clrType1 = expr.ClrType;
                if (expr.ClrType.IsValueType && !TypeSystem.IsNullableType(expr.ClrType))
                {
                    Type type = typeof(Nullable<>);
                    Type[] typeArray = new Type[1];
                    int index = 0;
                    Type clrType2 = expr.ClrType;
                    typeArray[index] = clrType2;
                    clrType1 = type.MakeGenericType(typeArray);
                }
                this.externals.Add(expr);
                SqlParameter sqlParameter = new SqlParameter(clrType1, expr.SqlType, "@x" + (object)(this.parameters.Count + 1), expr.SourceExpression);
                this.parameters.Add(sqlParameter);
                return (SqlExpression)sqlParameter;
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                SqlExpression[] sqlExpressionArray = new SqlExpression[link.KeyExpressions.Count];
                int index = 0;
                for (int length = sqlExpressionArray.Length; index < length; ++index)
                    sqlExpressionArray[index] = this.VisitExpression(link.KeyExpressions[index]);
                return (SqlNode)new SqlLink(new object(), link.RowType, link.ClrType, link.SqlType, (SqlExpression)null, link.Member, (IEnumerable<SqlExpression>)sqlExpressionArray, (SqlExpression)null, link.SourceExpression);
            }
        }
    }
}
