using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlUnionizer
    {
        internal static SqlNode Unionize(SqlNode node)
        {
            return new SqlUnionizer.Visitor().Visit(node);
        }

        private class Visitor : SqlVisitor
        {
            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                base.VisitSelect(select);
                SqlUnion union = this.GetUnion(select.From);
                if (union != null)
                {
                    SqlSelect sqlSelect1 = union.Left as SqlSelect;
                    SqlSelect sqlSelect2 = union.Right as SqlSelect;
                    if (sqlSelect1 != null & sqlSelect2 != null)
                    {
                        int index1 = 0;
                        for (int count = sqlSelect1.Row.Columns.Count; index1 < count; ++index1)
                            sqlSelect1.Row.Columns[index1].Ordinal = select.Row.Columns.Count + index1;
                        int index2 = 0;
                        for (int count = sqlSelect2.Row.Columns.Count; index2 < count; ++index2)
                            sqlSelect2.Row.Columns[index2].Ordinal = select.Row.Columns.Count + index2;
                        int index3 = 0;
                        for (int count1 = select.Row.Columns.Count; index3 < count1; ++index3)
                        {
                            SqlExprSet sqlExprSet = select.Row.Columns[index3].Expression as SqlExprSet;
                            if (sqlExprSet != null)
                            {
                                int index4 = 0;
                                for (int count2 = sqlExprSet.Expressions.Count; index4 < count2; ++index4)
                                {
                                    SqlColumnRef sqlColumnRef = sqlExprSet.Expressions[index4] as SqlColumnRef;
                                    if (sqlColumnRef != null && index4 >= select.Row.Columns.Count)
                                        sqlColumnRef.Column.Ordinal = index3;
                                }
                            }
                        }
                        Comparison<SqlColumn> comparison = (Comparison<SqlColumn>)((x, y) => x.Ordinal - y.Ordinal);
                        sqlSelect1.Row.Columns.Sort(comparison);
                        sqlSelect2.Row.Columns.Sort(comparison);
                    }
                }
                return select;
            }

            private SqlUnion GetUnion(SqlSource source)
            {
                SqlAlias sqlAlias = source as SqlAlias;
                if (sqlAlias != null)
                {
                    SqlUnion sqlUnion = sqlAlias.Node as SqlUnion;
                    if (sqlUnion != null)
                        return sqlUnion;
                }
                return (SqlUnion)null;
            }
        }
    }
}
