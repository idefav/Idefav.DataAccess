using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlDeflator
    {
        private SqlDeflator.SqlValueDeflator vDeflator;
        private SqlDeflator.SqlColumnDeflator cDeflator;
        private SqlDeflator.SqlAliasDeflator aDeflator;
        private SqlDeflator.SqlTopSelectDeflator tsDeflator;
        private SqlDeflator.SqlDuplicateColumnDeflator dupColumnDeflator;

        internal SqlDeflator()
        {
            this.vDeflator = new SqlDeflator.SqlValueDeflator();
            this.cDeflator = new SqlDeflator.SqlColumnDeflator();
            this.aDeflator = new SqlDeflator.SqlAliasDeflator();
            this.tsDeflator = new SqlDeflator.SqlTopSelectDeflator();
            this.dupColumnDeflator = new SqlDeflator.SqlDuplicateColumnDeflator();
        }

        internal SqlNode Deflate(SqlNode node)
        {
            node = this.vDeflator.Visit(node);
            node = this.cDeflator.Visit(node);
            node = this.aDeflator.Visit(node);
            node = this.tsDeflator.Visit(node);
            node = this.dupColumnDeflator.Visit(node);
            return node;
        }

        private class SqlValueDeflator : SqlVisitor
        {
            private bool isTopLevel = true;
            private SqlDeflator.SqlValueDeflator.SelectionDeflator sDeflator;

            internal SqlValueDeflator()
            {
                this.sDeflator = new SqlDeflator.SqlValueDeflator.SelectionDeflator();
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                if (this.isTopLevel)
                    select.Selection = this.sDeflator.VisitExpression(select.Selection);
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                bool flag = this.isTopLevel;
                try
                {
                    return base.VisitSubSelect(ss);
                }
                finally
                {
                    this.isTopLevel = flag;
                }
            }

            private class SelectionDeflator : SqlVisitor
            {
                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    return (SqlExpression)this.GetLiteralValue((SqlExpression)cref) ?? (SqlExpression)cref;
                }

                private SqlValue GetLiteralValue(SqlExpression expr)
                {
                    while (expr != null && expr.NodeType == SqlNodeType.ColumnRef)
                        expr = ((SqlColumnRef)expr).Column.Expression;
                    return expr as SqlValue;
                }
            }
        }

        private class SqlColumnDeflator : SqlVisitor
        {
            private Dictionary<SqlNode, SqlNode> referenceMap;
            private bool isTopLevel;
            private bool forceReferenceAll;
            private SqlAggregateChecker aggregateChecker;

            internal SqlColumnDeflator()
            {
                this.referenceMap = new Dictionary<SqlNode, SqlNode>();
                this.aggregateChecker = new SqlAggregateChecker();
                this.isTopLevel = true;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                this.referenceMap[(SqlNode)cref.Column] = (SqlNode)cref.Column;
                return (SqlExpression)cref;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                bool flag1 = this.isTopLevel;
                this.isTopLevel = false;
                bool flag2 = this.forceReferenceAll;
                this.forceReferenceAll = true;
                try
                {
                    return base.VisitScalarSubSelect(ss);
                }
                finally
                {
                    this.isTopLevel = flag1;
                    this.forceReferenceAll = flag2;
                }
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss)
            {
                bool flag = this.isTopLevel;
                this.isTopLevel = false;
                try
                {
                    return base.VisitExists(ss);
                }
                finally
                {
                    this.isTopLevel = flag;
                }
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                bool flag = this.forceReferenceAll;
                this.forceReferenceAll = true;
                su.Left = this.Visit(su.Left);
                su.Right = this.Visit(su.Right);
                this.forceReferenceAll = flag;
                return (SqlNode)su;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                bool flag1 = this.forceReferenceAll;
                this.forceReferenceAll = false;
                bool flag2 = this.isTopLevel;
                try
                {
                    if (this.isTopLevel)
                        select.Selection = this.VisitExpression(select.Selection);
                    this.isTopLevel = false;
                    for (int index = select.Row.Columns.Count - 1; index >= 0; --index)
                    {
                        SqlColumn sqlColumn = select.Row.Columns[index];
                        if ((flag1 || this.referenceMap.ContainsKey((SqlNode)sqlColumn) || select.IsDistinct ? 0 : (select.GroupBy.Count != 0 ? 1 : (!this.aggregateChecker.HasAggregates((SqlNode)sqlColumn.Expression) ? 1 : 0))) != 0)
                            select.Row.Columns.RemoveAt(index);
                        else
                            this.VisitExpression(sqlColumn.Expression);
                    }
                    select.Top = this.VisitExpression(select.Top);
                    for (int index = select.OrderBy.Count - 1; index >= 0; --index)
                        select.OrderBy[index].Expression = this.VisitExpression(select.OrderBy[index].Expression);
                    select.Having = this.VisitExpression(select.Having);
                    for (int index = select.GroupBy.Count - 1; index >= 0; --index)
                        select.GroupBy[index] = this.VisitExpression(select.GroupBy[index]);
                    select.Where = this.VisitExpression(select.Where);
                    select.From = this.VisitSource(select.From);
                }
                finally
                {
                    this.isTopLevel = flag2;
                    this.forceReferenceAll = flag1;
                }
                return select;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                join.Condition = this.VisitExpression(join.Condition);
                join.Right = this.VisitSource(join.Right);
                join.Left = this.VisitSource(join.Left);
                return (SqlSource)join;
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                int index = 0;
                for (int count = link.KeyExpressions.Count; index < count; ++index)
                    link.KeyExpressions[index] = this.VisitExpression(link.KeyExpressions[index]);
                return (SqlNode)link;
            }
        }

        private class SqlColumnEqualizer : SqlVisitor
        {
            private Dictionary<SqlColumn, SqlColumn> map;

            internal SqlColumnEqualizer()
            {
            }

            internal void BuildEqivalenceMap(SqlSource scope)
            {
                this.map = new Dictionary<SqlColumn, SqlColumn>();
                this.Visit((SqlNode)scope);
            }

            internal bool AreEquivalent(SqlExpression e1, SqlExpression e2)
            {
                if (SqlComparer.AreEqual((SqlNode)e1, (SqlNode)e2))
                    return true;
                SqlColumnRef sqlColumnRef1 = e1 as SqlColumnRef;
                SqlColumnRef sqlColumnRef2 = e2 as SqlColumnRef;
                if (sqlColumnRef1 == null || sqlColumnRef2 == null)
                    return false;
                SqlColumn rootColumn1 = sqlColumnRef1.GetRootColumn();
                SqlColumn rootColumn2 = sqlColumnRef2.GetRootColumn();
                SqlColumn sqlColumn;
                if (this.map.TryGetValue(rootColumn1, out sqlColumn))
                    return sqlColumn == rootColumn2;
                return false;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                base.VisitJoin(join);
                if (join.Condition != null)
                    this.CheckJoinCondition(join.Condition);
                return (SqlSource)join;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                base.VisitSelect(select);
                if (select.Where != null)
                    this.CheckJoinCondition(select.Where);
                return select;
            }

            private void CheckJoinCondition(SqlExpression expr)
            {
                switch (expr.NodeType)
                {
                    case SqlNodeType.And:
                        SqlBinary sqlBinary1 = (SqlBinary)expr;
                        this.CheckJoinCondition(sqlBinary1.Left);
                        this.CheckJoinCondition(sqlBinary1.Right);
                        break;
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        SqlBinary sqlBinary2 = (SqlBinary)expr;
                        SqlColumnRef sqlColumnRef1 = sqlBinary2.Left as SqlColumnRef;
                        SqlColumnRef sqlColumnRef2 = sqlBinary2.Right as SqlColumnRef;
                        if (sqlColumnRef1 == null || sqlColumnRef2 == null)
                            break;
                        SqlColumn rootColumn1 = sqlColumnRef1.GetRootColumn();
                        SqlColumn rootColumn2 = sqlColumnRef2.GetRootColumn();
                        this.map[rootColumn1] = rootColumn2;
                        this.map[rootColumn2] = rootColumn1;
                        break;
                }
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return (SqlExpression)ss;
            }
        }

        private class SqlAliasDeflator : SqlVisitor
        {
            private Dictionary<SqlAlias, SqlAlias> removedMap;

            internal SqlAliasDeflator()
            {
                this.removedMap = new Dictionary<SqlAlias, SqlAlias>();
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                SqlAlias sqlAlias;
                if (this.removedMap.TryGetValue(aref.Alias, out sqlAlias))
                    throw Error.InvalidReferenceToRemovedAliasDuringDeflation();
                return (SqlExpression)aref;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                if (cref.Column.Alias == null || !this.removedMap.ContainsKey(cref.Column.Alias))
                    return (SqlExpression)cref;
                SqlColumnRef cref1 = cref.Column.Expression as SqlColumnRef;
                if (cref1 == null || !(cref1.ClrType != cref.ClrType))
                    return (SqlExpression)cref1;
                cref1.SetClrType(cref.ClrType);
                return this.VisitColumnRef(cref1);
            }

            internal override SqlSource VisitSource(SqlSource node)
            {
                node = (SqlSource)this.Visit((SqlNode)node);
                SqlAlias sqlAlias = node as SqlAlias;
                if (sqlAlias != null)
                {
                    SqlSelect select = sqlAlias.Node as SqlSelect;
                    if (select != null && this.IsTrivialSelect(select))
                    {
                        SqlAlias index = null;
                        this.removedMap[index] = index = sqlAlias;
                        node = select.From;
                    }
                }
                return node;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                base.VisitJoin(join);
                switch (join.JoinType)
                {
                    case SqlJoinType.LeftOuter:
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply:
                        if (this.HasEmptySource(join.Right))
                        {
                            SqlAlias sqlAlias = (SqlAlias)join.Right;
                            SqlAlias index = null;
                            this.removedMap[index] = index = sqlAlias;
                            return join.Left;
                        }
                        break;
                }
                return (SqlSource)join;
            }

            private bool IsTrivialSelect(SqlSelect select)
            {
                if (select.OrderBy.Count != 0 || select.GroupBy.Count != 0 || (select.Having != null || select.Top != null) || (select.IsDistinct || select.Where != null || !this.HasTrivialSource(select.From)))
                    return false;
                return this.HasTrivialProjection(select);
            }

            private bool HasTrivialSource(SqlSource node)
            {
                SqlJoin sqlJoin = node as SqlJoin;
                if (sqlJoin == null)
                    return node is SqlAlias;
                if (this.HasTrivialSource(sqlJoin.Left))
                    return this.HasTrivialSource(sqlJoin.Right);
                return false;
            }

            private bool HasTrivialProjection(SqlSelect select)
            {
                foreach (SqlColumn sqlColumn in select.Row.Columns)
                {
                    if (sqlColumn.Expression != null && sqlColumn.Expression.NodeType != SqlNodeType.ColumnRef)
                        return false;
                }
                return true;
            }

            private bool HasEmptySource(SqlSource node)
            {
                SqlAlias sqlAlias = node as SqlAlias;
                if (sqlAlias == null)
                    return false;
                SqlSelect sqlSelect = sqlAlias.Node as SqlSelect;
                if (sqlSelect == null || sqlSelect.Row.Columns.Count != 0 || (sqlSelect.From != null || sqlSelect.Where != null) || (sqlSelect.GroupBy.Count != 0 || sqlSelect.Having != null))
                    return false;
                return sqlSelect.OrderBy.Count == 0;
            }
        }

        private class SqlDuplicateColumnDeflator : SqlVisitor
        {
            private SqlDeflator.SqlColumnEqualizer equalizer = new SqlDeflator.SqlColumnEqualizer();

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                select.From = this.VisitSource(select.From);
                select.Where = this.VisitExpression(select.Where);
                int index1 = 0;
                for (int count = select.GroupBy.Count; index1 < count; ++index1)
                    select.GroupBy[index1] = this.VisitExpression(select.GroupBy[index1]);
                for (int index2 = select.GroupBy.Count - 1; index2 >= 0; --index2)
                {
                    for (int index3 = index2 - 1; index3 >= 0; --index3)
                    {
                        if (SqlComparer.AreEqual((SqlNode)select.GroupBy[index2], (SqlNode)select.GroupBy[index3]))
                        {
                            select.GroupBy.RemoveAt(index2);
                            break;
                        }
                    }
                }
                select.Having = this.VisitExpression(select.Having);
                int index4 = 0;
                for (int count = select.OrderBy.Count; index4 < count; ++index4)
                    select.OrderBy[index4].Expression = this.VisitExpression(select.OrderBy[index4].Expression);
                if (select.OrderBy.Count > 0)
                {
                    this.equalizer.BuildEqivalenceMap(select.From);
                    for (int index2 = select.OrderBy.Count - 1; index2 >= 0; --index2)
                    {
                        for (int index3 = index2 - 1; index3 >= 0; --index3)
                        {
                            if (this.equalizer.AreEquivalent(select.OrderBy[index2].Expression, select.OrderBy[index3].Expression))
                            {
                                select.OrderBy.RemoveAt(index2);
                                break;
                            }
                        }
                    }
                }
                select.Top = this.VisitExpression(select.Top);
                select.Row = (SqlRow)this.Visit((SqlNode)select.Row);
                select.Selection = this.VisitExpression(select.Selection);
                return select;
            }
        }

        private class SqlTopSelectDeflator : SqlVisitor
        {
            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                if (!this.IsTrivialSelect(select))
                    return select;
                SqlSelect sqlSelect = (SqlSelect)((SqlAlias)select.From).Node;
                Dictionary<SqlColumn, SqlColumnRef> map = new Dictionary<SqlColumn, SqlColumnRef>();
                foreach (SqlColumn key in select.Row.Columns)
                {
                    SqlColumnRef sqlColumnRef = (SqlColumnRef)key.Expression;
                    map.Add(key, sqlColumnRef);
                    if (!string.IsNullOrEmpty(key.Name))
                        sqlColumnRef.Column.Name = key.Name;
                }
                sqlSelect.Selection = new SqlDeflator.SqlTopSelectDeflator.ColumnMapper(map).VisitExpression(select.Selection);
                return sqlSelect;
            }

            private bool IsTrivialSelect(SqlSelect select)
            {
                if (select.OrderBy.Count != 0 || select.GroupBy.Count != 0 || (select.Having != null || select.Top != null) || (select.IsDistinct || select.Where != null || !this.HasTrivialSource(select.From)))
                    return false;
                return this.HasTrivialProjection(select);
            }

            private bool HasTrivialSource(SqlSource node)
            {
                SqlAlias sqlAlias = node as SqlAlias;
                if (sqlAlias == null)
                    return false;
                return sqlAlias.Node is SqlSelect;
            }

            private bool HasTrivialProjection(SqlSelect select)
            {
                foreach (SqlColumn sqlColumn in select.Row.Columns)
                {
                    if (sqlColumn.Expression != null && sqlColumn.Expression.NodeType != SqlNodeType.ColumnRef)
                        return false;
                }
                return true;
            }

            private class ColumnMapper : SqlVisitor
            {
                private Dictionary<SqlColumn, SqlColumnRef> map;

                internal ColumnMapper(Dictionary<SqlColumn, SqlColumnRef> map)
                {
                    this.map = map;
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    SqlColumnRef sqlColumnRef;
                    if (this.map.TryGetValue(cref.Column, out sqlColumnRef))
                        return (SqlExpression)sqlColumnRef;
                    return (SqlExpression)cref;
                }
            }
        }
    }

}
