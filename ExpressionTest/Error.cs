using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExpressionTest
{
    internal static class Error
    {
        internal static Exception CouldNotGetSqlType()
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotGetSqlType);
        }
        internal static Exception InvalidProviderType(object p0)
        {
            return (Exception)new NotSupportedException(Strings.InvalidProviderType(p0));
        }

        internal static Exception ColumnIsDefinedInMultiplePlaces(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.ColumnIsDefinedInMultiplePlaces(p0));
        }
        internal static Exception ValueHasNoLiteralInSql(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.ValueHasNoLiteralInSql(p0));
        }
        internal static Exception CannotCompareItemsAssociatedWithDifferentTable()
        {
            return (Exception)new InvalidOperationException(Strings.CannotCompareItemsAssociatedWithDifferentTable);
        }
        internal static Exception InvalidFormatNode(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.InvalidFormatNode(p0));
        }

        

        internal static Exception MemberCouldNotBeTranslated(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.MemberCouldNotBeTranslated(p0, p1));
        }
        internal static Exception NoMethodInTypeMatchingArguments(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.NoMethodInTypeMatchingArguments(p0));
        }
        internal static Exception CouldNotGetClrType()
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotGetClrType);
        }
        internal static Exception MappedTypeMustHaveDefaultConstructor(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.MappedTypeMustHaveDefaultConstructor(p0));
        }
        internal static Exception UnexpectedTypeCode(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.UnexpectedTypeCode(p0));
        }
        internal static Exception ColumnReferencedIsNotInScope(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.ColumnReferencedIsNotInScope(p0));
        }
        internal static Exception EmptyCaseNotSupported()
        {
            return (Exception)new InvalidOperationException(Strings.EmptyCaseNotSupported);
        }
        internal static Exception CannotAssignNull(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CannotAssignNull(p0));
        }

        internal static Exception InheritanceTypeHasMultipleDefaults(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.InheritanceTypeHasMultipleDefaults(p0));
        }

        internal static Exception InheritanceHierarchyDoesNotDefineDefault(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.InheritanceHierarchyDoesNotDefineDefault(p0));
        }
        internal static Exception InheritanceCodeUsedForMultipleTypes(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.InheritanceCodeUsedForMultipleTypes(p0));
        }
        internal static Exception InheritanceTypeHasMultipleDiscriminators(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.InheritanceTypeHasMultipleDiscriminators(p0));
        }
        internal static Exception InheritanceCodeMayNotBeNull()
        {
            return (Exception)new InvalidOperationException(Strings.InheritanceCodeMayNotBeNull);
        }
        internal static Exception AbstractClassAssignInheritanceDiscriminator(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.AbstractClassAssignInheritanceDiscriminator(p0));
        }
        internal static Exception InheritanceTypeDoesNotDeriveFromRoot(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.InheritanceTypeDoesNotDeriveFromRoot(p0, p1));
        }

        internal static Exception DiscriminatorClrTypeNotSupported(object p0, object p1, object p2)
        {
            return (Exception)new NotSupportedException(Strings.DiscriminatorClrTypeNotSupported(p0, p1, p2));
        }
        internal static Exception NoDiscriminatorFound(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.NoDiscriminatorFound(p0));
        }   
        internal static Exception MemberMappedMoreThanOnce(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.MemberMappedMoreThanOnce(p0));
        }
        internal static Exception NonInheritanceClassHasDiscriminator(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.NonInheritanceClassHasDiscriminator(p0));
        }
        internal static Exception InheritanceSubTypeIsAlsoRoot(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.InheritanceSubTypeIsAlsoRoot(p0));
        }
        internal static Exception TooManyResultTypesDeclaredForFunction(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.TooManyResultTypesDeclaredForFunction(p0));
        }
        internal static Exception NoResultTypesDeclaredForFunction(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.NoResultTypesDeclaredForFunction(p0));
        }
        internal static Exception BadKeyMember(object p0, object p1, object p2)
        {
            return (Exception)new InvalidOperationException(Strings.BadKeyMember(p0, p1, p2));
        }
        internal static Exception InvalidDeleteOnNullSpecification(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.InvalidDeleteOnNullSpecification(p0));
        }

        internal static Exception MismatchedThisKeyOtherKey(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.MismatchedThisKeyOtherKey(p0, p1));
        }
        internal static Exception IncorrectAutoSyncSpecification(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.IncorrectAutoSyncSpecification(p0));
        }
        internal static Exception BadStorageProperty(object p0, object p1, object p2)
        {
            return (Exception)new InvalidOperationException(Strings.BadStorageProperty(p0, p1, p2));
        }
        internal static Exception InvalidFieldInfo(object p0, object p1, object p2)
        {
            return (Exception)new ArgumentException(Strings.InvalidFieldInfo(p0, p1, p2));
        }
        internal static Exception PrimaryKeyInSubTypeNotSupported(object p0, object p1)
        {
            return (Exception)new NotSupportedException(Strings.PrimaryKeyInSubTypeNotSupported(p0, p1));
        }
        internal static Exception UnableToAssignValueToReadonlyProperty(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.UnableToAssignValueToReadonlyProperty(p0));
        }
        internal static Exception CouldNotCreateAccessorToProperty(object p0, object p1, object p2)
        {
            return (Exception)new ArgumentException(Strings.CouldNotCreateAccessorToProperty(p0, p1, p2));
        }
        internal static Exception LinkAlreadyLoaded()
        {
            return (Exception)new InvalidOperationException(Strings.LinkAlreadyLoaded);
        }
        internal static Exception EntityRefAlreadyLoaded()
        {
            return (Exception)new InvalidOperationException(Strings.EntityRefAlreadyLoaded);
        }
        internal static Exception UnhandledDeferredStorageType(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.UnhandledDeferredStorageType(p0));
        }
        internal static Exception TwoMembersMarkedAsPrimaryKeyAndDBGenerated(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.TwoMembersMarkedAsPrimaryKeyAndDBGenerated(p0, p1));
        }
        internal static Exception IdentityClrTypeNotSupported(object p0, object p1, object p2)
        {
            return (Exception)new NotSupportedException(Strings.IdentityClrTypeNotSupported(p0, p1, p2));
        }
        internal static Exception TwoMembersMarkedAsRowVersion(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.TwoMembersMarkedAsRowVersion(p0, p1));
        }
        internal static Exception TwoMembersMarkedAsInheritanceDiscriminator(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.TwoMembersMarkedAsInheritanceDiscriminator(p0, p1));
        }

        internal static Exception MappingOfInterfacesMemberIsNotSupported(object p0, object p1)
        {
            return (Exception)new NotSupportedException(Strings.MappingOfInterfacesMemberIsNotSupported(p0, p1));
        }
        internal static Exception UnmappedClassMember(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.UnmappedClassMember(p0, p1));
        }
        internal static Exception RequiredColumnDoesNotExist(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.RequiredColumnDoesNotExist(p0));
        }
        internal static Exception GeneralCollectionMaterializationNotSupported()
        {
            return (Exception)new NotSupportedException(Strings.GeneralCollectionMaterializationNotSupported);
        }
        internal static Exception CannotConvertToEntityRef(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CannotConvertToEntityRef(p0));
        }
        internal static Exception ExpressionNotDeferredQuerySource()
        {
            return (Exception)new InvalidOperationException(Strings.ExpressionNotDeferredQuerySource);
        }

        internal static Exception CannotAssignToMember(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CannotAssignToMember(p0));
        }
        internal static Exception DeferredMemberWrongType()
        {
            return (Exception)new InvalidOperationException(Strings.DeferredMemberWrongType);
        }
        internal static Exception CannotMaterializeList(object p0)
        {
            return (Exception)new NotSupportedException(Strings.CannotMaterializeList(p0));
        }
        internal static Exception CouldNotTranslateExpressionForReading(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotTranslateExpressionForReading(p0));
        }
        internal static Exception UnhandledStringTypeComparison()
        {
            return (Exception)new NotSupportedException(Strings.UnhandledStringTypeComparison);
        }
        internal static Exception InvalidReferenceToRemovedAliasDuringDeflation()
        {
            return (Exception)new InvalidOperationException(Strings.InvalidReferenceToRemovedAliasDuringDeflation);
        }
        internal static Exception ColumnIsNotAccessibleThroughGroupBy(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.ColumnIsNotAccessibleThroughGroupBy(p0));
        }
        internal static Exception ColumnIsNotAccessibleThroughDistinct(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.ColumnIsNotAccessibleThroughDistinct(p0));
        }
        internal static Exception UnexpectedFloatingColumn()
        {
            return (Exception)new InvalidOperationException(Strings.UnexpectedFloatingColumn);
        }
        internal static Exception ExpectedBitFoundPredicate()
        {
            return (Exception)new ArgumentException(Strings.ExpectedBitFoundPredicate);
        }
        internal static Exception ExpectedPredicateFoundBit()
        {
            return (Exception)new ArgumentException(Strings.ExpectedPredicateFoundBit);
        }
        internal static Exception InvalidGroupByExpression()
        {
            return (Exception)new NotSupportedException(Strings.InvalidGroupByExpression);
        }
        internal static Exception InvalidGroupByExpressionType(object p0)
        {
            return (Exception)new NotSupportedException(Strings.InvalidGroupByExpressionType(p0));
        }
        internal static Exception InvalidOrderByExpression(object p0)
        {
            return (Exception)new NotSupportedException(Strings.InvalidOrderByExpression(p0));
        }
        internal static Exception UnsafeStringConversion(object p0, object p1)
        {
            return (Exception)new FormatException(Strings.UnsafeStringConversion(p0, p1));
        }
        internal static Exception ConvertToCharFromBoolNotSupported()
        {
            return (Exception)new NotSupportedException(Strings.ConvertToCharFromBoolNotSupported);
        }
        internal static Exception MemberAccessIllegal(object p0, object p1, object p2)
        {
            return (Exception)new InvalidOperationException(Strings.MemberAccessIllegal(p0, p1, p2));
        }
        internal static Exception TransactionDoesNotMatchConnection()
        {
            return (Exception)new InvalidOperationException(Strings.TransactionDoesNotMatchConnection);
        }

        internal static Exception CompiledQueryAgainstMultipleShapesNotSupported()
        {
            return (Exception)new NotSupportedException(Strings.CompiledQueryAgainstMultipleShapesNotSupported);
        }
        internal static Exception ArgumentTypeMismatch(object p0)
        {
            return (Exception)new ArgumentException(Strings.ArgumentTypeMismatch(p0));
        }
        internal static Exception CannotEnumerateResultsMoreThanOnce()
        {
            return (Exception)new InvalidOperationException(Strings.CannotEnumerateResultsMoreThanOnce);
        }
        internal static Exception UnexpectedSharedExpressionReference()
        {
            return (Exception)new InvalidOperationException(Strings.UnexpectedSharedExpressionReference);
        }
        internal static Exception UnexpectedSharedExpression()
        {
            return (Exception)new InvalidOperationException(Strings.UnexpectedSharedExpression);
        }
        internal static Exception IifReturnTypesMustBeEqual(object p0, object p1)
        {
            return (Exception)new NotSupportedException(Strings.IifReturnTypesMustBeEqual(p0, p1));
        }
        internal static Exception InvalidUseOfGenericMethodAsMappedFunction(object p0)
        {
            return (Exception)new NotSupportedException(Strings.InvalidUseOfGenericMethodAsMappedFunction(p0));
        }
        internal static Exception QueryOnLocalCollectionNotSupported()
        {
            return (Exception)new NotSupportedException(Strings.QueryOnLocalCollectionNotSupported);
        }
        internal static Exception SequenceOperatorsNotSupportedForType(object p0)
        {
            return (Exception)new NotSupportedException(Strings.SequenceOperatorsNotSupportedForType(p0));
        }
        internal static Exception DidNotExpectTypeChange(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.DidNotExpectTypeChange(p0, p1));
        }
        internal static Exception UnmappedDataMember(object p0, object p1, object p2)
        {
            return (Exception)new InvalidOperationException(Strings.UnmappedDataMember(p0, p1, p2));
        }
        internal static Exception MemberNotPartOfProjection(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.MemberNotPartOfProjection(p0, p1));
        }

        internal static Exception ExpectedClrTypesToAgree(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.ExpectedClrTypesToAgree(p0, p1));
        }
        internal static Exception DidNotExpectTypeBinding()
        {
            return (Exception)new InvalidOperationException(Strings.DidNotExpectTypeBinding);
        }
        internal static Exception UnexpectedNode(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.UnexpectedNode(p0));
        }
        internal static Exception ComparisonNotSupportedForType(object p0)
        {
            return (Exception)new NotSupportedException(Strings.ComparisonNotSupportedForType(p0));
        }
        internal static Exception UnionDifferentMemberOrder()
        {
            return (Exception)new NotSupportedException(Strings.UnionDifferentMemberOrder);
        }
        internal static Exception UnionDifferentMembers()
        {
            return (Exception)new NotSupportedException(Strings.UnionDifferentMembers);
        }
        internal static Exception UnionWithHierarchy()
        {
            return (Exception)new NotSupportedException(Strings.UnionWithHierarchy);
        }

        internal static Exception UnionIncompatibleConstruction()
        {
            return (Exception)new NotSupportedException(Strings.UnionIncompatibleConstruction);
        }
        internal static Exception CouldNotHandleAliasRef(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotHandleAliasRef(p0));
        }
        internal static Exception MemberCannotBeTranslated(object p0, object p1)
        {
            return (Exception)new NotSupportedException(Strings.MemberCannotBeTranslated(p0, p1));
        }
        internal static Exception MathRoundNotSupported()
        {
            return (Exception)new NotSupportedException(Strings.MathRoundNotSupported);
        }

        internal static Exception IndexOfWithStringComparisonArgNotSupported()
        {
            return (Exception)new NotSupportedException(Strings.IndexOfWithStringComparisonArgNotSupported);
        }
        internal static Exception VbLikeDoesNotSupportMultipleCharacterRanges()
        {
            return (Exception)new ArgumentException(Strings.VbLikeDoesNotSupportMultipleCharacterRanges);
        }

        internal static Exception VbLikeUnclosedBracket()
        {
            return (Exception)new ArgumentException(Strings.VbLikeUnclosedBracket);
        }
        internal static Exception LastIndexOfWithStringComparisonArgNotSupported()
        {
            return (Exception)new NotSupportedException(Strings.LastIndexOfWithStringComparisonArgNotSupported);
        }
        internal static Exception ConvertToDateTimeOnlyForDateTimeOrString()
        {
            return (Exception)new NotSupportedException(Strings.ConvertToDateTimeOnlyForDateTimeOrString);
        }
        internal static Exception MethodHasNoSupportConversionToSql(object p0)
        {
            return (Exception)new NotSupportedException(Strings.MethodHasNoSupportConversionToSql(p0));
        }
        internal static Exception ToStringOnlySupportedForPrimitiveTypes()
        {
            return (Exception)new NotSupportedException(Strings.ToStringOnlySupportedForPrimitiveTypes);
        }
        internal static Exception UnsupportedTimeSpanConstructorForm()
        {
            return (Exception)new NotSupportedException(Strings.UnsupportedTimeSpanConstructorForm);
        }
        internal static Exception UnsupportedDateTimeOffsetConstructorForm()
        {
            return (Exception)new NotSupportedException(Strings.UnsupportedDateTimeOffsetConstructorForm);
        }
        internal static Exception UnsupportedDateTimeConstructorForm()
        {
            return (Exception)new NotSupportedException(Strings.UnsupportedDateTimeConstructorForm);
        }
        internal static Exception UnsupportedStringConstructorForm()
        {
            return (Exception)new NotSupportedException(Strings.UnsupportedStringConstructorForm);
        }
        internal static Exception SqlMethodOnlyForSql(object p0)
        {
            return (Exception)new NotSupportedException(Strings.SqlMethodOnlyForSql(p0));
        }
        internal static Exception ClassLiteralsNotAllowed(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.ClassLiteralsNotAllowed(p0));
        }
        internal static Exception ColumnClrTypeDoesNotAgreeWithExpressionsClrType()
        {
            return (Exception)new InvalidOperationException(Strings.ColumnClrTypeDoesNotAgreeWithExpressionsClrType);
        }
        internal static Exception UpdateItemMustBeConstant()
        {
            return (Exception)new NotSupportedException(Strings.UpdateItemMustBeConstant);
        }
        internal static Exception InvalidDbGeneratedType(object p0)
        {
            return (Exception)new NotSupportedException(Strings.InvalidDbGeneratedType(p0));
        }
        internal static Exception InsertItemMustBeConstant()
        {
            return (Exception)new NotSupportedException(Strings.InsertItemMustBeConstant);
        }
        internal static Exception QueryOperatorNotSupported(object p0)
        {
            return (Exception)new NotSupportedException(Strings.QueryOperatorNotSupported(p0));
        }
        internal static Exception QueryOperatorOverloadNotSupported(object p0)
        {
            return (Exception)new NotSupportedException(Strings.QueryOperatorOverloadNotSupported(p0));
        }
        internal static Exception InvalidSequenceOperatorCall(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.InvalidSequenceOperatorCall(p0));
        }
        internal static Exception NonConstantExpressionsNotSupportedFor(object p0)
        {
            return (Exception)new NotSupportedException(Strings.NonConstantExpressionsNotSupportedFor(p0));
        }
        internal static Exception NonConstantExpressionsNotSupportedForRounding()
        {
            return (Exception)new NotSupportedException(Strings.NonConstantExpressionsNotSupportedForRounding);
        }
        internal static Exception MethodFormHasNoSupportConversionToSql(object p0, object p1)
        {
            return (Exception)new NotSupportedException(Strings.MethodFormHasNoSupportConversionToSql(p0, p1));
        }
        internal static Exception NonCountAggregateFunctionsAreNotValidOnProjections(object p0)
        {
            return (Exception)new NotSupportedException(Strings.NonCountAggregateFunctionsAreNotValidOnProjections(p0));
        }
        internal static Exception CannotAggregateType(object p0)
        {
            return (Exception)new NotSupportedException(Strings.CannotAggregateType(p0));
        }
        internal static Exception TypeCannotBeOrdered(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.TypeCannotBeOrdered(p0));
        }
        internal static Exception GroupingNotSupportedAsOrderCriterion()
        {
            return (Exception)new NotSupportedException(Strings.GroupingNotSupportedAsOrderCriterion);
        }

        internal static Exception ExceptNotSupportedForHierarchicalTypes()
        {
            return (Exception)new NotSupportedException(Strings.ExceptNotSupportedForHierarchicalTypes);
        }
        internal static Exception IntersectNotSupportedForHierarchicalTypes()
        {
            return (Exception)new NotSupportedException(Strings.IntersectNotSupportedForHierarchicalTypes);
        }
        internal static Exception BinaryOperatorNotRecognized(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.BinaryOperatorNotRecognized(p0));
        }
        internal static Exception InvalidReturnFromSproc(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.InvalidReturnFromSproc(p0));
        }
        internal static Exception SprocsCannotBeComposed()
        {
            return (Exception)new InvalidOperationException(Strings.SprocsCannotBeComposed);
        }
        internal static Exception ParameterNotInScope(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.ParameterNotInScope(p0));
        }
        internal static Exception SkipRequiresSingleTableQueryWithPKs()
        {
            return (Exception)new NotSupportedException(Strings.SkipRequiresSingleTableQueryWithPKs);
        }
        internal static Exception SkipNotSupportedForSequenceTypes()
        {
            return (Exception)new NotSupportedException(Strings.SkipNotSupportedForSequenceTypes);
        }
        internal static Exception CouldNotConvertToPropertyOrField(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotConvertToPropertyOrField(p0));
        }
        internal static Exception CannotMaterializeEntityType(object p0)
        {
            return (Exception)new NotSupportedException(Strings.CannotMaterializeEntityType(p0));
        }
        internal static Exception WrongDataContext()
        {
            return (Exception)new InvalidOperationException(Strings.WrongDataContext);
        }
        internal static Exception DidNotExpectAs(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.DidNotExpectAs(p0));
        }
        internal static Exception TypeBinaryOperatorNotRecognized()
        {
            return (Exception)new InvalidOperationException(Strings.TypeBinaryOperatorNotRecognized);
        }
        internal static Exception ColumnCannotReferToItself()
        {
            return (Exception)new InvalidOperationException(Strings.ColumnCannotReferToItself);
        }
        internal static Exception ArgumentWrongType(object p0, object p1, object p2)
        {
            return (Exception)new ArgumentException(Strings.ArgumentWrongType(p0, p1, p2));
        }
        internal static Exception ArgumentWrongValue(object p0)
        {
            return (Exception)new ArgumentException(Strings.ArgumentWrongValue(p0));
        }
        internal static Exception BadProjectionInSelect()
        {
            return (Exception)new InvalidOperationException(Strings.BadProjectionInSelect);
        }
        internal static Exception BadParameterType(object p0)
        {
            return (Exception)new NotSupportedException(Strings.BadParameterType(p0));
        }
        internal static Exception ParametersCannotBeSequences()
        {
            return (Exception)new NotSupportedException(Strings.ParametersCannotBeSequences);
        }
        internal static Exception ConstructedArraysNotSupported()
        {
            return (Exception)new NotSupportedException(Strings.ConstructedArraysNotSupported);
        }
        internal static Exception IQueryableCannotReturnSelfReferencingConstantExpression()
        {
            return (Exception)new NotSupportedException(Strings.IQueryableCannotReturnSelfReferencingConstantExpression);
        }
        internal static Exception CapturedValuesCannotBeSequences()
        {
            return (Exception)new NotSupportedException(Strings.CapturedValuesCannotBeSequences);
        }
        internal static Exception UnrecognizedExpressionNode(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.UnrecognizedExpressionNode(p0));
        }
        internal static Exception UnsupportedNodeType(object p0)
        {
            return (Exception)new NotSupportedException(Strings.UnsupportedNodeType(p0));
        }
        internal static Exception InvalidMethodExecution(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.InvalidMethodExecution(p0));
        }
        internal static Exception ExpressionNotSupportedForSqlServerVersion(Collection<string> reasons)
        {
            StringBuilder stringBuilder = new StringBuilder(Strings.CannotTranslateExpressionToSql);
            foreach (string str in reasons)
                stringBuilder.AppendLine(str);
            return (Exception)new NotSupportedException(stringBuilder.ToString());
        }
        internal static Exception CouldNotDetermineSqlType(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotDetermineSqlType(p0));
        }
        internal static Exception CouldNotDetermineDbGeneratedSqlType(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotDetermineDbGeneratedSqlType(p0));
        }
        internal static Exception CreateDatabaseFailedBecauseOfClassWithNoMembers(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CreateDatabaseFailedBecauseOfClassWithNoMembers(p0));
        }
        internal static Exception CreateDatabaseFailedBecauseOfContextWithNoTables(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CreateDatabaseFailedBecauseOfContextWithNoTables(p0));
        }
        internal static Exception CouldNotDetermineCatalogName()
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotDetermineCatalogName);
        }
        internal static Exception CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(p0));
        }
        internal static Exception ProviderCannotBeUsedAfterDispose()
        {
            return (Exception)new ObjectDisposedException(Strings.ProviderCannotBeUsedAfterDispose);
        }
        internal static Exception DatabaseDeleteThroughContext()
        {
            return (Exception)new InvalidOperationException(Strings.DatabaseDeleteThroughContext);
        }
        internal static Exception ContextNotInitialized()
        {
            return (Exception)new InvalidOperationException(Strings.ContextNotInitialized);
        }
        internal static Exception InvalidConnectionArgument(object p0)
        {
            return (Exception)new ArgumentException(Strings.InvalidConnectionArgument(p0));
        }
        internal static Exception ProviderNotInstalled(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.ProviderNotInstalled(p0, p1));
        }
        internal static Exception CannotAddChangeConflicts()
        {
            return (Exception)new NotSupportedException(Strings.CannotAddChangeConflicts);
        }

        internal static Exception CannotRemoveChangeConflicts()
        {
            return (Exception)new NotSupportedException(Strings.CannotRemoveChangeConflicts);
        }

        internal static Exception InconsistentAssociationAndKeyChange(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.InconsistentAssociationAndKeyChange(p0, p1));
        }

        internal static Exception UnableToDetermineDataContext()
        {
            return (Exception)new InvalidOperationException(Strings.UnableToDetermineDataContext);
        }

        internal static Exception ArgumentTypeHasNoIdentityKey(object p0)
        {
            return (Exception)new ArgumentException(Strings.ArgumentTypeHasNoIdentityKey(p0));
        }

        internal static Exception CouldNotConvert(object p0, object p1)
        {
            return (Exception)new InvalidCastException(Strings.CouldNotConvert(p0, p1));
        }

        internal static Exception CannotRemoveUnattachedEntity()
        {
            return (Exception)new InvalidOperationException(Strings.CannotRemoveUnattachedEntity);
        }

        internal static Exception ColumnMappedMoreThanOnce(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.ColumnMappedMoreThanOnce(p0));
        }

        internal static Exception CouldNotAttach()
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotAttach);
        }

        internal static Exception CouldNotGetTableForSubtype(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotGetTableForSubtype(p0, p1));
        }

        internal static Exception CouldNotRemoveRelationshipBecauseOneSideCannotBeNull(object p0, object p1, object p2)
        {
            return (Exception)new InvalidOperationException(Strings.CouldNotRemoveRelationshipBecauseOneSideCannotBeNull(p0, p1, p2));
        }

        internal static Exception EntitySetAlreadyLoaded()
        {
            return (Exception)new InvalidOperationException(Strings.EntitySetAlreadyLoaded);
        }

        internal static Exception EntitySetModifiedDuringEnumeration()
        {
            return (Exception)new InvalidOperationException(Strings.EntitySetModifiedDuringEnumeration);
        }

        internal static Exception ExpectedQueryableArgument(object p0, object p1)
        {
            return (Exception)new ArgumentException(Strings.ExpectedQueryableArgument(p0, p1));
        }

        internal static Exception ExpectedUpdateDeleteOrChange()
        {
            return (Exception)new InvalidOperationException(Strings.ExpectedUpdateDeleteOrChange);
        }

        internal static Exception KeyIsWrongSize(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.KeyIsWrongSize(p0, p1));
        }

        internal static Exception KeyValueIsWrongType(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.KeyValueIsWrongType(p0, p1));
        }

        internal static Exception IdentityChangeNotAllowed(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.IdentityChangeNotAllowed(p0, p1));
        }

        internal static Exception DbGeneratedChangeNotAllowed(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.DbGeneratedChangeNotAllowed(p0, p1));
        }

        internal static Exception ModifyDuringAddOrRemove()
        {
            return (Exception)new ArgumentException(Strings.ModifyDuringAddOrRemove);
        }

        internal static Exception ProviderDoesNotImplementRequiredInterface(object p0, object p1)
        {
            return (Exception)new InvalidOperationException(Strings.ProviderDoesNotImplementRequiredInterface(p0, p1));
        }

        internal static Exception ProviderTypeNull()
        {
            return (Exception)new InvalidOperationException(Strings.ProviderTypeNull);
        }

        internal static Exception TypeCouldNotBeAdded(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.TypeCouldNotBeAdded(p0));
        }

        internal static Exception TypeCouldNotBeRemoved(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.TypeCouldNotBeRemoved(p0));
        }

        internal static Exception TypeCouldNotBeTracked(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.TypeCouldNotBeTracked(p0));
        }

        internal static Exception TypeIsNotEntity(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.TypeIsNotEntity(p0));
        }

        internal static Exception UnrecognizedRefreshObject()
        {
            return (Exception)new ArgumentException(Strings.UnrecognizedRefreshObject);
        }

        internal static Exception UnhandledExpressionType(object p0)
        {
            return (Exception)new ArgumentException(Strings.UnhandledExpressionType(p0));
        }

        internal static Exception UnhandledBindingType(object p0)
        {
            return (Exception)new ArgumentException(Strings.UnhandledBindingType(p0));
        }

        internal static Exception ObjectTrackingRequired()
        {
            return (Exception)new InvalidOperationException(Strings.ObjectTrackingRequired);
        }

        internal static Exception OptionsCannotBeModifiedAfterQuery()
        {
            return (Exception)new InvalidOperationException(Strings.OptionsCannotBeModifiedAfterQuery);
        }

        internal static Exception DeferredLoadingRequiresObjectTracking()
        {
            return (Exception)new InvalidOperationException(Strings.DeferredLoadingRequiresObjectTracking);
        }

        internal static Exception SubqueryDoesNotSupportOperator(object p0)
        {
            return (Exception)new NotSupportedException(Strings.SubqueryDoesNotSupportOperator(p0));
        }

        internal static Exception SubqueryNotSupportedOn(object p0)
        {
            return (Exception)new NotSupportedException(Strings.SubqueryNotSupportedOn(p0));
        }

        internal static Exception SubqueryNotSupportedOnType(object p0, object p1)
        {
            return (Exception)new NotSupportedException(Strings.SubqueryNotSupportedOnType(p0, p1));
        }

        internal static Exception SubqueryNotAllowedAfterFreeze()
        {
            return (Exception)new InvalidOperationException(Strings.SubqueryNotAllowedAfterFreeze);
        }

        internal static Exception IncludeNotAllowedAfterFreeze()
        {
            return (Exception)new InvalidOperationException(Strings.IncludeNotAllowedAfterFreeze);
        }

        internal static Exception LoadOptionsChangeNotAllowedAfterQuery()
        {
            return (Exception)new InvalidOperationException(Strings.LoadOptionsChangeNotAllowedAfterQuery);
        }

        internal static Exception IncludeCycleNotAllowed()
        {
            return (Exception)new InvalidOperationException(Strings.IncludeCycleNotAllowed);
        }

        internal static Exception SubqueryMustBeSequence()
        {
            return (Exception)new InvalidOperationException(Strings.SubqueryMustBeSequence);
        }

        internal static Exception RefreshOfDeletedObject()
        {
            return (Exception)new InvalidOperationException(Strings.RefreshOfDeletedObject);
        }

        internal static Exception RefreshOfNewObject()
        {
            return (Exception)new InvalidOperationException(Strings.RefreshOfNewObject);
        }

        internal static Exception CannotChangeInheritanceType(object p0, object p1, object p2, object p3)
        {
            return (Exception)new InvalidOperationException(Strings.CannotChangeInheritanceType(p0, p1, p2, p3));
        }

        internal static Exception DataContextCannotBeUsedAfterDispose()
        {
            return (Exception)new ObjectDisposedException(Strings.DataContextCannotBeUsedAfterDispose);
        }

        internal static Exception TypeIsNotMarkedAsTable(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.TypeIsNotMarkedAsTable(p0));
        }

        internal static Exception NonEntityAssociationMapping(object p0, object p1, object p2)
        {
            return (Exception)new InvalidOperationException(Strings.NonEntityAssociationMapping(p0, p1, p2));
        }

        internal static Exception CannotPerformCUDOnReadOnlyTable(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.CannotPerformCUDOnReadOnlyTable(p0));
        }

        internal static Exception CycleDetected()
        {
            return (Exception)new InvalidOperationException(Strings.CycleDetected);
        }

        internal static Exception CantAddAlreadyExistingItem()
        {
            return (Exception)new InvalidOperationException(Strings.CantAddAlreadyExistingItem);
        }

        internal static Exception InsertAutoSyncFailure()
        {
            return (Exception)new InvalidOperationException(Strings.InsertAutoSyncFailure);
        }

        internal static Exception EntitySetDataBindingWithAbstractBaseClass(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.EntitySetDataBindingWithAbstractBaseClass(p0));
        }

        internal static Exception EntitySetDataBindingWithNonPublicDefaultConstructor(object p0)
        {
            return (Exception)new InvalidOperationException(Strings.EntitySetDataBindingWithNonPublicDefaultConstructor(p0));
        }

        internal static Exception InvalidLoadOptionsLoadMemberSpecification()
        {
            return (Exception)new InvalidOperationException(Strings.InvalidLoadOptionsLoadMemberSpecification);
        }

        internal static Exception EntityIsTheWrongType()
        {
            return (Exception)new InvalidOperationException(Strings.EntityIsTheWrongType);
        }

        internal static Exception OriginalEntityIsWrongType()
        {
            return (Exception)new InvalidOperationException(Strings.OriginalEntityIsWrongType);
        }

        internal static Exception CannotAttachAlreadyExistingEntity()
        {
            return (Exception)new InvalidOperationException(Strings.CannotAttachAlreadyExistingEntity);
        }

        internal static Exception CannotAttachAsModifiedWithoutOriginalState()
        {
            return (Exception)new InvalidOperationException(Strings.CannotAttachAsModifiedWithoutOriginalState);
        }

        internal static Exception CannotPerformOperationDuringSubmitChanges()
        {
            return (Exception)new InvalidOperationException(Strings.CannotPerformOperationDuringSubmitChanges);
        }

        internal static Exception CannotPerformOperationOutsideSubmitChanges()
        {
            return (Exception)new InvalidOperationException(Strings.CannotPerformOperationOutsideSubmitChanges);
        }

        internal static Exception CannotPerformOperationForUntrackedObject()
        {
            return (Exception)new InvalidOperationException(Strings.CannotPerformOperationForUntrackedObject);
        }

        internal static Exception CannotAttachAddNonNewEntities()
        {
            return (Exception)new NotSupportedException(Strings.CannotAttachAddNonNewEntities);
        }

        internal static Exception QueryWasCompiledForDifferentMappingSource()
        {
            return (Exception)new ArgumentException(Strings.QueryWasCompiledForDifferentMappingSource);
        }

        internal static Exception ArgumentNull(string paramName)
        {
            return (Exception)new ArgumentNullException(paramName);
        }

        internal static Exception ArgumentOutOfRange(string paramName)
        {
            return (Exception)new ArgumentOutOfRangeException(paramName);
        }

        internal static Exception NotImplemented()
        {
            return (Exception)new NotImplementedException();
        }

        internal static Exception NotSupported()
        {
            return (Exception)new NotSupportedException();
        }
    }
}
