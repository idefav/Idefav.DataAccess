using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class PreBindDotNetConverter
    {
        internal static SqlNode Convert(SqlNode node, SqlFactory sql, MetaModel model)
        {
            return new PreBindDotNetConverter.Visitor(sql, model).Visit(node);
        }

        internal static bool CanConvert(SqlNode node)
        {
            SqlBinary bo = node as SqlBinary;
            if (bo != null && (PreBindDotNetConverter.IsCompareToValue(bo) || PreBindDotNetConverter.IsVbCompareStringEqualsValue(bo)))
                return true;
            SqlMember m = node as SqlMember;
            if (m != null && PreBindDotNetConverter.IsSupportedMember(m))
                return true;
            SqlMethodCall mc = node as SqlMethodCall;
            return mc != null && (PreBindDotNetConverter.IsSupportedMethod(mc) || PreBindDotNetConverter.IsSupportedVbHelperMethod(mc));
        }

        private static bool IsCompareToValue(SqlBinary bo)
        {
            if (!PreBindDotNetConverter.IsComparison(bo.NodeType) || bo.Left.NodeType != SqlNodeType.MethodCall || bo.Right.NodeType != SqlNodeType.Value)
                return false;
            SqlMethodCall call = (SqlMethodCall)bo.Left;
            if (!PreBindDotNetConverter.IsCompareToMethod(call))
                return PreBindDotNetConverter.IsCompareMethod(call);
            return true;
        }

        private static bool IsCompareToMethod(SqlMethodCall call)
        {
            if (!call.Method.IsStatic && call.Method.Name == "CompareTo" && call.Arguments.Count == 1)
                return call.Method.ReturnType == typeof(int);
            return false;
        }

        private static bool IsCompareMethod(SqlMethodCall call)
        {
            if (call.Method.IsStatic && call.Method.Name == "Compare" && call.Arguments.Count > 1)
                return call.Method.ReturnType == typeof(int);
            return false;
        }

        private static bool IsComparison(SqlNodeType nodeType)
        {
            switch (nodeType)
            {
                case SqlNodeType.LT:
                case SqlNodeType.GE:
                case SqlNodeType.GT:
                case SqlNodeType.NE:
                case SqlNodeType.NE2V:
                case SqlNodeType.EQ:
                case SqlNodeType.EQ2V:
                case SqlNodeType.LE:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsVbCompareStringEqualsValue(SqlBinary bo)
        {
            if (PreBindDotNetConverter.IsComparison(bo.NodeType) && bo.Left.NodeType == SqlNodeType.MethodCall && bo.Right.NodeType == SqlNodeType.Value)
                return PreBindDotNetConverter.IsVbCompareString((SqlMethodCall)bo.Left);
            return false;
        }

        private static bool IsVbCompareString(SqlMethodCall call)
        {
            if (call.Method.IsStatic && call.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators")
                return call.Method.Name == "CompareString";
            return false;
        }

        private static bool IsSupportedVbHelperMethod(SqlMethodCall mc)
        {
            return PreBindDotNetConverter.IsVbIIF(mc);
        }

        private static bool IsVbIIF(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic && mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.Interaction")
                return mc.Method.Name == "IIf";
            return false;
        }

        private static bool IsSupportedMember(SqlMember m)
        {
            if (!PreBindDotNetConverter.IsNullableHasValue(m))
                return PreBindDotNetConverter.IsNullableHasValue(m);
            return true;
        }

        private static bool IsNullableValue(SqlMember m)
        {
            if (TypeSystem.IsNullableType(m.Expression.ClrType))
                return m.Member.Name == "Value";
            return false;
        }

        private static bool IsNullableHasValue(SqlMember m)
        {
            if (TypeSystem.IsNullableType(m.Expression.ClrType))
                return m.Member.Name == "HasValue";
            return false;
        }

        private static bool IsSupportedMethod(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic)
            {
                string name = mc.Method.Name;
                // ISSUE: reference to a compiler-generated method
                uint stringHash = uint.Parse(PrivateImplementationDetails.ComputeStringHash(name));
                if (stringHash <= 1516143579U)
                {
                    if (stringHash <= 1046310813U)
                    {
                        if (stringHash <= 835846267U)
                        {
                            if ((int)stringHash != 90588446)
                            {
                                if ((int)stringHash != 835846267 || !(name == "op_BitwiseAnd"))
                                    goto label_46;
                            }
                            else if (!(name == "op_OnesComplement"))
                                goto label_46;
                        }
                        else if ((int)stringHash != 906583475)
                        {
                            if ((int)stringHash == 1046310813 && name == "Concat")
                                return mc.Method.DeclaringType == typeof(string);
                            goto label_46;
                        }
                        else if (!(name == "op_Addition"))
                            goto label_46;
                    }
                    else if (stringHash <= 1195761148U)
                    {
                        if ((int)stringHash != 1050238388)
                        {
                            if ((int)stringHash != 1195761148 || !(name == "op_GreaterThan"))
                                goto label_46;
                        }
                        else
                        {
                            if (name == "Equals")
                                return mc.Arguments.Count == 2;
                            goto label_46;
                        }
                    }
                    else if ((int)stringHash != 1234170120)
                    {
                        if ((int)stringHash != 1258540185)
                        {
                            if ((int)stringHash != 1516143579 || !(name == "op_Equality"))
                                goto label_46;
                        }
                        else if (!(name == "op_LessThan"))
                            goto label_46;
                    }
                    else if (!(name == "op_LessThanOrEqual"))
                        goto label_46;
                }
                else if (stringHash <= 2459852411U)
                {
                    if (stringHash <= 1915672496U)
                    {
                        if ((int)stringHash != 1850069070)
                        {
                            if ((int)stringHash != 1915672496 || !(name == "op_Division"))
                                goto label_46;
                        }
                        else if (!(name == "op_False"))
                            goto label_46;
                    }
                    else if ((int)stringHash != -1928171460)
                    {
                        if ((int)stringHash != -1865288344)
                        {
                            if ((int)stringHash != -1835114885 || !(name == "op_GreaterThanOrEqual"))
                                goto label_46;
                        }
                        else if (!(name == "op_Modulus"))
                            goto label_46;
                    }
                    else if (!(name == "op_ExclusiveOr"))
                        goto label_46;
                }
                else if (stringHash <= 3279419199U)
                {
                    if ((int)stringHash != -1336714801)
                    {
                        if ((int)stringHash != -1015548097 || !(name == "op_Subtraction"))
                            goto label_46;
                    }
                    else if (!(name == "op_Multiply"))
                        goto label_46;
                }
                else if ((int)stringHash != -802416729)
                {
                    if ((int)stringHash != -578301403)
                    {
                        if ((int)stringHash != -500649512 || !(name == "op_Inequality"))
                            goto label_46;
                    }
                    else if (!(name == "op_UnaryNegation"))
                        goto label_46;
                }
                else if (!(name == "op_BitwiseOr"))
                    goto label_46;
                return true;
                label_46:
                return false;
            }
            if (mc.Method.Name == "Equals" && mc.Arguments.Count == 1)
                return true;
            if (mc.Method.Name == "GetType")
                return mc.Arguments.Count == 0;
            return false;
        }

        private class Visitor : SqlVisitor
        {
            private SqlFactory sql;
            private MetaModel model;

            internal Visitor(SqlFactory sql, MetaModel model)
            {
                this.sql = sql;
                this.model = model;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                if (PreBindDotNetConverter.IsCompareToValue(bo))
                {
                    SqlMethodCall call = (SqlMethodCall)bo.Left;
                    if (PreBindDotNetConverter.IsCompareToMethod(call))
                    {
                       
                        int iValue = System.Convert.ToInt32(this.Eval(bo.Right), CultureInfo.InvariantCulture);
                        bo = this.MakeCompareTo(call.Object, call.Arguments[0], bo.NodeType, iValue) ?? bo;
                    }
                    else if (PreBindDotNetConverter.IsCompareMethod(call))
                    {
                        int iValue = System.Convert.ToInt32(this.Eval(bo.Right), CultureInfo.InvariantCulture);
                        bo = this.MakeCompareTo(call.Arguments[0], call.Arguments[1], bo.NodeType, iValue) ?? bo;
                    }
                }
                else if (PreBindDotNetConverter.IsVbCompareStringEqualsValue(bo))
                {
                    SqlMethodCall sqlMethodCall = (SqlMethodCall)bo.Left;
                    int iValue = System.Convert.ToInt32(this.Eval(bo.Right), (IFormatProvider)CultureInfo.InvariantCulture);
                    SqlValue sqlValue1 = sqlMethodCall.Arguments[1] as SqlValue;
                    if (sqlValue1 != null && sqlValue1.Value == null)
                    {
                        SqlValue sqlValue2 = new SqlValue(sqlValue1.ClrType, sqlValue1.SqlType, (object)string.Empty, sqlValue1.IsClientSpecified, sqlValue1.SourceExpression);
                        bo = this.MakeCompareTo(sqlMethodCall.Arguments[0], (SqlExpression)sqlValue2, bo.NodeType, iValue) ?? bo;
                    }
                    else
                        bo = this.MakeCompareTo(sqlMethodCall.Arguments[0], sqlMethodCall.Arguments[1], bo.NodeType, iValue) ?? bo;
                }
                return base.VisitBinaryOperator(bo);
            }

            private SqlBinary MakeCompareTo(SqlExpression left, SqlExpression right, SqlNodeType op, int iValue)
            {
                if (iValue == 0)
                    return this.sql.Binary(op, left, right);
                if (op == SqlNodeType.EQ || op == SqlNodeType.EQ2V)
                {
                    if (iValue == -1)
                        return this.sql.Binary(SqlNodeType.LT, left, right);
                    if (iValue == 1)
                        return this.sql.Binary(SqlNodeType.GT, left, right);
                }
                return (SqlBinary)null;
            }

            private SqlExpression CreateComparison(SqlExpression a, SqlExpression b, Expression source)
            {
                SqlExpression match1 = (SqlExpression)this.sql.Binary(SqlNodeType.LT, a, b);
                SqlExpression match2 = (SqlExpression)this.sql.Binary(SqlNodeType.EQ2V, a, b);
                SqlFactory sqlFactory = this.sql;
                SqlWhen[] whens = new SqlWhen[2];
                int index1 = 0;
                SqlWhen sqlWhen1 = new SqlWhen(match1, this.sql.ValueFromObject((object)-1, false, source));
                whens[index1] = sqlWhen1;
                int index2 = 1;
                SqlWhen sqlWhen2 = new SqlWhen(match2, this.sql.ValueFromObject((object)0, false, source));
                whens[index2] = sqlWhen2;
                SqlExpression @else = this.sql.ValueFromObject((object)1, false, source);
                Expression sourceExpression = source;
                return (SqlExpression)sqlFactory.SearchedCase(whens, @else, sourceExpression);
            }

            internal override SqlNode VisitMember(SqlMember m)
            {
                m.Expression = this.VisitExpression(m.Expression);
                if (PreBindDotNetConverter.IsNullableValue(m))
                    return (SqlNode)this.sql.UnaryValueOf(m.Expression, m.SourceExpression);
                if (PreBindDotNetConverter.IsNullableHasValue(m))
                    return (SqlNode)this.sql.Unary(SqlNodeType.IsNotNull, m.Expression, m.SourceExpression);
                return (SqlNode)m;
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                mc.Object = this.VisitExpression(mc.Object);
                int index1 = 0;
                for (int count = mc.Arguments.Count; index1 < count; ++index1)
                    mc.Arguments[index1] = this.VisitExpression(mc.Arguments[index1]);
                if (mc.Method.IsStatic)
                {
                    if (mc.Method.Name == "Equals" && mc.Arguments.Count == 2)
                        return (SqlExpression)this.sql.Binary(SqlNodeType.EQ2V, mc.Arguments[0], mc.Arguments[1], mc.Method);
                    if (mc.Method.DeclaringType == typeof(string) && mc.Method.Name == "Concat")
                    {
                        SqlClientArray sqlClientArray = mc.Arguments[0] as SqlClientArray;
                        List<SqlExpression> list = sqlClientArray == null ? mc.Arguments : sqlClientArray.Expressions;
                        if (list.Count == 0)
                            return this.sql.ValueFromObject((object)"", false, mc.SourceExpression);
                        SqlExpression sqlExpression1 = list[0].SqlType.IsString || list[0].SqlType.IsChar ? list[0] : this.sql.ConvertTo(typeof(string), list[0]);
                        for (int index2 = 1; index2 < list.Count; ++index2)
                        {
                            if (list[index2].SqlType.IsString || list[index2].SqlType.IsChar)
                            {
                                SqlFactory sqlFactory = this.sql;
                                SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                                int index3 = 0;
                                SqlExpression sqlExpression2 = sqlExpression1;
                                sqlExpressionArray[index3] = sqlExpression2;
                                int index4 = 1;
                                SqlExpression sqlExpression3 = list[index2];
                                sqlExpressionArray[index4] = sqlExpression3;
                                sqlExpression1 = sqlFactory.Concat(sqlExpressionArray);
                            }
                            else
                            {
                                SqlFactory sqlFactory = this.sql;
                                SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                                int index3 = 0;
                                SqlExpression sqlExpression2 = sqlExpression1;
                                sqlExpressionArray[index3] = sqlExpression2;
                                int index4 = 1;
                                SqlExpression sqlExpression3 = this.sql.ConvertTo(typeof(string), list[index2]);
                                sqlExpressionArray[index4] = sqlExpression3;
                                sqlExpression1 = sqlFactory.Concat(sqlExpressionArray);
                            }
                        }
                        return sqlExpression1;
                    }
                    if (PreBindDotNetConverter.IsVbIIF(mc))
                        return this.TranslateVbIIF(mc);
                    string name = mc.Method.Name;
                    // ISSUE: reference to a compiler-generated method
                    uint stringHash = uint.Parse(PrivateImplementationDetails.ComputeStringHash(name));
                    if (stringHash <= 1850069070U)
                    {
                        if (stringHash <= 1195761148U)
                        {
                            if (stringHash <= 835846267U)
                            {
                                if ((int)stringHash != 90588446)
                                {
                                    if ((int)stringHash == 835846267 && name == "op_BitwiseAnd")
                                        return (SqlExpression)this.sql.Binary(SqlNodeType.BitAnd, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                                }
                                else if (name == "op_OnesComplement")
                                    return (SqlExpression)this.sql.Unary(SqlNodeType.BitNot, mc.Arguments[0], mc.Method, mc.SourceExpression);
                            }
                            else if ((int)stringHash != 906583475)
                            {
                                if ((int)stringHash == 1195761148 && name == "op_GreaterThan")
                                    return (SqlExpression)this.sql.Binary(SqlNodeType.GT, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            }
                            else if (name == "op_Addition")
                                return (SqlExpression)this.sql.Binary(SqlNodeType.Add, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                        }
                        else if (stringHash <= 1258540185U)
                        {
                            if ((int)stringHash != 1234170120)
                            {
                                if ((int)stringHash == 1258540185 && name == "op_LessThan")
                                    return (SqlExpression)this.sql.Binary(SqlNodeType.LT, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            }
                            else if (name == "op_LessThanOrEqual")
                                return (SqlExpression)this.sql.Binary(SqlNodeType.LE, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                        }
                        else if ((int)stringHash != 1516143579)
                        {
                            if ((int)stringHash == 1850069070 && name == "op_False")
                                return (SqlExpression)this.sql.Unary(SqlNodeType.Not, mc.Arguments[0], mc.Method, mc.SourceExpression);
                        }
                        else if (name == "op_Equality")
                            return (SqlExpression)this.sql.Binary(SqlNodeType.EQ, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                    }
                    else if (stringHash <= 2459852411U)
                    {
                        if (stringHash <= 2366795836U)
                        {
                            if ((int)stringHash != 1915672496)
                            {
                                if ((int)stringHash == -1928171460 && name == "op_ExclusiveOr")
                                    return (SqlExpression)this.sql.Binary(SqlNodeType.BitXor, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                            }
                            else if (name == "op_Division")
                                return (SqlExpression)this.sql.Binary(SqlNodeType.Div, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                        }
                        else if ((int)stringHash != -1865288344)
                        {
                            if ((int)stringHash == -1835114885 && name == "op_GreaterThanOrEqual")
                                return (SqlExpression)this.sql.Binary(SqlNodeType.GE, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                        }
                        else if (name == "op_Modulus")
                            return (SqlExpression)this.sql.Binary(SqlNodeType.Mod, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                    }
                    else if (stringHash <= 3279419199U)
                    {
                        if ((int)stringHash != -1336714801)
                        {
                            if ((int)stringHash == -1015548097 && name == "op_Subtraction")
                                return (SqlExpression)this.sql.Binary(SqlNodeType.Sub, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                        }
                        else if (name == "op_Multiply")
                            return (SqlExpression)this.sql.Binary(SqlNodeType.Mul, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                    }
                    else if ((int)stringHash != -802416729)
                    {
                        if ((int)stringHash != -578301403)
                        {
                            if ((int)stringHash == -500649512 && name == "op_Inequality")
                                return (SqlExpression)this.sql.Binary(SqlNodeType.NE, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                        }
                        else if (name == "op_UnaryNegation")
                            return (SqlExpression)this.sql.Unary(SqlNodeType.Negate, mc.Arguments[0], mc.Method, mc.SourceExpression);
                    }
                    else if (name == "op_BitwiseOr")
                        return (SqlExpression)this.sql.Binary(SqlNodeType.BitOr, mc.Arguments[0], mc.Arguments[1], mc.Method, mc.ClrType);
                }
                else
                {
                    if (mc.Method.Name == "Equals" && mc.Arguments.Count == 1)
                        return (SqlExpression)this.sql.Binary(SqlNodeType.EQ, mc.Object, mc.Arguments[0]);
                    if (mc.Method.Name == "GetType" && mc.Arguments.Count == 0)
                    {
                        MetaType sourceMetaType = TypeSource.GetSourceMetaType((SqlNode)mc.Object, this.model);
                        if (!sourceMetaType.HasInheritance)
                            return this.VisitExpression(this.sql.StaticType(sourceMetaType, mc.SourceExpression));
                        Type type = sourceMetaType.Discriminator.Type;
                        return this.VisitExpression(this.sql.DiscriminatedType((SqlExpression)new SqlDiscriminatorOf(mc.Object, type, this.sql.TypeProvider.From(type), mc.SourceExpression), sourceMetaType));
                    }
                }
                return (SqlExpression)mc;
            }

            private SqlExpression TranslateVbIIF(SqlMethodCall mc)
            {
                if (!(mc.Arguments[1].ClrType == mc.Arguments[2].ClrType))
                    throw Error.IifReturnTypesMustBeEqual((object)mc.Arguments[1].ClrType.Name, (object)mc.Arguments[2].ClrType.Name);
                List<SqlWhen> list = new List<SqlWhen>(1);
                list.Add(new SqlWhen(mc.Arguments[0], mc.Arguments[1]));
                SqlExpression @else;
                SqlSearchedCase sqlSearchedCase;
                for (@else = mc.Arguments[2]; @else.NodeType == SqlNodeType.SearchedCase; @else = sqlSearchedCase.Else)
                {
                    sqlSearchedCase = (SqlSearchedCase)@else;
                    list.AddRange((IEnumerable<SqlWhen>)sqlSearchedCase.Whens);
                }
                return (SqlExpression)this.sql.SearchedCase(list.ToArray(), @else, mc.SourceExpression);
            }

            internal override SqlExpression VisitTreat(SqlUnary t)
            {
                t.Operand = this.VisitExpression(t.Operand);
                Type clrType = t.ClrType;
                Type type = this.model.GetMetaType(t.Operand.ClrType).InheritanceRoot.Type;
                Type nonNullableType1 = TypeSystem.GetNonNullableType(clrType);
                Type nonNullableType2 = TypeSystem.GetNonNullableType(type);
                if (nonNullableType1 == nonNullableType2)
                    return t.Operand;
                if (nonNullableType1.IsAssignableFrom(nonNullableType2))
                {
                    t.Operand.SetClrType(nonNullableType1);
                    return t.Operand;
                }
                if (!nonNullableType1.IsAssignableFrom(nonNullableType2) && !nonNullableType2.IsAssignableFrom(nonNullableType1) && (!nonNullableType1.IsInterface && !nonNullableType2.IsInterface))
                    return this.sql.TypedLiteralNull(nonNullableType1, t.SourceExpression);
                return (SqlExpression)t;
            }
        }
    }
}
