using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlComparer
    {
        internal SqlComparer()
        {
        }

        internal static bool AreEqual(SqlNode node1, SqlNode node2)
        {
            if (node1 == node2)
                return true;
            if (node1 == null || node2 == null)
                return false;
            if (node1.NodeType == SqlNodeType.SimpleCase)
                node1 = (SqlNode)SqlComparer.UnwrapTrivialCaseExpression((SqlSimpleCase)node1);
            if (node2.NodeType == SqlNodeType.SimpleCase)
                node2 = (SqlNode)SqlComparer.UnwrapTrivialCaseExpression((SqlSimpleCase)node2);
            if (node1.NodeType != node2.NodeType)
            {
                if (node1.NodeType == SqlNodeType.ExprSet)
                {
                    SqlExprSet sqlExprSet = (SqlExprSet)node1;
                    int index = 0;
                    for (int count = sqlExprSet.Expressions.Count; index < count; ++index)
                    {
                        if (SqlComparer.AreEqual((SqlNode)sqlExprSet.Expressions[index], node2))
                            return true;
                    }
                }
                else if (node2.NodeType == SqlNodeType.ExprSet)
                {
                    SqlExprSet sqlExprSet = (SqlExprSet)node2;
                    int index = 0;
                    for (int count = sqlExprSet.Expressions.Count; index < count; ++index)
                    {
                        if (SqlComparer.AreEqual(node1, (SqlNode)sqlExprSet.Expressions[index]))
                            return true;
                    }
                }
                return false;
            }
            if (node1.Equals((object)node2))
                return true;
            switch (node1.NodeType)
            {
                case SqlNodeType.Add:
                case SqlNodeType.And:
                case SqlNodeType.BitAnd:
                case SqlNodeType.BitOr:
                case SqlNodeType.BitXor:
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
                    SqlBinary sqlBinary1 = (SqlBinary)node1;
                    SqlBinary sqlBinary2 = (SqlBinary)node2;
                    if (SqlComparer.AreEqual((SqlNode)sqlBinary1.Left, (SqlNode)sqlBinary2.Left))
                        return SqlComparer.AreEqual((SqlNode)sqlBinary1.Right, (SqlNode)sqlBinary2.Right);
                    return false;
                case SqlNodeType.Alias:
                    return SqlComparer.AreEqual(((SqlAlias)node1).Node, ((SqlAlias)node2).Node);
                case SqlNodeType.AliasRef:
                    return SqlComparer.AreEqual((SqlNode)((SqlAliasRef)node1).Alias, (SqlNode)((SqlAliasRef)node2).Alias);
                case SqlNodeType.Avg:
                case SqlNodeType.BitNot:
                case SqlNodeType.ClrLength:
                case SqlNodeType.Count:
                case SqlNodeType.IsNotNull:
                case SqlNodeType.IsNull:
                case SqlNodeType.Max:
                case SqlNodeType.Min:
                case SqlNodeType.Negate:
                case SqlNodeType.Not:
                case SqlNodeType.Not2V:
                case SqlNodeType.OuterJoinedValue:
                case SqlNodeType.Stddev:
                case SqlNodeType.Sum:
                case SqlNodeType.ValueOf:
                    return SqlComparer.AreEqual((SqlNode)((SqlUnary)node1).Operand, (SqlNode)((SqlUnary)node2).Operand);
                case SqlNodeType.Between:
                    SqlBetween sqlBetween1 = (SqlBetween)node1;
                    SqlBetween sqlBetween2 = (SqlBetween)node1;
                    if (SqlComparer.AreEqual((SqlNode)sqlBetween1.Expression, (SqlNode)sqlBetween2.Expression) && SqlComparer.AreEqual((SqlNode)sqlBetween1.Start, (SqlNode)sqlBetween2.Start))
                        return SqlComparer.AreEqual((SqlNode)sqlBetween1.End, (SqlNode)sqlBetween2.End);
                    return false;
                case SqlNodeType.ClientCase:
                    SqlClientCase sqlClientCase1 = (SqlClientCase)node1;
                    SqlClientCase sqlClientCase2 = (SqlClientCase)node2;
                    if (sqlClientCase1.Whens.Count != sqlClientCase2.Whens.Count)
                        return false;
                    int index1 = 0;
                    for (int count = sqlClientCase1.Whens.Count; index1 < count; ++index1)
                    {
                        if (!SqlComparer.AreEqual((SqlNode)sqlClientCase1.Whens[index1].Match, (SqlNode)sqlClientCase2.Whens[index1].Match) || !SqlComparer.AreEqual((SqlNode)sqlClientCase1.Whens[index1].Value, (SqlNode)sqlClientCase2.Whens[index1].Value))
                            return false;
                    }
                    return true;
                case SqlNodeType.Column:
                    SqlColumn sqlColumn1 = (SqlColumn)node1;
                    SqlColumn sqlColumn2 = (SqlColumn)node2;
                    if (sqlColumn1 == sqlColumn2)
                        return true;
                    if (sqlColumn1.Expression != null && sqlColumn2.Expression != null)
                        return SqlComparer.AreEqual((SqlNode)sqlColumn1.Expression, (SqlNode)sqlColumn2.Expression);
                    return false;
                case SqlNodeType.ColumnRef:
                    return SqlComparer.GetBaseColumn((SqlColumnRef)node1) == SqlComparer.GetBaseColumn((SqlColumnRef)node2);
                case SqlNodeType.Convert:
                case SqlNodeType.Treat:
                    SqlUnary sqlUnary1 = (SqlUnary)node1;
                    SqlUnary sqlUnary2 = (SqlUnary)node2;
                    if (sqlUnary1.ClrType == sqlUnary2.ClrType && sqlUnary1.SqlType == sqlUnary2.SqlType)
                        return SqlComparer.AreEqual((SqlNode)sqlUnary1.Operand, (SqlNode)sqlUnary2.Operand);
                    return false;
                case SqlNodeType.DiscriminatedType:
                    return SqlComparer.AreEqual((SqlNode)((SqlDiscriminatedType)node1).Discriminator, (SqlNode)((SqlDiscriminatedType)node2).Discriminator);
                case SqlNodeType.ExprSet:
                    SqlExprSet sqlExprSet1 = (SqlExprSet)node1;
                    SqlExprSet sqlExprSet2 = (SqlExprSet)node2;
                    if (sqlExprSet1.Expressions.Count != sqlExprSet2.Expressions.Count)
                        return false;
                    int index2 = 0;
                    for (int count = sqlExprSet1.Expressions.Count; index2 < count; ++index2)
                    {
                        if (!SqlComparer.AreEqual((SqlNode)sqlExprSet1.Expressions[index2], (SqlNode)sqlExprSet2.Expressions[index2]))
                            return false;
                    }
                    return true;
                case SqlNodeType.FunctionCall:
                    SqlFunctionCall sqlFunctionCall1 = (SqlFunctionCall)node1;
                    SqlFunctionCall sqlFunctionCall2 = (SqlFunctionCall)node2;
                    if (sqlFunctionCall1.Name != sqlFunctionCall2.Name || sqlFunctionCall1.Arguments.Count != sqlFunctionCall2.Arguments.Count)
                        return false;
                    int index3 = 0;
                    for (int count = sqlFunctionCall1.Arguments.Count; index3 < count; ++index3)
                    {
                        if (!SqlComparer.AreEqual((SqlNode)sqlFunctionCall1.Arguments[index3], (SqlNode)sqlFunctionCall2.Arguments[index3]))
                            return false;
                    }
                    return true;
                case SqlNodeType.Link:
                    SqlLink sqlLink1 = (SqlLink)node1;
                    SqlLink sqlLink2 = (SqlLink)node2;
                    if (!MetaPosition.AreSameMember(sqlLink1.Member.Member, sqlLink2.Member.Member) || !SqlComparer.AreEqual((SqlNode)sqlLink1.Expansion, (SqlNode)sqlLink2.Expansion) || sqlLink1.KeyExpressions.Count != sqlLink2.KeyExpressions.Count)
                        return false;
                    int index4 = 0;
                    for (int count = sqlLink1.KeyExpressions.Count; index4 < count; ++index4)
                    {
                        if (!SqlComparer.AreEqual((SqlNode)sqlLink1.KeyExpressions[index4], (SqlNode)sqlLink2.KeyExpressions[index4]))
                            return false;
                    }
                    return true;
                case SqlNodeType.Like:
                    SqlLike sqlLike1 = (SqlLike)node1;
                    SqlLike sqlLike2 = (SqlLike)node2;
                    if (SqlComparer.AreEqual((SqlNode)sqlLike1.Expression, (SqlNode)sqlLike2.Expression) && SqlComparer.AreEqual((SqlNode)sqlLike1.Pattern, (SqlNode)sqlLike2.Pattern))
                        return SqlComparer.AreEqual((SqlNode)sqlLike1.Escape, (SqlNode)sqlLike2.Escape);
                    return false;
                case SqlNodeType.Member:
                    if (((SqlMember)node1).Member == ((SqlMember)node2).Member)
                        return SqlComparer.AreEqual((SqlNode)((SqlMember)node1).Expression, (SqlNode)((SqlMember)node2).Expression);
                    return false;
                case SqlNodeType.OptionalValue:
                    return SqlComparer.AreEqual((SqlNode)((SqlOptionalValue)node1).Value, (SqlNode)((SqlOptionalValue)node2).Value);
                case SqlNodeType.Parameter:
                    return node1 == node2;
                case SqlNodeType.SearchedCase:
                    SqlSearchedCase sqlSearchedCase1 = (SqlSearchedCase)node1;
                    SqlSearchedCase sqlSearchedCase2 = (SqlSearchedCase)node2;
                    if (sqlSearchedCase1.Whens.Count != sqlSearchedCase2.Whens.Count)
                        return false;
                    int index5 = 0;
                    for (int count = sqlSearchedCase1.Whens.Count; index5 < count; ++index5)
                    {
                        if (!SqlComparer.AreEqual((SqlNode)sqlSearchedCase1.Whens[index5].Match, (SqlNode)sqlSearchedCase2.Whens[index5].Match) || !SqlComparer.AreEqual((SqlNode)sqlSearchedCase1.Whens[index5].Value, (SqlNode)sqlSearchedCase2.Whens[index5].Value))
                            return false;
                    }
                    return SqlComparer.AreEqual((SqlNode)sqlSearchedCase1.Else, (SqlNode)sqlSearchedCase2.Else);
                case SqlNodeType.SimpleCase:
                    SqlSimpleCase sqlSimpleCase1 = (SqlSimpleCase)node1;
                    SqlSimpleCase sqlSimpleCase2 = (SqlSimpleCase)node2;
                    if (sqlSimpleCase1.Whens.Count != sqlSimpleCase2.Whens.Count)
                        return false;
                    int index6 = 0;
                    for (int count = sqlSimpleCase1.Whens.Count; index6 < count; ++index6)
                    {
                        if (!SqlComparer.AreEqual((SqlNode)sqlSimpleCase1.Whens[index6].Match, (SqlNode)sqlSimpleCase2.Whens[index6].Match) || !SqlComparer.AreEqual((SqlNode)sqlSimpleCase1.Whens[index6].Value, (SqlNode)sqlSimpleCase2.Whens[index6].Value))
                            return false;
                    }
                    return true;
                case SqlNodeType.Table:
                    return ((SqlTable)node1).MetaTable == ((SqlTable)node2).MetaTable;
                case SqlNodeType.TypeCase:
                    SqlTypeCase sqlTypeCase1 = (SqlTypeCase)node1;
                    SqlTypeCase sqlTypeCase2 = (SqlTypeCase)node2;
                    if (!SqlComparer.AreEqual((SqlNode)sqlTypeCase1.Discriminator, (SqlNode)sqlTypeCase2.Discriminator) || sqlTypeCase1.Whens.Count != sqlTypeCase2.Whens.Count)
                        return false;
                    int index7 = 0;
                    for (int count = sqlTypeCase1.Whens.Count; index7 < count; ++index7)
                    {
                        if (!SqlComparer.AreEqual((SqlNode)sqlTypeCase1.Whens[index7].Match, (SqlNode)sqlTypeCase2.Whens[index7].Match) || !SqlComparer.AreEqual((SqlNode)sqlTypeCase1.Whens[index7].TypeBinding, (SqlNode)sqlTypeCase2.Whens[index7].TypeBinding))
                            return false;
                    }
                    return true;
                case SqlNodeType.Variable:
                    return ((SqlVariable)node1).Name == ((SqlVariable)node2).Name;
                case SqlNodeType.Value:
                    return object.Equals(((SqlValue)node1).Value, ((SqlValue)node2).Value);
                default:
                    return false;
            }
        }

        private static SqlColumn GetBaseColumn(SqlColumnRef cref)
        {
            SqlColumnRef sqlColumnRef;
            for (; cref != null && cref.Column.Expression != null; cref = sqlColumnRef)
            {
                sqlColumnRef = cref.Column.Expression as SqlColumnRef;
                if (sqlColumnRef == null)
                    break;
            }
            return cref.Column;
        }

        private static SqlExpression UnwrapTrivialCaseExpression(SqlSimpleCase sc)
        {
            if (sc.Whens.Count != 1)
                return (SqlExpression)sc;
            if (!SqlComparer.AreEqual((SqlNode)sc.Expression, (SqlNode)sc.Whens[0].Match))
                return (SqlExpression)sc;
            SqlExpression sqlExpression = sc.Whens[0].Value;
            if (sqlExpression.NodeType == SqlNodeType.SimpleCase)
                return SqlComparer.UnwrapTrivialCaseExpression((SqlSimpleCase)sqlExpression);
            return sqlExpression;
        }
    }
}
