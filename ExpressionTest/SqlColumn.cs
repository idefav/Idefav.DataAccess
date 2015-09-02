using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlColumn : SqlExpression
    {
        private SqlAlias alias;
        private string name;
        private int ordinal;
        private MetaDataMember member;
        private SqlExpression expression;
        private ProviderType sqlType;

        internal SqlAlias Alias
        {
            get
            {
                return this.alias;
            }
            set
            {
                this.alias = value;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        internal int Ordinal
        {
            get
            {
                return this.ordinal;
            }
            set
            {
                this.ordinal = value;
            }
        }

        internal MetaDataMember MetaMember
        {
            get
            {
                return this.member;
            }
        }

        internal SqlExpression Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                if (value != null)
                {
                    if (!this.ClrType.IsAssignableFrom(value.ClrType))
                        throw Error.ArgumentWrongType((object)"value", (object)this.ClrType, (object)value.ClrType);
                    SqlColumnRef sqlColumnRef = value as SqlColumnRef;
                    if (sqlColumnRef != null && sqlColumnRef.Column == this)
                        throw Error.ColumnCannotReferToItself();
                }
                this.expression = value;
            }
        }

        internal override ProviderType SqlType
        {
            get
            {
                if (this.expression != null)
                    return this.expression.SqlType;
                return this.sqlType;
            }
        }

        internal SqlColumn(Type clrType, ProviderType sqlType, string name, MetaDataMember member, SqlExpression expr, Expression sourceExpression)
          : base(SqlNodeType.Column, clrType, sourceExpression)
        {
            if (typeof(Type).IsAssignableFrom(clrType))
                throw Error.ArgumentWrongValue((object)"clrType");
            this.Name = name;
            this.member = member;
            this.Expression = expr;
            this.Ordinal = -1;
            if (sqlType == (ProviderType)null)
                throw Error.ArgumentNull("sqlType");
            this.sqlType = sqlType;
        }

        internal SqlColumn(string name, SqlExpression expr):base(SqlNodeType.Column, expr.ClrType,expr.SourceExpression)
        {
            Type clrType = expr.ClrType;
            ProviderType sqlType = expr.SqlType;
            string name1 = name;
            // ISSUE: variable of the null type
            
            SqlExpression expr1 = expr;
            Expression sourceExpression = expr1.SourceExpression;
            // ISSUE: explicit constructor call
           // this.base(clrType, sqlType, name1, (MetaDataMember)null, expr1, sourceExpression);

        }
    }
}
