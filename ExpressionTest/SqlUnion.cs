using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlUnion : SqlNode
    {
        private SqlNode left;
        private SqlNode right;
        private bool all;

        internal SqlNode Left
        {
            get
            {
                return this.left;
            }
            set
            {
                this.Validate(value);
                this.left = value;
            }
        }

        internal SqlNode Right
        {
            get
            {
                return this.right;
            }
            set
            {
                this.Validate(value);
                this.right = value;
            }
        }

        internal bool All
        {
            get
            {
                return this.all;
            }
            set
            {
                this.all = value;
            }
        }

        internal SqlUnion(SqlNode left, SqlNode right, bool all)
          : base(SqlNodeType.Union, right.SourceExpression)
        {
            this.Left = left;
            this.Right = right;
            this.All = all;
        }

        private void Validate(SqlNode node)
        {
            if (node == null)
                throw Error.ArgumentNull("node");
            if (!(node is SqlExpression) && !(node is SqlSelect) && !(node is SqlUnion))
                throw Error.UnexpectedNode((object)node.NodeType);
        }

        internal Type GetClrType()
        {
            SqlExpression sqlExpression = this.Left as SqlExpression;
            if (sqlExpression != null)
                return sqlExpression.ClrType;
            SqlSelect sqlSelect = this.Left as SqlSelect;
            if (sqlSelect != null)
                return sqlSelect.Selection.ClrType;
            throw Error.CouldNotGetClrType();
        }

        internal ProviderType GetSqlType()
        {
            SqlExpression sqlExpression = this.Left as SqlExpression;
            if (sqlExpression != null)
                return sqlExpression.SqlType;
            SqlSelect sqlSelect = this.Left as SqlSelect;
            if (sqlSelect != null)
                return sqlSelect.Selection.SqlType;
            throw Error.CouldNotGetSqlType();
        }
    }
}
