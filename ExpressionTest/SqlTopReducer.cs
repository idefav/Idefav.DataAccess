using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlTopReducer
    {
        internal static SqlNode Reduce(SqlNode node, SqlNodeAnnotations annotations, SqlFactory sql)
        {
            return new SqlTopReducer.Visitor(annotations, sql).Visit(node);
        }

        private class Visitor : SqlVisitor
        {
            private SqlNodeAnnotations annotations;
            private SqlFactory sql;

            internal Visitor(SqlNodeAnnotations annotations, SqlFactory sql)
            {
                this.annotations = annotations;
                this.sql = sql;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                base.VisitSelect(select);
                if (select.Top != null)
                {
                    if (select.Top.NodeType == SqlNodeType.Value)
                    {
                        SqlValue sqlValue = (SqlValue)select.Top;
                        if (sqlValue.IsClientSpecified)
                            select.Top = this.sql.Value(sqlValue.ClrType, sqlValue.SqlType, sqlValue.Value, false, sqlValue.SourceExpression);
                    }
                    else
                    {
                        SqlNodeAnnotations sqlNodeAnnotations = this.annotations;
                        SqlExpression top = select.Top;
                        string message = Strings.SourceExpressionAnnotation((object)select.Top.SourceExpression);
                        SqlProvider.ProviderMode[] providerModeArray = new SqlProvider.ProviderMode[1];
                        int index = 0;
                        int num = 1;
                        providerModeArray[index] = (SqlProvider.ProviderMode)num;
                        SqlServerCompatibilityAnnotation compatibilityAnnotation = new SqlServerCompatibilityAnnotation(message, providerModeArray);
                        sqlNodeAnnotations.Add((SqlNode)top, (SqlNodeAnnotation)compatibilityAnnotation);
                    }
                }
                return select;
            }
        }
    }
}
