using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlBinary : SqlSimpleTypeExpression
    {
        private SqlExpression left;
        private SqlExpression right;
        private MethodInfo method;

        internal SqlExpression Left
        {
            get
            {
                return this.left;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.left = value;
            }
        }

        internal SqlExpression Right
        {
            get
            {
                return this.right;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.right = value;
            }
        }

        internal MethodInfo Method
        {
            get
            {
                return this.method;
            }
        }

        internal SqlBinary(SqlNodeType nt, Type clrType, ProviderType sqlType, SqlExpression left, SqlExpression right)
          : this(nt, clrType, sqlType, left, right, (MethodInfo)null)
        {
        }

        internal SqlBinary(SqlNodeType nt, Type clrType, ProviderType sqlType, SqlExpression left, SqlExpression right, MethodInfo method)
          : base(nt, clrType, sqlType, right.SourceExpression)
        {
            switch (nt)
            {
                case SqlNodeType.Or:
                case SqlNodeType.Sub:
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.Mod:
                case SqlNodeType.Mul:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.LE:
                case SqlNodeType.Coalesce:
                case SqlNodeType.Concat:
                case SqlNodeType.Div:
                case SqlNodeType.Add:
                case SqlNodeType.And:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                    this.Left = left;
                    this.Right = right;
                    this.method = method;
                    break;
                default:
                    throw Error.UnexpectedNode((object)nt);
            }
        }
    }
}
