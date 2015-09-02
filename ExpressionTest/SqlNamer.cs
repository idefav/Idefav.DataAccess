using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlNamer
    {
        private SqlNamer.Visitor visitor;

        internal SqlNamer()
        {
            this.visitor = new SqlNamer.Visitor();
        }

        internal SqlNode AssignNames(SqlNode node)
        {
            return this.visitor.Visit(node);
        }

        internal static string DiscoverName(SqlExpression e)
        {
            if (e != null)
            {
                switch (e.NodeType)
                {
                    case SqlNodeType.Column:
                        return SqlNamer.DiscoverName(((SqlColumn)e).Expression);
                    case SqlNodeType.ColumnRef:
                        SqlColumnRef sqlColumnRef = (SqlColumnRef)e;
                        if (sqlColumnRef.Column.Name != null)
                            return sqlColumnRef.Column.Name;
                        return SqlNamer.DiscoverName((SqlExpression)sqlColumnRef.Column);
                    case SqlNodeType.ExprSet:
                        return SqlNamer.DiscoverName(((SqlExprSet)e).Expressions[0]);
                }
            }
            return "value";
        }

        private class Visitor : SqlVisitor
        {
            private int aliasCount;
            private SqlAlias alias;
            private bool makeUnique;
            private bool useMappedNames;
            private string lastName;

            internal Visitor()
            {
                this.makeUnique = true;
                this.useMappedNames = false;
            }

            internal string GetNextAlias()
            {
                string str = "t";
                int num = this.aliasCount;
                this.aliasCount = num + 1;
                // ISSUE: variable of a boxed type
                var local = (ValueType)num;
                return str + (object)local;
            }

            internal override SqlAlias VisitAlias(SqlAlias sqlAlias)
            {
                SqlAlias sqlAlias1 = this.alias;
                this.alias = sqlAlias;
                sqlAlias.Node = this.Visit(sqlAlias.Node);
                sqlAlias.Name = this.GetNextAlias();
                this.alias = sqlAlias1;
                return sqlAlias;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                base.VisitScalarSubSelect(ss);
                if (ss.Select.Row.Columns.Count > 0)
                    ss.Select.Row.Columns[0].Name = "";
                return (SqlExpression)ss;
            }

            internal override SqlStatement VisitInsert(SqlInsert insert)
            {
                bool flag1 = this.makeUnique;
                this.makeUnique = false;
                bool flag2 = this.useMappedNames;
                this.useMappedNames = true;
                SqlStatement sqlStatement = base.VisitInsert(insert);
                this.makeUnique = flag1;
                this.useMappedNames = flag2;
                return sqlStatement;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate update)
            {
                bool flag1 = this.makeUnique;
                this.makeUnique = false;
                bool flag2 = this.useMappedNames;
                this.useMappedNames = true;
                SqlStatement sqlStatement = base.VisitUpdate(update);
                this.makeUnique = flag1;
                this.useMappedNames = flag2;
                return sqlStatement;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                select = base.VisitSelect(select);
                string[] strArray = new string[select.Row.Columns.Count];
                int index1 = 0;
                for (int length = strArray.Length; index1 < length; ++index1)
                {
                    SqlColumn sqlColumn = select.Row.Columns[index1];
                    string str = sqlColumn.Name ?? SqlNamer.DiscoverName((SqlExpression)sqlColumn);
                    strArray[index1] = str;
                    sqlColumn.Name = (string)null;
                }
                ICollection<string> columnNames = this.GetColumnNames((IEnumerable<SqlOrderExpression>)select.OrderBy);
                int index2 = 0;
                for (int count = select.Row.Columns.Count; index2 < count; ++index2)
                {
                    SqlColumn c = select.Row.Columns[index2];
                    string str = strArray[index2];
                    string name = str;
                    if (this.makeUnique)
                    {
                        int num = 1;
                        for (; !this.IsUniqueName(select.Row.Columns, columnNames, c, name); name = str + (object)num)
                            ++num;
                    }
                    c.Name = name;
                    c.Ordinal = index2;
                }
                return select;
            }

            private bool IsUniqueName(List<SqlColumn> columns, ICollection<string> reservedNames, SqlColumn c, string name)
            {
                foreach (SqlColumn sqlColumn in columns)
                {
                    if (sqlColumn != c && string.Compare(sqlColumn.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                        return false;
                }
                if (!SqlNamer.Visitor.IsSimpleColumn(c, name))
                    return !reservedNames.Contains(name);
                return true;
            }

            private static bool IsSimpleColumn(SqlColumn c, string name)
            {
                if (c.Expression == null)
                    return true;
                if (c.Expression.NodeType != SqlNodeType.ColumnRef)
                    return false;
                SqlColumnRef sqlColumnRef = c.Expression as SqlColumnRef;
                if (!string.IsNullOrEmpty(name))
                    return string.Compare(name, sqlColumnRef.Column.Name, StringComparison.OrdinalIgnoreCase) == 0;
                return true;
            }

            internal override SqlExpression VisitExpression(SqlExpression expr)
            {
                string str = this.lastName;
                this.lastName = (string)null;
                try
                {
                    return (SqlExpression)this.Visit((SqlNode)expr);
                }
                finally
                {
                    this.lastName = str;
                }
            }

            private SqlExpression VisitNamedExpression(SqlExpression expr, string name)
            {
                string str = this.lastName;
                this.lastName = name;
                try
                {
                    return (SqlExpression)this.Visit((SqlNode)expr);
                }
                finally
                {
                    this.lastName = str;
                }
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                if (cref.Column.Name == null && this.lastName != null)
                    cref.Column.Name = this.lastName;
                return (SqlExpression)cref;
            }

            internal override SqlExpression VisitNew(SqlNew sox)
            {
                if (sox.Constructor != (ConstructorInfo)null)
                {
                    ParameterInfo[] parameters = sox.Constructor.GetParameters();
                    int index = 0;
                    for (int count = sox.Args.Count; index < count; ++index)
                        sox.Args[index] = this.VisitNamedExpression(sox.Args[index], parameters[index].Name);
                }
                else
                {
                    int index = 0;
                    for (int count = sox.Args.Count; index < count; ++index)
                        sox.Args[index] = this.VisitExpression(sox.Args[index]);
                }
                foreach (SqlMemberAssign sqlMemberAssign in sox.Members)
                {
                    string name = sqlMemberAssign.Member.Name;
                    if (this.useMappedNames)
                        name = sox.MetaType.GetDataMember(sqlMemberAssign.Member).MappedName;
                    sqlMemberAssign.Expression = this.VisitNamedExpression(sqlMemberAssign.Expression, name);
                }
                return (SqlExpression)sox;
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g)
            {
                g.Key = this.VisitNamedExpression(g.Key, "Key");
                g.Group = this.VisitNamedExpression(g.Group, "Group");
                return (SqlExpression)g;
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov)
            {
                sov.HasValue = this.VisitNamedExpression(sov.HasValue, "test");
                sov.Value = this.VisitExpression(sov.Value);
                return (SqlExpression)sov;
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                mc.Object = this.VisitExpression(mc.Object);
                ParameterInfo[] parameters = mc.Method.GetParameters();
                int index = 0;
                for (int count = mc.Arguments.Count; index < count; ++index)
                    mc.Arguments[index] = this.VisitNamedExpression(mc.Arguments[index], parameters[index].Name);
                return (SqlExpression)mc;
            }

            private ICollection<string> GetColumnNames(IEnumerable<SqlOrderExpression> orderList)
            {
                SqlNamer.ColumnNameGatherer columnNameGatherer = new SqlNamer.ColumnNameGatherer();
                foreach (SqlOrderExpression sqlOrderExpression in orderList)
                    columnNameGatherer.Visit((SqlNode)sqlOrderExpression.Expression);
                return (ICollection<string>)columnNameGatherer.Names;
            }
        }

        private class ColumnNameGatherer : SqlVisitor
        {
            public HashSet<string> Names { get; set; }

            public ColumnNameGatherer()
            {
                this.Names = new HashSet<string>();
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                if (!string.IsNullOrEmpty(col.Name))
                    this.Names.Add(col.Name);
                return base.VisitColumn(col);
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                this.Visit((SqlNode)cref.Column);
                return base.VisitColumnRef(cref);
            }
        }
    }
}
