using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlDuplicator
    {
        private SqlDuplicator.DuplicatingVisitor superDuper;

        internal SqlDuplicator()
          : this(true)
        {
        }

        internal SqlDuplicator(bool ignoreExternalRefs)
        {
            this.superDuper = new SqlDuplicator.DuplicatingVisitor(ignoreExternalRefs);
        }

        internal static SqlNode Copy(SqlNode node)
        {
            if (node == null)
                return (SqlNode)null;
            switch (node.NodeType)
            {
                case SqlNodeType.Variable:
                case SqlNodeType.Value:
                case SqlNodeType.ColumnRef:
                case SqlNodeType.Parameter:
                    return node;
                default:
                    return new SqlDuplicator().Duplicate(node);
            }
        }

        internal SqlNode Duplicate(SqlNode node)
        {
            return this.superDuper.Visit(node);
        }

        internal class DuplicatingVisitor : SqlVisitor
        {
            private Dictionary<SqlNode, SqlNode> nodeMap;
            private bool ingoreExternalRefs;

            internal DuplicatingVisitor(bool ignoreExternalRefs)
            {
                this.ingoreExternalRefs = ignoreExternalRefs;
                this.nodeMap = new Dictionary<SqlNode, SqlNode>();
            }

            internal override SqlNode Visit(SqlNode node)
            {
                if (node == null)
                    return (SqlNode)null;
                SqlNode sqlNode = (SqlNode)null;
                if (this.nodeMap.TryGetValue(node, out sqlNode))
                    return sqlNode;
                sqlNode = base.Visit(node);
                this.nodeMap[node] = sqlNode;
                return sqlNode;
            }

            internal override SqlExpression VisitDoNotVisit(SqlDoNotVisitExpression expr)
            {
                return (SqlExpression)new SqlDoNotVisitExpression(this.VisitExpression(expr.Expression));
            }

            internal override SqlAlias VisitAlias(SqlAlias a)
            {
                SqlAlias sqlAlias = new SqlAlias(a.Node);
                this.nodeMap[(SqlNode)a] = (SqlNode)sqlAlias;
                sqlAlias.Node = this.Visit(a.Node);
                sqlAlias.Name = a.Name;
                return sqlAlias;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey((SqlNode)aref.Alias))
                    return (SqlExpression)aref;
                return (SqlExpression)new SqlAliasRef((SqlAlias)this.Visit((SqlNode)aref.Alias));
            }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
            {
                List<SqlOrderExpression> orderByList = new List<SqlOrderExpression>();
                foreach (SqlOrderExpression sqlOrderExpression in rowNumber.OrderBy)
                    orderByList.Add(new SqlOrderExpression(sqlOrderExpression.OrderType, (SqlExpression)this.Visit((SqlNode)sqlOrderExpression.Expression)));
                return new SqlRowNumber(rowNumber.ClrType, rowNumber.SqlType, orderByList, rowNumber.SourceExpression);
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                SqlExpression left = (SqlExpression)this.Visit((SqlNode)bo.Left);
                SqlExpression right = (SqlExpression)this.Visit((SqlNode)bo.Right);
                return (SqlExpression)new SqlBinary(bo.NodeType, bo.ClrType, bo.SqlType, left, right, bo.Method);
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                SqlClientQuery sqlClientQuery = new SqlClientQuery((SqlSubSelect)this.VisitExpression((SqlExpression)cq.Query));
                int index1 = 0;
                for (int count = cq.Arguments.Count; index1 < count; ++index1)
                    sqlClientQuery.Arguments.Add(this.VisitExpression(cq.Arguments[index1]));
                int index2 = 0;
                for (int count = cq.Parameters.Count; index2 < count; ++index2)
                    sqlClientQuery.Parameters.Add((SqlParameter)this.VisitExpression((SqlExpression)cq.Parameters[index2]));
                return (SqlExpression)sqlClientQuery;
            }

            internal override SqlExpression VisitJoinedCollection(SqlJoinedCollection jc)
            {
                return (SqlExpression)new SqlJoinedCollection(jc.ClrType, jc.SqlType, this.VisitExpression(jc.Expression), this.VisitExpression(jc.Count), jc.SourceExpression);
            }

            internal override SqlExpression VisitClientArray(SqlClientArray scar)
            {
                SqlExpression[] exprs = new SqlExpression[scar.Expressions.Count];
                int index = 0;
                for (int length = exprs.Length; index < length; ++index)
                    exprs[index] = this.VisitExpression(scar.Expressions[index]);
                return (SqlExpression)new SqlClientArray(scar.ClrType, scar.SqlType, exprs, scar.SourceExpression);
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase tc)
            {
                SqlExpression discriminator = this.VisitExpression(tc.Discriminator);
                List<SqlTypeCaseWhen> list = new List<SqlTypeCaseWhen>();
                foreach (SqlTypeCaseWhen sqlTypeCaseWhen in tc.Whens)
                    list.Add(new SqlTypeCaseWhen(this.VisitExpression(sqlTypeCaseWhen.Match), this.VisitExpression(sqlTypeCaseWhen.TypeBinding)));
                return (SqlExpression)new SqlTypeCase(tc.ClrType, tc.SqlType, tc.RowType, discriminator, (IEnumerable<SqlTypeCaseWhen>)list, tc.SourceExpression);
            }

            internal override SqlExpression VisitNew(SqlNew sox)
            {
                SqlExpression[] sqlExpressionArray = new SqlExpression[sox.Args.Count];
                SqlMemberAssign[] sqlMemberAssignArray = new SqlMemberAssign[sox.Members.Count];
                int index1 = 0;
                for (int length = sqlExpressionArray.Length; index1 < length; ++index1)
                    sqlExpressionArray[index1] = this.VisitExpression(sox.Args[index1]);
                int index2 = 0;
                for (int length = sqlMemberAssignArray.Length; index2 < length; ++index2)
                    sqlMemberAssignArray[index2] = this.VisitMemberAssign(sox.Members[index2]);
                return (SqlExpression)new SqlNew(sox.MetaType, sox.SqlType, sox.Constructor, (IEnumerable<SqlExpression>)sqlExpressionArray, (IEnumerable<MemberInfo>)sox.ArgMembers, (IEnumerable<SqlMemberAssign>)sqlMemberAssignArray, sox.SourceExpression);
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                SqlExpression[] sqlExpressionArray = new SqlExpression[link.KeyExpressions.Count];
                int index = 0;
                for (int length = sqlExpressionArray.Length; index < length; ++index)
                    sqlExpressionArray[index] = this.VisitExpression(link.KeyExpressions[index]);
                SqlLink sqlLink = new SqlLink(new object(), link.RowType, link.ClrType, link.SqlType, (SqlExpression)null, link.Member, (IEnumerable<SqlExpression>)sqlExpressionArray, (SqlExpression)null, link.SourceExpression);
                this.nodeMap[(SqlNode)link] = (SqlNode)sqlLink;
                sqlLink.Expression = this.VisitExpression(link.Expression);
                sqlLink.Expansion = this.VisitExpression(link.Expansion);
                return (SqlNode)sqlLink;
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                SqlColumn sqlColumn = new SqlColumn(col.ClrType, col.SqlType, col.Name, col.MetaMember, (SqlExpression)null, col.SourceExpression);
                this.nodeMap[(SqlNode)col] = (SqlNode)sqlColumn;
                sqlColumn.Expression = this.VisitExpression(col.Expression);
                sqlColumn.Alias = (SqlAlias)this.Visit((SqlNode)col.Alias);
                return (SqlExpression)sqlColumn;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey((SqlNode)cref.Column))
                    return (SqlExpression)cref;
                return (SqlExpression)new SqlColumnRef((SqlColumn)this.Visit((SqlNode)cref.Column));
            }

            internal override SqlStatement VisitDelete(SqlDelete sd)
            {
                return (SqlStatement)new SqlDelete((SqlSelect)this.Visit((SqlNode)sd.Select), sd.SourceExpression);
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem)
            {
                return this.VisitMultiset(elem);
            }

            internal override SqlExpression VisitExists(SqlSubSelect sqlExpr)
            {
                return (SqlExpression)new SqlSubSelect(sqlExpr.NodeType, sqlExpr.ClrType, sqlExpr.SqlType, (SqlSelect)this.Visit((SqlNode)sqlExpr.Select));
            }

            internal override SqlStatement VisitInsert(SqlInsert si)
            {
                SqlInsert sqlInsert = new SqlInsert(si.Table, this.VisitExpression(si.Expression), si.SourceExpression);
                SqlColumn outputKey = si.OutputKey;
                sqlInsert.OutputKey = outputKey;
                int num = si.OutputToLocal ? 1 : 0;
                sqlInsert.OutputToLocal = num != 0;
                SqlRow sqlRow = this.VisitRow(si.Row);
                sqlInsert.Row = sqlRow;
                return (SqlStatement)sqlInsert;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                SqlSource left = this.VisitSource(join.Left);
                SqlSource right = this.VisitSource(join.Right);
                SqlExpression cond = (SqlExpression)this.Visit((SqlNode)join.Condition);
                return (SqlSource)new SqlJoin(join.JoinType, left, right, cond, join.SourceExpression);
            }

            internal override SqlExpression VisitValue(SqlValue value)
            {
                return (SqlExpression)value;
            }

            internal override SqlNode VisitMember(SqlMember m)
            {
                return (SqlNode)new SqlMember(m.ClrType, m.SqlType, (SqlExpression)this.Visit((SqlNode)m.Expression), m.Member);
            }

            internal override SqlMemberAssign VisitMemberAssign(SqlMemberAssign ma)
            {
                return new SqlMemberAssign(ma.Member, (SqlExpression)this.Visit((SqlNode)ma.Expression));
            }

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
                return (SqlExpression)new SqlSubSelect(sms.NodeType, sms.ClrType, sms.SqlType, (SqlSelect)this.Visit((SqlNode)sms.Select));
            }

            internal override SqlExpression VisitParameter(SqlParameter p)
            {
                SqlParameter sqlParameter = new SqlParameter(p.ClrType, p.SqlType, p.Name, p.SourceExpression);
                int num = (int)p.Direction;
                sqlParameter.Direction = (ParameterDirection)num;
                return (SqlExpression)sqlParameter;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                SqlRow sqlRow = new SqlRow(row.SourceExpression);
                foreach (SqlColumn sqlColumn in row.Columns)
                    sqlRow.Columns.Add((SqlColumn)this.Visit((SqlNode)sqlColumn));
                return sqlRow;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                return (SqlExpression)new SqlSubSelect(SqlNodeType.ScalarSubSelect, ss.ClrType, ss.SqlType, this.VisitSequence(ss.Select));
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SqlSource from = this.VisitSource(select.From);
                List<SqlExpression> list1 = (List<SqlExpression>)null;
                if (select.GroupBy.Count > 0)
                {
                    list1 = new List<SqlExpression>(select.GroupBy.Count);
                    foreach (SqlExpression sqlExpression in select.GroupBy)
                        list1.Add((SqlExpression)this.Visit((SqlNode)sqlExpression));
                }
                SqlExpression sqlExpression1 = (SqlExpression)this.Visit((SqlNode)select.Having);
                List<SqlOrderExpression> list2 = (List<SqlOrderExpression>)null;
                if (select.OrderBy.Count > 0)
                {
                    list2 = new List<SqlOrderExpression>(select.OrderBy.Count);
                    foreach (SqlOrderExpression sqlOrderExpression1 in select.OrderBy)
                    {
                        SqlOrderExpression sqlOrderExpression2 = new SqlOrderExpression(sqlOrderExpression1.OrderType, (SqlExpression)this.Visit((SqlNode)sqlOrderExpression1.Expression));
                        list2.Add(sqlOrderExpression2);
                    }
                }
                SqlExpression sqlExpression2 = (SqlExpression)this.Visit((SqlNode)select.Top);
                SqlExpression sqlExpression3 = (SqlExpression)this.Visit((SqlNode)select.Where);
                SqlRow sqlRow = (SqlRow)this.Visit((SqlNode)select.Row);
                SqlSelect sqlSelect = new SqlSelect(this.VisitExpression(select.Selection), from, select.SourceExpression);
                if (list1 != null)
                    sqlSelect.GroupBy.AddRange((IEnumerable<SqlExpression>)list1);
                sqlSelect.Having = sqlExpression1;
                if (list2 != null)
                    sqlSelect.OrderBy.AddRange((IEnumerable<SqlOrderExpression>)list2);
                sqlSelect.OrderingType = select.OrderingType;
                sqlSelect.Row = sqlRow;
                sqlSelect.Top = sqlExpression2;
                sqlSelect.IsDistinct = select.IsDistinct;
                sqlSelect.IsPercent = select.IsPercent;
                sqlSelect.Where = sqlExpression3;
                sqlSelect.DoNotOutput = select.DoNotOutput;
                return sqlSelect;
            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                SqlTable sqlTable = new SqlTable(tab.MetaTable, tab.RowType, tab.SqlRowType, tab.SourceExpression);
                this.nodeMap[(SqlNode)tab] = (SqlNode)sqlTable;
                foreach (SqlColumn sqlColumn in tab.Columns)
                    sqlTable.Columns.Add((SqlColumn)this.Visit((SqlNode)sqlColumn));
                return sqlTable;
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq)
            {
                List<SqlExpression> list = new List<SqlExpression>(suq.Arguments.Count);
                foreach (SqlExpression exp in suq.Arguments)
                    list.Add(this.VisitExpression(exp));
                SqlExpression projection = this.VisitExpression(suq.Projection);
                SqlUserQuery sqlUserQuery = new SqlUserQuery(suq.QueryText, projection, (IEnumerable<SqlExpression>)list, suq.SourceExpression);
                this.nodeMap[(SqlNode)suq] = (SqlNode)sqlUserQuery;
                foreach (SqlUserColumn sqlUserColumn1 in suq.Columns)
                {
                    SqlUserColumn sqlUserColumn2 = new SqlUserColumn(sqlUserColumn1.ClrType, sqlUserColumn1.SqlType, sqlUserColumn1.Query, sqlUserColumn1.Name, sqlUserColumn1.IsRequired, sqlUserColumn1.SourceExpression);
                    this.nodeMap[(SqlNode)sqlUserColumn1] = (SqlNode)sqlUserColumn2;
                    sqlUserQuery.Columns.Add(sqlUserColumn2);
                }
                return sqlUserQuery;
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                List<SqlExpression> list = new List<SqlExpression>(spc.Arguments.Count);
                foreach (SqlExpression exp in spc.Arguments)
                    list.Add(this.VisitExpression(exp));
                SqlExpression projection = this.VisitExpression(spc.Projection);
                SqlStoredProcedureCall storedProcedureCall = new SqlStoredProcedureCall(spc.Function, projection, (IEnumerable<SqlExpression>)list, spc.SourceExpression);
                this.nodeMap[(SqlNode)spc] = (SqlNode)storedProcedureCall;
                foreach (SqlUserColumn sqlUserColumn in spc.Columns)
                    storedProcedureCall.Columns.Add((SqlUserColumn)this.Visit((SqlNode)sqlUserColumn));
                return storedProcedureCall;
            }

            internal override SqlExpression VisitUserColumn(SqlUserColumn suc)
            {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey((SqlNode)suc))
                    return (SqlExpression)suc;
                return (SqlExpression)new SqlUserColumn(suc.ClrType, suc.SqlType, suc.Query, suc.Name, suc.IsRequired, suc.SourceExpression);
            }

            internal override SqlExpression VisitUserRow(SqlUserRow row)
            {
                return (SqlExpression)new SqlUserRow(row.RowType, row.SqlType, (SqlUserQuery)this.Visit((SqlNode)row.Query), row.SourceExpression);
            }

            internal override SqlExpression VisitTreat(SqlUnary t)
            {
                return (SqlExpression)new SqlUnary(SqlNodeType.Treat, t.ClrType, t.SqlType, (SqlExpression)this.Visit((SqlNode)t.Operand), t.SourceExpression);
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                return (SqlExpression)new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType, (SqlExpression)this.Visit((SqlNode)uo.Operand), uo.Method, uo.SourceExpression);
            }

            internal override SqlStatement VisitUpdate(SqlUpdate su)
            {
                SqlSelect select = (SqlSelect)this.Visit((SqlNode)su.Select);
                List<SqlAssign> list = new List<SqlAssign>(su.Assignments.Count);
                foreach (SqlAssign sqlAssign in su.Assignments)
                    list.Add((SqlAssign)this.Visit((SqlNode)sqlAssign));
                return (SqlStatement)new SqlUpdate(select, (IEnumerable<SqlAssign>)list, su.SourceExpression);
            }

            internal override SqlStatement VisitAssign(SqlAssign sa)
            {
                return (SqlStatement)new SqlAssign(this.VisitExpression(sa.LValue), this.VisitExpression(sa.RValue), sa.SourceExpression);
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
            {
                SqlExpression @else = this.VisitExpression(c.Else);
                SqlWhen[] sqlWhenArray = new SqlWhen[c.Whens.Count];
                int index = 0;
                for (int length = sqlWhenArray.Length; index < length; ++index)
                {
                    SqlWhen sqlWhen = c.Whens[index];
                    sqlWhenArray[index] = new SqlWhen(this.VisitExpression(sqlWhen.Match), this.VisitExpression(sqlWhen.Value));
                }
                return (SqlExpression)new SqlSearchedCase(c.ClrType, (IEnumerable<SqlWhen>)sqlWhenArray, @else, c.SourceExpression);
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c)
            {
                SqlExpression expr = this.VisitExpression(c.Expression);
                SqlClientWhen[] sqlClientWhenArray = new SqlClientWhen[c.Whens.Count];
                int index = 0;
                for (int length = sqlClientWhenArray.Length; index < length; ++index)
                {
                    SqlClientWhen sqlClientWhen = c.Whens[index];
                    sqlClientWhenArray[index] = new SqlClientWhen(this.VisitExpression(sqlClientWhen.Match), this.VisitExpression(sqlClientWhen.Value));
                }
                return (SqlExpression)new SqlClientCase(c.ClrType, expr, (IEnumerable<SqlClientWhen>)sqlClientWhenArray, c.SourceExpression);
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                SqlExpression expr = this.VisitExpression(c.Expression);
                SqlWhen[] sqlWhenArray = new SqlWhen[c.Whens.Count];
                int index = 0;
                for (int length = sqlWhenArray.Length; index < length; ++index)
                {
                    SqlWhen sqlWhen = c.Whens[index];
                    sqlWhenArray[index] = new SqlWhen(this.VisitExpression(sqlWhen.Match), this.VisitExpression(sqlWhen.Value));
                }
                return (SqlExpression)new SqlSimpleCase(c.ClrType, expr, (IEnumerable<SqlWhen>)sqlWhenArray, c.SourceExpression);
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                return (SqlNode)new SqlUnion(this.Visit(su.Left), this.Visit(su.Right), su.All);
            }

            internal override SqlExpression VisitExprSet(SqlExprSet xs)
            {
                SqlExpression[] sqlExpressionArray = new SqlExpression[xs.Expressions.Count];
                int index = 0;
                for (int length = sqlExpressionArray.Length; index < length; ++index)
                    sqlExpressionArray[index] = this.VisitExpression(xs.Expressions[index]);
                return (SqlExpression)new SqlExprSet(xs.ClrType, (IEnumerable<SqlExpression>)sqlExpressionArray, xs.SourceExpression);
            }

            internal override SqlBlock VisitBlock(SqlBlock block)
            {
                SqlBlock sqlBlock = new SqlBlock(block.SourceExpression);
                foreach (SqlStatement sqlStatement in block.Statements)
                    sqlBlock.Statements.Add((SqlStatement)this.Visit((SqlNode)sqlStatement));
                return sqlBlock;
            }

            internal override SqlExpression VisitVariable(SqlVariable v)
            {
                return (SqlExpression)v;
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov)
            {
                return (SqlExpression)new SqlOptionalValue(this.VisitExpression(sov.HasValue), this.VisitExpression(sov.Value));
            }

            internal override SqlExpression VisitBetween(SqlBetween between)
            {
                return (SqlExpression)new SqlBetween(between.ClrType, between.SqlType, this.VisitExpression(between.Expression), this.VisitExpression(between.Start), this.VisitExpression(between.End), between.SourceExpression);
            }

            internal override SqlExpression VisitIn(SqlIn sin)
            {
                SqlIn sqlIn = new SqlIn(sin.ClrType, sin.SqlType, this.VisitExpression(sin.Expression), (IEnumerable<SqlExpression>)sin.Values, sin.SourceExpression);
                int index = 0;
                for (int count = sqlIn.Values.Count; index < count; ++index)
                    sqlIn.Values[index] = this.VisitExpression(sqlIn.Values[index]);
                return (SqlExpression)sqlIn;
            }

            internal override SqlExpression VisitLike(SqlLike like)
            {
                return (SqlExpression)new SqlLike(like.ClrType, like.SqlType, this.VisitExpression(like.Expression), this.VisitExpression(like.Pattern), this.VisitExpression(like.Escape), like.SourceExpression);
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                SqlExpression[] sqlExpressionArray = new SqlExpression[fc.Arguments.Count];
                int index = 0;
                for (int count = fc.Arguments.Count; index < count; ++index)
                    sqlExpressionArray[index] = this.VisitExpression(fc.Arguments[index]);
                return (SqlExpression)new SqlFunctionCall(fc.ClrType, fc.SqlType, fc.Name, (IEnumerable<SqlExpression>)sqlExpressionArray, fc.SourceExpression);
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
            {
                SqlExpression[] sqlExpressionArray = new SqlExpression[fc.Arguments.Count];
                int index = 0;
                for (int count = fc.Arguments.Count; index < count; ++index)
                    sqlExpressionArray[index] = this.VisitExpression(fc.Arguments[index]);
                SqlTableValuedFunctionCall valuedFunctionCall = new SqlTableValuedFunctionCall(fc.RowType, fc.ClrType, fc.SqlType, fc.Name, (IEnumerable<SqlExpression>)sqlExpressionArray, fc.SourceExpression);
                this.nodeMap[(SqlNode)fc] = (SqlNode)valuedFunctionCall;
                foreach (SqlColumn sqlColumn in fc.Columns)
                    valuedFunctionCall.Columns.Add((SqlColumn)this.Visit((SqlNode)sqlColumn));
                return (SqlExpression)valuedFunctionCall;
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                SqlExpression[] sqlExpressionArray = new SqlExpression[mc.Arguments.Count];
                int index = 0;
                for (int count = mc.Arguments.Count; index < count; ++index)
                    sqlExpressionArray[index] = this.VisitExpression(mc.Arguments[index]);
                return (SqlExpression)new SqlMethodCall(mc.ClrType, mc.SqlType, mc.Method, this.VisitExpression(mc.Object), (IEnumerable<SqlExpression>)sqlExpressionArray, mc.SourceExpression);
            }

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression sub)
            {
                SqlSharedExpression sharedExpression = new SqlSharedExpression(sub.Expression);
                this.nodeMap[(SqlNode)sub] = (SqlNode)sharedExpression;
                sharedExpression.Expression = this.VisitExpression(sub.Expression);
                return (SqlExpression)sharedExpression;
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
            {
                if (this.ingoreExternalRefs && !this.nodeMap.ContainsKey((SqlNode)sref.SharedExpression))
                    return (SqlExpression)sref;
                return (SqlExpression)new SqlSharedExpressionRef((SqlSharedExpression)this.Visit((SqlNode)sref.SharedExpression));
            }

            internal override SqlExpression VisitSimpleExpression(SqlSimpleExpression simple)
            {
                return (SqlExpression)new SqlSimpleExpression(this.VisitExpression(simple.Expression));
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g)
            {
                return (SqlExpression)new SqlGrouping(g.ClrType, g.SqlType, this.VisitExpression(g.Key), this.VisitExpression(g.Group), g.SourceExpression);
            }

            internal override SqlExpression VisitDiscriminatedType(SqlDiscriminatedType dt)
            {
                return (SqlExpression)new SqlDiscriminatedType(dt.SqlType, this.VisitExpression(dt.Discriminator), dt.TargetType, dt.SourceExpression);
            }

            internal override SqlExpression VisitLift(SqlLift lift)
            {
                return (SqlExpression)new SqlLift(lift.ClrType, this.VisitExpression(lift.Expression), lift.SourceExpression);
            }

            internal override SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof)
            {
                return (SqlExpression)new SqlDiscriminatorOf(this.VisitExpression(dof.Object), dof.ClrType, dof.SqlType, dof.SourceExpression);
            }

            internal override SqlNode VisitIncludeScope(SqlIncludeScope scope)
            {
                return (SqlNode)new SqlIncludeScope(this.Visit(scope.Child), scope.SourceExpression);
            }
        }
    }
}
