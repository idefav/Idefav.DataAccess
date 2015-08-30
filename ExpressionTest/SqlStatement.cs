using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class SqlStatement : SqlNode
    {
        internal SqlStatement(SqlNodeType nodeType, Expression sourceExpression)
          : base(nodeType, sourceExpression)
        {
        }
    }
}
