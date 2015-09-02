using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlLiftWhereClauses
    {
        internal static SqlNode Lift(SqlNode node, TypeSystemProvider typeProvider, MetaModel model)
        {
            return new SqlLiftWhereClauses.Lifter(typeProvider, model).Visit(node);
        }

        private class Lifter : SqlVisitor
        {
            private SqlLiftWhereClauses.Lifter.Scope current;
            private SqlFactory sql;
            private SqlAggregateChecker aggregateChecker;
            private SqlRowNumberChecker rowNumberChecker;

            internal Lifter(TypeSystemProvider typeProvider, MetaModel model)
            {
                this.sql = new SqlFactory(typeProvider, model);
                this.aggregateChecker = new SqlAggregateChecker();
                this.rowNumberChecker = new SqlRowNumberChecker();
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SqlLiftWhereClauses.Lifter.Scope scope = this.current;
                this.current = new SqlLiftWhereClauses.Lifter.Scope(select.Where, this.current);
                SqlSelect sqlSelect = base.VisitSelect(select);
                bool flag = select.IsDistinct || select.GroupBy.Count > 0 || (this.aggregateChecker.HasAggregates((SqlNode)select) || select.Top != null) || this.rowNumberChecker.HasRowNumber((SqlNode)select);
                if (this.current != null)
                {
                    if (this.current.Parent != null && !flag)
                    {
                        this.current.Parent.Where = this.sql.AndAccumulate(this.current.Parent.Where, this.current.Where);
                        this.current.Where = (SqlExpression)null;
                    }
                    select.Where = this.current.Where;
                }
                this.current = scope;
                return sqlSelect;
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                SqlLiftWhereClauses.Lifter.Scope scope = this.current;
                this.current = (SqlLiftWhereClauses.Lifter.Scope)null;
                SqlNode sqlNode = base.VisitUnion(su);
                this.current = scope;
                return sqlNode;
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                SqlLiftWhereClauses.Lifter.Scope scope = this.current;
                try
                {
                    switch (join.JoinType)
                    {
                        case SqlJoinType.Cross:
                        case SqlJoinType.Inner:
                        case SqlJoinType.CrossApply:
                            return base.VisitJoin(join);
                        case SqlJoinType.LeftOuter:
                        case SqlJoinType.OuterApply:
                            join.Left = this.VisitSource(join.Left);
                            this.current = (SqlLiftWhereClauses.Lifter.Scope)null;
                            join.Right = this.VisitSource(join.Right);
                            join.Condition = this.VisitExpression(join.Condition);
                            return (SqlSource)join;
                        default:
                            this.current = (SqlLiftWhereClauses.Lifter.Scope)null;
                            return base.VisitJoin(join);
                    }
                }
                finally
                {
                    this.current = scope;
                }
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                SqlLiftWhereClauses.Lifter.Scope scope = this.current;
                this.current = (SqlLiftWhereClauses.Lifter.Scope)null;
                SqlExpression sqlExpression = base.VisitSubSelect(ss);
                this.current = scope;
                return sqlExpression;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                SqlLiftWhereClauses.Lifter.Scope scope = this.current;
                this.current = (SqlLiftWhereClauses.Lifter.Scope)null;
                SqlExpression sqlExpression = base.VisitClientQuery(cq);
                this.current = scope;
                return sqlExpression;
            }

            private class Scope
            {
                internal SqlLiftWhereClauses.Lifter.Scope Parent;
                internal SqlExpression Where;

                internal Scope(SqlExpression where, SqlLiftWhereClauses.Lifter.Scope parent)
                {
                    this.Where = where;
                    this.Parent = parent;
                }
            }
        }
    }
}
