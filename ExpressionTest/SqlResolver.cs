using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlResolver
    {
        private SqlResolver.Visitor visitor;

        internal SqlResolver()
        {
            this.visitor = new SqlResolver.Visitor();
        }

        internal SqlNode Resolve(SqlNode node)
        {
            return this.visitor.Visit(node);
        }

        private static string GetColumnName(SqlColumn c)
        {
            return c.Name;
        }

        private class Visitor : SqlResolver.SqlScopedVisitor
        {
            private SqlResolver.SqlBubbler bubbler;

            internal Visitor()
            {
                this.bubbler = new SqlResolver.SqlBubbler();
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                SqlColumnRef sqlColumnRef = this.BubbleUp(cref);
                if (sqlColumnRef != null)
                    return (SqlExpression)sqlColumnRef;
                throw Error.ColumnReferencedIsNotInScope((object)SqlResolver.GetColumnName(cref.Column));
            }

            private SqlColumnRef BubbleUp(SqlColumnRef cref)
            {
                for (SqlResolver.SqlScopedVisitor.Scope scope = this.CurrentScope; scope != null; scope = scope.ContainingScope)
                {
                    if (scope.Source != null)
                    {
                        SqlColumn col = this.bubbler.BubbleUp(cref.Column, scope.Source);
                        if (col != null)
                        {
                            if (col != cref.Column)
                                return new SqlColumnRef(col);
                            return cref;
                        }
                    }
                }
                return (SqlColumnRef)null;
            }
        }

        internal class SqlScopedVisitor : SqlVisitor
        {
            internal SqlResolver.SqlScopedVisitor.Scope CurrentScope;

            internal SqlScopedVisitor()
            {
                this.CurrentScope = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)null, (SqlResolver.SqlScopedVisitor.Scope)null);
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                SqlResolver.SqlScopedVisitor.Scope scope = this.CurrentScope;
                this.CurrentScope = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)null, this.CurrentScope);
                base.VisitSubSelect(ss);
                this.CurrentScope = scope;
                return (SqlExpression)ss;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                select.From = (SqlSource)this.Visit((SqlNode)select.From);
                SqlResolver.SqlScopedVisitor.Scope scope = this.CurrentScope;
                this.CurrentScope = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)select.From, this.CurrentScope.ContainingScope);
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
                this.CurrentScope = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)select, this.CurrentScope.ContainingScope);
                select.Selection = this.VisitExpression(select.Selection);
                this.CurrentScope = scope;
                return select;
            }

            internal override SqlStatement VisitInsert(SqlInsert sin)
            {
                SqlResolver.SqlScopedVisitor.Scope scope = this.CurrentScope;
                this.CurrentScope = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)sin, this.CurrentScope.ContainingScope);
                base.VisitInsert(sin);
                this.CurrentScope = scope;
                return (SqlStatement)sin;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate sup)
            {
                SqlResolver.SqlScopedVisitor.Scope scope = this.CurrentScope;
                this.CurrentScope = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)sup.Select, this.CurrentScope.ContainingScope);
                base.VisitUpdate(sup);
                this.CurrentScope = scope;
                return (SqlStatement)sup;
            }

            internal override SqlStatement VisitDelete(SqlDelete sd)
            {
                SqlResolver.SqlScopedVisitor.Scope scope = this.CurrentScope;
                this.CurrentScope = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)sd, this.CurrentScope.ContainingScope);
                base.VisitDelete(sd);
                this.CurrentScope = scope;
                return (SqlStatement)sd;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                SqlResolver.SqlScopedVisitor.Scope scope = this.CurrentScope;
                switch (join.JoinType)
                {
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply:
                        this.Visit((SqlNode)join.Left);
                        SqlResolver.SqlScopedVisitor.Scope containing = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)join.Left, this.CurrentScope.ContainingScope);
                        this.CurrentScope = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)null, containing);
                        this.Visit((SqlNode)join.Right);
                        this.CurrentScope = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)null, new SqlResolver.SqlScopedVisitor.Scope((SqlNode)join.Right, containing));
                        this.Visit((SqlNode)join.Condition);
                        break;
                    default:
                        this.Visit((SqlNode)join.Left);
                        this.Visit((SqlNode)join.Right);
                        this.CurrentScope = new SqlResolver.SqlScopedVisitor.Scope((SqlNode)null, new SqlResolver.SqlScopedVisitor.Scope((SqlNode)join.Right, new SqlResolver.SqlScopedVisitor.Scope((SqlNode)join.Left, this.CurrentScope.ContainingScope)));
                        this.Visit((SqlNode)join.Condition);
                        break;
                }
                this.CurrentScope = scope;
                return (SqlSource)join;
            }

            internal class Scope
            {
                private SqlNode source;
                private SqlResolver.SqlScopedVisitor.Scope containing;

                internal SqlNode Source
                {
                    get
                    {
                        return this.source;
                    }
                }

                internal SqlResolver.SqlScopedVisitor.Scope ContainingScope
                {
                    get
                    {
                        return this.containing;
                    }
                }

                internal Scope(SqlNode source, SqlResolver.SqlScopedVisitor.Scope containing)
                {
                    this.source = source;
                    this.containing = containing;
                }
            }
        }

        internal class SqlBubbler : SqlVisitor
        {
            private SqlColumn match;
            private SqlColumn found;

            internal SqlBubbler()
            {
            }

            internal SqlColumn BubbleUp(SqlColumn col, SqlNode source)
            {
                this.match = this.GetOriginatingColumn(col);
                this.found = (SqlColumn)null;
                this.Visit(source);
                return this.found;
            }

            internal SqlColumn GetOriginatingColumn(SqlColumn col)
            {
                SqlColumnRef sqlColumnRef = col.Expression as SqlColumnRef;
                if (sqlColumnRef != null)
                    return this.GetOriginatingColumn(sqlColumnRef.Column);
                return col;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                foreach (SqlColumn sqlColumn in row.Columns)
                {
                    if (this.RefersToColumn((SqlExpression)sqlColumn, this.match))
                    {
                        if (this.found != null)
                            throw Error.ColumnIsDefinedInMultiplePlaces((object)SqlResolver.GetColumnName(this.match));
                        this.found = sqlColumn;
                        break;
                    }
                }
                return row;
            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                foreach (SqlColumn sqlColumn in tab.Columns)
                {
                    if (sqlColumn == this.match)
                    {
                        if (this.found != null)
                            throw Error.ColumnIsDefinedInMultiplePlaces((object)SqlResolver.GetColumnName(this.match));
                        this.found = sqlColumn;
                        break;
                    }
                }
                return tab;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                switch (join.JoinType)
                {
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply:
                        this.Visit((SqlNode)join.Left);
                        if (this.found == null)
                        {
                            this.Visit((SqlNode)join.Right);
                            break;
                        }
                        break;
                    default:
                        this.Visit((SqlNode)join.Left);
                        this.Visit((SqlNode)join.Right);
                        break;
                }
                return (SqlSource)join;
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
            {
                foreach (SqlColumn sqlColumn in fc.Columns)
                {
                    if (sqlColumn == this.match)
                    {
                        if (this.found != null)
                            throw Error.ColumnIsDefinedInMultiplePlaces((object)SqlResolver.GetColumnName(this.match));
                        this.found = sqlColumn;
                        break;
                    }
                }
                return (SqlExpression)fc;
            }

            private void ForceLocal(SqlRow row, string name)
            {
                bool flag = false;
                foreach (SqlColumn sqlColumn in row.Columns)
                {
                    if (this.RefersToColumn((SqlExpression)sqlColumn, this.found))
                    {
                        this.found = sqlColumn;
                        flag = true;
                        break;
                    }
                }
                if (flag)
                    return;
                SqlColumn sqlColumn1 = new SqlColumn(this.found.ClrType, this.found.SqlType, name, this.found.MetaMember, (SqlExpression)new SqlColumnRef(this.found), row.SourceExpression);
                row.Columns.Add(sqlColumn1);
                this.found = sqlColumn1;
            }

            private bool IsFoundInGroup(SqlSelect select)
            {
                foreach (SqlExpression exp in select.GroupBy)
                {
                    if (this.RefersToColumn(exp, this.found) || this.RefersToColumn(exp, this.match))
                        return true;
                }
                return false;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                this.Visit((SqlNode)select.Row);
                if (this.found == null)
                {
                    this.Visit((SqlNode)select.From);
                    if (this.found != null)
                    {
                        if (select.IsDistinct && !this.match.IsConstantColumn)
                            throw Error.ColumnIsNotAccessibleThroughDistinct((object)SqlResolver.GetColumnName(this.match));
                        if (select.GroupBy.Count != 0 && !this.IsFoundInGroup(select))
                            throw Error.ColumnIsNotAccessibleThroughGroupBy((object)SqlResolver.GetColumnName(this.match));
                        this.ForceLocal(select.Row, this.found.Name);
                    }
                }
                return select;
            }
        }
    }
}
