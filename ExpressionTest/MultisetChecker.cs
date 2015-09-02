using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class MultisetChecker
    {
        internal static bool HasMultiset(SqlExpression expr)
        {
            MultisetChecker.Visitor visitor = new MultisetChecker.Visitor();
            SqlExpression sqlExpression = expr;
            visitor.Visit((SqlNode)sqlExpression);
            return visitor.foundMultiset;
        }

        private class Visitor : SqlVisitor
        {
            internal bool foundMultiset;

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
                this.foundMultiset = true;
                return (SqlExpression)sms;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem)
            {
                return (SqlExpression)elem;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                return (SqlExpression)cq;
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss)
            {
                return (SqlExpression)ss;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                return (SqlExpression)ss;
            }
        }
    }
}
