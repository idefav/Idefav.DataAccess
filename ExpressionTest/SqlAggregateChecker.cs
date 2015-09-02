using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlAggregateChecker
    {
        private SqlAggregateChecker.Visitor visitor;

        internal SqlAggregateChecker()
        {
            this.visitor = new SqlAggregateChecker.Visitor();
        }

        internal bool HasAggregates(SqlNode node)
        {
            this.visitor.hasAggregates = false;
            this.visitor.Visit(node);
            return this.visitor.hasAggregates;
        }

        private class Visitor : SqlVisitor
        {
            internal bool hasAggregates;

            internal Visitor()
            {
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return (SqlExpression)ss;
            }

            internal override SqlSource VisitSource(SqlSource source)
            {
                return source;
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                switch (uo.NodeType)
                {
                    case SqlNodeType.Max:
                    case SqlNodeType.Min:
                    case SqlNodeType.Sum:
                    case SqlNodeType.Avg:
                    case SqlNodeType.Count:
                    case SqlNodeType.LongCount:
                        this.hasAggregates = true;
                        return (SqlExpression)uo;
                    default:
                        return base.VisitUnaryOperator(uo);
                }
            }
        }
    }
}
