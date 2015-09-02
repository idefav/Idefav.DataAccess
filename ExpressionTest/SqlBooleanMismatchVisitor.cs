using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class SqlBooleanMismatchVisitor : SqlVisitor
    {
        internal SqlBooleanMismatchVisitor()
        {
        }

        internal abstract SqlExpression ConvertValueToPredicate(SqlExpression valueExpression);

        internal abstract SqlExpression ConvertPredicateToValue(SqlExpression predicateExpression);

        internal override SqlSelect VisitSelect(SqlSelect select)
        {
            select.From = this.VisitSource(select.From);
            select.Where = this.VisitPredicate(select.Where);
            int index1 = 0;
            for (int count = select.GroupBy.Count; index1 < count; ++index1)
                select.GroupBy[index1] = this.VisitExpression(select.GroupBy[index1]);
            select.Having = this.VisitPredicate(select.Having);
            int index2 = 0;
            for (int count = select.OrderBy.Count; index2 < count; ++index2)
                select.OrderBy[index2].Expression = this.VisitExpression(select.OrderBy[index2].Expression);
            select.Top = this.VisitExpression(select.Top);
            select.Row = (SqlRow)this.Visit((SqlNode)select.Row);
            return select;
        }

        internal override SqlSource VisitJoin(SqlJoin join)
        {
            join.Left = this.VisitSource(join.Left);
            join.Right = this.VisitSource(join.Right);
            join.Condition = this.VisitPredicate(join.Condition);
            return (SqlSource)join;
        }

        internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
        {
            uo.Operand = !SqlNodeTypeOperators.IsUnaryOperatorExpectingPredicateOperand(uo.NodeType) ? this.VisitExpression(uo.Operand) : this.VisitPredicate(uo.Operand);
            return (SqlExpression)uo;
        }

        internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
        {
            if (SqlNodeTypeOperators.IsBinaryOperatorExpectingPredicateOperands(bo.NodeType))
            {
                bo.Left = this.VisitPredicate(bo.Left);
                bo.Right = this.VisitPredicate(bo.Right);
            }
            else
            {
                bo.Left = this.VisitExpression(bo.Left);
                bo.Right = this.VisitExpression(bo.Right);
            }
            return (SqlExpression)bo;
        }

        internal override SqlStatement VisitAssign(SqlAssign sa)
        {
            sa.LValue = this.VisitExpression(sa.LValue);
            sa.RValue = this.VisitExpression(sa.RValue);
            return (SqlStatement)sa;
        }

        internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
        {
            int index = 0;
            for (int count = c.Whens.Count; index < count; ++index)
            {
                SqlWhen sqlWhen = c.Whens[index];
                sqlWhen.Match = this.VisitPredicate(sqlWhen.Match);
                sqlWhen.Value = this.VisitExpression(sqlWhen.Value);
            }
            c.Else = this.VisitExpression(c.Else);
            return (SqlExpression)c;
        }

        internal override SqlExpression VisitLift(SqlLift lift)
        {
            lift.Expression = base.VisitExpression(lift.Expression);
            return (SqlExpression)lift;
        }

        internal SqlExpression VisitPredicate(SqlExpression exp)
        {
            exp = (SqlExpression)this.Visit((SqlNode)exp);
            if (exp != null && !SqlBooleanMismatchVisitor.IsPredicateExpression(exp))
                exp = this.ConvertValueToPredicate(exp);
            return exp;
        }

        internal override SqlExpression VisitExpression(SqlExpression exp)
        {
            exp = (SqlExpression)this.Visit((SqlNode)exp);
            if (exp != null && SqlBooleanMismatchVisitor.IsPredicateExpression(exp))
                exp = this.ConvertPredicateToValue(exp);
            return exp;
        }

        private static bool IsPredicateExpression(SqlExpression exp)
        {
            switch (exp.NodeType)
            {
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.Exists:
                case SqlNodeType.In:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.IsNull:
                case SqlNodeType.LE:
                case SqlNodeType.Like:
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                case SqlNodeType.Or:
                case SqlNodeType.And:
                case SqlNodeType.Between:
                    return true;
                case SqlNodeType.Lift:
                    return SqlBooleanMismatchVisitor.IsPredicateExpression(((SqlLift)exp).Expression);
                default:
                    return false;
            }
        }
    }
}
