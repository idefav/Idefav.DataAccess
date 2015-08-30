using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlTable : SqlNode
    {
        private MetaTable table;
        private MetaType rowType;
        private ProviderType sqlRowType;
        private List<SqlColumn> columns;

        internal MetaTable MetaTable
        {
            get
            {
                return this.table;
            }
        }

        internal string Name
        {
            get
            {
                return this.table.TableName;
            }
        }

        internal List<SqlColumn> Columns
        {
            get
            {
                return this.columns;
            }
        }

        internal MetaType RowType
        {
            get
            {
                return this.rowType;
            }
        }

        internal ProviderType SqlRowType
        {
            get
            {
                return this.sqlRowType;
            }
        }

        internal SqlTable(MetaTable table, MetaType rowType, ProviderType sqlRowType, Expression sourceExpression)
          : base(SqlNodeType.Table, sourceExpression)
        {
            this.table = table;
            this.rowType = rowType;
            this.sqlRowType = sqlRowType;
            this.columns = new List<SqlColumn>();
        }

        internal SqlColumn Find(string columnName)
        {
            foreach (SqlColumn sqlColumn in this.Columns)
            {
                if (sqlColumn.Name == columnName)
                    return sqlColumn;
            }
            return (SqlColumn)null;
        }
    }
}
