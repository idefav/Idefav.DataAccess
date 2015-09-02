using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlFlattener
    {
        private SqlFlattener.Visitor visitor;

        internal SqlFlattener(SqlFactory sql, SqlColumnizer columnizer)
        {
            this.visitor = new SqlFlattener.Visitor(sql, columnizer);
        }

        internal SqlNode Flatten(SqlNode node)
        {
            node = this.visitor.Visit(node);
            return node;
        }

        private class Visitor : SqlVisitor
        {
            private Dictionary<SqlColumn, SqlColumn> map = new Dictionary<SqlColumn, SqlColumn>();
            private SqlFactory sql;
            private SqlColumnizer columnizer;
            private bool isTopLevel;

            internal Visitor(SqlFactory sql, SqlColumnizer columnizer)
            {
                this.sql = sql;
                this.columnizer = columnizer;
                this.isTopLevel = true;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                SqlColumn col;
                if (this.map.TryGetValue(cref.Column, out col))
                    return (SqlExpression)new SqlColumnRef(col);
                return (SqlExpression)cref;
            }

            internal override SqlSelect VisitSelectCore(SqlSelect select)
            {
                bool flag = this.isTopLevel;
                this.isTopLevel = false;
                try
                {
                    return base.VisitSelectCore(select);
                }
                finally
                {
                    this.isTopLevel = flag;
                }
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                select = base.VisitSelect(select);
                select.Selection = this.FlattenSelection(select.Row, false, select.Selection);
                if (select.GroupBy.Count > 0)
                    this.FlattenGroupBy(select.GroupBy);
                if (select.OrderBy.Count > 0)
                    this.FlattenOrderBy(select.OrderBy);
                if (!this.isTopLevel)
                {
                    SqlSelect sqlSelect = select;
                    SqlNop sqlNop = new SqlNop(sqlSelect.Selection.ClrType, select.Selection.SqlType, select.SourceExpression);
                    sqlSelect.Selection = (SqlExpression)sqlNop;
                }
                return select;
            }

            internal override SqlStatement VisitInsert(SqlInsert sin)
            {
                base.VisitInsert(sin);
                sin.Expression = this.FlattenSelection(sin.Row, true, sin.Expression);
                return (SqlStatement)sin;
            }

            private SqlExpression FlattenSelection(SqlRow row, bool isInput, SqlExpression selection)
            {
                selection = this.columnizer.ColumnizeSelection(selection);
                return new SqlFlattener.Visitor.SelectionFlattener(row, this.map, isInput).VisitExpression(selection);
            }

            private void FlattenGroupBy(List<SqlExpression> exprs)
            {
                List<SqlExpression> exprs1 = new List<SqlExpression>(exprs.Count);
                foreach (SqlExpression expr in exprs)
                {
                    if (TypeSystem.IsSequenceType(expr.ClrType))
                        throw Error.InvalidGroupByExpressionType((object)expr.ClrType.Name);
                    this.FlattenGroupByExpression(exprs1, expr);
                }
                exprs.Clear();
                exprs.AddRange((IEnumerable<SqlExpression>)exprs1);
            }

            private void FlattenGroupByExpression(List<SqlExpression> exprs, SqlExpression expr)
            {
                SqlNew sqlNew = expr as SqlNew;
                if (sqlNew != null)
                {
                    foreach (SqlMemberAssign sqlMemberAssign in sqlNew.Members)
                        this.FlattenGroupByExpression(exprs, sqlMemberAssign.Expression);
                    foreach (SqlExpression expr1 in sqlNew.Args)
                        this.FlattenGroupByExpression(exprs, expr1);
                }
                else if (expr.NodeType == SqlNodeType.TypeCase)
                {
                    SqlTypeCase sqlTypeCase = (SqlTypeCase)expr;
                    this.FlattenGroupByExpression(exprs, sqlTypeCase.Discriminator);
                    foreach (SqlTypeCaseWhen sqlTypeCaseWhen in sqlTypeCase.Whens)
                        this.FlattenGroupByExpression(exprs, sqlTypeCaseWhen.TypeBinding);
                }
                else if (expr.NodeType == SqlNodeType.Link)
                {
                    SqlLink sqlLink = (SqlLink)expr;
                    if (sqlLink.Expansion != null)
                    {
                        this.FlattenGroupByExpression(exprs, sqlLink.Expansion);
                    }
                    else
                    {
                        foreach (SqlExpression expr1 in sqlLink.KeyExpressions)
                            this.FlattenGroupByExpression(exprs, expr1);
                    }
                }
                else if (expr.NodeType == SqlNodeType.OptionalValue)
                {
                    SqlOptionalValue sqlOptionalValue = (SqlOptionalValue)expr;
                    this.FlattenGroupByExpression(exprs, sqlOptionalValue.HasValue);
                    this.FlattenGroupByExpression(exprs, sqlOptionalValue.Value);
                }
                else if (expr.NodeType == SqlNodeType.OuterJoinedValue)
                    this.FlattenGroupByExpression(exprs, ((SqlUnary)expr).Operand);
                else if (expr.NodeType == SqlNodeType.DiscriminatedType)
                {
                    SqlDiscriminatedType discriminatedType = (SqlDiscriminatedType)expr;
                    this.FlattenGroupByExpression(exprs, discriminatedType.Discriminator);
                }
                else
                {
                    if (expr.NodeType != SqlNodeType.ColumnRef && expr.NodeType != SqlNodeType.ExprSet)
                    {
                        if (!expr.SqlType.CanBeColumn)
                            throw Error.InvalidGroupByExpressionType((object)expr.SqlType.ToQueryString());
                        throw Error.InvalidGroupByExpression();
                    }
                    exprs.Add(expr);
                }
            }

            private void FlattenOrderBy(List<SqlOrderExpression> exprs)
            {
                foreach (SqlOrderExpression sqlOrderExpression in exprs)
                {
                    if (!sqlOrderExpression.Expression.SqlType.IsOrderable)
                    {
                        if (sqlOrderExpression.Expression.SqlType.CanBeColumn)
                            throw Error.InvalidOrderByExpression((object)sqlOrderExpression.Expression.SqlType.ToQueryString());
                        throw Error.InvalidOrderByExpression((object)sqlOrderExpression.Expression.ClrType.Name);
                    }
                }
            }

            private class SelectionFlattener : SqlVisitor
            {
                private SqlRow row;
                private Dictionary<SqlColumn, SqlColumn> map;
                private bool isInput;
                private bool isNew;

                internal SelectionFlattener(SqlRow row, Dictionary<SqlColumn, SqlColumn> map, bool isInput)
                {
                    this.row = row;
                    this.map = map;
                    this.isInput = isInput;
                }

                internal override SqlExpression VisitNew(SqlNew sox)
                {
                    this.isNew = true;
                    return base.VisitNew(sox);
                }

                internal override SqlExpression VisitColumn(SqlColumn col)
                {
                    SqlColumn col1 = this.FindColumn((IEnumerable<SqlColumn>)this.row.Columns, col);
                    if (col1 == null && col.Expression != null && !this.isInput && (!this.isNew || this.isNew && !col.Expression.IsConstantColumn))
                        col1 = this.FindColumnWithExpression((IEnumerable<SqlColumn>)this.row.Columns, col.Expression);
                    if (col1 == null)
                    {
                        this.row.Columns.Add(col);
                        col1 = col;
                    }
                    else if (col1 != col)
                    {
                        if (col.Expression.NodeType == SqlNodeType.ExprSet && col1.Expression.NodeType != SqlNodeType.ExprSet)
                            col1.Expression = col.Expression;
                        this.map[col] = col1;
                    }
                    return (SqlExpression)new SqlColumnRef(col1);
                }

                internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
                {
                    SqlColumn column = this.FindColumn((IEnumerable<SqlColumn>)this.row.Columns, cref.Column);
                    if (column == null)
                        return (SqlExpression)this.MakeFlattenedColumn((SqlExpression)cref, (string)null);
                    return (SqlExpression)new SqlColumnRef(column);
                }

                internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
                {
                    return (SqlExpression)ss;
                }

                internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
                {
                    return (SqlExpression)cq;
                }

                private SqlColumnRef MakeFlattenedColumn(SqlExpression expr, string name)
                {
                    SqlColumn col = !this.isInput ? this.FindColumnWithExpression((IEnumerable<SqlColumn>)this.row.Columns, expr) : (SqlColumn)null;
                    if (col == null)
                    {
                        Type clrType = expr.ClrType;
                        ProviderType sqlType = expr.SqlType;
                        string name1 = name;
                        // ISSUE: variable of the null type
                        //__Null local = null;
                        SqlExpression expr1 = expr;
                        Expression sourceExpression = expr1.SourceExpression;
                        col = new SqlColumn(clrType, sqlType, name1, (MetaDataMember)null, expr1, sourceExpression);
                        this.row.Columns.Add(col);
                    }
                    return new SqlColumnRef(col);
                }

                private SqlColumn FindColumn(IEnumerable<SqlColumn> columns, SqlColumn col)
                {
                    foreach (SqlColumn sqlColumn in columns)
                    {
                        if (this.RefersToColumn((SqlExpression)sqlColumn, col))
                            return sqlColumn;
                    }
                    return (SqlColumn)null;
                }

                private SqlColumn FindColumnWithExpression(IEnumerable<SqlColumn> columns, SqlExpression expr)
                {
                    foreach (SqlColumn sqlColumn in columns)
                    {
                        if (sqlColumn == expr || SqlComparer.AreEqual((SqlNode)sqlColumn.Expression, (SqlNode)expr))
                            return sqlColumn;
                    }
                    return (SqlColumn)null;
                }
            }
        }
    }
}
