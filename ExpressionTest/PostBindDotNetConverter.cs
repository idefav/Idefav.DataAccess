using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class PostBindDotNetConverter
    {
        private static readonly string[] dateParts;

        static PostBindDotNetConverter()
        {
            string[] strArray = new string[9];
            int index1 = 0;
            string str1 = "Year";
            strArray[index1] = str1;
            int index2 = 1;
            string str2 = "Month";
            strArray[index2] = str2;
            int index3 = 2;
            string str3 = "Day";
            strArray[index3] = str3;
            int index4 = 3;
            string str4 = "Hour";
            strArray[index4] = str4;
            int index5 = 4;
            string str5 = "Minute";
            strArray[index5] = str5;
            int index6 = 5;
            string str6 = "Second";
            strArray[index6] = str6;
            int index7 = 6;
            string str7 = "Millisecond";
            strArray[index7] = str7;
            int index8 = 7;
            string str8 = "Microsecond";
            strArray[index8] = str8;
            int index9 = 8;
            string str9 = "Nanosecond";
            strArray[index9] = str9;
            PostBindDotNetConverter.dateParts = strArray;
        }

        internal static SqlNode Convert(SqlNode node, SqlFactory sql, SqlProvider.ProviderMode providerMode)
        {
            return new PostBindDotNetConverter.Visitor(sql, providerMode).Visit(node);
        }

        internal static bool CanConvert(SqlNode node)
        {
            SqlUnary uo = node as SqlUnary;
            if (uo != null && PostBindDotNetConverter.IsSupportedUnary(uo))
                return true;
            SqlNew snew = node as SqlNew;
            if (snew != null && PostBindDotNetConverter.IsSupportedNew(snew))
                return true;
            SqlMember m = node as SqlMember;
            if (m != null && PostBindDotNetConverter.IsSupportedMember(m))
                return true;
            SqlMethodCall mc = node as SqlMethodCall;
            return mc != null && PostBindDotNetConverter.GetMethodSupport(mc) == PostBindDotNetConverter.MethodSupport.Method;
        }

        private static bool IsSupportedUnary(SqlUnary uo)
        {
            if (uo.NodeType != SqlNodeType.Convert || !(uo.ClrType == typeof(char)))
                return uo.Operand.ClrType == typeof(char);
            return true;
        }

        private static bool IsSupportedNew(SqlNew snew)
        {
            if (snew.ClrType == typeof(string))
                return PostBindDotNetConverter.IsSupportedStringNew(snew);
            if (snew.ClrType == typeof(TimeSpan))
                return PostBindDotNetConverter.IsSupportedTimeSpanNew(snew);
            if (snew.ClrType == typeof(DateTime))
                return PostBindDotNetConverter.IsSupportedDateTimeNew(snew);
            return false;
        }

        private static bool IsSupportedStringNew(SqlNew snew)
        {
            if (snew.Args.Count == 2 && snew.Args[0].ClrType == typeof(char))
                return snew.Args[1].ClrType == typeof(int);
            return false;
        }

        private static bool IsSupportedDateTimeNew(SqlNew sox)
        {
            return sox.ClrType == typeof(DateTime) && sox.Args.Count >= 3 && (sox.Args[0].ClrType == typeof(int) && sox.Args[1].ClrType == typeof(int)) && sox.Args[2].ClrType == typeof(int) && (sox.Args.Count == 3 || sox.Args.Count >= 6 && sox.Args[3].ClrType == typeof(int) && (sox.Args[4].ClrType == typeof(int) && sox.Args[5].ClrType == typeof(int)) && (sox.Args.Count == 6 || sox.Args.Count == 7 && sox.Args[6].ClrType == typeof(int)));
        }

        private static bool IsSupportedTimeSpanNew(SqlNew sox)
        {
            return sox.Args.Count == 1 || sox.Args.Count == 3 || (sox.Args.Count == 4 || sox.Args.Count == 5);
        }

        private static PostBindDotNetConverter.MethodSupport GetMethodSupport(SqlMethodCall mc)
        {
            PostBindDotNetConverter.MethodSupport methodSupport = PostBindDotNetConverter.MethodSupport.None;
            PostBindDotNetConverter.MethodSupport methodsMethodSupport = PostBindDotNetConverter.GetSqlMethodsMethodSupport(mc);
            if (methodsMethodSupport > methodSupport)
                methodSupport = methodsMethodSupport;
            PostBindDotNetConverter.MethodSupport timeMethodSupport = PostBindDotNetConverter.GetDateTimeMethodSupport(mc);
            if (timeMethodSupport > methodSupport)
                methodSupport = timeMethodSupport;
            PostBindDotNetConverter.MethodSupport offsetMethodSupport = PostBindDotNetConverter.GetDateTimeOffsetMethodSupport(mc);
            if (offsetMethodSupport > methodSupport)
                methodSupport = offsetMethodSupport;
            PostBindDotNetConverter.MethodSupport spanMethodSupport = PostBindDotNetConverter.GetTimeSpanMethodSupport(mc);
            if (spanMethodSupport > methodSupport)
                methodSupport = spanMethodSupport;
            PostBindDotNetConverter.MethodSupport convertMethodSupport = PostBindDotNetConverter.GetConvertMethodSupport(mc);
            if (convertMethodSupport > methodSupport)
                methodSupport = convertMethodSupport;
            PostBindDotNetConverter.MethodSupport decimalMethodSupport = PostBindDotNetConverter.GetDecimalMethodSupport(mc);
            if (decimalMethodSupport > methodSupport)
                methodSupport = decimalMethodSupport;
            PostBindDotNetConverter.MethodSupport mathMethodSupport = PostBindDotNetConverter.GetMathMethodSupport(mc);
            if (mathMethodSupport > methodSupport)
                methodSupport = mathMethodSupport;
            PostBindDotNetConverter.MethodSupport stringMethodSupport = PostBindDotNetConverter.GetStringMethodSupport(mc);
            if (stringMethodSupport > methodSupport)
                methodSupport = stringMethodSupport;
            PostBindDotNetConverter.MethodSupport comparisonMethodSupport = PostBindDotNetConverter.GetComparisonMethodSupport(mc);
            if (comparisonMethodSupport > methodSupport)
                methodSupport = comparisonMethodSupport;
            PostBindDotNetConverter.MethodSupport nullableMethodSupport = PostBindDotNetConverter.GetNullableMethodSupport(mc);
            if (nullableMethodSupport > methodSupport)
                methodSupport = nullableMethodSupport;
            PostBindDotNetConverter.MethodSupport coercionMethodSupport = PostBindDotNetConverter.GetCoercionMethodSupport(mc);
            if (coercionMethodSupport > methodSupport)
                methodSupport = coercionMethodSupport;
            PostBindDotNetConverter.MethodSupport objectMethodSupport = PostBindDotNetConverter.GetObjectMethodSupport(mc);
            if (objectMethodSupport > methodSupport)
                methodSupport = objectMethodSupport;
            PostBindDotNetConverter.MethodSupport helperMethodSupport = PostBindDotNetConverter.GetVbHelperMethodSupport(mc);
            if (helperMethodSupport > methodSupport)
                methodSupport = helperMethodSupport;
            return methodSupport;
        }

        private static PostBindDotNetConverter.MethodSupport GetCoercionMethodSupport(SqlMethodCall mc)
        {
            return mc.Method.IsStatic && mc.SqlType.CanBeColumn && (mc.Method.Name == "op_Implicit" || mc.Method.Name == "op_Explicit") ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetComparisonMethodSupport(SqlMethodCall mc)
        {
            return mc.Method.IsStatic && mc.Method.Name == "Compare" && mc.Method.ReturnType == typeof(int) ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetObjectMethodSupport(SqlMethodCall mc)
        {
            if (!mc.Method.IsStatic)
            {
                string name = mc.Method.Name;
                if (name == "Equals")
                    return PostBindDotNetConverter.MethodSupport.Method;
                if (!(name == "ToString"))
                {
                    if (name == "GetType" && mc.Arguments.Count == 0)
                        return PostBindDotNetConverter.MethodSupport.Method;
                }
                else
                    return mc.Object.SqlType.CanBeColumn ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.None;
            }
            return PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetNullableMethodSupport(SqlMethodCall mc)
        {
            return mc.Method.Name == "GetValueOrDefault" && TypeSystem.IsNullableType(mc.Object.ClrType) ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetSqlMethodsMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic && mc.Method.DeclaringType == typeof(SqlMethods))
            {
                if (mc.Method.Name.StartsWith("DateDiff", StringComparison.Ordinal) && mc.Arguments.Count == 2)
                {
                    foreach (string str in PostBindDotNetConverter.dateParts)
                    {
                        if (mc.Method.Name == "DateDiff" + str)
                            return mc.Arguments.Count == 2 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                    }
                }
                else
                {
                    if (mc.Method.Name == "Like")
                        return mc.Arguments.Count == 2 || mc.Arguments.Count == 3 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                    if (mc.Method.Name == "RawLength")
                        return PostBindDotNetConverter.MethodSupport.Method;
                }
            }
            return PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetDateTimeMethodSupport(SqlMethodCall mc)
        {
            if (!mc.Method.IsStatic && mc.Method.DeclaringType == typeof(DateTime))
            {
                string name = mc.Method.Name;
                // ISSUE: reference to a compiler-generated method
                uint stringHash = 0;
                if (stringHash <= 1400856914U)
                {
                    if (stringHash <= 670371335U)
                    {
                        if ((int)stringHash != 344475515)
                        {
                            if ((int)stringHash != 670371335 || !(name == "AddDays"))
                                goto label_24;
                        }
                        else if (!(name == "AddHours"))
                            goto label_24;
                    }
                    else if ((int)stringHash != 986892477)
                    {
                        if ((int)stringHash != 1176009147)
                        {
                            if ((int)stringHash != 1400856914 || !(name == "AddMilliseconds"))
                                goto label_24;
                        }
                        else if (!(name == "AddMonths"))
                            goto label_24;
                    }
                    else if (!(name == "CompareTo"))
                        goto label_24;
                }
                else if (stringHash <= 2646845972U)
                {
                    if ((int)stringHash != 1609981065)
                    {
                        if ((int)stringHash == -1648121324 && name == "Add")
                            return mc.Arguments.Count == 1 && mc.Arguments[0].ClrType == typeof(TimeSpan) ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                        goto label_24;
                    }
                    else if (!(name == "AddSeconds"))
                        goto label_24;
                }
                else if ((int)stringHash != -1328879915)
                {
                    if ((int)stringHash != -1121767774)
                    {
                        if ((int)stringHash != -652534610 || !(name == "AddTicks"))
                            goto label_24;
                    }
                    else if (!(name == "AddYears"))
                        goto label_24;
                }
                else if (!(name == "AddMinutes"))
                    goto label_24;
                return PostBindDotNetConverter.MethodSupport.Method;
            }
            label_24:
            return PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetDateTimeOffsetMethodSupport(SqlMethodCall mc)
        {
            if (!mc.Method.IsStatic && mc.Method.DeclaringType == typeof(DateTimeOffset))
            {
                string name = mc.Method.Name;
                // ISSUE: reference to a compiler-generated method
                uint stringHash = 0;
                if (stringHash <= 1400856914U)
                {
                    if (stringHash <= 670371335U)
                    {
                        if ((int)stringHash != 344475515)
                        {
                            if ((int)stringHash != 670371335 || !(name == "AddDays"))
                                goto label_24;
                        }
                        else if (!(name == "AddHours"))
                            goto label_24;
                    }
                    else if ((int)stringHash != 986892477)
                    {
                        if ((int)stringHash != 1176009147)
                        {
                            if ((int)stringHash != 1400856914 || !(name == "AddMilliseconds"))
                                goto label_24;
                        }
                        else if (!(name == "AddMonths"))
                            goto label_24;
                    }
                    else if (!(name == "CompareTo"))
                        goto label_24;
                }
                else if (stringHash <= 2646845972U)
                {
                    if ((int)stringHash != 1609981065)
                    {
                        if ((int)stringHash == -1648121324 && name == "Add")
                            return mc.Arguments.Count == 1 && mc.Arguments[0].ClrType == typeof(TimeSpan) ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                        goto label_24;
                    }
                    else if (!(name == "AddSeconds"))
                        goto label_24;
                }
                else if ((int)stringHash != -1328879915)
                {
                    if ((int)stringHash != -1121767774)
                    {
                        if ((int)stringHash != -652534610 || !(name == "AddTicks"))
                            goto label_24;
                    }
                    else if (!(name == "AddYears"))
                        goto label_24;
                }
                else if (!(name == "AddMinutes"))
                    goto label_24;
                return PostBindDotNetConverter.MethodSupport.Method;
            }
            label_24:
            return PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetTimeSpanMethodSupport(SqlMethodCall mc)
        {
            if (!mc.Method.IsStatic && mc.Method.DeclaringType == typeof(TimeSpan))
            {
                string name = mc.Method.Name;
                if (name == "Add" || name == "Subtract" || (name == "CompareTo" || name == "Duration") || name == "Negate")
                    return PostBindDotNetConverter.MethodSupport.Method;
            }
            return PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetConvertMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic && mc.Method.DeclaringType == typeof(Convert) && mc.Arguments.Count == 1)
            {
                string name = mc.Method.Name;
                // ISSUE: reference to a compiler-generated method
                uint stringHash = 0;
                if (stringHash <= 1628624528U)
                {
                    if (stringHash <= 851977407U)
                    {
                        if ((int)stringHash != 356562880)
                        {
                            if ((int)stringHash != 851977407 || !(name == "ToInt64"))
                                goto label_26;
                        }
                        else if (!(name == "ToBoolean"))
                            goto label_26;
                    }
                    else if ((int)stringHash != 852124502)
                    {
                        if ((int)stringHash != 1600551161)
                        {
                            if ((int)stringHash != 1628624528 || !(name == "ToChar"))
                                goto label_26;
                        }
                        else
                        {
                            if (name == "ToDateTime")
                                return mc.Arguments[0].ClrType == typeof(string) || mc.Arguments[0].ClrType == typeof(DateTime) ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                            goto label_26;
                        }
                    }
                    else if (!(name == "ToInt16"))
                        goto label_26;
                }
                else if (stringHash <= 3017764115U)
                {
                    if ((int)stringHash != 1883303205)
                    {
                        if ((int)stringHash != 1938964185)
                        {
                            if ((int)stringHash != -1277203181 || !(name == "ToDecimal"))
                                goto label_26;
                        }
                        else if (!(name == "ToDouble"))
                            goto label_26;
                    }
                    else if (!(name == "ToString"))
                        goto label_26;
                }
                else if ((int)stringHash != -1107071570)
                {
                    if ((int)stringHash != -1093681944)
                    {
                        if ((int)stringHash != -522456384 || !(name == "ToSingle"))
                            goto label_26;
                    }
                    else if (!(name == "ToInt32"))
                        goto label_26;
                }
                else if (!(name == "ToByte"))
                    goto label_26;
                return PostBindDotNetConverter.MethodSupport.Method;
            }
            label_26:
            return PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetDecimalMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic)
            {
                if (mc.Arguments.Count == 2)
                {
                    string name = mc.Method.Name;
                    if (name == "Multiply" || name == "Divide" || (name == "Subtract" || name == "Add") || (name == "Remainder" || name == "Round"))
                        return PostBindDotNetConverter.MethodSupport.Method;
                }
                else if (mc.Arguments.Count == 1)
                {
                    string name = mc.Method.Name;
                    if (name == "Negate" || name == "Floor" || (name == "Truncate" || name == "Round") || mc.Method.Name.StartsWith("To", StringComparison.Ordinal))
                        return PostBindDotNetConverter.MethodSupport.Method;
                }
            }
            return PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetStringMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.DeclaringType == typeof(string))
            {
                if (mc.Method.IsStatic)
                {
                    if (mc.Method.Name == "Concat")
                        return PostBindDotNetConverter.MethodSupport.Method;
                }
                else
                {
                    string name = mc.Method.Name;
                    // ISSUE: reference to a compiler-generated method
                    uint stringHash = 0;
                    if (stringHash <= 1846593938U)
                    {
                        if (stringHash <= 585210133U)
                        {
                            if (stringHash <= 254900552U)
                            {
                                if ((int)stringHash != 169760743)
                                {
                                    if ((int)stringHash == 254900552 && name == "Insert")
                                        return mc.Arguments.Count == 2 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                                    goto label_54;
                                }
                                else if (name == "PadLeft")
                                    goto label_44;
                                else
                                    goto label_54;
                            }
                            else if ((int)stringHash != 564498461)
                            {
                                if ((int)stringHash != 585210133 || !(name == "get_Chars"))
                                    goto label_54;
                            }
                            else if (name == "Remove")
                                goto label_44;
                            else
                                goto label_54;
                        }
                        else if (stringHash <= 1128357681U)
                        {
                            if ((int)stringHash != 986892477)
                            {
                                if ((int)stringHash != 1128357681 || !(name == "ToLower"))
                                    goto label_54;
                                else
                                    goto label_48;
                            }
                            else if (!(name == "CompareTo"))
                                goto label_54;
                        }
                        else if ((int)stringHash != 1721518424)
                        {
                            if ((int)stringHash != 1846593938 || !(name == "LastIndexOf"))
                                goto label_54;
                            else
                                goto label_38;
                        }
                        else if (name == "Contains")
                            goto label_35;
                        else
                            goto label_54;
                        return mc.Arguments.Count == 1 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                    }
                    if (stringHash <= 3243991448U)
                    {
                        if (stringHash <= 3035490584U)
                        {
                            if ((int)stringHash != -1428418018)
                            {
                                if ((int)stringHash != -1259476712 || !(name == "ToUpper"))
                                    goto label_54;
                                else
                                    goto label_48;
                            }
                            else if (name == "IndexOf")
                                goto label_38;
                            else
                                goto label_54;
                        }
                        else if ((int)stringHash != -1142066180)
                        {
                            if ((int)stringHash != -1050975848 || !(name == "PadRight"))
                                goto label_54;
                            else
                                goto label_44;
                        }
                        else if (!(name == "StartsWith"))
                            goto label_54;
                    }
                    else if (stringHash <= 3839184739U)
                    {
                        if ((int)stringHash != -1028557880)
                        {
                            if ((int)stringHash == -455782557 && name == "Replace")
                                return PostBindDotNetConverter.MethodSupport.Method;
                            goto label_54;
                        }
                        else if (name == "Substring")
                            goto label_44;
                        else
                            goto label_54;
                    }
                    else if ((int)stringHash != -171931671)
                    {
                        if ((int)stringHash != -31485789 || !(name == "EndsWith"))
                            goto label_54;
                    }
                    else if (name == "Trim")
                        goto label_48;
                    else
                        goto label_54;
                    label_35:
                    return mc.Arguments.Count == 1 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                    label_38:
                    return mc.Arguments.Count == 1 || mc.Arguments.Count == 2 || mc.Arguments.Count == 3 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                    label_44:
                    return mc.Arguments.Count == 1 || mc.Arguments.Count == 2 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                    label_48:
                    return mc.Arguments.Count == 0 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                }
            }
            label_54:
            return PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetMathMethodSupport(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic && mc.Method.DeclaringType == typeof(Math))
            {
                string name = mc.Method.Name;
                // ISSUE: reference to a compiler-generated method
                uint stringHash =0;
                if (stringHash <= 2405646532U)
                {
                    if (stringHash <= 1587810493U)
                    {
                        if (stringHash <= 1017635769U)
                        {
                            if ((int)stringHash != 583084759)
                            {
                                if ((int)stringHash != 781469175)
                                {
                                    if ((int)stringHash != 1017635769 || !(name == "Max"))
                                        goto label_63;
                                    else
                                        goto label_54;
                                }
                                else if (name == "Min")
                                    goto label_54;
                                else
                                    goto label_63;
                            }
                            else if (name == "Truncate")
                                goto label_60;
                            else
                                goto label_63;
                        }
                        else if ((int)stringHash != 1119638765)
                        {
                            if ((int)stringHash != 1578662460)
                            {
                                if ((int)stringHash != 1587810493 || !(name == "Floor"))
                                    goto label_63;
                            }
                            else if (!(name == "Cos"))
                                goto label_63;
                        }
                        else if (name == "Sin")
                            goto label_60;
                        else
                            goto label_63;
                    }
                    else if (stringHash <= 2258966239U)
                    {
                        if ((int)stringHash != 1955107388)
                        {
                            if ((int)stringHash != 2128371142)
                            {
                                if ((int)stringHash != -2036001057 || !(name == "Atan"))
                                    goto label_63;
                            }
                            else if (!(name == "Asin"))
                                goto label_63;
                        }
                        else if (!(name == "Cosh"))
                            goto label_63;
                    }
                    else if ((int)stringHash != -1949365520)
                    {
                        if ((int)stringHash != -1932355109)
                        {
                            if ((int)stringHash != -1889320764 || !(name == "Sign"))
                                goto label_63;
                            else
                                goto label_60;
                        }
                        else if (!(name == "Abs"))
                            goto label_63;
                    }
                    else if (name == "Tanh")
                        goto label_60;
                    else
                        goto label_63;
                }
                else if (stringHash <= 3174591349U)
                {
                    if (stringHash <= 2715646961U)
                    {
                        if ((int)stringHash != -1820783265)
                        {
                            if ((int)stringHash != -1739485872)
                            {
                                if ((int)stringHash == -1579320335 && name == "Log")
                                    return mc.Arguments.Count == 1 || mc.Arguments.Count == 2 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                                goto label_63;
                            }
                            else if (!(name == "Log10"))
                                goto label_63;
                        }
                        else if (name == "Sinh")
                            goto label_60;
                        else
                            goto label_63;
                    }
                    else if ((int)stringHash != -1525441595)
                    {
                        if ((int)stringHash != -1137824961)
                        {
                            if ((int)stringHash != -1120375947 || !(name == "Pow"))
                                goto label_63;
                            else
                                goto label_54;
                        }
                        else if (!(name == "Acos"))
                            goto label_63;
                    }
                    else if (name == "BigMul")
                        goto label_54;
                    else
                        goto label_63;
                }
                else if (stringHash <= 3935307552U)
                {
                    if ((int)stringHash != -676624440)
                    {
                        if ((int)stringHash != -488433897)
                        {
                            if ((int)stringHash != -359659744 || !(name == "Ceiling"))
                                goto label_63;
                        }
                        else if (name == "Atan2")
                            goto label_54;
                        else
                            goto label_63;
                    }
                    else if (!(name == "Exp"))
                        goto label_63;
                }
                else if ((int)stringHash != -188263461)
                {
                    if ((int)stringHash != -31815697)
                    {
                        if ((int)stringHash != -8167624 || !(name == "Tan"))
                            goto label_63;
                        else
                            goto label_60;
                    }
                    else if (name == "Sqrt")
                        goto label_60;
                    else
                        goto label_63;
                }
                else
                {
                    if (name == "Round")
                        return mc.Arguments[mc.Arguments.Count - 1].ClrType == typeof(MidpointRounding) && (mc.Arguments.Count == 2 || mc.Arguments.Count == 3) ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                    goto label_63;
                }
                return mc.Arguments.Count == 1 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                label_54:
                return mc.Arguments.Count == 2 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
                label_60:
                return mc.Arguments.Count == 1 ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.MethodGroup;
            }
            label_63:
            return PostBindDotNetConverter.MethodSupport.None;
        }

        private static PostBindDotNetConverter.MethodSupport GetVbHelperMethodSupport(SqlMethodCall mc)
        {
            return PostBindDotNetConverter.IsVbConversionMethod(mc) || PostBindDotNetConverter.IsVbCompareString(mc) || PostBindDotNetConverter.IsVbLike(mc) ? PostBindDotNetConverter.MethodSupport.Method : PostBindDotNetConverter.MethodSupport.None;
        }

        private static bool IsVbCompareString(SqlMethodCall call)
        {
            if (call.Method.IsStatic && call.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators")
                return call.Method.Name == "CompareString";
            return false;
        }

        private static bool IsVbLike(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic && mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.LikeOperator" && mc.Method.Name == "LikeString")
                return true;
            if (mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators")
                return mc.Method.Name == "LikeString";
            return false;
        }

        private static bool IsVbConversionMethod(SqlMethodCall mc)
        {
            if (mc.Method.IsStatic && mc.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Conversions")
            {
                string name = mc.Method.Name;
                // ISSUE: reference to a compiler-generated method
                uint stringHash = 0;
                if (stringHash <= 1938964185U)
                {
                    if (stringHash <= 1628624528U)
                    {
                        if (stringHash <= 989231474U)
                        {
                            if ((int)stringHash != 356562880)
                            {
                                if ((int)stringHash != 989231474 || !(name == "ToLong"))
                                    goto label_33;
                            }
                            else if (!(name == "ToBoolean"))
                                goto label_33;
                        }
                        else if ((int)stringHash != 1104427872)
                        {
                            if ((int)stringHash != 1628624528 || !(name == "ToChar"))
                                goto label_33;
                        }
                        else if (!(name == "ToDate"))
                            goto label_33;
                    }
                    else if (stringHash <= 1883303205U)
                    {
                        if ((int)stringHash != 1862051603)
                        {
                            if ((int)stringHash != 1883303205 || !(name == "ToString"))
                                goto label_33;
                        }
                        else if (!(name == "ToUShort"))
                            goto label_33;
                    }
                    else if ((int)stringHash != 1903862909)
                    {
                        if ((int)stringHash != 1938964185 || !(name == "ToDouble"))
                            goto label_33;
                    }
                    else if (!(name == "ToULong"))
                        goto label_33;
                }
                else if (stringHash <= 3049974302U)
                {
                    if (stringHash <= 2978282195U)
                    {
                        if ((int)stringHash != -1829286225)
                        {
                            if ((int)stringHash != -1316685101 || !(name == "ToUInteger"))
                                goto label_33;
                        }
                        else if (!(name == "ToSByte"))
                            goto label_33;
                    }
                    else if ((int)stringHash != -1277203181)
                    {
                        if ((int)stringHash != -1244992994 || !(name == "ToShort"))
                            goto label_33;
                    }
                    else if (!(name == "ToDecimal"))
                        goto label_33;
                }
                else if (stringHash <= 3300893262U)
                {
                    if ((int)stringHash != -1107071570)
                    {
                        if ((int)stringHash != -994074034 || !(name == "ToInteger"))
                            goto label_33;
                    }
                    else if (!(name == "ToByte"))
                        goto label_33;
                }
                else if ((int)stringHash != -522456384)
                {
                    if ((int)stringHash != -237980003 || !(name == "ToCharArrayRankOne"))
                        goto label_33;
                }
                else if (!(name == "ToSingle"))
                    goto label_33;
                return true;
            }
            label_33:
            return false;
        }

        private static bool IsSupportedMember(SqlMember m)
        {
            if (!PostBindDotNetConverter.IsSupportedStringMember(m) && !PostBindDotNetConverter.IsSupportedBinaryMember(m) && (!PostBindDotNetConverter.IsSupportedDateTimeMember(m) && !PostBindDotNetConverter.IsSupportedDateTimeOffsetMember(m)))
                return PostBindDotNetConverter.IsSupportedTimeSpanMember(m);
            return true;
        }

        private static bool IsSupportedStringMember(SqlMember m)
        {
            if (m.Expression.ClrType == typeof(string))
                return m.Member.Name == "Length";
            return false;
        }

        private static bool IsSupportedBinaryMember(SqlMember m)
        {
            if (m.Expression.ClrType == typeof(Binary))
                return m.Member.Name == "Length";
            return false;
        }

        private static string GetDatePart(string memberName)
        {
            // ISSUE: reference to a compiler-generated method
            uint stringHash = 0;
            if (stringHash <= 909183791U)
            {
                if (stringHash <= 465588106U)
                {
                    if ((int)stringHash != 358540348)
                    {
                        if ((int)stringHash != 465588106 || !(memberName == "Millisecond"))
                            goto label_16;
                    }
                    else if (!(memberName == "Year"))
                        goto label_16;
                }
                else if ((int)stringHash != 641982345)
                {
                    if ((int)stringHash != 909183791 || !(memberName == "Hour"))
                        goto label_16;
                }
                else if (!(memberName == "Minute"))
                    goto label_16;
            }
            else if (stringHash <= 1945110009U)
            {
                if ((int)stringHash != 1230250653)
                {
                    if ((int)stringHash != 1945110009 || !(memberName == "DayOfYear"))
                        goto label_16;
                }
                else if (!(memberName == "Day"))
                    goto label_16;
            }
            else if ((int)stringHash != -1468654683)
            {
                if ((int)stringHash != -192497667 || !(memberName == "Second"))
                    goto label_16;
            }
            else if (!(memberName == "Month"))
                goto label_16;
            return memberName;
            label_16:
            return (string)null;
        }

        private static bool IsSupportedDateTimeMember(SqlMember m)
        {
            if (m.Expression.ClrType == typeof(DateTime))
            {
                if (PostBindDotNetConverter.GetDatePart(m.Member.Name) != null)
                    return true;
                string name = m.Member.Name;
                if (name == "Date" || name == "TimeOfDay" || name == "DayOfWeek")
                    return true;
            }
            return false;
        }

        private static bool IsSupportedDateTimeOffsetMember(SqlMember m)
        {
            if (m.Expression.ClrType == typeof(DateTimeOffset))
            {
                if (PostBindDotNetConverter.GetDatePart(m.Member.Name) != null)
                    return true;
                string name = m.Member.Name;
                if (name == "Date" || name == "DateTime" || (name == "TimeOfDay" || name == "DayOfWeek"))
                    return true;
            }
            return false;
        }

        private static bool IsSupportedTimeSpanMember(SqlMember m)
        {
            if (m.Expression.ClrType == typeof(TimeSpan))
            {
                string name = m.Member.Name;
                // ISSUE: reference to a compiler-generated method
                uint stringHash = 0;
                if (stringHash <= 2115171466U)
                {
                    if (stringHash <= 920229518U)
                    {
                        if ((int)stringHash != 740138773)
                        {
                            if ((int)stringHash != 920229518 || !(name == "Minutes"))
                                goto label_23;
                        }
                        else if (!(name == "Ticks"))
                            goto label_23;
                    }
                    else if ((int)stringHash != 1215240326)
                    {
                        if ((int)stringHash != 1567816874)
                        {
                            if ((int)stringHash != 2115171466 || !(name == "Seconds"))
                                goto label_23;
                        }
                        else if (!(name == "Days"))
                            goto label_23;
                    }
                    else if (!(name == "TotalMinutes"))
                        goto label_23;
                }
                else if (stringHash <= 2745576355U)
                {
                    if ((int)stringHash != -2090719662)
                    {
                        if ((int)stringHash != -1800856484)
                        {
                            if ((int)stringHash != -1549390941 || !(name == "TotalMilliseconds"))
                                goto label_23;
                        }
                        else if (!(name == "TotalHours"))
                            goto label_23;
                    }
                    else if (!(name == "TotalDays"))
                        goto label_23;
                }
                else if ((int)stringHash != -1463950085)
                {
                    if ((int)stringHash != -1422597676)
                    {
                        if ((int)stringHash != -61377358 || !(name == "TotalSeconds"))
                            goto label_23;
                    }
                    else if (!(name == "Hours"))
                        goto label_23;
                }
                else if (!(name == "Milliseconds"))
                    goto label_23;
                return true;
            }
            label_23:
            return false;
        }

        internal enum MethodSupport
        {
            None,
            MethodGroup,
            Method,
        }

        private class SqlSelectionSkipper : SqlVisitor
        {
            private SqlVisitor parent;

            internal SqlSelectionSkipper(SqlVisitor parent)
            {
                this.parent = parent;
            }

            internal override SqlExpression VisitColumn(SqlColumn col)
            {
                return this.parent.VisitColumn(col);
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return this.parent.VisitSubSelect(ss);
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                return this.parent.VisitClientQuery(cq);
            }
        }

        private class Visitor : SqlVisitor
        {
            private SqlFactory sql;
            private SqlProvider.ProviderMode providerMode;
            private PostBindDotNetConverter.SqlSelectionSkipper skipper;

            internal Visitor(SqlFactory sql, SqlProvider.ProviderMode providerMode)
            {
                this.sql = sql;
                this.providerMode = providerMode;
                this.skipper = new PostBindDotNetConverter.SqlSelectionSkipper((SqlVisitor)this);
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                select = this.VisitSelectCore(select);
                select.Selection = this.skipper.VisitExpression(select.Selection);
                return select;
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                if (uo.NodeType == SqlNodeType.Convert)
                {
                    Type clrType = uo.ClrType;
                    SqlExpression operand = uo.Operand;
                    if (clrType == typeof(char) || operand.ClrType == typeof(char))
                    {
                        SqlExpression expr = this.VisitExpression(uo.Operand);
                        uo.Operand = expr;
                        return this.sql.ConvertTo(clrType, uo.SqlType, expr);
                    }
                }
                return base.VisitUnaryOperator(uo);
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                bo = (SqlBinary)base.VisitBinaryOperator(bo);
                Type nonNullableType = TypeSystem.GetNonNullableType(bo.Left.ClrType);
                if (nonNullableType == typeof(DateTime) || nonNullableType == typeof(DateTimeOffset))
                    return this.TranslateDateTimeBinary(bo);
                return (SqlExpression)bo;
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase tc)
            {
                tc.Discriminator = this.VisitExpression(tc.Discriminator);
                List<SqlExpression> matches = new List<SqlExpression>();
                List<SqlExpression> values = new List<SqlExpression>();
                bool flag = true;
                foreach (SqlTypeCaseWhen sqlTypeCaseWhen in tc.Whens)
                {
                    SqlExpression sqlExpression1 = this.VisitExpression(sqlTypeCaseWhen.Match);
                    SqlExpression sqlExpression2 = this.VisitExpression(sqlTypeCaseWhen.TypeBinding);
                    flag = flag && sqlExpression2 is SqlNew;
                    matches.Add(sqlExpression1);
                    values.Add(sqlExpression2);
                }
                if (!flag)
                    return this.sql.Case(tc.ClrType, tc.Discriminator, matches, values, tc.SourceExpression);
                int index = 0;
                for (int count = tc.Whens.Count; index < count; ++index)
                {
                    SqlTypeCaseWhen sqlTypeCaseWhen = tc.Whens[index];
                    SqlExpression sqlExpression = matches[index];
                    sqlTypeCaseWhen.Match = sqlExpression;
                    SqlNew sqlNew = (SqlNew)values[index];
                    sqlTypeCaseWhen.TypeBinding = (SqlExpression)sqlNew;
                }
                return (SqlExpression)tc;
            }

            internal override SqlExpression VisitNew(SqlNew sox)
            {
                sox = (SqlNew)base.VisitNew(sox);
                if (sox.ClrType == typeof(string))
                    return this.TranslateNewString(sox);
                if (sox.ClrType == typeof(TimeSpan))
                    return this.TranslateNewTimeSpan(sox);
                if (sox.ClrType == typeof(DateTime))
                    return this.TranslateNewDateTime(sox);
                if (sox.ClrType == typeof(DateTimeOffset))
                    return this.TranslateNewDateTimeOffset(sox);
                return (SqlExpression)sox;
            }

            private SqlExpression TranslateNewString(SqlNew sox)
            {
                if (!(sox.ClrType == typeof(string)) || sox.Args.Count != 2 || (!(sox.Args[0].ClrType == typeof(char)) || !(sox.Args[1].ClrType == typeof(int))))
                    throw Error.UnsupportedStringConstructorForm();
                SqlFactory sqlFactory = this.sql;
                Type clrType = typeof(string);
                string name = "REPLICATE";
                SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                int index1 = 0;
                SqlExpression sqlExpression1 = sox.Args[0];
                sqlExpressionArray[index1] = sqlExpression1;
                int index2 = 1;
                SqlExpression sqlExpression2 = sox.Args[1];
                sqlExpressionArray[index2] = sqlExpression2;
                Expression sourceExpression = sox.SourceExpression;
                return (SqlExpression)sqlFactory.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, sourceExpression);
            }

            private SqlExpression TranslateNewDateTime(SqlNew sox)
            {
                Expression sourceExpression = sox.SourceExpression;
                if (sox.ClrType == typeof(DateTime) && sox.Args.Count >= 3 && (sox.Args[0].ClrType == typeof(int) && sox.Args[1].ClrType == typeof(int)) && sox.Args[2].ClrType == typeof(int))
                {
                    SqlFactory sqlFactory1 = this.sql;
                    Type clrType1 = typeof(void);
                    string name1 = "NCHAR";
                    SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
                    int index1 = 0;
                    SqlExpression sqlExpression1 = this.sql.ValueFromObject((object)2, false, sourceExpression);
                    sqlExpressionArray1[index1] = sqlExpression1;
                    Expression source1 = sourceExpression;
                    SqlExpression sqlExpression2 = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name1, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                    SqlFactory sqlFactory2 = this.sql;
                    Type clrType2 = typeof(void);
                    string name2 = "NCHAR";
                    SqlExpression[] sqlExpressionArray2 = new SqlExpression[1];
                    int index2 = 0;
                    SqlExpression sqlExpression3 = this.sql.ValueFromObject((object)4, false, sourceExpression);
                    sqlExpressionArray2[index2] = sqlExpression3;
                    Expression source2 = sourceExpression;
                    SqlExpression sqlExpression4 = (SqlExpression)sqlFactory2.FunctionCall(clrType2, name2, (IEnumerable<SqlExpression>)sqlExpressionArray2, source2);
                    SqlFactory sqlFactory3 = this.sql;
                    Type clrType3 = typeof(string);
                    string name3 = "CONVERT";
                    SqlExpression[] sqlExpressionArray3 = new SqlExpression[2];
                    int index3 = 0;
                    SqlExpression sqlExpression5 = sqlExpression4;
                    sqlExpressionArray3[index3] = sqlExpression5;
                    int index4 = 1;
                    SqlExpression sqlExpression6 = sox.Args[0];
                    sqlExpressionArray3[index4] = sqlExpression6;
                    Expression source3 = sourceExpression;
                    SqlExpression sqlExpression7 = (SqlExpression)sqlFactory3.FunctionCall(clrType3, name3, (IEnumerable<SqlExpression>)sqlExpressionArray3, source3);
                    SqlFactory sqlFactory4 = this.sql;
                    Type clrType4 = typeof(string);
                    string name4 = "CONVERT";
                    SqlExpression[] sqlExpressionArray4 = new SqlExpression[2];
                    int index5 = 0;
                    SqlExpression sqlExpression8 = sqlExpression2;
                    sqlExpressionArray4[index5] = sqlExpression8;
                    int index6 = 1;
                    SqlExpression sqlExpression9 = sox.Args[1];
                    sqlExpressionArray4[index6] = sqlExpression9;
                    Expression source4 = sourceExpression;
                    SqlExpression sqlExpression10 = (SqlExpression)sqlFactory4.FunctionCall(clrType4, name4, (IEnumerable<SqlExpression>)sqlExpressionArray4, source4);
                    SqlFactory sqlFactory5 = this.sql;
                    Type clrType5 = typeof(string);
                    string name5 = "CONVERT";
                    SqlExpression[] sqlExpressionArray5 = new SqlExpression[2];
                    int index7 = 0;
                    SqlExpression sqlExpression11 = sqlExpression2;
                    sqlExpressionArray5[index7] = sqlExpression11;
                    int index8 = 1;
                    SqlExpression sqlExpression12 = sox.Args[2];
                    sqlExpressionArray5[index8] = sqlExpression12;
                    Expression source5 = sourceExpression;
                    SqlExpression sqlExpression13 = (SqlExpression)sqlFactory5.FunctionCall(clrType5, name5, (IEnumerable<SqlExpression>)sqlExpressionArray5, source5);
                    SqlExpression sqlExpression14 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, "DATETIME", sourceExpression);
                    if (sox.Args.Count == 3)
                    {
                        SqlFactory sqlFactory6 = this.sql;
                        SqlExpression[] sqlExpressionArray6 = new SqlExpression[5];
                        int index9 = 0;
                        SqlExpression sqlExpression15 = sqlExpression10;
                        sqlExpressionArray6[index9] = sqlExpression15;
                        int index10 = 1;
                        SqlExpression sqlExpression16 = this.sql.ValueFromObject((object)"/", false, sourceExpression);
                        sqlExpressionArray6[index10] = sqlExpression16;
                        int index11 = 2;
                        SqlExpression sqlExpression17 = sqlExpression13;
                        sqlExpressionArray6[index11] = sqlExpression17;
                        int index12 = 3;
                        SqlExpression sqlExpression18 = this.sql.ValueFromObject((object)"/", false, sourceExpression);
                        sqlExpressionArray6[index12] = sqlExpression18;
                        int index13 = 4;
                        SqlExpression sqlExpression19 = sqlExpression7;
                        sqlExpressionArray6[index13] = sqlExpression19;
                        SqlExpression sqlExpression20 = sqlFactory6.Concat(sqlExpressionArray6);
                        SqlFactory sqlFactory7 = this.sql;
                        Type clrType6 = typeof(DateTime);
                        string name6 = "CONVERT";
                        SqlExpression[] sqlExpressionArray7 = new SqlExpression[3];
                        int index14 = 0;
                        SqlExpression sqlExpression21 = sqlExpression14;
                        sqlExpressionArray7[index14] = sqlExpression21;
                        int index15 = 1;
                        SqlExpression sqlExpression22 = sqlExpression20;
                        sqlExpressionArray7[index15] = sqlExpression22;
                        int index16 = 2;
                        SqlExpression sqlExpression23 = this.sql.ValueFromObject((object)101, false, sourceExpression);
                        sqlExpressionArray7[index16] = sqlExpression23;
                        Expression source6 = sourceExpression;
                        return (SqlExpression)sqlFactory7.FunctionCall(clrType6, name6, (IEnumerable<SqlExpression>)sqlExpressionArray7, source6);
                    }
                    if (sox.Args.Count >= 6 && sox.Args[3].ClrType == typeof(int) && (sox.Args[4].ClrType == typeof(int) && sox.Args[5].ClrType == typeof(int)))
                    {
                        SqlFactory sqlFactory6 = this.sql;
                        Type clrType6 = typeof(string);
                        string name6 = "CONVERT";
                        SqlExpression[] sqlExpressionArray6 = new SqlExpression[2];
                        int index9 = 0;
                        SqlExpression sqlExpression15 = sqlExpression2;
                        sqlExpressionArray6[index9] = sqlExpression15;
                        int index10 = 1;
                        SqlExpression sqlExpression16 = sox.Args[3];
                        sqlExpressionArray6[index10] = sqlExpression16;
                        Expression source6 = sourceExpression;
                        SqlExpression sqlExpression17 = (SqlExpression)sqlFactory6.FunctionCall(clrType6, name6, (IEnumerable<SqlExpression>)sqlExpressionArray6, source6);
                        SqlFactory sqlFactory7 = this.sql;
                        Type clrType7 = typeof(string);
                        string name7 = "CONVERT";
                        SqlExpression[] sqlExpressionArray7 = new SqlExpression[2];
                        int index11 = 0;
                        SqlExpression sqlExpression18 = sqlExpression2;
                        sqlExpressionArray7[index11] = sqlExpression18;
                        int index12 = 1;
                        SqlExpression sqlExpression19 = sox.Args[4];
                        sqlExpressionArray7[index12] = sqlExpression19;
                        Expression source7 = sourceExpression;
                        SqlExpression sqlExpression20 = (SqlExpression)sqlFactory7.FunctionCall(clrType7, name7, (IEnumerable<SqlExpression>)sqlExpressionArray7, source7);
                        SqlFactory sqlFactory8 = this.sql;
                        Type clrType8 = typeof(string);
                        string name8 = "CONVERT";
                        SqlExpression[] sqlExpressionArray8 = new SqlExpression[2];
                        int index13 = 0;
                        SqlExpression sqlExpression21 = sqlExpression2;
                        sqlExpressionArray8[index13] = sqlExpression21;
                        int index14 = 1;
                        SqlExpression sqlExpression22 = sox.Args[5];
                        sqlExpressionArray8[index14] = sqlExpression22;
                        Expression source8 = sourceExpression;
                        SqlExpression sqlExpression23 = (SqlExpression)sqlFactory8.FunctionCall(clrType8, name8, (IEnumerable<SqlExpression>)sqlExpressionArray8, source8);
                        SqlFactory sqlFactory9 = this.sql;
                        SqlExpression[] sqlExpressionArray9 = new SqlExpression[5];
                        int index15 = 0;
                        SqlExpression sqlExpression24 = sqlExpression7;
                        sqlExpressionArray9[index15] = sqlExpression24;
                        int index16 = 1;
                        SqlExpression sqlExpression25 = this.sql.ValueFromObject((object)"-", false, sourceExpression);
                        sqlExpressionArray9[index16] = sqlExpression25;
                        int index17 = 2;
                        SqlExpression sqlExpression26 = sqlExpression10;
                        sqlExpressionArray9[index17] = sqlExpression26;
                        int index18 = 3;
                        SqlExpression sqlExpression27 = this.sql.ValueFromObject((object)"-", false, sourceExpression);
                        sqlExpressionArray9[index18] = sqlExpression27;
                        int index19 = 4;
                        SqlExpression sqlExpression28 = sqlExpression13;
                        sqlExpressionArray9[index19] = sqlExpression28;
                        SqlExpression sqlExpression29 = sqlFactory9.Concat(sqlExpressionArray9);
                        SqlFactory sqlFactory10 = this.sql;
                        SqlExpression[] sqlExpressionArray10 = new SqlExpression[5];
                        int index20 = 0;
                        SqlExpression sqlExpression30 = sqlExpression17;
                        sqlExpressionArray10[index20] = sqlExpression30;
                        int index21 = 1;
                        SqlExpression sqlExpression31 = this.sql.ValueFromObject((object)":", false, sourceExpression);
                        sqlExpressionArray10[index21] = sqlExpression31;
                        int index22 = 2;
                        SqlExpression sqlExpression32 = sqlExpression20;
                        sqlExpressionArray10[index22] = sqlExpression32;
                        int index23 = 3;
                        SqlExpression sqlExpression33 = this.sql.ValueFromObject((object)":", false, sourceExpression);
                        sqlExpressionArray10[index23] = sqlExpression33;
                        int index24 = 4;
                        SqlExpression sqlExpression34 = sqlExpression23;
                        sqlExpressionArray10[index24] = sqlExpression34;
                        SqlExpression sqlExpression35 = sqlFactory10.Concat(sqlExpressionArray10);
                        SqlFactory sqlFactory11 = this.sql;
                        SqlExpression[] sqlExpressionArray11 = new SqlExpression[3];
                        int index25 = 0;
                        SqlExpression sqlExpression36 = sqlExpression29;
                        sqlExpressionArray11[index25] = sqlExpression36;
                        int index26 = 1;
                        SqlExpression sqlExpression37 = this.sql.ValueFromObject((object)' ', false, sourceExpression);
                        sqlExpressionArray11[index26] = sqlExpression37;
                        int index27 = 2;
                        SqlExpression sqlExpression38 = sqlExpression35;
                        sqlExpressionArray11[index27] = sqlExpression38;
                        SqlExpression sqlExpression39 = sqlFactory11.Concat(sqlExpressionArray11);
                        if (sox.Args.Count == 6)
                        {
                            SqlFactory sqlFactory12 = this.sql;
                            Type clrType9 = typeof(DateTime);
                            string name9 = "CONVERT";
                            SqlExpression[] sqlExpressionArray12 = new SqlExpression[3];
                            int index28 = 0;
                            SqlExpression sqlExpression40 = sqlExpression14;
                            sqlExpressionArray12[index28] = sqlExpression40;
                            int index29 = 1;
                            SqlExpression sqlExpression41 = sqlExpression39;
                            sqlExpressionArray12[index29] = sqlExpression41;
                            int index30 = 2;
                            SqlExpression sqlExpression42 = this.sql.ValueFromObject((object)120, false, sourceExpression);
                            sqlExpressionArray12[index30] = sqlExpression42;
                            Expression source9 = sourceExpression;
                            return (SqlExpression)sqlFactory12.FunctionCall(clrType9, name9, (IEnumerable<SqlExpression>)sqlExpressionArray12, source9);
                        }
                        if (sox.Args.Count == 7 && sox.Args[6].ClrType == typeof(int))
                        {
                            SqlFactory sqlFactory12 = this.sql;
                            Type clrType9 = typeof(string);
                            string name9 = "CONVERT";
                            SqlExpression[] sqlExpressionArray12 = new SqlExpression[2];
                            int index28 = 0;
                            SqlExpression sqlExpression40 = sqlExpression4;
                            sqlExpressionArray12[index28] = sqlExpression40;
                            int index29 = 1;
                            SqlFactory sqlFactory13 = this.sql;
                            SqlExpression[] sqlExpressionArray13 = new SqlExpression[2];
                            int index30 = 0;
                            SqlExpression sqlExpression41 = this.sql.ValueFromObject((object)1000, false, sourceExpression);
                            sqlExpressionArray13[index30] = sqlExpression41;
                            int index31 = 1;
                            SqlExpression sqlExpression42 = sox.Args[6];
                            sqlExpressionArray13[index31] = sqlExpression42;
                            SqlExpression sqlExpression43 = sqlFactory13.Add(sqlExpressionArray13);
                            sqlExpressionArray12[index29] = sqlExpression43;
                            Expression source9 = sourceExpression;
                            SqlExpression sqlExpression44 = (SqlExpression)sqlFactory12.FunctionCall(clrType9, name9, (IEnumerable<SqlExpression>)sqlExpressionArray12, source9);
                            SqlExpression sqlExpression45;
                            if (this.providerMode == SqlProvider.ProviderMode.SqlCE)
                            {
                                SqlFactory sqlFactory14 = this.sql;
                                Type clrType10 = typeof(int);
                                string name10 = "LEN";
                                SqlExpression[] sqlExpressionArray14 = new SqlExpression[1];
                                int index32 = 0;
                                SqlExpression sqlExpression46 = sqlExpression44;
                                sqlExpressionArray14[index32] = sqlExpression46;
                                Expression source10 = sourceExpression;
                                SqlExpression sqlExpression47 = (SqlExpression)this.sql.Binary(SqlNodeType.Sub, (SqlExpression)sqlFactory14.FunctionCall(clrType10, name10, (IEnumerable<SqlExpression>)sqlExpressionArray14, source10), this.sql.ValueFromObject((object)2, false, sourceExpression));
                                SqlFactory sqlFactory15 = this.sql;
                                Type clrType11 = typeof(string);
                                string name11 = "SUBSTRING";
                                SqlExpression[] sqlExpressionArray15 = new SqlExpression[3];
                                int index33 = 0;
                                SqlExpression sqlExpression48 = sqlExpression44;
                                sqlExpressionArray15[index33] = sqlExpression48;
                                int index34 = 1;
                                SqlExpression sqlExpression49 = sqlExpression47;
                                sqlExpressionArray15[index34] = sqlExpression49;
                                int index35 = 2;
                                SqlExpression sqlExpression50 = this.sql.ValueFromObject((object)3, false, sourceExpression);
                                sqlExpressionArray15[index35] = sqlExpression50;
                                Expression source11 = sourceExpression;
                                sqlExpression45 = (SqlExpression)sqlFactory15.FunctionCall(clrType11, name11, (IEnumerable<SqlExpression>)sqlExpressionArray15, source11);
                            }
                            else
                            {
                                SqlFactory sqlFactory14 = this.sql;
                                Type clrType10 = typeof(string);
                                string name10 = "RIGHT";
                                SqlExpression[] sqlExpressionArray14 = new SqlExpression[2];
                                int index32 = 0;
                                SqlExpression sqlExpression46 = sqlExpression44;
                                sqlExpressionArray14[index32] = sqlExpression46;
                                int index33 = 1;
                                SqlExpression sqlExpression47 = this.sql.ValueFromObject((object)3, false, sourceExpression);
                                sqlExpressionArray14[index33] = sqlExpression47;
                                Expression source10 = sourceExpression;
                                sqlExpression45 = (SqlExpression)sqlFactory14.FunctionCall(clrType10, name10, (IEnumerable<SqlExpression>)sqlExpressionArray14, source10);
                            }
                            SqlFactory sqlFactory16 = this.sql;
                            SqlExpression[] sqlExpressionArray16 = new SqlExpression[3];
                            int index36 = 0;
                            SqlExpression sqlExpression51 = sqlExpression39;
                            sqlExpressionArray16[index36] = sqlExpression51;
                            int index37 = 1;
                            SqlExpression sqlExpression52 = this.sql.ValueFromObject((object)'.', false, sourceExpression);
                            sqlExpressionArray16[index37] = sqlExpression52;
                            int index38 = 2;
                            SqlExpression sqlExpression53 = sqlExpression45;
                            sqlExpressionArray16[index38] = sqlExpression53;
                            SqlExpression sqlExpression54 = sqlFactory16.Concat(sqlExpressionArray16);
                            SqlFactory sqlFactory17 = this.sql;
                            Type clrType12 = typeof(DateTime);
                            string name12 = "CONVERT";
                            SqlExpression[] sqlExpressionArray17 = new SqlExpression[3];
                            int index39 = 0;
                            SqlExpression sqlExpression55 = sqlExpression14;
                            sqlExpressionArray17[index39] = sqlExpression55;
                            int index40 = 1;
                            SqlExpression sqlExpression56 = sqlExpression54;
                            sqlExpressionArray17[index40] = sqlExpression56;
                            int index41 = 2;
                            SqlExpression sqlExpression57 = this.sql.ValueFromObject((object)121, false, sourceExpression);
                            sqlExpressionArray17[index41] = sqlExpression57;
                            Expression source12 = sourceExpression;
                            return (SqlExpression)sqlFactory17.FunctionCall(clrType12, name12, (IEnumerable<SqlExpression>)sqlExpressionArray17, source12);
                        }
                    }
                }
                throw Error.UnsupportedDateTimeConstructorForm();
            }

            private SqlExpression TranslateNewDateTimeOffset(SqlNew sox)
            {
                Expression sourceExpression = sox.SourceExpression;
                if (sox.ClrType == typeof(DateTimeOffset))
                {
                    if (sox.Args.Count == 1 && sox.Args[0].ClrType == typeof(DateTime))
                    {
                        SqlFactory sqlFactory = this.sql;
                        Type clrType = typeof(DateTimeOffset);
                        string name = "TODATETIMEOFFSET";
                        SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                        int index1 = 0;
                        SqlExpression sqlExpression1 = sox.Args[0];
                        sqlExpressionArray[index1] = sqlExpression1;
                        int index2 = 1;
                        SqlExpression sqlExpression2 = this.sql.ValueFromObject((object)0, false, sourceExpression);
                        sqlExpressionArray[index2] = sqlExpression2;
                        Expression source = sourceExpression;
                        return (SqlExpression)sqlFactory.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                    }
                    if (sox.Args.Count == 2 && sox.Args[0].ClrType == typeof(DateTime) && sox.Args[1].ClrType == typeof(TimeSpan))
                    {
                        SqlFactory sqlFactory = this.sql;
                        Type clrType = typeof(DateTimeOffset);
                        string name = "TODATETIMEOFFSET";
                        SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                        int index1 = 0;
                        SqlExpression sqlExpression1 = sox.Args[0];
                        sqlExpressionArray[index1] = sqlExpression1;
                        int index2 = 1;
                        SqlExpression sqlExpression2 = this.sql.ConvertToInt(this.sql.ConvertToBigint(this.sql.Divide(this.sql.ConvertTimeToDouble(sox.Args[1]), 600000000L)));
                        sqlExpressionArray[index2] = sqlExpression2;
                        Expression source = sourceExpression;
                        return (SqlExpression)sqlFactory.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                    }
                    if (sox.Args.Count >= 7 && sox.Args[0].ClrType == typeof(int) && (sox.Args[1].ClrType == typeof(int) && sox.Args[2].ClrType == typeof(int)) && (sox.Args[3].ClrType == typeof(int) && sox.Args[4].ClrType == typeof(int) && sox.Args[5].ClrType == typeof(int)))
                    {
                        SqlFactory sqlFactory1 = this.sql;
                        Type clrType1 = typeof(void);
                        string name1 = "NCHAR";
                        SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
                        int index1 = 0;
                        SqlExpression sqlExpression1 = this.sql.ValueFromObject((object)2, false, sourceExpression);
                        sqlExpressionArray1[index1] = sqlExpression1;
                        Expression source1 = sourceExpression;
                        SqlExpression sqlExpression2 = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name1, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                        SqlFactory sqlFactory2 = this.sql;
                        Type clrType2 = typeof(void);
                        string name2 = "NCHAR";
                        SqlExpression[] sqlExpressionArray2 = new SqlExpression[1];
                        int index2 = 0;
                        SqlExpression sqlExpression3 = this.sql.ValueFromObject((object)4, false, sourceExpression);
                        sqlExpressionArray2[index2] = sqlExpression3;
                        Expression source2 = sourceExpression;
                        SqlExpression sqlExpression4 = (SqlExpression)sqlFactory2.FunctionCall(clrType2, name2, (IEnumerable<SqlExpression>)sqlExpressionArray2, source2);
                        SqlFactory sqlFactory3 = this.sql;
                        Type clrType3 = typeof(void);
                        string name3 = "NCHAR";
                        SqlExpression[] sqlExpressionArray3 = new SqlExpression[1];
                        int index3 = 0;
                        SqlExpression sqlExpression5 = this.sql.ValueFromObject((object)5, false, sourceExpression);
                        sqlExpressionArray3[index3] = sqlExpression5;
                        Expression source3 = sourceExpression;
                        SqlExpression sqlExpression6 = (SqlExpression)sqlFactory3.FunctionCall(clrType3, name3, (IEnumerable<SqlExpression>)sqlExpressionArray3, source3);
                        SqlFactory sqlFactory4 = this.sql;
                        Type clrType4 = typeof(string);
                        string name4 = "CONVERT";
                        SqlExpression[] sqlExpressionArray4 = new SqlExpression[2];
                        int index4 = 0;
                        SqlExpression sqlExpression7 = sqlExpression6;
                        sqlExpressionArray4[index4] = sqlExpression7;
                        int index5 = 1;
                        SqlFactory sqlFactory5 = this.sql;
                        SqlExpression[] sqlExpressionArray5 = new SqlExpression[2];
                        int index6 = 0;
                        SqlExpression sqlExpression8 = this.sql.ValueFromObject((object)10000, false, sourceExpression);
                        sqlExpressionArray5[index6] = sqlExpression8;
                        int index7 = 1;
                        SqlExpression sqlExpression9 = sox.Args[0];
                        sqlExpressionArray5[index7] = sqlExpression9;
                        SqlExpression sqlExpression10 = sqlFactory5.Add(sqlExpressionArray5);
                        sqlExpressionArray4[index5] = sqlExpression10;
                        Expression source4 = sourceExpression;
                        SqlExpression sqlExpression11 = (SqlExpression)sqlFactory4.FunctionCall(clrType4, name4, (IEnumerable<SqlExpression>)sqlExpressionArray4, source4);
                        SqlFactory sqlFactory6 = this.sql;
                        Type clrType5 = typeof(string);
                        string name5 = "RIGHT";
                        SqlExpression[] sqlExpressionArray6 = new SqlExpression[2];
                        int index8 = 0;
                        SqlExpression sqlExpression12 = sqlExpression11;
                        sqlExpressionArray6[index8] = sqlExpression12;
                        int index9 = 1;
                        SqlExpression sqlExpression13 = this.sql.ValueFromObject((object)4, false, sourceExpression);
                        sqlExpressionArray6[index9] = sqlExpression13;
                        Expression source5 = sourceExpression;
                        SqlExpression sqlExpression14 = (SqlExpression)sqlFactory6.FunctionCall(clrType5, name5, (IEnumerable<SqlExpression>)sqlExpressionArray6, source5);
                        SqlFactory sqlFactory7 = this.sql;
                        Type clrType6 = typeof(string);
                        string name6 = "CONVERT";
                        SqlExpression[] sqlExpressionArray7 = new SqlExpression[2];
                        int index10 = 0;
                        SqlExpression sqlExpression15 = sqlExpression2;
                        sqlExpressionArray7[index10] = sqlExpression15;
                        int index11 = 1;
                        SqlExpression sqlExpression16 = sox.Args[1];
                        sqlExpressionArray7[index11] = sqlExpression16;
                        Expression source6 = sourceExpression;
                        SqlExpression sqlExpression17 = (SqlExpression)sqlFactory7.FunctionCall(clrType6, name6, (IEnumerable<SqlExpression>)sqlExpressionArray7, source6);
                        SqlFactory sqlFactory8 = this.sql;
                        Type clrType7 = typeof(string);
                        string name7 = "CONVERT";
                        SqlExpression[] sqlExpressionArray8 = new SqlExpression[2];
                        int index12 = 0;
                        SqlExpression sqlExpression18 = sqlExpression2;
                        sqlExpressionArray8[index12] = sqlExpression18;
                        int index13 = 1;
                        SqlExpression sqlExpression19 = sox.Args[2];
                        sqlExpressionArray8[index13] = sqlExpression19;
                        Expression source7 = sourceExpression;
                        SqlExpression sqlExpression20 = (SqlExpression)sqlFactory8.FunctionCall(clrType7, name7, (IEnumerable<SqlExpression>)sqlExpressionArray8, source7);
                        SqlFactory sqlFactory9 = this.sql;
                        Type clrType8 = typeof(string);
                        string name8 = "CONVERT";
                        SqlExpression[] sqlExpressionArray9 = new SqlExpression[2];
                        int index14 = 0;
                        SqlExpression sqlExpression21 = sqlExpression2;
                        sqlExpressionArray9[index14] = sqlExpression21;
                        int index15 = 1;
                        SqlExpression sqlExpression22 = sox.Args[3];
                        sqlExpressionArray9[index15] = sqlExpression22;
                        Expression source8 = sourceExpression;
                        SqlExpression sqlExpression23 = (SqlExpression)sqlFactory9.FunctionCall(clrType8, name8, (IEnumerable<SqlExpression>)sqlExpressionArray9, source8);
                        SqlFactory sqlFactory10 = this.sql;
                        Type clrType9 = typeof(string);
                        string name9 = "CONVERT";
                        SqlExpression[] sqlExpressionArray10 = new SqlExpression[2];
                        int index16 = 0;
                        SqlExpression sqlExpression24 = sqlExpression2;
                        sqlExpressionArray10[index16] = sqlExpression24;
                        int index17 = 1;
                        SqlExpression sqlExpression25 = sox.Args[4];
                        sqlExpressionArray10[index17] = sqlExpression25;
                        Expression source9 = sourceExpression;
                        SqlExpression sqlExpression26 = (SqlExpression)sqlFactory10.FunctionCall(clrType9, name9, (IEnumerable<SqlExpression>)sqlExpressionArray10, source9);
                        SqlFactory sqlFactory11 = this.sql;
                        Type clrType10 = typeof(string);
                        string name10 = "CONVERT";
                        SqlExpression[] sqlExpressionArray11 = new SqlExpression[2];
                        int index18 = 0;
                        SqlExpression sqlExpression27 = sqlExpression2;
                        sqlExpressionArray11[index18] = sqlExpression27;
                        int index19 = 1;
                        SqlExpression sqlExpression28 = sox.Args[5];
                        sqlExpressionArray11[index19] = sqlExpression28;
                        Expression source10 = sourceExpression;
                        SqlExpression sqlExpression29 = (SqlExpression)sqlFactory11.FunctionCall(clrType10, name10, (IEnumerable<SqlExpression>)sqlExpressionArray11, source10);
                        SqlFactory sqlFactory12 = this.sql;
                        SqlExpression[] sqlExpressionArray12 = new SqlExpression[5];
                        int index20 = 0;
                        SqlExpression sqlExpression30 = sqlExpression14;
                        sqlExpressionArray12[index20] = sqlExpression30;
                        int index21 = 1;
                        SqlExpression sqlExpression31 = this.sql.ValueFromObject((object)"-", false, sourceExpression);
                        sqlExpressionArray12[index21] = sqlExpression31;
                        int index22 = 2;
                        SqlExpression sqlExpression32 = sqlExpression17;
                        sqlExpressionArray12[index22] = sqlExpression32;
                        int index23 = 3;
                        SqlExpression sqlExpression33 = this.sql.ValueFromObject((object)"-", false, sourceExpression);
                        sqlExpressionArray12[index23] = sqlExpression33;
                        int index24 = 4;
                        SqlExpression sqlExpression34 = sqlExpression20;
                        sqlExpressionArray12[index24] = sqlExpression34;
                        SqlExpression sqlExpression35 = sqlFactory12.Concat(sqlExpressionArray12);
                        SqlFactory sqlFactory13 = this.sql;
                        SqlExpression[] sqlExpressionArray13 = new SqlExpression[5];
                        int index25 = 0;
                        SqlExpression sqlExpression36 = sqlExpression23;
                        sqlExpressionArray13[index25] = sqlExpression36;
                        int index26 = 1;
                        SqlExpression sqlExpression37 = this.sql.ValueFromObject((object)":", false, sourceExpression);
                        sqlExpressionArray13[index26] = sqlExpression37;
                        int index27 = 2;
                        SqlExpression sqlExpression38 = sqlExpression26;
                        sqlExpressionArray13[index27] = sqlExpression38;
                        int index28 = 3;
                        SqlExpression sqlExpression39 = this.sql.ValueFromObject((object)":", false, sourceExpression);
                        sqlExpressionArray13[index28] = sqlExpression39;
                        int index29 = 4;
                        SqlExpression sqlExpression40 = sqlExpression29;
                        sqlExpressionArray13[index29] = sqlExpression40;
                        SqlExpression sqlExpression41 = sqlFactory13.Concat(sqlExpressionArray13);
                        SqlExpression sqlExpression42 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, "DATETIMEOFFSET", sourceExpression);
                        int index30;
                        SqlExpression sqlExpression43;
                        if (sox.Args.Count == 7 && sox.Args[6].ClrType == typeof(TimeSpan))
                        {
                            index30 = 6;
                            SqlFactory sqlFactory14 = this.sql;
                            SqlExpression[] sqlExpressionArray14 = new SqlExpression[3];
                            int index31 = 0;
                            SqlExpression sqlExpression44 = sqlExpression35;
                            sqlExpressionArray14[index31] = sqlExpression44;
                            int index32 = 1;
                            SqlExpression sqlExpression45 = this.sql.ValueFromObject((object)' ', false, sourceExpression);
                            sqlExpressionArray14[index32] = sqlExpression45;
                            int index33 = 2;
                            SqlExpression sqlExpression46 = sqlExpression41;
                            sqlExpressionArray14[index33] = sqlExpression46;
                            SqlExpression sqlExpression47 = sqlFactory14.Concat(sqlExpressionArray14);
                            SqlFactory sqlFactory15 = this.sql;
                            Type clrType11 = typeof(DateTimeOffset);
                            string name11 = "CONVERT";
                            SqlExpression[] sqlExpressionArray15 = new SqlExpression[3];
                            int index34 = 0;
                            SqlExpression sqlExpression48 = sqlExpression42;
                            sqlExpressionArray15[index34] = sqlExpression48;
                            int index35 = 1;
                            SqlExpression sqlExpression49 = sqlExpression47;
                            sqlExpressionArray15[index35] = sqlExpression49;
                            int index36 = 2;
                            SqlExpression sqlExpression50 = this.sql.ValueFromObject((object)120, false, sourceExpression);
                            sqlExpressionArray15[index36] = sqlExpression50;
                            Expression source11 = sourceExpression;
                            sqlExpression43 = (SqlExpression)sqlFactory15.FunctionCall(clrType11, name11, (IEnumerable<SqlExpression>)sqlExpressionArray15, source11);
                        }
                        else
                        {
                            if (sox.Args.Count != 8 || !(sox.Args[6].ClrType == typeof(int)) || !(sox.Args[7].ClrType == typeof(TimeSpan)))
                                throw Error.UnsupportedDateTimeOffsetConstructorForm();
                            index30 = 7;
                            SqlFactory sqlFactory14 = this.sql;
                            Type clrType11 = typeof(string);
                            string name11 = "CONVERT";
                            SqlExpression[] sqlExpressionArray14 = new SqlExpression[2];
                            int index31 = 0;
                            SqlExpression sqlExpression44 = sqlExpression4;
                            sqlExpressionArray14[index31] = sqlExpression44;
                            int index32 = 1;
                            SqlFactory sqlFactory15 = this.sql;
                            SqlExpression[] sqlExpressionArray15 = new SqlExpression[2];
                            int index33 = 0;
                            SqlExpression sqlExpression45 = this.sql.ValueFromObject((object)1000, false, sourceExpression);
                            sqlExpressionArray15[index33] = sqlExpression45;
                            int index34 = 1;
                            SqlExpression sqlExpression46 = sox.Args[6];
                            sqlExpressionArray15[index34] = sqlExpression46;
                            SqlExpression sqlExpression47 = sqlFactory15.Add(sqlExpressionArray15);
                            sqlExpressionArray14[index32] = sqlExpression47;
                            Expression source11 = sourceExpression;
                            SqlExpression sqlExpression48 = (SqlExpression)sqlFactory14.FunctionCall(clrType11, name11, (IEnumerable<SqlExpression>)sqlExpressionArray14, source11);
                            SqlFactory sqlFactory16 = this.sql;
                            Type clrType12 = typeof(string);
                            string name12 = "RIGHT";
                            SqlExpression[] sqlExpressionArray16 = new SqlExpression[2];
                            int index35 = 0;
                            SqlExpression sqlExpression49 = sqlExpression48;
                            sqlExpressionArray16[index35] = sqlExpression49;
                            int index36 = 1;
                            SqlExpression sqlExpression50 = this.sql.ValueFromObject((object)3, false, sourceExpression);
                            sqlExpressionArray16[index36] = sqlExpression50;
                            Expression source12 = sourceExpression;
                            SqlExpression sqlExpression51 = (SqlExpression)sqlFactory16.FunctionCall(clrType12, name12, (IEnumerable<SqlExpression>)sqlExpressionArray16, source12);
                            SqlFactory sqlFactory17 = this.sql;
                            SqlExpression[] sqlExpressionArray17 = new SqlExpression[5];
                            int index37 = 0;
                            SqlExpression sqlExpression52 = sqlExpression35;
                            sqlExpressionArray17[index37] = sqlExpression52;
                            int index38 = 1;
                            SqlExpression sqlExpression53 = this.sql.ValueFromObject((object)' ', false, sourceExpression);
                            sqlExpressionArray17[index38] = sqlExpression53;
                            int index39 = 2;
                            SqlExpression sqlExpression54 = sqlExpression41;
                            sqlExpressionArray17[index39] = sqlExpression54;
                            int index40 = 3;
                            SqlExpression sqlExpression55 = this.sql.ValueFromObject((object)'.', false, sourceExpression);
                            sqlExpressionArray17[index40] = sqlExpression55;
                            int index41 = 4;
                            SqlExpression sqlExpression56 = sqlExpression51;
                            sqlExpressionArray17[index41] = sqlExpression56;
                            SqlExpression sqlExpression57 = sqlFactory17.Concat(sqlExpressionArray17);
                            SqlFactory sqlFactory18 = this.sql;
                            Type clrType13 = typeof(DateTimeOffset);
                            string name13 = "CONVERT";
                            SqlExpression[] sqlExpressionArray18 = new SqlExpression[3];
                            int index42 = 0;
                            SqlExpression sqlExpression58 = sqlExpression42;
                            sqlExpressionArray18[index42] = sqlExpression58;
                            int index43 = 1;
                            SqlExpression sqlExpression59 = sqlExpression57;
                            sqlExpressionArray18[index43] = sqlExpression59;
                            int index44 = 2;
                            SqlExpression sqlExpression60 = this.sql.ValueFromObject((object)121, false, sourceExpression);
                            sqlExpressionArray18[index44] = sqlExpression60;
                            Expression source13 = sourceExpression;
                            sqlExpression43 = (SqlExpression)sqlFactory18.FunctionCall(clrType13, name13, (IEnumerable<SqlExpression>)sqlExpressionArray18, source13);
                        }
                        SqlFactory sqlFactory19 = this.sql;
                        Type clrType14 = typeof(DateTimeOffset);
                        string name14 = "TODATETIMEOFFSET";
                        SqlExpression[] sqlExpressionArray19 = new SqlExpression[2];
                        int index45 = 0;
                        SqlExpression sqlExpression61 = sqlExpression43;
                        sqlExpressionArray19[index45] = sqlExpression61;
                        int index46 = 1;
                        SqlExpression sqlExpression62 = this.sql.ConvertToInt(this.sql.ConvertToBigint(this.sql.Divide(this.sql.ConvertTimeToDouble(sox.Args[index30]), 600000000L)));
                        sqlExpressionArray19[index46] = sqlExpression62;
                        Expression source14 = sourceExpression;
                        return (SqlExpression)sqlFactory19.FunctionCall(clrType14, name14, (IEnumerable<SqlExpression>)sqlExpressionArray19, source14);
                    }
                }
                throw Error.UnsupportedDateTimeOffsetConstructorForm();
            }

            private SqlExpression TranslateNewTimeSpan(SqlNew sox)
            {
                if (sox.Args.Count == 1)
                    return this.sql.ConvertTo(typeof(TimeSpan), sox.Args[0]);
                if (sox.Args.Count == 3)
                {
                    SqlExpression expr1 = this.sql.ConvertToBigint(sox.Args[0]);
                    SqlExpression expr2 = this.sql.ConvertToBigint(sox.Args[1]);
                    SqlExpression expr3 = this.sql.ConvertToBigint(sox.Args[2]);
                    SqlExpression sqlExpression1 = this.sql.Multiply(expr1, 36000000000L);
                    SqlExpression sqlExpression2 = this.sql.Multiply(expr2, 600000000L);
                    SqlExpression sqlExpression3 = this.sql.Multiply(expr3, 10000000L);
                    SqlFactory sqlFactory1 = this.sql;
                    Type clrType = typeof(TimeSpan);
                    SqlFactory sqlFactory2 = this.sql;
                    SqlExpression[] sqlExpressionArray = new SqlExpression[3];
                    int index1 = 0;
                    SqlExpression sqlExpression4 = sqlExpression1;
                    sqlExpressionArray[index1] = sqlExpression4;
                    int index2 = 1;
                    SqlExpression sqlExpression5 = sqlExpression2;
                    sqlExpressionArray[index2] = sqlExpression5;
                    int index3 = 2;
                    SqlExpression sqlExpression6 = sqlExpression3;
                    sqlExpressionArray[index3] = sqlExpression6;
                    SqlExpression expr4 = sqlFactory2.Add(sqlExpressionArray);
                    return sqlFactory1.ConvertTo(clrType, expr4);
                }
                SqlExpression expr5 = this.sql.ConvertToBigint(sox.Args[0]);
                SqlExpression expr6 = this.sql.ConvertToBigint(sox.Args[1]);
                SqlExpression expr7 = this.sql.ConvertToBigint(sox.Args[2]);
                SqlExpression expr8 = this.sql.ConvertToBigint(sox.Args[3]);
                SqlExpression sqlExpression7 = this.sql.Multiply(expr5, 864000000000L);
                SqlExpression sqlExpression8 = this.sql.Multiply(expr6, 36000000000L);
                SqlExpression sqlExpression9 = this.sql.Multiply(expr7, 600000000L);
                SqlExpression sqlExpression10 = this.sql.Multiply(expr8, 10000000L);
                SqlFactory sqlFactory3 = this.sql;
                SqlExpression[] sqlExpressionArray1 = new SqlExpression[4];
                int index4 = 0;
                SqlExpression sqlExpression11 = sqlExpression7;
                sqlExpressionArray1[index4] = sqlExpression11;
                int index5 = 1;
                SqlExpression sqlExpression12 = sqlExpression8;
                sqlExpressionArray1[index5] = sqlExpression12;
                int index6 = 2;
                SqlExpression sqlExpression13 = sqlExpression9;
                sqlExpressionArray1[index6] = sqlExpression13;
                int index7 = 3;
                SqlExpression sqlExpression14 = sqlExpression10;
                sqlExpressionArray1[index7] = sqlExpression14;
                SqlExpression expr9 = sqlFactory3.Add(sqlExpressionArray1);
                if (sox.Args.Count == 4)
                    return this.sql.ConvertTo(typeof(TimeSpan), expr9);
                if (sox.Args.Count != 5)
                    throw Error.UnsupportedTimeSpanConstructorForm();
                SqlExpression sqlExpression15 = this.sql.Multiply(this.sql.ConvertToBigint(sox.Args[4]), 10000L);
                SqlFactory sqlFactory4 = this.sql;
                Type clrType1 = typeof(TimeSpan);
                SqlFactory sqlFactory5 = this.sql;
                SqlExpression[] sqlExpressionArray2 = new SqlExpression[2];
                int index8 = 0;
                SqlExpression sqlExpression16 = expr9;
                sqlExpressionArray2[index8] = sqlExpression16;
                int index9 = 1;
                SqlExpression sqlExpression17 = sqlExpression15;
                sqlExpressionArray2[index9] = sqlExpression17;
                SqlExpression expr10 = sqlFactory5.Add(sqlExpressionArray2);
                return sqlFactory4.ConvertTo(clrType1, expr10);
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                Type declaringType = mc.Method.DeclaringType;
                Expression sourceExpression = mc.SourceExpression;
                SqlExpression sqlExpression = (SqlExpression)null;
                mc.Object = this.VisitExpression(mc.Object);
                int index = 0;
                for (int count = mc.Arguments.Count; index < count; ++index)
                    mc.Arguments[index] = this.VisitExpression(mc.Arguments[index]);
                if (mc.Method.IsStatic)
                {
                    if (mc.Method.Name == "op_Explicit" || mc.Method.Name == "op_Implicit")
                    {
                        if (mc.SqlType.CanBeColumn && mc.Arguments[0].SqlType.CanBeColumn)
                            sqlExpression = this.sql.ConvertTo(mc.ClrType, mc.Arguments[0]);
                    }
                    else if (mc.Method.Name == "Compare" && mc.Arguments.Count == 2 && mc.Method.ReturnType == typeof(int))
                        sqlExpression = this.CreateComparison(mc.Arguments[0], mc.Arguments[1], mc.SourceExpression);
                    else if (declaringType == typeof(Math))
                        sqlExpression = this.TranslateMathMethod(mc);
                    else if (declaringType == typeof(string))
                        sqlExpression = this.TranslateStringStaticMethod(mc);
                    else if (declaringType == typeof(Convert))
                        sqlExpression = this.TranslateConvertStaticMethod(mc);
                    else if (declaringType == typeof(SqlMethods))
                        sqlExpression = this.TranslateSqlMethodsMethod(mc);
                    else if (declaringType == typeof(Decimal))
                    {
                        sqlExpression = this.TranslateDecimalMethod(mc);
                    }
                    else
                    {
                        if (PostBindDotNetConverter.IsVbConversionMethod(mc))
                            return this.TranslateVbConversionMethod(mc);
                        if (PostBindDotNetConverter.IsVbCompareString(mc))
                            return this.TranslateVbCompareString(mc);
                        if (PostBindDotNetConverter.IsVbLike(mc))
                            return this.TranslateVbLikeString(mc);
                    }
                    if (sqlExpression != null)
                        return sqlExpression;
                }
                else
                {
                    if (mc.Method.Name == "Equals" && mc.Arguments.Count == 1)
                        return (SqlExpression)this.sql.Binary(SqlNodeType.EQ, mc.Object, mc.Arguments[0]);
                    if (mc.Method.Name == "GetValueOrDefault" && mc.Method.DeclaringType.IsGenericType && mc.Method.DeclaringType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return this.TranslateGetValueOrDefaultMethod(mc);
                    if (mc.Method.Name == "ToString" && mc.Arguments.Count == 0)
                    {
                        SqlExpression @object = mc.Object;
                        if (!@object.SqlType.IsRuntimeOnlyType)
                            return this.sql.ConvertTo(typeof(string), @object);
                        throw Error.ToStringOnlySupportedForPrimitiveTypes();
                    }
                    if (declaringType == typeof(string))
                        return this.TranslateStringMethod(mc);
                    if (declaringType == typeof(TimeSpan))
                        sqlExpression = this.TranslateTimeSpanInstanceMethod(mc);
                    else if (declaringType == typeof(DateTime))
                        sqlExpression = this.TranslateDateTimeInstanceMethod(mc);
                    else if (declaringType == typeof(DateTimeOffset))
                        sqlExpression = this.TranslateDateTimeOffsetInstanceMethod(mc);
                    if (sqlExpression != null)
                        return sqlExpression;
                }
                throw PostBindDotNetConverter.Visitor.GetMethodSupportException(mc);
            }

            internal static Exception GetMethodSupportException(SqlMethodCall mc)
            {
                if (PostBindDotNetConverter.GetMethodSupport(mc) == PostBindDotNetConverter.MethodSupport.MethodGroup)
                    return Error.MethodFormHasNoSupportConversionToSql((object)mc.Method.Name, (object)mc.Method);
                return Error.MethodHasNoSupportConversionToSql((object)mc.Method);
            }

            private SqlExpression TranslateGetValueOrDefaultMethod(SqlMethodCall mc)
            {
                if (mc.Arguments.Count != 0)
                    return (SqlExpression)this.sql.Binary(SqlNodeType.Coalesce, mc.Object, mc.Arguments[0]);
                Type type = mc.Object.ClrType.GetGenericArguments()[0];
                return (SqlExpression)this.sql.Binary(SqlNodeType.Coalesce, mc.Object, this.sql.ValueFromObject(Activator.CreateInstance(type), mc.SourceExpression));
            }

            private SqlExpression TranslateSqlMethodsMethod(SqlMethodCall mc)
            {
                Expression sourceExpression = mc.SourceExpression;
                SqlExpression sqlExpression1 = (SqlExpression)null;
                string name1 = mc.Method.Name;
                if (name1.StartsWith("DateDiff", StringComparison.Ordinal) && mc.Arguments.Count == 2)
                {
                    foreach (string name2 in PostBindDotNetConverter.dateParts)
                    {
                        if (mc.Method.Name == "DateDiff" + name2)
                        {
                            SqlExpression sqlExpression2 = mc.Arguments[0];
                            SqlExpression sqlExpression3 = mc.Arguments[1];
                            SqlExpression sqlExpression4 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, name2, sourceExpression);
                            SqlFactory sqlFactory = this.sql;
                            Type clrType = typeof(int);
                            string name3 = "DATEDIFF";
                            SqlExpression[] sqlExpressionArray = new SqlExpression[3];
                            int index1 = 0;
                            SqlExpression sqlExpression5 = sqlExpression4;
                            sqlExpressionArray[index1] = sqlExpression5;
                            int index2 = 1;
                            SqlExpression sqlExpression6 = sqlExpression2;
                            sqlExpressionArray[index2] = sqlExpression6;
                            int index3 = 2;
                            SqlExpression sqlExpression7 = sqlExpression3;
                            sqlExpressionArray[index3] = sqlExpression7;
                            Expression source = sourceExpression;
                            return (SqlExpression)sqlFactory.FunctionCall(clrType, name3, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                        }
                    }
                }
                else if (name1 == "Like")
                {
                    if (mc.Arguments.Count == 2)
                        return (SqlExpression)this.sql.Like(mc.Arguments[0], mc.Arguments[1], (SqlExpression)null, sourceExpression);
                    if (mc.Arguments.Count == 3)
                        return (SqlExpression)this.sql.Like(mc.Arguments[0], mc.Arguments[1], this.sql.ConvertTo(typeof(string), mc.Arguments[2]), sourceExpression);
                }
                else if (name1 == "RawLength")
                    return this.sql.DATALENGTH(mc.Arguments[0]);
                return sqlExpression1;
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

            private SqlExpression TranslateDateTimeInstanceMethod(SqlMethodCall mc)
            {
                SqlExpression sqlExpression1 = (SqlExpression)null;
                Expression sourceExpression = mc.SourceExpression;
                if (mc.Method.Name == "CompareTo")
                {
                    sqlExpression1 = this.CreateComparison(mc.Object, mc.Arguments[0], sourceExpression);
                }
                else
                {
                    if (mc.Method.Name == "Add" && mc.Arguments.Count == 1 && mc.Arguments[0].ClrType == typeof(TimeSpan) || mc.Method.Name == "AddTicks")
                    {
                        SqlExpression sqlTicks = mc.Arguments[0];
                        if (SqlFactory.IsSqlTimeType(mc.Arguments[0]))
                        {
                            SqlExpression expr1 = this.sql.DATEPART("NANOSECOND", mc.Arguments[0]);
                            SqlExpression expr2 = this.sql.DATEPART("SECOND", mc.Arguments[0]);
                            SqlExpression expr3 = this.sql.DATEPART("MINUTE", mc.Arguments[0]);
                            SqlExpression expr4 = this.sql.DATEPART("HOUR", mc.Arguments[0]);
                            SqlFactory sqlFactory1 = this.sql;
                            SqlExpression[] sqlExpressionArray1 = new SqlExpression[2];
                            int index1 = 0;
                            SqlExpression sqlExpression2 = this.sql.Divide(expr1, 100L);
                            sqlExpressionArray1[index1] = sqlExpression2;
                            int index2 = 1;
                            SqlFactory sqlFactory2 = this.sql;
                            SqlFactory sqlFactory3 = this.sql;
                            SqlExpression[] sqlExpressionArray2 = new SqlExpression[3];
                            int index3 = 0;
                            SqlExpression sqlExpression3 = this.sql.Multiply(this.sql.ConvertToBigint(expr4), 3600000L);
                            sqlExpressionArray2[index3] = sqlExpression3;
                            int index4 = 1;
                            SqlExpression sqlExpression4 = this.sql.Multiply(this.sql.ConvertToBigint(expr3), 60000L);
                            sqlExpressionArray2[index4] = sqlExpression4;
                            int index5 = 2;
                            SqlExpression sqlExpression5 = this.sql.Multiply(this.sql.ConvertToBigint(expr2), 1000L);
                            sqlExpressionArray2[index5] = sqlExpression5;
                            SqlExpression expr5 = sqlFactory3.Add(sqlExpressionArray2);
                            long second = 10000L;
                            SqlExpression sqlExpression6 = sqlFactory2.Multiply(expr5, second);
                            sqlExpressionArray1[index2] = sqlExpression6;
                            sqlTicks = sqlFactory1.Add(sqlExpressionArray1);
                        }
                        return this.CreateDateTimeFromDateAndTicks(mc.Object, sqlTicks, sourceExpression);
                    }
                    if (mc.Method.Name == "AddMonths")
                        sqlExpression1 = this.sql.DATEADD("MONTH", mc.Arguments[0], mc.Object);
                    else if (mc.Method.Name == "AddYears")
                        sqlExpression1 = this.sql.DATEADD("YEAR", mc.Arguments[0], mc.Object);
                    else if (mc.Method.Name == "AddMilliseconds")
                        sqlExpression1 = this.CreateDateTimeFromDateAndMs(mc.Object, mc.Arguments[0], sourceExpression);
                    else if (mc.Method.Name == "AddSeconds")
                    {
                        SqlExpression ms = this.sql.Multiply(mc.Arguments[0], 1000L);
                        sqlExpression1 = this.CreateDateTimeFromDateAndMs(mc.Object, ms, sourceExpression);
                    }
                    else if (mc.Method.Name == "AddMinutes")
                    {
                        SqlExpression ms = this.sql.Multiply(mc.Arguments[0], 60000L);
                        sqlExpression1 = this.CreateDateTimeFromDateAndMs(mc.Object, ms, sourceExpression);
                    }
                    else if (mc.Method.Name == "AddHours")
                    {
                        SqlExpression ms = this.sql.Multiply(mc.Arguments[0], 3600000L);
                        sqlExpression1 = this.CreateDateTimeFromDateAndMs(mc.Object, ms, sourceExpression);
                    }
                    else if (mc.Method.Name == "AddDays")
                    {
                        SqlExpression ms = this.sql.Multiply(mc.Arguments[0], 86400000L);
                        sqlExpression1 = this.CreateDateTimeFromDateAndMs(mc.Object, ms, sourceExpression);
                    }
                }
                return sqlExpression1;
            }

            private SqlExpression TranslateDateTimeOffsetInstanceMethod(SqlMethodCall mc)
            {
                SqlExpression sqlExpression1 = (SqlExpression)null;
                Expression sourceExpression = mc.SourceExpression;
                if (mc.Method.Name == "CompareTo")
                    sqlExpression1 = this.CreateComparison(mc.Object, mc.Arguments[0], sourceExpression);
                else if (mc.Method.Name == "Add" && mc.Arguments.Count == 1 && mc.Arguments[0].ClrType == typeof(TimeSpan) || mc.Method.Name == "AddTicks")
                {
                    SqlExpression expr1 = this.sql.DATEPART("NANOSECOND", mc.Arguments[0]);
                    SqlExpression expr2 = this.sql.DATEPART("SECOND", mc.Arguments[0]);
                    SqlExpression expr3 = this.sql.DATEPART("MINUTE", mc.Arguments[0]);
                    SqlExpression expr4 = this.sql.DATEPART("HOUR", mc.Arguments[0]);
                    SqlFactory sqlFactory1 = this.sql;
                    SqlExpression[] sqlExpressionArray1 = new SqlExpression[2];
                    int index1 = 0;
                    SqlExpression sqlExpression2 = this.sql.Divide(expr1, 100L);
                    sqlExpressionArray1[index1] = sqlExpression2;
                    int index2 = 1;
                    SqlFactory sqlFactory2 = this.sql;
                    SqlFactory sqlFactory3 = this.sql;
                    SqlExpression[] sqlExpressionArray2 = new SqlExpression[3];
                    int index3 = 0;
                    SqlExpression sqlExpression3 = this.sql.Multiply(this.sql.ConvertToBigint(expr4), 3600000L);
                    sqlExpressionArray2[index3] = sqlExpression3;
                    int index4 = 1;
                    SqlExpression sqlExpression4 = this.sql.Multiply(this.sql.ConvertToBigint(expr3), 60000L);
                    sqlExpressionArray2[index4] = sqlExpression4;
                    int index5 = 2;
                    SqlExpression sqlExpression5 = this.sql.Multiply(this.sql.ConvertToBigint(expr2), 1000L);
                    sqlExpressionArray2[index5] = sqlExpression5;
                    SqlExpression expr5 = sqlFactory3.Add(sqlExpressionArray2);
                    long second = 10000L;
                    SqlExpression sqlExpression6 = sqlFactory2.Multiply(expr5, second);
                    sqlExpressionArray1[index2] = sqlExpression6;
                    SqlExpression sqlTicks = sqlFactory1.Add(sqlExpressionArray1);
                    sqlExpression1 = this.CreateDateTimeOffsetFromDateAndTicks(mc.Object, sqlTicks, sourceExpression);
                }
                else if (mc.Method.Name == "AddMonths")
                    sqlExpression1 = this.sql.DATETIMEOFFSETADD("MONTH", mc.Arguments[0], mc.Object);
                else if (mc.Method.Name == "AddYears")
                    sqlExpression1 = this.sql.DATETIMEOFFSETADD("YEAR", mc.Arguments[0], mc.Object);
                else if (mc.Method.Name == "AddMilliseconds")
                    sqlExpression1 = this.CreateDateTimeOffsetFromDateAndMs(mc.Object, mc.Arguments[0], sourceExpression);
                else if (mc.Method.Name == "AddSeconds")
                {
                    SqlExpression ms = this.sql.Multiply(mc.Arguments[0], 1000L);
                    sqlExpression1 = this.CreateDateTimeOffsetFromDateAndMs(mc.Object, ms, sourceExpression);
                }
                else if (mc.Method.Name == "AddMinutes")
                {
                    SqlExpression ms = this.sql.Multiply(mc.Arguments[0], 60000L);
                    sqlExpression1 = this.CreateDateTimeOffsetFromDateAndMs(mc.Object, ms, sourceExpression);
                }
                else if (mc.Method.Name == "AddHours")
                {
                    SqlExpression ms = this.sql.Multiply(mc.Arguments[0], 3600000L);
                    sqlExpression1 = this.CreateDateTimeOffsetFromDateAndMs(mc.Object, ms, sourceExpression);
                }
                else if (mc.Method.Name == "AddDays")
                {
                    SqlExpression ms = this.sql.Multiply(mc.Arguments[0], 86400000L);
                    sqlExpression1 = this.CreateDateTimeOffsetFromDateAndMs(mc.Object, ms, sourceExpression);
                }
                return sqlExpression1;
            }

            private SqlExpression TranslateTimeSpanInstanceMethod(SqlMethodCall mc)
            {
                SqlExpression sqlExpression1 = (SqlExpression)null;
                Expression sourceExpression = mc.SourceExpression;
                if (mc.Method.Name == "Add")
                {
                    SqlFactory sqlFactory = this.sql;
                    SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                    int index1 = 0;
                    SqlExpression @object = mc.Object;
                    sqlExpressionArray[index1] = @object;
                    int index2 = 1;
                    SqlExpression sqlExpression2 = mc.Arguments[0];
                    sqlExpressionArray[index2] = sqlExpression2;
                    sqlExpression1 = sqlFactory.Add(sqlExpressionArray);
                }
                else if (mc.Method.Name == "Subtract")
                    sqlExpression1 = this.sql.Subtract(mc.Object, mc.Arguments[0]);
                else if (mc.Method.Name == "CompareTo")
                    sqlExpression1 = this.CreateComparison(mc.Object, mc.Arguments[0], sourceExpression);
                else if (mc.Method.Name == "Duration")
                {
                    if (SqlFactory.IsSqlTimeType(mc.Object))
                        return mc.Object;
                    SqlFactory sqlFactory = this.sql;
                    Type clrType = typeof(TimeSpan);
                    string name = "ABS";
                    SqlExpression[] sqlExpressionArray = new SqlExpression[1];
                    int index = 0;
                    SqlExpression @object = mc.Object;
                    sqlExpressionArray[index] = @object;
                    Expression source = sourceExpression;
                    sqlExpression1 = (SqlExpression)sqlFactory.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                }
                else if (mc.Method.Name == "Negate")
                    sqlExpression1 = (SqlExpression)this.sql.Unary(SqlNodeType.Negate, mc.Object, sourceExpression);
                return sqlExpression1;
            }

            private SqlExpression TranslateConvertStaticMethod(SqlMethodCall mc)
            {
                SqlExpression sqlExpression = (SqlExpression)null;
                if (mc.Arguments.Count == 1)
                {
                    SqlExpression expr = mc.Arguments[0];
                    string name = mc.Method.Name;
                    // ISSUE: reference to a compiler-generated method
                    uint stringHash = 0;
                    Type type1;
                    if (stringHash <= 1883303205U)
                    {
                        if (stringHash <= 851977407U)
                        {
                            if ((int)stringHash != 172651214)
                            {
                                if ((int)stringHash != 356562880)
                                {
                                    if ((int)stringHash == 851977407 && name == "ToInt64")
                                    {
                                        type1 = typeof(long);
                                        goto label_45;
                                    }
                                }
                                else if (name == "ToBoolean")
                                {
                                    type1 = typeof(bool);
                                    goto label_45;
                                }
                            }
                            else if (name == "ToUInt64") { }
                                
                        }
                        else if (stringHash <= 1600551161U)
                        {
                            if ((int)stringHash != 852124502)
                            {
                                if ((int)stringHash == 1600551161 && name == "ToDateTime")
                                {
                                    Type nonNullableType = TypeSystem.GetNonNullableType(expr.ClrType);
                                    if (!(nonNullableType == typeof(string)) && !(nonNullableType == typeof(DateTime)))
                                        throw Error.ConvertToDateTimeOnlyForDateTimeOrString();
                                    type1 = typeof(DateTime);
                                    goto label_45;
                                }
                            }
                            else if (name == "ToInt16")
                            {
                                type1 = typeof(short);
                                goto label_45;
                            }
                        }
                        else if ((int)stringHash != 1628624528)
                        {
                            if ((int)stringHash == 1883303205 && name == "ToString")
                            {
                                type1 = typeof(string);
                                goto label_45;
                            }
                        }
                        else if (name == "ToChar")
                        {
                            type1 = typeof(char);
                            if (expr.SqlType.IsChar)
                            {
                                this.sql.TypeProvider.From(type1, new int?(1));
                                goto label_45;
                            }
                            else
                                goto label_45;
                        }
                    }
                    else if (stringHash <= 2465681071U)
                    {
                        if (stringHash <= 2052185827U)
                        {
                            if ((int)stringHash != 1938964185)
                            {
                                if ((int)stringHash != 2052185827 || name == "ToUInt16")
                                { }
                            }
                            else if (name == "ToDouble")
                            {
                                type1 = typeof(double);
                                goto label_45;
                            }
                        }
                        else if ((int)stringHash != -1906934899)
                        {
                            if ((int)stringHash != -1829286225 || name == "ToSByte")
                            { }
                        }
                        else if (name == "ToUInt32")
                        {  }
                    }
                    else if (stringHash <= 3187895726U)
                    {
                        if ((int)stringHash != -1277203181)
                        {
                            if ((int)stringHash == -1107071570 && name == "ToByte")
                            {
                                type1 = typeof(byte);
                                goto label_45;
                            }
                        }
                        else if (name == "ToDecimal")
                        {
                            type1 = typeof(Decimal);
                            goto label_45;
                        }
                    }
                    else if ((int)stringHash != -1093681944)
                    {
                        if ((int)stringHash == -522456384 && name == "ToSingle")
                        {
                            type1 = typeof(float);
                            goto label_45;
                        }
                    }
                    else if (name == "ToInt32")
                    {
                        type1 = typeof(int);
                        goto label_45;
                    }
                    throw PostBindDotNetConverter.Visitor.GetMethodSupportException(mc);
                    label_45:
                    if (this.sql.TypeProvider.From(type1) != expr.SqlType || expr.ClrType == typeof(bool) && type1 == typeof(int))
                        sqlExpression = this.sql.ConvertTo(type1, expr);
                    else if (type1 != (Type)null)
                    {
                        if (this.sql.TypeProvider.From(type1) != expr.SqlType)
                            sqlExpression = this.sql.ConvertTo(type1, expr);
                        else if (type1 != expr.ClrType && TypeSystem.GetNonNullableType(type1) == TypeSystem.GetNonNullableType(expr.ClrType))
                        {
                            Type type2 = type1;
                            SqlExpression liftedExpression = expr;
                            Expression sourceExpression = liftedExpression.SourceExpression;
                            sqlExpression = (SqlExpression)new SqlLift(type2, liftedExpression, sourceExpression);
                        }
                        else
                            sqlExpression = expr;
                    }
                }
                return sqlExpression;
            }

            private SqlExpression TranslateDateTimeBinary(SqlBinary bo)
            {
                bool asNullable = TypeSystem.IsNullableType(bo.ClrType);
                Type nonNullableType = TypeSystem.GetNonNullableType(bo.Right.ClrType);
                switch (bo.NodeType)
                {
                    case SqlNodeType.Add:
                        if (nonNullableType == typeof(TimeSpan))
                        {
                            if (SqlFactory.IsSqlTimeType(bo.Right))
                                return this.sql.AddTimeSpan(bo.Left, bo.Right, asNullable);
                            if (TypeSystem.GetNonNullableType(bo.Left.ClrType) == typeof(DateTimeOffset))
                                return this.CreateDateTimeOffsetFromDateAndTicks(bo.Left, bo.Right, bo.SourceExpression, asNullable);
                            return this.CreateDateTimeFromDateAndTicks(bo.Left, bo.Right, bo.SourceExpression, asNullable);
                        }
                        break;
                    case SqlNodeType.Sub:
                        if (nonNullableType == typeof(DateTime))
                        {
                            Type clrType1 = bo.ClrType;
                            SqlExpression left = bo.Left;
                            SqlExpression right = bo.Right;
                            SqlExpression sqlExpression1 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, "DAY", bo.SourceExpression);
                            SqlExpression sqlExpression2 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, "MILLISECOND", bo.SourceExpression);
                            SqlFactory sqlFactory1 = this.sql;
                            Type clrType2 = typeof(int);
                            string name1 = "DATEDIFF";
                            SqlExpression[] sqlExpressionArray1 = new SqlExpression[3];
                            int index1 = 0;
                            SqlExpression sqlExpression3 = sqlExpression1;
                            sqlExpressionArray1[index1] = sqlExpression3;
                            int index2 = 1;
                            SqlExpression sqlExpression4 = right;
                            sqlExpressionArray1[index2] = sqlExpression4;
                            int index3 = 2;
                            SqlExpression sqlExpression5 = left;
                            sqlExpressionArray1[index3] = sqlExpression5;
                            Expression sourceExpression1 = bo.SourceExpression;
                            SqlExpression expr1 = (SqlExpression)sqlFactory1.FunctionCall(clrType2, name1, (IEnumerable<SqlExpression>)sqlExpressionArray1, sourceExpression1);
                            SqlFactory sqlFactory2 = this.sql;
                            Type clrType3 = typeof(DateTime);
                            string name2 = "DATEADD";
                            SqlExpression[] sqlExpressionArray2 = new SqlExpression[3];
                            int index4 = 0;
                            SqlExpression sqlExpression6 = sqlExpression1;
                            sqlExpressionArray2[index4] = sqlExpression6;
                            int index5 = 1;
                            SqlExpression sqlExpression7 = expr1;
                            sqlExpressionArray2[index5] = sqlExpression7;
                            int index6 = 2;
                            SqlExpression sqlExpression8 = right;
                            sqlExpressionArray2[index6] = sqlExpression8;
                            Expression sourceExpression2 = bo.SourceExpression;
                            SqlExpression sqlExpression9 = (SqlExpression)sqlFactory2.FunctionCall(clrType3, name2, (IEnumerable<SqlExpression>)sqlExpressionArray2, sourceExpression2);
                            SqlFactory sqlFactory3 = this.sql;
                            Type clrType4 = typeof(int);
                            string name3 = "DATEDIFF";
                            SqlExpression[] sqlExpressionArray3 = new SqlExpression[3];
                            int index7 = 0;
                            SqlExpression sqlExpression10 = sqlExpression2;
                            sqlExpressionArray3[index7] = sqlExpression10;
                            int index8 = 1;
                            SqlExpression sqlExpression11 = sqlExpression9;
                            sqlExpressionArray3[index8] = sqlExpression11;
                            int index9 = 2;
                            SqlExpression sqlExpression12 = left;
                            sqlExpressionArray3[index9] = sqlExpression12;
                            Expression sourceExpression3 = bo.SourceExpression;
                            SqlExpression sqlExpression13 = (SqlExpression)sqlFactory3.FunctionCall(clrType4, name3, (IEnumerable<SqlExpression>)sqlExpressionArray3, sourceExpression3);
                            SqlFactory sqlFactory4 = this.sql;
                            SqlFactory sqlFactory5 = this.sql;
                            SqlExpression[] sqlExpressionArray4 = new SqlExpression[2];
                            int index10 = 0;
                            SqlExpression sqlExpression14 = this.sql.Multiply(this.sql.ConvertToBigint(expr1), 86400000L);
                            sqlExpressionArray4[index10] = sqlExpression14;
                            int index11 = 1;
                            SqlExpression sqlExpression15 = sqlExpression13;
                            sqlExpressionArray4[index11] = sqlExpression15;
                            SqlExpression expr2 = sqlFactory5.Add(sqlExpressionArray4);
                            long second = 10000L;
                            SqlExpression expr3 = sqlFactory4.Multiply(expr2, second);
                            return this.sql.ConvertTo(clrType1, expr3);
                        }
                        if (nonNullableType == typeof(DateTimeOffset))
                        {
                            Type clrType1 = bo.ClrType;
                            SqlExpression left = bo.Left;
                            SqlExpression right = bo.Right;
                            SqlExpression sqlExpression1 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, "DAY", bo.SourceExpression);
                            SqlExpression sqlExpression2 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, "MILLISECOND", bo.SourceExpression);
                            SqlExpression sqlExpression3 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, "MICROSECOND", bo.SourceExpression);
                            SqlExpression sqlExpression4 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, "NANOSECOND", bo.SourceExpression);
                            SqlFactory sqlFactory1 = this.sql;
                            Type clrType2 = typeof(int);
                            string name1 = "DATEDIFF";
                            SqlExpression[] sqlExpressionArray1 = new SqlExpression[3];
                            int index1 = 0;
                            SqlExpression sqlExpression5 = sqlExpression1;
                            sqlExpressionArray1[index1] = sqlExpression5;
                            int index2 = 1;
                            SqlExpression sqlExpression6 = right;
                            sqlExpressionArray1[index2] = sqlExpression6;
                            int index3 = 2;
                            SqlExpression sqlExpression7 = left;
                            sqlExpressionArray1[index3] = sqlExpression7;
                            Expression sourceExpression1 = bo.SourceExpression;
                            SqlExpression expr1 = (SqlExpression)sqlFactory1.FunctionCall(clrType2, name1, (IEnumerable<SqlExpression>)sqlExpressionArray1, sourceExpression1);
                            SqlFactory sqlFactory2 = this.sql;
                            Type clrType3 = typeof(DateTimeOffset);
                            string name2 = "DATEADD";
                            SqlExpression[] sqlExpressionArray2 = new SqlExpression[3];
                            int index4 = 0;
                            SqlExpression sqlExpression8 = sqlExpression1;
                            sqlExpressionArray2[index4] = sqlExpression8;
                            int index5 = 1;
                            SqlExpression sqlExpression9 = expr1;
                            sqlExpressionArray2[index5] = sqlExpression9;
                            int index6 = 2;
                            SqlExpression sqlExpression10 = right;
                            sqlExpressionArray2[index6] = sqlExpression10;
                            Expression sourceExpression2 = bo.SourceExpression;
                            SqlExpression sqlExpression11 = (SqlExpression)sqlFactory2.FunctionCall(clrType3, name2, (IEnumerable<SqlExpression>)sqlExpressionArray2, sourceExpression2);
                            SqlFactory sqlFactory3 = this.sql;
                            Type clrType4 = typeof(int);
                            string name3 = "DATEDIFF";
                            SqlExpression[] sqlExpressionArray3 = new SqlExpression[3];
                            int index7 = 0;
                            SqlExpression sqlExpression12 = sqlExpression2;
                            sqlExpressionArray3[index7] = sqlExpression12;
                            int index8 = 1;
                            SqlExpression sqlExpression13 = sqlExpression11;
                            sqlExpressionArray3[index8] = sqlExpression13;
                            int index9 = 2;
                            SqlExpression sqlExpression14 = left;
                            sqlExpressionArray3[index9] = sqlExpression14;
                            Expression sourceExpression3 = bo.SourceExpression;
                            SqlExpression sqlExpression15 = (SqlExpression)sqlFactory3.FunctionCall(clrType4, name3, (IEnumerable<SqlExpression>)sqlExpressionArray3, sourceExpression3);
                            SqlFactory sqlFactory4 = this.sql;
                            Type clrType5 = typeof(DateTimeOffset);
                            string name4 = "DATEADD";
                            SqlExpression[] sqlExpressionArray4 = new SqlExpression[3];
                            int index10 = 0;
                            SqlExpression sqlExpression16 = sqlExpression2;
                            sqlExpressionArray4[index10] = sqlExpression16;
                            int index11 = 1;
                            SqlExpression sqlExpression17 = sqlExpression15;
                            sqlExpressionArray4[index11] = sqlExpression17;
                            int index12 = 2;
                            SqlExpression sqlExpression18 = sqlExpression11;
                            sqlExpressionArray4[index12] = sqlExpression18;
                            Expression sourceExpression4 = bo.SourceExpression;
                            SqlExpression sqlExpression19 = (SqlExpression)sqlFactory4.FunctionCall(clrType5, name4, (IEnumerable<SqlExpression>)sqlExpressionArray4, sourceExpression4);
                            SqlFactory sqlFactory5 = this.sql;
                            Type clrType6 = typeof(int);
                            string name5 = "DATEDIFF";
                            SqlExpression[] sqlExpressionArray5 = new SqlExpression[3];
                            int index13 = 0;
                            SqlExpression sqlExpression20 = sqlExpression3;
                            sqlExpressionArray5[index13] = sqlExpression20;
                            int index14 = 1;
                            SqlExpression sqlExpression21 = sqlExpression19;
                            sqlExpressionArray5[index14] = sqlExpression21;
                            int index15 = 2;
                            SqlExpression sqlExpression22 = left;
                            sqlExpressionArray5[index15] = sqlExpression22;
                            Expression sourceExpression5 = bo.SourceExpression;
                            SqlExpression expr2 = (SqlExpression)sqlFactory5.FunctionCall(clrType6, name5, (IEnumerable<SqlExpression>)sqlExpressionArray5, sourceExpression5);
                            SqlFactory sqlFactory6 = this.sql;
                            Type clrType7 = typeof(DateTimeOffset);
                            string name6 = "DATEADD";
                            SqlExpression[] sqlExpressionArray6 = new SqlExpression[3];
                            int index16 = 0;
                            SqlExpression sqlExpression23 = sqlExpression3;
                            sqlExpressionArray6[index16] = sqlExpression23;
                            int index17 = 1;
                            SqlExpression sqlExpression24 = expr2;
                            sqlExpressionArray6[index17] = sqlExpression24;
                            int index18 = 2;
                            SqlExpression sqlExpression25 = sqlExpression19;
                            sqlExpressionArray6[index18] = sqlExpression25;
                            Expression sourceExpression6 = bo.SourceExpression;
                            SqlExpression sqlExpression26 = (SqlExpression)sqlFactory6.FunctionCall(clrType7, name6, (IEnumerable<SqlExpression>)sqlExpressionArray6, sourceExpression6);
                            SqlFactory sqlFactory7 = this.sql;
                            Type clrType8 = typeof(int);
                            string name7 = "DATEDIFF";
                            SqlExpression[] sqlExpressionArray7 = new SqlExpression[3];
                            int index19 = 0;
                            SqlExpression sqlExpression27 = sqlExpression4;
                            sqlExpressionArray7[index19] = sqlExpression27;
                            int index20 = 1;
                            SqlExpression sqlExpression28 = sqlExpression26;
                            sqlExpressionArray7[index20] = sqlExpression28;
                            int index21 = 2;
                            SqlExpression sqlExpression29 = left;
                            sqlExpressionArray7[index21] = sqlExpression29;
                            Expression sourceExpression7 = bo.SourceExpression;
                            SqlExpression expr3 = (SqlExpression)sqlFactory7.FunctionCall(clrType8, name7, (IEnumerable<SqlExpression>)sqlExpressionArray7, sourceExpression7);
                            SqlFactory sqlFactory8 = this.sql;
                            Type clrType9 = typeof(DateTimeOffset);
                            string name8 = "DATEADD";
                            SqlExpression[] sqlExpressionArray8 = new SqlExpression[3];
                            int index22 = 0;
                            SqlExpression sqlExpression30 = sqlExpression4;
                            sqlExpressionArray8[index22] = sqlExpression30;
                            int index23 = 1;
                            SqlExpression sqlExpression31 = expr3;
                            sqlExpressionArray8[index23] = sqlExpression31;
                            int index24 = 2;
                            SqlExpression sqlExpression32 = sqlExpression26;
                            sqlExpressionArray8[index24] = sqlExpression32;
                            Expression sourceExpression8 = bo.SourceExpression;
                            sqlFactory8.FunctionCall(clrType9, name8, (IEnumerable<SqlExpression>)sqlExpressionArray8, sourceExpression8);
                            SqlFactory sqlFactory9 = this.sql;
                            SqlExpression[] sqlExpressionArray9 = new SqlExpression[3];
                            int index25 = 0;
                            SqlExpression sqlExpression33 = this.sql.Divide(expr3, 100L);
                            sqlExpressionArray9[index25] = sqlExpression33;
                            int index26 = 1;
                            SqlExpression sqlExpression34 = this.sql.Multiply(expr2, 10L);
                            sqlExpressionArray9[index26] = sqlExpression34;
                            int index27 = 2;
                            SqlFactory sqlFactory10 = this.sql;
                            SqlFactory sqlFactory11 = this.sql;
                            SqlExpression[] sqlExpressionArray10 = new SqlExpression[2];
                            int index28 = 0;
                            SqlExpression sqlExpression35 = this.sql.Multiply(this.sql.ConvertToBigint(expr1), 86400000L);
                            sqlExpressionArray10[index28] = sqlExpression35;
                            int index29 = 1;
                            SqlExpression sqlExpression36 = sqlExpression15;
                            sqlExpressionArray10[index29] = sqlExpression36;
                            SqlExpression expr4 = sqlFactory11.Add(sqlExpressionArray10);
                            long second = 10000L;
                            SqlExpression sqlExpression37 = sqlFactory10.Multiply(expr4, second);
                            sqlExpressionArray9[index27] = sqlExpression37;
                            SqlExpression expr5 = sqlFactory9.Add(sqlExpressionArray9);
                            return this.sql.ConvertTo(clrType1, expr5);
                        }
                        if (nonNullableType == typeof(TimeSpan))
                        {
                            SqlExpression sqlExpression1 = bo.Right;
                            if (SqlFactory.IsSqlTimeType(bo.Right))
                            {
                                SqlExpression expr1 = this.sql.DATEPART("NANOSECOND", sqlExpression1);
                                SqlExpression expr2 = this.sql.DATEPART("SECOND", sqlExpression1);
                                SqlExpression expr3 = this.sql.DATEPART("MINUTE", sqlExpression1);
                                SqlExpression expr4 = this.sql.DATEPART("HOUR", sqlExpression1);
                                SqlFactory sqlFactory1 = this.sql;
                                SqlExpression[] sqlExpressionArray1 = new SqlExpression[2];
                                int index1 = 0;
                                SqlExpression sqlExpression2 = this.sql.Divide(expr1, 100L);
                                sqlExpressionArray1[index1] = sqlExpression2;
                                int index2 = 1;
                                SqlFactory sqlFactory2 = this.sql;
                                SqlFactory sqlFactory3 = this.sql;
                                SqlExpression[] sqlExpressionArray2 = new SqlExpression[3];
                                int index3 = 0;
                                SqlExpression sqlExpression3 = this.sql.Multiply(this.sql.ConvertToBigint(expr4), 3600000L);
                                sqlExpressionArray2[index3] = sqlExpression3;
                                int index4 = 1;
                                SqlExpression sqlExpression4 = this.sql.Multiply(this.sql.ConvertToBigint(expr3), 60000L);
                                sqlExpressionArray2[index4] = sqlExpression4;
                                int index5 = 2;
                                SqlExpression sqlExpression5 = this.sql.Multiply(this.sql.ConvertToBigint(expr2), 1000L);
                                sqlExpressionArray2[index5] = sqlExpression5;
                                SqlExpression expr5 = sqlFactory3.Add(sqlExpressionArray2);
                                long second = 10000L;
                                SqlExpression sqlExpression6 = sqlFactory2.Multiply(expr5, second);
                                sqlExpressionArray1[index2] = sqlExpression6;
                                sqlExpression1 = sqlFactory1.Add(sqlExpressionArray1);
                            }
                            if (!(TypeSystem.GetNonNullableType(bo.Left.ClrType) == typeof(DateTimeOffset)))
                                return this.CreateDateTimeFromDateAndTicks(bo.Left, (SqlExpression)this.sql.Unary(SqlNodeType.Negate, sqlExpression1, bo.SourceExpression), bo.SourceExpression, asNullable);
                            return this.CreateDateTimeOffsetFromDateAndTicks(bo.Left, (SqlExpression)this.sql.Unary(SqlNodeType.Negate, sqlExpression1, bo.SourceExpression), bo.SourceExpression, asNullable);
                        }
                        break;
                }
                return (SqlExpression)bo;
            }

            internal SqlExpression TranslateDecimalMethod(SqlMethodCall mc)
            {
                Expression sourceExpression1 = mc.SourceExpression;
                if (mc.Method.IsStatic)
                {
                    if (mc.Arguments.Count == 2)
                    {
                        string name = mc.Method.Name;
                        if (name == "Multiply")
                            return (SqlExpression)this.sql.Binary(SqlNodeType.Mul, mc.Arguments[0], mc.Arguments[1]);
                        if (name == "Divide")
                            return (SqlExpression)this.sql.Binary(SqlNodeType.Div, mc.Arguments[0], mc.Arguments[1]);
                        if (name == "Subtract")
                            return (SqlExpression)this.sql.Binary(SqlNodeType.Sub, mc.Arguments[0], mc.Arguments[1]);
                        if (name == "Add")
                            return (SqlExpression)this.sql.Binary(SqlNodeType.Add, mc.Arguments[0], mc.Arguments[1]);
                        if (name == "Remainder")
                            return (SqlExpression)this.sql.Binary(SqlNodeType.Mod, mc.Arguments[0], mc.Arguments[1]);
                        if (name == "Round")
                            return (SqlExpression)this.sql.FunctionCall(mc.Method.ReturnType, "ROUND", (IEnumerable<SqlExpression>)mc.Arguments, mc.SourceExpression);
                    }
                    else if (mc.Arguments.Count == 1)
                    {
                        string name1 = mc.Method.Name;
                        if (name1 == "Negate")
                            return (SqlExpression)this.sql.Unary(SqlNodeType.Negate, mc.Arguments[0], sourceExpression1);
                        if (!(name1 == "Floor") && !(name1 == "Truncate"))
                        {
                            if (name1 == "Round")
                            {
                                SqlFactory sqlFactory = this.sql;
                                Type returnType = mc.Method.ReturnType;
                                string name2 = "ROUND";
                                SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                                int index1 = 0;
                                SqlExpression sqlExpression1 = mc.Arguments[0];
                                sqlExpressionArray[index1] = sqlExpression1;
                                int index2 = 1;
                                SqlExpression sqlExpression2 = this.sql.ValueFromObject((object)0, false, mc.SourceExpression);
                                sqlExpressionArray[index2] = sqlExpression2;
                                Expression sourceExpression2 = mc.SourceExpression;
                                return (SqlExpression)sqlFactory.FunctionCall(returnType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, sourceExpression2);
                            }
                            if (mc.Method.Name.StartsWith("To", StringComparison.Ordinal))
                                return this.TranslateConvertStaticMethod(mc);
                        }
                        else
                        {
                            SqlFactory sqlFactory = this.sql;
                            Type returnType = mc.Method.ReturnType;
                            string name2 = "ROUND";
                            SqlExpression[] sqlExpressionArray = new SqlExpression[3];
                            int index1 = 0;
                            SqlExpression sqlExpression1 = mc.Arguments[0];
                            sqlExpressionArray[index1] = sqlExpression1;
                            int index2 = 1;
                            SqlExpression sqlExpression2 = this.sql.ValueFromObject((object)0, false, mc.SourceExpression);
                            sqlExpressionArray[index2] = sqlExpression2;
                            int index3 = 2;
                            SqlExpression sqlExpression3 = this.sql.ValueFromObject((object)1, false, mc.SourceExpression);
                            sqlExpressionArray[index3] = sqlExpression3;
                            Expression sourceExpression2 = mc.SourceExpression;
                            return (SqlExpression)sqlFactory.FunctionCall(returnType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, sourceExpression2);
                        }
                    }
                }
                throw PostBindDotNetConverter.Visitor.GetMethodSupportException(mc);
            }

            private SqlExpression TranslateStringStaticMethod(SqlMethodCall mc)
            {
                Expression sourceExpression = mc.SourceExpression;
                if (mc.Method.Name == "Concat")
                {
                    SqlClientArray sqlClientArray = mc.Arguments[0] as SqlClientArray;
                    List<SqlExpression> list = sqlClientArray == null ? mc.Arguments : sqlClientArray.Expressions;
                    if (list.Count == 0)
                    {
                        this.sql.ValueFromObject((object)"", false, sourceExpression);
                    }
                    else
                    {
                        SqlExpression sqlExpression1 = list[0].SqlType.IsString || list[0].SqlType.IsChar ? list[0] : this.sql.ConvertTo(typeof(string), list[0]);
                        for (int index1 = 1; index1 < list.Count; ++index1)
                        {
                            if (list[index1].SqlType.IsString || list[index1].SqlType.IsChar)
                            {
                                SqlFactory sqlFactory = this.sql;
                                SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                                int index2 = 0;
                                SqlExpression sqlExpression2 = sqlExpression1;
                                sqlExpressionArray[index2] = sqlExpression2;
                                int index3 = 1;
                                SqlExpression sqlExpression3 = list[index1];
                                sqlExpressionArray[index3] = sqlExpression3;
                                sqlExpression1 = sqlFactory.Concat(sqlExpressionArray);
                            }
                            else
                            {
                                SqlFactory sqlFactory = this.sql;
                                SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                                int index2 = 0;
                                SqlExpression sqlExpression2 = sqlExpression1;
                                sqlExpressionArray[index2] = sqlExpression2;
                                int index3 = 1;
                                SqlExpression sqlExpression3 = this.sql.ConvertTo(typeof(string), list[index1]);
                                sqlExpressionArray[index3] = sqlExpression3;
                                sqlExpression1 = sqlFactory.Concat(sqlExpressionArray);
                            }
                        }
                    }
                }
                else if (mc.Method.Name == "Equals" && mc.Arguments.Count == 2)
                    this.sql.Binary(SqlNodeType.EQ2V, mc.Arguments[0], mc.Arguments[1]);
                else if (mc.Method.Name == "Compare" && mc.Arguments.Count == 2)
                    this.CreateComparison(mc.Arguments[0], mc.Arguments[1], sourceExpression);
                throw PostBindDotNetConverter.Visitor.GetMethodSupportException(mc);
            }

            private SqlExpression CreateFunctionCallStatic1(Type type, string functionName, List<SqlExpression> arguments, Expression source)
            {
                SqlFactory sqlFactory = this.sql;
                Type clrType = type;
                string name = functionName;
                SqlExpression[] sqlExpressionArray = new SqlExpression[1];
                int index = 0;
                SqlExpression sqlExpression = arguments[0];
                sqlExpressionArray[index] = sqlExpression;
                Expression source1 = source;
                return (SqlExpression)sqlFactory.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, source1);
            }

            private SqlExpression CreateFunctionCallStatic2(Type type, string functionName, List<SqlExpression> arguments, Expression source)
            {
                SqlFactory sqlFactory = this.sql;
                Type clrType = type;
                string name = functionName;
                SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                int index1 = 0;
                SqlExpression sqlExpression1 = arguments[0];
                sqlExpressionArray[index1] = sqlExpression1;
                int index2 = 1;
                SqlExpression sqlExpression2 = arguments[1];
                sqlExpressionArray[index2] = sqlExpression2;
                Expression source1 = source;
                return (SqlExpression)sqlFactory.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, source1);
            }

            private SqlExpression TranslateStringMethod(SqlMethodCall mc)
            {
                Expression sourceExpression1 = mc.SourceExpression;
                string name1 = mc.Method.Name;
                // ISSUE: reference to a compiler-generated method
                uint stringHash =0;
                if (stringHash <= 1846593938U)
                {
                    if (stringHash <= 585210133U)
                    {
                        if (stringHash <= 254900552U)
                        {
                            if ((int)stringHash != 169760743)
                            {
                                if ((int)stringHash == 254900552 && name1 == "Insert" && mc.Arguments.Count == 2)
                                {
                                    if (mc.Arguments[1] is SqlValue && ((SqlValue)mc.Arguments[1]).Value == null)
                                        throw Error.ArgumentNull("value");
                                    SqlFactory sqlFactory1 = this.sql;
                                    Type clrType = typeof(string);
                                    string name2 = "STUFF";
                                    SqlExpression[] sqlExpressionArray1 = new SqlExpression[4];
                                    int index1 = 0;
                                    SqlExpression object1 = mc.Object;
                                    sqlExpressionArray1[index1] = object1;
                                    int index2 = 1;
                                    SqlExpression sqlExpression1 = this.sql.Add(mc.Arguments[0], 1);
                                    sqlExpressionArray1[index2] = sqlExpression1;
                                    int index3 = 2;
                                    SqlExpression sqlExpression2 = this.sql.ValueFromObject((object)0, false, sourceExpression1);
                                    sqlExpressionArray1[index3] = sqlExpression2;
                                    int index4 = 3;
                                    SqlExpression sqlExpression3 = mc.Arguments[1];
                                    sqlExpressionArray1[index4] = sqlExpression3;
                                    Expression source = sourceExpression1;
                                    SqlFunctionCall sqlFunctionCall1 = sqlFactory1.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source);
                                    SqlExpression match = (SqlExpression)this.sql.Binary(SqlNodeType.EQ, this.sql.CLRLENGTH(mc.Object), mc.Arguments[0]);
                                    SqlFactory sqlFactory2 = this.sql;
                                    SqlExpression[] sqlExpressionArray2 = new SqlExpression[2];
                                    int index5 = 0;
                                    SqlExpression object2 = mc.Object;
                                    sqlExpressionArray2[index5] = object2;
                                    int index6 = 1;
                                    SqlExpression sqlExpression4 = mc.Arguments[1];
                                    sqlExpressionArray2[index6] = sqlExpression4;
                                    SqlExpression sqlExpression5 = sqlFactory2.Concat(sqlExpressionArray2);
                                    SqlFactory sqlFactory3 = this.sql;
                                    SqlWhen[] whens = new SqlWhen[1];
                                    int index7 = 0;
                                    SqlWhen sqlWhen = new SqlWhen(match, sqlExpression5);
                                    whens[index7] = sqlWhen;
                                    SqlFunctionCall sqlFunctionCall2 = sqlFunctionCall1;
                                    Expression sourceExpression2 = sourceExpression1;
                                    return (SqlExpression)sqlFactory3.SearchedCase(whens, (SqlExpression)sqlFunctionCall2, sourceExpression2);
                                }
                            }
                            else if (name1 == "PadLeft")
                            {
                                if (mc.Arguments.Count == 1)
                                {
                                    SqlExpression @object = mc.Object;
                                    SqlExpression sqlExpression1 = mc.Arguments[0];
                                    SqlExpression sqlExpression2 = this.sql.CLRLENGTH(@object);
                                    SqlExpression match = (SqlExpression)this.sql.Binary(SqlNodeType.GE, sqlExpression2, sqlExpression1);
                                    SqlExpression sqlExpression3 = this.sql.Subtract(sqlExpression1, sqlExpression2);
                                    SqlFactory sqlFactory1 = this.sql;
                                    Type clrType = typeof(string);
                                    string name2 = "SPACE";
                                    SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
                                    int index1 = 0;
                                    SqlExpression sqlExpression4 = sqlExpression3;
                                    sqlExpressionArray1[index1] = sqlExpression4;
                                    Expression source = sourceExpression1;
                                    SqlExpression sqlExpression5 = (SqlExpression)sqlFactory1.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source);
                                    SqlFactory sqlFactory2 = this.sql;
                                    SqlExpression[] sqlExpressionArray2 = new SqlExpression[2];
                                    int index2 = 0;
                                    SqlExpression sqlExpression6 = sqlExpression5;
                                    sqlExpressionArray2[index2] = sqlExpression6;
                                    int index3 = 1;
                                    SqlExpression sqlExpression7 = @object;
                                    sqlExpressionArray2[index3] = sqlExpression7;
                                    SqlExpression sqlExpression8 = sqlFactory2.Concat(sqlExpressionArray2);
                                    SqlFactory sqlFactory3 = this.sql;
                                    SqlWhen[] whens = new SqlWhen[1];
                                    int index4 = 0;
                                    SqlWhen sqlWhen = new SqlWhen(match, @object);
                                    whens[index4] = sqlWhen;
                                    SqlExpression @else = sqlExpression8;
                                    Expression sourceExpression2 = sourceExpression1;
                                    return (SqlExpression)sqlFactory3.SearchedCase(whens, @else, sourceExpression2);
                                }
                                if (mc.Arguments.Count == 2)
                                {
                                    SqlExpression @object = mc.Object;
                                    SqlExpression sqlExpression1 = mc.Arguments[0];
                                    SqlExpression sqlExpression2 = mc.Arguments[1];
                                    SqlExpression match = (SqlExpression)this.sql.Binary(SqlNodeType.GE, this.sql.CLRLENGTH(@object), sqlExpression1);
                                    SqlExpression second = this.sql.CLRLENGTH(@object);
                                    SqlExpression sqlExpression3 = this.sql.Subtract(sqlExpression1, second);
                                    SqlFactory sqlFactory1 = this.sql;
                                    Type clrType = typeof(string);
                                    string name2 = "REPLICATE";
                                    SqlExpression[] sqlExpressionArray1 = new SqlExpression[2];
                                    int index1 = 0;
                                    SqlExpression sqlExpression4 = sqlExpression2;
                                    sqlExpressionArray1[index1] = sqlExpression4;
                                    int index2 = 1;
                                    SqlExpression sqlExpression5 = sqlExpression3;
                                    sqlExpressionArray1[index2] = sqlExpression5;
                                    Expression source = sourceExpression1;
                                    SqlExpression sqlExpression6 = (SqlExpression)sqlFactory1.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source);
                                    SqlFactory sqlFactory2 = this.sql;
                                    SqlExpression[] sqlExpressionArray2 = new SqlExpression[2];
                                    int index3 = 0;
                                    SqlExpression sqlExpression7 = sqlExpression6;
                                    sqlExpressionArray2[index3] = sqlExpression7;
                                    int index4 = 1;
                                    SqlExpression sqlExpression8 = @object;
                                    sqlExpressionArray2[index4] = sqlExpression8;
                                    SqlExpression sqlExpression9 = sqlFactory2.Concat(sqlExpressionArray2);
                                    SqlFactory sqlFactory3 = this.sql;
                                    SqlWhen[] whens = new SqlWhen[1];
                                    int index5 = 0;
                                    SqlWhen sqlWhen = new SqlWhen(match, @object);
                                    whens[index5] = sqlWhen;
                                    SqlExpression @else = sqlExpression9;
                                    Expression sourceExpression2 = sourceExpression1;
                                    return (SqlExpression)sqlFactory3.SearchedCase(whens, @else, sourceExpression2);
                                }
                            }
                        }
                        else if ((int)stringHash != 564498461)
                        {
                            if ((int)stringHash == 585210133 && name1 == "get_Chars" && mc.Arguments.Count == 1)
                            {
                                SqlFactory sqlFactory = this.sql;
                                Type clrType = typeof(char);
                                string name2 = "SUBSTRING";
                                SqlExpression[] sqlExpressionArray = new SqlExpression[3];
                                int index1 = 0;
                                SqlExpression @object = mc.Object;
                                sqlExpressionArray[index1] = @object;
                                int index2 = 1;
                                SqlExpression sqlExpression1 = this.sql.Add(mc.Arguments[0], 1);
                                sqlExpressionArray[index2] = sqlExpression1;
                                int index3 = 2;
                                SqlExpression sqlExpression2 = this.sql.ValueFromObject((object)1, false, sourceExpression1);
                                sqlExpressionArray[index3] = sqlExpression2;
                                Expression source = sourceExpression1;
                                return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                            }
                        }
                        else if (name1 == "Remove")
                        {
                            if (mc.Arguments.Count == 1)
                            {
                                SqlFactory sqlFactory = this.sql;
                                Type clrType = typeof(string);
                                string name2 = "STUFF";
                                SqlExpression[] sqlExpressionArray = new SqlExpression[4];
                                int index1 = 0;
                                SqlExpression @object = mc.Object;
                                sqlExpressionArray[index1] = @object;
                                int index2 = 1;
                                SqlExpression sqlExpression1 = this.sql.Add(mc.Arguments[0], 1);
                                sqlExpressionArray[index2] = sqlExpression1;
                                int index3 = 2;
                                SqlExpression sqlExpression2 = this.sql.CLRLENGTH(mc.Object);
                                sqlExpressionArray[index3] = sqlExpression2;
                                int index4 = 3;
                                SqlExpression sqlExpression3 = this.sql.ValueFromObject((object)"", false, sourceExpression1);
                                sqlExpressionArray[index4] = sqlExpression3;
                                Expression source = sourceExpression1;
                                return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                            }
                            if (mc.Arguments.Count == 2)
                            {
                                SqlFactory sqlFactory = this.sql;
                                Type clrType = typeof(string);
                                string name2 = "STUFF";
                                SqlExpression[] sqlExpressionArray = new SqlExpression[4];
                                int index1 = 0;
                                SqlExpression @object = mc.Object;
                                sqlExpressionArray[index1] = @object;
                                int index2 = 1;
                                SqlExpression sqlExpression1 = this.sql.Add(mc.Arguments[0], 1);
                                sqlExpressionArray[index2] = sqlExpression1;
                                int index3 = 2;
                                SqlExpression sqlExpression2 = mc.Arguments[1];
                                sqlExpressionArray[index3] = sqlExpression2;
                                int index4 = 3;
                                SqlExpression sqlExpression3 = this.sql.ValueFromObject((object)"", false, sourceExpression1);
                                sqlExpressionArray[index4] = sqlExpression3;
                                Expression source = sourceExpression1;
                                return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                            }
                        }
                    }
                    else if (stringHash <= 1128357681U)
                    {
                        if ((int)stringHash != 986892477)
                        {
                            if ((int)stringHash == 1128357681 && name1 == "ToLower" && mc.Arguments.Count == 0)
                            {
                                SqlFactory sqlFactory = this.sql;
                                Type clrType = typeof(string);
                                string name2 = "LOWER";
                                SqlExpression[] sqlExpressionArray = new SqlExpression[1];
                                int index = 0;
                                SqlExpression @object = mc.Object;
                                sqlExpressionArray[index] = @object;
                                Expression source = sourceExpression1;
                                return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                            }
                        }
                        else if (name1 == "CompareTo" && mc.Arguments.Count == 1)
                        {
                            if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value == null)
                                throw Error.ArgumentNull("value");
                            return this.CreateComparison(mc.Object, mc.Arguments[0], sourceExpression1);
                        }
                    }
                    else if ((int)stringHash != 1721518424)
                    {
                        if ((int)stringHash == 1846593938 && name1 == "LastIndexOf")
                        {
                            if (mc.Arguments.Count == 1)
                            {
                                SqlExpression expr = mc.Arguments[0];
                                if (expr is SqlValue && ((SqlValue)expr).Value == null)
                                    throw Error.ArgumentNull("value");
                                SqlExpression @object = mc.Object;
                                SqlFactory sqlFactory1 = this.sql;
                                Type clrType1 = typeof(string);
                                string name2 = "REVERSE";
                                SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
                                int index1 = 0;
                                SqlExpression sqlExpression1 = @object;
                                sqlExpressionArray1[index1] = sqlExpression1;
                                Expression source1 = sourceExpression1;
                                SqlExpression sqlExpression2 = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                                SqlFactory sqlFactory2 = this.sql;
                                Type clrType2 = typeof(string);
                                string name3 = "REVERSE";
                                SqlExpression[] sqlExpressionArray2 = new SqlExpression[1];
                                int index2 = 0;
                                SqlExpression sqlExpression3 = expr;
                                sqlExpressionArray2[index2] = sqlExpression3;
                                Expression source2 = sourceExpression1;
                                SqlExpression sqlExpression4 = (SqlExpression)sqlFactory2.FunctionCall(clrType2, name3, (IEnumerable<SqlExpression>)sqlExpressionArray2, source2);
                                SqlFactory sqlFactory3 = this.sql;
                                Type clrType3 = typeof(int);
                                string name4 = "CHARINDEX";
                                SqlExpression[] sqlExpressionArray3 = new SqlExpression[2];
                                int index3 = 0;
                                SqlExpression sqlExpression5 = expr;
                                sqlExpressionArray3[index3] = sqlExpression5;
                                int index4 = 1;
                                SqlExpression sqlExpression6 = @object;
                                sqlExpressionArray3[index4] = sqlExpression6;
                                Expression source3 = sourceExpression1;
                                SqlExpression left = (SqlExpression)sqlFactory3.FunctionCall(clrType3, name4, (IEnumerable<SqlExpression>)sqlExpressionArray3, source3);
                                SqlFactory sqlFactory4 = this.sql;
                                Type clrType4 = typeof(int);
                                string name5 = "CHARINDEX";
                                SqlExpression[] sqlExpressionArray4 = new SqlExpression[2];
                                int index5 = 0;
                                SqlExpression sqlExpression7 = sqlExpression4;
                                sqlExpressionArray4[index5] = sqlExpression7;
                                int index6 = 1;
                                SqlExpression sqlExpression8 = sqlExpression2;
                                sqlExpressionArray4[index6] = sqlExpression8;
                                Expression source4 = sourceExpression1;
                                SqlExpression sqlExpression9 = (SqlExpression)sqlFactory4.FunctionCall(clrType4, name5, (IEnumerable<SqlExpression>)sqlExpressionArray4, source4);
                                SqlExpression match = (SqlExpression)this.sql.Binary(SqlNodeType.EQ, left, this.sql.ValueFromObject((object)0, false, sourceExpression1));
                                SqlExpression sqlExpression10 = this.sql.CLRLENGTH(@object);
                                SqlExpression sqlExpression11 = this.sql.CLRLENGTH(expr);
                                SqlFactory sqlFactory5 = this.sql;
                                SqlExpression[] sqlExpressionArray5 = new SqlExpression[2];
                                int index7 = 0;
                                SqlExpression sqlExpression12 = this.sql.ValueFromObject((object)1, false, sourceExpression1);
                                sqlExpressionArray5[index7] = sqlExpression12;
                                int index8 = 1;
                                SqlFactory sqlFactory6 = this.sql;
                                SqlExpression first = sqlExpression10;
                                SqlFactory sqlFactory7 = this.sql;
                                SqlExpression[] sqlExpressionArray6 = new SqlExpression[2];
                                int index9 = 0;
                                SqlExpression sqlExpression13 = sqlExpression11;
                                sqlExpressionArray6[index9] = sqlExpression13;
                                int index10 = 1;
                                SqlExpression sqlExpression14 = sqlExpression9;
                                sqlExpressionArray6[index10] = sqlExpression14;
                                SqlExpression second = sqlFactory7.Add(sqlExpressionArray6);
                                SqlExpression sqlExpression15 = sqlFactory6.Subtract(first, second);
                                sqlExpressionArray5[index8] = sqlExpression15;
                                SqlExpression sqlExpression16 = sqlFactory5.Add(sqlExpressionArray5);
                                SqlWhen sqlWhen1 = new SqlWhen(match, this.sql.ValueFromObject((object)-1, false, sourceExpression1));
                                SqlWhen sqlWhen2 = new SqlWhen((SqlExpression)this.sql.Binary(SqlNodeType.EQ, this.sql.CLRLENGTH(mc.Arguments[0]), this.sql.ValueFromObject((object)0, sourceExpression1)), this.sql.Subtract(this.sql.CLRLENGTH(@object), 1));
                                SqlFactory sqlFactory8 = this.sql;
                                SqlWhen[] whens = new SqlWhen[2];
                                int index11 = 0;
                                SqlWhen sqlWhen3 = sqlWhen2;
                                whens[index11] = sqlWhen3;
                                int index12 = 1;
                                SqlWhen sqlWhen4 = sqlWhen1;
                                whens[index12] = sqlWhen4;
                                SqlExpression @else = sqlExpression16;
                                Expression sourceExpression2 = sourceExpression1;
                                return (SqlExpression)sqlFactory8.SearchedCase(whens, @else, sourceExpression2);
                            }
                            if (mc.Arguments.Count == 2)
                            {
                                if (mc.Arguments[1].ClrType == typeof(StringComparison))
                                    throw Error.LastIndexOfWithStringComparisonArgNotSupported();
                                SqlExpression @object = mc.Object;
                                SqlExpression expr1 = mc.Arguments[0];
                                if (expr1 is SqlValue && ((SqlValue)expr1).Value == null)
                                    throw Error.ArgumentNull("value");
                                SqlExpression expr2 = mc.Arguments[1];
                                SqlFactory sqlFactory1 = this.sql;
                                Type clrType1 = typeof(string);
                                string name2 = "LEFT";
                                SqlExpression[] sqlExpressionArray1 = new SqlExpression[2];
                                int index1 = 0;
                                SqlExpression sqlExpression1 = @object;
                                sqlExpressionArray1[index1] = sqlExpression1;
                                int index2 = 1;
                                SqlExpression sqlExpression2 = this.sql.Add(expr2, 1);
                                sqlExpressionArray1[index2] = sqlExpression2;
                                Expression source1 = sourceExpression1;
                                SqlExpression expr3 = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                                SqlFactory sqlFactory2 = this.sql;
                                Type clrType2 = typeof(string);
                                string name3 = "REVERSE";
                                SqlExpression[] sqlExpressionArray2 = new SqlExpression[1];
                                int index3 = 0;
                                SqlExpression sqlExpression3 = expr3;
                                sqlExpressionArray2[index3] = sqlExpression3;
                                Expression source2 = sourceExpression1;
                                SqlExpression sqlExpression4 = (SqlExpression)sqlFactory2.FunctionCall(clrType2, name3, (IEnumerable<SqlExpression>)sqlExpressionArray2, source2);
                                SqlFactory sqlFactory3 = this.sql;
                                Type clrType3 = typeof(string);
                                string name4 = "REVERSE";
                                SqlExpression[] sqlExpressionArray3 = new SqlExpression[1];
                                int index4 = 0;
                                SqlExpression sqlExpression5 = expr1;
                                sqlExpressionArray3[index4] = sqlExpression5;
                                Expression source3 = sourceExpression1;
                                SqlExpression sqlExpression6 = (SqlExpression)sqlFactory3.FunctionCall(clrType3, name4, (IEnumerable<SqlExpression>)sqlExpressionArray3, source3);
                                SqlFactory sqlFactory4 = this.sql;
                                Type clrType4 = typeof(int);
                                string name5 = "CHARINDEX";
                                SqlExpression[] sqlExpressionArray4 = new SqlExpression[2];
                                int index5 = 0;
                                SqlExpression sqlExpression7 = expr1;
                                sqlExpressionArray4[index5] = sqlExpression7;
                                int index6 = 1;
                                SqlExpression sqlExpression8 = expr3;
                                sqlExpressionArray4[index6] = sqlExpression8;
                                Expression source4 = sourceExpression1;
                                SqlExpression left = (SqlExpression)sqlFactory4.FunctionCall(clrType4, name5, (IEnumerable<SqlExpression>)sqlExpressionArray4, source4);
                                SqlFactory sqlFactory5 = this.sql;
                                Type clrType5 = typeof(int);
                                string name6 = "CHARINDEX";
                                SqlExpression[] sqlExpressionArray5 = new SqlExpression[2];
                                int index7 = 0;
                                SqlExpression sqlExpression9 = sqlExpression6;
                                sqlExpressionArray5[index7] = sqlExpression9;
                                int index8 = 1;
                                SqlExpression sqlExpression10 = sqlExpression4;
                                sqlExpressionArray5[index8] = sqlExpression10;
                                Expression source5 = sourceExpression1;
                                SqlExpression sqlExpression11 = (SqlExpression)sqlFactory5.FunctionCall(clrType5, name6, (IEnumerable<SqlExpression>)sqlExpressionArray5, source5);
                                SqlExpression match = (SqlExpression)this.sql.Binary(SqlNodeType.EQ, left, this.sql.ValueFromObject((object)0, false, sourceExpression1));
                                SqlExpression sqlExpression12 = this.sql.CLRLENGTH(expr3);
                                SqlExpression sqlExpression13 = this.sql.CLRLENGTH(expr1);
                                SqlFactory sqlFactory6 = this.sql;
                                SqlExpression[] sqlExpressionArray6 = new SqlExpression[2];
                                int index9 = 0;
                                SqlExpression sqlExpression14 = this.sql.ValueFromObject((object)1, false, sourceExpression1);
                                sqlExpressionArray6[index9] = sqlExpression14;
                                int index10 = 1;
                                SqlFactory sqlFactory7 = this.sql;
                                SqlExpression first = sqlExpression12;
                                SqlFactory sqlFactory8 = this.sql;
                                SqlExpression[] sqlExpressionArray7 = new SqlExpression[2];
                                int index11 = 0;
                                SqlExpression sqlExpression15 = sqlExpression13;
                                sqlExpressionArray7[index11] = sqlExpression15;
                                int index12 = 1;
                                SqlExpression sqlExpression16 = sqlExpression11;
                                sqlExpressionArray7[index12] = sqlExpression16;
                                SqlExpression second = sqlFactory8.Add(sqlExpressionArray7);
                                SqlExpression sqlExpression17 = sqlFactory7.Subtract(first, second);
                                sqlExpressionArray6[index10] = sqlExpression17;
                                SqlExpression sqlExpression18 = sqlFactory6.Add(sqlExpressionArray6);
                                SqlWhen sqlWhen1 = new SqlWhen(match, this.sql.ValueFromObject((object)-1, false, sourceExpression1));
                                SqlWhen sqlWhen2 = new SqlWhen(this.sql.AndAccumulate((SqlExpression)this.sql.Binary(SqlNodeType.EQ, this.sql.CLRLENGTH(mc.Arguments[0]), this.sql.ValueFromObject((object)0, sourceExpression1)), (SqlExpression)this.sql.Binary(SqlNodeType.LE, this.sql.Add(mc.Arguments[1], 1), this.sql.CLRLENGTH(@object))), mc.Arguments[1]);
                                SqlFactory sqlFactory9 = this.sql;
                                SqlWhen[] whens = new SqlWhen[2];
                                int index13 = 0;
                                SqlWhen sqlWhen3 = sqlWhen2;
                                whens[index13] = sqlWhen3;
                                int index14 = 1;
                                SqlWhen sqlWhen4 = sqlWhen1;
                                whens[index14] = sqlWhen4;
                                SqlExpression @else = sqlExpression18;
                                Expression sourceExpression2 = sourceExpression1;
                                return (SqlExpression)sqlFactory9.SearchedCase(whens, @else, sourceExpression2);
                            }
                            if (mc.Arguments.Count == 3)
                            {
                                if (mc.Arguments[2].ClrType == typeof(StringComparison))
                                    throw Error.LastIndexOfWithStringComparisonArgNotSupported();
                                SqlExpression @object = mc.Object;
                                SqlExpression expr1 = mc.Arguments[0];
                                if (expr1 is SqlValue && ((SqlValue)expr1).Value == null)
                                    throw Error.ArgumentNull("value");
                                SqlExpression sqlExpression1 = mc.Arguments[1];
                                SqlExpression second1 = mc.Arguments[2];
                                SqlFactory sqlFactory1 = this.sql;
                                Type clrType1 = typeof(string);
                                string name2 = "LEFT";
                                SqlExpression[] sqlExpressionArray1 = new SqlExpression[2];
                                int index1 = 0;
                                SqlExpression sqlExpression2 = @object;
                                sqlExpressionArray1[index1] = sqlExpression2;
                                int index2 = 1;
                                SqlExpression sqlExpression3 = this.sql.Add(sqlExpression1, 1);
                                sqlExpressionArray1[index2] = sqlExpression3;
                                Expression source1 = sourceExpression1;
                                SqlExpression expr2 = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                                SqlFactory sqlFactory2 = this.sql;
                                Type clrType2 = typeof(string);
                                string name3 = "REVERSE";
                                SqlExpression[] sqlExpressionArray2 = new SqlExpression[1];
                                int index3 = 0;
                                SqlExpression sqlExpression4 = expr2;
                                sqlExpressionArray2[index3] = sqlExpression4;
                                Expression source2 = sourceExpression1;
                                SqlExpression sqlExpression5 = (SqlExpression)sqlFactory2.FunctionCall(clrType2, name3, (IEnumerable<SqlExpression>)sqlExpressionArray2, source2);
                                SqlFactory sqlFactory3 = this.sql;
                                Type clrType3 = typeof(string);
                                string name4 = "REVERSE";
                                SqlExpression[] sqlExpressionArray3 = new SqlExpression[1];
                                int index4 = 0;
                                SqlExpression sqlExpression6 = expr1;
                                sqlExpressionArray3[index4] = sqlExpression6;
                                Expression source3 = sourceExpression1;
                                SqlExpression sqlExpression7 = (SqlExpression)sqlFactory3.FunctionCall(clrType3, name4, (IEnumerable<SqlExpression>)sqlExpressionArray3, source3);
                                SqlFactory sqlFactory4 = this.sql;
                                Type clrType4 = typeof(int);
                                string name5 = "CHARINDEX";
                                SqlExpression[] sqlExpressionArray4 = new SqlExpression[2];
                                int index5 = 0;
                                SqlExpression sqlExpression8 = expr1;
                                sqlExpressionArray4[index5] = sqlExpression8;
                                int index6 = 1;
                                SqlExpression sqlExpression9 = expr2;
                                sqlExpressionArray4[index6] = sqlExpression9;
                                Expression source4 = sourceExpression1;
                                SqlExpression left1 = (SqlExpression)sqlFactory4.FunctionCall(clrType4, name5, (IEnumerable<SqlExpression>)sqlExpressionArray4, source4);
                                SqlFactory sqlFactory5 = this.sql;
                                Type clrType5 = typeof(int);
                                string name6 = "CHARINDEX";
                                SqlExpression[] sqlExpressionArray5 = new SqlExpression[2];
                                int index7 = 0;
                                SqlExpression sqlExpression10 = sqlExpression7;
                                sqlExpressionArray5[index7] = sqlExpression10;
                                int index8 = 1;
                                SqlExpression sqlExpression11 = sqlExpression5;
                                sqlExpressionArray5[index8] = sqlExpression11;
                                Expression source5 = sourceExpression1;
                                SqlExpression sqlExpression12 = (SqlExpression)sqlFactory5.FunctionCall(clrType5, name6, (IEnumerable<SqlExpression>)sqlExpressionArray5, source5);
                                SqlExpression sqlExpression13 = this.sql.CLRLENGTH(expr2);
                                SqlExpression sqlExpression14 = this.sql.CLRLENGTH(expr1);
                                SqlFactory sqlFactory6 = this.sql;
                                SqlExpression[] sqlExpressionArray6 = new SqlExpression[2];
                                int index9 = 0;
                                SqlExpression sqlExpression15 = this.sql.ValueFromObject((object)1, false, sourceExpression1);
                                sqlExpressionArray6[index9] = sqlExpression15;
                                int index10 = 1;
                                SqlFactory sqlFactory7 = this.sql;
                                SqlExpression first = sqlExpression13;
                                SqlFactory sqlFactory8 = this.sql;
                                SqlExpression[] sqlExpressionArray7 = new SqlExpression[2];
                                int index11 = 0;
                                SqlExpression sqlExpression16 = sqlExpression14;
                                sqlExpressionArray7[index11] = sqlExpression16;
                                int index12 = 1;
                                SqlExpression sqlExpression17 = sqlExpression12;
                                sqlExpressionArray7[index12] = sqlExpression17;
                                SqlExpression second2 = sqlFactory8.Add(sqlExpressionArray7);
                                SqlExpression sqlExpression18 = sqlFactory7.Subtract(first, second2);
                                sqlExpressionArray6[index10] = sqlExpression18;
                                SqlExpression left2 = sqlFactory6.Add(sqlExpressionArray6);
                                SqlWhen sqlWhen1 = new SqlWhen(this.sql.OrAccumulate((SqlExpression)this.sql.Binary(SqlNodeType.EQ, left1, this.sql.ValueFromObject((object)0, false, sourceExpression1)), (SqlExpression)this.sql.Binary(SqlNodeType.LE, left2, this.sql.Subtract(sqlExpression1, second1))), this.sql.ValueFromObject((object)-1, false, sourceExpression1));
                                SqlWhen sqlWhen2 = new SqlWhen(this.sql.AndAccumulate((SqlExpression)this.sql.Binary(SqlNodeType.EQ, this.sql.CLRLENGTH(mc.Arguments[0]), this.sql.ValueFromObject((object)0, sourceExpression1)), (SqlExpression)this.sql.Binary(SqlNodeType.LE, this.sql.Add(mc.Arguments[1], 1), this.sql.CLRLENGTH(@object))), mc.Arguments[1]);
                                SqlFactory sqlFactory9 = this.sql;
                                SqlWhen[] whens = new SqlWhen[2];
                                int index13 = 0;
                                SqlWhen sqlWhen3 = sqlWhen2;
                                whens[index13] = sqlWhen3;
                                int index14 = 1;
                                SqlWhen sqlWhen4 = sqlWhen1;
                                whens[index14] = sqlWhen4;
                                SqlExpression @else = left2;
                                Expression sourceExpression2 = sourceExpression1;
                                return (SqlExpression)sqlFactory9.SearchedCase(whens, @else, sourceExpression2);
                            }
                        }
                    }
                    else if (name1 == "Contains" && mc.Arguments.Count == 1)
                    {
                        SqlExpression sqlExpression = mc.Arguments[0];
                        SqlExpression escape = (SqlExpression)null;
                        bool usedEscapeChar = true;
                        SqlExpression pattern;
                        if (sqlExpression.NodeType == SqlNodeType.Value)
                        {
                            pattern = this.sql.ValueFromObject((object)SqlHelpers.GetStringContainsPattern((string)((SqlValue)sqlExpression).Value, '~', out usedEscapeChar), true, sqlExpression.SourceExpression);
                        }
                        else
                        {
                            if (sqlExpression.NodeType != SqlNodeType.ClientParameter)
                                throw Error.NonConstantExpressionsNotSupportedFor((object)"String.Contains");
                            SqlClientParameter sqlClientParameter = (SqlClientParameter)sqlExpression;
                            Func<string, char, string> func = new Func<string, char, string>(SqlHelpers.GetStringContainsPatternForced);
                            Type clrType = sqlClientParameter.ClrType;
                            ProviderType sqlType = sqlClientParameter.SqlType;
                            ConstantExpression constantExpression1 = Expression.Constant((object)func);
                            Expression[] expressionArray = new Expression[2];
                            int index1 = 0;
                            Expression body = sqlClientParameter.Accessor.Body;
                            expressionArray[index1] = body;
                            int index2 = 1;
                            ConstantExpression constantExpression2 = Expression.Constant((object)'~');
                            expressionArray[index2] = (Expression)constantExpression2;
                            InvocationExpression invocationExpression = Expression.Invoke((Expression)constantExpression1, expressionArray);
                            ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                            int index3 = 0;
                            ParameterExpression parameterExpression = sqlClientParameter.Accessor.Parameters[0];
                            parameterExpressionArray[index3] = parameterExpression;
                            LambdaExpression accessor = Expression.Lambda((Expression)invocationExpression, parameterExpressionArray);
                            Expression sourceExpression2 = sqlClientParameter.SourceExpression;
                            pattern = (SqlExpression)new SqlClientParameter(clrType, sqlType, accessor, sourceExpression2);
                        }
                        if (usedEscapeChar)
                            escape = this.sql.ValueFromObject((object)"~", false, sourceExpression1);
                        return (SqlExpression)this.sql.Like(mc.Object, pattern, escape, sourceExpression1);
                    }
                }
                else if (stringHash <= 3243991448U)
                {
                    if (stringHash <= 3035490584U)
                    {
                        if ((int)stringHash != -1428418018)
                        {
                            if ((int)stringHash == -1259476712 && name1 == "ToUpper" && mc.Arguments.Count == 0)
                            {
                                SqlFactory sqlFactory = this.sql;
                                Type clrType = typeof(string);
                                string name2 = "UPPER";
                                SqlExpression[] sqlExpressionArray = new SqlExpression[1];
                                int index = 0;
                                SqlExpression @object = mc.Object;
                                sqlExpressionArray[index] = @object;
                                Expression source = sourceExpression1;
                                return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                            }
                        }
                        else if (name1 == "IndexOf")
                        {
                            if (mc.Arguments.Count == 1)
                            {
                                if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value == null)
                                    throw Error.ArgumentNull("value");
                                SqlWhen sqlWhen1 = new SqlWhen((SqlExpression)this.sql.Binary(SqlNodeType.EQ, this.sql.CLRLENGTH(mc.Arguments[0]), this.sql.ValueFromObject((object)0, sourceExpression1)), this.sql.ValueFromObject((object)0, sourceExpression1));
                                SqlFactory sqlFactory1 = this.sql;
                                SqlFactory sqlFactory2 = this.sql;
                                Type clrType = typeof(int);
                                string name2 = "CHARINDEX";
                                SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                                int index1 = 0;
                                SqlExpression sqlExpression1 = mc.Arguments[0];
                                sqlExpressionArray[index1] = sqlExpression1;
                                int index2 = 1;
                                SqlExpression @object = mc.Object;
                                sqlExpressionArray[index2] = @object;
                                Expression source = sourceExpression1;
                                SqlFunctionCall sqlFunctionCall = sqlFactory2.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                                int second = 1;
                                SqlExpression sqlExpression2 = sqlFactory1.Subtract((SqlExpression)sqlFunctionCall, second);
                                SqlFactory sqlFactory3 = this.sql;
                                SqlWhen[] whens = new SqlWhen[1];
                                int index3 = 0;
                                SqlWhen sqlWhen2 = sqlWhen1;
                                whens[index3] = sqlWhen2;
                                SqlExpression @else = sqlExpression2;
                                Expression sourceExpression2 = sourceExpression1;
                                return (SqlExpression)sqlFactory3.SearchedCase(whens, @else, sourceExpression2);
                            }
                            if (mc.Arguments.Count == 2)
                            {
                                if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value == null)
                                    throw Error.ArgumentNull("value");
                                if (mc.Arguments[1].ClrType == typeof(StringComparison))
                                    throw Error.IndexOfWithStringComparisonArgNotSupported();
                                SqlWhen sqlWhen1 = new SqlWhen(this.sql.AndAccumulate((SqlExpression)this.sql.Binary(SqlNodeType.EQ, this.sql.CLRLENGTH(mc.Arguments[0]), this.sql.ValueFromObject((object)0, sourceExpression1)), (SqlExpression)this.sql.Binary(SqlNodeType.LE, this.sql.Add(mc.Arguments[1], 1), this.sql.CLRLENGTH(mc.Object))), mc.Arguments[1]);
                                SqlFactory sqlFactory1 = this.sql;
                                SqlFactory sqlFactory2 = this.sql;
                                Type clrType = typeof(int);
                                string name2 = "CHARINDEX";
                                SqlExpression[] sqlExpressionArray = new SqlExpression[3];
                                int index1 = 0;
                                SqlExpression sqlExpression1 = mc.Arguments[0];
                                sqlExpressionArray[index1] = sqlExpression1;
                                int index2 = 1;
                                SqlExpression @object = mc.Object;
                                sqlExpressionArray[index2] = @object;
                                int index3 = 2;
                                SqlExpression sqlExpression2 = this.sql.Add(mc.Arguments[1], 1);
                                sqlExpressionArray[index3] = sqlExpression2;
                                Expression source = sourceExpression1;
                                SqlFunctionCall sqlFunctionCall = sqlFactory2.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                                int second = 1;
                                SqlExpression sqlExpression3 = sqlFactory1.Subtract((SqlExpression)sqlFunctionCall, second);
                                SqlFactory sqlFactory3 = this.sql;
                                SqlWhen[] whens = new SqlWhen[1];
                                int index4 = 0;
                                SqlWhen sqlWhen2 = sqlWhen1;
                                whens[index4] = sqlWhen2;
                                SqlExpression @else = sqlExpression3;
                                Expression sourceExpression2 = sourceExpression1;
                                return (SqlExpression)sqlFactory3.SearchedCase(whens, @else, sourceExpression2);
                            }
                            if (mc.Arguments.Count == 3)
                            {
                                if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value == null)
                                    throw Error.ArgumentNull("value");
                                if (mc.Arguments[2].ClrType == typeof(StringComparison))
                                    throw Error.IndexOfWithStringComparisonArgNotSupported();
                                SqlWhen sqlWhen1 = new SqlWhen(this.sql.AndAccumulate((SqlExpression)this.sql.Binary(SqlNodeType.EQ, this.sql.CLRLENGTH(mc.Arguments[0]), this.sql.ValueFromObject((object)0, sourceExpression1)), (SqlExpression)this.sql.Binary(SqlNodeType.LE, this.sql.Add(mc.Arguments[1], 1), this.sql.CLRLENGTH(mc.Object))), mc.Arguments[1]);
                                SqlFactory sqlFactory1 = this.sql;
                                Type clrType1 = typeof(string);
                                string name2 = "SUBSTRING";
                                SqlExpression[] sqlExpressionArray1 = new SqlExpression[3];
                                int index1 = 0;
                                SqlExpression @object = mc.Object;
                                sqlExpressionArray1[index1] = @object;
                                int index2 = 1;
                                SqlExpression sqlExpression1 = this.sql.ValueFromObject((object)1, false, sourceExpression1);
                                sqlExpressionArray1[index2] = sqlExpression1;
                                int index3 = 2;
                                SqlFactory sqlFactory2 = this.sql;
                                SqlExpression[] sqlExpressionArray2 = new SqlExpression[2];
                                int index4 = 0;
                                SqlExpression sqlExpression2 = mc.Arguments[1];
                                sqlExpressionArray2[index4] = sqlExpression2;
                                int index5 = 1;
                                SqlExpression sqlExpression3 = mc.Arguments[2];
                                sqlExpressionArray2[index5] = sqlExpression3;
                                SqlExpression sqlExpression4 = sqlFactory2.Add(sqlExpressionArray2);
                                sqlExpressionArray1[index3] = sqlExpression4;
                                Expression source1 = sourceExpression1;
                                SqlExpression sqlExpression5 = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                                SqlFactory sqlFactory3 = this.sql;
                                SqlFactory sqlFactory4 = this.sql;
                                Type clrType2 = typeof(int);
                                string name3 = "CHARINDEX";
                                SqlExpression[] sqlExpressionArray3 = new SqlExpression[3];
                                int index6 = 0;
                                SqlExpression sqlExpression6 = mc.Arguments[0];
                                sqlExpressionArray3[index6] = sqlExpression6;
                                int index7 = 1;
                                SqlExpression sqlExpression7 = sqlExpression5;
                                sqlExpressionArray3[index7] = sqlExpression7;
                                int index8 = 2;
                                SqlExpression sqlExpression8 = this.sql.Add(mc.Arguments[1], 1);
                                sqlExpressionArray3[index8] = sqlExpression8;
                                Expression source2 = sourceExpression1;
                                SqlFunctionCall sqlFunctionCall = sqlFactory4.FunctionCall(clrType2, name3, (IEnumerable<SqlExpression>)sqlExpressionArray3, source2);
                                int second = 1;
                                SqlExpression sqlExpression9 = sqlFactory3.Subtract((SqlExpression)sqlFunctionCall, second);
                                SqlFactory sqlFactory5 = this.sql;
                                SqlWhen[] whens = new SqlWhen[1];
                                int index9 = 0;
                                SqlWhen sqlWhen2 = sqlWhen1;
                                whens[index9] = sqlWhen2;
                                SqlExpression @else = sqlExpression9;
                                Expression sourceExpression2 = sourceExpression1;
                                return (SqlExpression)sqlFactory5.SearchedCase(whens, @else, sourceExpression2);
                            }
                        }
                    }
                    else if ((int)stringHash != -1142066180)
                    {
                        if ((int)stringHash == -1050975848 && name1 == "PadRight")
                        {
                            if (mc.Arguments.Count == 1)
                            {
                                SqlExpression @object = mc.Object;
                                SqlExpression sqlExpression1 = mc.Arguments[0];
                                SqlExpression match = (SqlExpression)this.sql.Binary(SqlNodeType.GE, this.sql.CLRLENGTH(@object), sqlExpression1);
                                SqlExpression second = this.sql.CLRLENGTH(@object);
                                SqlExpression sqlExpression2 = this.sql.Subtract(sqlExpression1, second);
                                SqlFactory sqlFactory1 = this.sql;
                                Type clrType = typeof(string);
                                string name2 = "SPACE";
                                SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
                                int index1 = 0;
                                SqlExpression sqlExpression3 = sqlExpression2;
                                sqlExpressionArray1[index1] = sqlExpression3;
                                Expression source = sourceExpression1;
                                SqlExpression sqlExpression4 = (SqlExpression)sqlFactory1.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source);
                                SqlFactory sqlFactory2 = this.sql;
                                SqlExpression[] sqlExpressionArray2 = new SqlExpression[2];
                                int index2 = 0;
                                SqlExpression sqlExpression5 = @object;
                                sqlExpressionArray2[index2] = sqlExpression5;
                                int index3 = 1;
                                SqlExpression sqlExpression6 = sqlExpression4;
                                sqlExpressionArray2[index3] = sqlExpression6;
                                SqlExpression sqlExpression7 = sqlFactory2.Concat(sqlExpressionArray2);
                                SqlFactory sqlFactory3 = this.sql;
                                SqlWhen[] whens = new SqlWhen[1];
                                int index4 = 0;
                                SqlWhen sqlWhen = new SqlWhen(match, @object);
                                whens[index4] = sqlWhen;
                                SqlExpression @else = sqlExpression7;
                                Expression sourceExpression2 = sourceExpression1;
                                return (SqlExpression)sqlFactory3.SearchedCase(whens, @else, sourceExpression2);
                            }
                            if (mc.Arguments.Count == 2)
                            {
                                SqlExpression @object = mc.Object;
                                SqlExpression sqlExpression1 = mc.Arguments[0];
                                SqlExpression sqlExpression2 = mc.Arguments[1];
                                SqlExpression match = (SqlExpression)this.sql.Binary(SqlNodeType.GE, this.sql.CLRLENGTH(@object), sqlExpression1);
                                SqlExpression second = this.sql.CLRLENGTH(@object);
                                SqlExpression sqlExpression3 = this.sql.Subtract(sqlExpression1, second);
                                SqlFactory sqlFactory1 = this.sql;
                                Type clrType = typeof(string);
                                string name2 = "REPLICATE";
                                SqlExpression[] sqlExpressionArray1 = new SqlExpression[2];
                                int index1 = 0;
                                SqlExpression sqlExpression4 = sqlExpression2;
                                sqlExpressionArray1[index1] = sqlExpression4;
                                int index2 = 1;
                                SqlExpression sqlExpression5 = sqlExpression3;
                                sqlExpressionArray1[index2] = sqlExpression5;
                                Expression source = sourceExpression1;
                                SqlExpression sqlExpression6 = (SqlExpression)sqlFactory1.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source);
                                SqlFactory sqlFactory2 = this.sql;
                                SqlExpression[] sqlExpressionArray2 = new SqlExpression[2];
                                int index3 = 0;
                                SqlExpression sqlExpression7 = @object;
                                sqlExpressionArray2[index3] = sqlExpression7;
                                int index4 = 1;
                                SqlExpression sqlExpression8 = sqlExpression6;
                                sqlExpressionArray2[index4] = sqlExpression8;
                                SqlExpression sqlExpression9 = sqlFactory2.Concat(sqlExpressionArray2);
                                SqlFactory sqlFactory3 = this.sql;
                                SqlWhen[] whens = new SqlWhen[1];
                                int index5 = 0;
                                SqlWhen sqlWhen = new SqlWhen(match, @object);
                                whens[index5] = sqlWhen;
                                SqlExpression @else = sqlExpression9;
                                Expression sourceExpression2 = sourceExpression1;
                                return (SqlExpression)sqlFactory3.SearchedCase(whens, @else, sourceExpression2);
                            }
                        }
                    }
                    else if (name1 == "StartsWith" && mc.Arguments.Count == 1)
                    {
                        SqlExpression sqlExpression = mc.Arguments[0];
                        SqlExpression escape = (SqlExpression)null;
                        bool usedEscapeChar = true;
                        SqlExpression pattern;
                        if (sqlExpression.NodeType == SqlNodeType.Value)
                        {
                            pattern = this.sql.ValueFromObject((object)SqlHelpers.GetStringStartsWithPattern((string)((SqlValue)sqlExpression).Value, '~', out usedEscapeChar), true, sqlExpression.SourceExpression);
                        }
                        else
                        {
                            if (sqlExpression.NodeType != SqlNodeType.ClientParameter)
                                throw Error.NonConstantExpressionsNotSupportedFor((object)"String.StartsWith");
                            SqlClientParameter sqlClientParameter = (SqlClientParameter)sqlExpression;
                            Func<string, char, string> func = new Func<string, char, string>(SqlHelpers.GetStringStartsWithPatternForced);
                            Type clrType = sqlClientParameter.ClrType;
                            ProviderType sqlType = sqlClientParameter.SqlType;
                            ConstantExpression constantExpression1 = Expression.Constant((object)func);
                            Expression[] expressionArray = new Expression[2];
                            int index1 = 0;
                            Expression body = sqlClientParameter.Accessor.Body;
                            expressionArray[index1] = body;
                            int index2 = 1;
                            ConstantExpression constantExpression2 = Expression.Constant((object)'~');
                            expressionArray[index2] = (Expression)constantExpression2;
                            InvocationExpression invocationExpression = Expression.Invoke((Expression)constantExpression1, expressionArray);
                            ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                            int index3 = 0;
                            ParameterExpression parameterExpression = sqlClientParameter.Accessor.Parameters[0];
                            parameterExpressionArray[index3] = parameterExpression;
                            LambdaExpression accessor = Expression.Lambda((Expression)invocationExpression, parameterExpressionArray);
                            Expression sourceExpression2 = sqlClientParameter.SourceExpression;
                            pattern = (SqlExpression)new SqlClientParameter(clrType, sqlType, accessor, sourceExpression2);
                        }
                        if (usedEscapeChar)
                            escape = this.sql.ValueFromObject((object)"~", false, sourceExpression1);
                        return (SqlExpression)this.sql.Like(mc.Object, pattern, escape, sourceExpression1);
                    }
                }
                else if (stringHash <= 3839184739U)
                {
                    if ((int)stringHash != -1028557880)
                    {
                        if ((int)stringHash == -455782557 && name1 == "Replace")
                        {
                            if (mc.Arguments[0] is SqlValue && ((SqlValue)mc.Arguments[0]).Value == null)
                                throw Error.ArgumentNull("old");
                            if (mc.Arguments[1] is SqlValue && ((SqlValue)mc.Arguments[1]).Value == null)
                                throw Error.ArgumentNull("new");
                            SqlFactory sqlFactory = this.sql;
                            Type clrType = typeof(string);
                            string name2 = "REPLACE";
                            SqlExpression[] sqlExpressionArray = new SqlExpression[3];
                            int index1 = 0;
                            SqlExpression @object = mc.Object;
                            sqlExpressionArray[index1] = @object;
                            int index2 = 1;
                            SqlExpression sqlExpression1 = mc.Arguments[0];
                            sqlExpressionArray[index2] = sqlExpression1;
                            int index3 = 2;
                            SqlExpression sqlExpression2 = mc.Arguments[1];
                            sqlExpressionArray[index3] = sqlExpression2;
                            Expression source = sourceExpression1;
                            return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                        }
                    }
                    else if (name1 == "Substring")
                    {
                        if (mc.Arguments.Count == 1)
                        {
                            SqlFactory sqlFactory = this.sql;
                            Type clrType = typeof(string);
                            string name2 = "SUBSTRING";
                            SqlExpression[] sqlExpressionArray = new SqlExpression[3];
                            int index1 = 0;
                            SqlExpression @object = mc.Object;
                            sqlExpressionArray[index1] = @object;
                            int index2 = 1;
                            SqlExpression sqlExpression1 = this.sql.Add(mc.Arguments[0], 1);
                            sqlExpressionArray[index2] = sqlExpression1;
                            int index3 = 2;
                            SqlExpression sqlExpression2 = this.sql.CLRLENGTH(mc.Object);
                            sqlExpressionArray[index3] = sqlExpression2;
                            Expression source = sourceExpression1;
                            return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                        }
                        if (mc.Arguments.Count == 2)
                        {
                            SqlFactory sqlFactory = this.sql;
                            Type clrType = typeof(string);
                            string name2 = "SUBSTRING";
                            SqlExpression[] sqlExpressionArray = new SqlExpression[3];
                            int index1 = 0;
                            SqlExpression @object = mc.Object;
                            sqlExpressionArray[index1] = @object;
                            int index2 = 1;
                            SqlExpression sqlExpression1 = this.sql.Add(mc.Arguments[0], 1);
                            sqlExpressionArray[index2] = sqlExpression1;
                            int index3 = 2;
                            SqlExpression sqlExpression2 = mc.Arguments[1];
                            sqlExpressionArray[index3] = sqlExpression2;
                            Expression source = sourceExpression1;
                            return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                        }
                    }
                }
                else if ((int)stringHash != -171931671)
                {
                    if ((int)stringHash == -31485789 && name1 == "EndsWith" && mc.Arguments.Count == 1)
                    {
                        SqlExpression sqlExpression = mc.Arguments[0];
                        SqlExpression escape = (SqlExpression)null;
                        bool usedEscapeChar = true;
                        SqlExpression pattern;
                        if (sqlExpression.NodeType == SqlNodeType.Value)
                        {
                            pattern = this.sql.ValueFromObject((object)SqlHelpers.GetStringEndsWithPattern((string)((SqlValue)sqlExpression).Value, '~', out usedEscapeChar), true, sqlExpression.SourceExpression);
                        }
                        else
                        {
                            if (sqlExpression.NodeType != SqlNodeType.ClientParameter)
                                throw Error.NonConstantExpressionsNotSupportedFor((object)"String.EndsWith");
                            SqlClientParameter sqlClientParameter = (SqlClientParameter)sqlExpression;
                            Func<string, char, string> func = new Func<string, char, string>(SqlHelpers.GetStringEndsWithPatternForced);
                            Type clrType = sqlClientParameter.ClrType;
                            ProviderType sqlType = sqlClientParameter.SqlType;
                            ConstantExpression constantExpression1 = Expression.Constant((object)func);
                            Expression[] expressionArray = new Expression[2];
                            int index1 = 0;
                            Expression body = sqlClientParameter.Accessor.Body;
                            expressionArray[index1] = body;
                            int index2 = 1;
                            ConstantExpression constantExpression2 = Expression.Constant((object)'~');
                            expressionArray[index2] = (Expression)constantExpression2;
                            InvocationExpression invocationExpression = Expression.Invoke((Expression)constantExpression1, expressionArray);
                            ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                            int index3 = 0;
                            ParameterExpression parameterExpression = sqlClientParameter.Accessor.Parameters[0];
                            parameterExpressionArray[index3] = parameterExpression;
                            LambdaExpression accessor = Expression.Lambda((Expression)invocationExpression, parameterExpressionArray);
                            Expression sourceExpression2 = sqlClientParameter.SourceExpression;
                            pattern = (SqlExpression)new SqlClientParameter(clrType, sqlType, accessor, sourceExpression2);
                        }
                        if (usedEscapeChar)
                            escape = this.sql.ValueFromObject((object)"~", false, sourceExpression1);
                        return (SqlExpression)this.sql.Like(mc.Object, pattern, escape, sourceExpression1);
                    }
                }
                else if (name1 == "Trim" && mc.Arguments.Count == 0)
                {
                    SqlFactory sqlFactory1 = this.sql;
                    Type clrType1 = typeof(string);
                    string name2 = "LTRIM";
                    SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
                    int index1 = 0;
                    SqlFactory sqlFactory2 = this.sql;
                    Type clrType2 = typeof(string);
                    string name3 = "RTRIM";
                    SqlExpression[] sqlExpressionArray2 = new SqlExpression[1];
                    int index2 = 0;
                    SqlExpression @object = mc.Object;
                    sqlExpressionArray2[index2] = @object;
                    Expression source1 = sourceExpression1;
                    SqlFunctionCall sqlFunctionCall = sqlFactory2.FunctionCall(clrType2, name3, (IEnumerable<SqlExpression>)sqlExpressionArray2, source1);
                    sqlExpressionArray1[index1] = (SqlExpression)sqlFunctionCall;
                    Expression source2 = sourceExpression1;
                    return (SqlExpression)sqlFactory1.FunctionCall(clrType1, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source2);
                }
                throw PostBindDotNetConverter.Visitor.GetMethodSupportException(mc);
            }

            private SqlExpression TranslateMathMethod(SqlMethodCall mc)
            {
                Expression sourceExpression1 = mc.SourceExpression;
                string name1 = mc.Method.Name;
                // ISSUE: reference to a compiler-generated method
                uint stringHash = uint.Parse(PrivateImplementationDetails.ComputeStringHash(name1));
                if (stringHash <= 2405646532U)
                {
                    if (stringHash <= 1587810493U)
                    {
                        if (stringHash <= 1017635769U)
                        {
                            if ((int)stringHash != 583084759)
                            {
                                if ((int)stringHash != 781469175)
                                {
                                    if ((int)stringHash == 1017635769 && name1 == "Max" && mc.Arguments.Count == 2)
                                    {
                                        SqlExpression left = mc.Arguments[0];
                                        SqlExpression right = mc.Arguments[1];
                                        SqlExpression match = (SqlExpression)this.sql.Binary(SqlNodeType.LT, left, right);
                                        Type returnType = mc.Method.ReturnType;
                                        SqlWhen[] sqlWhenArray = new SqlWhen[1];
                                        int index = 0;
                                        SqlWhen sqlWhen = new SqlWhen(match, right);
                                        sqlWhenArray[index] = sqlWhen;
                                        SqlExpression @else = left;
                                        Expression sourceExpression2 = sourceExpression1;
                                        return (SqlExpression)new SqlSearchedCase(returnType, (IEnumerable<SqlWhen>)sqlWhenArray, @else, sourceExpression2);
                                    }
                                }
                                else if (name1 == "Min" && mc.Arguments.Count == 2)
                                {
                                    SqlExpression left = mc.Arguments[0];
                                    SqlExpression right = mc.Arguments[1];
                                    SqlExpression match = (SqlExpression)this.sql.Binary(SqlNodeType.LT, left, right);
                                    SqlFactory sqlFactory = this.sql;
                                    SqlWhen[] whens = new SqlWhen[1];
                                    int index = 0;
                                    SqlWhen sqlWhen = new SqlWhen(match, left);
                                    whens[index] = sqlWhen;
                                    SqlExpression @else = right;
                                    Expression sourceExpression2 = sourceExpression1;
                                    return (SqlExpression)sqlFactory.SearchedCase(whens, @else, sourceExpression2);
                                }
                            }
                            else if (name1 == "Truncate" && mc.Arguments.Count == 1)
                            {
                                SqlExpression sqlExpression1 = mc.Arguments[0];
                                SqlFactory sqlFactory = this.sql;
                                Type returnType = mc.Method.ReturnType;
                                string name2 = "ROUND";
                                SqlExpression[] sqlExpressionArray = new SqlExpression[3];
                                int index1 = 0;
                                SqlExpression sqlExpression2 = sqlExpression1;
                                sqlExpressionArray[index1] = sqlExpression2;
                                int index2 = 1;
                                SqlExpression sqlExpression3 = this.sql.ValueFromObject((object)0, false, sourceExpression1);
                                sqlExpressionArray[index2] = sqlExpression3;
                                int index3 = 2;
                                SqlExpression sqlExpression4 = this.sql.ValueFromObject((object)1, false, sourceExpression1);
                                sqlExpressionArray[index3] = sqlExpression4;
                                Expression source = sourceExpression1;
                                return (SqlExpression)sqlFactory.FunctionCall(returnType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                            }
                        }
                        else if ((int)stringHash != 1119638765)
                        {
                            if ((int)stringHash != 1578662460)
                            {
                                if ((int)stringHash == 1587810493 && name1 == "Floor" && mc.Arguments.Count == 1)
                                    return this.CreateFunctionCallStatic1(mc.Arguments[0].ClrType, "FLOOR", mc.Arguments, sourceExpression1);
                            }
                            else if (name1 == "Cos" && mc.Arguments.Count == 1)
                                return this.CreateFunctionCallStatic1(typeof(double), "COS", mc.Arguments, sourceExpression1);
                        }
                        else if (name1 == "Sin" && mc.Arguments.Count == 1)
                            return this.CreateFunctionCallStatic1(typeof(double), "SIN", mc.Arguments, sourceExpression1);
                    }
                    else if (stringHash <= 2258966239U)
                    {
                        if ((int)stringHash != 1955107388)
                        {
                            if ((int)stringHash != 2128371142)
                            {
                                if ((int)stringHash == -2036001057 && name1 == "Atan" && mc.Arguments.Count == 1)
                                    return this.CreateFunctionCallStatic1(typeof(double), "ATAN", mc.Arguments, sourceExpression1);
                            }
                            else if (name1 == "Asin" && mc.Arguments.Count == 1)
                                return this.CreateFunctionCallStatic1(typeof(double), "ASIN", mc.Arguments, sourceExpression1);
                        }
                        else if (name1 == "Cosh" && mc.Arguments.Count == 1)
                        {
                            SqlExpression expression = mc.Arguments[0];
                            SqlFactory sqlFactory1 = this.sql;
                            Type clrType1 = typeof(double);
                            string name2 = "EXP";
                            SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
                            int index1 = 0;
                            SqlExpression sqlExpression1 = expression;
                            sqlExpressionArray1[index1] = sqlExpression1;
                            Expression source1 = sourceExpression1;
                            SqlExpression sqlExpression2 = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                            SqlExpression sqlExpression3 = (SqlExpression)this.sql.Unary(SqlNodeType.Negate, expression, sourceExpression1);
                            SqlFactory sqlFactory2 = this.sql;
                            Type clrType2 = typeof(double);
                            string name3 = "EXP";
                            SqlExpression[] sqlExpressionArray2 = new SqlExpression[1];
                            int index2 = 0;
                            SqlExpression sqlExpression4 = sqlExpression3;
                            sqlExpressionArray2[index2] = sqlExpression4;
                            Expression source2 = sourceExpression1;
                            SqlExpression sqlExpression5 = (SqlExpression)sqlFactory2.FunctionCall(clrType2, name3, (IEnumerable<SqlExpression>)sqlExpressionArray2, source2);
                            SqlFactory sqlFactory3 = this.sql;
                            SqlFactory sqlFactory4 = this.sql;
                            SqlExpression[] sqlExpressionArray3 = new SqlExpression[2];
                            int index3 = 0;
                            SqlExpression sqlExpression6 = sqlExpression2;
                            sqlExpressionArray3[index3] = sqlExpression6;
                            int index4 = 1;
                            SqlExpression sqlExpression7 = sqlExpression5;
                            sqlExpressionArray3[index4] = sqlExpression7;
                            SqlExpression expr = sqlFactory4.Add(sqlExpressionArray3);
                            long second = 2L;
                            return sqlFactory3.Divide(expr, second);
                        }
                    }
                    else if ((int)stringHash != -1949365520)
                    {
                        if ((int)stringHash != -1932355109)
                        {
                            if ((int)stringHash == -1889320764 && name1 == "Sign" && mc.Arguments.Count == 1)
                            {
                                SqlFactory sqlFactory = this.sql;
                                Type clrType = typeof(int);
                                string name2 = "SIGN";
                                SqlExpression[] sqlExpressionArray = new SqlExpression[1];
                                int index = 0;
                                SqlExpression sqlExpression = mc.Arguments[0];
                                sqlExpressionArray[index] = sqlExpression;
                                Expression source = sourceExpression1;
                                return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                            }
                        }
                        else if (name1 == "Abs" && mc.Arguments.Count == 1)
                        {
                            SqlFactory sqlFactory = this.sql;
                            Type clrType = mc.Arguments[0].ClrType;
                            string name2 = "ABS";
                            SqlExpression[] sqlExpressionArray = new SqlExpression[1];
                            int index = 0;
                            SqlExpression sqlExpression = mc.Arguments[0];
                            sqlExpressionArray[index] = sqlExpression;
                            Expression source = sourceExpression1;
                            return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                        }
                    }
                    else if (name1 == "Tanh" && mc.Arguments.Count == 1)
                    {
                        SqlExpression expression = mc.Arguments[0];
                        SqlFactory sqlFactory1 = this.sql;
                        Type clrType1 = typeof(double);
                        string name2 = "EXP";
                        SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
                        int index1 = 0;
                        SqlExpression sqlExpression1 = expression;
                        sqlExpressionArray1[index1] = sqlExpression1;
                        Expression source1 = sourceExpression1;
                        SqlExpression first1 = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                        SqlExpression sqlExpression2 = (SqlExpression)this.sql.Unary(SqlNodeType.Negate, expression, sourceExpression1);
                        SqlFactory sqlFactory2 = this.sql;
                        Type clrType2 = typeof(double);
                        string name3 = "EXP";
                        SqlExpression[] sqlExpressionArray2 = new SqlExpression[1];
                        int index2 = 0;
                        SqlExpression sqlExpression3 = sqlExpression2;
                        sqlExpressionArray2[index2] = sqlExpression3;
                        Expression source2 = sourceExpression1;
                        SqlExpression second1 = (SqlExpression)sqlFactory2.FunctionCall(clrType2, name3, (IEnumerable<SqlExpression>)sqlExpressionArray2, source2);
                        SqlFactory sqlFactory3 = this.sql;
                        SqlExpression first2 = this.sql.Subtract(first1, second1);
                        SqlFactory sqlFactory4 = this.sql;
                        SqlExpression[] sqlExpressionArray3 = new SqlExpression[2];
                        int index3 = 0;
                        SqlExpression sqlExpression4 = first1;
                        sqlExpressionArray3[index3] = sqlExpression4;
                        int index4 = 1;
                        SqlExpression sqlExpression5 = second1;
                        sqlExpressionArray3[index4] = sqlExpression5;
                        SqlExpression second2 = sqlFactory4.Add(sqlExpressionArray3);
                        return sqlFactory3.Divide(first2, second2);
                    }
                }
                else if (stringHash <= 3174591349U)
                {
                    if (stringHash <= 2715646961U)
                    {
                        if ((int)stringHash != -1820783265)
                        {
                            if ((int)stringHash != -1739485872)
                            {
                                if ((int)stringHash == -1579320335 && name1 == "Log")
                                {
                                    if (mc.Arguments.Count == 1)
                                        return this.CreateFunctionCallStatic1(typeof(double), "LOG", mc.Arguments, sourceExpression1);
                                    if (mc.Arguments.Count == 2)
                                    {
                                        SqlFactory sqlFactory1 = this.sql;
                                        Type clrType1 = typeof(double);
                                        string name2 = "LOG";
                                        SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
                                        int index1 = 0;
                                        SqlExpression sqlExpression1 = mc.Arguments[0];
                                        sqlExpressionArray1[index1] = sqlExpression1;
                                        Expression source1 = sourceExpression1;
                                        SqlExpression first = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                                        SqlFactory sqlFactory2 = this.sql;
                                        Type clrType2 = typeof(double);
                                        string name3 = "LOG";
                                        SqlExpression[] sqlExpressionArray2 = new SqlExpression[1];
                                        int index2 = 0;
                                        SqlExpression sqlExpression2 = mc.Arguments[1];
                                        sqlExpressionArray2[index2] = sqlExpression2;
                                        Expression source2 = sourceExpression1;
                                        SqlExpression second = (SqlExpression)sqlFactory2.FunctionCall(clrType2, name3, (IEnumerable<SqlExpression>)sqlExpressionArray2, source2);
                                        return this.sql.Divide(first, second);
                                    }
                                }
                            }
                            else if (name1 == "Log10" && mc.Arguments.Count == 1)
                                return this.CreateFunctionCallStatic1(typeof(double), "LOG10", mc.Arguments, sourceExpression1);
                        }
                        else if (name1 == "Sinh" && mc.Arguments.Count == 1)
                        {
                            SqlExpression expression = mc.Arguments[0];
                            SqlFactory sqlFactory1 = this.sql;
                            Type clrType1 = typeof(double);
                            string name2 = "EXP";
                            SqlExpression[] sqlExpressionArray1 = new SqlExpression[1];
                            int index1 = 0;
                            SqlExpression sqlExpression1 = expression;
                            sqlExpressionArray1[index1] = sqlExpression1;
                            Expression source1 = sourceExpression1;
                            SqlExpression first = (SqlExpression)sqlFactory1.FunctionCall(clrType1, name2, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                            SqlExpression sqlExpression2 = (SqlExpression)this.sql.Unary(SqlNodeType.Negate, expression, sourceExpression1);
                            SqlFactory sqlFactory2 = this.sql;
                            Type clrType2 = typeof(double);
                            string name3 = "EXP";
                            SqlExpression[] sqlExpressionArray2 = new SqlExpression[1];
                            int index2 = 0;
                            SqlExpression sqlExpression3 = sqlExpression2;
                            sqlExpressionArray2[index2] = sqlExpression3;
                            Expression source2 = sourceExpression1;
                            SqlExpression second = (SqlExpression)sqlFactory2.FunctionCall(clrType2, name3, (IEnumerable<SqlExpression>)sqlExpressionArray2, source2);
                            return this.sql.Divide(this.sql.Subtract(first, second), 2L);
                        }
                    }
                    else if ((int)stringHash != -1525441595)
                    {
                        if ((int)stringHash != -1137824961)
                        {
                            if ((int)stringHash == -1120375947 && name1 == "Pow" && mc.Arguments.Count == 2)
                                return this.CreateFunctionCallStatic2(mc.ClrType, "POWER", mc.Arguments, sourceExpression1);
                        }
                        else if (name1 == "Acos" && mc.Arguments.Count == 1)
                            return this.CreateFunctionCallStatic1(typeof(double), "ACOS", mc.Arguments, sourceExpression1);
                    }
                    else if (name1 == "BigMul" && mc.Arguments.Count == 2)
                    {
                        SqlFactory sqlFactory = this.sql;
                        SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                        int index1 = 0;
                        SqlExpression sqlExpression1 = this.sql.ConvertToBigint(mc.Arguments[0]);
                        sqlExpressionArray[index1] = sqlExpression1;
                        int index2 = 1;
                        SqlExpression sqlExpression2 = this.sql.ConvertToBigint(mc.Arguments[1]);
                        sqlExpressionArray[index2] = sqlExpression2;
                        return sqlFactory.Multiply(sqlExpressionArray);
                    }
                }
                else if (stringHash <= 3935307552U)
                {
                    if ((int)stringHash != -676624440)
                    {
                        if ((int)stringHash != -488433897)
                        {
                            if ((int)stringHash == -359659744 && name1 == "Ceiling" && mc.Arguments.Count == 1)
                                return this.CreateFunctionCallStatic1(mc.Arguments[0].ClrType, "CEILING", mc.Arguments, sourceExpression1);
                        }
                        else if (name1 == "Atan2" && mc.Arguments.Count == 2)
                            return this.CreateFunctionCallStatic2(typeof(double), "ATN2", mc.Arguments, sourceExpression1);
                    }
                    else if (name1 == "Exp" && mc.Arguments.Count == 1)
                        return this.CreateFunctionCallStatic1(typeof(double), "EXP", mc.Arguments, sourceExpression1);
                }
                else if ((int)stringHash != -188263461)
                {
                    if ((int)stringHash != -31815697)
                    {
                        if ((int)stringHash == -8167624 && name1 == "Tan" && mc.Arguments.Count == 1)
                            return this.CreateFunctionCallStatic1(typeof(double), "TAN", mc.Arguments, sourceExpression1);
                    }
                    else if (name1 == "Sqrt" && mc.Arguments.Count == 1)
                        return this.CreateFunctionCallStatic1(typeof(double), "SQRT", mc.Arguments, sourceExpression1);
                }
                else if (name1 == "Round")
                {
                    int count = mc.Arguments.Count;
                    if (mc.Arguments[count - 1].ClrType != typeof(MidpointRounding))
                        throw Error.MathRoundNotSupported();
                    SqlExpression sqlExpression1 = mc.Arguments[0];
                    SqlExpression sqlExpression2 = count != 2 ? mc.Arguments[1] : this.sql.ValueFromObject((object)0, false, sourceExpression1);
                    SqlExpression expr = mc.Arguments[count - 1];
                    if (expr.NodeType != SqlNodeType.Value)
                        throw Error.NonConstantExpressionsNotSupportedForRounding();
                    if ((MidpointRounding)this.Eval(expr) == MidpointRounding.AwayFromZero)
                    {
                        SqlFactory sqlFactory = this.sql;
                        Type clrType = sqlExpression1.ClrType;
                        string name2 = "round";
                        SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                        int index1 = 0;
                        SqlExpression sqlExpression3 = sqlExpression1;
                        sqlExpressionArray[index1] = sqlExpression3;
                        int index2 = 1;
                        SqlExpression sqlExpression4 = sqlExpression2;
                        sqlExpressionArray[index2] = sqlExpression4;
                        Expression source = sourceExpression1;
                        return (SqlExpression)sqlFactory.FunctionCall(clrType, name2, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                    }
                    Type clrType1 = sqlExpression1.ClrType;
                    SqlFactory sqlFactory1 = this.sql;
                    Type clrType2 = clrType1;
                    string name3 = "round";
                    SqlExpression[] sqlExpressionArray1 = new SqlExpression[2];
                    int index3 = 0;
                    SqlExpression sqlExpression5 = sqlExpression1;
                    sqlExpressionArray1[index3] = sqlExpression5;
                    int index4 = 1;
                    SqlExpression sqlExpression6 = sqlExpression2;
                    sqlExpressionArray1[index4] = sqlExpression6;
                    Expression source1 = sourceExpression1;
                    SqlExpression right1 = (SqlExpression)sqlFactory1.FunctionCall(clrType2, name3, (IEnumerable<SqlExpression>)sqlExpressionArray1, source1);
                    SqlExpression left = this.sql.Multiply(sqlExpression1, 2L);
                    SqlFactory sqlFactory2 = this.sql;
                    Type clrType3 = clrType1;
                    string name4 = "round";
                    SqlExpression[] sqlExpressionArray2 = new SqlExpression[2];
                    int index5 = 0;
                    SqlExpression sqlExpression7 = left;
                    sqlExpressionArray2[index5] = sqlExpression7;
                    int index6 = 1;
                    SqlExpression sqlExpression8 = sqlExpression2;
                    sqlExpressionArray2[index6] = sqlExpression8;
                    Expression source2 = sourceExpression1;
                    SqlExpression right2 = (SqlExpression)sqlFactory2.FunctionCall(clrType3, name4, (IEnumerable<SqlExpression>)sqlExpressionArray2, source2);
                    SqlExpression match = this.sql.AndAccumulate((SqlExpression)this.sql.Binary(SqlNodeType.EQ, left, right2), (SqlExpression)this.sql.Binary(SqlNodeType.NE, sqlExpression1, right1));
                    SqlFactory sqlFactory3 = this.sql;
                    SqlFactory sqlFactory4 = this.sql;
                    Type clrType4 = clrType1;
                    string name5 = "round";
                    SqlExpression[] sqlExpressionArray3 = new SqlExpression[2];
                    int index7 = 0;
                    SqlExpression sqlExpression9 = this.sql.Divide(sqlExpression1, 2L);
                    sqlExpressionArray3[index7] = sqlExpression9;
                    int index8 = 1;
                    SqlExpression sqlExpression10 = sqlExpression2;
                    sqlExpressionArray3[index8] = sqlExpression10;
                    Expression source3 = sourceExpression1;
                    SqlFunctionCall sqlFunctionCall = sqlFactory4.FunctionCall(clrType4, name5, (IEnumerable<SqlExpression>)sqlExpressionArray3, source3);
                    long second = 2L;
                    SqlExpression sqlExpression11 = sqlFactory3.Multiply((SqlExpression)sqlFunctionCall, second);
                    SqlFactory sqlFactory5 = this.sql;
                    SqlWhen[] whens = new SqlWhen[1];
                    int index9 = 0;
                    SqlWhen sqlWhen = new SqlWhen(match, sqlExpression11);
                    whens[index9] = sqlWhen;
                    SqlExpression @else = right1;
                    Expression sourceExpression2 = sourceExpression1;
                    return (SqlExpression)sqlFactory5.SearchedCase(whens, @else, sourceExpression2);
                }
                throw PostBindDotNetConverter.Visitor.GetMethodSupportException(mc);
            }

            internal override SqlNode VisitMember(SqlMember m)
            {
                SqlExpression sqlExpression1 = this.VisitExpression(m.Expression);
                MemberInfo member = m.Member;
                Expression sourceExpression = m.SourceExpression;
                Type nonNullableType = TypeSystem.GetNonNullableType(sqlExpression1.ClrType);
                if (nonNullableType == typeof(string) && member.Name == "Length")
                    return (SqlNode)this.sql.LEN(sqlExpression1);
                if (nonNullableType == typeof(Binary) && member.Name == "Length")
                    return (SqlNode)this.sql.DATALENGTH(sqlExpression1);
                if (nonNullableType == typeof(DateTime) || nonNullableType == typeof(DateTimeOffset))
                {
                    string datePart = PostBindDotNetConverter.GetDatePart(member.Name);
                    if (datePart != null)
                        return (SqlNode)this.sql.DATEPART(datePart, sqlExpression1);
                    if (member.Name == "Date")
                    {
                        if (this.providerMode == SqlProvider.ProviderMode.Sql2008)
                        {
                            SqlExpression sqlExpression2 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, "DATE", sourceExpression);
                            SqlFactory sqlFactory = this.sql;
                            Type clrType = typeof(DateTime);
                            string name = "CONVERT";
                            SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                            int index1 = 0;
                            SqlExpression sqlExpression3 = sqlExpression2;
                            sqlExpressionArray[index1] = sqlExpression3;
                            int index2 = 1;
                            SqlExpression sqlExpression4 = sqlExpression1;
                            sqlExpressionArray[index2] = sqlExpression4;
                            Expression source = sourceExpression;
                            return (SqlNode)sqlFactory.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                        }
                        SqlExpression expression1 = this.sql.DATEPART("MILLISECOND", sqlExpression1);
                        SqlExpression expression2 = this.sql.DATEPART("SECOND", sqlExpression1);
                        SqlExpression expression3 = this.sql.DATEPART("MINUTE", sqlExpression1);
                        SqlExpression expression4 = this.sql.DATEPART("HOUR", sqlExpression1);
                        SqlExpression expr1 = sqlExpression1;
                        SqlExpression expr2 = this.sql.DATEADD("MILLISECOND", (SqlExpression)this.sql.Unary(SqlNodeType.Negate, expression1), expr1);
                        SqlExpression expr3 = this.sql.DATEADD("SECOND", (SqlExpression)this.sql.Unary(SqlNodeType.Negate, expression2), expr2);
                        SqlExpression expr4 = this.sql.DATEADD("MINUTE", (SqlExpression)this.sql.Unary(SqlNodeType.Negate, expression3), expr3);
                        return (SqlNode)this.sql.DATEADD("HOUR", (SqlExpression)this.sql.Unary(SqlNodeType.Negate, expression4), expr4);
                    }
                    if (member.Name == "DateTime")
                    {
                        SqlExpression sqlExpression2 = (SqlExpression)new SqlVariable(typeof(void), (ProviderType)null, "DATETIME", sourceExpression);
                        SqlFactory sqlFactory = this.sql;
                        Type clrType = typeof(DateTime);
                        string name = "CONVERT";
                        SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                        int index1 = 0;
                        SqlExpression sqlExpression3 = sqlExpression2;
                        sqlExpressionArray[index1] = sqlExpression3;
                        int index2 = 1;
                        SqlExpression sqlExpression4 = sqlExpression1;
                        sqlExpressionArray[index2] = sqlExpression4;
                        Expression source = sourceExpression;
                        return (SqlNode)sqlFactory.FunctionCall(clrType, name, (IEnumerable<SqlExpression>)sqlExpressionArray, source);
                    }
                    if (member.Name == "TimeOfDay")
                    {
                        SqlExpression expr1 = this.sql.DATEPART("HOUR", sqlExpression1);
                        SqlExpression expr2 = this.sql.DATEPART("MINUTE", sqlExpression1);
                        SqlExpression expr3 = this.sql.DATEPART("SECOND", sqlExpression1);
                        SqlExpression expr4 = this.sql.DATEPART("MILLISECOND", sqlExpression1);
                        SqlExpression sqlExpression2 = this.sql.Multiply(this.sql.ConvertToBigint(expr1), 36000000000L);
                        SqlExpression sqlExpression3 = this.sql.Multiply(this.sql.ConvertToBigint(expr2), 600000000L);
                        SqlExpression sqlExpression4 = this.sql.Multiply(this.sql.ConvertToBigint(expr3), 10000000L);
                        SqlExpression sqlExpression5 = this.sql.Multiply(this.sql.ConvertToBigint(expr4), 10000L);
                        SqlFactory sqlFactory1 = this.sql;
                        Type clrType = typeof(TimeSpan);
                        SqlFactory sqlFactory2 = this.sql;
                        SqlExpression[] sqlExpressionArray = new SqlExpression[4];
                        int index1 = 0;
                        SqlExpression sqlExpression6 = sqlExpression2;
                        sqlExpressionArray[index1] = sqlExpression6;
                        int index2 = 1;
                        SqlExpression sqlExpression7 = sqlExpression3;
                        sqlExpressionArray[index2] = sqlExpression7;
                        int index3 = 2;
                        SqlExpression sqlExpression8 = sqlExpression4;
                        sqlExpressionArray[index3] = sqlExpression8;
                        int index4 = 3;
                        SqlExpression sqlExpression9 = sqlExpression5;
                        sqlExpressionArray[index4] = sqlExpression9;
                        SqlExpression expr5 = sqlFactory2.Add(sqlExpressionArray);
                        return (SqlNode)sqlFactory1.ConvertTo(clrType, expr5);
                    }
                    if (member.Name == "DayOfWeek")
                    {
                        SqlExpression sqlExpression2 = this.sql.DATEPART("dw", sqlExpression1);
                        SqlFactory sqlFactory1 = this.sql;
                        Type clrType = typeof(DayOfWeek);
                        SqlFactory sqlFactory2 = this.sql;
                        SqlFactory sqlFactory3 = this.sql;
                        SqlExpression[] sqlExpressionArray = new SqlExpression[2];
                        int index1 = 0;
                        SqlExpression sqlExpression3 = sqlExpression2;
                        sqlExpressionArray[index1] = sqlExpression3;
                        int index2 = 1;
                        SqlExpression sqlExpression4 = this.sql.Add((SqlExpression)new SqlVariable(typeof(int), this.sql.Default(typeof(int)), "@@DATEFIRST", sourceExpression), 6);
                        sqlExpressionArray[index2] = sqlExpression4;
                        SqlExpression expr1 = sqlFactory3.Add(sqlExpressionArray);
                        long second = 7L;
                        SqlExpression expr2 = sqlFactory2.Mod(expr1, second);
                        return (SqlNode)sqlFactory1.ConvertTo(clrType, expr2);
                    }
                }
                else if (nonNullableType == typeof(TimeSpan))
                {
                    string name = member.Name;
                    // ISSUE: reference to a compiler-generated method
                    uint stringHash = 0;
                    if (stringHash <= 2115171466U)
                    {
                        if (stringHash <= 920229518U)
                        {
                            if ((int)stringHash != 740138773)
                            {
                                if ((int)stringHash == 920229518 && name == "Minutes")
                                {
                                    if (SqlFactory.IsSqlTimeType(sqlExpression1))
                                        return (SqlNode)this.sql.DATEPART("MINUTE", sqlExpression1);
                                    return (SqlNode)this.sql.ConvertToInt(this.sql.Mod(this.sql.ConvertToBigint(this.sql.Divide(sqlExpression1, 600000000L)), 60L));
                                }
                            }
                            else if (name == "Ticks")
                            {
                                if (!SqlFactory.IsSqlTimeType(sqlExpression1))
                                    return (SqlNode)this.sql.ConvertToBigint(sqlExpression1);
                                SqlFactory sqlFactory1 = this.sql;
                                SqlFactory sqlFactory2 = this.sql;
                                SqlFactory sqlFactory3 = this.sql;
                                SqlExpression[] sqlExpressionArray = new SqlExpression[4];
                                int index1 = 0;
                                SqlExpression sqlExpression2 = this.sql.Multiply(this.sql.ConvertToBigint(this.sql.DATEPART("HOUR", sqlExpression1)), 3600000000000L);
                                sqlExpressionArray[index1] = sqlExpression2;
                                int index2 = 1;
                                SqlExpression sqlExpression3 = this.sql.Multiply(this.sql.ConvertToBigint(this.sql.DATEPART("MINUTE", sqlExpression1)), 60000000000L);
                                sqlExpressionArray[index2] = sqlExpression3;
                                int index3 = 2;
                                SqlExpression sqlExpression4 = this.sql.Multiply(this.sql.ConvertToBigint(this.sql.DATEPART("SECOND", sqlExpression1)), 1000000000L);
                                sqlExpressionArray[index3] = sqlExpression4;
                                int index4 = 3;
                                SqlExpression sqlExpression5 = this.sql.DATEPART("NANOSECOND", sqlExpression1);
                                sqlExpressionArray[index4] = sqlExpression5;
                                SqlExpression expr1 = sqlFactory3.Add(sqlExpressionArray);
                                SqlExpression expr2 = sqlFactory2.ConvertToBigint(expr1);
                                long second = 100L;
                                return (SqlNode)sqlFactory1.Divide(expr2, second);
                            }
                        }
                        else if ((int)stringHash != 1215240326)
                        {
                            if ((int)stringHash != 1567816874)
                            {
                                if ((int)stringHash == 2115171466 && name == "Seconds")
                                {
                                    if (SqlFactory.IsSqlTimeType(sqlExpression1))
                                        return (SqlNode)this.sql.DATEPART("SECOND", sqlExpression1);
                                    return (SqlNode)this.sql.ConvertToInt(this.sql.Mod(this.sql.ConvertToBigint(this.sql.Divide(sqlExpression1, 10000000L)), 60L));
                                }
                            }
                            else if (name == "Days")
                            {
                                if (SqlFactory.IsSqlTimeType(sqlExpression1))
                                    return (SqlNode)this.sql.ValueFromObject((object)0, false, sqlExpression1.SourceExpression);
                                return (SqlNode)this.sql.ConvertToInt(this.sql.Divide(sqlExpression1, 864000000000L));
                            }
                        }
                        else if (name == "TotalMinutes")
                        {
                            if (!SqlFactory.IsSqlTimeType(sqlExpression1))
                                return (SqlNode)this.sql.Divide(this.sql.ConvertToDouble(sqlExpression1), 600000000L);
                            SqlFactory sqlFactory = this.sql;
                            SqlExpression[] sqlExpressionArray = new SqlExpression[4];
                            int index1 = 0;
                            SqlExpression sqlExpression2 = this.sql.Multiply(this.sql.DATEPART("HOUR", sqlExpression1), 60L);
                            sqlExpressionArray[index1] = sqlExpression2;
                            int index2 = 1;
                            SqlExpression sqlExpression3 = this.sql.DATEPART("MINUTE", sqlExpression1);
                            sqlExpressionArray[index2] = sqlExpression3;
                            int index3 = 2;
                            SqlExpression sqlExpression4 = this.sql.Divide(this.sql.ConvertToDouble(this.sql.DATEPART("SECOND", sqlExpression1)), 60L);
                            sqlExpressionArray[index3] = sqlExpression4;
                            int index4 = 3;
                            SqlExpression sqlExpression5 = this.sql.Divide(this.sql.ConvertToDouble(this.sql.ConvertToBigint(this.sql.DATEPART("NANOSECOND", sqlExpression1))), 60000000000L);
                            sqlExpressionArray[index4] = sqlExpression5;
                            return (SqlNode)sqlFactory.Add(sqlExpressionArray);
                        }
                    }
                    else if (stringHash <= 2745576355U)
                    {
                        if ((int)stringHash != -2090719662)
                        {
                            if ((int)stringHash != -1800856484)
                            {
                                if ((int)stringHash == -1549390941 && name == "TotalMilliseconds")
                                {
                                    if (!SqlFactory.IsSqlTimeType(sqlExpression1))
                                        return (SqlNode)this.sql.Divide(this.sql.ConvertToDouble(sqlExpression1), 10000L);
                                    SqlFactory sqlFactory = this.sql;
                                    SqlExpression[] sqlExpressionArray = new SqlExpression[4];
                                    int index1 = 0;
                                    SqlExpression sqlExpression2 = this.sql.Multiply(this.sql.DATEPART("HOUR", sqlExpression1), 3600000L);
                                    sqlExpressionArray[index1] = sqlExpression2;
                                    int index2 = 1;
                                    SqlExpression sqlExpression3 = this.sql.Multiply(this.sql.DATEPART("MINUTE", sqlExpression1), 60000L);
                                    sqlExpressionArray[index2] = sqlExpression3;
                                    int index3 = 2;
                                    SqlExpression sqlExpression4 = this.sql.Multiply(this.sql.DATEPART("SECOND", sqlExpression1), 1000L);
                                    sqlExpressionArray[index3] = sqlExpression4;
                                    int index4 = 3;
                                    SqlExpression sqlExpression5 = this.sql.Divide(this.sql.ConvertToDouble(this.sql.ConvertToBigint(this.sql.DATEPART("NANOSECOND", sqlExpression1))), 1000000L);
                                    sqlExpressionArray[index4] = sqlExpression5;
                                    return (SqlNode)sqlFactory.Add(sqlExpressionArray);
                                }
                            }
                            else if (name == "TotalHours")
                            {
                                if (!SqlFactory.IsSqlTimeType(sqlExpression1))
                                    return (SqlNode)this.sql.Divide(this.sql.ConvertToDouble(sqlExpression1), 36000000000L);
                                SqlFactory sqlFactory = this.sql;
                                SqlExpression[] sqlExpressionArray = new SqlExpression[4];
                                int index1 = 0;
                                SqlExpression sqlExpression2 = this.sql.DATEPART("HOUR", sqlExpression1);
                                sqlExpressionArray[index1] = sqlExpression2;
                                int index2 = 1;
                                SqlExpression sqlExpression3 = this.sql.Divide(this.sql.ConvertToDouble(this.sql.DATEPART("MINUTE", sqlExpression1)), 60L);
                                sqlExpressionArray[index2] = sqlExpression3;
                                int index3 = 2;
                                SqlExpression sqlExpression4 = this.sql.Divide(this.sql.ConvertToDouble(this.sql.DATEPART("SECOND", sqlExpression1)), 3600L);
                                sqlExpressionArray[index3] = sqlExpression4;
                                int index4 = 3;
                                SqlExpression sqlExpression5 = this.sql.Divide(this.sql.ConvertToDouble(this.sql.ConvertToBigint(this.sql.DATEPART("NANOSECOND", sqlExpression1))), 3600000000000L);
                                sqlExpressionArray[index4] = sqlExpression5;
                                return (SqlNode)sqlFactory.Add(sqlExpressionArray);
                            }
                        }
                        else if (name == "TotalDays")
                        {
                            if (!SqlFactory.IsSqlTimeType(sqlExpression1))
                                return (SqlNode)this.sql.Divide(this.sql.ConvertToDouble(sqlExpression1), 864000000000L);
                            SqlFactory sqlFactory1 = this.sql;
                            SqlFactory sqlFactory2 = this.sql;
                            SqlExpression[] sqlExpressionArray = new SqlExpression[4];
                            int index1 = 0;
                            SqlExpression sqlExpression2 = this.sql.DATEPART("HOUR", sqlExpression1);
                            sqlExpressionArray[index1] = sqlExpression2;
                            int index2 = 1;
                            SqlExpression sqlExpression3 = this.sql.Divide(this.sql.ConvertToDouble(this.sql.DATEPART("MINUTE", sqlExpression1)), 60L);
                            sqlExpressionArray[index2] = sqlExpression3;
                            int index3 = 2;
                            SqlExpression sqlExpression4 = this.sql.Divide(this.sql.ConvertToDouble(this.sql.DATEPART("SECOND", sqlExpression1)), 3600L);
                            sqlExpressionArray[index3] = sqlExpression4;
                            int index4 = 3;
                            SqlExpression sqlExpression5 = this.sql.Divide(this.sql.ConvertToDouble(this.sql.ConvertToBigint(this.sql.DATEPART("NANOSECOND", sqlExpression1))), 3600000000000L);
                            sqlExpressionArray[index4] = sqlExpression5;
                            SqlExpression expr = sqlFactory2.Add(sqlExpressionArray);
                            long second = 24L;
                            return (SqlNode)sqlFactory1.Divide(expr, second);
                        }
                    }
                    else if ((int)stringHash != -1463950085)
                    {
                        if ((int)stringHash != -1422597676)
                        {
                            if ((int)stringHash == -61377358 && name == "TotalSeconds")
                            {
                                if (!SqlFactory.IsSqlTimeType(sqlExpression1))
                                    return (SqlNode)this.sql.Divide(this.sql.ConvertToDouble(sqlExpression1), 10000000L);
                                SqlFactory sqlFactory = this.sql;
                                SqlExpression[] sqlExpressionArray = new SqlExpression[4];
                                int index1 = 0;
                                SqlExpression sqlExpression2 = this.sql.Multiply(this.sql.DATEPART("HOUR", sqlExpression1), 3600L);
                                sqlExpressionArray[index1] = sqlExpression2;
                                int index2 = 1;
                                SqlExpression sqlExpression3 = this.sql.Multiply(this.sql.DATEPART("MINUTE", sqlExpression1), 60L);
                                sqlExpressionArray[index2] = sqlExpression3;
                                int index3 = 2;
                                SqlExpression sqlExpression4 = this.sql.DATEPART("SECOND", sqlExpression1);
                                sqlExpressionArray[index3] = sqlExpression4;
                                int index4 = 3;
                                SqlExpression sqlExpression5 = this.sql.Divide(this.sql.ConvertToDouble(this.sql.ConvertToBigint(this.sql.DATEPART("NANOSECOND", sqlExpression1))), 1000000000L);
                                sqlExpressionArray[index4] = sqlExpression5;
                                return (SqlNode)sqlFactory.Add(sqlExpressionArray);
                            }
                        }
                        else if (name == "Hours")
                        {
                            if (SqlFactory.IsSqlTimeType(sqlExpression1))
                                return (SqlNode)this.sql.DATEPART("HOUR", sqlExpression1);
                            return (SqlNode)this.sql.ConvertToInt(this.sql.Mod(this.sql.ConvertToBigint(this.sql.Divide(sqlExpression1, 36000000000L)), 24L));
                        }
                    }
                    else if (name == "Milliseconds")
                    {
                        if (SqlFactory.IsSqlTimeType(sqlExpression1))
                            return (SqlNode)this.sql.DATEPART("MILLISECOND", sqlExpression1);
                        return (SqlNode)this.sql.ConvertToInt(this.sql.Mod(this.sql.ConvertToBigint(this.sql.Divide(sqlExpression1, 10000L)), 1000L));
                    }
                    throw Error.MemberCannotBeTranslated((object)member.DeclaringType, (object)member.Name);
                }
                throw Error.MemberCannotBeTranslated((object)member.DeclaringType, (object)member.Name);
            }

            private SqlExpression CreateDateTimeFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source)
            {
                return this.CreateDateTimeFromDateAndTicks(sqlDate, sqlTicks, source, false);
            }

            private SqlExpression CreateDateTimeFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source, bool asNullable)
            {
                SqlExpression expr = this.sql.DATEADD("day", this.sql.Divide(sqlTicks, 864000000000L), sqlDate, source, asNullable);
                return this.sql.DATEADD("ms", this.sql.Mod(this.sql.Divide(sqlTicks, 10000L), 86400000L), expr, source, asNullable);
            }

            private SqlExpression CreateDateTimeFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source)
            {
                return this.CreateDateTimeFromDateAndMs(sqlDate, ms, source, false);
            }

            private SqlExpression CreateDateTimeFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source, bool asNullable)
            {
                SqlExpression expr1 = this.sql.ConvertToBigint(ms);
                SqlExpression expr2 = this.sql.DATEADD("day", this.sql.Divide(expr1, 86400000L), sqlDate, source, asNullable);
                return this.sql.DATEADD("ms", this.sql.Mod(expr1, 86400000L), expr2, source, asNullable);
            }

            private SqlExpression CreateDateTimeOffsetFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source)
            {
                return this.CreateDateTimeOffsetFromDateAndTicks(sqlDate, sqlTicks, source, false);
            }

            private SqlExpression CreateDateTimeOffsetFromDateAndTicks(SqlExpression sqlDate, SqlExpression sqlTicks, Expression source, bool asNullable)
            {
                SqlExpression expr = this.sql.DATETIMEOFFSETADD("day", this.sql.Divide(sqlTicks, 864000000000L), sqlDate, source, asNullable);
                return this.sql.DATETIMEOFFSETADD("ms", this.sql.Mod(this.sql.Divide(sqlTicks, 10000L), 86400000L), expr, source, asNullable);
            }

            private SqlExpression CreateDateTimeOffsetFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source)
            {
                return this.CreateDateTimeOffsetFromDateAndMs(sqlDate, ms, source, false);
            }

            private SqlExpression CreateDateTimeOffsetFromDateAndMs(SqlExpression sqlDate, SqlExpression ms, Expression source, bool asNullable)
            {
                SqlExpression expr1 = this.sql.ConvertToBigint(ms);
                SqlExpression expr2 = this.sql.DATETIMEOFFSETADD("day", this.sql.Divide(expr1, 86400000L), sqlDate, source, asNullable);
                return this.sql.DATETIMEOFFSETADD("ms", this.sql.Mod(expr1, 86400000L), expr2, source, asNullable);
            }

            private SqlExpression TranslateVbConversionMethod(SqlMethodCall mc)
            {
                Expression sourceExpression = mc.SourceExpression;
                if (mc.Arguments.Count == 1)
                {
                    SqlExpression sqlExpression = mc.Arguments[0];
                    Type clrType = (Type)null;
                    string name = mc.Method.Name;
                    // ISSUE: reference to a compiler-generated method
                    uint stringHash = uint.Parse(PrivateImplementationDetails.ComputeStringHash(name));
                    if (stringHash <= 1938964185U)
                    {
                        if (stringHash <= 1628624528U)
                        {
                            if (stringHash <= 989231474U)
                            {
                                if ((int)stringHash != 356562880)
                                {
                                    if ((int)stringHash == 989231474 && name == "ToLong")
                                        clrType = typeof(long);
                                }
                                else if (name == "ToBoolean")
                                    clrType = typeof(bool);
                            }
                            else if ((int)stringHash != 1104427872)
                            {
                                if ((int)stringHash == 1628624528 && name == "ToChar")
                                    clrType = typeof(char);
                            }
                            else if (name == "ToDate")
                                clrType = typeof(DateTime);
                        }
                        else if (stringHash <= 1883303205U)
                        {
                            if ((int)stringHash != 1862051603)
                            {
                                if ((int)stringHash == 1883303205 && name == "ToString")
                                    clrType = typeof(string);
                            }
                            else if (name == "ToUShort")
                                clrType = typeof(ushort);
                        }
                        else if ((int)stringHash != 1903862909)
                        {
                            if ((int)stringHash == 1938964185 && name == "ToDouble")
                                clrType = typeof(double);
                        }
                        else if (name == "ToULong")
                            clrType = typeof(ulong);
                    }
                    else if (stringHash <= 3049974302U)
                    {
                        if (stringHash <= 2978282195U)
                        {
                            if ((int)stringHash != -1829286225)
                            {
                                if ((int)stringHash == -1316685101 && name == "ToUInteger")
                                    clrType = typeof(uint);
                            }
                            else if (name == "ToSByte")
                                clrType = typeof(sbyte);
                        }
                        else if ((int)stringHash != -1277203181)
                        {
                            if ((int)stringHash == -1244992994 && name == "ToShort")
                                clrType = typeof(short);
                        }
                        else if (name == "ToDecimal")
                            clrType = typeof(Decimal);
                    }
                    else if (stringHash <= 3300893262U)
                    {
                        if ((int)stringHash != -1107071570)
                        {
                            if ((int)stringHash == -994074034 && name == "ToInteger")
                                clrType = typeof(int);
                        }
                        else if (name == "ToByte")
                            clrType = typeof(byte);
                    }
                    else if ((int)stringHash != -522456384)
                    {
                        if ((int)stringHash == -237980003 && name == "ToCharArrayRankOne")
                            clrType = typeof(char[]);
                    }
                    else if (name == "ToSingle")
                        clrType = typeof(float);
                    if (clrType != (Type)null)
                    {
                        if ((clrType == typeof(int) || clrType == typeof(float)) && sqlExpression.ClrType == typeof(bool))
                        {
                            List<SqlExpression> matches = new List<SqlExpression>();
                            List<SqlExpression> values = new List<SqlExpression>();
                            matches.Add(this.sql.ValueFromObject((object)true, false, sourceExpression));
                            values.Add(this.sql.ValueFromObject((object)-1, false, sourceExpression));
                            matches.Add(this.sql.ValueFromObject((object)false, false, sourceExpression));
                            values.Add(this.sql.ValueFromObject((object)0, false, sourceExpression));
                            return this.sql.Case(clrType, sqlExpression, matches, values, sourceExpression);
                        }
                        if (mc.ClrType != mc.Arguments[0].ClrType)
                            return this.sql.ConvertTo(clrType, sqlExpression);
                        return mc.Arguments[0];
                    }
                }
                throw PostBindDotNetConverter.Visitor.GetMethodSupportException(mc);
            }

            private SqlExpression TranslateVbCompareString(SqlMethodCall mc)
            {
                if (mc.Arguments.Count >= 2)
                    return this.CreateComparison(mc.Arguments[0], mc.Arguments[1], mc.SourceExpression);
                throw PostBindDotNetConverter.Visitor.GetMethodSupportException(mc);
            }

            private SqlExpression TranslateVbLikeString(SqlMethodCall mc)
            {
                bool flag = true;
                Expression sourceExpression1 = mc.SourceExpression;
                SqlExpression sqlExpression = mc.Arguments[1];
                SqlExpression pattern1;
                if (sqlExpression.NodeType == SqlNodeType.Value)
                {
                    string pattern2 = (string)((SqlValue)sqlExpression).Value;
                    int num = 126;
                    string str1 = SqlHelpers.TranslateVBLikePattern(pattern2, (char)num);
                    pattern1 = this.sql.ValueFromObject((object)str1, typeof(string), true, sourceExpression1);
                    string str2 = str1;
                    flag = pattern2 != str2;
                }
                else
                {
                    if (sqlExpression.NodeType != SqlNodeType.ClientParameter)
                        throw Error.NonConstantExpressionsNotSupportedFor((object)"LIKE");
                    SqlClientParameter sqlClientParameter = (SqlClientParameter)sqlExpression;
                    Type clrType = sqlClientParameter.ClrType;
                    ProviderType sqlType = sqlClientParameter.SqlType;
                    Type type = typeof(SqlHelpers);
                    string methodName = "TranslateVBLikePattern";
                    Type[] typeArguments = Type.EmptyTypes;
                    Expression[] expressionArray = new Expression[2];
                    int index1 = 0;
                    Expression body = sqlClientParameter.Accessor.Body;
                    expressionArray[index1] = body;
                    int index2 = 1;
                    ConstantExpression constantExpression = Expression.Constant((object)'~');
                    expressionArray[index2] = (Expression)constantExpression;
                    MethodCallExpression methodCallExpression = Expression.Call(type, methodName, typeArguments, expressionArray);
                    ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                    int index3 = 0;
                    ParameterExpression parameterExpression = sqlClientParameter.Accessor.Parameters[0];
                    parameterExpressionArray[index3] = parameterExpression;
                    LambdaExpression accessor = Expression.Lambda((Expression)methodCallExpression, parameterExpressionArray);
                    Expression sourceExpression2 = sqlClientParameter.SourceExpression;
                    pattern1 = (SqlExpression)new SqlClientParameter(clrType, sqlType, accessor, sourceExpression2);
                }
                SqlExpression escape = flag ? this.sql.ValueFromObject((object)"~", false, mc.SourceExpression) : (SqlExpression)null;
                return (SqlExpression)this.sql.Like(mc.Arguments[0], pattern1, escape, sourceExpression1);
            }
        }
    }
}
