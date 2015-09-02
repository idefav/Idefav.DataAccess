using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class SqlBinder
    {
        private bool optimizeLinkExpansions = true;
        private bool simplifyCaseStatements = true;
        private SqlColumnizer columnizer;
        private SqlBinder.Visitor visitor;
        private SqlFactory sql;
        private Func<SqlNode, SqlNode> prebinder;

        internal Func<SqlNode, SqlNode> PreBinder
        {
            get
            {
                return this.prebinder;
            }
            set
            {
                this.prebinder = value;
            }
        }

        internal bool OptimizeLinkExpansions
        {
            get
            {
                return this.optimizeLinkExpansions;
            }
            set
            {
                this.optimizeLinkExpansions = value;
            }
        }

        internal bool SimplifyCaseStatements
        {
            get
            {
                return this.simplifyCaseStatements;
            }
            set
            {
                this.simplifyCaseStatements = value;
            }
        }

        internal SqlBinder(Translator translator, SqlFactory sqlFactory, MetaModel model, DataLoadOptions shape, SqlColumnizer columnizer, bool canUseOuterApply)
        {
            this.sql = sqlFactory;
            this.columnizer = columnizer;
            this.visitor = new SqlBinder.Visitor(this, translator, this.columnizer, this.sql, model, shape, canUseOuterApply);
        }

        private SqlNode Prebind(SqlNode node)
        {
            if (this.prebinder != null)
                node = this.prebinder(node);
            return node;
        }

        internal SqlNode Bind(SqlNode node)
        {
            node = this.Prebind(node);
            node = this.visitor.Visit(node);
            return node;
        }

        private class LinkOptimizationScope
        {
            private Dictionary<object, SqlExpression> map;
            private SqlBinder.LinkOptimizationScope previous;

            internal LinkOptimizationScope(SqlBinder.LinkOptimizationScope previous)
            {
                this.previous = previous;
            }

            internal void Add(object linkId, SqlExpression expr)
            {
                if (this.map == null)
                    this.map = new Dictionary<object, SqlExpression>();
                this.map.Add(linkId, expr);
            }

            internal bool TryGetValue(object linkId, out SqlExpression expr)
            {
                expr = (SqlExpression)null;
                if (this.map != null && this.map.TryGetValue(linkId, out expr))
                    return true;
                if (this.previous != null)
                    return this.previous.TryGetValue(linkId, out expr);
                return false;
            }
        }

        private class Visitor : SqlVisitor
        {
            private SqlBinder binder;
            private Translator translator;
            private SqlFactory sql;
            private TypeSystemProvider typeProvider;
            private SqlExpander expander;
            private SqlColumnizer columnizer;
            private SqlAggregateChecker aggregateChecker;
            private SqlSelect currentSelect;
            private SqlAlias currentAlias;
            private Dictionary<SqlAlias, SqlAlias> outerAliasMap;
            private SqlBinder.LinkOptimizationScope linkMap;
            private MetaModel model;
            private HashSet<MetaType> alreadyIncluded;
            private DataLoadOptions shape;
            private bool disableInclude;
            private bool inGroupBy;
            private bool canUseOuterApply;

            internal Visitor(SqlBinder binder, Translator translator, SqlColumnizer columnizer, SqlFactory sqlFactory, MetaModel model, DataLoadOptions shape, bool canUseOuterApply)
            {
                this.binder = binder;
                this.translator = translator;
                this.columnizer = columnizer;
                this.sql = sqlFactory;
                this.typeProvider = sqlFactory.TypeProvider;
                this.expander = new SqlExpander(this.sql);
                this.aggregateChecker = new SqlAggregateChecker();
                this.linkMap = new SqlBinder.LinkOptimizationScope((SqlBinder.LinkOptimizationScope)null);
                this.outerAliasMap = new Dictionary<SqlAlias, SqlAlias>();
                this.model = model;
                this.shape = shape;
                this.canUseOuterApply = canUseOuterApply;
            }

            internal override SqlExpression VisitExpression(SqlExpression expr)
            {
                return this.ConvertToExpression(this.Visit((SqlNode)expr));
            }

            internal override SqlNode VisitIncludeScope(SqlIncludeScope scope)
            {
                this.alreadyIncluded = new HashSet<MetaType>();
                try
                {
                    return this.Visit(scope.Child);
                }
                finally
                {
                    this.alreadyIncluded = (HashSet<MetaType>)null;
                }
            }

            internal override SqlUserQuery VisitUserQuery(SqlUserQuery suq)
            {
                this.disableInclude = true;
                return base.VisitUserQuery(suq);
            }

            internal SqlExpression FetchExpression(SqlExpression expr)
            {
                return this.ConvertToExpression((SqlNode)this.ConvertToFetchedExpression((SqlNode)this.ConvertLinks(this.VisitExpression(expr))));
            }

            internal override SqlExpression VisitFunctionCall(SqlFunctionCall fc)
            {
                int index = 0;
                for (int count = fc.Arguments.Count; index < count; ++index)
                    fc.Arguments[index] = this.FetchExpression(fc.Arguments[index]);
                return (SqlExpression)fc;
            }

            internal override SqlExpression VisitLike(SqlLike like)
            {
                like.Expression = this.FetchExpression(like.Expression);
                like.Pattern = this.FetchExpression(like.Pattern);
                return base.VisitLike(like);
            }

            internal override SqlExpression VisitGrouping(SqlGrouping g)
            {
                g.Key = this.FetchExpression(g.Key);
                g.Group = this.FetchExpression(g.Group);
                return (SqlExpression)g;
            }

            internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
            {
                mc.Object = this.FetchExpression(mc.Object);
                int index = 0;
                for (int count = mc.Arguments.Count; index < count; ++index)
                    mc.Arguments[index] = this.FetchExpression(mc.Arguments[index]);
                return (SqlExpression)mc;
            }

            internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
            {
                switch (bo.NodeType)
                {
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                        if (this.IsConstNull(bo.Left) && !TypeSystem.IsNullableType(bo.ClrType))
                            return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNotNull, bo.Right, bo.SourceExpression));
                        if (this.IsConstNull(bo.Right) && !TypeSystem.IsNullableType(bo.ClrType))
                            return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNotNull, bo.Left, bo.SourceExpression));
                        break;
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        if (this.IsConstNull(bo.Left) && !TypeSystem.IsNullableType(bo.ClrType))
                            return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNull, bo.Right, bo.SourceExpression));
                        if (this.IsConstNull(bo.Right) && !TypeSystem.IsNullableType(bo.ClrType))
                            return this.VisitUnaryOperator(this.sql.Unary(SqlNodeType.IsNull, bo.Left, bo.SourceExpression));
                        break;
                }
                bo.Left = this.VisitExpression(bo.Left);
                bo.Right = this.VisitExpression(bo.Right);
                switch (bo.NodeType)
                {
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        SqlValue sqlValue1 = bo.Left as SqlValue;
                        SqlValue sqlValue2 = bo.Right as SqlValue;
                        bool flag1 = sqlValue1 != null && sqlValue1.Value is bool;
                        bool flag2 = sqlValue2 != null && sqlValue2.Value is bool;
                        if (flag1 | flag2)
                        {
                            bool flag3 = bo.NodeType != SqlNodeType.NE && bo.NodeType != SqlNodeType.NE2V;
                            SqlNodeType nt = (bo.NodeType == SqlNodeType.EQ2V ? 1 : (bo.NodeType == SqlNodeType.NE2V ? 1 : 0)) != 0 ? SqlNodeType.Not2V : SqlNodeType.Not;
                            if (flag1 && !flag2)
                            {
                                if ((bool)sqlValue1.Value ^ flag3)
                                    return this.VisitUnaryOperator(new SqlUnary(nt, bo.ClrType, bo.SqlType, (SqlExpression)this.sql.DoNotVisitExpression(bo.Right), bo.SourceExpression));
                                if (bo.Right.ClrType == typeof(bool))
                                    return bo.Right;
                                break;
                            }
                            if (!flag1 & flag2)
                            {
                                if ((bool)sqlValue2.Value ^ flag3)
                                    return this.VisitUnaryOperator(new SqlUnary(nt, bo.ClrType, bo.SqlType, (SqlExpression)this.sql.DoNotVisitExpression(bo.Left), bo.SourceExpression));
                                if (bo.Left.ClrType == typeof(bool))
                                    return bo.Left;
                                break;
                            }
                            if (flag1 & flag2)
                            {
                                bool flag4 = (bool)sqlValue1.Value;
                                bool flag5 = (bool)sqlValue2.Value;
                                if (flag3)
                                    return this.sql.ValueFromObject((object)(bool)(flag4 == flag5 ), false, bo.SourceExpression);
                                return this.sql.ValueFromObject((object)(bool)(flag4 != flag5), false, bo.SourceExpression);
                            }
                            break;
                        }
                        break;
                }
                switch (bo.NodeType)
                {
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        SqlExpression exp1 = this.translator.TranslateLinkEquals(bo);
                        if (exp1 != bo)
                            return this.VisitExpression(exp1);
                        break;
                    case SqlNodeType.Or:
                        SqlValue sqlValue3 = bo.Left as SqlValue;
                        SqlValue sqlValue4 = bo.Right as SqlValue;
                        if (sqlValue3 != null && sqlValue4 == null)
                        {
                            if (sqlValue3.Value != null && !(bool)sqlValue3.Value)
                                return bo.Right;
                            return this.sql.ValueFromObject((object)true, false, bo.SourceExpression);
                        }
                        if (sqlValue3 == null && sqlValue4 != null)
                        {
                            if (sqlValue4.Value != null && !(bool)sqlValue4.Value)
                                return bo.Left;
                            return this.sql.ValueFromObject((object)true, false, bo.SourceExpression);
                        }
                        if (sqlValue3 != null && sqlValue4 != null)
                            return this.sql.ValueFromObject(((bool)(sqlValue3.Value ?? false) ? 1 : ((bool)(sqlValue4.Value ?? false) ? 1 : 0))==1?true:false, false, bo.SourceExpression);
                        break;
                    case SqlNodeType.And:
                        SqlValue sqlValue5 = bo.Left as SqlValue;
                        SqlValue sqlValue6 = bo.Right as SqlValue;
                        if (sqlValue5 != null && sqlValue6 == null)
                        {
                            if (sqlValue5.Value != null && (bool)sqlValue5.Value)
                                return bo.Right;
                            return this.sql.ValueFromObject((object)false, false, bo.SourceExpression);
                        }
                        if (sqlValue5 == null && sqlValue6 != null)
                        {
                            if (sqlValue6.Value != null && (bool)sqlValue6.Value)
                                return bo.Left;
                            return this.sql.ValueFromObject((object)false, false, bo.SourceExpression);
                        }
                        if (sqlValue5 != null && sqlValue6 != null)
                            return this.sql.ValueFromObject((!(bool)(sqlValue5.Value ?? (object)false) ? 0 : ((bool)(sqlValue6.Value ?? (object)false) ? 1 : 0))==1, false, bo.SourceExpression);
                        break;
                }
                bo.Left = this.ConvertToFetchedExpression((SqlNode)bo.Left);
                bo.Right = this.ConvertToFetchedExpression((SqlNode)bo.Right);
                switch (bo.NodeType)
                {
                    case SqlNodeType.NE:
                    case SqlNodeType.NE2V:
                    case SqlNodeType.EQ:
                    case SqlNodeType.EQ2V:
                        SqlExpression exp2 = this.translator.TranslateEquals(bo);
                        if (exp2 != bo)
                            return this.VisitExpression(exp2);
                        if (typeof(Type).IsAssignableFrom(bo.Left.ClrType))
                        {
                            SqlExpression typeSource1 = TypeSource.GetTypeSource(bo.Left);
                            SqlExpression typeSource2 = TypeSource.GetTypeSource(bo.Right);
                            MetaType[] possibleTypes1 = this.GetPossibleTypes(typeSource1);
                            MetaType[] possibleTypes2 = this.GetPossibleTypes(typeSource2);
                            bool flag3 = false;
                            for (int index1 = 0; index1 < possibleTypes1.Length; ++index1)
                            {
                                for (int index2 = 0; index2 < possibleTypes2.Length; ++index2)
                                {
                                    if (possibleTypes1[index1] == possibleTypes2[index2])
                                    {
                                        flag3 = true;
                                        break;
                                    }
                                }
                            }
                            if (!flag3)
                                return this.VisitExpression(this.sql.ValueFromObject((bo.NodeType == SqlNodeType.NE ? 1 : 0)==1, false, bo.SourceExpression));
                            if (possibleTypes1.Length == 1 && possibleTypes2.Length == 1)
                                return this.VisitExpression(this.sql.ValueFromObject((bo.NodeType == SqlNodeType.EQ == (possibleTypes1[0] == possibleTypes2[0]) ? 1 : 0)==1, false, bo.SourceExpression));
                            SqlDiscriminatedType discriminatedType1 = bo.Left as SqlDiscriminatedType;
                            SqlDiscriminatedType discriminatedType2 = bo.Right as SqlDiscriminatedType;
                            if (discriminatedType1 != null && discriminatedType2 != null)
                                return this.VisitExpression((SqlExpression)this.sql.Binary(bo.NodeType, discriminatedType1.Discriminator, discriminatedType2.Discriminator));
                        }
                        if (TypeSystem.IsSequenceType(bo.Left.ClrType))
                            throw Error.ComparisonNotSupportedForType((object)bo.Left.ClrType);
                        if (TypeSystem.IsSequenceType(bo.Right.ClrType))
                            throw Error.ComparisonNotSupportedForType((object)bo.Right.ClrType);
                        break;
                }
                return (SqlExpression)bo;
            }

            private MetaType[] GetPossibleTypes(SqlExpression typeExpression)
            {
                if (!typeof(Type).IsAssignableFrom(typeExpression.ClrType))
                    return new MetaType[0];
                if (typeExpression.NodeType == SqlNodeType.DiscriminatedType)
                {
                    SqlDiscriminatedType discriminatedType = (SqlDiscriminatedType)typeExpression;
                    List<MetaType> list = new List<MetaType>();
                    foreach (MetaType metaType in discriminatedType.TargetType.InheritanceTypes)
                    {
                        if (!metaType.Type.IsAbstract)
                            list.Add(metaType);
                    }
                    return list.ToArray();
                }
                if (typeExpression.NodeType == SqlNodeType.Value)
                {
                    MetaType metaType1 = this.model.GetMetaType((Type)((SqlValue)typeExpression).Value);
                    MetaType[] metaTypeArray = new MetaType[1];
                    int index = 0;
                    MetaType metaType2 = metaType1;
                    metaTypeArray[index] = metaType2;
                    return metaTypeArray;
                }
                if (typeExpression.NodeType != SqlNodeType.SearchedCase)
                    throw Error.UnexpectedNode((object)typeExpression.NodeType);
                SqlSearchedCase sqlSearchedCase = (SqlSearchedCase)typeExpression;
                HashSet<MetaType> hashSet = new HashSet<MetaType>();
                foreach (SqlWhen sqlWhen in sqlSearchedCase.Whens)
                    hashSet.UnionWith((IEnumerable<MetaType>)this.GetPossibleTypes(sqlWhen.Value));
                return System.Linq.Enumerable.ToArray<MetaType>((IEnumerable<MetaType>)hashSet);
            }

            internal override SqlExpression VisitDiscriminatorOf(SqlDiscriminatorOf dof)
            {
                SqlExpression sqlExpression1 = this.FetchExpression(dof.Object);
                while (sqlExpression1.NodeType == SqlNodeType.OptionalValue || sqlExpression1.NodeType == SqlNodeType.OuterJoinedValue)
                    sqlExpression1 = sqlExpression1.NodeType != SqlNodeType.OptionalValue ? ((SqlUnary)sqlExpression1).Operand : ((SqlOptionalValue)sqlExpression1).Value;
                if (sqlExpression1.NodeType == SqlNodeType.TypeCase)
                {
                    SqlTypeCase sqlTypeCase = (SqlTypeCase)sqlExpression1;
                    List<SqlExpression> matches = new List<SqlExpression>();
                    List<SqlExpression> values = new List<SqlExpression>();
                    MetaType inheritanceDefault = sqlTypeCase.RowType.InheritanceDefault;
                    object inheritanceCode = inheritanceDefault.InheritanceCode;
                    foreach (SqlTypeCaseWhen sqlTypeCaseWhen in sqlTypeCase.Whens)
                    {
                        matches.Add(sqlTypeCaseWhen.Match);
                        if (sqlTypeCaseWhen.Match == null)
                        {
                            SqlExpression sqlExpression2 = this.sql.Value(inheritanceCode.GetType(), sqlTypeCase.Whens[0].Match.SqlType, inheritanceDefault.InheritanceCode, true, sqlTypeCase.SourceExpression);
                            values.Add(sqlExpression2);
                        }
                        else
                            values.Add(this.sql.Value(inheritanceCode.GetType(), sqlTypeCaseWhen.Match.SqlType, ((SqlValue)sqlTypeCaseWhen.Match).Value, true, sqlTypeCase.SourceExpression));
                    }
                    return this.sql.Case(sqlTypeCase.Discriminator.ClrType, sqlTypeCase.Discriminator, matches, values, sqlTypeCase.SourceExpression);
                }
                MetaType inheritanceRoot = this.model.GetMetaType(sqlExpression1.ClrType).InheritanceRoot;
                if (inheritanceRoot.HasInheritance)
                    return this.VisitExpression((SqlExpression)this.sql.Member(dof.Object, inheritanceRoot.Discriminator.Member));
                return this.sql.TypedLiteralNull(dof.ClrType, dof.SourceExpression);
            }

            internal override SqlExpression VisitSearchedCase(SqlSearchedCase c)
            {
                if ((c.ClrType == typeof(bool) || c.ClrType == typeof(bool?)) && (c.Whens.Count == 1 && c.Else != null))
                {
                    SqlValue sqlValue1 = c.Else as SqlValue;
                    SqlValue sqlValue2 = c.Whens[0].Value as SqlValue;
                    if (sqlValue1 != null && sqlValue1.Value != null && !(bool)sqlValue1.Value)
                        return this.VisitExpression((SqlExpression)this.sql.Binary(SqlNodeType.And, c.Whens[0].Match, c.Whens[0].Value));
                    if (sqlValue2 != null && sqlValue2.Value != null && (bool)sqlValue2.Value)
                        return this.VisitExpression((SqlExpression)this.sql.Binary(SqlNodeType.Or, c.Whens[0].Match, c.Else));
                }
                return base.VisitSearchedCase(c);
            }

            private bool IsConstNull(SqlExpression sqlExpr)
            {
                SqlValue sqlValue = sqlExpr as SqlValue;
                if (sqlValue == null || sqlValue.Value != null)
                    return false;
                return !sqlValue.IsClientSpecified;
            }

            private SqlExpression ApplyTreat(SqlExpression target, Type type)
            {
                switch (target.NodeType)
                {
                    case SqlNodeType.OuterJoinedValue:
                        return this.ApplyTreat(((SqlUnary)target).Operand, type);
                    case SqlNodeType.TypeCase:
                        SqlTypeCase sqlTypeCase = (SqlTypeCase)target;
                        int num = 0;
                        foreach (SqlTypeCaseWhen sqlTypeCaseWhen in sqlTypeCase.Whens)
                        {
                            sqlTypeCaseWhen.TypeBinding = this.ApplyTreat(sqlTypeCaseWhen.TypeBinding, type);
                            if (this.IsConstNull(sqlTypeCaseWhen.TypeBinding))
                                ++num;
                        }
                        if (num == sqlTypeCase.Whens.Count)
                        {
                            sqlTypeCase.Whens[0].TypeBinding.SetClrType(type);
                            return sqlTypeCase.Whens[0].TypeBinding;
                        }
                        sqlTypeCase.SetClrType(type);
                        return target;
                    case SqlNodeType.New:
                        SqlNew sqlNew = (SqlNew)target;
                        if (!type.IsAssignableFrom(sqlNew.ClrType))
                            return this.sql.TypedLiteralNull(type, target.SourceExpression);
                        return target;
                    case SqlNodeType.OptionalValue:
                        return this.ApplyTreat(((SqlOptionalValue)target).Value, type);
                    default:
                        SqlExpression sqlExpression = target;
                        if (sqlExpression != null && !type.IsAssignableFrom(sqlExpression.ClrType) && !sqlExpression.ClrType.IsAssignableFrom(type))
                            return this.sql.TypedLiteralNull(type, target.SourceExpression);
                        return target;
                }
            }

            internal override SqlExpression VisitTreat(SqlUnary a)
            {
                return this.VisitUnaryOperator(a);
            }

            internal override SqlExpression VisitUnaryOperator(SqlUnary uo)
            {
                uo.Operand = this.VisitExpression(uo.Operand);
                if (uo.NodeType == SqlNodeType.IsNull || uo.NodeType == SqlNodeType.IsNotNull)
                {
                    SqlExpression exp = this.translator.TranslateLinkIsNull(uo);
                    if (exp != uo)
                        return this.VisitExpression(exp);
                    if (uo.Operand.NodeType == SqlNodeType.OuterJoinedValue)
                    {
                        SqlUnary sqlUnary = uo.Operand as SqlUnary;
                        if (sqlUnary.Operand.NodeType == SqlNodeType.OptionalValue)
                        {
                            SqlOptionalValue sqlOptionalValue = (SqlOptionalValue)sqlUnary.Operand;
                            return this.VisitUnaryOperator(new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType, (SqlExpression)new SqlUnary(SqlNodeType.OuterJoinedValue, sqlOptionalValue.ClrType, sqlOptionalValue.SqlType, sqlOptionalValue.HasValue, sqlOptionalValue.SourceExpression), uo.SourceExpression));
                        }
                        if (sqlUnary.Operand.NodeType == SqlNodeType.TypeCase)
                        {
                            SqlTypeCase sqlTypeCase = (SqlTypeCase)sqlUnary.Operand;
                            return (SqlExpression)new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType, (SqlExpression)new SqlUnary(SqlNodeType.OuterJoinedValue, sqlTypeCase.Discriminator.ClrType, sqlTypeCase.Discriminator.SqlType, sqlTypeCase.Discriminator, sqlTypeCase.SourceExpression), uo.SourceExpression);
                        }
                    }
                }
                uo.Operand = this.ConvertToFetchedExpression((SqlNode)uo.Operand);
                if ((uo.NodeType == SqlNodeType.Not || uo.NodeType == SqlNodeType.Not2V) && uo.Operand.NodeType == SqlNodeType.Value)
                {
                    SqlValue sqlValue = (SqlValue)uo.Operand;
                    return this.sql.Value(typeof(bool), sqlValue.SqlType, (!(bool)sqlValue.Value ? 1 : 0)==1, sqlValue.IsClientSpecified, sqlValue.SourceExpression);
                }
                if (uo.NodeType == SqlNodeType.Not2V)
                {
                    bool? nullable = SqlExpressionNullability.CanBeNull(uo.Operand);
                    bool flag = false;
                    if ((nullable.GetValueOrDefault() == flag ? (!nullable.HasValue ? 1 : 0) : 1) == 0)
                        return (SqlExpression)this.sql.Unary(SqlNodeType.Not, uo.Operand);
                    Type clrType = typeof(int);
                    SqlWhen[] sqlWhenArray = new SqlWhen[1];
                    int index = 0;
                    SqlWhen sqlWhen = new SqlWhen(uo.Operand, this.sql.ValueFromObject((object)1, false, uo.SourceExpression));
                    sqlWhenArray[index] = sqlWhen;
                    SqlExpression @else = this.sql.ValueFromObject((object)0, false, uo.SourceExpression);
                    Expression sourceExpression = uo.SourceExpression;
                    return (SqlExpression)this.sql.Binary(SqlNodeType.EQ, (SqlExpression)new SqlSearchedCase(clrType, (IEnumerable<SqlWhen>)sqlWhenArray, @else, sourceExpression), this.sql.ValueFromObject((object)0, false, uo.SourceExpression));
                }
                if (uo.NodeType == SqlNodeType.Convert && uo.Operand.NodeType == SqlNodeType.Value)
                {
                    SqlValue sqlValue = (SqlValue)uo.Operand;
                    return this.sql.Value(uo.ClrType, uo.SqlType, DBConvert.ChangeType(sqlValue.Value, uo.ClrType), sqlValue.IsClientSpecified, sqlValue.SourceExpression);
                }
                if (uo.NodeType == SqlNodeType.IsNull || uo.NodeType == SqlNodeType.IsNotNull)
                {
                    bool? nullable = SqlExpressionNullability.CanBeNull(uo.Operand);
                    bool flag = false;
                    if ((nullable.GetValueOrDefault() == flag ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                        return this.sql.ValueFromObject((uo.NodeType == SqlNodeType.IsNotNull ? 1 : 0)==1, false, uo.SourceExpression);
                    SqlExpression operand = uo.Operand;
                    switch (operand.NodeType)
                    {
                        case SqlNodeType.OptionalValue:
                            uo.Operand = ((SqlOptionalValue)operand).HasValue;
                            return (SqlExpression)uo;
                        case SqlNodeType.TypeCase:
                            SqlTypeCase sqlTypeCase = (SqlTypeCase)uo.Operand;
                            List<SqlExpression> matches1 = new List<SqlExpression>();
                            List<SqlExpression> values1 = new List<SqlExpression>();
                            foreach (SqlTypeCaseWhen sqlTypeCaseWhen in sqlTypeCase.Whens)
                            {
                                SqlExpression sqlExpression = this.VisitUnaryOperator(new SqlUnary(uo.NodeType, uo.ClrType, uo.SqlType, sqlTypeCaseWhen.TypeBinding, sqlTypeCaseWhen.TypeBinding.SourceExpression));
                                if (sqlExpression is SqlNew)
                                    throw Error.DidNotExpectTypeBinding();
                                matches1.Add(sqlTypeCaseWhen.Match);
                                values1.Add(sqlExpression);
                            }
                            return this.sql.Case(uo.ClrType, sqlTypeCase.Discriminator, matches1, values1, sqlTypeCase.SourceExpression);
                        case SqlNodeType.Value:
                            SqlValue sqlValue = (SqlValue)uo.Operand;
                            return this.sql.Value(typeof(bool), this.typeProvider.From(typeof(int)), (sqlValue.Value == null == (uo.NodeType == SqlNodeType.IsNull) ? 1 : 0)==1, sqlValue.IsClientSpecified, uo.SourceExpression);
                        case SqlNodeType.ClientCase:
                            SqlClientCase sqlClientCase = (SqlClientCase)uo.Operand;
                            List<SqlExpression> matches2 = new List<SqlExpression>();
                            List<SqlExpression> values2 = new List<SqlExpression>();
                            foreach (SqlClientWhen sqlClientWhen in sqlClientCase.Whens)
                            {
                                matches2.Add(sqlClientWhen.Match);
                                values2.Add(this.VisitUnaryOperator(this.sql.Unary(uo.NodeType, sqlClientWhen.Value, sqlClientWhen.Value.SourceExpression)));
                            }
                            return this.sql.Case(sqlClientCase.ClrType, sqlClientCase.Expression, matches2, values2, sqlClientCase.SourceExpression);
                        case SqlNodeType.ClientQuery:
                            SqlClientQuery sqlClientQuery = (SqlClientQuery)operand;
                            if (sqlClientQuery.Query.NodeType != SqlNodeType.Element)
                                return this.sql.ValueFromObject((uo.NodeType == SqlNodeType.IsNotNull ? 1 : 0)==1, false, uo.SourceExpression);
                            SqlExpression sqlExpression1 = (SqlExpression)this.sql.SubSelect(SqlNodeType.Exists, sqlClientQuery.Query.Select);
                            if (uo.NodeType == SqlNodeType.IsNull)
                            {
                                SqlFactory sqlFactory = this.sql;
                                int num = 62;
                                SqlExpression expression = sqlExpression1;
                                Expression sourceExpression = expression.SourceExpression;
                                sqlExpression1 = (SqlExpression)sqlFactory.Unary((SqlNodeType)num, expression, sourceExpression);
                            }
                            return sqlExpression1;
                        case SqlNodeType.Element:
                            SqlExpression sqlExpression2 = (SqlExpression)this.sql.SubSelect(SqlNodeType.Exists, ((SqlSubSelect)operand).Select);
                            if (uo.NodeType == SqlNodeType.IsNull)
                            {
                                SqlFactory sqlFactory = this.sql;
                                int num = 62;
                                SqlExpression expression = sqlExpression2;
                                Expression sourceExpression = expression.SourceExpression;
                                sqlExpression2 = (SqlExpression)sqlFactory.Unary((SqlNodeType)num, expression, sourceExpression);
                            }
                            return sqlExpression2;
                    }
                }
                else if (uo.NodeType == SqlNodeType.Treat)
                    return this.ApplyTreat(this.VisitExpression(uo.Operand), uo.ClrType);
                return (SqlExpression)uo;
            }

            internal override SqlExpression VisitNew(SqlNew sox)
            {
                int index1 = 0;
                for (int count = sox.Args.Count; index1 < count; ++index1)
                    sox.Args[index1] = !this.inGroupBy ? this.FetchExpression(sox.Args[index1]) : this.VisitExpression(sox.Args[index1]);
                int index2 = 0;
                for (int count = sox.Members.Count; index2 < count; ++index2)
                {
                    SqlMemberAssign sqlMemberAssign = sox.Members[index2];
                    MetaDataMember dataMember = sox.MetaType.GetDataMember(sqlMemberAssign.Member);
                    MetaType inheritanceRoot = dataMember.DeclaringType.InheritanceRoot;
                    if (dataMember.IsAssociation && sqlMemberAssign.Expression != null && (sqlMemberAssign.Expression.NodeType != SqlNodeType.Link && this.shape != null) && (this.shape.IsPreloaded(dataMember.Member) && dataMember.LoadMethod == (MethodInfo)null && (this.alreadyIncluded != null && !this.alreadyIncluded.Contains(inheritanceRoot))))
                    {
                        this.alreadyIncluded.Add(inheritanceRoot);
                        sqlMemberAssign.Expression = this.VisitExpression(sqlMemberAssign.Expression);
                        this.alreadyIncluded.Remove(inheritanceRoot);
                    }
                    else
                        sqlMemberAssign.Expression = dataMember.IsAssociation || dataMember.IsDeferred ? this.VisitExpression(sqlMemberAssign.Expression) : this.FetchExpression(sqlMemberAssign.Expression);
                }
                return (SqlExpression)sox;
            }

            internal override SqlNode VisitMember(SqlMember m)
            {
                return this.AccessMember(m, this.FetchExpression(m.Expression));
            }

            private SqlNode AccessMember(SqlMember m, SqlExpression expo)
            {
                SqlExpression expr = expo;
                switch (expr.NodeType)
                {
                    case SqlNodeType.UserRow:
                        SqlUserRow sqlUserRow = (SqlUserRow)expr;
                        SqlUserQuery query = sqlUserRow.Query;
                        MetaDataMember inheritanceDataMember1 = SqlBinder.Visitor.GetRequiredInheritanceDataMember(sqlUserRow.RowType, m.Member);
                        string mappedName1 = inheritanceDataMember1.MappedName;
                        SqlUserColumn sqlUserColumn = query.Find(mappedName1);
                        if (sqlUserColumn == null)
                        {
                            ProviderType sqlType = this.sql.Default(inheritanceDataMember1);
                            sqlUserColumn = new SqlUserColumn(m.ClrType, sqlType, query, mappedName1, inheritanceDataMember1.IsPrimaryKey, m.SourceExpression);
                            query.Columns.Add(sqlUserColumn);
                        }
                        return (SqlNode)sqlUserColumn;
                    case SqlNodeType.Value:
                        SqlValue sqlValue = (SqlValue)expr;
                        if (sqlValue.Value == null)
                            return (SqlNode)this.sql.Value(m.ClrType, m.SqlType, (object)null, sqlValue.IsClientSpecified, m.SourceExpression);
                        if (m.Member is PropertyInfo)
                        {
                            PropertyInfo propertyInfo = (PropertyInfo)m.Member;
                            return (SqlNode)this.sql.Value(m.ClrType, m.SqlType, propertyInfo.GetValue(sqlValue.Value, (object[])null), sqlValue.IsClientSpecified, m.SourceExpression);
                        }
                        FieldInfo fieldInfo = (FieldInfo)m.Member;
                        return (SqlNode)this.sql.Value(m.ClrType, m.SqlType, fieldInfo.GetValue(sqlValue.Value), sqlValue.IsClientSpecified, m.SourceExpression);
                    case SqlNodeType.SimpleCase:
                        SqlSimpleCase sqlSimpleCase = (SqlSimpleCase)expr;
                        Type clrType1 = (Type)null;
                        List<SqlExpression> matches1 = new List<SqlExpression>();
                        List<SqlExpression> values1 = new List<SqlExpression>();
                        foreach (SqlWhen sqlWhen in sqlSimpleCase.Whens)
                        {
                            SqlExpression sqlExpression = (SqlExpression)this.AccessMember(m, sqlWhen.Value);
                            if (clrType1 == (Type)null)
                                clrType1 = sqlExpression.ClrType;
                            else if (clrType1 != sqlExpression.ClrType)
                                throw Error.ExpectedClrTypesToAgree((object)clrType1, (object)sqlExpression.ClrType);
                            matches1.Add(sqlWhen.Match);
                            values1.Add(sqlExpression);
                        }
                        return (SqlNode)this.sql.Case(clrType1, sqlSimpleCase.Expression, matches1, values1, sqlSimpleCase.SourceExpression);
                    case SqlNodeType.TypeCase:
                        SqlTypeCase sqlTypeCase = (SqlTypeCase)expr;
                        SqlNew sqlNew1 = sqlTypeCase.Whens[0].TypeBinding as SqlNew;
                        foreach (SqlTypeCaseWhen sqlTypeCaseWhen in sqlTypeCase.Whens)
                        {
                            if (sqlTypeCaseWhen.TypeBinding.NodeType == SqlNodeType.New)
                            {
                                SqlNew sqlNew2 = (SqlNew)sqlTypeCaseWhen.TypeBinding;
                                if (m.Member.DeclaringType.IsAssignableFrom(sqlNew2.ClrType))
                                {
                                    sqlNew1 = sqlNew2;
                                    break;
                                }
                            }
                        }
                        return this.AccessMember(m, (SqlExpression)sqlNew1);
                    case SqlNodeType.ScalarSubSelect:
                    case SqlNodeType.Element:
                        SqlSubSelect sqlSubSelect = (SqlSubSelect)expr;
                        SqlAlias alias1 = new SqlAlias((SqlNode)sqlSubSelect.Select);
                        SqlAliasRef sqlAliasRef1 = new SqlAliasRef(alias1);
                        SqlSelect sqlSelect1 = this.currentSelect;
                        try
                        {
                            SqlSelect select = new SqlSelect((SqlExpression)sqlAliasRef1, (SqlSource)alias1, sqlSubSelect.SourceExpression);
                            this.currentSelect = select;
                            SqlNode sqlNode = this.Visit((SqlNode)this.sql.Member((SqlExpression)sqlAliasRef1, m.Member));
                            SqlExpression expression = sqlNode as SqlExpression;
                            if (expression != null)
                            {
                                if (expression.NodeType == SqlNodeType.Member && !SqlColumnizer.CanBeColumn(expression))
                                {
                                    if (this.canUseOuterApply && expr.NodeType == SqlNodeType.Element && this.currentSelect != null)
                                    {
                                        this.currentSelect = sqlSelect1;
                                        this.currentSelect.From = (SqlSource)this.sql.MakeJoin(SqlJoinType.OuterApply, this.currentSelect.From, alias1, (SqlExpression)null, sqlSubSelect.SourceExpression);
                                        expr = this.VisitExpression((SqlExpression)sqlAliasRef1);
                                    }
                                    return (SqlNode)this.sql.Member(expr, m.Member);
                                }
                                select.Selection = expression;
                                select.Selection = this.columnizer.ColumnizeSelection(select.Selection);
                                select.Selection = this.ConvertLinks(select.Selection);
                                return (SqlNode)this.FoldSubquery(this.sql.SubSelect(expression is SqlTypeCase || !expression.SqlType.CanBeColumn ? SqlNodeType.Element : SqlNodeType.ScalarSubSelect, select));
                            }
                            SqlSelect sqlSelect2 = sqlNode as SqlSelect;
                            if (sqlSelect2 == null)
                                throw Error.UnexpectedNode((object)sqlNode.NodeType);
                            SqlAlias alias2 = new SqlAlias((SqlNode)sqlSelect2);
                            SqlAliasRef sqlAliasRef2 = new SqlAliasRef(alias2);
                            select.Selection = this.ConvertLinks(this.VisitExpression((SqlExpression)sqlAliasRef2));
                            select.From = (SqlSource)new SqlJoin(SqlJoinType.CrossApply, (SqlSource)alias1, (SqlSource)alias2, (SqlExpression)null, m.SourceExpression);
                            return (SqlNode)select;
                        }
                        finally
                        {
                            this.currentSelect = sqlSelect1;
                        }
                    case SqlNodeType.SearchedCase:
                        SqlSearchedCase sqlSearchedCase = (SqlSearchedCase)expr;
                        List<SqlWhen> list = new List<SqlWhen>(sqlSearchedCase.Whens.Count);
                        foreach (SqlWhen sqlWhen in sqlSearchedCase.Whens)
                        {
                            SqlExpression sqlExpression = (SqlExpression)this.AccessMember(m, sqlWhen.Value);
                            list.Add(new SqlWhen(sqlWhen.Match, sqlExpression));
                        }
                        SqlExpression @else = (SqlExpression)this.AccessMember(m, sqlSearchedCase.Else);
                        return (SqlNode)this.sql.SearchedCase(list.ToArray(), @else, sqlSearchedCase.SourceExpression);
                    case SqlNodeType.OptionalValue:
                        return this.AccessMember(m, ((SqlOptionalValue)expr).Value);
                    case SqlNodeType.OuterJoinedValue:
                        SqlNode sqlNode1 = this.AccessMember(m, ((SqlUnary)expr).Operand);
                        SqlExpression expression1 = sqlNode1 as SqlExpression;
                        if (expression1 != null)
                            return (SqlNode)this.sql.Unary(SqlNodeType.OuterJoinedValue, expression1);
                        return sqlNode1;
                    case SqlNodeType.Grouping:
                        SqlGrouping sqlGrouping = (SqlGrouping)expr;
                        if (m.Member.Name == "Key")
                            return (SqlNode)sqlGrouping.Key;
                        break;
                    case SqlNodeType.New:
                        SqlNew sqlNew3 = (SqlNew)expr;
                        SqlExpression sqlExpression1 = sqlNew3.Find(m.Member);
                        if (sqlExpression1 != null)
                            return (SqlNode)sqlExpression1;
                        MetaDataMember metaDataMember = System.Linq.Enumerable.FirstOrDefault<MetaDataMember>((IEnumerable<MetaDataMember>)sqlNew3.MetaType.PersistentDataMembers, (Func<MetaDataMember, bool>)(p => p.Member == m.Member));
                        if (!sqlNew3.SqlType.CanBeColumn && metaDataMember != null)
                            throw Error.MemberNotPartOfProjection((object)m.Member.DeclaringType, (object)m.Member.Name);
                        break;
                    case SqlNodeType.Lift:
                        return this.AccessMember(m, ((SqlLift)expr).Expression);
                    case SqlNodeType.AliasRef:
                        SqlAliasRef sqlAliasRef3 = (SqlAliasRef)expr;
                        SqlTable sqlTable = sqlAliasRef3.Alias.Node as SqlTable;
                        if (sqlTable != null)
                        {
                            MetaDataMember inheritanceDataMember2 = SqlBinder.Visitor.GetRequiredInheritanceDataMember(sqlTable.RowType, m.Member);
                            string mappedName2 = inheritanceDataMember2.MappedName;
                            SqlColumn col = sqlTable.Find(mappedName2);
                            if (col == null)
                            {
                                ProviderType sqlType = this.sql.Default(inheritanceDataMember2);
                                col = new SqlColumn(m.ClrType, sqlType, mappedName2, inheritanceDataMember2, (SqlExpression)null, m.SourceExpression);
                                col.Alias = sqlAliasRef3.Alias;
                                sqlTable.Columns.Add(col);
                            }
                            return (SqlNode)new SqlColumnRef(col);
                        }
                        SqlTableValuedFunctionCall valuedFunctionCall = sqlAliasRef3.Alias.Node as SqlTableValuedFunctionCall;
                        if (valuedFunctionCall != null)
                        {
                            MetaDataMember inheritanceDataMember2 = SqlBinder.Visitor.GetRequiredInheritanceDataMember(valuedFunctionCall.RowType, m.Member);
                            string mappedName2 = inheritanceDataMember2.MappedName;
                            SqlColumn col = valuedFunctionCall.Find(mappedName2);
                            if (col == null)
                            {
                                ProviderType sqlType = this.sql.Default(inheritanceDataMember2);
                                col = new SqlColumn(m.ClrType, sqlType, mappedName2, inheritanceDataMember2, (SqlExpression)null, m.SourceExpression);
                                col.Alias = sqlAliasRef3.Alias;
                                valuedFunctionCall.Columns.Add(col);
                            }
                            return (SqlNode)new SqlColumnRef(col);
                        }
                        break;
                    case SqlNodeType.ClientCase:
                        SqlClientCase sqlClientCase = (SqlClientCase)expr;
                        Type clrType2 = (Type)null;
                        List<SqlExpression> matches2 = new List<SqlExpression>();
                        List<SqlExpression> values2 = new List<SqlExpression>();
                        foreach (SqlClientWhen sqlClientWhen in sqlClientCase.Whens)
                        {
                            SqlExpression sqlExpression2 = (SqlExpression)this.AccessMember(m, sqlClientWhen.Value);
                            if (clrType2 == (Type)null)
                                clrType2 = sqlExpression2.ClrType;
                            else if (clrType2 != sqlExpression2.ClrType)
                                throw Error.ExpectedClrTypesToAgree((object)clrType2, (object)sqlExpression2.ClrType);
                            matches2.Add(sqlClientWhen.Match);
                            values2.Add(sqlExpression2);
                        }
                        return (SqlNode)this.sql.Case(clrType2, sqlClientCase.Expression, matches2, values2, sqlClientCase.SourceExpression);
                    case SqlNodeType.ClientParameter:
                        SqlClientParameter sqlClientParameter = (SqlClientParameter)expr;
                        Type type1 = typeof(Func<,>);
                        Type[] typeArray = new Type[2];
                        int index1 = 0;
                        Type type2 = typeof(object[]);
                        typeArray[index1] = type2;
                        int index2 = 1;
                        Type clrType3 = m.ClrType;
                        typeArray[index2] = clrType3;
                        LambdaExpression accessor = Expression.Lambda(type1.MakeGenericType(typeArray), (Expression)Expression.MakeMemberAccess(sqlClientParameter.Accessor.Body, m.Member), (IEnumerable<ParameterExpression>)sqlClientParameter.Accessor.Parameters);
                        return (SqlNode)new SqlClientParameter(m.ClrType, m.SqlType, accessor, sqlClientParameter.SourceExpression);
                }
                if (m.Expression == expr)
                    return (SqlNode)m;
                return (SqlNode)this.sql.Member(expr, m.Member);
            }

            private SqlExpression FoldSubquery(SqlSubSelect ss)
            {
                while (true)
                {
                    SqlSelect select1 = null;
                    SqlSelect select2;
                    for (ss = this.sql.SubSelect(SqlNodeType.Multiset, select1, ss.ClrType); ss.NodeType != SqlNodeType.Element || ss.Select.Selection.NodeType != SqlNodeType.Multiset; ss = this.sql.SubSelect(SqlNodeType.Element, select2))
                    {
                        if (ss.NodeType != SqlNodeType.Element || ss.Select.Selection.NodeType != SqlNodeType.Element)
                            return (SqlExpression)ss;
                        SqlAlias alias = new SqlAlias((SqlNode)((SqlSubSelect)ss.Select.Selection).Select);
                        SqlAliasRef sqlAliasRef = new SqlAliasRef(alias);
                        select2 = ss.Select;
                        select2.Selection = this.ConvertLinks(this.VisitExpression((SqlExpression)sqlAliasRef));
                        select2.From = (SqlSource)new SqlJoin(SqlJoinType.CrossApply, select2.From, (SqlSource)alias, (SqlExpression)null, ss.SourceExpression);
                    }
                    SqlAlias alias1 = new SqlAlias((SqlNode)((SqlSubSelect)ss.Select.Selection).Select);
                    SqlAliasRef sqlAliasRef1 = new SqlAliasRef(alias1);
                    select1 = ss.Select;
                    select1.Selection = this.ConvertLinks(this.VisitExpression((SqlExpression)sqlAliasRef1));
                    select1.From = (SqlSource)new SqlJoin(SqlJoinType.CrossApply, select1.From, (SqlSource)alias1, (SqlExpression)null, ss.SourceExpression);
                }
            }

            private static MetaDataMember GetRequiredInheritanceDataMember(MetaType type, MemberInfo mi)
            {
                MetaType inheritanceType = type.GetInheritanceType(mi.DeclaringType);
                if (inheritanceType == null)
                    throw Error.UnmappedDataMember((object)mi, (object)mi.DeclaringType, (object)type);
                MemberInfo member = mi;
                return inheritanceType.GetDataMember(member);
            }

            internal override SqlStatement VisitAssign(SqlAssign sa)
            {
                sa.LValue = this.FetchExpression(sa.LValue);
                sa.RValue = this.FetchExpression(sa.RValue);
                return (SqlStatement)sa;
            }

            internal SqlExpression ExpandExpression(SqlExpression expression)
            {
                SqlExpression exp = this.expander.Expand(expression);
                if (exp != expression)
                    exp = this.VisitExpression(exp);
                return exp;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                return this.ExpandExpression((SqlExpression)aref);
            }

            internal override SqlAlias VisitAlias(SqlAlias a)
            {
                SqlAlias sqlAlias = this.currentAlias;
                if (a.Node.NodeType == SqlNodeType.Table)
                    this.outerAliasMap[a] = this.currentAlias;
                this.currentAlias = a;
                try
                {
                    a.Node = this.ConvertToFetchedSequence(this.Visit(a.Node));
                    return a;
                }
                finally
                {
                    this.currentAlias = sqlAlias;
                }
            }

            internal override SqlNode VisitLink(SqlLink link)
            {
                link = (SqlLink)base.VisitLink(link);
                if (!this.disableInclude && this.shape != null && this.alreadyIncluded != null)
                {
                    MetaDataMember member = link.Member;
                    if (this.shape.IsPreloaded(member.Member) && member.LoadMethod == (MethodInfo)null)
                    {
                        MetaType inheritanceRoot = member.DeclaringType.InheritanceRoot;
                        if (!this.alreadyIncluded.Contains(inheritanceRoot))
                        {
                            this.alreadyIncluded.Add(inheritanceRoot);
                            SqlExpression sqlExpression = this.ConvertToFetchedExpression((SqlNode)link);
                            this.alreadyIncluded.Remove(inheritanceRoot);
                            return (SqlNode)sqlExpression;
                        }
                    }
                }
                if (this.inGroupBy && link.Expansion != null)
                    return (SqlNode)this.VisitLinkExpansion(link);
                return (SqlNode)link;
            }

            internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
            {
                return (SqlExpression)SqlDuplicator.Copy((SqlNode)sref.SharedExpression.Expression);
            }

            internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared)
            {
                shared.Expression = this.VisitExpression(shared.Expression);
                if (shared.Expression.NodeType == SqlNodeType.ColumnRef)
                    return shared.Expression;
                shared.Expression = this.PushDownExpression(shared.Expression);
                return shared.Expression;
            }

            internal override SqlExpression VisitSimpleExpression(SqlSimpleExpression simple)
            {
                simple.Expression = this.VisitExpression(simple.Expression);
                if (SimpleExpression.IsSimple(simple.Expression))
                    return simple.Expression;
                return this.PushDownExpression(simple.Expression);
            }

            private SqlExpression PushDownExpression(SqlExpression expr)
            {
                if (expr.NodeType == SqlNodeType.Value && expr.SqlType.CanBeColumn)
                {
                    Type clrType = expr.ClrType;
                    ProviderType sqlType = expr.SqlType;
                    // ISSUE: variable of the null type
                    //__Null local1 = null;
                    // ISSUE: variable of the null type
                    //__Null local2 = null;
                    SqlExpression expr1 = expr;
                    Expression sourceExpression = expr1.SourceExpression;
                    expr = (SqlExpression)new SqlColumn(clrType, sqlType, (string)null, (MetaDataMember)null, expr1, sourceExpression);
                }
                else
                    expr = this.columnizer.ColumnizeSelection(expr);
                this.currentSelect.From = (SqlSource)new SqlAlias((SqlNode)new SqlSelect(expr, this.currentSelect.From, expr.SourceExpression));
                return this.ExpandExpression(expr);
            }

            internal override SqlSource VisitJoin(SqlJoin join)
            {
                if (join.JoinType != SqlJoinType.CrossApply && join.JoinType != SqlJoinType.OuterApply)
                    return base.VisitJoin(join);
                join.Left = this.VisitSource(join.Left);
                SqlSelect sqlSelect = this.currentSelect;
                try
                {
                    this.currentSelect = this.GetSourceSelect(join.Left);
                    join.Right = this.VisitSource(join.Right);
                    this.currentSelect = (SqlSelect)null;
                    join.Condition = this.VisitExpression(join.Condition);
                    return (SqlSource)join;
                }
                finally
                {
                    this.currentSelect = sqlSelect;
                }
            }

            private SqlSelect GetSourceSelect(SqlSource source)
            {
                SqlAlias sqlAlias = source as SqlAlias;
                if (sqlAlias == null)
                    return (SqlSelect)null;
                return sqlAlias.Node as SqlSelect;
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                SqlBinder.LinkOptimizationScope optimizationScope = this.linkMap;
                SqlSelect sqlSelect = this.currentSelect;
                bool flag1 = this.inGroupBy;
                this.inGroupBy = false;
                try
                {
                    bool flag2 = true;
                    if (this.binder.optimizeLinkExpansions && (select.GroupBy.Count > 0 || this.aggregateChecker.HasAggregates((SqlNode)select) || select.IsDistinct))
                    {
                        flag2 = false;
                        this.linkMap = new SqlBinder.LinkOptimizationScope(this.linkMap);
                    }
                    select.From = this.VisitSource(select.From);
                    this.currentSelect = select;
                    select.Where = this.VisitExpression(select.Where);
                    this.inGroupBy = true;
                    int index1 = 0;
                    for (int count = select.GroupBy.Count; index1 < count; ++index1)
                        select.GroupBy[index1] = this.VisitExpression(select.GroupBy[index1]);
                    this.inGroupBy = false;
                    select.Having = this.VisitExpression(select.Having);
                    int index2 = 0;
                    for (int count = select.OrderBy.Count; index2 < count; ++index2)
                        select.OrderBy[index2].Expression = this.VisitExpression(select.OrderBy[index2].Expression);
                    select.Top = this.VisitExpression(select.Top);
                    select.Row = (SqlRow)this.Visit((SqlNode)select.Row);
                    select.Selection = this.VisitExpression(select.Selection);
                    select.Selection = this.columnizer.ColumnizeSelection(select.Selection);
                    if (flag2)
                        select.Selection = this.ConvertLinks(select.Selection);
                    if (select.Where != null)
                    {
                        if (select.Where.NodeType == SqlNodeType.Value)
                        {
                            if ((bool)((SqlValue)select.Where).Value)
                                select.Where = (SqlExpression)null;
                        }
                    }
                }
                finally
                {
                    this.currentSelect = sqlSelect;
                    this.linkMap = optimizationScope;
                    this.inGroupBy = flag1;
                }
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                SqlBinder.LinkOptimizationScope optimizationScope = this.linkMap;
                SqlSelect sqlSelect = this.currentSelect;
                try
                {
                    this.linkMap = new SqlBinder.LinkOptimizationScope(this.linkMap);
                    this.currentSelect = (SqlSelect)null;
                    return base.VisitSubSelect(ss);
                }
                finally
                {
                    this.linkMap = optimizationScope;
                    this.currentSelect = sqlSelect;
                }
            }

            private SqlExpression ConvertLinks(SqlExpression node)
            {
                if (node == null)
                    return (SqlExpression)null;
                switch (node.NodeType)
                {
                    case SqlNodeType.Link:
                        return this.ConvertToFetchedExpression((SqlNode)node);
                    case SqlNodeType.OuterJoinedValue:
                        SqlExpression operand = ((SqlUnary)node).Operand;
                        SqlExpression expression = this.ConvertLinks(operand);
                        if (expression == operand)
                            return node;
                        if (expression.NodeType != SqlNodeType.OuterJoinedValue)
                            return (SqlExpression)this.sql.Unary(SqlNodeType.OuterJoinedValue, expression);
                        return expression;
                    case SqlNodeType.ClientCase:
                        SqlClientCase sqlClientCase = (SqlClientCase)node;
                        foreach (SqlClientWhen sqlClientWhen in sqlClientCase.Whens)
                        {
                            SqlExpression sqlExpression = this.ConvertLinks(sqlClientWhen.Value);
                            sqlClientWhen.Value = sqlExpression;
                            if (!sqlClientCase.ClrType.IsAssignableFrom(sqlClientWhen.Value.ClrType))
                                throw Error.DidNotExpectTypeChange((object)sqlClientWhen.Value.ClrType, (object)sqlClientCase.ClrType);
                        }
                        return node;
                    case SqlNodeType.Column:
                        SqlColumn sqlColumn = (SqlColumn)node;
                        if (sqlColumn.Expression != null)
                            sqlColumn.Expression = this.ConvertLinks(sqlColumn.Expression);
                        return node;
                    default:
                        return node;
                }
            }

            internal SqlExpression ConvertToExpression(SqlNode node)
            {
                if (node == null)
                    return (SqlExpression)null;
                SqlExpression sqlExpression = node as SqlExpression;
                if (sqlExpression != null)
                    return sqlExpression;
                SqlSelect select = node as SqlSelect;
                if (select != null)
                    return (SqlExpression)this.sql.SubSelect(SqlNodeType.Multiset, select);
                throw Error.UnexpectedNode((object)node.NodeType);
            }

            internal SqlExpression ConvertToFetchedExpression(SqlNode node)
            {
                if (node == null)
                    return (SqlExpression)null;
                switch (node.NodeType)
                {
                    case SqlNodeType.OuterJoinedValue:
                        SqlExpression operand = ((SqlUnary)node).Operand;
                        SqlExpression sqlExpression1 = this.ConvertLinks(operand);
                        if (sqlExpression1 == operand)
                            return (SqlExpression)node;
                        return sqlExpression1;
                    case SqlNodeType.SearchedCase:
                        SqlSearchedCase sqlSearchedCase = (SqlSearchedCase)node;
                        foreach (SqlWhen sqlWhen in sqlSearchedCase.Whens)
                        {
                            sqlWhen.Match = this.ConvertToFetchedExpression((SqlNode)sqlWhen.Match);
                            sqlWhen.Value = this.ConvertToFetchedExpression((SqlNode)sqlWhen.Value);
                        }
                        sqlSearchedCase.Else = this.ConvertToFetchedExpression((SqlNode)sqlSearchedCase.Else);
                        break;
                    case SqlNodeType.TypeCase:
                        SqlTypeCase sqlTypeCase = (SqlTypeCase)node;
                        List<SqlNode> list = new List<SqlNode>();
                        foreach (SqlTypeCaseWhen sqlTypeCaseWhen in sqlTypeCase.Whens)
                        {
                            SqlNode sqlNode = (SqlNode)this.ConvertToFetchedExpression((SqlNode)sqlTypeCaseWhen.TypeBinding);
                            list.Add(sqlNode);
                        }
                        int index1 = 0;
                        for (int count = list.Count; index1 < count; ++index1)
                        {
                            SqlExpression sqlExpression2 = (SqlExpression)list[index1];
                            sqlTypeCase.Whens[index1].TypeBinding = sqlExpression2;
                        }
                        break;
                    case SqlNodeType.ClientCase:
                        SqlClientCase clientCase = (SqlClientCase)node;
                        List<SqlNode> sequences = new List<SqlNode>();
                        bool flag = true;
                        foreach (SqlClientWhen sqlClientWhen in clientCase.Whens)
                        {
                            SqlNode sqlNode = (SqlNode)this.ConvertToFetchedExpression((SqlNode)sqlClientWhen.Value);
                            flag = flag && sqlNode is SqlExpression;
                            sequences.Add(sqlNode);
                        }
                        if (flag)
                        {
                            List<SqlExpression> matches = new List<SqlExpression>();
                            List<SqlExpression> values = new List<SqlExpression>();
                            int index2 = 0;
                            for (int count = sequences.Count; index2 < count; ++index2)
                            {
                                SqlExpression sqlExpression2 = (SqlExpression)sequences[index2];
                                if (!clientCase.ClrType.IsAssignableFrom(sqlExpression2.ClrType))
                                    throw Error.DidNotExpectTypeChange((object)clientCase.ClrType, (object)sqlExpression2.ClrType);
                                matches.Add(clientCase.Whens[index2].Match);
                                values.Add(sqlExpression2);
                            }
                            node = (SqlNode)this.sql.Case(clientCase.ClrType, clientCase.Expression, matches, values, clientCase.SourceExpression);
                            break;
                        }
                        node = (SqlNode)this.SimulateCaseOfSequences(clientCase, sequences);
                        break;
                    case SqlNodeType.Link:
                        SqlLink link = (SqlLink)node;
                        if (link.Expansion != null)
                            return this.VisitLinkExpansion(link);
                        SqlExpression expr;
                        if (this.linkMap.TryGetValue(link.Id, out expr))
                            return this.VisitExpression(expr);
                        node = this.translator.TranslateLink(link, true);
                        node = this.binder.Prebind(node);
                        node = (SqlNode)this.ConvertToExpression(node);
                        node = this.Visit(node);
                        if (this.currentSelect != null && node != null && (node.NodeType == SqlNodeType.Element && link.Member.IsAssociation) && this.binder.OptimizeLinkExpansions)
                        {
                            SqlJoinType joinType = !link.Member.Association.IsForeignKey || link.Member.Association.IsNullable ? SqlJoinType.LeftOuter : SqlJoinType.Inner;
                            SqlSubSelect sqlSubSelect = (SqlSubSelect)node;
                            SqlExpression where = sqlSubSelect.Select.Where;
                            sqlSubSelect.Select.Where = (SqlExpression)null;
                            SqlAlias alias = new SqlAlias((SqlNode)sqlSubSelect.Select);
                            if (joinType == SqlJoinType.Inner && this.IsOuterDependent(this.currentSelect.From, alias, where))
                                joinType = SqlJoinType.LeftOuter;
                            this.currentSelect.From = (SqlSource)this.sql.MakeJoin(joinType, this.currentSelect.From, alias, where, sqlSubSelect.SourceExpression);
                            SqlExpression sqlExpression2 = (SqlExpression)new SqlAliasRef(alias);
                            this.linkMap.Add(link.Id, sqlExpression2);
                            return this.VisitExpression(sqlExpression2);
                        }
                        break;
                }
                return (SqlExpression)node;
            }

            private bool IsOuterDependent(SqlSource location, SqlAlias alias, SqlExpression where)
            {
                HashSet<SqlAlias> consumed = SqlGatherConsumedAliases.Gather((SqlNode)where);
                consumed.ExceptWith((IEnumerable<SqlAlias>)SqlGatherProducedAliases.Gather((SqlNode)alias));
                HashSet<SqlAlias> produced;
                return this.IsOuterDependent(false, location, consumed, out produced);
            }

            private bool IsOuterDependent(bool isOuterDependent, SqlSource location, HashSet<SqlAlias> consumed, out HashSet<SqlAlias> produced)
            {
                if (location.NodeType == SqlNodeType.Join)
                {
                    SqlJoin sqlJoin = (SqlJoin)location;
                    HashSet<SqlAlias> produced1;
                    if (this.IsOuterDependent(isOuterDependent, sqlJoin.Left, consumed, out produced) || this.IsOuterDependent(sqlJoin.JoinType == SqlJoinType.LeftOuter || sqlJoin.JoinType == SqlJoinType.OuterApply, sqlJoin.Right, consumed, out produced1))
                        return true;
                    produced.UnionWith((IEnumerable<SqlAlias>)produced1);
                }
                else
                {
                    SqlAlias sqlAlias = location as SqlAlias;
                    if (sqlAlias != null)
                    {
                        SqlSelect sqlSelect = sqlAlias.Node as SqlSelect;
                        if (sqlSelect != null && !isOuterDependent && (sqlSelect.From != null && this.IsOuterDependent(false, sqlSelect.From, consumed, out produced)))
                            return true;
                    }
                    produced = SqlGatherProducedAliases.Gather((SqlNode)location);
                }
                if (consumed.IsSubsetOf((IEnumerable<SqlAlias>)produced))
                    return isOuterDependent;
                return false;
            }

            internal SqlNode ConvertToFetchedSequence(SqlNode node)
            {
                if (node == null)
                    return node;
                while (node.NodeType == SqlNodeType.OuterJoinedValue)
                    node = (SqlNode)((SqlUnary)node).Operand;
                SqlExpression sqlExpression = node as SqlExpression;
                if (sqlExpression == null)
                    return node;
                if (!TypeSystem.IsSequenceType(sqlExpression.ClrType))
                    throw Error.SequenceOperatorsNotSupportedForType((object)sqlExpression.ClrType);
                if (sqlExpression.NodeType == SqlNodeType.Value)
                    throw Error.QueryOnLocalCollectionNotSupported();
                if (sqlExpression.NodeType == SqlNodeType.Link)
                {
                    SqlLink link = (SqlLink)sqlExpression;
                    if (link.Expansion != null)
                        return (SqlNode)this.VisitLinkExpansion(link);
                    node = this.translator.TranslateLink(link, false);
                    node = this.binder.Prebind(node);
                    node = this.Visit(node);
                }
                else if (sqlExpression.NodeType == SqlNodeType.Grouping)
                    node = (SqlNode)((SqlGrouping)sqlExpression).Group;
                else if (sqlExpression.NodeType == SqlNodeType.ClientCase)
                {
                    SqlClientCase clientCase = (SqlClientCase)sqlExpression;
                    List<SqlNode> sequences = new List<SqlNode>();
                    bool flag1 = false;
                    bool flag2 = true;
                    foreach (SqlClientWhen sqlClientWhen in clientCase.Whens)
                    {
                        SqlNode sqlNode = this.ConvertToFetchedSequence((SqlNode)sqlClientWhen.Value);
                        flag1 = flag1 || sqlNode != sqlClientWhen.Value;
                        sequences.Add(sqlNode);
                        flag2 = flag2 && SqlComparer.AreEqual((SqlNode)sqlClientWhen.Value, (SqlNode)clientCase.Whens[0].Value);
                    }
                    if (flag1)
                        node = !flag2 ? (SqlNode)this.SimulateCaseOfSequences(clientCase, sequences) : sequences[0];
                }
                SqlSubSelect sqlSubSelect = node as SqlSubSelect;
                if (sqlSubSelect != null)
                    node = (SqlNode)sqlSubSelect.Select;
                return node;
            }

            private SqlExpression VisitLinkExpansion(SqlLink link)
            {
                SqlAliasRef sqlAliasRef = link.Expansion as SqlAliasRef;
                SqlAlias alias;
                if (sqlAliasRef != null && sqlAliasRef.Alias.Node.NodeType == SqlNodeType.Table && this.outerAliasMap.TryGetValue(sqlAliasRef.Alias, out alias))
                    return this.VisitAliasRef(new SqlAliasRef(alias));
                return this.VisitExpression(link.Expansion);
            }

            private SqlSelect SimulateCaseOfSequences(SqlClientCase clientCase, List<SqlNode> sequences)
            {
                if (sequences.Count == 1)
                    return (SqlSelect)sequences[0];
                SqlNode sqlNode = (SqlNode)null;
                int index1 = clientCase.Whens.Count - 1;
                int num = clientCase.Whens[index1].Match == null ? 1 : 0;
                SqlExpression sqlExpression = (SqlExpression)null;
                for (int index2 = 0; index2 < sequences.Count - num; ++index2)
                {
                    SqlSelect sqlSelect = (SqlSelect)sequences[index2];
                    SqlExpression right = (SqlExpression)this.sql.Binary(SqlNodeType.EQ, clientCase.Expression, clientCase.Whens[index2].Match);
                    sqlSelect.Where = this.sql.AndAccumulate(sqlSelect.Where, right);
                    sqlExpression = this.sql.AndAccumulate(sqlExpression, (SqlExpression)this.sql.Binary(SqlNodeType.NE, clientCase.Expression, clientCase.Whens[index2].Match));
                    sqlNode = sqlNode != null ? (SqlNode)new SqlUnion((SqlNode)sqlSelect, sqlNode, true) : (SqlNode)sqlSelect;
                }
                if (num == 1)
                {
                    SqlSelect sqlSelect = (SqlSelect)sequences[index1];
                    sqlSelect.Where = this.sql.AndAccumulate(sqlSelect.Where, sqlExpression);
                    sqlNode = sqlNode != null ? (SqlNode)new SqlUnion((SqlNode)sqlSelect, sqlNode, true) : (SqlNode)sqlSelect;
                }
                SqlAlias alias = new SqlAlias(sqlNode);
                return new SqlSelect((SqlExpression)new SqlAliasRef(alias), (SqlSource)alias, sqlNode.SourceExpression);
            }
        }
    }
}
