using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlUnary : SqlSimpleTypeExpression
    {
        private SqlExpression operand;
        private MethodInfo method;

        internal SqlExpression Operand
        {
            get
            {
                return this.operand;
            }
            set
            {
                if (value == null && this.NodeType != SqlNodeType.Count && this.NodeType != SqlNodeType.LongCount)
                    throw Error.ArgumentNull("value");
                this.operand = value;
            }
        }

        internal MethodInfo Method
        {
            get
            {
                return this.method;
            }
        }

        internal SqlUnary(SqlNodeType nt, Type clrType, ProviderType sqlType, SqlExpression expr, Expression sourceExpression)
          : this(nt, clrType, sqlType, expr, (MethodInfo)null, sourceExpression)
        {
        }

        internal SqlUnary(SqlNodeType nt, Type clrType, ProviderType sqlType, SqlExpression expr, MethodInfo method, Expression sourceExpression)
          : base(nt, clrType, sqlType, sourceExpression)
        {
            switch (nt)
            {
                case SqlNodeType.Treat:
                case SqlNodeType.ValueOf:
                case SqlNodeType.Stddev:
                case SqlNodeType.Sum:
                case SqlNodeType.Negate:
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                case SqlNodeType.OuterJoinedValue:
                case SqlNodeType.Max:
                case SqlNodeType.Min:
                case SqlNodeType.IsNull:
                case SqlNodeType.LongCount:
                case SqlNodeType.Count:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.ClrLength:
                case SqlNodeType.Convert:
                case SqlNodeType.Avg:
                case SqlNodeType.BitNot:
                    this.Operand = expr;
                    this.method = method;
                    break;
                default:
                    throw Error.UnexpectedNode((object)nt);
            }
        }
    }
}
