using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class SqlAliasesReferenced
    {
        internal static bool ReferencesAny(SqlNode node, IEnumerable<SqlAlias> aliases)
        {
            SqlAliasesReferenced.Visitor visitor = new SqlAliasesReferenced.Visitor();
            visitor.aliases = aliases;
            SqlNode node1 = node;
            visitor.Visit(node1);
            return visitor.referencesAnyMatchingAliases;
        }

        private class Visitor : SqlVisitor
        {
            internal IEnumerable<SqlAlias> aliases;
            internal bool referencesAnyMatchingAliases;

            internal override SqlNode Visit(SqlNode node)
            {
                if (this.referencesAnyMatchingAliases)
                    return node;
                return base.Visit(node);
            }

            internal SqlAlias VisitAliasConsumed(SqlAlias a)
            {
                if (a == null)
                    return a;
                bool flag = false;
                foreach (SqlAlias sqlAlias in this.aliases)
                {
                    if (sqlAlias == a)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                    this.referencesAnyMatchingAliases = true;
                return a;
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
                this.VisitExpression(cref.Column.Expression);
                return (SqlExpression)cref;
            }
        }
    }
}
