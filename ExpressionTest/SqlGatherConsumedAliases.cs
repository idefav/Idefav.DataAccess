using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlGatherConsumedAliases
    {
        internal static HashSet<SqlAlias> Gather(SqlNode node)
        {
            SqlGatherConsumedAliases.Gatherer gatherer = new SqlGatherConsumedAliases.Gatherer();
            SqlNode node1 = node;
            gatherer.Visit(node1);
            return gatherer.Consumed;
        }

        private class Gatherer : SqlVisitor
        {
            internal HashSet<SqlAlias> Consumed = new HashSet<SqlAlias>();

            internal void VisitAliasConsumed(SqlAlias a)
            {
                this.Consumed.Add(a);
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                this.VisitAliasConsumed(col.Alias);
                this.VisitExpression(col.Expression);
                return (SqlExpression)col;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                this.VisitAliasConsumed(cref.Column.Alias);
                return (SqlExpression)cref;
            }
        }
    }
}
