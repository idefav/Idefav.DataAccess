using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class SqlExpression : SqlNode
    {
        private Type clrType;

        internal Type ClrType
        {
            get
            {
                return this.clrType;
            }
        }

        internal abstract ProviderType SqlType { get; }

        internal bool IsConstantColumn
        {
            get
            {
                if (this.NodeType == SqlNodeType.Column)
                {
                    SqlColumn sqlColumn = (SqlColumn)this;
                    if (sqlColumn.Expression != null)
                        return sqlColumn.Expression.IsConstantColumn;
                }
                else
                {
                    if (this.NodeType == SqlNodeType.ColumnRef)
                        return ((SqlColumnRef)this).Column.IsConstantColumn;
                    if (this.NodeType == SqlNodeType.OptionalValue)
                        return ((SqlOptionalValue)this).Value.IsConstantColumn;
                    if (this.NodeType == SqlNodeType.Value || this.NodeType == SqlNodeType.Parameter)
                        return true;
                }
                return false;
            }
        }

        internal SqlExpression(SqlNodeType nodeType, Type clrType, Expression sourceExpression)
          : base(nodeType, sourceExpression)
        {
            this.clrType = clrType;
        }

        internal void SetClrType(Type type)
        {
            this.clrType = type;
        }
    }
}
