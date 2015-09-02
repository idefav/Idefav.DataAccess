using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlAliaser
    {
        private SqlAliaser.Visitor visitor;

        internal SqlAliaser()
        {
            this.visitor = new SqlAliaser.Visitor();
        }

        internal SqlNode AssociateColumnsWithAliases(SqlNode node)
        {
            return this.visitor.Visit(node);
        }

        private class Visitor : SqlVisitor
        {
            private SqlAlias alias;

            internal Visitor()
            {
            }

            internal override SqlAlias VisitAlias(SqlAlias sqlAlias)
            {
                SqlAlias sqlAlias1 = this.alias;
                this.alias = sqlAlias;
                sqlAlias.Node = this.Visit(sqlAlias.Node);
                this.alias = sqlAlias1;
                return sqlAlias;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                foreach (SqlColumn sqlColumn in row.Columns)
                    sqlColumn.Alias = this.alias;
                return base.VisitRow(row);
            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                foreach (SqlColumn sqlColumn in tab.Columns)
                    sqlColumn.Alias = this.alias;
                return base.VisitTable(tab);
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
            {
                foreach (SqlColumn sqlColumn in fc.Columns)
                    sqlColumn.Alias = this.alias;
                return base.VisitTableValuedFunctionCall(fc);
            }
        }
    }
}
