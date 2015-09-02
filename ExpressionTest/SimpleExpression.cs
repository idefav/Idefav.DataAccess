﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class SimpleExpression
    {
        internal static bool IsSimple(SqlExpression expr)
        {
            SimpleExpression.Visitor visitor = new SimpleExpression.Visitor();
            SqlExpression sqlExpression = expr;
            visitor.Visit((SqlNode)sqlExpression);
            return visitor.IsSimple;
        }

        private class Visitor : SqlVisitor
        {
            private bool isSimple = true;

            internal bool IsSimple
            {
                get
                {
                    return this.isSimple;
                }
            }

            internal override SqlNode Visit(SqlNode node)
            {
                if (node == null)
                    return (SqlNode)null;
                if (!this.isSimple)
                    return node;
                switch (node.NodeType)
                {
                    case SqlNodeType.Add:
                    case SqlNodeType.And:
                    case SqlNodeType.Between:
                    case SqlNodeType.BitAnd:
                    case SqlNodeType.BitNot:
                    case SqlNodeType.BitOr:
                    case SqlNodeType.BitXor:
                    case SqlNodeType.ClrLength:
                    case SqlNodeType.ColumnRef:
                    case SqlNodeType.Concat:
                    case SqlNodeType.Convert:
                    case SqlNodeType.Div:
                    case SqlNodeType.ExprSet:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.FunctionCall:
                    case SqlNodeType.IsNotNull:
                    case SqlNodeType.IsNull:
                    case SqlNodeType.LE:
                    case SqlNodeType.Like:
                    case SqlNodeType.LT:
                    case SqlNodeType.GE:
                    case SqlNodeType.GT:
                    case SqlNodeType.Member:
                    case SqlNodeType.Mod:
                    case SqlNodeType.Mul:
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.Negate:
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                    case SqlNodeType.Or:
                    case SqlNodeType.OptionalValue:
                    case SqlNodeType.OuterJoinedValue:
                    case SqlNodeType.Parameter:
                    case SqlNodeType.SearchedCase:
                    case SqlNodeType.SimpleCase:
                    case SqlNodeType.Sub:
                    case SqlNodeType.Treat:
                    case SqlNodeType.TypeCase:
                    case SqlNodeType.Variable:
                    case SqlNodeType.Value:
                    case SqlNodeType.ValueOf:
                        return base.Visit(node);
                    default:
                        this.isSimple = false;
                        return node;
                }
            }
        }
    }
}