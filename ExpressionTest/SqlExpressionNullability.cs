using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class SqlExpressionNullability
    {
        internal static bool? CanBeNull(SqlExpression expr)
        {
            switch (expr.NodeType)
            {
                case SqlNodeType.SimpleCase:
                    return SqlExpressionNullability.CanBeNull(Enumerable.Select<SqlWhen, SqlExpression>((IEnumerable<SqlWhen>)((SqlSimpleCase)expr).Whens, (Func<SqlWhen, SqlExpression>)(w => w.Value)));
                case SqlNodeType.Sub:
                case SqlNodeType.Mod:
                case SqlNodeType.Mul:
                case SqlNodeType.Add:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
                case SqlNodeType.Concat:
                case SqlNodeType.Div:
                    SqlBinary sqlBinary = (SqlBinary)expr;
                    bool? nullable1 = SqlExpressionNullability.CanBeNull(sqlBinary.Left);
                    bool? nullable2 = SqlExpressionNullability.CanBeNull(sqlBinary.Right);
                    bool? nullable3 = nullable1;
                    bool flag1 = false;
                    int num;
                    if ((nullable3.GetValueOrDefault() == flag1 ? (!nullable3.HasValue ? 1 : 0) : 1) == 0)
                    {
                        bool? nullable4 = nullable2;
                        bool flag2 = false;
                        num = nullable4.GetValueOrDefault() == flag2 ? (!nullable4.HasValue ? 1 : 0) : 1;
                    }
                    else
                        num = 1;
                    return new bool?(num != 0);
                case SqlNodeType.Value:
                    return new bool?(((SqlValue)expr).Value == null);
                case SqlNodeType.Grouping:
                case SqlNodeType.Multiset:
                case SqlNodeType.New:
                case SqlNodeType.Exists:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.IsNull:
                case SqlNodeType.DiscriminatedType:
                    return new bool?(false);
                case SqlNodeType.Negate:
                case SqlNodeType.BitNot:
                    return SqlExpressionNullability.CanBeNull(((SqlUnary)expr).Operand);
                case SqlNodeType.OuterJoinedValue:
                    return new bool?(true);
                case SqlNodeType.ExprSet:
                    return SqlExpressionNullability.CanBeNull((IEnumerable<SqlExpression>)((SqlExprSet)expr).Expressions);
                case SqlNodeType.Lift:
                    return SqlExpressionNullability.CanBeNull(((SqlLift)expr).Expression);
                case SqlNodeType.Column:
                    SqlColumn sqlColumn = (SqlColumn)expr;
                    if (sqlColumn.MetaMember != null)
                        return new bool?(sqlColumn.MetaMember.CanBeNull);
                    if (sqlColumn.Expression != null)
                        return SqlExpressionNullability.CanBeNull(sqlColumn.Expression);
                    return new bool?();
                case SqlNodeType.ColumnRef:
                    return SqlExpressionNullability.CanBeNull((SqlExpression)((SqlColumnRef)expr).Column);
                default:
                    return new bool?();
            }
        }

        private static bool? CanBeNull(IEnumerable<SqlExpression> exprs)
        {
            bool flag1 = false;
            foreach (SqlExpression expr in exprs)
            {
                bool? nullable1 = SqlExpressionNullability.CanBeNull(expr);
                bool? nullable2 = nullable1;
                bool flag2 = true;
                if ((nullable2.GetValueOrDefault() == flag2 ? (nullable2.HasValue ? 1 : 0) : 0) != 0)
                    return new bool?(true);
                if (!nullable1.HasValue)
                    flag1 = true;
            }
            if (flag1)
                return new bool?();
            return new bool?(false);
        }
    }
}
