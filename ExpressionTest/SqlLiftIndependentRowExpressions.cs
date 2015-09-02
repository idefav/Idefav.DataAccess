using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlLiftIndependentRowExpressions
    {
        internal static SqlNode Lift(SqlNode node)
        {
            node = new SqlLiftIndependentRowExpressions.ColumnLifter().Visit(node);
            return node;
        }

        private class ColumnLifter : SqlVisitor
        {
            private SqlLiftIndependentRowExpressions.ColumnLifter.SelectScope expressionSink;
            private SqlAggregateChecker aggregateChecker;

            internal ColumnLifter()
            {
                this.aggregateChecker = new SqlAggregateChecker();
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SqlLiftIndependentRowExpressions.ColumnLifter.SelectScope selectScope = this.expressionSink;
                if (select.Top != null)
                    this.expressionSink = (SqlLiftIndependentRowExpressions.ColumnLifter.SelectScope)null;
                if (select.GroupBy.Count > 0 || this.aggregateChecker.HasAggregates((SqlNode)select))
                    this.expressionSink = (SqlLiftIndependentRowExpressions.ColumnLifter.SelectScope)null;
                if (select.IsDistinct)
                    this.expressionSink = (SqlLiftIndependentRowExpressions.ColumnLifter.SelectScope)null;
                if (this.expressionSink != null)
                {
                    List<SqlColumn> list1 = new List<SqlColumn>();
                    List<SqlColumn> list2 = new List<SqlColumn>();
                    foreach (SqlColumn sqlColumn in select.Row.Columns)
                    {
                        if (SqlAliasesReferenced.ReferencesAny((SqlNode)sqlColumn.Expression, this.expressionSink.LeftProduction) && !this.expressionSink.ReferencedExpressions.Contains(sqlColumn))
                            list2.Add(sqlColumn);
                        else
                            list1.Add(sqlColumn);
                    }
                    select.Row.Columns.Clear();
                    select.Row.Columns.AddRange((IEnumerable<SqlColumn>)list1);
                    if (list2.Count > 0)
                        this.expressionSink.Lifted.Push(list2);
                }
                SqlSelect sqlSelect = base.VisitSelect(select);
                this.expressionSink = selectScope;
                return sqlSelect;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                if (this.expressionSink != null)
                    this.expressionSink.ReferencedExpressions.Add(cref.Column);
                return (SqlExpression)cref;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                if (join.JoinType != SqlJoinType.CrossApply)
                    return base.VisitJoin(join);
                join.Left = this.VisitSource(join.Left);
                join.Condition = this.VisitExpression(join.Condition);
                SqlLiftIndependentRowExpressions.ColumnLifter.SelectScope selectScope = this.expressionSink;
                this.expressionSink = new SqlLiftIndependentRowExpressions.ColumnLifter.SelectScope();
                this.expressionSink.LeftProduction = (IEnumerable<SqlAlias>)SqlGatherProducedAliases.Gather((SqlNode)join.Left);
                join.Right = this.VisitSource(join.Right);
                SqlSource sqlSource = (SqlSource)join;
                foreach (List<SqlColumn> cols in this.expressionSink.Lifted)
                    sqlSource = this.PushSourceDown(sqlSource, cols);
                this.expressionSink = selectScope;
                return sqlSource;
            }

            private SqlSource PushSourceDown(SqlSource sqlSource, List<SqlColumn> cols)
            {
                SqlNop sqlNop = new SqlNop(cols[0].ClrType, cols[0].SqlType, sqlSource.SourceExpression);
                SqlSource from = sqlSource;
                Expression sourceExpression = from.SourceExpression;
                SqlSelect sqlSelect = new SqlSelect((SqlExpression)sqlNop, from, sourceExpression);
                sqlSelect.Row.Columns.AddRange((IEnumerable<SqlColumn>)cols);
                return (SqlSource)new SqlAlias((SqlNode)sqlSelect);
            }

            private class SelectScope
            {
                internal Stack<List<SqlColumn>> Lifted = new Stack<List<SqlColumn>>();
                internal HashSet<SqlColumn> ReferencedExpressions = new HashSet<SqlColumn>();
                internal IEnumerable<SqlAlias> LeftProduction;
            }
        }
    }
}
