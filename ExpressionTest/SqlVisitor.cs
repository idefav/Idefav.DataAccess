using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class SqlVisitor
    {
        private int nDepth;

        internal virtual SqlNode Visit(SqlNode node)
        {
            if (node == null)
                return (SqlNode)null;
            try
            {
                this.nDepth = this.nDepth + 1;
                switch (node.NodeType)
                {
                    case SqlNodeType.Add:
                    case SqlNodeType.And:
                    case SqlNodeType.BitAnd:
                    case SqlNodeType.BitOr:
                    case SqlNodeType.BitXor:
                    case SqlNodeType.Coalesce:
                    case SqlNodeType.Concat:
                    case SqlNodeType.Div:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                    case SqlNodeType.LE:
                    case SqlNodeType.LT:
                    case SqlNodeType.GE:
                    case SqlNodeType.GT:
                    case SqlNodeType.Mod:
                    case SqlNodeType.Mul:
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.Or:
                    case SqlNodeType.Sub:
                        return (SqlNode)this.VisitBinaryOperator((SqlBinary)node);
                    case SqlNodeType.Alias:
                        return (SqlNode)this.VisitAlias((SqlAlias)node);
                    case SqlNodeType.AliasRef:
                        return (SqlNode)this.VisitAliasRef((SqlAliasRef)node);
                    case SqlNodeType.Assign:
                        return (SqlNode)this.VisitAssign((SqlAssign)node);
                    case SqlNodeType.Avg:
                    case SqlNodeType.BitNot:
                    case SqlNodeType.ClrLength:
                    case SqlNodeType.Convert:
                    case SqlNodeType.Count:
                    case SqlNodeType.IsNotNull:
                    case SqlNodeType.IsNull:
                    case SqlNodeType.LongCount:
                    case SqlNodeType.Max:
                    case SqlNodeType.Min:
                    case SqlNodeType.Negate:
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                    case SqlNodeType.OuterJoinedValue:
                    case SqlNodeType.Stddev:
                    case SqlNodeType.Sum:
                    case SqlNodeType.ValueOf:
                        return (SqlNode)this.VisitUnaryOperator((SqlUnary)node);
                    case SqlNodeType.Between:
                        return (SqlNode)this.VisitBetween((SqlBetween)node);
                    case SqlNodeType.Block:
                        return (SqlNode)this.VisitBlock((SqlBlock)node);
                    case SqlNodeType.ClientArray:
                        return (SqlNode)this.VisitClientArray((SqlClientArray)node);
                    case SqlNodeType.ClientCase:
                        return (SqlNode)this.VisitClientCase((SqlClientCase)node);
                    case SqlNodeType.ClientParameter:
                        return (SqlNode)this.VisitClientParameter((SqlClientParameter)node);
                    case SqlNodeType.ClientQuery:
                        return (SqlNode)this.VisitClientQuery((SqlClientQuery)node);
                    case SqlNodeType.Column:
                        return (SqlNode)this.VisitColumn((SqlColumn)node);
                    case SqlNodeType.ColumnRef:
                        return (SqlNode)this.VisitColumnRef((SqlColumnRef)node);
                    case SqlNodeType.Delete:
                        return (SqlNode)this.VisitDelete((SqlDelete)node);
                    case SqlNodeType.DiscriminatedType:
                        return (SqlNode)this.VisitDiscriminatedType((SqlDiscriminatedType)node);
                    case SqlNodeType.DiscriminatorOf:
                        return (SqlNode)this.VisitDiscriminatorOf((SqlDiscriminatorOf)node);
                    case SqlNodeType.DoNotVisit:
                        return (SqlNode)this.VisitDoNotVisit((SqlDoNotVisitExpression)node);
                    case SqlNodeType.Element:
                    case SqlNodeType.Exists:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.ScalarSubSelect:
                        return (SqlNode)this.VisitSubSelect((SqlSubSelect)node);
                    case SqlNodeType.ExprSet:
                        return (SqlNode)this.VisitExprSet((SqlExprSet)node);
                    case SqlNodeType.FunctionCall:
                        return (SqlNode)this.VisitFunctionCall((SqlFunctionCall)node);
                    case SqlNodeType.In:
                        return (SqlNode)this.VisitIn((SqlIn)node);
                    case SqlNodeType.IncludeScope:
                        return this.VisitIncludeScope((SqlIncludeScope)node);
                    case SqlNodeType.Lift:
                        return (SqlNode)this.VisitLift((SqlLift)node);
                    case SqlNodeType.Link:
                        return this.VisitLink((SqlLink)node);
                    case SqlNodeType.Like:
                        return (SqlNode)this.VisitLike((SqlLike)node);
                    case SqlNodeType.Grouping:
                        return (SqlNode)this.VisitGrouping((SqlGrouping)node);
                    case SqlNodeType.Insert:
                        return (SqlNode)this.VisitInsert((SqlInsert)node);
                    case SqlNodeType.Join:
                        return (SqlNode)this.VisitJoin((SqlJoin)node);
                    case SqlNodeType.JoinedCollection:
                        return (SqlNode)this.VisitJoinedCollection((SqlJoinedCollection)node);
                    case SqlNodeType.MethodCall:
                        return (SqlNode)this.VisitMethodCall((SqlMethodCall)node);
                    case SqlNodeType.Member:
                        return this.VisitMember((SqlMember)node);
                    case SqlNodeType.MemberAssign:
                        return (SqlNode)this.VisitMemberAssign((SqlMemberAssign)node);
                    case SqlNodeType.New:
                        return (SqlNode)this.VisitNew((SqlNew)node);
                    case SqlNodeType.Nop:
                        return (SqlNode)this.VisitNop((SqlNop)node);
                    case SqlNodeType.OptionalValue:
                        return (SqlNode)this.VisitOptionalValue((SqlOptionalValue)node);
                    case SqlNodeType.Parameter:
                        return (SqlNode)this.VisitParameter((SqlParameter)node);
                    case SqlNodeType.Row:
                        return (SqlNode)this.VisitRow((SqlRow)node);
                    case SqlNodeType.RowNumber:
                        return (SqlNode)this.VisitRowNumber((SqlRowNumber)node);
                    case SqlNodeType.SearchedCase:
                        return (SqlNode)this.VisitSearchedCase((SqlSearchedCase)node);
                    case SqlNodeType.Select:
                        return (SqlNode)this.VisitSelect((SqlSelect)node);
                    case SqlNodeType.SharedExpression:
                        return (SqlNode)this.VisitSharedExpression((SqlSharedExpression)node);
                    case SqlNodeType.SharedExpressionRef:
                        return (SqlNode)this.VisitSharedExpressionRef((SqlSharedExpressionRef)node);
                    case SqlNodeType.SimpleCase:
                        return (SqlNode)this.VisitSimpleCase((SqlSimpleCase)node);
                    case SqlNodeType.SimpleExpression:
                        return (SqlNode)this.VisitSimpleExpression((SqlSimpleExpression)node);
                    case SqlNodeType.StoredProcedureCall:
                        return (SqlNode)this.VisitStoredProcedureCall((SqlStoredProcedureCall)node);
                    case SqlNodeType.Table:
                        return (SqlNode)this.VisitTable((SqlTable)node);
                    case SqlNodeType.TableValuedFunctionCall:
                        return (SqlNode)this.VisitTableValuedFunctionCall((SqlTableValuedFunctionCall)node);
                    case SqlNodeType.Treat:
                        return (SqlNode)this.VisitTreat((SqlUnary)node);
                    case SqlNodeType.TypeCase:
                        return (SqlNode)this.VisitTypeCase((SqlTypeCase)node);
                    case SqlNodeType.Union:
                        return this.VisitUnion((SqlUnion)node);
                    case SqlNodeType.Update:
                        return (SqlNode)this.VisitUpdate((SqlUpdate)node);
                    case SqlNodeType.UserColumn:
                        return (SqlNode)this.VisitUserColumn((SqlUserColumn)node);
                    case SqlNodeType.UserQuery:
                        return (SqlNode)this.VisitUserQuery((SqlUserQuery)node);
                    case SqlNodeType.UserRow:
                        return (SqlNode)this.VisitUserRow((SqlUserRow)node);
                    case SqlNodeType.Variable:
                        return (SqlNode)this.VisitVariable((SqlVariable)node);
                    case SqlNodeType.Value:
                        return (SqlNode)this.VisitValue((SqlValue)node);
                    default:
                        throw Error.UnexpectedNode((object)node);
                }
            }
            finally
            {
                this.nDepth = this.nDepth - 1;
            }
        }

        [Conditional("DEBUG")]
        internal static void CheckRecursionDepth(int maxLevel, int level)
        {
            if (level > maxLevel)
                throw new Exception("Infinite Descent?");
        }

        internal object Eval(SqlExpression expr)
        {
            if (expr.NodeType == SqlNodeType.Value)
                return ((SqlValue)expr).Value;
            throw Error.UnexpectedNode((object)expr.NodeType);
        }

        internal virtual SqlExpression VisitDoNotVisit(SqlDoNotVisitExpression expr)
        {
            return expr.Expression;
        }

        internal virtual SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
        {
            int index = 0;
            for (int count = rowNumber.OrderBy.Count; index < count; ++index)
                rowNumber.OrderBy[index].Expression = this.VisitExpression(rowNumber.OrderBy[index].Expression);
            return rowNumber;
        }

        internal virtual SqlExpression VisitExpression(SqlExpression exp)
        {
            return (SqlExpression)this.Visit((SqlNode)exp);
        }

        internal virtual SqlSelect VisitSequence(SqlSelect sel)
        {
            return (SqlSelect)this.Visit((SqlNode)sel);
        }

        internal virtual SqlExpression VisitNop(SqlNop nop)
        {
            return (SqlExpression)nop;
        }

        internal virtual SqlExpression VisitLift(SqlLift lift)
        {
            lift.Expression = this.VisitExpression(lift.Expression);
            return (SqlExpression)lift;
        }

        internal virtual SqlExpression VisitUnaryOperator(SqlUnary uo)
        {
            uo.Operand = this.VisitExpression(uo.Operand);
            return (SqlExpression)uo;
        }

        internal virtual SqlExpression VisitBinaryOperator(SqlBinary bo)
        {
            bo.Left = this.VisitExpression(bo.Left);
            bo.Right = this.VisitExpression(bo.Right);
            return (SqlExpression)bo;
        }

        internal virtual SqlAlias VisitAlias(SqlAlias a)
        {
            a.Node = this.Visit(a.Node);
            return a;
        }

        internal virtual SqlExpression VisitAliasRef(SqlAliasRef aref)
        {
            return (SqlExpression)aref;
        }

        internal virtual SqlNode VisitMember(SqlMember m)
        {
            m.Expression = this.VisitExpression(m.Expression);
            return (SqlNode)m;
        }

        internal virtual SqlExpression VisitCast(SqlUnary c)
        {
            c.Operand = this.VisitExpression(c.Operand);
            return (SqlExpression)c;
        }

        internal virtual SqlExpression VisitTreat(SqlUnary t)
        {
            t.Operand = this.VisitExpression(t.Operand);
            return (SqlExpression)t;
        }

        internal virtual SqlTable VisitTable(SqlTable tab)
        {
            return tab;
        }

        internal virtual SqlUserQuery VisitUserQuery(SqlUserQuery suq)
        {
            int index1 = 0;
            for (int count = suq.Arguments.Count; index1 < count; ++index1)
                suq.Arguments[index1] = this.VisitExpression(suq.Arguments[index1]);
            suq.Projection = this.VisitExpression(suq.Projection);
            int index2 = 0;
            for (int count = suq.Columns.Count; index2 < count; ++index2)
                suq.Columns[index2] = (SqlUserColumn)this.Visit((SqlNode)suq.Columns[index2]);
            return suq;
        }

        internal virtual SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
        {
            int index1 = 0;
            for (int count = spc.Arguments.Count; index1 < count; ++index1)
                spc.Arguments[index1] = this.VisitExpression(spc.Arguments[index1]);
            spc.Projection = this.VisitExpression(spc.Projection);
            int index2 = 0;
            for (int count = spc.Columns.Count; index2 < count; ++index2)
                spc.Columns[index2] = (SqlUserColumn)this.Visit((SqlNode)spc.Columns[index2]);
            return spc;
        }

        internal virtual SqlExpression VisitUserColumn(SqlUserColumn suc)
        {
            return (SqlExpression)suc;
        }

        internal virtual SqlExpression VisitUserRow(SqlUserRow row)
        {
            return (SqlExpression)row;
        }

        internal virtual SqlRow VisitRow(SqlRow row)
        {
            int index = 0;
            for (int count = row.Columns.Count; index < count; ++index)
                row.Columns[index].Expression = this.VisitExpression(row.Columns[index].Expression);
            return row;
        }

        internal virtual SqlExpression VisitNew(SqlNew sox)
        {
            int index1 = 0;
            for (int count = sox.Args.Count; index1 < count; ++index1)
                sox.Args[index1] = this.VisitExpression(sox.Args[index1]);
            int index2 = 0;
            for (int count = sox.Members.Count; index2 < count; ++index2)
                sox.Members[index2].Expression = this.VisitExpression(sox.Members[index2].Expression);
            return (SqlExpression)sox;
        }

        internal virtual SqlNode VisitLink(SqlLink link)
        {
            int index = 0;
            for (int count = link.KeyExpressions.Count; index < count; ++index)
                link.KeyExpressions[index] = this.VisitExpression(link.KeyExpressions[index]);
            return (SqlNode)link;
        }

        internal virtual SqlExpression VisitClientQuery(SqlClientQuery cq)
        {
            int index = 0;
            for (int count = cq.Arguments.Count; index < count; ++index)
                cq.Arguments[index] = this.VisitExpression(cq.Arguments[index]);
            return (SqlExpression)cq;
        }

        internal virtual SqlExpression VisitJoinedCollection(SqlJoinedCollection jc)
        {
            jc.Expression = this.VisitExpression(jc.Expression);
            jc.Count = this.VisitExpression(jc.Count);
            return (SqlExpression)jc;
        }

        internal virtual SqlExpression VisitClientArray(SqlClientArray scar)
        {
            int index = 0;
            for (int count = scar.Expressions.Count; index < count; ++index)
                scar.Expressions[index] = this.VisitExpression(scar.Expressions[index]);
            return (SqlExpression)scar;
        }

        internal virtual SqlExpression VisitClientParameter(SqlClientParameter cp)
        {
            return (SqlExpression)cp;
        }

        internal virtual SqlExpression VisitColumn(SqlColumn col)
        {
            col.Expression = this.VisitExpression(col.Expression);
            return (SqlExpression)col;
        }

        internal virtual SqlExpression VisitColumnRef(SqlColumnRef cref)
        {
            return (SqlExpression)cref;
        }

        internal virtual SqlExpression VisitParameter(SqlParameter p)
        {
            return (SqlExpression)p;
        }

        internal virtual SqlExpression VisitValue(SqlValue value)
        {
            return (SqlExpression)value;
        }

        internal virtual SqlExpression VisitSubSelect(SqlSubSelect ss)
        {
            switch (ss.NodeType)
            {
                case SqlNodeType.Multiset:
                    return this.VisitMultiset(ss);
                case SqlNodeType.ScalarSubSelect:
                    return this.VisitScalarSubSelect(ss);
                case SqlNodeType.Element:
                    return this.VisitElement(ss);
                case SqlNodeType.Exists:
                    return this.VisitExists(ss);
                default:
                    throw Error.UnexpectedNode((object)ss.NodeType);
            }
        }

        internal virtual SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
        {
            ss.Select = this.VisitSequence(ss.Select);
            return (SqlExpression)ss;
        }

        internal virtual SqlExpression VisitMultiset(SqlSubSelect sms)
        {
            sms.Select = this.VisitSequence(sms.Select);
            return (SqlExpression)sms;
        }

        internal virtual SqlExpression VisitElement(SqlSubSelect elem)
        {
            elem.Select = this.VisitSequence(elem.Select);
            return (SqlExpression)elem;
        }

        internal virtual SqlExpression VisitExists(SqlSubSelect sqlExpr)
        {
            sqlExpr.Select = this.VisitSequence(sqlExpr.Select);
            return (SqlExpression)sqlExpr;
        }

        internal virtual SqlSource VisitJoin(SqlJoin join)
        {
            join.Left = this.VisitSource(join.Left);
            join.Right = this.VisitSource(join.Right);
            join.Condition = this.VisitExpression(join.Condition);
            return (SqlSource)join;
        }

        internal virtual SqlSource VisitSource(SqlSource source)
        {
            return (SqlSource)this.Visit((SqlNode)source);
        }

        internal virtual SqlSelect VisitSelectCore(SqlSelect select)
        {
            select.From = this.VisitSource(select.From);
            select.Where = this.VisitExpression(select.Where);
            int index1 = 0;
            for (int count = select.GroupBy.Count; index1 < count; ++index1)
                select.GroupBy[index1] = this.VisitExpression(select.GroupBy[index1]);
            select.Having = this.VisitExpression(select.Having);
            int index2 = 0;
            for (int count = select.OrderBy.Count; index2 < count; ++index2)
                select.OrderBy[index2].Expression = this.VisitExpression(select.OrderBy[index2].Expression);
            select.Top = this.VisitExpression(select.Top);
            select.Row = (SqlRow)this.Visit((SqlNode)select.Row);
            return select;
        }

        internal virtual SqlSelect VisitSelect(SqlSelect select)
        {
            select = this.VisitSelectCore(select);
            select.Selection = this.VisitExpression(select.Selection);
            return select;
        }

        internal virtual SqlStatement VisitInsert(SqlInsert insert)
        {
            insert.Table = (SqlTable)this.Visit((SqlNode)insert.Table);
            insert.Expression = this.VisitExpression(insert.Expression);
            insert.Row = (SqlRow)this.Visit((SqlNode)insert.Row);
            return (SqlStatement)insert;
        }

        internal virtual SqlStatement VisitUpdate(SqlUpdate update)
        {
            update.Select = this.VisitSequence(update.Select);
            int index = 0;
            for (int count = update.Assignments.Count; index < count; ++index)
                update.Assignments[index] = (SqlAssign)this.Visit((SqlNode)update.Assignments[index]);
            return (SqlStatement)update;
        }

        internal virtual SqlStatement VisitDelete(SqlDelete delete)
        {
            delete.Select = this.VisitSequence(delete.Select);
            return (SqlStatement)delete;
        }

        internal virtual SqlMemberAssign VisitMemberAssign(SqlMemberAssign ma)
        {
            ma.Expression = this.VisitExpression(ma.Expression);
            return ma;
        }

        internal virtual SqlStatement VisitAssign(SqlAssign sa)
        {
            sa.LValue = this.VisitExpression(sa.LValue);
            sa.RValue = this.VisitExpression(sa.RValue);
            return (SqlStatement)sa;
        }

        internal virtual SqlBlock VisitBlock(SqlBlock b)
        {
            int index = 0;
            for (int count = b.Statements.Count; index < count; ++index)
                b.Statements[index] = (SqlStatement)this.Visit((SqlNode)b.Statements[index]);
            return b;
        }

        internal virtual SqlExpression VisitSearchedCase(SqlSearchedCase c)
        {
            int index = 0;
            for (int count = c.Whens.Count; index < count; ++index)
            {
                SqlWhen sqlWhen = c.Whens[index];
                sqlWhen.Match = this.VisitExpression(sqlWhen.Match);
                sqlWhen.Value = this.VisitExpression(sqlWhen.Value);
            }
            c.Else = this.VisitExpression(c.Else);
            return (SqlExpression)c;
        }

        internal virtual SqlExpression VisitClientCase(SqlClientCase c)
        {
            c.Expression = this.VisitExpression(c.Expression);
            int index = 0;
            for (int count = c.Whens.Count; index < count; ++index)
            {
                SqlClientWhen sqlClientWhen = c.Whens[index];
                sqlClientWhen.Match = this.VisitExpression(sqlClientWhen.Match);
                sqlClientWhen.Value = this.VisitExpression(sqlClientWhen.Value);
            }
            return (SqlExpression)c;
        }

        internal virtual SqlExpression VisitSimpleCase(SqlSimpleCase c)
        {
            c.Expression = this.VisitExpression(c.Expression);
            int index = 0;
            for (int count = c.Whens.Count; index < count; ++index)
            {
                SqlWhen sqlWhen = c.Whens[index];
                sqlWhen.Match = this.VisitExpression(sqlWhen.Match);
                sqlWhen.Value = this.VisitExpression(sqlWhen.Value);
            }
            return (SqlExpression)c;
        }

        internal virtual SqlExpression VisitTypeCase(SqlTypeCase tc)
        {
            tc.Discriminator = this.VisitExpression(tc.Discriminator);
            int index = 0;
            for (int count = tc.Whens.Count; index < count; ++index)
            {
                SqlTypeCaseWhen sqlTypeCaseWhen = tc.Whens[index];
                sqlTypeCaseWhen.Match = this.VisitExpression(sqlTypeCaseWhen.Match);
                sqlTypeCaseWhen.TypeBinding = this.VisitExpression(sqlTypeCaseWhen.TypeBinding);
            }
            return (SqlExpression)tc;
        }

        internal virtual SqlNode VisitUnion(SqlUnion su)
        {
            su.Left = this.Visit(su.Left);
            su.Right = this.Visit(su.Right);
            return (SqlNode)su;
        }

        internal virtual SqlExpression VisitExprSet(SqlExprSet xs)
        {
            int index = 0;
            for (int count = xs.Expressions.Count; index < count; ++index)
                xs.Expressions[index] = this.VisitExpression(xs.Expressions[index]);
            return (SqlExpression)xs;
        }

        internal virtual SqlExpression VisitVariable(SqlVariable v)
        {
            return (SqlExpression)v;
        }

        internal virtual SqlExpression VisitOptionalValue(SqlOptionalValue sov)
        {
            sov.HasValue = this.VisitExpression(sov.HasValue);
            sov.Value = this.VisitExpression(sov.Value);
            return (SqlExpression)sov;
        }

        internal virtual SqlExpression VisitBetween(SqlBetween between)
        {
            between.Expression = this.VisitExpression(between.Expression);
            between.Start = this.VisitExpression(between.Start);
            between.End = this.VisitExpression(between.End);
            return (SqlExpression)between;
        }

        internal virtual SqlExpression VisitIn(SqlIn sin)
        {
            sin.Expression = this.VisitExpression(sin.Expression);
            int index = 0;
            for (int count = sin.Values.Count; index < count; ++index)
                sin.Values[index] = this.VisitExpression(sin.Values[index]);
            return (SqlExpression)sin;
        }

        internal virtual SqlExpression VisitLike(SqlLike like)
        {
            like.Expression = this.VisitExpression(like.Expression);
            like.Pattern = this.VisitExpression(like.Pattern);
            like.Escape = this.VisitExpression(like.Escape);
            return (SqlExpression)like;
        }

        internal virtual SqlExpression VisitFunctionCall(SqlFunctionCall fc)
        {
            int index = 0;
            for (int count = fc.Arguments.Count; index < count; ++index)
                fc.Arguments[index] = this.VisitExpression(fc.Arguments[index]);
            return (SqlExpression)fc;
        }

        internal virtual SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
        {
            int index = 0;
            for (int count = fc.Arguments.Count; index < count; ++index)
                fc.Arguments[index] = this.VisitExpression(fc.Arguments[index]);
            return (SqlExpression)fc;
        }

        internal virtual SqlExpression VisitMethodCall(SqlMethodCall mc)
        {
            mc.Object = this.VisitExpression(mc.Object);
            int index = 0;
            for (int count = mc.Arguments.Count; index < count; ++index)
                mc.Arguments[index] = this.VisitExpression(mc.Arguments[index]);
            return (SqlExpression)mc;
        }

        internal virtual SqlExpression VisitSharedExpression(SqlSharedExpression shared)
        {
            shared.Expression = this.VisitExpression(shared.Expression);
            return (SqlExpression)shared;
        }

        internal virtual SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
        {
            return (SqlExpression)sref;
        }

        internal virtual SqlExpression VisitSimpleExpression(SqlSimpleExpression simple)
        {
            simple.Expression = this.VisitExpression(simple.Expression);
            return (SqlExpression)simple;
        }

        internal virtual SqlExpression VisitGrouping(SqlGrouping g)
        {
            g.Key = this.VisitExpression(g.Key);
            g.Group = this.VisitExpression(g.Group);
            return (SqlExpression)g;
        }

        internal virtual SqlExpression VisitDiscriminatedType(SqlDiscriminatedType dt)
        {
            dt.Discriminator = this.VisitExpression(dt.Discriminator);
            return (SqlExpression)dt;
        }

        internal virtual SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof)
        {
            dof.Object = this.VisitExpression(dof.Object);
            return (SqlExpression)dof;
        }

        internal virtual SqlNode VisitIncludeScope(SqlIncludeScope node)
        {
            node.Child = this.Visit(node.Child);
            return (SqlNode)node;
        }

        internal bool RefersToColumn(SqlExpression exp, SqlColumn col)
        {
            if (exp != null)
            {
                switch (exp.NodeType)
                {
                    case SqlNodeType.ExprSet:
                        SqlExprSet sqlExprSet = (SqlExprSet)exp;
                        int index = 0;
                        for (int count = sqlExprSet.Expressions.Count; index < count; ++index)
                        {
                            if (this.RefersToColumn(sqlExprSet.Expressions[index], col))
                                return true;
                        }
                        break;
                    case SqlNodeType.OuterJoinedValue:
                        return this.RefersToColumn(((SqlUnary)exp).Operand, col);
                    case SqlNodeType.Column:
                        if (exp != col)
                            return this.RefersToColumn(((SqlColumn)exp).Expression, col);
                        return true;
                    case SqlNodeType.ColumnRef:
                        SqlColumnRef sqlColumnRef = (SqlColumnRef)exp;
                        if (sqlColumnRef.Column != col)
                            return this.RefersToColumn(sqlColumnRef.Column.Expression, col);
                        return true;
                }
            }
            return false;
        }
    }
}
