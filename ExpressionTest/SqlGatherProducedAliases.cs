using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlGatherProducedAliases
    {
        internal static HashSet<SqlAlias> Gather(SqlNode node)
        {
            SqlGatherProducedAliases.Gatherer gatherer = new SqlGatherProducedAliases.Gatherer();
            SqlNode node1 = node;
            gatherer.Visit(node1);
            return gatherer.Produced;
        }

        private class Gatherer : SqlVisitor
        {
            internal HashSet<SqlAlias> Produced = new HashSet<SqlAlias>();

            internal override SqlAlias VisitAlias(SqlAlias a)
            {
                this.Produced.Add(a);
                return base.VisitAlias(a);
            }
        }
    }
}
