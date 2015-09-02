using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlMultiplexer
    {
        private SqlMultiplexer.Visitor visitor;

        internal SqlMultiplexer(SqlMultiplexer.Options options, IEnumerable<SqlParameter> parentParameters, SqlFactory sqlFactory)
        {
            this.visitor = new SqlMultiplexer.Visitor(options, parentParameters, sqlFactory);
        }

        internal SqlNode Multiplex(SqlNode node)
        {
            return this.visitor.Visit(node);
        }

        internal enum Options
        {
            None,
            EnableBigJoin,
        }

        private class Visitor : SqlVisitor
        {
            private SqlMultiplexer.Options options;
            private SqlFactory sql;
            private SqlSelect outerSelect;
            private bool hasBigJoin;
            private bool canJoin;
            private bool isTopLevel;
            private IEnumerable<SqlParameter> parentParameters;

            internal Visitor(SqlMultiplexer.Options options, IEnumerable<SqlParameter> parentParameters, SqlFactory sqlFactory)
            {
                this.options = options;
                this.sql = sqlFactory;
                this.canJoin = true;
                this.isTopLevel = true;
                this.parentParameters = parentParameters;
            }

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
                if ((this.options & SqlMultiplexer.Options.EnableBigJoin) == SqlMultiplexer.Options.None || this.hasBigJoin || (!this.canJoin || !this.isTopLevel) || (this.outerSelect == null || MultisetChecker.HasMultiset(sms.Select.Selection) || !BigJoinChecker.CanBigJoin(sms.Select)))
                    return (SqlExpression)QueryExtractor.Extract(sms, this.parentParameters);
                sms.Select = this.VisitSelect(sms.Select);
                this.outerSelect.From = (SqlSource)new SqlJoin(SqlJoinType.OuterApply, this.outerSelect.From, (SqlSource)new SqlAlias((SqlNode)sms.Select), (SqlExpression)null, sms.SourceExpression);
                this.outerSelect.OrderingType = SqlOrderingType.Always;
                SqlExpression expression = (SqlExpression)SqlDuplicator.Copy((SqlNode)sms.Select.Selection);
                SqlAlias sqlAlias = new SqlAlias(SqlDuplicator.Copy((SqlNode)sms.Select));
                SqlExpression count = (SqlExpression)this.sql.SubSelect(SqlNodeType.ScalarSubSelect, new SqlSelect((SqlExpression)this.sql.Unary(SqlNodeType.Count, (SqlExpression)null, sms.SourceExpression), (SqlSource)sqlAlias, sms.SourceExpression)
                {
                    OrderingType = SqlOrderingType.Never
                });
                SqlJoinedCollection joinedCollection = new SqlJoinedCollection(sms.ClrType, sms.SqlType, expression, count, sms.SourceExpression);
                this.hasBigJoin = true;
                return (SqlExpression)joinedCollection;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem)
            {
                return (SqlExpression)QueryExtractor.Extract(elem, this.parentParameters);
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                bool flag1 = this.isTopLevel;
                this.isTopLevel = false;
                bool flag2 = this.canJoin;
                this.canJoin = false;
                try
                {
                    return base.VisitScalarSubSelect(ss);
                }
                finally
                {
                    this.isTopLevel = flag1;
                    this.canJoin = flag2;
                }
            }

            internal override SqlExpression VisitExists(SqlSubSelect ss)
            {
                bool flag1 = this.isTopLevel;
                this.isTopLevel = false;
                bool flag2 = this.canJoin;
                this.canJoin = false;
                try
                {
                    return base.VisitExists(ss);
                }
                finally
                {
                    this.isTopLevel = flag1;
                    this.canJoin = flag2;
                }
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SqlSelect sqlSelect = this.outerSelect;
                this.outerSelect = select;
                this.canJoin = ((this.canJoin ? 1 : 0) & (select.GroupBy.Count != 0 || select.Top != null ? 0 : (!select.IsDistinct ? 1 : 0))) != 0;
                bool flag = this.isTopLevel;
                this.isTopLevel = false;
                select = this.VisitSelectCore(select);
                this.isTopLevel = flag;
                select.Selection = this.VisitExpression(select.Selection);
                this.isTopLevel = flag;
                this.outerSelect = sqlSelect;
                if (select.IsDistinct && HierarchyChecker.HasHierarchy(select.Selection))
                    select.IsDistinct = false;
                return select;
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                this.canJoin = false;
                return base.VisitUnion(su);
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c)
            {
                bool flag = this.canJoin;
                this.canJoin = false;
                try
                {
                    return base.VisitClientCase(c);
                }
                finally
                {
                    this.canJoin = flag;
                }
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                bool flag = this.canJoin;
                this.canJoin = false;
                try
                {
                    return base.VisitSimpleCase(c);
                }
                finally
                {
                    this.canJoin = flag;
                }
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
            {
                bool flag = this.canJoin;
                this.canJoin = false;
                try
                {
                    return base.VisitSearchedCase(c);
                }
                finally
                {
                    this.canJoin = flag;
                }
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase tc)
            {
                bool flag = this.canJoin;
                this.canJoin = false;
                try
                {
                    return base.VisitTypeCase(tc);
                }
                finally
                {
                    this.canJoin = flag;
                }
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov)
            {
                bool flag = this.canJoin;
                this.canJoin = false;
                try
                {
                    return base.VisitOptionalValue(sov);
                }
                finally
                {
                    this.canJoin = flag;
                }
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq)
            {
                this.canJoin = false;
                return base.VisitUserQuery(suq);
            }
        }
    }
}
