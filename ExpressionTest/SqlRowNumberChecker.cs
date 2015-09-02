using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlRowNumberChecker
    {
        private SqlRowNumberChecker.Visitor rowNumberVisitor;

        internal SqlColumn RowNumberColumn
        {
            get
            {
                if (!this.rowNumberVisitor.HasRowNumber)
                    return (SqlColumn)null;
                return this.rowNumberVisitor.CurrentColumn;
            }
        }

        internal SqlRowNumberChecker()
        {
            this.rowNumberVisitor = new SqlRowNumberChecker.Visitor();
        }

        internal bool HasRowNumber(SqlNode node)
        {
            this.rowNumberVisitor.Visit(node);
            return this.rowNumberVisitor.HasRowNumber;
        }

        internal bool HasRowNumber(SqlRow row)
        {
            foreach (SqlNode node in row.Columns)
            {
                if (this.HasRowNumber(node))
                    return true;
            }
            return false;
        }

        private class Visitor : SqlVisitor
        {
            private bool hasRowNumber;

            public bool HasRowNumber
            {
                get
                {
                    return this.hasRowNumber;
                }
            }

            public SqlColumn CurrentColumn { get; private set; }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
            {
                this.hasRowNumber = true;
                return rowNumber;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                return (SqlExpression)ss;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return (SqlExpression)ss;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                int index = 0;
                for (int count = row.Columns.Count; index < count; ++index)
                {
                    row.Columns[index].Expression = this.VisitExpression(row.Columns[index].Expression);
                    if (this.hasRowNumber)
                    {
                        this.CurrentColumn = row.Columns[index];
                        break;
                    }
                }
                return row;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                this.Visit((SqlNode)select.Row);
                this.Visit((SqlNode)select.Where);
                return select;
            }
        }
    }
}
