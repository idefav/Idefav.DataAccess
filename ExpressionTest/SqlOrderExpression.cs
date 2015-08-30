using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlOrderExpression : IEquatable<SqlOrderExpression>
    {
        private SqlOrderType orderType;
        private SqlExpression expression;

        internal SqlOrderType OrderType
        {
            get
            {
                return this.orderType;
            }
            set
            {
                this.orderType = value;
            }
        }

        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.expression != null && !this.expression.ClrType.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType((object)"value", (object)this.expression.ClrType, (object)value.ClrType);
                this.expression = value;
            }
        }

        internal SqlOrderExpression(SqlOrderType type, SqlExpression expr)
        {
            this.OrderType = type;
            this.Expression = expr;
        }

        public override bool Equals(object obj)
        {
            if (this.EqualsTo(obj as SqlOrderExpression))
                return true;
            return base.Equals(obj);
        }

        public bool Equals(SqlOrderExpression other)
        {
            if (this.EqualsTo(other))
                return true;
            return base.Equals((object)other);
        }

        private bool EqualsTo(SqlOrderExpression other)
        {
            if (other == null)
                return false;
            if (this == other)
                return true;
            if (this.OrderType != other.OrderType || !this.Expression.SqlType.Equals((object)other.Expression.SqlType))
                return false;
            SqlColumn sqlColumn1 = SqlOrderExpression.UnwrapColumn(this.Expression);
            SqlColumn sqlColumn2 = SqlOrderExpression.UnwrapColumn(other.Expression);
            if (sqlColumn1 == null || sqlColumn2 == null)
                return false;
            return sqlColumn1 == sqlColumn2;
        }

        public override int GetHashCode()
        {
            SqlColumn sqlColumn = SqlOrderExpression.UnwrapColumn(this.Expression);
            if (sqlColumn != null)
                return sqlColumn.GetHashCode();
            return base.GetHashCode();
        }

        private static SqlColumn UnwrapColumn(SqlExpression expr)
        {
            SqlUnary sqlUnary = expr as SqlUnary;
            if (sqlUnary != null)
                expr = sqlUnary.Operand;
            SqlColumn sqlColumn = expr as SqlColumn;
            if (sqlColumn != null)
                return sqlColumn;
            SqlColumnRef sqlColumnRef = expr as SqlColumnRef;
            if (sqlColumnRef != null)
                return sqlColumnRef.GetRootColumn();
            return (SqlColumn)null;
        }
    }

    internal enum SqlOrderType
    {
        Ascending,
        Descending,
    }
}
