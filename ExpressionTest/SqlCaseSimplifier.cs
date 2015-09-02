using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlCaseSimplifier
    {
        internal static SqlNode Simplify(SqlNode node, SqlFactory sql)
        {
            return new SqlCaseSimplifier.Visitor(sql).Visit(node);
        }

        private class Visitor : SqlVisitor
        {
            private SqlFactory sql;

            internal Visitor(SqlFactory sql)
            {
                this.sql = sql;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                switch (bo.NodeType)
                {
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        if (bo.Left.NodeType == SqlNodeType.SimpleCase && bo.Right.NodeType == SqlNodeType.Value && this.AreCaseWhenValuesConstant((SqlSimpleCase)bo.Left))
                            return this.DistributeOperatorIntoCase(bo.NodeType, (SqlSimpleCase)bo.Left, bo.Right);
                        if (bo.Right.NodeType == SqlNodeType.SimpleCase && bo.Left.NodeType == SqlNodeType.Value && this.AreCaseWhenValuesConstant((SqlSimpleCase)bo.Right))
                            return this.DistributeOperatorIntoCase(bo.NodeType, (SqlSimpleCase)bo.Right, bo.Left);
                        break;
                }
                return base.VisitBinaryOperator(bo);
            }

            internal bool AreCaseWhenValuesConstant(SqlSimpleCase sc)
            {
                foreach (SqlWhen sqlWhen in sc.Whens)
                {
                    if (sqlWhen.Value.NodeType != SqlNodeType.Value)
                        return false;
                }
                return true;
            }

            private SqlExpression DistributeOperatorIntoCase(SqlNodeType nt, SqlSimpleCase sc, SqlExpression expr)
            {
                if (nt != SqlNodeType.EQ && nt != SqlNodeType.NE && (nt != SqlNodeType.EQ2V && nt != SqlNodeType.NE2V))
                    throw Error.ArgumentOutOfRange("nt");
                object o2 = this.Eval(expr);
                List<SqlExpression> values = new List<SqlExpression>();
                List<SqlExpression> matches = new List<SqlExpression>();
                foreach (SqlWhen sqlWhen in sc.Whens)
                {
                    matches.Add(sqlWhen.Match);
                    object o1 = this.Eval(sqlWhen.Value);
                    bool flag = sqlWhen.Value.SqlType.AreValuesEqual(o1, o2);
                    values.Add(this.sql.ValueFromObject(((nt == SqlNodeType.EQ ? 1 : (nt == SqlNodeType.EQ2V ? 1 : 0)) == (flag ? 1 : 0) ? 1 : 0), false, sc.SourceExpression));
                }
                return this.VisitExpression(this.sql.Case(typeof(bool), sc.Expression, matches, values, sc.SourceExpression));
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                c.Expression = this.VisitExpression(c.Expression);
                int index1 = 0;
                int index2 = 0;
                for (int count = c.Whens.Count; index2 < count; ++index2)
                {
                    if (c.Whens[index2].Match == null)
                    {
                        index1 = index2;
                        break;
                    }
                }
                c.Whens[index1].Match = this.VisitExpression(c.Whens[index1].Match);
                c.Whens[index1].Value = this.VisitExpression(c.Whens[index1].Value);
                List<SqlWhen> newWhens = new List<SqlWhen>();
                bool allValuesLiteral = true;
                int index3 = 0;
                for (int count = c.Whens.Count; index3 < count; ++index3)
                {
                    if (index1 != index3)
                    {
                        SqlWhen sqlWhen = c.Whens[index3];
                        sqlWhen.Match = this.VisitExpression(sqlWhen.Match);
                        sqlWhen.Value = this.VisitExpression(sqlWhen.Value);
                        if (!SqlComparer.AreEqual((SqlNode)c.Whens[index1].Value, (SqlNode)sqlWhen.Value))
                            newWhens.Add(sqlWhen);
                        allValuesLiteral = allValuesLiteral && sqlWhen.Value.NodeType == SqlNodeType.Value;
                    }
                }
                newWhens.Add(c.Whens[index1]);
                return this.TryToConsolidateAllValueExpressions(newWhens.Count, c.Whens[index1].Value) ?? this.TryToWriteAsSimpleBooleanExpression(c.ClrType, c.Expression, newWhens, allValuesLiteral) ?? this.TryToWriteAsReducedCase(c.ClrType, c.Expression, newWhens, c.Whens[index1].Match, c.Whens.Count) ?? (SqlExpression)c;
            }

            private SqlExpression TryToConsolidateAllValueExpressions(int valueCount, SqlExpression value)
            {
                if (valueCount == 1)
                    return value;
                return (SqlExpression)null;
            }

            private SqlExpression TryToWriteAsSimpleBooleanExpression(Type caseType, SqlExpression discriminator, List<SqlWhen> newWhens, bool allValuesLiteral)
            {
                SqlExpression left1 = (SqlExpression)null;
                if (caseType == typeof(bool) & allValuesLiteral)
                {
                    bool? nullable1 = SqlExpressionNullability.CanBeNull(discriminator);
                    bool? nullable2 = new bool?();
                    for (int index = 0; index < newWhens.Count; ++index)
                    {
                        bool flag = (bool)((SqlValue)newWhens[index].Value).Value;
                        if (newWhens[index].Match != null)
                            left1 = !flag ? this.sql.AndAccumulate(left1, (SqlExpression)this.sql.Binary(SqlNodeType.NE, discriminator, newWhens[index].Match)) : this.sql.OrAccumulate(left1, (SqlExpression)this.sql.Binary(SqlNodeType.EQ, discriminator, newWhens[index].Match));
                        else
                            nullable2 = new bool?(flag);
                    }
                    bool? nullable3 = nullable1;
                    bool flag1 = false;
                    if ((nullable3.GetValueOrDefault() == flag1 ? (!nullable3.HasValue ? 1 : 0) : 1) != 0 && nullable2.HasValue)
                    {
                        bool? nullable4 = nullable2;
                        bool flag2 = true;
                        if ((nullable4.GetValueOrDefault() == flag2 ? (nullable4.HasValue ? 1 : 0) : 0) != 0)
                        {
                            SqlFactory sqlFactory1 = this.sql;
                            SqlExpression left2 = left1;
                            SqlFactory sqlFactory2 = this.sql;
                            int num = 37;
                            SqlExpression expression = discriminator;
                            Expression sourceExpression = expression.SourceExpression;
                            SqlUnary sqlUnary = sqlFactory2.Unary((SqlNodeType)num, expression, sourceExpression);
                            left1 = sqlFactory1.OrAccumulate(left2, (SqlExpression)sqlUnary);
                        }
                        else
                        {
                            SqlFactory sqlFactory1 = this.sql;
                            SqlExpression left2 = left1;
                            SqlFactory sqlFactory2 = this.sql;
                            int num = 36;
                            SqlExpression expression = discriminator;
                            Expression sourceExpression = expression.SourceExpression;
                            SqlUnary sqlUnary = sqlFactory2.Unary((SqlNodeType)num, expression, sourceExpression);
                            left1 = sqlFactory1.AndAccumulate(left2, (SqlExpression)sqlUnary);
                        }
                    }
                }
                return left1;
            }

            private SqlExpression TryToWriteAsReducedCase(Type caseType, SqlExpression discriminator, List<SqlWhen> newWhens, SqlExpression elseCandidate, int originalWhenCount)
            {
                if (newWhens.Count != originalWhenCount && elseCandidate == null)
                    return (SqlExpression)new SqlSimpleCase(caseType, discriminator, (IEnumerable<SqlWhen>)newWhens, discriminator.SourceExpression);
                return (SqlExpression)null;
            }
        }
    }
}
