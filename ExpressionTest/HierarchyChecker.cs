using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class HierarchyChecker
    {
        internal static bool HasHierarchy(SqlExpression expr)
        {
            HierarchyChecker.Visitor visitor = new HierarchyChecker.Visitor();
            SqlExpression sqlExpression = expr;
            visitor.Visit((SqlNode)sqlExpression);
            return visitor.foundHierarchy;
        }

        private class Visitor : SqlVisitor
        {
            internal bool foundHierarchy;

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
                this.foundHierarchy = true;
                return (SqlExpression)sms;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem)
            {
                this.foundHierarchy = true;
                return (SqlExpression)elem;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                this.foundHierarchy = true;
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
