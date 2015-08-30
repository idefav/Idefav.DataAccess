using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class SqlSimpleTypeExpression : SqlExpression
    {
        private ProviderType sqlType;

        internal override ProviderType SqlType
        {
            get
            {
                return this.sqlType;
            }
        }

        internal SqlSimpleTypeExpression(SqlNodeType nodeType, Type clrType, ProviderType sqlType, Expression sourceExpression)
          : base(nodeType, clrType, sourceExpression)
        {
            this.sqlType = sqlType;
        }

        internal void SetSqlType(ProviderType type)
        {
            this.sqlType = type;
        }
    }
}
