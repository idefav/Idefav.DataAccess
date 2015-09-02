using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class BigJoinChecker
    {
        internal static bool CanBigJoin(SqlSelect select)
        {
            BigJoinChecker.Visitor visitor = new BigJoinChecker.Visitor();
            SqlSelect sqlSelect = select;
            visitor.Visit((SqlNode)sqlSelect);
            return visitor.canBigJoin;
        }

        private class Visitor : SqlVisitor
        {
            internal bool canBigJoin = true;

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
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

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                this.canBigJoin = ((this.canBigJoin ? 1 : 0) & (select.GroupBy.Count != 0 || select.Top != null ? 0 : (!select.IsDistinct ? 1 : 0))) != 0;
                if (!this.canBigJoin)
                    return select;
                return base.VisitSelect(select);
            }
        }
    }
}
