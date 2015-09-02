using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlOuterApplyReducer
    {
        internal static SqlNode Reduce(SqlNode node, SqlFactory factory, SqlNodeAnnotations annotations)
        {
            return new SqlOuterApplyReducer.Visitor(factory, annotations).Visit(node);
        }

        private class Visitor : SqlVisitor
        {
            private SqlFactory factory;
            private SqlNodeAnnotations annotations;

            internal Visitor(SqlFactory factory, SqlNodeAnnotations annotations)
            {
                this.factory = factory;
                this.annotations = annotations;
            }

            internal override SqlSource VisitSource(SqlSource source)
            {
                source = base.VisitSource(source);
                SqlJoin sqlJoin1 = source as SqlJoin;
                if (sqlJoin1 != null)
                {
                    if (sqlJoin1.JoinType == SqlJoinType.OuterApply)
                    {
                        HashSet<SqlAlias> hashSet1 = SqlGatherProducedAliases.Gather((SqlNode)sqlJoin1.Left);
                        HashSet<SqlExpression> hashSet2 = new HashSet<SqlExpression>();
                        if (SqlOuterApplyReducer.SqlPredicateLifter.CanLift(sqlJoin1.Right, hashSet1, hashSet2) && SqlOuterApplyReducer.SqlSelectionLifter.CanLift(sqlJoin1.Right, hashSet1, hashSet2) && !SqlOuterApplyReducer.SqlAliasDependencyChecker.IsDependent((SqlNode)sqlJoin1.Right, hashSet1, hashSet2))
                        {
                            SqlExpression sqlExpression = SqlOuterApplyReducer.SqlPredicateLifter.Lift(sqlJoin1.Right, hashSet1);
                            List<List<SqlColumn>> list = SqlOuterApplyReducer.SqlSelectionLifter.Lift(sqlJoin1.Right, hashSet1, hashSet2);
                            sqlJoin1.JoinType = SqlJoinType.LeftOuter;
                            sqlJoin1.Condition = sqlExpression;
                            if (list != null)
                            {
                                foreach (List<SqlColumn> cols in list)
                                    source = this.PushSourceDown(source, cols);
                            }
                        }
                        else
                        {
                            SqlJoin sqlJoin2 = sqlJoin1;
                            SqlProvider.ProviderMode[] providerModeArray = new SqlProvider.ProviderMode[1];
                            int index = 0;
                            int num = 1;
                            providerModeArray[index] = (SqlProvider.ProviderMode)num;
                            this.AnnotateSqlIncompatibility((SqlNode)sqlJoin2, providerModeArray);
                        }
                    }
                    else if (sqlJoin1.JoinType == SqlJoinType.CrossApply)
                    {
                        SqlJoin unreferencedSingletonOnLeft = this.GetLeftOuterWithUnreferencedSingletonOnLeft(sqlJoin1.Right);
                        if (unreferencedSingletonOnLeft != null)
                        {
                            HashSet<SqlAlias> hashSet1 = SqlGatherProducedAliases.Gather((SqlNode)sqlJoin1.Left);
                            HashSet<SqlExpression> hashSet2 = new HashSet<SqlExpression>();
                            if (SqlOuterApplyReducer.SqlPredicateLifter.CanLift(unreferencedSingletonOnLeft.Right, hashSet1, hashSet2) && SqlOuterApplyReducer.SqlSelectionLifter.CanLift(unreferencedSingletonOnLeft.Right, hashSet1, hashSet2) && !SqlOuterApplyReducer.SqlAliasDependencyChecker.IsDependent((SqlNode)unreferencedSingletonOnLeft.Right, hashSet1, hashSet2))
                            {
                                SqlExpression right = SqlOuterApplyReducer.SqlPredicateLifter.Lift(unreferencedSingletonOnLeft.Right, hashSet1);
                                List<List<SqlColumn>> selections = SqlOuterApplyReducer.SqlSelectionLifter.Lift(unreferencedSingletonOnLeft.Right, hashSet1, hashSet2);
                                this.GetSelectionsBeforeJoin(sqlJoin1.Right, selections);
                                foreach (List<SqlColumn> cols in Enumerable.Where<List<SqlColumn>>((IEnumerable<List<SqlColumn>>)selections, (Func<List<SqlColumn>, bool>)(s => s.Count > 0)))
                                    source = this.PushSourceDown(source, cols);
                                sqlJoin1.JoinType = SqlJoinType.LeftOuter;
                                sqlJoin1.Condition = this.factory.AndAccumulate(unreferencedSingletonOnLeft.Condition, right);
                                sqlJoin1.Right = unreferencedSingletonOnLeft.Right;
                            }
                            else
                            {
                                SqlJoin sqlJoin2 = sqlJoin1;
                                SqlProvider.ProviderMode[] providerModeArray = new SqlProvider.ProviderMode[1];
                                int index = 0;
                                int num = 1;
                                providerModeArray[index] = (SqlProvider.ProviderMode)num;
                                this.AnnotateSqlIncompatibility((SqlNode)sqlJoin2, providerModeArray);
                            }
                        }
                    }
                    while (sqlJoin1.JoinType == SqlJoinType.LeftOuter)
                    {
                        SqlJoin unreferencedSingletonOnLeft = this.GetLeftOuterWithUnreferencedSingletonOnLeft(sqlJoin1.Left);
                        if (unreferencedSingletonOnLeft != null)
                        {
                            List<List<SqlColumn>> selections = new List<List<SqlColumn>>();
                            this.GetSelectionsBeforeJoin(sqlJoin1.Left, selections);
                            foreach (List<SqlColumn> cols in selections)
                                source = this.PushSourceDown(source, cols);
                            SqlSource right1 = sqlJoin1.Right;
                            SqlExpression condition = sqlJoin1.Condition;
                            sqlJoin1.Left = unreferencedSingletonOnLeft.Left;
                            sqlJoin1.Right = (SqlSource)unreferencedSingletonOnLeft;
                            sqlJoin1.Condition = unreferencedSingletonOnLeft.Condition;
                            SqlJoin sqlJoin2 = unreferencedSingletonOnLeft;
                            SqlSource right2 = sqlJoin2.Right;
                            sqlJoin2.Left = right2;
                            unreferencedSingletonOnLeft.Right = right1;
                            unreferencedSingletonOnLeft.Condition = condition;
                        }
                        else
                            break;
                    }
                }
                return source;
            }

            private void AnnotateSqlIncompatibility(SqlNode node, params SqlProvider.ProviderMode[] providers)
            {
                this.annotations.Add(node, (SqlNodeAnnotation)new SqlServerCompatibilityAnnotation(Strings.SourceExpressionAnnotation((object)node.SourceExpression), providers));
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

            private SqlJoin GetLeftOuterWithUnreferencedSingletonOnLeft(SqlSource source)
            {
                SqlAlias sqlAlias = source as SqlAlias;
                if (sqlAlias != null)
                {
                    SqlSelect sqlSelect = sqlAlias.Node as SqlSelect;
                    if (sqlSelect != null && sqlSelect.Where == null && (sqlSelect.Top == null && sqlSelect.GroupBy.Count == 0) && sqlSelect.OrderBy.Count == 0)
                        return this.GetLeftOuterWithUnreferencedSingletonOnLeft(sqlSelect.From);
                }
                SqlJoin sqlJoin = source as SqlJoin;
                if (sqlJoin == null || sqlJoin.JoinType != SqlJoinType.LeftOuter)
                    return (SqlJoin)null;
                if (!this.IsSingletonSelect(sqlJoin.Left))
                    return (SqlJoin)null;
                if (SqlGatherProducedAliases.Gather((SqlNode)sqlJoin.Left).Overlaps((IEnumerable<SqlAlias>)SqlGatherConsumedAliases.Gather((SqlNode)sqlJoin.Right)))
                    return (SqlJoin)null;
                return sqlJoin;
            }

            private void GetSelectionsBeforeJoin(SqlSource source, List<List<SqlColumn>> selections)
            {
                if (source is SqlJoin)
                    return;
                SqlAlias sqlAlias = source as SqlAlias;
                if (sqlAlias == null)
                    return;
                SqlSelect sqlSelect = sqlAlias.Node as SqlSelect;
                if (sqlSelect == null)
                    return;
                this.GetSelectionsBeforeJoin(sqlSelect.From, selections);
                selections.Add(sqlSelect.Row.Columns);
            }

            private bool IsSingletonSelect(SqlSource source)
            {
                SqlAlias sqlAlias = source as SqlAlias;
                if (sqlAlias == null)
                    return false;
                SqlSelect sqlSelect = sqlAlias.Node as SqlSelect;
                return sqlSelect != null && sqlSelect.From == null;
            }
        }

        private class SqlGatherReferencedColumns
        {
            private SqlGatherReferencedColumns()
            {
            }

            internal static HashSet<SqlColumn> Gather(SqlNode node, HashSet<SqlColumn> columns)
            {
                new SqlOuterApplyReducer.SqlGatherReferencedColumns.Visitor(columns).Visit(node);
                return columns;
            }

            private class Visitor : SqlVisitor
            {
                private HashSet<SqlColumn> columns;

                internal Visitor(HashSet<SqlColumn> columns)
                {
                    this.columns = columns;
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    if (!this.columns.Contains(cref.Column))
                    {
                        this.columns.Add(cref.Column);
                        if (cref.Column.Expression != null)
                            this.Visit((SqlNode)cref.Column.Expression);
                    }
                    return (SqlExpression)cref;
                }
            }
        }

        private class SqlAliasesReferenced
        {
            private HashSet<SqlAlias> aliases;
            private bool referencesAny;
            private SqlOuterApplyReducer.SqlAliasesReferenced.Visitor visitor;

            internal SqlAliasesReferenced(HashSet<SqlAlias> aliases)
            {
                this.aliases = aliases;
                this.visitor = new SqlOuterApplyReducer.SqlAliasesReferenced.Visitor(this);
            }

            internal bool ReferencesAny(SqlExpression expression)
            {
                this.referencesAny = false;
                this.visitor.Visit((SqlNode)expression);
                return this.referencesAny;
            }

            private class Visitor : SqlVisitor
            {
                private SqlOuterApplyReducer.SqlAliasesReferenced parent;

                internal Visitor(SqlOuterApplyReducer.SqlAliasesReferenced parent)
                {
                    this.parent = parent;
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    if (this.parent.aliases.Contains(cref.Column.Alias))
                        this.parent.referencesAny = true;
                    else if (cref.Column.Expression != null)
                        this.Visit((SqlNode)cref.Column.Expression);
                    return (SqlExpression)cref;
                }

                internal override SqlExpression VisitColumn(SqlColumn col)
                {
                    if (col.Expression != null)
                        this.Visit((SqlNode)col.Expression);
                    return (SqlExpression)col;
                }
            }
        }

        private static class SqlAliasDependencyChecker
        {
            internal static bool IsDependent(SqlNode node, HashSet<SqlAlias> aliasesToCheck, HashSet<SqlExpression> ignoreExpressions)
            {
                SqlOuterApplyReducer.SqlAliasDependencyChecker.Visitor visitor = new SqlOuterApplyReducer.SqlAliasDependencyChecker.Visitor(aliasesToCheck, ignoreExpressions);
                SqlNode node1 = node;
                visitor.Visit(node1);
                return visitor.hasDependency;
            }

            private class Visitor : SqlVisitor
            {
                private HashSet<SqlAlias> aliasesToCheck;
                private HashSet<SqlExpression> ignoreExpressions;
                internal bool hasDependency;

                internal Visitor(HashSet<SqlAlias> aliasesToCheck, HashSet<SqlExpression> ignoreExpressions)
                {
                    this.aliasesToCheck = aliasesToCheck;
                    this.ignoreExpressions = ignoreExpressions;
                }

                internal override SqlNode Visit(SqlNode node)
                {
                    SqlExpression sqlExpression = node as SqlExpression;
                    if (this.hasDependency || sqlExpression != null && this.ignoreExpressions.Contains(sqlExpression))
                        return node;
                    return base.Visit(node);
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    if (this.aliasesToCheck.Contains(cref.Column.Alias))
                        this.hasDependency = true;
                    else if (cref.Column.Expression != null)
                        this.Visit((SqlNode)cref.Column.Expression);
                    return (SqlExpression)cref;
                }

                internal override SqlExpression VisitColumn(SqlColumn col)
                {
                    if (col.Expression != null)
                        this.Visit((SqlNode)col.Expression);
                    return (SqlExpression)col;
                }
            }
        }

        private static class SqlPredicateLifter
        {
            internal static bool CanLift(SqlSource source, HashSet<SqlAlias> aliasesForLifting, HashSet<SqlExpression> liftedExpressions)
            {
                SqlOuterApplyReducer.SqlPredicateLifter.Visitor visitor = new SqlOuterApplyReducer.SqlPredicateLifter.Visitor(false, aliasesForLifting, liftedExpressions);
                SqlSource source1 = source;
                visitor.VisitSource(source1);
                return visitor.canLiftAll;
            }

            internal static SqlExpression Lift(SqlSource source, HashSet<SqlAlias> aliasesForLifting)
            {
                SqlOuterApplyReducer.SqlPredicateLifter.Visitor visitor = new SqlOuterApplyReducer.SqlPredicateLifter.Visitor(true, aliasesForLifting, (HashSet<SqlExpression>)null);
                SqlSource source1 = source;
                visitor.VisitSource(source1);
                return visitor.lifted;
            }

            private class Visitor : SqlVisitor
            {
                private SqlOuterApplyReducer.SqlAliasesReferenced aliases;
                private HashSet<SqlExpression> liftedExpressions;
                private bool doLifting;
                internal bool canLiftAll;
                internal SqlExpression lifted;
                private SqlAggregateChecker aggregateChecker;

                internal Visitor(bool doLifting, HashSet<SqlAlias> aliasesForLifting, HashSet<SqlExpression> liftedExpressions)
                {
                    this.doLifting = doLifting;
                    this.aliases = new SqlOuterApplyReducer.SqlAliasesReferenced(aliasesForLifting);
                    this.liftedExpressions = liftedExpressions;
                    this.canLiftAll = true;
                    this.aggregateChecker = new SqlAggregateChecker();
                }

                internal override SqlSelect VisitSelect(SqlSelect select)
                {
                    this.VisitSource(select.From);
                    if (select.Top != null || select.GroupBy.Count > 0 || (this.aggregateChecker.HasAggregates((SqlNode)select) || select.IsDistinct))
                        this.canLiftAll = false;
                    if (this.canLiftAll && select.Where != null && this.aliases.ReferencesAny(select.Where))
                    {
                        if (this.liftedExpressions != null)
                            this.liftedExpressions.Add(select.Where);
                        if (this.doLifting)
                        {
                            this.lifted = this.lifted == null ? select.Where : (SqlExpression)new SqlBinary(SqlNodeType.And, this.lifted.ClrType, this.lifted.SqlType, this.lifted, select.Where);
                            select.Where = (SqlExpression)null;
                        }
                    }
                    return select;
                }
            }
        }

        private static class SqlSelectionLifter
        {
            internal static bool CanLift(SqlSource source, HashSet<SqlAlias> aliasesForLifting, HashSet<SqlExpression> liftedExpressions)
            {
                SqlOuterApplyReducer.SqlSelectionLifter.Visitor visitor = new SqlOuterApplyReducer.SqlSelectionLifter.Visitor(false, aliasesForLifting, liftedExpressions);
                SqlSource source1 = source;
                visitor.VisitSource(source1);
                return visitor.canLiftAll;
            }

            internal static List<List<SqlColumn>> Lift(SqlSource source, HashSet<SqlAlias> aliasesForLifting, HashSet<SqlExpression> liftedExpressions)
            {
                SqlOuterApplyReducer.SqlSelectionLifter.Visitor visitor = new SqlOuterApplyReducer.SqlSelectionLifter.Visitor(true, aliasesForLifting, liftedExpressions);
                SqlSource source1 = source;
                visitor.VisitSource(source1);
                return visitor.lifted;
            }

            private class Visitor : SqlVisitor
            {
                private SqlOuterApplyReducer.SqlAliasesReferenced aliases;
                private HashSet<SqlColumn> referencedColumns;
                private HashSet<SqlExpression> liftedExpressions;
                internal List<List<SqlColumn>> lifted;
                internal bool canLiftAll;
                private bool hasLifted;
                private bool doLifting;
                private SqlAggregateChecker aggregateChecker;

                internal Visitor(bool doLifting, HashSet<SqlAlias> aliasesForLifting, HashSet<SqlExpression> liftedExpressions)
                {
                    this.doLifting = doLifting;
                    this.aliases = new SqlOuterApplyReducer.SqlAliasesReferenced(aliasesForLifting);
                    this.referencedColumns = new HashSet<SqlColumn>();
                    this.liftedExpressions = liftedExpressions;
                    this.canLiftAll = true;
                    if (doLifting)
                        this.lifted = new List<List<SqlColumn>>();
                    this.aggregateChecker = new SqlAggregateChecker();
                }

                internal override SqlSource VisitJoin(SqlJoin join)
                {
                    this.ReferenceColumns(join.Condition);
                    return base.VisitJoin(join);
                }

                internal override SqlSelect VisitSelect(SqlSelect select)
                {
                    this.ReferenceColumns(select.Where);
                    foreach (SqlOrderExpression sqlOrderExpression in select.OrderBy)
                        this.ReferenceColumns(sqlOrderExpression.Expression);
                    foreach (SqlExpression expression in select.GroupBy)
                        this.ReferenceColumns(expression);
                    this.ReferenceColumns(select.Having);
                    List<SqlColumn> list1 = (List<SqlColumn>)null;
                    List<SqlColumn> list2 = (List<SqlColumn>)null;
                    foreach (SqlColumn sqlColumn in select.Row.Columns)
                    {
                        int num = this.aliases.ReferencesAny(sqlColumn.Expression) ? 1 : 0;
                        bool flag = this.referencedColumns.Contains(sqlColumn);
                        if (num != 0)
                        {
                            if (flag)
                            {
                                this.canLiftAll = false;
                                this.ReferenceColumns((SqlExpression)sqlColumn);
                            }
                            else
                            {
                                this.hasLifted = true;
                                if (this.doLifting)
                                {
                                    if (list1 == null)
                                        list1 = new List<SqlColumn>();
                                    list1.Add(sqlColumn);
                                }
                            }
                        }
                        else
                        {
                            if (this.doLifting)
                            {
                                if (list2 == null)
                                    list2 = new List<SqlColumn>();
                                list2.Add(sqlColumn);
                            }
                            this.ReferenceColumns((SqlExpression)sqlColumn);
                        }
                    }
                    if (this.canLiftAll)
                        this.VisitSource(select.From);
                    if ((select.Top != null || select.GroupBy.Count > 0 || (this.aggregateChecker.HasAggregates((SqlNode)select) || select.IsDistinct)) && this.hasLifted)
                        this.canLiftAll = false;
                    if (this.doLifting && this.canLiftAll)
                    {
                        select.Row.Columns.Clear();
                        if (list2 != null)
                            select.Row.Columns.AddRange((IEnumerable<SqlColumn>)list2);
                        if (list1 != null)
                            this.lifted.Add(list1);
                    }
                    return select;
                }

                private void ReferenceColumns(SqlExpression expression)
                {
                    if (expression == null || this.liftedExpressions != null && this.liftedExpressions.Contains(expression))
                        return;
                    SqlOuterApplyReducer.SqlGatherReferencedColumns.Gather((SqlNode)expression, this.referencedColumns);
                }
            }
        }
    }
}
