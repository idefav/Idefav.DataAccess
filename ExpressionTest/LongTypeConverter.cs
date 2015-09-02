using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class LongTypeConverter
    {
        private LongTypeConverter.Visitor visitor;

        internal LongTypeConverter(SqlFactory sql)
        {
            this.visitor = new LongTypeConverter.Visitor(sql);
        }

        internal SqlNode AddConversions(SqlNode node, SqlNodeAnnotations annotations)
        {
            this.visitor.Annotations = annotations;
            return this.visitor.Visit(node);
        }

        private class Visitor : SqlVisitor
        {
            private SqlFactory sql;
            private SqlNodeAnnotations annotations;

            internal SqlNodeAnnotations Annotations
            {
                set
                {
                    this.annotations = value;
                }
            }

            internal Visitor(SqlFactory sql)
            {
                this.sql = sql;
            }

            private SqlExpression ConvertToMax(SqlExpression expr, ProviderType newType)
            {
                SqlFactory sqlFactory = this.sql;
                Type clrType = expr.ClrType;
                ProviderType targetSqlType = newType;
                SqlExpression expression = expr;
                Expression sourceExpression = expression.SourceExpression;
                return (SqlExpression)sqlFactory.UnaryConvert(clrType, targetSqlType, expression, sourceExpression);
            }

            private SqlExpression ConvertToMax(SqlExpression expr, out bool changed)
            {
                changed = false;
                if (!expr.SqlType.IsLargeType)
                    return expr;
                ProviderType bestLargeType = this.sql.TypeProvider.GetBestLargeType(expr.SqlType);
                changed = true;
                if (expr.SqlType != bestLargeType)
                    return this.ConvertToMax(expr, bestLargeType);
                changed = false;
                return expr;
            }

            private void ConvertColumnsToMax(SqlSelect select, out bool changed, out bool containsLongExpressions)
            {
                SqlRow row = select.Row;
                changed = false;
                containsLongExpressions = false;
                foreach (SqlColumn sqlColumn in row.Columns)
                {
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    //bool&local = @containsLongExpressions;
                    // ISSUE: explicit reference operation
                    //int num = ^ local ? 1 : (sqlColumn.SqlType.IsLargeType ? 1 : 0);
          // ISSUE: explicit reference operation
          //^ local = num != 0;
                    bool changed1;
                    sqlColumn.Expression = this.ConvertToMax(sqlColumn.Expression, out changed1);
                    changed |= changed1;
                }
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                if (select.IsDistinct)
                {
                    bool changed;
                    bool containsLongExpressions;
                    this.ConvertColumnsToMax(select, out changed, out containsLongExpressions);
                    if (containsLongExpressions)
                    {
                        SqlNodeAnnotations sqlNodeAnnotations = this.annotations;
                        SqlSelect sqlSelect = select;
                        string message = Strings.TextNTextAndImageCannotOccurInDistinct((object)select.SourceExpression);
                        SqlProvider.ProviderMode[] providerModeArray = new SqlProvider.ProviderMode[2];
                        int index1 = 0;
                        int num1 = 1;
                        providerModeArray[index1] = (SqlProvider.ProviderMode)num1;
                        int index2 = 1;
                        int num2 = 4;
                        providerModeArray[index2] = (SqlProvider.ProviderMode)num2;
                        SqlServerCompatibilityAnnotation compatibilityAnnotation = new SqlServerCompatibilityAnnotation(message, providerModeArray);
                        sqlNodeAnnotations.Add((SqlNode)sqlSelect, (SqlNodeAnnotation)compatibilityAnnotation);
                    }
                }
                return base.VisitSelect(select);
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                bool changed1 = false;
                bool containsLongExpressions1 = false;
                SqlSelect select1 = su.Left as SqlSelect;
                if (select1 != null)
                    this.ConvertColumnsToMax(select1, out changed1, out containsLongExpressions1);
                bool changed2 = false;
                bool containsLongExpressions2 = false;
                SqlSelect select2 = su.Right as SqlSelect;
                if (select2 != null)
                    this.ConvertColumnsToMax(select2, out changed2, out containsLongExpressions2);
                if (!su.All && containsLongExpressions1 | containsLongExpressions2)
                {
                    SqlNodeAnnotations sqlNodeAnnotations = this.annotations;
                    SqlUnion sqlUnion = su;
                    string message = Strings.TextNTextAndImageCannotOccurInUnion((object)su.SourceExpression);
                    SqlProvider.ProviderMode[] providerModeArray = new SqlProvider.ProviderMode[2];
                    int index1 = 0;
                    int num1 = 1;
                    providerModeArray[index1] = (SqlProvider.ProviderMode)num1;
                    int index2 = 1;
                    int num2 = 4;
                    providerModeArray[index2] = (SqlProvider.ProviderMode)num2;
                    SqlServerCompatibilityAnnotation compatibilityAnnotation = new SqlServerCompatibilityAnnotation(message, providerModeArray);
                    sqlNodeAnnotations.Add((SqlNode)sqlUnion, (SqlNodeAnnotation)compatibilityAnnotation);
                }
                return base.VisitUnion(su);
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                if (fc.Name == "LEN")
                {
                    bool changed;
                    fc.Arguments[0] = this.ConvertToMax(fc.Arguments[0], out changed);
                    if (fc.Arguments[0].SqlType.IsLargeType)
                    {
                        SqlNodeAnnotations sqlNodeAnnotations = this.annotations;
                        SqlFunctionCall sqlFunctionCall = fc;
                        string message = Strings.LenOfTextOrNTextNotSupported((object)fc.SourceExpression);
                        SqlProvider.ProviderMode[] providerModeArray = new SqlProvider.ProviderMode[1];
                        int index = 0;
                        int num = 1;
                        providerModeArray[index] = (SqlProvider.ProviderMode)num;
                        SqlServerCompatibilityAnnotation compatibilityAnnotation = new SqlServerCompatibilityAnnotation(message, providerModeArray);
                        sqlNodeAnnotations.Add((SqlNode)sqlFunctionCall, (SqlNodeAnnotation)compatibilityAnnotation);
                    }
                }
                return base.VisitFunctionCall(fc);
            }
        }
    }
}
