using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlRow : SqlNode
    {
        private List<SqlColumn> columns;

        internal List<SqlColumn> Columns
        {
            get
            {
                return this.columns;
            }
        }

        internal SqlRow(Expression sourceExpression)
          : base(SqlNodeType.Row, sourceExpression)
        {
            this.columns = new List<SqlColumn>();
        }

        internal SqlColumn Find(string name)
        {
            foreach (SqlColumn sqlColumn in this.columns)
            {
                if (name == sqlColumn.Name)
                    return sqlColumn;
            }
            return (SqlColumn)null;
        }
    }
}
