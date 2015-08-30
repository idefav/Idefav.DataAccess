using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class SqlSource : SqlNode
    {
        internal SqlSource(SqlNodeType nt, Expression sourceExpression)
          : base(nt, sourceExpression)
        {
        }
    }
}
