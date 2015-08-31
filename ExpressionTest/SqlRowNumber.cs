using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlRowNumber : SqlSimpleTypeExpression
    {
        private List<SqlOrderExpression> orderBy;

        internal List<SqlOrderExpression> OrderBy
        {
            get
            {
                return this.orderBy;
            }
        }

        internal SqlRowNumber(Type clrType, ProviderType sqlType, List<SqlOrderExpression> orderByList, Expression sourceExpression)
          : base(SqlNodeType.RowNumber, clrType, sqlType, sourceExpression)
        {
            if (orderByList == null)
                throw Error.ArgumentNull("orderByList");
            this.orderBy = orderByList;
        }
    }
}
