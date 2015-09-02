using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlCrossApplyToCrossJoin
    {
        internal static SqlNode Reduce(SqlNode node, SqlNodeAnnotations annotations)
        {
            SqlCrossApplyToCrossJoin.Reducer reducer = new SqlCrossApplyToCrossJoin.Reducer();
            reducer.Annotations = annotations;
            SqlNode node1 = node;
            return reducer.Visit(node1);
        }

        private class Reducer : SqlVisitor
        {
            internal SqlNodeAnnotations Annotations;

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                if (join.JoinType != SqlJoinType.CrossApply)
                    return base.VisitJoin(join);
                if (SqlGatherProducedAliases.Gather((SqlNode)join.Left).Overlaps((IEnumerable<SqlAlias>)SqlGatherConsumedAliases.Gather((SqlNode)join.Right)))
                {
                    SqlNodeAnnotations sqlNodeAnnotations = this.Annotations;
                    SqlJoin sqlJoin = join;
                    string message = Strings.SourceExpressionAnnotation((object)join.SourceExpression);
                    SqlProvider.ProviderMode[] providerModeArray = new SqlProvider.ProviderMode[1];
                    int index = 0;
                    int num = 1;
                    providerModeArray[index] = (SqlProvider.ProviderMode)num;
                    SqlServerCompatibilityAnnotation compatibilityAnnotation = new SqlServerCompatibilityAnnotation(message, providerModeArray);
                    sqlNodeAnnotations.Add((SqlNode)sqlJoin, (SqlNodeAnnotation)compatibilityAnnotation);
                    return base.VisitJoin(join);
                }
                join.JoinType = SqlJoinType.Cross;
                return this.VisitJoin(join);
            }
        }
    }
}
