using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlFormatter : DbFormatter
    {
        private SqlFormatter.Visitor visitor;

        internal bool ParenthesizeTop
        {
            get
            {
                return this.visitor.parenthesizeTop;
            }
            set
            {
                this.visitor.parenthesizeTop = value;
            }
        }

        internal SqlFormatter()
        {
            this.visitor = new SqlFormatter.Visitor();
        }

        internal override string Format(SqlNode node, bool isDebug)
        {
            return this.visitor.Format(node, isDebug);
        }

        internal string[] FormatBlock(SqlBlock block, bool isDebug)
        {
            List<string> list = new List<string>(block.Statements.Count);
            int index = 0;
            for (int count = block.Statements.Count; index < count; ++index)
            {
                SqlStatement sqlStatement = block.Statements[index];
                SqlSelect sqlSelect = sqlStatement as SqlSelect;
                if (sqlSelect == null || !sqlSelect.DoNotOutput)
                    list.Add(this.Format((SqlNode)sqlStatement, isDebug));
            }
            return list.ToArray();
        }

        internal override string Format(SqlNode node)
        {
            return this.visitor.Format(node);
        }

        internal class Visitor : SqlVisitor
        {
            internal List<SqlSource> suppressedAliases = new List<SqlSource>();
            internal Dictionary<SqlNode, string> names = new Dictionary<SqlNode, string>();
            internal Dictionary<SqlColumn, SqlAlias> aliasMap = new Dictionary<SqlColumn, SqlAlias>();
            internal StringBuilder sb;
            internal bool isDebugMode;
            internal int depth;
            internal bool parenthesizeTop;

            internal Visitor()
            {
            }

            internal string Format(SqlNode node, bool isDebug)
            {
                this.sb = new StringBuilder();
                this.isDebugMode = isDebug;
                this.aliasMap.Clear();
                if (isDebug)
                    new SqlFormatter.AliasMapper(this.aliasMap).Visit(node);
                this.Visit(node);
                return this.sb.ToString();
            }

            internal string Format(SqlNode node)
            {
                return this.Format(node, false);
            }

            internal virtual void VisitWithParens(SqlNode node, SqlNode outer)
            {
                if (node == null)
                    return;
                switch (node.NodeType)
                {
                    case SqlNodeType.TableValuedFunctionCall:
                    case SqlNodeType.Value:
                    case SqlNodeType.OuterJoinedValue:
                    case SqlNodeType.Parameter:
                    case SqlNodeType.FunctionCall:
                    case SqlNodeType.Member:
                    case SqlNodeType.ColumnRef:
                        this.Visit(node);
                        return;
                    case SqlNodeType.Mul:
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                    case SqlNodeType.Or:
                    case SqlNodeType.Add:
                    case SqlNodeType.And:
                    case SqlNodeType.BitAnd:
                    case SqlNodeType.BitNot:
                    case SqlNodeType.BitOr:
                    case SqlNodeType.BitXor:
                        if (outer.NodeType == node.NodeType)
                        {
                            this.Visit(node);
                            return;
                        }
                        break;
                }
                this.sb.Append("(");
                this.Visit(node);
                this.sb.Append(")");
            }

            internal override SqlExpression VisitNop(SqlNop nop)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"NOP");
                this.sb.Append("NOP()");
                return (SqlExpression)nop;
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                switch (uo.NodeType)
                {
                    case SqlNodeType.Sum:
                    case SqlNodeType.Stddev:
                    case SqlNodeType.Min:
                    case SqlNodeType.LongCount:
                    case SqlNodeType.Max:
                    case SqlNodeType.Count:
                    case SqlNodeType.Avg:
                    case SqlNodeType.ClrLength:
                        this.sb.Append(this.GetOperator(uo.NodeType));
                        this.sb.Append("(");
                        if (uo.Operand == null)
                            this.sb.Append("*");
                        else
                            this.Visit((SqlNode)uo.Operand);
                        this.sb.Append(")");
                        break;
                    case SqlNodeType.ValueOf:
                    case SqlNodeType.OuterJoinedValue:
                        this.Visit((SqlNode)uo.Operand);
                        break;
                    case SqlNodeType.Negate:
                    case SqlNodeType.BitNot:
                        this.sb.Append(this.GetOperator(uo.NodeType));
                        this.VisitWithParens((SqlNode)uo.Operand, (SqlNode)uo);
                        break;
                    case SqlNodeType.Not:
                    case SqlNodeType.Not2V:
                        this.sb.Append(this.GetOperator(uo.NodeType));
                        this.sb.Append(" ");
                        this.VisitWithParens((SqlNode)uo.Operand, (SqlNode)uo);
                        break;
                    case SqlNodeType.IsNotNull:
                    case SqlNodeType.IsNull:
                        this.VisitWithParens((SqlNode)uo.Operand, (SqlNode)uo);
                        this.sb.Append(" ");
                        this.sb.Append(this.GetOperator(uo.NodeType));
                        break;
                    case SqlNodeType.Convert:
                        this.sb.Append("CONVERT(");
                        QueryFormatOptions formatOptions = QueryFormatOptions.None;
                        if (uo.Operand.SqlType.CanSuppressSizeForConversionToString)
                            formatOptions = QueryFormatOptions.SuppressSize;
                        this.sb.Append(uo.SqlType.ToQueryString(formatOptions));
                        this.sb.Append(",");
                        this.Visit((SqlNode)uo.Operand);
                        this.sb.Append(")");
                        break;
                    default:
                        throw Error.InvalidFormatNode((object)uo.NodeType);
                }
                return (SqlExpression)uo;
            }

            internal override SqlRowNumber VisitRowNumber(SqlRowNumber rowNumber)
            {
                this.sb.Append("ROW_NUMBER() OVER (ORDER BY ");
                int index = 0;
                for (int count = rowNumber.OrderBy.Count; index < count; ++index)
                {
                    SqlOrderExpression sqlOrderExpression = rowNumber.OrderBy[index];
                    if (index > 0)
                        this.sb.Append(", ");
                    this.Visit((SqlNode)sqlOrderExpression.Expression);
                    if (sqlOrderExpression.OrderType == SqlOrderType.Descending)
                        this.sb.Append(" DESC");
                }
                this.sb.Append(")");
                return rowNumber;
            }

            internal override SqlExpression VisitLift(SqlLift lift)
            {
                this.Visit((SqlNode)lift.Expression);
                return (SqlExpression)lift;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                if (bo.NodeType == SqlNodeType.Coalesce)
                {
                    this.sb.Append("COALESCE(");
                    this.Visit((SqlNode)bo.Left);
                    this.sb.Append(",");
                    this.Visit((SqlNode)bo.Right);
                    this.sb.Append(")");
                }
                else
                {
                    this.VisitWithParens((SqlNode)bo.Left, (SqlNode)bo);
                    this.sb.Append(" ");
                    this.sb.Append(this.GetOperator(bo.NodeType));
                    this.sb.Append(" ");
                    this.VisitWithParens((SqlNode)bo.Right, (SqlNode)bo);
                }
                return (SqlExpression)bo;
            }

            internal override SqlExpression VisitBetween(SqlBetween between)
            {
                this.VisitWithParens((SqlNode)between.Expression, (SqlNode)between);
                this.sb.Append(" BETWEEN ");
                this.Visit((SqlNode)between.Start);
                this.sb.Append(" AND ");
                this.Visit((SqlNode)between.End);
                return (SqlExpression)between;
            }

            internal override SqlExpression VisitIn(SqlIn sin)
            {
                this.VisitWithParens((SqlNode)sin.Expression, (SqlNode)sin);
                this.sb.Append(" IN (");
                int index = 0;
                for (int count = sin.Values.Count; index < count; ++index)
                {
                    if (index > 0)
                        this.sb.Append(", ");
                    this.Visit((SqlNode)sin.Values[index]);
                }
                this.sb.Append(")");
                return (SqlExpression)sin;
            }

            internal override SqlExpression VisitLike(SqlLike like)
            {
                this.VisitWithParens((SqlNode)like.Expression, (SqlNode)like);
                this.sb.Append(" LIKE ");
                this.Visit((SqlNode)like.Pattern);
                if (like.Escape != null)
                {
                    this.sb.Append(" ESCAPE ");
                    this.Visit((SqlNode)like.Escape);
                }
                return (SqlExpression)like;
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                if (fc.Name.Contains("."))
                    this.WriteName(fc.Name);
                else
                    this.sb.Append(fc.Name);
                this.sb.Append("(");
                int index = 0;
                for (int count = fc.Arguments.Count; index < count; ++index)
                {
                    if (index > 0)
                        this.sb.Append(", ");
                    this.Visit((SqlNode)fc.Arguments[index]);
                }
                this.sb.Append(")");
                return (SqlExpression)fc;
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
            {
                return this.VisitFunctionCall((SqlFunctionCall)fc);
            }

            internal override SqlExpression VisitCast(SqlUnary c)
            {
                this.sb.Append("CAST(");
                this.Visit((SqlNode)c.Operand);
                this.sb.Append(" AS ");
                QueryFormatOptions formatOptions = QueryFormatOptions.None;
                if (c.Operand.SqlType.CanSuppressSizeForConversionToString)
                    formatOptions = QueryFormatOptions.SuppressSize;
                this.sb.Append(c.SqlType.ToQueryString(formatOptions));
                this.sb.Append(")");
                return (SqlExpression)c;
            }

            internal override SqlExpression VisitTreat(SqlUnary t)
            {
                this.sb.Append("TREAT(");
                this.Visit((SqlNode)t.Operand);
                this.sb.Append(" AS ");
                this.FormatType(t.SqlType);
                this.sb.Append(")");
                return (SqlExpression)t;
            }

            internal override SqlExpression VisitColumn(SqlColumn c)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"Column");
                this.sb.Append("COLUMN(");
                if (c.Expression != null)
                {
                    this.Visit((SqlNode)c.Expression);
                }
                else
                {
                    string str = (string)null;
                    if (c.Alias != null)
                    {
                        if (c.Alias.Name == null)
                        {
                            if (!this.names.TryGetValue((SqlNode)c.Alias, out str))
                            {
                                str = "A" + (object)this.names.Count;
                                this.names[(SqlNode)c.Alias] = str;
                            }
                        }
                        else
                            str = c.Alias.Name;
                    }
                    this.sb.Append(str);
                    this.sb.Append(".");
                    this.sb.Append(c.Name);
                }
                this.sb.Append(")");
                return (SqlExpression)c;
            }

            internal override SqlExpression VisitDiscriminatedType(SqlDiscriminatedType dt)
            {
                if (this.isDebugMode)
                    this.sb.Append("DTYPE(");
                base.VisitDiscriminatedType(dt);
                if (this.isDebugMode)
                    this.sb.Append(")");
                return (SqlExpression)dt;
            }

            internal override SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof)
            {
                if (this.isDebugMode)
                    this.sb.Append("DISCO(");
                base.VisitDiscriminatorOf(dof);
                if (this.isDebugMode)
                    this.sb.Append(")");
                return (SqlExpression)dof;
            }

            internal override SqlExpression VisitSimpleExpression(SqlSimpleExpression simple)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"SIMPLE");
                this.sb.Append("SIMPLE(");
                base.VisitSimpleExpression(simple);
                this.sb.Append(")");
                return (SqlExpression)simple;
            }

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"Shared");
                this.sb.Append("SHARED(");
                this.Visit((SqlNode)shared.Expression);
                this.sb.Append(")");
                return (SqlExpression)shared;
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"SharedRef");
                this.sb.Append("SHAREDREF(");
                this.Visit((SqlNode)sref.SharedExpression.Expression);
                this.sb.Append(")");
                return (SqlExpression)sref;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                string s1 = (string)null;
                SqlColumn column = cref.Column;
                SqlAlias alias = column.Alias;
                if (alias == null)
                    this.aliasMap.TryGetValue(column, out alias);
                if (alias != null)
                {
                    if (alias.Name == null)
                    {
                        if (!this.names.TryGetValue((SqlNode)alias, out s1))
                        {
                            s1 = "A" + (object)this.names.Count;
                            this.names[(SqlNode)column.Alias] = s1;
                        }
                    }
                    else
                        s1 = column.Alias.Name;
                }
                if (!this.suppressedAliases.Contains((SqlSource)column.Alias) && s1 != null && s1.Length != 0)
                {
                    this.WriteName(s1);
                    this.sb.Append(".");
                }
                string s2 = column.Name;
                string str = this.InferName(column.Expression, (string)null);
                if (s2 == null)
                    s2 = str;
                if (s2 == null && !this.names.TryGetValue((SqlNode)column, out s2))
                {
                    s2 = "C" + (object)this.names.Count;
                    this.names[(SqlNode)column] = s2;
                }
                this.WriteName(s2);
                return (SqlExpression)cref;
            }

            internal virtual void WriteName(string s)
            {
                this.sb.Append(SqlIdentifier.QuoteCompoundIdentifier(s));
            }

            internal virtual void WriteVariableName(string s)
            {
                if (s.StartsWith("@", StringComparison.Ordinal))
                    this.sb.Append(SqlIdentifier.QuoteCompoundIdentifier(s));
                else
                    this.sb.Append(SqlIdentifier.QuoteCompoundIdentifier("@" + s));
            }

            internal override SqlExpression VisitParameter(SqlParameter p)
            {
                this.sb.Append(p.Name);
                return (SqlExpression)p;
            }

            internal override SqlExpression VisitValue(SqlValue value)
            {
                if (value.IsClientSpecified && !this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"Value");
                this.FormatValue(value.Value);
                return (SqlExpression)value;
            }

            internal override SqlExpression VisitClientParameter(SqlClientParameter cp)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"ClientParameter");
                this.sb.Append("client-parameter(");
                object obj;
                try
                {
                    obj = cp.Accessor.Compile().DynamicInvoke(new object[1]);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
                this.sb.Append(obj);
                this.sb.Append(")");
                return (SqlExpression)cp;
            }

            internal override SqlExpression VisitScalarSubSelect(SqlSubSelect ss)
            {
                int num = this.depth;
                this.depth = this.depth + 1;
                if (this.isDebugMode)
                    this.sb.Append("SCALAR");
                this.sb.Append("(");
                this.NewLine();
                this.Visit((SqlNode)ss.Select);
                this.NewLine();
                this.sb.Append(")");
                this.depth = num;
                return (SqlExpression)ss;
            }

            internal override SqlExpression VisitElement(SqlSubSelect elem)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"Element");
                int num = this.depth;
                this.depth = this.depth + 1;
                this.sb.Append("ELEMENT(");
                this.NewLine();
                this.Visit((SqlNode)elem.Select);
                this.NewLine();
                this.sb.Append(")");
                this.depth = num;
                return (SqlExpression)elem;
            }

            internal override SqlExpression VisitMultiset(SqlSubSelect sms)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"Multiset");
                int num = this.depth;
                this.depth = this.depth + 1;
                this.sb.Append("MULTISET(");
                this.NewLine();
                this.Visit((SqlNode)sms.Select);
                this.NewLine();
                this.sb.Append(")");
                this.depth = num;
                return (SqlExpression)sms;
            }

            internal override SqlExpression VisitExists(SqlSubSelect sqlExpr)
            {
                int num = this.depth;
                this.depth = this.depth + 1;
                this.sb.Append("EXISTS(");
                this.NewLine();
                this.Visit((SqlNode)sqlExpr.Select);
                this.NewLine();
                this.sb.Append(")");
                this.depth = num;
                return (SqlExpression)sqlExpr;
            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                this.WriteName(tab.Name);
                return tab;
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq)
            {
                if (suq.Arguments.Count > 0)
                {
                    StringBuilder stringBuilder = this.sb;
                    this.sb = new StringBuilder();
                    object[] objArray = new object[suq.Arguments.Count];
                    int index = 0;
                    for (int length = objArray.Length; index < length; ++index)
                    {
                        this.Visit((SqlNode)suq.Arguments[index]);
                        objArray[index] = (object)this.sb.ToString();
                        this.sb.Length = 0;
                    }
                    this.sb = stringBuilder;
                    this.sb.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, suq.QueryText, objArray));
                }
                else
                    this.sb.Append(suq.QueryText);
                return suq;
            }

            internal override SqlExpression VisitUserColumn(SqlUserColumn suc)
            {
                this.sb.Append(suc.Name);
                return (SqlExpression)suc;
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                this.sb.Append("EXEC @RETURN_VALUE = ");
                this.WriteName(spc.Function.MappedName);
                this.sb.Append(" ");
                int count = spc.Function.Parameters.Count;
                for (int index = 0; index < count; ++index)
                {
                    MetaParameter metaParameter = spc.Function.Parameters[index];
                    if (index > 0)
                        this.sb.Append(", ");
                    this.WriteVariableName(metaParameter.MappedName);
                    this.sb.Append(" = ");
                    this.Visit((SqlNode)spc.Arguments[index]);
                    if (metaParameter.Parameter.IsOut || metaParameter.Parameter.ParameterType.IsByRef)
                        this.sb.Append(" OUTPUT");
                }
                if (spc.Arguments.Count > count)
                {
                    if (count > 0)
                        this.sb.Append(", ");
                    this.WriteVariableName(spc.Function.ReturnParameter.MappedName);
                    this.sb.Append(" = ");
                    this.Visit((SqlNode)spc.Arguments[count]);
                    this.sb.Append(" OUTPUT");
                }
                return spc;
            }

            internal override SqlAlias VisitAlias(SqlAlias alias)
            {
                int num1 = alias.Node is SqlSelect ? 1 : 0;
                int num2 = this.depth;
                string s = (string)null;
                string str = "";
                SqlTable sqlTable = alias.Node as SqlTable;
                if (sqlTable != null)
                    str = sqlTable.Name;
                if (alias.Name == null)
                {
                    if (!this.names.TryGetValue((SqlNode)alias, out s))
                    {
                        s = "A" + (object)this.names.Count;
                        this.names[(SqlNode)alias] = s;
                    }
                }
                else
                    s = alias.Name;
                if (num1 != 0)
                {
                    this.depth = this.depth + 1;
                    this.sb.Append("(");
                    this.NewLine();
                }
                this.Visit(alias.Node);
                if (num1 != 0)
                {
                    this.NewLine();
                    this.sb.Append(")");
                    this.depth = num2;
                }
                if (!this.suppressedAliases.Contains((SqlSource)alias) && s != null && str != s)
                {
                    this.sb.Append(" AS ");
                    this.WriteName(s);
                }
                return alias;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                this.sb.Append("AREF(");
                this.WriteAliasName(aref.Alias);
                this.sb.Append(")");
                return (SqlExpression)aref;
            }

            private void WriteAliasName(SqlAlias alias)
            {
                string s = (string)null;
                if (alias.Name == null)
                {
                    if (!this.names.TryGetValue((SqlNode)alias, out s))
                    {
                        s = "A" + (object)this.names.Count;
                        this.names[(SqlNode)alias] = s;
                    }
                }
                else
                    s = alias.Name;
                this.WriteName(s);
            }

            internal override SqlNode VisitUnion(SqlUnion su)
            {
                this.sb.Append("(");
                int num = this.depth;
                this.depth = this.depth + 1;
                this.NewLine();
                this.Visit(su.Left);
                this.NewLine();
                this.sb.Append("UNION");
                if (su.All)
                    this.sb.Append(" ALL");
                this.NewLine();
                this.Visit(su.Right);
                this.NewLine();
                this.sb.Append(")");
                this.depth = num;
                return (SqlNode)su;
            }

            internal override SqlExpression VisitExprSet(SqlExprSet xs)
            {
                if (this.isDebugMode)
                {
                    this.sb.Append("ES(");
                    int index = 0;
                    for (int count = xs.Expressions.Count; index < count; ++index)
                    {
                        if (index > 0)
                            this.sb.Append(", ");
                        this.Visit((SqlNode)xs.Expressions[index]);
                    }
                    this.sb.Append(")");
                }
                else
                    this.Visit((SqlNode)xs.GetFirstExpression());
                return (SqlExpression)xs;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                int index = 0;
                for (int count = row.Columns.Count; index < count; ++index)
                {
                    SqlColumn sqlColumn = row.Columns[index];
                    if (index > 0)
                        this.sb.Append(", ");
                    this.Visit((SqlNode)sqlColumn.Expression);
                    string s = sqlColumn.Name;
                    string str = this.InferName(sqlColumn.Expression, (string)null);
                    if (s == null)
                        s = str;
                    if (s == null && !this.names.TryGetValue((SqlNode)sqlColumn, out s))
                    {
                        s = "C" + (object)this.names.Count;
                        this.names[(SqlNode)sqlColumn] = s;
                    }
                    if (s != str && !string.IsNullOrEmpty(s))
                    {
                        this.sb.Append(" AS ");
                        this.WriteName(s);
                    }
                }
                return row;
            }

            internal override SqlExpression VisitNew(SqlNew sox)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"New");
                this.sb.Append("new ");
                this.sb.Append(sox.ClrType.Name);
                this.sb.Append("{ ");
                int index1 = 0;
                for (int count = sox.Args.Count; index1 < count; ++index1)
                {
                    SqlExpression sqlExpression = sox.Args[index1];
                    if (index1 > 0)
                        this.sb.Append(", ");
                    this.sb.Append(sox.ArgMembers[index1].Name);
                    this.sb.Append(" = ");
                    this.Visit((SqlNode)sqlExpression);
                }
                int index2 = 0;
                for (int count = sox.Members.Count; index2 < count; ++index2)
                {
                    SqlMemberAssign sqlMemberAssign = sox.Members[index2];
                    if (index2 > 0)
                        this.sb.Append(", ");
                    if (this.InferName(sqlMemberAssign.Expression, (string)null) != sqlMemberAssign.Member.Name)
                    {
                        this.sb.Append(sqlMemberAssign.Member.Name);
                        this.sb.Append(" = ");
                    }
                    this.Visit((SqlNode)sqlMemberAssign.Expression);
                }
                this.sb.Append(" }");
                return (SqlExpression)sox;
            }

            internal override SqlExpression VisitClientArray(SqlClientArray scar)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"ClientArray");
                this.sb.Append("new []{");
                int index = 0;
                for (int count = scar.Expressions.Count; index < count; ++index)
                {
                    if (index > 0)
                        this.sb.Append(", ");
                    this.Visit((SqlNode)scar.Expressions[index]);
                }
                this.sb.Append("}");
                return (SqlExpression)scar;
            }

            internal override SqlNode VisitMember(SqlMember m)
            {
                this.Visit((SqlNode)m.Expression);
                this.sb.Append(".");
                this.sb.Append(m.Member.Name);
                return (SqlNode)m;
            }

            internal virtual void NewLine()
            {
                if (this.sb.Length > 0)
                    this.sb.AppendLine();
                for (int index = 0; index < this.depth; ++index)
                    this.sb.Append("    ");
            }

            internal override SqlSelect VisitSelect(SqlSelect ss)
            {
                if (ss.DoNotOutput)
                    return ss;
                string str = (string)null;
                if (ss.From != null)
                {
                    StringBuilder stringBuilder = this.sb;
                    this.sb = new StringBuilder();
                    if (this.IsSimpleCrossJoinList((SqlNode)ss.From))
                        this.VisitCrossJoinList((SqlNode)ss.From);
                    else
                        this.Visit((SqlNode)ss.From);
                    str = this.sb.ToString();
                    this.sb = stringBuilder;
                }
                this.sb.Append("SELECT ");
                if (ss.IsDistinct)
                    this.sb.Append("DISTINCT ");
                if (ss.Top != null)
                {
                    this.sb.Append("TOP ");
                    if (this.parenthesizeTop)
                        this.sb.Append("(");
                    this.Visit((SqlNode)ss.Top);
                    if (this.parenthesizeTop)
                        this.sb.Append(")");
                    this.sb.Append(" ");
                    if (ss.IsPercent)
                        this.sb.Append(" PERCENT ");
                }
                if (ss.Row.Columns.Count > 0)
                    this.VisitRow(ss.Row);
                else if (this.isDebugMode)
                    this.Visit((SqlNode)ss.Selection);
                else
                    this.sb.Append("NULL AS [EMPTY]");
                if (str != null)
                {
                    this.NewLine();
                    this.sb.Append("FROM ");
                    this.sb.Append(str);
                }
                if (ss.Where != null)
                {
                    this.NewLine();
                    this.sb.Append("WHERE ");
                    this.Visit((SqlNode)ss.Where);
                }
                if (ss.GroupBy.Count > 0)
                {
                    this.NewLine();
                    this.sb.Append("GROUP BY ");
                    int index = 0;
                    for (int count = ss.GroupBy.Count; index < count; ++index)
                    {
                        SqlExpression sqlExpression = ss.GroupBy[index];
                        if (index > 0)
                            this.sb.Append(", ");
                        this.Visit((SqlNode)sqlExpression);
                    }
                    if (ss.Having != null)
                    {
                        this.NewLine();
                        this.sb.Append("HAVING ");
                        this.Visit((SqlNode)ss.Having);
                    }
                }
                if (ss.OrderBy.Count > 0 && ss.OrderingType != SqlOrderingType.Never)
                {
                    this.NewLine();
                    this.sb.Append("ORDER BY ");
                    int index = 0;
                    for (int count = ss.OrderBy.Count; index < count; ++index)
                    {
                        SqlOrderExpression sqlOrderExpression = ss.OrderBy[index];
                        if (index > 0)
                            this.sb.Append(", ");
                        this.Visit((SqlNode)sqlOrderExpression.Expression);
                        if (sqlOrderExpression.OrderType == SqlOrderType.Descending)
                            this.sb.Append(" DESC");
                    }
                }
                return ss;
            }

            internal virtual bool IsSimpleCrossJoinList(SqlNode node)
            {
                SqlJoin sqlJoin = node as SqlJoin;
                if (sqlJoin != null)
                {
                    if (sqlJoin.JoinType == SqlJoinType.Cross && this.IsSimpleCrossJoinList((SqlNode)sqlJoin.Left))
                        return this.IsSimpleCrossJoinList((SqlNode)sqlJoin.Right);
                    return false;
                }
                SqlAlias sqlAlias = node as SqlAlias;
                if (sqlAlias != null)
                    return sqlAlias.Node is SqlTable;
                return false;
            }

            internal virtual void VisitCrossJoinList(SqlNode node)
            {
                SqlJoin sqlJoin = node as SqlJoin;
                if (sqlJoin != null)
                {
                    this.VisitCrossJoinList((SqlNode)sqlJoin.Left);
                    this.sb.Append(", ");
                    this.VisitCrossJoinList((SqlNode)sqlJoin.Right);
                }
                else
                    this.Visit(node);
            }

            internal void VisitJoinSource(SqlSource src)
            {
                if (src.NodeType == SqlNodeType.Join)
                {
                    this.depth = this.depth + 1;
                    this.sb.Append("(");
                    this.Visit((SqlNode)src);
                    this.sb.Append(")");
                    this.depth = this.depth - 1;
                }
                else
                    this.Visit((SqlNode)src);
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                this.Visit((SqlNode)join.Left);
                this.NewLine();
                switch (join.JoinType)
                {
                    case SqlJoinType.Cross:
                        this.sb.Append("CROSS JOIN ");
                        break;
                    case SqlJoinType.Inner:
                        this.sb.Append("INNER JOIN ");
                        break;
                    case SqlJoinType.LeftOuter:
                        this.sb.Append("LEFT OUTER JOIN ");
                        break;
                    case SqlJoinType.CrossApply:
                        this.sb.Append("CROSS APPLY ");
                        break;
                    case SqlJoinType.OuterApply:
                        this.sb.Append("OUTER APPLY ");
                        break;
                }
                SqlJoin sqlJoin = join.Right as SqlJoin;
                if (sqlJoin == null || sqlJoin.JoinType == SqlJoinType.Cross && join.JoinType != SqlJoinType.CrossApply && join.JoinType != SqlJoinType.OuterApply)
                    this.Visit((SqlNode)join.Right);
                else
                    this.VisitJoinSource(join.Right);
                if (join.Condition != null)
                {
                    this.sb.Append(" ON ");
                    this.Visit((SqlNode)join.Condition);
                }
                else if (this.RequiresOnCondition(join.JoinType))
                    this.sb.Append(" ON 1=1 ");
                return (SqlSource)join;
            }

            internal bool RequiresOnCondition(SqlJoinType joinType)
            {
                switch (joinType)
                {
                    case SqlJoinType.Cross:
                    case SqlJoinType.CrossApply:
                    case SqlJoinType.OuterApply:
                        return false;
                    case SqlJoinType.Inner:
                    case SqlJoinType.LeftOuter:
                        return true;
                    default:
                        throw Error.InvalidFormatNode((object)joinType);
                }
            }

            internal override SqlBlock VisitBlock(SqlBlock block)
            {
                int index = 0;
                for (int count = block.Statements.Count; index < count; ++index)
                {
                    this.Visit((SqlNode)block.Statements[index]);
                    if (index < count - 1)
                    {
                        SqlSelect sqlSelect = block.Statements[index + 1] as SqlSelect;
                        if (sqlSelect == null || !sqlSelect.DoNotOutput)
                        {
                            this.NewLine();
                            this.NewLine();
                        }
                    }
                }
                return block;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"ClientQuery");
                this.sb.Append("client(");
                int index = 0;
                for (int count = cq.Arguments.Count; index < count; ++index)
                {
                    if (index > 0)
                        this.sb.Append(", ");
                    this.Visit((SqlNode)cq.Arguments[index]);
                }
                this.sb.Append("; ");
                this.Visit((SqlNode)cq.Query);
                this.sb.Append(")");
                return (SqlExpression)cq;
            }

            internal override SqlExpression VisitJoinedCollection(SqlJoinedCollection jc)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"JoinedCollection");
                this.sb.Append("big-join(");
                this.Visit((SqlNode)jc.Expression);
                this.sb.Append(", ");
                this.Visit((SqlNode)jc.Count);
                this.sb.Append(")");
                return (SqlExpression)jc;
            }

            internal override SqlStatement VisitDelete(SqlDelete sd)
            {
                this.sb.Append("DELETE FROM ");
                this.suppressedAliases.Add(sd.Select.From);
                this.Visit((SqlNode)sd.Select.From);
                if (sd.Select.Where != null)
                {
                    this.sb.Append(" WHERE ");
                    this.Visit((SqlNode)sd.Select.Where);
                }
                this.suppressedAliases.Remove(sd.Select.From);
                return (SqlStatement)sd;
            }

            internal override SqlStatement VisitInsert(SqlInsert si)
            {
                if (si.OutputKey != null)
                {
                    this.sb.Append("DECLARE @output TABLE(");
                    this.WriteName(si.OutputKey.Name);
                    this.sb.Append(" ");
                    this.sb.Append(si.OutputKey.SqlType.ToQueryString());
                    this.sb.Append(")");
                    this.NewLine();
                    if (si.OutputToLocal)
                    {
                        this.sb.Append("DECLARE @id ");
                        this.sb.Append(si.OutputKey.SqlType.ToQueryString());
                        this.NewLine();
                    }
                }
                this.sb.Append("INSERT INTO ");
                this.Visit((SqlNode)si.Table);
                if (si.Row.Columns.Count != 0)
                {
                    this.sb.Append("(");
                    int index = 0;
                    for (int count = si.Row.Columns.Count; index < count; ++index)
                    {
                        if (index > 0)
                            this.sb.Append(", ");
                        this.WriteName(si.Row.Columns[index].Name);
                    }
                    this.sb.Append(")");
                }
                if (si.OutputKey != null)
                {
                    this.NewLine();
                    this.sb.Append("OUTPUT INSERTED.");
                    this.WriteName(si.OutputKey.MetaMember.MappedName);
                    this.sb.Append(" INTO @output");
                }
                if (si.Row.Columns.Count == 0)
                {
                    this.sb.Append(" DEFAULT VALUES");
                }
                else
                {
                    this.NewLine();
                    this.sb.Append("VALUES (");
                    if (this.isDebugMode && si.Row.Columns.Count == 0)
                    {
                        this.Visit((SqlNode)si.Expression);
                    }
                    else
                    {
                        int index = 0;
                        for (int count = si.Row.Columns.Count; index < count; ++index)
                        {
                            if (index > 0)
                                this.sb.Append(", ");
                            this.Visit((SqlNode)si.Row.Columns[index].Expression);
                        }
                    }
                    this.sb.Append(")");
                }
                if (si.OutputKey != null)
                {
                    this.NewLine();
                    if (si.OutputToLocal)
                    {
                        this.sb.Append("SELECT @id = ");
                        this.sb.Append(si.OutputKey.Name);
                        this.sb.Append(" FROM @output");
                    }
                    else
                    {
                        this.sb.Append("SELECT ");
                        this.WriteName(si.OutputKey.Name);
                        this.sb.Append(" FROM @output");
                    }
                }
                return (SqlStatement)si;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate su)
            {
                this.sb.Append("UPDATE ");
                this.suppressedAliases.Add(su.Select.From);
                this.Visit((SqlNode)su.Select.From);
                this.NewLine();
                this.sb.Append("SET ");
                int index = 0;
                for (int count = su.Assignments.Count; index < count; ++index)
                {
                    if (index > 0)
                        this.sb.Append(", ");
                    SqlAssign sqlAssign = su.Assignments[index];
                    this.Visit((SqlNode)sqlAssign.LValue);
                    this.sb.Append(" = ");
                    this.Visit((SqlNode)sqlAssign.RValue);
                }
                if (su.Select.Where != null)
                {
                    this.NewLine();
                    this.sb.Append("WHERE ");
                    this.Visit((SqlNode)su.Select.Where);
                }
                this.suppressedAliases.Remove(su.Select.From);
                return (SqlStatement)su;
            }

            internal override SqlStatement VisitAssign(SqlAssign sa)
            {
                this.sb.Append("SET ");
                this.Visit((SqlNode)sa.LValue);
                this.sb.Append(" = ");
                this.Visit((SqlNode)sa.RValue);
                return (SqlStatement)sa;
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
            {
                this.depth = this.depth + 1;
                this.NewLine();
                this.sb.Append("(CASE ");
                this.depth = this.depth + 1;
                int index = 0;
                for (int count = c.Whens.Count; index < count; ++index)
                {
                    SqlWhen sqlWhen = c.Whens[index];
                    this.NewLine();
                    this.sb.Append("WHEN ");
                    this.Visit((SqlNode)sqlWhen.Match);
                    this.sb.Append(" THEN ");
                    this.Visit((SqlNode)sqlWhen.Value);
                }
                if (c.Else != null)
                {
                    this.NewLine();
                    this.sb.Append("ELSE ");
                    this.Visit((SqlNode)c.Else);
                }
                this.depth = this.depth - 1;
                this.NewLine();
                this.sb.Append(" END)");
                this.depth = this.depth - 1;
                return (SqlExpression)c;
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                this.depth = this.depth + 1;
                this.NewLine();
                this.sb.Append("(CASE");
                this.depth = this.depth + 1;
                if (c.Expression != null)
                {
                    this.sb.Append(" ");
                    this.Visit((SqlNode)c.Expression);
                }
                int index = 0;
                for (int count = c.Whens.Count; index < count; ++index)
                {
                    SqlWhen sqlWhen = c.Whens[index];
                    if (index == count - 1 && sqlWhen.Match == null)
                    {
                        this.NewLine();
                        this.sb.Append("ELSE ");
                        this.Visit((SqlNode)sqlWhen.Value);
                    }
                    else
                    {
                        this.NewLine();
                        this.sb.Append("WHEN ");
                        this.Visit((SqlNode)sqlWhen.Match);
                        this.sb.Append(" THEN ");
                        this.Visit((SqlNode)sqlWhen.Value);
                    }
                }
                this.depth = this.depth - 1;
                this.NewLine();
                this.sb.Append(" END)");
                this.depth = this.depth - 1;
                return (SqlExpression)c;
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"ClientCase");
                this.depth = this.depth + 1;
                this.NewLine();
                this.sb.Append("(CASE");
                this.depth = this.depth + 1;
                if (c.Expression != null)
                {
                    this.sb.Append(" ");
                    this.Visit((SqlNode)c.Expression);
                }
                int index = 0;
                for (int count = c.Whens.Count; index < count; ++index)
                {
                    SqlClientWhen sqlClientWhen = c.Whens[index];
                    if (index == count - 1 && sqlClientWhen.Match == null)
                    {
                        this.NewLine();
                        this.sb.Append("ELSE ");
                        this.Visit((SqlNode)sqlClientWhen.Value);
                    }
                    else
                    {
                        this.NewLine();
                        this.sb.Append("WHEN ");
                        this.Visit((SqlNode)sqlClientWhen.Match);
                        this.sb.Append(" THEN ");
                        this.Visit((SqlNode)sqlClientWhen.Value);
                    }
                }
                this.depth = this.depth - 1;
                this.NewLine();
                this.sb.Append(" END)");
                this.depth = this.depth - 1;
                return (SqlExpression)c;
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase c)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"TypeCase");
                this.depth = this.depth + 1;
                this.NewLine();
                this.sb.Append("(CASE");
                this.depth = this.depth + 1;
                if (c.Discriminator != null)
                {
                    this.sb.Append(" ");
                    this.Visit((SqlNode)c.Discriminator);
                }
                int index = 0;
                for (int count = c.Whens.Count; index < count; ++index)
                {
                    SqlTypeCaseWhen sqlTypeCaseWhen = c.Whens[index];
                    if (index == count - 1 && sqlTypeCaseWhen.Match == null)
                    {
                        this.NewLine();
                        this.sb.Append("ELSE ");
                        this.Visit((SqlNode)sqlTypeCaseWhen.TypeBinding);
                    }
                    else
                    {
                        this.NewLine();
                        this.sb.Append("WHEN ");
                        this.Visit((SqlNode)sqlTypeCaseWhen.Match);
                        this.sb.Append(" THEN ");
                        this.Visit((SqlNode)sqlTypeCaseWhen.TypeBinding);
                    }
                }
                this.depth = this.depth - 1;
                this.NewLine();
                this.sb.Append(" END)");
                this.depth = this.depth - 1;
                return (SqlExpression)c;
            }

            internal override SqlExpression VisitVariable(SqlVariable v)
            {
                this.sb.Append(v.Name);
                return (SqlExpression)v;
            }

            private string InferName(SqlExpression exp, string def)
            {
                if (exp == null)
                    return (string)null;
                switch (exp.NodeType)
                {
                    case SqlNodeType.ExprSet:
                        return this.InferName(((SqlExprSet)exp).Expressions[0], def);
                    case SqlNodeType.Member:
                        return ((SqlMember)exp).Member.Name;
                    case SqlNodeType.Column:
                        return ((SqlColumn)exp).Name;
                    case SqlNodeType.ColumnRef:
                        return ((SqlColumnRef)exp).Column.Name;
                    default:
                        return def;
                }
            }

            private void FormatType(ProviderType type)
            {
                this.sb.Append(type.ToQueryString());
            }

            internal virtual void FormatValue(object value)
            {
                if (value == null)
                {
                    this.sb.Append("NULL");
                }
                else
                {
                    Type type1 = value.GetType();
                    if (type1.IsGenericType && type1.GetGenericTypeDefinition() == typeof(Nullable<>))
                        type1 = type1.GetGenericArguments()[0];
                    switch (Type.GetTypeCode(type1))
                    {
                        case TypeCode.Object:
                            if (value is Guid)
                            {
                                this.sb.Append("'");
                                this.sb.Append(value);
                                this.sb.Append("'");
                                return;
                            }
                            Type type2 = value as Type;
                            if (type2 != (Type)null)
                            {
                                if (this.isDebugMode)
                                {
                                    this.sb.Append("typeof(");
                                    this.sb.Append(type2.Name);
                                    this.sb.Append(")");
                                    return;
                                }
                                this.FormatValue((object)"");
                                return;
                            }
                            break;
                        case TypeCode.Boolean:
                            this.sb.Append(this.GetBoolValue((bool)value));
                            return;
                        case TypeCode.Char:
                        case TypeCode.DateTime:
                        case TypeCode.String:
                            this.sb.Append("'");
                            this.sb.Append(this.EscapeSingleQuotes(value.ToString()));
                            this.sb.Append("'");
                            return;
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            this.sb.Append(value);
                            return;
                    }
                    if (!this.isDebugMode)
                        throw Error.ValueHasNoLiteralInSql(value);
                    this.sb.Append("value(");
                    this.sb.Append(value.ToString());
                    this.sb.Append(")");
                }
            }

            internal virtual string GetBoolValue(bool value)
            {
                return !value ? "0" : "1";
            }

            internal virtual string EscapeSingleQuotes(string str)
            {
                if (str.IndexOf('\'') < 0)
                    return str;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (int num in str)
                {
                    if (num == 39)
                        stringBuilder.Append("''");
                    else
                        stringBuilder.Append("'");
                }
                return stringBuilder.ToString();
            }

            internal virtual string GetOperator(SqlNodeType nt)
            {
                switch (nt)
                {
                    case SqlNodeType.EQ:
                        return "=";
                    case SqlNodeType.EQ2V:
                        return "=";
                    case SqlNodeType.IsNotNull:
                        return "IS NOT NULL";
                    case SqlNodeType.IsNull:
                        return "IS NULL";
                    case SqlNodeType.LE:
                        return "<=";
                    case SqlNodeType.LongCount:
                        return "COUNT_BIG";
                    case SqlNodeType.LT:
                        return "<";
                    case SqlNodeType.GE:
                        return ">=";
                    case SqlNodeType.GT:
                        return ">";
                    case SqlNodeType.Max:
                        return "MAX";
                    case SqlNodeType.Min:
                        return "MIN";
                    case SqlNodeType.Mod:
                        return "%";
                    case SqlNodeType.Mul:
                        return "*";
                    case SqlNodeType.NE:
                        return "<>";
                    case SqlNodeType.NE2V:
                        return "<>";
                    case SqlNodeType.Negate:
                        return "-";
                    case SqlNodeType.Not:
                        return "NOT";
                    case SqlNodeType.Not2V:
                        return "NOT";
                    case SqlNodeType.Or:
                        return "OR";
                    case SqlNodeType.Stddev:
                        return "STDEV";
                    case SqlNodeType.Sub:
                        return "-";
                    case SqlNodeType.Sum:
                        return "SUM";
                    case SqlNodeType.Count:
                        return "COUNT";
                    case SqlNodeType.Div:
                        return "/";
                    case SqlNodeType.Add:
                        return "+";
                    case SqlNodeType.And:
                        return "AND";
                    case SqlNodeType.Avg:
                        return "AVG";
                    case SqlNodeType.BitAnd:
                        return "&";
                    case SqlNodeType.BitNot:
                        return "~";
                    case SqlNodeType.BitOr:
                        return "|";
                    case SqlNodeType.BitXor:
                        return "^";
                    case SqlNodeType.ClrLength:
                        return "CLRLENGTH";
                    case SqlNodeType.Concat:
                        return "+";
                    default:
                        throw Error.InvalidFormatNode((object)nt);
                }
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"Link");
                if (link.Expansion != null)
                {
                    this.sb.Append("LINK(");
                    this.Visit((SqlNode)link.Expansion);
                    this.sb.Append(")");
                }
                else
                {
                    this.sb.Append("LINK(");
                    int index = 0;
                    for (int count = link.KeyExpressions.Count; index < count; ++index)
                    {
                        if (index > 0)
                            this.sb.Append(", ");
                        this.Visit((SqlNode)link.KeyExpressions[index]);
                    }
                    this.sb.Append(")");
                }
                return (SqlNode)link;
            }

            internal override SqlMemberAssign VisitMemberAssign(SqlMemberAssign ma)
            {
                throw Error.InvalidFormatNode((object)"MemberAssign");
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"MethodCall");
                if (mc.Method.IsStatic)
                    this.sb.Append((object)mc.Method.DeclaringType);
                else
                    this.Visit((SqlNode)mc.Object);
                this.sb.Append(".");
                this.sb.Append(mc.Method.Name);
                this.sb.Append("(");
                int index = 0;
                for (int count = mc.Arguments.Count; index < count; ++index)
                {
                    if (index > 0)
                        this.sb.Append(", ");
                    this.Visit((SqlNode)mc.Arguments[index]);
                }
                this.sb.Append(")");
                return (SqlExpression)mc;
            }

            internal override SqlExpression VisitOptionalValue(SqlOptionalValue sov)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"OptionalValue");
                this.sb.Append("opt(");
                this.Visit((SqlNode)sov.HasValue);
                this.sb.Append(", ");
                this.Visit((SqlNode)sov.Value);
                this.sb.Append(")");
                return (SqlExpression)sov;
            }

            internal override SqlExpression VisitUserRow(SqlUserRow row)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"UserRow");
                return (SqlExpression)row;
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g)
            {
                if (!this.isDebugMode)
                    throw Error.InvalidFormatNode((object)"Grouping");
                this.sb.Append("Group(");
                this.Visit((SqlNode)g.Key);
                this.sb.Append(", ");
                this.Visit((SqlNode)g.Group);
                this.sb.Append(")");
                return (SqlExpression)g;
            }
        }

        private class AliasMapper : SqlVisitor
        {
            private Dictionary<SqlColumn, SqlAlias> aliasMap;
            private SqlAlias currentAlias;

            internal AliasMapper(Dictionary<SqlColumn, SqlAlias> aliasMap)
            {
                this.aliasMap = aliasMap;
            }

            internal override SqlAlias VisitAlias(SqlAlias a)
            {
                SqlAlias sqlAlias = this.currentAlias;
                this.currentAlias = a;
                base.VisitAlias(a);
                this.currentAlias = sqlAlias;
                return a;
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                this.aliasMap[col] = this.currentAlias;
                this.Visit((SqlNode)col.Expression);
                return (SqlExpression)col;
            }

            internal override SqlRow VisitRow(SqlRow row)
            {
                foreach (SqlColumn col in row.Columns)
                    this.VisitColumn(col);
                return row;
            }

            internal override SqlTable VisitTable(SqlTable tab)
            {
                foreach (SqlColumn col in tab.Columns)
                    this.VisitColumn(col);
                return tab;
            }

            internal override SqlExpression VisitTableValuedFunctionCall(SqlTableValuedFunctionCall fc)
            {
                foreach (SqlColumn col in fc.Columns)
                    this.VisitColumn(col);
                return (SqlExpression)fc;
            }
        }
    }
}
