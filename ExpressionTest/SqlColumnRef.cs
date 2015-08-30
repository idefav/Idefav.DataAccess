using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlColumnRef : SqlExpression
    {
        private SqlColumn column;

        internal SqlColumn Column
        {
            get
            {
                return this.column;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.column.SqlType;
            }
        }

        internal SqlColumnRef(SqlColumn col)
          : base(SqlNodeType.ColumnRef, col.ClrType, col.SourceExpression)
        {
            this.column = col;
        }

        public override bool Equals(object obj)
        {
            SqlColumnRef sqlColumnRef = obj as SqlColumnRef;
            if (sqlColumnRef != null)
                return sqlColumnRef.Column == this.column;
            return false;
        }

        public override int GetHashCode()
        {
            return this.column.GetHashCode();
        }

        internal SqlColumn GetRootColumn()
        {
            SqlColumn sqlColumn = this.column;
            while (sqlColumn.Expression != null && sqlColumn.Expression.NodeType == SqlNodeType.ColumnRef)
                sqlColumn = ((SqlColumnRef)sqlColumn.Expression).Column;
            return sqlColumn;
        }
    }
}
