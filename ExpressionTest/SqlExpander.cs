using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlExpander
    {
        private SqlFactory factory;

        internal SqlExpander(SqlFactory factory)
        {
            this.factory = factory;
        }

        internal SqlExpression Expand(SqlExpression exp)
        {
            return new SqlExpander.Visitor(this.factory).VisitExpression(exp);
        }

        private class Visitor : SqlDuplicator.DuplicatingVisitor
        {
            private SqlFactory factory;
            private Expression sourceExpression;

            internal Visitor(SqlFactory factory)
              : base(true)
            {
                this.factory = factory;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                return (SqlExpression)cref;
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                return (SqlExpression)new SqlColumnRef(col);
            }

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared)
            {
                return this.VisitExpression(shared.Expression);
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
            {
                return this.VisitExpression(sref.SharedExpression.Expression);
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                SqlNode node = aref.Alias.Node;
                if (node is SqlTable || node is SqlTableValuedFunctionCall)
                    return (SqlExpression)aref;
                SqlUnion union = node as SqlUnion;
                if (union != null)
                    return this.ExpandUnion(union);
                SqlSelect sqlSelect = node as SqlSelect;
                if (sqlSelect != null)
                    return this.VisitExpression(sqlSelect.Selection);
                SqlExpression exp = node as SqlExpression;
                if (exp != null)
                    return this.VisitExpression(exp);
                throw Error.CouldNotHandleAliasRef((object)node.NodeType);
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return (SqlExpression)new SqlDuplicator().Duplicate((SqlNode)ss);
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                SqlExpression expansion = this.VisitExpression(link.Expansion);
                SqlExpression[] sqlExpressionArray = new SqlExpression[link.KeyExpressions.Count];
                int index = 0;
                for (int length = sqlExpressionArray.Length; index < length; ++index)
                    sqlExpressionArray[index] = this.VisitExpression(link.KeyExpressions[index]);
                return (SqlNode)new SqlLink(link.Id, link.RowType, link.ClrType, link.SqlType, link.Expression, link.Member, (IEnumerable<SqlExpression>)sqlExpressionArray, expansion, link.SourceExpression);
            }

            private SqlExpression ExpandUnion(SqlUnion union)
            {
                List<SqlExpression> exprs = new List<SqlExpression>(2);
                this.GatherUnionExpressions((SqlNode)union, exprs);
                this.sourceExpression = union.SourceExpression;
                return this.ExpandTogether(exprs);
            }

            private void GatherUnionExpressions(SqlNode node, List<SqlExpression> exprs)
            {
                SqlUnion sqlUnion = node as SqlUnion;
                if (sqlUnion != null)
                {
                    this.GatherUnionExpressions(sqlUnion.Left, exprs);
                    this.GatherUnionExpressions(sqlUnion.Right, exprs);
                }
                else
                {
                    SqlSelect sqlSelect = node as SqlSelect;
                    if (sqlSelect == null)
                        return;
                    SqlAliasRef sqlAliasRef = sqlSelect.Selection as SqlAliasRef;
                    if (sqlAliasRef != null)
                        this.GatherUnionExpressions(sqlAliasRef.Alias.Node, exprs);
                    else
                        exprs.Add(sqlSelect.Selection);
                }
            }

            private SqlExpression ExpandTogether(List<SqlExpression> exprs)
            {
                switch (exprs[0].NodeType)
                {
                    case SqlNodeType.TypeCase:
                        SqlTypeCase[] sqlTypeCaseArray = new SqlTypeCase[exprs.Count];
                        sqlTypeCaseArray[0] = (SqlTypeCase)exprs[0];
                        for (int index = 1; index < sqlTypeCaseArray.Length; ++index)
                            sqlTypeCaseArray[index] = (SqlTypeCase)exprs[index];
                        List<SqlExpression> exprs1 = new List<SqlExpression>();
                        for (int index = 0; index < sqlTypeCaseArray.Length; ++index)
                            exprs1.Add(sqlTypeCaseArray[index].Discriminator);
                        SqlExpression discriminator = this.ExpandTogether(exprs1);
                        for (int index = 0; index < sqlTypeCaseArray.Length; ++index)
                            sqlTypeCaseArray[index].Discriminator = exprs1[index];
                        List<SqlTypeCaseWhen> list1 = new List<SqlTypeCaseWhen>();
                        for (int index1 = 0; index1 < sqlTypeCaseArray[0].Whens.Count; ++index1)
                        {
                            List<SqlExpression> exprs2 = new List<SqlExpression>();
                            for (int index2 = 0; index2 < sqlTypeCaseArray.Length; ++index2)
                            {
                                SqlTypeCaseWhen sqlTypeCaseWhen = sqlTypeCaseArray[index2].Whens[index1];
                                exprs2.Add(sqlTypeCaseWhen.TypeBinding);
                            }
                            SqlExpression typeBinding = this.ExpandTogether(exprs2);
                            list1.Add(new SqlTypeCaseWhen(sqlTypeCaseArray[0].Whens[index1].Match, typeBinding));
                        }
                        return this.factory.TypeCase(sqlTypeCaseArray[0].ClrType, sqlTypeCaseArray[0].RowType, discriminator, (IEnumerable<SqlTypeCaseWhen>)list1, sqlTypeCaseArray[0].SourceExpression);
                    case SqlNodeType.Value:
                        SqlValue sqlValue = (SqlValue)exprs[0];
                        for (int index = 1; index < exprs.Count; ++index)
                        {
                            if (!object.Equals(((SqlValue)exprs[index]).Value, sqlValue.Value))
                                return this.ExpandIntoExprSet(exprs);
                        }
                        return (SqlExpression)sqlValue;
                    case SqlNodeType.OptionalValue:
                        if (!exprs[0].SqlType.CanBeColumn)
                        {
                            List<SqlExpression> exprs2 = new List<SqlExpression>(exprs.Count);
                            List<SqlExpression> exprs3 = new List<SqlExpression>(exprs.Count);
                            int index = 0;
                            for (int count = exprs.Count; index < count; ++index)
                            {
                                if (exprs[index] == null || exprs[index].NodeType != SqlNodeType.OptionalValue)
                                    throw Error.UnionIncompatibleConstruction();
                                SqlOptionalValue sqlOptionalValue = (SqlOptionalValue)exprs[index];
                                exprs2.Add(sqlOptionalValue.HasValue);
                                exprs3.Add(sqlOptionalValue.Value);
                            }
                            return (SqlExpression)new SqlOptionalValue(this.ExpandTogether(exprs2), this.ExpandTogether(exprs3));
                        }
                        break;
                    case SqlNodeType.OuterJoinedValue:
                        if (!exprs[0].SqlType.CanBeColumn)
                        {
                            List<SqlExpression> exprs2 = new List<SqlExpression>(exprs.Count);
                            int index = 0;
                            for (int count = exprs.Count; index < count; ++index)
                            {
                                if (exprs[index] == null || exprs[index].NodeType != SqlNodeType.OuterJoinedValue)
                                    throw Error.UnionIncompatibleConstruction();
                                SqlUnary sqlUnary = (SqlUnary)exprs[index];
                                exprs2.Add(sqlUnary.Operand);
                            }
                            return (SqlExpression)this.factory.Unary(SqlNodeType.OuterJoinedValue, this.ExpandTogether(exprs2));
                        }
                        break;
                    case SqlNodeType.MethodCall:
                        SqlMethodCall[] sqlMethodCallArray = new SqlMethodCall[exprs.Count];
                        for (int index = 0; index < sqlMethodCallArray.Length; ++index)
                            sqlMethodCallArray[index] = (SqlMethodCall)exprs[index];
                        List<SqlExpression> list2 = new List<SqlExpression>();
                        for (int index1 = 0; index1 < sqlMethodCallArray[0].Arguments.Count; ++index1)
                        {
                            List<SqlExpression> exprs2 = new List<SqlExpression>();
                            for (int index2 = 0; index2 < sqlMethodCallArray.Length; ++index2)
                                exprs2.Add(sqlMethodCallArray[index2].Arguments[index1]);
                            SqlExpression sqlExpression = this.ExpandTogether(exprs2);
                            list2.Add(sqlExpression);
                        }
                        return (SqlExpression)this.factory.MethodCall(sqlMethodCallArray[0].Method, sqlMethodCallArray[0].Object, list2.ToArray(), sqlMethodCallArray[0].SourceExpression);
                    case SqlNodeType.Multiset:
                    case SqlNodeType.Element:
                    case SqlNodeType.Grouping:
                    case SqlNodeType.ClientQuery:
                        throw Error.UnionWithHierarchy();
                    case SqlNodeType.New:
                        SqlNew[] sqlNewArray = new SqlNew[exprs.Count];
                        sqlNewArray[0] = (SqlNew)exprs[0];
                        int index3 = 1;
                        for (int count1 = exprs.Count; index3 < count1; ++index3)
                        {
                            if (exprs[index3] == null || exprs[index3].NodeType != SqlNodeType.New)
                                throw Error.UnionIncompatibleConstruction();
                            sqlNewArray[index3] = (SqlNew)exprs[1];
                            if (sqlNewArray[index3].Members.Count != sqlNewArray[0].Members.Count)
                                throw Error.UnionDifferentMembers();
                            int index1 = 0;
                            for (int count2 = sqlNewArray[0].Members.Count; index1 < count2; ++index1)
                            {
                                if (sqlNewArray[index3].Members[index1].Member != sqlNewArray[0].Members[index1].Member)
                                    throw Error.UnionDifferentMemberOrder();
                            }
                        }
                        SqlMemberAssign[] sqlMemberAssignArray = new SqlMemberAssign[sqlNewArray[0].Members.Count];
                        int index4 = 0;
                        for (int length = sqlMemberAssignArray.Length; index4 < length; ++index4)
                        {
                            List<SqlExpression> exprs2 = new List<SqlExpression>();
                            int index1 = 0;
                            for (int count = exprs.Count; index1 < count; ++index1)
                                exprs2.Add(sqlNewArray[index1].Members[index4].Expression);
                            sqlMemberAssignArray[index4] = new SqlMemberAssign(sqlNewArray[0].Members[index4].Member, this.ExpandTogether(exprs2));
                            int index2 = 0;
                            for (int count = exprs.Count; index2 < count; ++index2)
                                sqlNewArray[index2].Members[index4].Expression = exprs2[index2];
                        }
                        SqlExpression[] sqlExpressionArray1 = new SqlExpression[sqlNewArray[0].Args.Count];
                        int index5 = 0;
                        for (int length = sqlExpressionArray1.Length; index5 < length; ++index5)
                        {
                            List<SqlExpression> exprs2 = new List<SqlExpression>();
                            int index1 = 0;
                            for (int count = exprs.Count; index1 < count; ++index1)
                                exprs2.Add(sqlNewArray[index1].Args[index5]);
                            sqlExpressionArray1[index5] = this.ExpandTogether(exprs2);
                        }
                        return (SqlExpression)this.factory.New(sqlNewArray[0].MetaType, sqlNewArray[0].Constructor, (IEnumerable<SqlExpression>)sqlExpressionArray1, (IEnumerable<MemberInfo>)sqlNewArray[0].ArgMembers, (IEnumerable<SqlMemberAssign>)sqlMemberAssignArray, exprs[0].SourceExpression);
                    case SqlNodeType.Link:
                        SqlLink[] sqlLinkArray = new SqlLink[exprs.Count];
                        sqlLinkArray[0] = (SqlLink)exprs[0];
                        int index6 = 1;
                        for (int count = exprs.Count; index6 < count; ++index6)
                        {
                            if (exprs[index6] == null || exprs[index6].NodeType != SqlNodeType.Link)
                                throw Error.UnionIncompatibleConstruction();
                            sqlLinkArray[index6] = (SqlLink)exprs[index6];
                            if (sqlLinkArray[index6].KeyExpressions.Count != sqlLinkArray[0].KeyExpressions.Count || sqlLinkArray[index6].Member != sqlLinkArray[0].Member || sqlLinkArray[index6].Expansion != null != (sqlLinkArray[0].Expansion != null))
                                throw Error.UnionIncompatibleConstruction();
                        }
                        SqlExpression[] sqlExpressionArray2 = new SqlExpression[sqlLinkArray[0].KeyExpressions.Count];
                        List<SqlExpression> exprs4 = new List<SqlExpression>();
                        int index7 = 0;
                        for (int count1 = sqlLinkArray[0].KeyExpressions.Count; index7 < count1; ++index7)
                        {
                            exprs4.Clear();
                            int index1 = 0;
                            for (int count2 = exprs.Count; index1 < count2; ++index1)
                                exprs4.Add(sqlLinkArray[index1].KeyExpressions[index7]);
                            sqlExpressionArray2[index7] = this.ExpandTogether(exprs4);
                            int index2 = 0;
                            for (int count2 = exprs.Count; index2 < count2; ++index2)
                                sqlLinkArray[index2].KeyExpressions[index7] = exprs4[index2];
                        }
                        SqlExpression expansion = (SqlExpression)null;
                        if (sqlLinkArray[0].Expansion != null)
                        {
                            exprs4.Clear();
                            int index1 = 0;
                            for (int count = exprs.Count; index1 < count; ++index1)
                                exprs4.Add(sqlLinkArray[index1].Expansion);
                            expansion = this.ExpandTogether(exprs4);
                            int index2 = 0;
                            for (int count = exprs.Count; index2 < count; ++index2)
                                sqlLinkArray[index2].Expansion = exprs4[index2];
                        }
                        return (SqlExpression)new SqlLink(sqlLinkArray[0].Id, sqlLinkArray[0].RowType, sqlLinkArray[0].ClrType, sqlLinkArray[0].SqlType, sqlLinkArray[0].Expression, sqlLinkArray[0].Member, (IEnumerable<SqlExpression>)sqlExpressionArray2, expansion, sqlLinkArray[0].SourceExpression);
                    case SqlNodeType.ClientCase:
                        SqlClientCase[] sqlClientCaseArray = new SqlClientCase[exprs.Count];
                        sqlClientCaseArray[0] = (SqlClientCase)exprs[0];
                        for (int index1 = 1; index1 < sqlClientCaseArray.Length; ++index1)
                            sqlClientCaseArray[index1] = (SqlClientCase)exprs[index1];
                        List<SqlExpression> exprs5 = new List<SqlExpression>();
                        for (int index1 = 0; index1 < sqlClientCaseArray.Length; ++index1)
                            exprs5.Add(sqlClientCaseArray[index1].Expression);
                        SqlExpression expr = this.ExpandTogether(exprs5);
                        List<SqlClientWhen> list3 = new List<SqlClientWhen>();
                        for (int index1 = 0; index1 < sqlClientCaseArray[0].Whens.Count; ++index1)
                        {
                            List<SqlExpression> exprs2 = new List<SqlExpression>();
                            for (int index2 = 0; index2 < sqlClientCaseArray.Length; ++index2)
                            {
                                SqlClientWhen sqlClientWhen = sqlClientCaseArray[index2].Whens[index1];
                                exprs2.Add(sqlClientWhen.Value);
                            }
                            list3.Add(new SqlClientWhen(sqlClientCaseArray[0].Whens[index1].Match, this.ExpandTogether(exprs2)));
                        }
                        return (SqlExpression)new SqlClientCase(sqlClientCaseArray[0].ClrType, expr, (IEnumerable<SqlClientWhen>)list3, sqlClientCaseArray[0].SourceExpression);
                    case SqlNodeType.DiscriminatedType:
                        SqlDiscriminatedType discriminatedType1 = (SqlDiscriminatedType)exprs[0];
                        List<SqlExpression> exprs6 = new List<SqlExpression>(exprs.Count);
                        exprs6.Add(discriminatedType1.Discriminator);
                        int index8 = 1;
                        for (int count = exprs.Count; index8 < count; ++index8)
                        {
                            SqlDiscriminatedType discriminatedType2 = (SqlDiscriminatedType)exprs[index8];
                            if (discriminatedType2.TargetType != discriminatedType1.TargetType)
                                throw Error.UnionIncompatibleConstruction();
                            exprs6.Add(discriminatedType2.Discriminator);
                        }
                        return this.factory.DiscriminatedType(this.ExpandTogether(exprs6), ((SqlDiscriminatedType)exprs[0]).TargetType);
                }
                return this.ExpandIntoExprSet(exprs);
            }

            private SqlExpression ExpandIntoExprSet(List<SqlExpression> exprs)
            {
                SqlExpression[] exprs1 = new SqlExpression[exprs.Count];
                int index = 0;
                for (int count = exprs.Count; index < count; ++index)
                    exprs1[index] = this.VisitExpression(exprs[index]);
                return (SqlExpression)this.factory.ExprSet(exprs1, this.sourceExpression);
            }
        }
    }
}
