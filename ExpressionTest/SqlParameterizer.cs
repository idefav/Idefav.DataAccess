using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlParameterizer
    {
        private TypeSystemProvider typeProvider;
        private SqlNodeAnnotations annotations;
        private int index;

        internal SqlParameterizer(TypeSystemProvider typeProvider, SqlNodeAnnotations annotations)
        {
            this.typeProvider = typeProvider;
            this.annotations = annotations;
        }

        internal ReadOnlyCollection<SqlParameterInfo> Parameterize(SqlNode node)
        {
            return this.ParameterizeInternal(node).AsReadOnly();
        }

        private List<SqlParameterInfo> ParameterizeInternal(SqlNode node)
        {
            SqlParameterizer.Visitor visitor = new SqlParameterizer.Visitor(this);
            SqlNode node1 = node;
            visitor.Visit(node1);
            return new List<SqlParameterInfo>((IEnumerable<SqlParameterInfo>)visitor.currentParams);
        }

        internal ReadOnlyCollection<ReadOnlyCollection<SqlParameterInfo>> ParameterizeBlock(SqlBlock block)
        {
            SqlParameterInfo sqlParameterInfo = new SqlParameterInfo(new SqlParameter(typeof(int), this.typeProvider.From(typeof(int)), "@ROWCOUNT", block.SourceExpression));
            List<ReadOnlyCollection<SqlParameterInfo>> list1 = new List<ReadOnlyCollection<SqlParameterInfo>>();
            int index = 0;
            for (int count = block.Statements.Count; index < count; ++index)
            {
                List<SqlParameterInfo> list2 = this.ParameterizeInternal((SqlNode)block.Statements[index]);
                if (index > 0)
                    list2.Add(sqlParameterInfo);
                list1.Add(list2.AsReadOnly());
            }
            return list1.AsReadOnly();
        }

        internal virtual string CreateParameterName()
        {
            string str = "@p";
            int num = this.index;
            this.index = num + 1;
            // ISSUE: variable of a boxed type
            var local = (System.ValueType)num;
            return str + (object)local;
        }

        private class Visitor : SqlVisitor
        {
            private SqlParameterizer parameterizer;
            internal Dictionary<object, SqlParameterInfo> map;
            internal List<SqlParameterInfo> currentParams;
            private bool topLevel;
            private ProviderType timeProviderType;

            internal Visitor(SqlParameterizer parameterizer)
            {
                this.parameterizer = parameterizer;
                this.topLevel = true;
                this.map = new Dictionary<object, SqlParameterInfo>();
                this.currentParams = new List<SqlParameterInfo>();
            }

            private SqlParameter InsertLookup(SqlValue cp)
            {
                SqlParameterInfo sqlParameterInfo = (SqlParameterInfo)null;
                if (!this.map.TryGetValue((object)cp, out sqlParameterInfo))
                {
                    sqlParameterInfo = !(this.timeProviderType == (ProviderType)null) ? new SqlParameterInfo(new SqlParameter(cp.ClrType, this.timeProviderType, this.parameterizer.CreateParameterName(), cp.SourceExpression), (object)((DateTime)cp.Value).TimeOfDay) : new SqlParameterInfo(new SqlParameter(cp.ClrType, cp.SqlType, this.parameterizer.CreateParameterName(), cp.SourceExpression), cp.Value);
                    this.map.Add((object)cp, sqlParameterInfo);
                    this.currentParams.Add(sqlParameterInfo);
                }
                return sqlParameterInfo.Parameter;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                switch (bo.NodeType)
                {
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        SqlDbType sqlDbType1 = ((SqlTypeSystem.SqlType)bo.Left.SqlType).SqlDbType;
                        SqlDbType sqlDbType2 = ((SqlTypeSystem.SqlType)bo.Right.SqlType).SqlDbType;
                        if (sqlDbType1 != sqlDbType2)
                        {
                            bool flag1 = bo.Left is SqlColumnRef;
                            bool flag2 = bo.Right is SqlColumnRef;
                            if (flag1 != flag2)
                            {
                                if (flag1 && sqlDbType1 == SqlDbType.Time && bo.Right.ClrType == typeof(DateTime))
                                {
                                    this.timeProviderType = bo.Left.SqlType;
                                    break;
                                }
                                if (flag2 && sqlDbType2 == SqlDbType.Time && bo.Left.ClrType == typeof(DateTime))
                                {
                                    this.timeProviderType = bo.Left.SqlType;
                                    break;
                                }
                                break;
                            }
                            break;
                        }
                        break;
                }
                base.VisitBinaryOperator(bo);
                return (SqlExpression)bo;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                bool flag = this.topLevel;
                this.topLevel = false;
                select = this.VisitSelectCore(select);
                this.topLevel = flag;
                select.Selection = this.VisitExpression(select.Selection);
                return select;
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq)
            {
                bool flag = this.topLevel;
                this.topLevel = false;
                int index = 0;
                for (int count = suq.Arguments.Count; index < count; ++index)
                    suq.Arguments[index] = this.VisitParameter(suq.Arguments[index]);
                this.topLevel = flag;
                suq.Projection = this.VisitExpression(suq.Projection);
                return suq;
            }

            internal SqlExpression VisitParameter(SqlExpression expr)
            {
                SqlExpression sqlExpression = this.VisitExpression(expr);
                switch (sqlExpression.NodeType)
                {
                    case SqlNodeType.Parameter:
                        return sqlExpression;
                    case SqlNodeType.Value:
                        return (SqlExpression)this.InsertLookup((SqlValue)sqlExpression);
                    default:
                        return sqlExpression;
                }
            }

            internal override SqlStoredProcedureCall VisitStoredProcedureCall(SqlStoredProcedureCall spc)
            {
                this.VisitUserQuery((SqlUserQuery)spc);
                int index = 0;
                for (int count = spc.Function.Parameters.Count; index < count; ++index)
                {
                    MetaParameter p = spc.Function.Parameters[index];
                    SqlParameter node = spc.Arguments[index] as SqlParameter;
                    if (node != null)
                    {
                        node.Direction = this.GetParameterDirection(p);
                        if (node.Direction == ParameterDirection.InputOutput || node.Direction == ParameterDirection.Output)
                            this.RetypeOutParameter(node);
                    }
                }
                this.currentParams.Add(new SqlParameterInfo(new SqlParameter(typeof(int?), this.parameterizer.typeProvider.From(typeof(int)), "@RETURN_VALUE", spc.SourceExpression)
                {
                    Direction = ParameterDirection.Output
                }));
                return spc;
            }

            private bool RetypeOutParameter(SqlParameter node)
            {
                if (!node.SqlType.IsLargeType)
                    return false;
                ProviderType bestLargeType = this.parameterizer.typeProvider.GetBestLargeType(node.SqlType);
                if (node.SqlType != bestLargeType)
                {
                    node.SetSqlType(bestLargeType);
                    return true;
                }
                SqlNodeAnnotations sqlNodeAnnotations = this.parameterizer.annotations;
                SqlParameter sqlParameter = node;
                string message = Strings.MaxSizeNotSupported((object)node.SourceExpression);
                SqlProvider.ProviderMode[] providerModeArray = new SqlProvider.ProviderMode[1];
                int index = 0;
                int num = 1;
                providerModeArray[index] = (SqlProvider.ProviderMode)num;
                SqlServerCompatibilityAnnotation compatibilityAnnotation = new SqlServerCompatibilityAnnotation(message, providerModeArray);
                sqlNodeAnnotations.Add((SqlNode)sqlParameter, (SqlNodeAnnotation)compatibilityAnnotation);
                return false;
            }

            private ParameterDirection GetParameterDirection(MetaParameter p)
            {
                if (p.Parameter.IsRetval)
                    return ParameterDirection.ReturnValue;
                if (p.Parameter.IsOut)
                    return ParameterDirection.Output;
                return p.Parameter.ParameterType.IsByRef ? ParameterDirection.InputOutput : ParameterDirection.Input;
            }

            internal override SqlStatement VisitInsert(SqlInsert sin)
            {
                bool flag = this.topLevel;
                this.topLevel = false;
                base.VisitInsert(sin);
                this.topLevel = flag;
                return (SqlStatement)sin;
            }

            internal override SqlStatement VisitUpdate(SqlUpdate sup)
            {
                bool flag = this.topLevel;
                this.topLevel = false;
                base.VisitUpdate(sup);
                this.topLevel = flag;
                return (SqlStatement)sup;
            }

            internal override SqlStatement VisitDelete(SqlDelete sd)
            {
                bool flag = this.topLevel;
                this.topLevel = false;
                base.VisitDelete(sd);
                this.topLevel = flag;
                return (SqlStatement)sd;
            }

            internal override SqlExpression VisitValue(SqlValue value)
            {
                if (this.topLevel || !value.IsClientSpecified || !value.SqlType.CanBeParameter)
                    return (SqlExpression)value;
                return (SqlExpression)this.InsertLookup(value);
            }

            internal override SqlExpression VisitClientParameter(SqlClientParameter cp)
            {
                if (!cp.SqlType.CanBeParameter)
                    return (SqlExpression)cp;
                SqlParameter parameter = new SqlParameter(cp.ClrType, cp.SqlType, this.parameterizer.CreateParameterName(), cp.SourceExpression);
                this.currentParams.Add(new SqlParameterInfo(parameter, cp.Accessor.Compile()));
                return (SqlExpression)parameter;
            }
        }
    }
}
