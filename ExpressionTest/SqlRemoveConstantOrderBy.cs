using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlRemoveConstantOrderBy
    {
        internal static SqlNode Remove(SqlNode node)
        {
            return new SqlRemoveConstantOrderBy.Visitor().Visit(node);
        }

        private class Visitor : SqlVisitor
        {
            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                int index = 0;
                List<SqlOrderExpression> orderBy = select.OrderBy;
                while (index < orderBy.Count)
                {
                    SqlExpression sqlExpression = orderBy[index].Expression;
                    while (sqlExpression.NodeType == SqlNodeType.DiscriminatedType)
                        sqlExpression = ((SqlDiscriminatedType)sqlExpression).Discriminator;
                    switch (sqlExpression.NodeType)
                    {
                        case SqlNodeType.Parameter:
                        case SqlNodeType.Value:
                            orderBy.RemoveAt(index);
                            continue;
                        default:
                            ++index;
                            continue;
                    }
                }
                return base.VisitSelect(select);
            }
        }
    }
}
