using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class SqlNode
    {
        private SqlNodeType nodeType;
        private Expression sourceExpression;

        internal Expression SourceExpression
        {
            get
            {
                return this.sourceExpression;
            }
        }

        internal SqlNodeType NodeType
        {
            get
            {
                return this.nodeType;
            }
        }

        internal SqlNode(SqlNodeType nodeType, Expression sourceExpression)
        {
            this.nodeType = nodeType;
            this.sourceExpression = sourceExpression;
        }

        internal void ClearSourceExpression()
        {
            this.sourceExpression = (Expression)null;
        }
    }
}
