// Decompiled with JetBrains decompiler
// Type: System.Data.Linq.SqlClient.Translator
// Assembly: System.Data.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: 5EB3665A-A733-4A1E-9362-A98E33F2FF9B
// Assembly location: C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Data.Linq.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionTest
{
    internal class Translator
    {
        private IDataServices services;
        private SqlFactory sql;
        private TypeSystemProvider typeProvider;

        internal Translator(IDataServices services, SqlFactory sqlFactory, TypeSystemProvider typeProvider)
        {
            this.services = services;
            this.sql = sqlFactory;
            this.typeProvider = typeProvider;
        }

        internal SqlSelect BuildDefaultQuery(MetaType rowType, bool allowDeferred, SqlLink link, Expression source)
        {
            if (rowType.HasInheritance && rowType.InheritanceRoot != rowType)
                throw Error.ArgumentWrongValue((object)"rowType");
            SqlTable sqlTable = this.sql.Table(rowType.Table, rowType, source);
            SqlAlias alias = new SqlAlias((SqlNode)sqlTable);
            return new SqlSelect(this.BuildProjection((SqlExpression)new SqlAliasRef(alias), sqlTable.RowType, allowDeferred, link, source), (SqlSource)alias, source);
        }

        internal SqlExpression BuildProjection(SqlExpression item, MetaType rowType, bool allowDeferred, SqlLink link, Expression source)
        {
            if (!rowType.HasInheritance)
            {
                SqlExpression sqlExpression = item;
                MetaType rowType1 = rowType;
                ReadOnlyCollection<MetaDataMember> readOnlyCollection = rowType1.Table != null ? rowType.PersistentDataMembers : rowType.DataMembers;
                int num = allowDeferred ? 1 : 0;
                SqlLink link1 = link;
                Expression source1 = source;
                return (SqlExpression)this.BuildProjectionInternal(sqlExpression, rowType1, (IEnumerable<MetaDataMember>)readOnlyCollection, num != 0, link1, source1);
            }
            List<MetaType> list1 = new List<MetaType>((IEnumerable<MetaType>)rowType.InheritanceTypes);
            List<SqlTypeCaseWhen> list2 = new List<SqlTypeCaseWhen>();
            SqlTypeCaseWhen sqlTypeCaseWhen = (SqlTypeCaseWhen)null;
            MetaType inheritanceRoot = rowType.InheritanceRoot;
            MetaDataMember discriminator = inheritanceRoot.Discriminator;
            Type type = discriminator.Type;
            SqlMember sqlMember = this.sql.Member(item, discriminator.Member);
            foreach (MetaType metaType in list1)
            {
                if (metaType.HasInheritanceCode)
                {
                    SqlExpression sqlExpression = item;
                    MetaType rowType1 = metaType;
                    ReadOnlyCollection<MetaDataMember> persistentDataMembers = rowType1.PersistentDataMembers;
                    int num = allowDeferred ? 1 : 0;
                    SqlLink link1 = link;
                    Expression source1 = source;
                    SqlNew sqlNew = this.BuildProjectionInternal(sqlExpression, rowType1, (IEnumerable<MetaDataMember>)persistentDataMembers, num != 0, link1, source1);
                    if (metaType.IsInheritanceDefault)
                        sqlTypeCaseWhen = new SqlTypeCaseWhen((SqlExpression)null, (SqlExpression)sqlNew);
                    object obj = InheritanceRules.InheritanceCodeForClientCompare(metaType.InheritanceCode, sqlMember.SqlType);
                    SqlExpression match = this.sql.Value(type, this.sql.Default(discriminator), obj, true, source);
                    list2.Add(new SqlTypeCaseWhen(match, (SqlExpression)sqlNew));
                }
            }
            if (sqlTypeCaseWhen == null)
                throw Error.EmptyCaseNotSupported();
            list2.Add(sqlTypeCaseWhen);
            return this.sql.TypeCase(inheritanceRoot.Type, inheritanceRoot, (SqlExpression)sqlMember, (IEnumerable<SqlTypeCaseWhen>)list2.ToArray(), source);
        }

        private bool IsPreloaded(MemberInfo member)
        {
            if (this.services.Context.LoadOptions == null)
                return false;
            return this.services.Context.LoadOptions.IsPreloaded(member);
        }

        private SqlNew BuildProjectionInternal(SqlExpression item, MetaType rowType, IEnumerable<MetaDataMember> members, bool allowDeferred, SqlLink link, Expression source)
        {
            List<SqlMemberAssign> list = new List<SqlMemberAssign>();
            foreach (MetaDataMember member in members)
            {
                if (allowDeferred && (member.IsAssociation || member.IsDeferred))
                {
                    if (link != null && member != link.Member && (member.IsAssociation && member.MappedName == link.Member.MappedName) && (!member.Association.IsMany && !this.IsPreloaded(link.Member.Member)))
                    {
                        SqlLink sqlLink = this.BuildLink(item, member, source);
                        sqlLink.Expansion = link.Expression;
                        list.Add(new SqlMemberAssign(member.Member, (SqlExpression)sqlLink));
                    }
                    else
                        list.Add(new SqlMemberAssign(member.Member, (SqlExpression)this.BuildLink(item, member, source)));
                }
                else if (!member.IsAssociation)
                    list.Add(new SqlMemberAssign(member.Member, (SqlExpression)this.sql.Member(item, member)));
            }
            ConstructorInfo constructor = rowType.Type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, Type.EmptyTypes, (ParameterModifier[])null);
            if (constructor == (ConstructorInfo)null)
                throw Error.MappedTypeMustHaveDefaultConstructor((object)rowType.Type);
            return this.sql.New(rowType, constructor, (IEnumerable<SqlExpression>)null, (IEnumerable<MemberInfo>)null, (IEnumerable<SqlMemberAssign>)list, source);
        }

        private SqlLink BuildLink(SqlExpression item, MetaDataMember member, Expression source)
        {
            if (member.IsAssociation)
            {
                SqlExpression[] sqlExpressionArray = new SqlExpression[member.Association.ThisKey.Count];
                int index = 0;
                for (int length = sqlExpressionArray.Length; index < length; ++index)
                {
                    MetaDataMember metaDataMember = member.Association.ThisKey[index];
                    sqlExpressionArray[index] = (SqlExpression)this.sql.Member(item, metaDataMember.Member);
                }
                return new SqlLink(new object(), member.Association.OtherType, member.Type, this.typeProvider.From(member.Type), item, member, (IEnumerable<SqlExpression>)sqlExpressionArray, (SqlExpression)null, source);
            }
            MetaType declaringType = member.DeclaringType;
            List<SqlExpression> list = new List<SqlExpression>();
            foreach (MetaDataMember metaDataMember in declaringType.IdentityMembers)
                list.Add((SqlExpression)this.sql.Member(item, metaDataMember.Member));
            SqlExpression expansion = (SqlExpression)this.sql.Member(item, member.Member);
            return new SqlLink(new object(), declaringType, member.Type, this.typeProvider.From(member.Type), item, member, (IEnumerable<SqlExpression>)list, expansion, source);
        }

        internal SqlNode TranslateLink(SqlLink link, bool asExpression)
        {
            SqlLink link1 = link;
            List<SqlExpression> keyExpressions = link1.KeyExpressions;
            int num = asExpression ? 1 : 0;
            return this.TranslateLink(link1, keyExpressions, num != 0);
        }

        internal static Expression TranslateAssociation(DataContext context, MetaAssociation association, Expression otherSource, Expression[] keyValues, Expression thisInstance)
        {
            if (association == null)
                throw Error.ArgumentNull("association");
            if (keyValues == null)
                throw Error.ArgumentNull("keyValues");
            if (context.LoadOptions != null)
            {
                LambdaExpression associationSubquery = context.LoadOptions.GetAssociationSubquery(association.ThisMember.Member);
                if (associationSubquery != null)
                    return new Translator.RelationComposer(associationSubquery.Parameters[0], association, otherSource, thisInstance).Visit(associationSubquery.Body);
            }
            return Translator.WhereClauseFromSourceAndKeys(otherSource, System.Linq.Enumerable.ToArray<MetaDataMember>((IEnumerable<MetaDataMember>)association.OtherKey), keyValues);
        }

        internal static Expression WhereClauseFromSourceAndKeys(Expression source, MetaDataMember[] keyMembers, Expression[] keyValues)
        {
            Type elementType = TypeSystem.GetElementType(source.Type);
            ParameterExpression parameterExpression1 = Expression.Parameter(elementType, "p");
            Expression left1 = (Expression)null;
            for (int index = 0; index < keyMembers.Length; ++index)
            {
                MetaDataMember metaDataMember = keyMembers[index];
                Expression expression1 = elementType == metaDataMember.Member.DeclaringType ? (Expression)parameterExpression1 : (Expression)Expression.Convert((Expression)parameterExpression1, metaDataMember.Member.DeclaringType);
                Expression left2 = metaDataMember.Member is FieldInfo ? (Expression)Expression.Field(expression1, (FieldInfo)metaDataMember.Member) : (Expression)Expression.Property(expression1, (PropertyInfo)metaDataMember.Member);
                Expression expression2 = keyValues[index];
                if (expression2.Type != left2.Type)
                    expression2 = (Expression)Expression.Convert(expression2, left2.Type);
                Expression right = (Expression)Expression.Equal(left2, expression2);
                left1 = left1 != null ? (Expression)Expression.And(left1, right) : right;
            }
            Type type1 = typeof(System.Linq.Enumerable);
            string methodName = "Where";
            Type[] typeArguments = new Type[1];
            int index1 = 0;
            Type type2 = parameterExpression1.Type;
            typeArguments[index1] = type2;
            Expression[] expressionArray = new Expression[2];
            int index2 = 0;
            Expression expression = source;
            expressionArray[index2] = expression;
            int index3 = 1;
            Expression body = left1;
            ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
            int index4 = 0;
            ParameterExpression parameterExpression2 = parameterExpression1;
            parameterExpressionArray[index4] = parameterExpression2;
            LambdaExpression lambdaExpression = Expression.Lambda(body, parameterExpressionArray);
            expressionArray[index3] = (Expression)lambdaExpression;
            return (Expression)Expression.Call(type1, methodName, typeArguments, expressionArray);
        }

        internal SqlNode TranslateLink(SqlLink link, List<SqlExpression> keyExpressions, bool asExpression)
        {
            MetaDataMember member = link.Member;
            if (!member.IsAssociation)
                return (SqlNode)link.Expansion;
            MetaType otherType = member.Association.OtherType;
            ITable table1 = this.services.Context.GetTable(otherType.InheritanceRoot.Type);
            SqlLink link1 = link;
            ITable table2 = table1;
            Type type1 = typeof(IQueryable<>);
            Type[] typeArray = new Type[1];
            int index1 = 0;
            Type type2 = otherType.Type;
            typeArray[index1] = type2;
            Type type3 = type1.MakeGenericType(typeArray);
            Expression otherSource = (Expression)new LinkedTableExpression(link1, table2, type3);
            Expression[] keyValues = new Expression[keyExpressions.Count];
            for (int index2 = 0; index2 < keyExpressions.Count; ++index2)
            {
                Type memberType = TypeSystem.GetMemberType(member.Association.OtherKey[index2].Member);
                keyValues[index2] = (Expression)InternalExpression.Known((SqlNode)keyExpressions[index2], memberType);
            }
            Expression thisInstance = link.Expression != null ? (Expression)InternalExpression.Known(link.Expression) : (Expression)Expression.Constant((object)null, link.Member.Member.DeclaringType);
            SqlSelect select = (SqlSelect)new QueryConverter(this.services, this.typeProvider, this, this.sql).ConvertInner(Translator.TranslateAssociation(this.services.Context, member.Association, otherSource, keyValues, thisInstance), link.SourceExpression);
            SqlNode sqlNode = (SqlNode)select;
            if (asExpression)
                sqlNode = !member.Association.IsMany ? (SqlNode)new SqlSubSelect(SqlNodeType.Element, link.ClrType, link.SqlType, select) : (SqlNode)new SqlSubSelect(SqlNodeType.Multiset, link.ClrType, link.SqlType, select);
            return sqlNode;
        }

        internal SqlExpression TranslateEquals(SqlBinary expr)
        {
            SqlExpression expr1 = expr.Left;
            SqlExpression expr2 = expr.Right;
            if (expr2.NodeType == SqlNodeType.Element)
            {
                SqlAlias alias = new SqlAlias((SqlNode)((SqlSubSelect)expr2).Select);
                SqlAliasRef sqlAliasRef = new SqlAliasRef(alias);
                return (SqlExpression)this.sql.SubSelect(SqlNodeType.Exists, new SqlSelect((SqlExpression)sqlAliasRef, (SqlSource)alias, expr.SourceExpression)
                {
                    Where = (SqlExpression)this.sql.Binary(expr.NodeType, (SqlExpression)this.sql.DoNotVisitExpression(expr1), (SqlExpression)sqlAliasRef)
                });
            }
            if (expr1.NodeType == SqlNodeType.Element)
            {
                SqlAlias alias = new SqlAlias((SqlNode)((SqlSubSelect)expr1).Select);
                SqlAliasRef sqlAliasRef = new SqlAliasRef(alias);
                return (SqlExpression)this.sql.SubSelect(SqlNodeType.Exists, new SqlSelect((SqlExpression)sqlAliasRef, (SqlSource)alias, expr.SourceExpression)
                {
                    Where = (SqlExpression)this.sql.Binary(expr.NodeType, (SqlExpression)this.sql.DoNotVisitExpression(expr2), (SqlExpression)sqlAliasRef)
                });
            }
            MetaType sourceMetaType1 = TypeSource.GetSourceMetaType((SqlNode)expr1, this.services.Model);
            MetaType sourceMetaType2 = TypeSource.GetSourceMetaType((SqlNode)expr2, this.services.Model);
            if (expr1.NodeType == SqlNodeType.TypeCase)
                expr1 = Translator.BestIdentityNode((SqlTypeCase)expr1);
            if (expr2.NodeType == SqlNodeType.TypeCase)
                expr2 = Translator.BestIdentityNode((SqlTypeCase)expr2);
            if (sourceMetaType1.IsEntity && sourceMetaType2.IsEntity && sourceMetaType1.Table != sourceMetaType2.Table)
                throw Error.CannotCompareItemsAssociatedWithDifferentTable();
            if (!sourceMetaType1.IsEntity && !sourceMetaType2.IsEntity && (expr1.NodeType != SqlNodeType.New || expr1.SqlType.CanBeColumn) && (expr2.NodeType != SqlNodeType.New || expr2.SqlType.CanBeColumn))
            {
                if (expr.NodeType == SqlNodeType.EQ2V || expr.NodeType == SqlNodeType.NE2V)
                    return this.TranslateEqualsOp(expr.NodeType, (SqlExpression)this.sql.DoNotVisitExpression(expr.Left), (SqlExpression)this.sql.DoNotVisitExpression(expr.Right), false);
                return (SqlExpression)expr;
            }
            if (sourceMetaType1 != sourceMetaType2 && sourceMetaType1.InheritanceRoot != sourceMetaType2.InheritanceRoot)
                return (SqlExpression)this.sql.Binary(SqlNodeType.EQ, this.sql.ValueFromObject((object)0, expr.SourceExpression), this.sql.ValueFromObject((object)1, expr.SourceExpression));
            SqlLink sqlLink1 = expr1 as SqlLink;
            List<SqlExpression> list1 = sqlLink1 == null || !sqlLink1.Member.IsAssociation || !sqlLink1.Member.Association.IsForeignKey ? this.GetIdentityExpressions(sourceMetaType1, (SqlExpression)this.sql.DoNotVisitExpression(expr1)) : sqlLink1.KeyExpressions;
            SqlLink sqlLink2 = expr2 as SqlLink;
            List<SqlExpression> list2 = sqlLink2 == null || !sqlLink2.Member.IsAssociation || !sqlLink2.Member.Association.IsForeignKey ? this.GetIdentityExpressions(sourceMetaType2, (SqlExpression)this.sql.DoNotVisitExpression(expr2)) : sqlLink2.KeyExpressions;
            SqlExpression left = (SqlExpression)null;
            SqlNodeType op = expr.NodeType == SqlNodeType.EQ2V || expr.NodeType == SqlNodeType.NE2V ? SqlNodeType.EQ2V : SqlNodeType.EQ;
            int index = 0;
            for (int count = list1.Count; index < count; ++index)
            {
                SqlExpression right = this.TranslateEqualsOp(op, list1[index], list2[index], !sourceMetaType1.IsEntity);
                left = left != null ? (SqlExpression)this.sql.Binary(SqlNodeType.And, left, right) : right;
            }
            if (expr.NodeType == SqlNodeType.NE || expr.NodeType == SqlNodeType.NE2V)
            {
                SqlFactory sqlFactory = this.sql;
                int num = 62;
                SqlExpression expression = left;
                Expression sourceExpression = expression.SourceExpression;
                left = (SqlExpression)sqlFactory.Unary((SqlNodeType)num, expression, sourceExpression);
            }
            return left;
        }

        private SqlExpression TranslateEqualsOp(SqlNodeType op, SqlExpression left, SqlExpression right, bool allowExpand)
        {
            if (op <= SqlNodeType.EQ2V)
            {
                if (op != SqlNodeType.EQ)
                {
                    if (op == SqlNodeType.EQ2V)
                    {
                        bool? nullable = SqlExpressionNullability.CanBeNull(left);
                        bool flag1 = false;
                        if ((nullable.GetValueOrDefault() == flag1 ? (!nullable.HasValue ? 1 : 0) : 1) != 0)
                        {
                            nullable = SqlExpressionNullability.CanBeNull(right);
                            bool flag2 = false;
                            if ((nullable.GetValueOrDefault() == flag2 ? (!nullable.HasValue ? 1 : 0) : 1) != 0)
                            {
                                SqlNodeType nodeType = allowExpand ? SqlNodeType.EQ2V : SqlNodeType.EQ;
                                return (SqlExpression)this.sql.Binary(SqlNodeType.Or, (SqlExpression)this.sql.Binary(SqlNodeType.And, (SqlExpression)this.sql.Unary(SqlNodeType.IsNull, (SqlExpression)SqlDuplicator.Copy((SqlNode)left)), (SqlExpression)this.sql.Unary(SqlNodeType.IsNull, (SqlExpression)SqlDuplicator.Copy((SqlNode)right))), (SqlExpression)this.sql.Binary(SqlNodeType.And, (SqlExpression)this.sql.Binary(SqlNodeType.And, (SqlExpression)this.sql.Unary(SqlNodeType.IsNotNull, (SqlExpression)SqlDuplicator.Copy((SqlNode)left)), (SqlExpression)this.sql.Unary(SqlNodeType.IsNotNull, (SqlExpression)SqlDuplicator.Copy((SqlNode)right))), (SqlExpression)this.sql.Binary(nodeType, left, right)));
                            }
                        }
                        return (SqlExpression)this.sql.Binary(allowExpand ? SqlNodeType.EQ2V : SqlNodeType.EQ, left, right);
                    }
                    goto label_14;
                }
            }
            else if (op != SqlNodeType.NE)
            {
                if (op == SqlNodeType.NE2V)
                {
                    bool? nullable = SqlExpressionNullability.CanBeNull(left);
                    bool flag1 = false;
                    if ((nullable.GetValueOrDefault() == flag1 ? (!nullable.HasValue ? 1 : 0) : 1) != 0)
                    {
                        nullable = SqlExpressionNullability.CanBeNull(right);
                        bool flag2 = false;
                        if ((nullable.GetValueOrDefault() == flag2 ? (!nullable.HasValue ? 1 : 0) : 1) != 0)
                        {
                            SqlNodeType nodeType = allowExpand ? SqlNodeType.EQ2V : SqlNodeType.EQ;
                            return (SqlExpression)this.sql.Unary(SqlNodeType.Not, (SqlExpression)this.sql.Binary(SqlNodeType.Or, (SqlExpression)this.sql.Binary(SqlNodeType.And, (SqlExpression)this.sql.Unary(SqlNodeType.IsNull, (SqlExpression)SqlDuplicator.Copy((SqlNode)left)), (SqlExpression)this.sql.Unary(SqlNodeType.IsNull, (SqlExpression)SqlDuplicator.Copy((SqlNode)right))), (SqlExpression)this.sql.Binary(SqlNodeType.And, (SqlExpression)this.sql.Binary(SqlNodeType.And, (SqlExpression)this.sql.Unary(SqlNodeType.IsNotNull, (SqlExpression)SqlDuplicator.Copy((SqlNode)left)), (SqlExpression)this.sql.Unary(SqlNodeType.IsNotNull, (SqlExpression)SqlDuplicator.Copy((SqlNode)right))), (SqlExpression)this.sql.Binary(nodeType, left, right))));
                        }
                    }
                    return (SqlExpression)this.sql.Binary(allowExpand ? SqlNodeType.NE2V : SqlNodeType.NE, left, right);
                }
                goto label_14;
            }
            return (SqlExpression)this.sql.Binary(op, left, right);
            label_14:
            throw Error.UnexpectedNode((object)op);
        }

        internal SqlExpression TranslateLinkEquals(SqlBinary bo)
        {
            SqlLink sqlLink1 = bo.Left as SqlLink;
            SqlLink sqlLink2 = bo.Right as SqlLink;
            if (sqlLink1 != null && sqlLink1.Member.IsAssociation && sqlLink1.Member.Association.IsForeignKey || sqlLink2 != null && sqlLink2.Member.IsAssociation && sqlLink2.Member.Association.IsForeignKey)
                return this.TranslateEquals(bo);
            return (SqlExpression)bo;
        }

        internal SqlExpression TranslateLinkIsNull(SqlUnary expr)
        {
            SqlLink sqlLink = expr.Operand as SqlLink;
            if (sqlLink == null || !sqlLink.Member.IsAssociation || !sqlLink.Member.Association.IsForeignKey)
                return (SqlExpression)expr;
            List<SqlExpression> keyExpressions = sqlLink.KeyExpressions;
            SqlExpression left = (SqlExpression)null;
            SqlNodeType nodeType = expr.NodeType == SqlNodeType.IsNull ? SqlNodeType.Or : SqlNodeType.And;
            int index = 0;
            for (int count = keyExpressions.Count; index < count; ++index)
            {
                SqlExpression right = (SqlExpression)this.sql.Unary(expr.NodeType, (SqlExpression)this.sql.DoNotVisitExpression(keyExpressions[index]), expr.SourceExpression);
                left = left != null ? (SqlExpression)this.sql.Binary(nodeType, left, right) : right;
            }
            return left;
        }

        private static SqlExpression BestIdentityNode(SqlTypeCase tc)
        {
            foreach (SqlTypeCaseWhen sqlTypeCaseWhen in tc.Whens)
            {
                if (sqlTypeCaseWhen.TypeBinding.NodeType == SqlNodeType.New)
                    return sqlTypeCaseWhen.TypeBinding;
            }
            return tc.Whens[0].TypeBinding;
        }

        private static bool IsPublic(MemberInfo mi)
        {
            FieldInfo fieldInfo = mi as FieldInfo;
            if (fieldInfo != (FieldInfo)null)
                return fieldInfo.IsPublic;
            PropertyInfo propertyInfo = mi as PropertyInfo;
            if (propertyInfo != (PropertyInfo)null && propertyInfo.CanRead)
            {
                MethodInfo getMethod = propertyInfo.GetGetMethod();
                if (getMethod != (MethodInfo)null)
                    return getMethod.IsPublic;
            }
            return false;
        }

        private IEnumerable<MetaDataMember> GetIdentityMembers(MetaType type)
        {
            if (type.IsEntity)
                return (IEnumerable<MetaDataMember>)type.IdentityMembers;
            return System.Linq.Enumerable.Where<MetaDataMember>((IEnumerable<MetaDataMember>)type.DataMembers, (Func<MetaDataMember, bool>)(m => Translator.IsPublic(m.Member)));
        }

        private List<SqlExpression> GetIdentityExpressions(MetaType type, SqlExpression expr)
        {
            List<MetaDataMember> list1 = System.Linq.Enumerable.ToList<MetaDataMember>(this.GetIdentityMembers(type));
            List<SqlExpression> list2 = new List<SqlExpression>(list1.Count);
            foreach (MetaDataMember member in list1)
                list2.Add((SqlExpression)this.sql.Member((SqlExpression)SqlDuplicator.Copy((SqlNode)expr), member));
            return list2;
        }

        private class RelationComposer : ExpressionVisitor
        {
            private ParameterExpression parameter;
            private MetaAssociation association;
            private Expression otherSouce;
            private Expression parameterReplacement;

            internal RelationComposer(ParameterExpression parameter, MetaAssociation association, Expression otherSouce, Expression parameterReplacement)
            {
                if (parameter == null)
                    throw Error.ArgumentNull("parameter");
                if (association == null)
                    throw Error.ArgumentNull("association");
                if (otherSouce == null)
                    throw Error.ArgumentNull("otherSouce");
                if (parameterReplacement == null)
                    throw Error.ArgumentNull("parameterReplacement");
                this.parameter = parameter;
                this.association = association;
                this.otherSouce = otherSouce;
                this.parameterReplacement = parameterReplacement;
            }

            internal override Expression VisitParameter(ParameterExpression p)
            {
                if (p == this.parameter)
                    return this.parameterReplacement;
                return base.VisitParameter(p);
            }

            private static Expression[] GetKeyValues(Expression expr, ReadOnlyCollection<MetaDataMember> keys)
            {
                List<Expression> list = new List<Expression>();
                foreach (MetaDataMember metaDataMember in keys)
                    list.Add((Expression)Expression.PropertyOrField(expr, metaDataMember.Name));
                return list.ToArray();
            }

            internal override Expression VisitMemberAccess(MemberExpression m)
            {
                if (MetaPosition.AreSameMember(m.Member, this.association.ThisMember.Member))
                    return Translator.WhereClauseFromSourceAndKeys(this.otherSouce, System.Linq.Enumerable.ToArray<MetaDataMember>((IEnumerable<MetaDataMember>)this.association.OtherKey), Translator.RelationComposer.GetKeyValues(this.Visit(m.Expression), this.association.ThisKey));
                Expression expression1 = this.Visit(m.Expression);
                if (expression1 == m.Expression)
                    return (Expression)m;
                if (!(expression1.Type != m.Expression.Type) || !(m.Member.Name == "Count") || !TypeSystem.IsSequenceType(expression1.Type))
                    return (Expression)Expression.MakeMemberAccess(expression1, m.Member);
                Type type = typeof(System.Linq.Enumerable);
                string methodName = "Count";
                Type[] typeArguments = new Type[1];
                int index1 = 0;
                Type elementType = TypeSystem.GetElementType(expression1.Type);
                typeArguments[index1] = elementType;
                Expression[] expressionArray = new Expression[1];
                int index2 = 0;
                Expression expression2 = expression1;
                expressionArray[index2] = expression2;
                return (Expression)Expression.Call(type, methodName, typeArguments, expressionArray);
            }
        }
    }
}
