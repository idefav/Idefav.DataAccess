using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlReorderer
    {
        private TypeSystemProvider typeProvider;
        private SqlFactory sql;

        internal SqlReorderer(TypeSystemProvider typeProvider, SqlFactory sqlFactory)
        {
            this.typeProvider = typeProvider;
            this.sql = sqlFactory;
        }

        internal SqlNode Reorder(SqlNode node)
        {
            return new SqlReorderer.Visitor(this.typeProvider, this.sql).Visit(node);
        }

        private class Visitor : SqlVisitor
        {
            private bool topSelect = true;
            private TypeSystemProvider typeProvider;
            private bool addPrimaryKeys;
            private List<SqlOrderExpression> orders;
            private List<SqlOrderExpression> rowNumberOrders;
            private SqlSelect currentSelect;
            private SqlFactory sql;
            private SqlAggregateChecker aggregateChecker;

            private List<SqlOrderExpression> Orders
            {
                get
                {
                    if (this.orders == null)
                        this.orders = new List<SqlOrderExpression>();
                    return this.orders;
                }
            }

            internal Visitor(TypeSystemProvider typeProvider, SqlFactory sqlFactory)
            {
                this.orders = new List<SqlOrderExpression>();
                this.typeProvider = typeProvider;
                this.sql = sqlFactory;
                this.aggregateChecker = new SqlAggregateChecker();
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                List<SqlOrderExpression> list = this.orders;
                this.orders = new List<SqlOrderExpression>();
                base.VisitSubSelect(ss);
                this.orders = list;
                return (SqlExpression)ss;
            }

            private void PrependOrderExpressions(IEnumerable<SqlOrderExpression> exprs)
            {
                if (exprs == null)
                    return;
                this.Orders.InsertRange(0, exprs);
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                this.Visit((SqlNode)join.Left);
                List<SqlOrderExpression> list = this.orders;
                this.orders = (List<SqlOrderExpression>)null;
                this.Visit((SqlNode)join.Right);
                this.PrependOrderExpressions((IEnumerable<SqlOrderExpression>)list);
                return (SqlSource)join;
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                this.orders = (List<SqlOrderExpression>)null;
                su.Left = this.Visit(su.Left);
                this.orders = (List<SqlOrderExpression>)null;
                su.Right = this.Visit(su.Right);
                this.orders = (List<SqlOrderExpression>)null;
                return (SqlNode)su;
            }

            internal override SqlAlias VisitAlias(SqlAlias a)
            {
                SqlTable sqlTable = a.Node as SqlTable;
                SqlTableValuedFunctionCall valuedFunctionCall = a.Node as SqlTableValuedFunctionCall;
                if (!this.addPrimaryKeys || sqlTable == null && valuedFunctionCall == null)
                    return base.VisitAlias(a);
                List<SqlOrderExpression> list = new List<SqlOrderExpression>();
                bool flag = sqlTable != null;
                foreach (MetaDataMember member in (flag ? sqlTable.RowType : valuedFunctionCall.RowType).IdentityMembers)
                {
                    string mappedName = member.MappedName;
                    SqlColumn col;
                    Expression sourceExpression;
                    List<SqlColumn> columns;
                    if (flag)
                    {
                        col = sqlTable.Find(mappedName);
                        sourceExpression = sqlTable.SourceExpression;
                        columns = sqlTable.Columns;
                    }
                    else
                    {
                        col = valuedFunctionCall.Find(mappedName);
                        sourceExpression = valuedFunctionCall.SourceExpression;
                        columns = valuedFunctionCall.Columns;
                    }
                    if (col == null)
                    {
                        col = new SqlColumn(member.MemberAccessor.Type, this.typeProvider.From(member.MemberAccessor.Type), mappedName, member, (SqlExpression)null, sourceExpression);
                        col.Alias = a;
                        columns.Add(col);
                    }
                    list.Add(new SqlOrderExpression(SqlOrderType.Ascending, (SqlExpression)new SqlColumnRef(col)));
                }
                this.PrependOrderExpressions((IEnumerable<SqlOrderExpression>)list);
                return a;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                bool flag1 = this.topSelect;
                bool flag2 = this.addPrimaryKeys;
                SqlSelect sqlSelect = this.currentSelect;
                this.currentSelect = select;
                if (select.OrderingType == SqlOrderingType.Always)
                    this.addPrimaryKeys = true;
                this.topSelect = false;
                if (select.GroupBy.Count > 0)
                {
                    this.Visit((SqlNode)select.From);
                    this.orders = (List<SqlOrderExpression>)null;
                }
                else
                    this.Visit((SqlNode)select.From);
                if (select.OrderBy.Count > 0)
                    this.PrependOrderExpressions((IEnumerable<SqlOrderExpression>)select.OrderBy);
                List<SqlOrderExpression> list = this.orders;
                this.orders = (List<SqlOrderExpression>)null;
                this.rowNumberOrders = list;
                select.Where = this.VisitExpression(select.Where);
                int index1 = 0;
                for (int count = select.GroupBy.Count; index1 < count; ++index1)
                    select.GroupBy[index1] = this.VisitExpression(select.GroupBy[index1]);
                select.Having = this.VisitExpression(select.Having);
                int index2 = 0;
                for (int count = select.OrderBy.Count; index2 < count; ++index2)
                    select.OrderBy[index2].Expression = this.VisitExpression(select.OrderBy[index2].Expression);
                select.Top = this.VisitExpression(select.Top);
                select.Selection = this.VisitExpression(select.Selection);
                select.Row = (SqlRow)this.Visit((SqlNode)select.Row);
                this.topSelect = flag1;
                this.addPrimaryKeys = flag2;
                this.orders = list;
                if (select.OrderingType == SqlOrderingType.Blocked)
                    this.orders = (List<SqlOrderExpression>)null;
                select.OrderBy.Clear();
                SqlRowNumberChecker rowNumberChecker = new SqlRowNumberChecker();
                if (rowNumberChecker.HasRowNumber((SqlNode)select) && rowNumberChecker.RowNumberColumn != null)
                {
                    select.Row.Columns.Remove(rowNumberChecker.RowNumberColumn);
                    this.PushDown(rowNumberChecker.RowNumberColumn);
                    this.Orders.Add(new SqlOrderExpression(SqlOrderType.Ascending, (SqlExpression)new SqlColumnRef(rowNumberChecker.RowNumberColumn)));
                }
                if ((this.topSelect || select.Top != null) && (select.OrderingType != SqlOrderingType.Never && this.orders != null))
                {
                    this.orders = Enumerable.ToList<SqlOrderExpression>((IEnumerable<SqlOrderExpression>)new HashSet<SqlOrderExpression>((IEnumerable<SqlOrderExpression>)this.orders));
                    SqlDuplicator sqlDuplicator = new SqlDuplicator(true);
                    foreach (SqlOrderExpression sqlOrderExpression in this.orders)
                        select.OrderBy.Add(new SqlOrderExpression(sqlOrderExpression.OrderType, (SqlExpression)sqlDuplicator.Duplicate((SqlNode)sqlOrderExpression.Expression)));
                }
                this.currentSelect = sqlSelect;
                return select;
            }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
            {
                if (rowNumber.OrderBy.Count > 0)
                    return rowNumber;
                SqlDuplicator sqlDuplicator = new SqlDuplicator(true);
                List<SqlOrderExpression> list1 = new List<SqlOrderExpression>();
                List<SqlOrderExpression> list2 = new List<SqlOrderExpression>();
                if (this.rowNumberOrders != null && this.rowNumberOrders.Count != 0)
                    list2 = new List<SqlOrderExpression>((IEnumerable<SqlOrderExpression>)this.rowNumberOrders);
                else if (this.orders != null)
                    list2 = new List<SqlOrderExpression>((IEnumerable<SqlOrderExpression>)this.orders);
                foreach (SqlOrderExpression sqlOrderExpression in list2)
                {
                    if (!sqlOrderExpression.Expression.IsConstantColumn)
                    {
                        list1.Add(sqlOrderExpression);
                        if (this.rowNumberOrders != null)
                            this.rowNumberOrders.Remove(sqlOrderExpression);
                        if (this.orders != null)
                            this.orders.Remove(sqlOrderExpression);
                    }
                }
                rowNumber.OrderBy.Clear();
                if (list1.Count == 0)
                {
                    foreach (SqlColumn col in SqlReorderer.SqlGatherColumnsProduced.GatherColumns(this.currentSelect.From))
                    {
                        if (col.Expression.SqlType.IsOrderable)
                            list1.Add(new SqlOrderExpression(SqlOrderType.Ascending, (SqlExpression)new SqlColumnRef(col)));
                    }
                    if (list1.Count == 0)
                    {
                        SqlColumn sqlColumn = new SqlColumn("rowNumberOrder", this.sql.Value(typeof(int), this.typeProvider.From(typeof(int)), (object)1, false, rowNumber.SourceExpression));
                        this.PushDown(sqlColumn);
                        list1.Add(new SqlOrderExpression(SqlOrderType.Ascending, (SqlExpression)new SqlColumnRef(sqlColumn)));
                    }
                }
                foreach (SqlOrderExpression sqlOrderExpression in list1)
                    rowNumber.OrderBy.Add(new SqlOrderExpression(sqlOrderExpression.OrderType, (SqlExpression)sqlDuplicator.Duplicate((SqlNode)sqlOrderExpression.Expression)));
                return rowNumber;
            }

            private void PushDown(SqlColumn column)
            {
                SqlSelect sqlSelect = new SqlSelect((SqlExpression)new SqlNop(column.ClrType, column.SqlType, column.SourceExpression), this.currentSelect.From, this.currentSelect.SourceExpression);
                this.currentSelect.From = (SqlSource)new SqlAlias((SqlNode)sqlSelect);
                sqlSelect.Row.Columns.Add(column);
            }
        }

        internal class SqlGatherColumnsProduced
        {
            internal static List<SqlColumn> GatherColumns(SqlSource source)
            {
                List<SqlColumn> columns = new List<SqlColumn>();
                new SqlReorderer.SqlGatherColumnsProduced.Visitor(columns).Visit((SqlNode)source);
                return columns;
            }

            private class Visitor : SqlVisitor
            {
                private List<SqlColumn> columns;

                internal Visitor(List<SqlColumn> columns)
                {
                    this.columns = columns;
                }

                internal override SqlSelect VisitSelect(SqlSelect select)
                {
                    foreach (SqlColumn sqlColumn in select.Row.Columns)
                        this.columns.Add(sqlColumn);
                    return select;
                }

                internal override SqlNode VisitUnion(SqlUnion su)
                {
                    return (SqlNode)su;
                }
            }
        }
    }
}
