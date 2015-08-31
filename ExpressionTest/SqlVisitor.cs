using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

    internal class SqlLift : SqlExpression
    {
        internal SqlExpression liftedExpression;

        internal SqlExpression Expression
        {
            get
            {
                return this.liftedExpression;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.liftedExpression = value;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.liftedExpression.SqlType;
            }
        }

        internal SqlLift(Type type, SqlExpression liftedExpression, System.Linq.Expressions.Expression sourceExpression)
          : base(SqlNodeType.Lift, type, sourceExpression)
        {
            if (liftedExpression == null)
                throw Error.ArgumentNull("liftedExpression");
            this.liftedExpression = liftedExpression;
        }
    }

    internal class SqlMember : SqlSimpleTypeExpression
    {
        private SqlExpression expression;
        private MemberInfo member;

        internal MemberInfo Member
        {
            get
            {
                return this.member;
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
                if (!this.member.ReflectedType.IsAssignableFrom(value.ClrType) && !value.ClrType.IsAssignableFrom(this.member.ReflectedType))
                    throw Error.MemberAccessIllegal((object)this.member, (object)this.member.ReflectedType, (object)value.ClrType);
                this.expression = value;
            }
        }

        internal SqlMember(Type clrType, ProviderType sqlType, SqlExpression expr, MemberInfo member)
          : base(SqlNodeType.Member, clrType, sqlType, expr.SourceExpression)
        {
            this.member = member;
            this.Expression = expr;
        }
    }

    internal class SqlAliasRef : SqlExpression
    {
        private SqlAlias alias;

        internal SqlAlias Alias
        {
            get
            {
                return this.alias;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return SqlAliasRef.GetSqlType(this.alias.Node);
            }
        }

        internal SqlAliasRef(SqlAlias alias)
          : base(SqlNodeType.AliasRef, SqlAliasRef.GetClrType(alias.Node), alias.SourceExpression)
        {
            if (alias == null)
                throw Error.ArgumentNull("alias");
            this.alias = alias;
        }

        private static Type GetClrType(SqlNode node)
        {
            SqlTableValuedFunctionCall valuedFunctionCall = node as SqlTableValuedFunctionCall;
            if (valuedFunctionCall != null)
                return valuedFunctionCall.RowType.Type;
            SqlExpression sqlExpression = node as SqlExpression;
            if (sqlExpression != null)
            {
                if (TypeSystem.IsSequenceType(sqlExpression.ClrType))
                    return TypeSystem.GetElementType(sqlExpression.ClrType);
                return sqlExpression.ClrType;
            }
            SqlSelect sqlSelect = node as SqlSelect;
            if (sqlSelect != null)
                return sqlSelect.Selection.ClrType;
            SqlTable sqlTable = node as SqlTable;
            if (sqlTable != null)
                return sqlTable.RowType.Type;
            SqlUnion sqlUnion = node as SqlUnion;
            if (sqlUnion != null)
                return sqlUnion.GetClrType();
            throw Error.UnexpectedNode((object)node.NodeType);
        }

        private static ProviderType GetSqlType(SqlNode node)
        {
            SqlExpression sqlExpression = node as SqlExpression;
            if (sqlExpression != null)
                return sqlExpression.SqlType;
            SqlSelect sqlSelect = node as SqlSelect;
            if (sqlSelect != null)
                return sqlSelect.Selection.SqlType;
            SqlTable sqlTable = node as SqlTable;
            if (sqlTable != null)
                return sqlTable.SqlRowType;
            SqlUnion sqlUnion = node as SqlUnion;
            if (sqlUnion != null)
                return sqlUnion.GetSqlType();
            throw Error.UnexpectedNode((object)node.NodeType);
        }
    }

    internal class SqlStoredProcedureCall : SqlUserQuery
    {
        private MetaFunction function;

        internal MetaFunction Function
        {
            get
            {
                return this.function;
            }
        }

        internal SqlStoredProcedureCall(MetaFunction function, SqlExpression projection, IEnumerable<SqlExpression> args, Expression source)
          : base(SqlNodeType.StoredProcedureCall, projection, args, source)
        {
            if (function == null)
                throw Error.ArgumentNull("function");
            this.function = function;
        }
    }

    internal class SqlUserRow : SqlSimpleTypeExpression
    {
        private SqlUserQuery query;
        private MetaType rowType;

        internal MetaType RowType
        {
            get
            {
                return this.rowType;
            }
        }

        internal SqlUserQuery Query
        {
            get
            {
                return this.query;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.Projection != null && value.Projection.ClrType != this.ClrType)
                    throw Error.ArgumentWrongType((object)"value", (object)this.ClrType, (object)value.Projection.ClrType);
                this.query = value;
            }
        }

        internal SqlUserRow(MetaType rowType, ProviderType sqlType, SqlUserQuery query, Expression source)
          : base(SqlNodeType.UserRow, rowType.Type, sqlType, source)
        {
            this.Query = query;
            this.rowType = rowType;
        }
    }

    internal class SqlUserQuery : SqlNode
    {
        private string queryText;
        private SqlExpression projection;
        private List<SqlExpression> args;
        private List<SqlUserColumn> columns;

        internal string QueryText
        {
            get
            {
                return this.queryText;
            }
        }

        internal SqlExpression Projection
        {
            get
            {
                return this.projection;
            }
            set
            {
                if (this.projection != null && this.projection.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType((object)"value", (object)this.projection.ClrType, (object)value.ClrType);
                this.projection = value;
            }
        }

        internal List<SqlExpression> Arguments
        {
            get
            {
                return this.args;
            }
        }

        internal List<SqlUserColumn> Columns
        {
            get
            {
                return this.columns;
            }
        }

        internal SqlUserQuery(SqlNodeType nt, SqlExpression projection, IEnumerable<SqlExpression> args, Expression source)
          : base(nt, source)
        {
            this.Projection = projection;
            this.args = args != null ? new List<SqlExpression>(args) : new List<SqlExpression>();
            this.columns = new List<SqlUserColumn>();
        }

        internal SqlUserQuery(string queryText, SqlExpression projection, IEnumerable<SqlExpression> args, Expression source)
          : base(SqlNodeType.UserQuery, source)
        {
            this.queryText = queryText;
            this.Projection = projection;
            this.args = args != null ? new List<SqlExpression>(args) : new List<SqlExpression>();
            this.columns = new List<SqlUserColumn>();
        }

        internal SqlUserColumn Find(string name)
        {
            foreach (SqlUserColumn sqlUserColumn in this.Columns)
            {
                if (sqlUserColumn.Name == name)
                    return sqlUserColumn;
            }
            return (SqlUserColumn)null;
        }
    }

    internal class SqlUserColumn : SqlSimpleTypeExpression
    {
        private SqlUserQuery query;
        private string name;
        private bool isRequired;

        internal SqlUserQuery Query
        {
            get
            {
                return this.query;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.query != null && this.query != value)
                    throw Error.ArgumentWrongValue((object)"value");
                this.query = value;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal bool IsRequired
        {
            get
            {
                return this.isRequired;
            }
        }

        internal SqlUserColumn(Type clrType, ProviderType sqlType, SqlUserQuery query, string name, bool isRequired, Expression source)
          : base(SqlNodeType.UserColumn, clrType, sqlType, source)
        {
            this.Query = query;
            this.name = name;
            this.isRequired = isRequired;
        }
    }

    internal class SqlNew : SqlSimpleTypeExpression
    {
        private MetaType metaType;
        private ConstructorInfo constructor;
        private List<SqlExpression> args;
        private List<MemberInfo> argMembers;
        private List<SqlMemberAssign> members;

        internal MetaType MetaType
        {
            get
            {
                return this.metaType;
            }
        }

        internal ConstructorInfo Constructor
        {
            get
            {
                return this.constructor;
            }
        }

        internal List<SqlExpression> Args
        {
            get
            {
                return this.args;
            }
        }

        internal List<MemberInfo> ArgMembers
        {
            get
            {
                return this.argMembers;
            }
        }

        internal List<SqlMemberAssign> Members
        {
            get
            {
                return this.members;
            }
        }

        internal SqlNew(MetaType metaType, ProviderType sqlType, ConstructorInfo cons, IEnumerable<SqlExpression> args, IEnumerable<MemberInfo> argMembers, IEnumerable<SqlMemberAssign> members, Expression sourceExpression)
          : base(SqlNodeType.New, metaType.Type, sqlType, sourceExpression)
        {
            this.metaType = metaType;
            if (cons == (ConstructorInfo)null && metaType.Type.IsClass)
                throw Error.ArgumentNull("cons");
            this.constructor = cons;
            this.args = new List<SqlExpression>();
            this.argMembers = new List<MemberInfo>();
            this.members = new List<SqlMemberAssign>();
            if (args != null)
                this.args.AddRange(args);
            if (argMembers != null)
                this.argMembers.AddRange(argMembers);
            if (members == null)
                return;
            this.members.AddRange(members);
        }

        internal SqlExpression Find(MemberInfo mi)
        {
            int index = 0;
            for (int count = this.argMembers.Count; index < count; ++index)
            {
                if (this.argMembers[index].Name == mi.Name)
                    return this.args[index];
            }
            foreach (SqlMemberAssign sqlMemberAssign in this.Members)
            {
                if (sqlMemberAssign.Member.Name == mi.Name)
                    return sqlMemberAssign.Expression;
            }
            return (SqlExpression)null;
        }
    }

    internal class SqlLink : SqlSimpleTypeExpression
    {
        private MetaType rowType;
        private SqlExpression expression;
        private MetaDataMember member;
        private List<SqlExpression> keyExpressions;
        private SqlExpression expansion;
        private object id;

        internal MetaType RowType
        {
            get
            {
                return this.rowType;
            }
        }

        internal SqlExpression Expansion
        {
            get
            {
                return this.expansion;
            }
            set
            {
                this.expansion = value;
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
                this.expression = value;
            }
        }

        internal MetaDataMember Member
        {
            get
            {
                return this.member;
            }
        }

        internal List<SqlExpression> KeyExpressions
        {
            get
            {
                return this.keyExpressions;
            }
        }

        internal object Id
        {
            get
            {
                return this.id;
            }
        }

        internal SqlLink(object id, MetaType rowType, Type clrType, ProviderType sqlType, SqlExpression expression, MetaDataMember member, IEnumerable<SqlExpression> keyExpressions, SqlExpression expansion, System.Linq.Expressions.Expression sourceExpression)
          : base(SqlNodeType.Link, clrType, sqlType, sourceExpression)
        {
            this.id = id;
            this.rowType = rowType;
            this.expansion = expansion;
            this.expression = expression;
            this.member = member;
            this.keyExpressions = new List<SqlExpression>();
            if (keyExpressions == null)
                return;
            this.keyExpressions.AddRange(keyExpressions);
        }
    }

    internal class SqlClientQuery : SqlSimpleTypeExpression
    {
        private SqlSubSelect query;
        private List<SqlExpression> arguments;
        private List<SqlParameter> parameters;
        private int ordinal;

        internal SqlSubSelect Query
        {
            get
            {
                return this.query;
            }
            set
            {
                if (value == null || this.query != null && this.query.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType((object)value, (object)this.query.ClrType, (object)value.ClrType);
                this.query = value;
            }
        }

        internal List<SqlExpression> Arguments
        {
            get
            {
                return this.arguments;
            }
        }

        internal List<SqlParameter> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        internal int Ordinal
        {
            get
            {
                return this.ordinal;
            }
            set
            {
                this.ordinal = value;
            }
        }

        internal SqlClientQuery(SqlSubSelect subquery)
          : base(SqlNodeType.ClientQuery, subquery.ClrType, subquery.SqlType, subquery.SourceExpression)
        {
            this.query = subquery;
            this.arguments = new List<SqlExpression>();
            this.parameters = new List<SqlParameter>();
        }
    }

    internal class SqlJoinedCollection : SqlSimpleTypeExpression
    {
        private SqlExpression expression;
        private SqlExpression count;

        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                if (value == null || this.expression != null && this.expression.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType((object)value, (object)this.expression.ClrType, (object)value.ClrType);
                this.expression = value;
            }
        }

        internal SqlExpression Count
        {
            get
            {
                return this.count;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.ClrType != typeof(int))
                    throw Error.ArgumentWrongType((object)value, (object)typeof(int), (object)value.ClrType);
                this.count = value;
            }
        }

        internal SqlJoinedCollection(Type clrType, ProviderType sqlType, SqlExpression expression, SqlExpression count, System.Linq.Expressions.Expression sourceExpression)
          : base(SqlNodeType.JoinedCollection, clrType, sqlType, sourceExpression)
        {
            this.expression = expression;
            this.count = count;
        }
    }

    internal class SqlClientArray : SqlSimpleTypeExpression
    {
        private List<SqlExpression> expressions;

        internal List<SqlExpression> Expressions
        {
            get
            {
                return this.expressions;
            }
        }

        internal SqlClientArray(Type clrType, ProviderType sqlType, SqlExpression[] exprs, Expression sourceExpression)
          : base(SqlNodeType.ClientArray, clrType, sqlType, sourceExpression)
        {
            this.expressions = new List<SqlExpression>();
            if (exprs == null)
                return;
            this.Expressions.AddRange((IEnumerable<SqlExpression>)exprs);
        }
    }

    internal class SqlClientParameter : SqlSimpleTypeExpression
    {
        private LambdaExpression accessor;

        internal LambdaExpression Accessor
        {
            get
            {
                return this.accessor;
            }
        }

        internal SqlClientParameter(Type clrType, ProviderType sqlType, LambdaExpression accessor, Expression sourceExpression)
          : base(SqlNodeType.ClientParameter, clrType, sqlType, sourceExpression)
        {
            this.accessor = accessor;
        }
    }

    internal class SqlParameter : SqlSimpleTypeExpression
    {
        private string name;
        private ParameterDirection direction;

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal ParameterDirection Direction
        {
            get
            {
                return this.direction;
            }
            set
            {
                this.direction = value;
            }
        }

        internal SqlParameter(Type clrType, ProviderType sqlType, string name, Expression sourceExpression)
          : base(SqlNodeType.Parameter, clrType, sqlType, sourceExpression)
        {
            if (name == null)
                throw Error.ArgumentNull("name");
            if (typeof(Type).IsAssignableFrom(clrType))
                throw Error.ArgumentWrongValue((object)"clrType");
            this.name = name;
            this.direction = ParameterDirection.Input;
        }
    }
    internal class SqlValue : SqlSimpleTypeExpression
    {
        private object value;
        private bool isClient;

        internal object Value
        {
            get
            {
                return this.value;
            }
        }

        internal bool IsClientSpecified
        {
            get
            {
                return this.isClient;
            }
        }

        internal SqlValue(Type clrType, ProviderType sqlType, object value, bool isClientSpecified, Expression sourceExpression)
          : base(SqlNodeType.Value, clrType, sqlType, sourceExpression)
        {
            this.value = value;
            this.isClient = isClientSpecified;
        }
    }

    internal class SqlSubSelect : SqlSimpleTypeExpression
    {
        private SqlSelect select;

        internal SqlSelect Select
        {
            get
            {
                return this.select;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.select = value;
            }
        }

        internal SqlSubSelect(SqlNodeType nt, Type clrType, ProviderType sqlType, SqlSelect select)
          : base(nt, clrType, sqlType, select.SourceExpression)
        {
            if (nt <= SqlNodeType.Exists)
            {
                if (nt == SqlNodeType.Element || nt == SqlNodeType.Exists)
                    goto label_4;
            }
            else if (nt == SqlNodeType.Multiset || nt == SqlNodeType.ScalarSubSelect)
                goto label_4;
            throw Error.UnexpectedNode((object)nt);
            label_4:
            this.Select = select;
        }
    }

    internal class SqlJoin : SqlSource
    {
        private SqlJoinType joinType;
        private SqlSource left;
        private SqlSource right;
        private SqlExpression condition;

        internal SqlJoinType JoinType
        {
            get
            {
                return this.joinType;
            }
            set
            {
                this.joinType = value;
            }
        }

        internal SqlSource Left
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

        internal SqlSource Right
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

        internal SqlExpression Condition
        {
            get
            {
                return this.condition;
            }
            set
            {
                this.condition = value;
            }
        }

        internal SqlJoin(SqlJoinType type, SqlSource left, SqlSource right, SqlExpression cond, Expression sourceExpression)
          : base(SqlNodeType.Join, sourceExpression)
        {
            this.JoinType = type;
            this.Left = left;
            this.Right = right;
            this.Condition = cond;
        }
    }
    internal enum SqlJoinType
    {
        Cross,
        Inner,
        LeftOuter,
        CrossApply,
        OuterApply,
    }
    internal class SqlInsert : SqlStatement
    {
        private SqlTable table;
        private SqlRow row;
        private SqlExpression expression;
        private SqlColumn outputKey;
        private bool outputToLocal;

        internal SqlTable Table
        {
            get
            {
                return this.table;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("null");
                this.table = value;
            }
        }

        internal SqlRow Row
        {
            get
            {
                return this.row;
            }
            set
            {
                this.row = value;
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
                    throw Error.ArgumentNull("null");
                if (!this.table.RowType.Type.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType((object)"value", (object)this.table.RowType, (object)value.ClrType);
                this.expression = value;
            }
        }

        internal SqlColumn OutputKey
        {
            get
            {
                return this.outputKey;
            }
            set
            {
                this.outputKey = value;
            }
        }

        internal bool OutputToLocal
        {
            get
            {
                return this.outputToLocal;
            }
            set
            {
                this.outputToLocal = value;
            }
        }

        internal SqlInsert(SqlTable table, SqlExpression expr, System.Linq.Expressions.Expression sourceExpression)
          : base(SqlNodeType.Insert, sourceExpression)
        {
            this.Table = table;
            this.Expression = expr;
            this.Row = new SqlRow(sourceExpression);
        }
    }

    internal class SqlUpdate : SqlStatement
    {
        private SqlSelect select;
        private List<SqlAssign> assignments;

        internal SqlSelect Select
        {
            get
            {
                return this.select;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.select = value;
            }
        }

        internal List<SqlAssign> Assignments
        {
            get
            {
                return this.assignments;
            }
        }

        internal SqlUpdate(SqlSelect select, IEnumerable<SqlAssign> assignments, Expression sourceExpression)
          : base(SqlNodeType.Update, sourceExpression)
        {
            this.Select = select;
            this.assignments = new List<SqlAssign>(assignments);
        }
    }

    internal class SqlDelete : SqlStatement
    {
        private SqlSelect select;

        internal SqlSelect Select
        {
            get
            {
                return this.select;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.select = value;
            }
        }

        internal SqlDelete(SqlSelect select, Expression sourceExpression)
          : base(SqlNodeType.Delete, sourceExpression)
        {
            this.Select = select;
        }
    }

    internal class SqlMemberAssign : SqlNode
    {
        private MemberInfo member;
        private SqlExpression expression;

        internal MemberInfo Member
        {
            get
            {
                return this.member;
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
                this.expression = value;
            }
        }

        internal SqlMemberAssign(MemberInfo member, SqlExpression expr)
          : base(SqlNodeType.MemberAssign, expr.SourceExpression)
        {
            if (member == (MemberInfo)null)
                throw Error.ArgumentNull("member");
            this.member = member;
            this.Expression = expr;
        }
    }

    internal class SqlAssign : SqlStatement
    {
        private SqlExpression leftValue;
        private SqlExpression rightValue;

        internal SqlExpression LValue
        {
            get
            {
                return this.leftValue;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.rightValue != null && !value.ClrType.IsAssignableFrom(this.rightValue.ClrType))
                    throw Error.ArgumentWrongType((object)"value", (object)this.rightValue.ClrType, (object)value.ClrType);
                this.leftValue = value;
            }
        }

        internal SqlExpression RValue
        {
            get
            {
                return this.rightValue;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.leftValue != null && !this.leftValue.ClrType.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType((object)"value", (object)this.leftValue.ClrType, (object)value.ClrType);
                this.rightValue = value;
            }
        }

        internal SqlAssign(SqlExpression lValue, SqlExpression rValue, Expression sourceExpression)
          : base(SqlNodeType.Assign, sourceExpression)
        {
            this.LValue = lValue;
            this.RValue = rValue;
        }
    }
    internal class SqlBlock : SqlStatement
    {
        private List<SqlStatement> statements;

        internal List<SqlStatement> Statements
        {
            get
            {
                return this.statements;
            }
        }

        internal SqlBlock(Expression sourceExpression)
          : base(SqlNodeType.Block, sourceExpression)
        {
            this.statements = new List<SqlStatement>();
        }
    }
    internal class SqlSearchedCase : SqlExpression
    {
        private List<SqlWhen> whens;
        private SqlExpression @else;

        internal List<SqlWhen> Whens
        {
            get
            {
                return this.whens;
            }
        }

        internal SqlExpression Else
        {
            get
            {
                return this.@else;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.@else != null && !this.@else.ClrType.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType((object)"value", (object)this.@else.ClrType, (object)value.ClrType);
                this.@else = value;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.whens[0].Value.SqlType;
            }
        }

        internal SqlSearchedCase(Type clrType, IEnumerable<SqlWhen> whens, SqlExpression @else, Expression sourceExpression)
          : base(SqlNodeType.SearchedCase, clrType, sourceExpression)
        {
            if (whens == null)
                throw Error.ArgumentNull("whens");
            this.whens = new List<SqlWhen>(whens);
            if (this.whens.Count == 0)
                throw Error.ArgumentOutOfRange("whens");
            this.Else = @else;
        }
    }
    internal class SqlClientWhen
    {
        private SqlExpression matchExpression;
        private SqlExpression matchValue;

        internal SqlExpression Match
        {
            get
            {
                return this.matchExpression;
            }
            set
            {
                if (this.matchExpression != null && value != null && this.matchExpression.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType((object)"value", (object)this.matchExpression.ClrType, (object)value.ClrType);
                this.matchExpression = value;
            }
        }

        internal SqlExpression Value
        {
            get
            {
                return this.matchValue;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.matchValue != null && this.matchValue.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType((object)"value", (object)this.matchValue.ClrType, (object)value.ClrType);
                this.matchValue = value;
            }
        }

        internal SqlClientWhen(SqlExpression match, SqlExpression value)
        {
            if (value == null)
                throw Error.ArgumentNull("value");
            this.Match = match;
            this.Value = value;
        }
    }
    internal class SqlClientCase : SqlExpression
    {
        private List<SqlClientWhen> whens = new List<SqlClientWhen>();
        private SqlExpression expression;

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
                if (this.expression != null && this.expression.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType((object)"value", (object)this.expression.ClrType, (object)value.ClrType);
                this.expression = value;
            }
        }
       
        internal List<SqlClientWhen> Whens
        {
            get
            {
                return this.whens;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.whens[0].Value.SqlType;
            }
        }

        internal SqlClientCase(Type clrType, SqlExpression expr, IEnumerable<SqlClientWhen> whens, System.Linq.Expressions.Expression sourceExpression)
          : base(SqlNodeType.ClientCase, clrType, sourceExpression)
        {
            this.Expression = expr;
            if (whens == null)
                throw Error.ArgumentNull("whens");
            this.whens.AddRange(whens);
            if (this.whens.Count == 0)
                throw Error.ArgumentOutOfRange("whens");
        }
    }
    internal class SqlWhen
    {
        private SqlExpression matchExpression;
        private SqlExpression valueExpression;

        internal SqlExpression Match
        {
            get
            {
                return this.matchExpression;
            }
            set
            {
                if (this.matchExpression != null && value != null && (this.matchExpression.ClrType != value.ClrType && !TypeSystem.GetNonNullableType(this.matchExpression.ClrType).Equals(typeof(bool))) && !TypeSystem.GetNonNullableType(value.ClrType).Equals(typeof(bool)))
                    throw Error.ArgumentWrongType((object)"value", (object)this.matchExpression.ClrType, (object)value.ClrType);
                this.matchExpression = value;
            }
        }

        internal SqlExpression Value
        {
            get
            {
                return this.valueExpression;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.valueExpression != null && !this.valueExpression.ClrType.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType((object)"value", (object)this.valueExpression.ClrType, (object)value.ClrType);
                this.valueExpression = value;
            }
        }

        internal SqlWhen(SqlExpression match, SqlExpression value)
        {
            if (value == null)
                throw Error.ArgumentNull("value");
            this.Match = match;
            this.Value = value;
        }
    }
    internal class SqlSimpleCase : SqlExpression
    {
        private List<SqlWhen> whens = new List<SqlWhen>();
        private SqlExpression expression;

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
                if (this.expression != null && this.expression.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType((object)"value", (object)this.expression.ClrType, (object)value.ClrType);
                this.expression = value;
            }
        }

        internal List<SqlWhen> Whens
        {
            get
            {
                return this.whens;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.whens[0].Value.SqlType;
            }
        }

        internal SqlSimpleCase(Type clrType, SqlExpression expr, IEnumerable<SqlWhen> whens, System.Linq.Expressions.Expression sourceExpression)
          : base(SqlNodeType.SimpleCase, clrType, sourceExpression)
        {
            this.Expression = expr;
            if (whens == null)
                throw Error.ArgumentNull("whens");
            this.whens.AddRange(whens);
            if (this.whens.Count == 0)
                throw Error.ArgumentOutOfRange("whens");
        }
    }
    internal class SqlTypeCaseWhen
    {
        private SqlExpression match;
        private SqlExpression @new;

        internal SqlExpression Match
        {
            get
            {
                return this.match;
            }
            set
            {
                if (this.match != null && value != null && this.match.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType((object)"value", (object)this.match.ClrType, (object)value.ClrType);
                this.match = value;
            }
        }

        internal SqlExpression TypeBinding
        {
            get
            {
                return this.@new;
            }
            set
            {
                this.@new = value;
            }
        }

        internal SqlTypeCaseWhen(SqlExpression match, SqlExpression typeBinding)
        {
            this.Match = match;
            this.TypeBinding = typeBinding;
        }
    }
    internal class SqlTypeCase : SqlExpression
    {
        private List<SqlTypeCaseWhen> whens = new List<SqlTypeCaseWhen>();
        private MetaType rowType;
        private SqlExpression discriminator;
        private ProviderType sqlType;

        internal SqlExpression Discriminator
        {
            get
            {
                return this.discriminator;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (this.discriminator != null && this.discriminator.ClrType != value.ClrType)
                    throw Error.ArgumentWrongType((object)"value", (object)this.discriminator.ClrType, (object)value.ClrType);
                this.discriminator = value;
            }
        }

        internal List<SqlTypeCaseWhen> Whens
        {
            get
            {
                return this.whens;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.sqlType;
            }
        }

        internal MetaType RowType
        {
            get
            {
                return this.rowType;
            }
        }

        internal SqlTypeCase(Type clrType, ProviderType sqlType, MetaType rowType, SqlExpression discriminator, IEnumerable<SqlTypeCaseWhen> whens, Expression sourceExpression)
          : base(SqlNodeType.TypeCase, clrType, sourceExpression)
        {
            this.Discriminator = discriminator;
            if (whens == null)
                throw Error.ArgumentNull("whens");
            this.whens.AddRange(whens);
            if (this.whens.Count == 0)
                throw Error.ArgumentOutOfRange("whens");
            this.sqlType = sqlType;
            this.rowType = rowType;
        }
    }
    internal class SqlVariable : SqlSimpleTypeExpression
    {
        private string name;

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal SqlVariable(Type clrType, ProviderType sqlType, string name, Expression sourceExpression)
          : base(SqlNodeType.Variable, clrType, sqlType, sourceExpression)
        {
            if (name == null)
                throw Error.ArgumentNull("name");
            this.name = name;
        }
    }
    internal class SqlOptionalValue : SqlSimpleTypeExpression
    {
        private SqlExpression hasValue;
        private SqlExpression expressionValue;

        internal SqlExpression HasValue
        {
            get
            {
                return this.hasValue;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                this.hasValue = value;
            }
        }

        internal SqlExpression Value
        {
            get
            {
                return this.expressionValue;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.ClrType != this.ClrType)
                    throw Error.ArgumentWrongType((object)"value", (object)this.ClrType, (object)value.ClrType);
                this.expressionValue = value;
            }
        }

        internal SqlOptionalValue(SqlExpression hasValue, SqlExpression value)
          : base(SqlNodeType.OptionalValue, value.ClrType, value.SqlType, value.SourceExpression)
        {
            this.HasValue = hasValue;
            this.Value = value;
        }
    }
    internal class SqlBetween : SqlSimpleTypeExpression
    {
        private SqlExpression expression;
        private SqlExpression start;
        private SqlExpression end;

        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                this.expression = value;
            }
        }

        internal SqlExpression Start
        {
            get
            {
                return this.start;
            }
            set
            {
                this.start = value;
            }
        }

        internal SqlExpression End
        {
            get
            {
                return this.end;
            }
            set
            {
                this.end = value;
            }
        }

        internal SqlBetween(Type clrType, ProviderType sqlType, SqlExpression expr, SqlExpression start, SqlExpression end, System.Linq.Expressions.Expression source)
          : base(SqlNodeType.Between, clrType, sqlType, source)
        {
            this.expression = expr;
            this.start = start;
            this.end = end;
        }
    }
    internal class SqlIn : SqlSimpleTypeExpression
    {
        private SqlExpression expression;
        private List<SqlExpression> values;

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
                this.expression = value;
            }
        }

        internal List<SqlExpression> Values
        {
            get
            {
                return this.values;
            }
        }

        internal SqlIn(Type clrType, ProviderType sqlType, SqlExpression expression, IEnumerable<SqlExpression> values, System.Linq.Expressions.Expression sourceExpression)
          : base(SqlNodeType.In, clrType, sqlType, sourceExpression)
        {
            this.expression = expression;
            this.values = values != null ? new List<SqlExpression>(values) : new List<SqlExpression>(0);
        }
    }
    internal class SqlLike : SqlSimpleTypeExpression
    {
        private SqlExpression expression;
        private SqlExpression pattern;
        private SqlExpression escape;

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
                if (value.ClrType != typeof(string))
                    throw Error.ArgumentWrongType((object)"value", (object)"string", (object)value.ClrType);
                this.expression = value;
            }
        }

        internal SqlExpression Pattern
        {
            get
            {
                return this.pattern;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.ClrType != typeof(string))
                    throw Error.ArgumentWrongType((object)"value", (object)"string", (object)value.ClrType);
                this.pattern = value;
            }
        }

        internal SqlExpression Escape
        {
            get
            {
                return this.escape;
            }
            set
            {
                if (value != null && value.ClrType != typeof(string))
                    throw Error.ArgumentWrongType((object)"value", (object)"string", (object)value.ClrType);
                this.escape = value;
            }
        }

        internal SqlLike(Type clrType, ProviderType sqlType, SqlExpression expr, SqlExpression pattern, SqlExpression escape, System.Linq.Expressions.Expression source)
          : base(SqlNodeType.Like, clrType, sqlType, source)
        {
            if (expr == null)
                throw Error.ArgumentNull("expr");
            if (pattern == null)
                throw Error.ArgumentNull("pattern");
            this.Expression = expr;
            this.Pattern = pattern;
            this.Escape = escape;
        }
    }
    internal class SqlFunctionCall : SqlSimpleTypeExpression
    {
        private string name;
        private List<SqlExpression> arguments;

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal List<SqlExpression> Arguments
        {
            get
            {
                return this.arguments;
            }
        }

        internal SqlFunctionCall(Type clrType, ProviderType sqlType, string name, IEnumerable<SqlExpression> args, Expression source)
          : this(SqlNodeType.FunctionCall, clrType, sqlType, name, args, source)
        {
        }

        internal SqlFunctionCall(SqlNodeType nodeType, Type clrType, ProviderType sqlType, string name, IEnumerable<SqlExpression> args, Expression source)
          : base(nodeType, clrType, sqlType, source)
        {
            this.name = name;
            this.arguments = new List<SqlExpression>(args);
        }
    }

    internal class SqlTableValuedFunctionCall : SqlFunctionCall
    {
        private MetaType rowType;
        private List<SqlColumn> columns;

        internal MetaType RowType
        {
            get
            {
                return this.rowType;
            }
        }

        internal List<SqlColumn> Columns
        {
            get
            {
                return this.columns;
            }
        }

        internal SqlTableValuedFunctionCall(MetaType rowType, Type clrType, ProviderType sqlType, string name, IEnumerable<SqlExpression> args, Expression source)
          : base(SqlNodeType.TableValuedFunctionCall, clrType, sqlType, name, args, source)
        {
            this.rowType = rowType;
            this.columns = new List<SqlColumn>();
        }

        internal SqlColumn Find(string name)
        {
            foreach (SqlColumn sqlColumn in this.Columns)
            {
                if (sqlColumn.Name == name)
                    return sqlColumn;
            }
            return (SqlColumn)null;
        }
    }
    internal class SqlMethodCall : SqlSimpleTypeExpression
    {
        private MethodInfo method;
        private SqlExpression obj;
        private List<SqlExpression> arguments;

        internal MethodInfo Method
        {
            get
            {
                return this.method;
            }
        }

        internal SqlExpression Object
        {
            get
            {
                return this.obj;
            }
            set
            {
                if (value == null && !this.method.IsStatic)
                    throw Error.ArgumentNull("value");
                if (value != null && !this.method.DeclaringType.IsAssignableFrom(value.ClrType))
                    throw Error.ArgumentWrongType((object)"value", (object)this.method.DeclaringType, (object)value.ClrType);
                this.obj = value;
            }
        }

        internal List<SqlExpression> Arguments
        {
            get
            {
                return this.arguments;
            }
        }

        internal SqlMethodCall(Type clrType, ProviderType sqlType, MethodInfo method, SqlExpression obj, IEnumerable<SqlExpression> args, Expression sourceExpression)
          : base(SqlNodeType.MethodCall, clrType, sqlType, sourceExpression)
        {
            if (method == (MethodInfo)null)
                throw Error.ArgumentNull("method");
            this.method = method;
            this.Object = obj;
            this.arguments = new List<SqlExpression>();
            if (args == null)
                return;
            this.arguments.AddRange(args);
        }
    }
    internal class SqlSharedExpression : SqlExpression
    {
        private SqlExpression expr;

        internal SqlExpression Expression
        {
            get
            {
                return this.expr;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (!this.ClrType.IsAssignableFrom(value.ClrType) && !value.ClrType.IsAssignableFrom(this.ClrType))
                    throw Error.ArgumentWrongType((object)"value", (object)this.ClrType, (object)value.ClrType);
                this.expr = value;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.expr.SqlType;
            }
        }

        internal SqlSharedExpression(SqlExpression expr)
          : base(SqlNodeType.SharedExpression, expr.ClrType, expr.SourceExpression)
        {
            this.expr = expr;
        }
    }

    internal class SqlSharedExpressionRef : SqlExpression
    {
        private SqlSharedExpression expr;

        internal SqlSharedExpression SharedExpression
        {
            get
            {
                return this.expr;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.expr.SqlType;
            }
        }

        internal SqlSharedExpressionRef(SqlSharedExpression expr)
          : base(SqlNodeType.SharedExpressionRef, expr.ClrType, expr.SourceExpression)
        {
            this.expr = expr;
        }
    }
    internal class SqlSimpleExpression : SqlExpression
    {
        private SqlExpression expr;

        internal SqlExpression Expression
        {
            get
            {
                return this.expr;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (!TypeSystem.GetNonNullableType(this.ClrType).IsAssignableFrom(TypeSystem.GetNonNullableType(value.ClrType)))
                    throw Error.ArgumentWrongType((object)"value", (object)this.ClrType, (object)value.ClrType);
                this.expr = value;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.expr.SqlType;
            }
        }

        internal SqlSimpleExpression(SqlExpression expr)
          : base(SqlNodeType.SimpleExpression, expr.ClrType, expr.SourceExpression)
        {
            this.expr = expr;
        }
    }

    internal class SqlGrouping : SqlSimpleTypeExpression
    {
        private SqlExpression key;
        private SqlExpression group;

        internal SqlExpression Key
        {
            get
            {
                return this.key;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (!this.key.ClrType.IsAssignableFrom(value.ClrType) && !value.ClrType.IsAssignableFrom(this.key.ClrType))
                    throw Error.ArgumentWrongType((object)"value", (object)this.key.ClrType, (object)value.ClrType);
                this.key = value;
            }
        }

        internal SqlExpression Group
        {
            get
            {
                return this.group;
            }
            set
            {
                if (value == null)
                    throw Error.ArgumentNull("value");
                if (value.ClrType != this.group.ClrType)
                    throw Error.ArgumentWrongType((object)"value", (object)this.group.ClrType, (object)value.ClrType);
                this.group = value;
            }
        }

        internal SqlGrouping(Type clrType, ProviderType sqlType, SqlExpression key, SqlExpression group, Expression sourceExpression)
          : base(SqlNodeType.Grouping, clrType, sqlType, sourceExpression)
        {
            if (key == null)
                throw Error.ArgumentNull("key");
            if (group == null)
                throw Error.ArgumentNull("group");
            this.key = key;
            this.group = group;
        }
    }

    internal class SqlDiscriminatedType : SqlExpression
    {
        private ProviderType sqlType;
        private SqlExpression discriminator;
        private MetaType targetType;

        internal override ProviderType SqlType
        {
            get
            {
                return this.sqlType;
            }
        }

        internal SqlExpression Discriminator
        {
            get
            {
                return this.discriminator;
            }
            set
            {
                this.discriminator = value;
            }
        }

        internal MetaType TargetType
        {
            get
            {
                return this.targetType;
            }
        }

        internal SqlDiscriminatedType(ProviderType sqlType, SqlExpression discriminator, MetaType targetType, Expression sourceExpression)
          : base(SqlNodeType.DiscriminatedType, typeof(Type), sourceExpression)
        {
            if (discriminator == null)
                throw Error.ArgumentNull("discriminator");
            this.discriminator = discriminator;
            this.targetType = targetType;
            this.sqlType = sqlType;
        }
    }

    internal class SqlDiscriminatorOf : SqlSimpleTypeExpression
    {
        private SqlExpression obj;

        internal SqlExpression Object
        {
            get
            {
                return this.obj;
            }
            set
            {
                this.obj = value;
            }
        }

        internal SqlDiscriminatorOf(SqlExpression obj, Type clrType, ProviderType sqlType, Expression sourceExpression)
          : base(SqlNodeType.DiscriminatorOf, clrType, sqlType, sourceExpression)
        {
            this.obj = obj;
        }
    }

    internal class SqlIncludeScope : SqlNode
    {
        private SqlNode child;

        internal SqlNode Child
        {
            get
            {
                return this.child;
            }
            set
            {
                this.child = value;
            }
        }

        internal SqlIncludeScope(SqlNode child, Expression sourceExpression)
          : base(SqlNodeType.IncludeScope, sourceExpression)
        {
            this.child = child;
        }
    }

    internal class SqlExprSet : SqlExpression
    {
        private List<SqlExpression> expressions;

        internal List<SqlExpression> Expressions
        {
            get
            {
                return this.expressions;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.expressions[0].SqlType;
            }
        }

        internal SqlExprSet(Type clrType, IEnumerable<SqlExpression> exprs, Expression sourceExpression)
          : base(SqlNodeType.ExprSet, clrType, sourceExpression)
        {
            this.expressions = new List<SqlExpression>(exprs);
        }

        internal SqlExpression GetFirstExpression()
        {
            SqlExpression sqlExpression = this.expressions[0];
            while (sqlExpression is SqlExprSet)
                sqlExpression = ((SqlExprSet)sqlExpression).Expressions[0];
            return sqlExpression;
        }
    }
    internal class SqlDoNotVisitExpression : SqlExpression
    {
        private SqlExpression expression;

        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                return this.expression.SqlType;
            }
        }

        internal SqlDoNotVisitExpression(SqlExpression expr)
          : base(SqlNodeType.DoNotVisit, expr.ClrType, expr.SourceExpression)
        {
            if (expr == null)
                throw Error.ArgumentNull("expr");
            this.expression = expr;
        }
    }
}
