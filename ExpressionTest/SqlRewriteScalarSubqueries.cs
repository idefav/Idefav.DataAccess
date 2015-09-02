using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlRewriteScalarSubqueries
    {
        private SqlRewriteScalarSubqueries.Visitor visitor;

        internal SqlRewriteScalarSubqueries(SqlFactory sqlFactory)
        {
            this.visitor = new SqlRewriteScalarSubqueries.Visitor(sqlFactory);
        }

        internal SqlNode Rewrite(SqlNode node)
        {
            return this.visitor.Visit(node);
        }

        private class Visitor : SqlVisitor
        {
            private SqlFactory sql;
            private SqlSelect currentSelect;
            private SqlAggregateChecker aggregateChecker;

            internal Visitor(SqlFactory sqlFactory)
            {
                this.sql = sqlFactory;
                this.aggregateChecker = new SqlAggregateChecker();
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                SqlSelect sqlSelect = this.VisitSelect(ss.Select);
                if (!this.aggregateChecker.HasAggregates((SqlNode)sqlSelect))
                    sqlSelect.Top = this.sql.ValueFromObject((object)1, ss.SourceExpression);
                sqlSelect.OrderingType = SqlOrderingType.Blocked;
                this.currentSelect.From = (SqlSource)new SqlJoin(SqlJoinType.OuterApply, this.currentSelect.From, (SqlSource)new SqlAlias((SqlNode)sqlSelect), (SqlExpression)null, ss.SourceExpression);
                return (SqlExpression)new SqlColumnRef(sqlSelect.Row.Columns[0]);
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SqlSelect sqlSelect = this.currentSelect;
                try
                {
                    this.currentSelect = select;
                    return base.VisitSelect(select);
                }
                finally
                {
                    this.currentSelect = sqlSelect;
                }
            }
        }
    }
}
