using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlNop : SqlSimpleTypeExpression
    {
        internal SqlNop(Type clrType, ProviderType sqlType, Expression sourceExpression)
          : base(SqlNodeType.Nop, clrType, sqlType, sourceExpression)
        {
        }
    }
}
