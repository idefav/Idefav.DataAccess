using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal static class Strings
    {
        internal static string OwningTeam
        {
            get
            {
                return SR.GetString("OwningTeam");
            }
        }

        internal static string VbLikeDoesNotSupportMultipleCharacterRanges
        {
            get
            {
                return SR.GetString("VbLikeDoesNotSupportMultipleCharacterRanges");
            }
        }

        internal static string VbLikeUnclosedBracket
        {
            get
            {
                return SR.GetString("VbLikeUnclosedBracket");
            }
        }

        internal static string ProviderCannotBeUsedAfterDispose
        {
            get
            {
                return SR.GetString("ProviderCannotBeUsedAfterDispose");
            }
        }

        internal static string ContextNotInitialized
        {
            get
            {
                return SR.GetString("ContextNotInitialized");
            }
        }

        internal static string CouldNotDetermineCatalogName
        {
            get
            {
                return SR.GetString("CouldNotDetermineCatalogName");
            }
        }

        internal static string DistributedTransactionsAreNotAllowed
        {
            get
            {
                return SR.GetString("DistributedTransactionsAreNotAllowed");
            }
        }

        internal static string CannotEnumerateResultsMoreThanOnce
        {
            get
            {
                return SR.GetString("CannotEnumerateResultsMoreThanOnce");
            }
        }

        internal static string ToStringOnlySupportedForPrimitiveTypes
        {
            get
            {
                return SR.GetString("ToStringOnlySupportedForPrimitiveTypes");
            }
        }

        internal static string TransactionDoesNotMatchConnection
        {
            get
            {
                return SR.GetString("TransactionDoesNotMatchConnection");
            }
        }

        internal static string UnsupportedDateTimeConstructorForm
        {
            get
            {
                return SR.GetString("UnsupportedDateTimeConstructorForm");
            }
        }

        internal static string UnsupportedDateTimeOffsetConstructorForm
        {
            get
            {
                return SR.GetString("UnsupportedDateTimeOffsetConstructorForm");
            }
        }

        internal static string UnsupportedStringConstructorForm
        {
            get
            {
                return SR.GetString("UnsupportedStringConstructorForm");
            }
        }

        internal static string UnsupportedTimeSpanConstructorForm
        {
            get
            {
                return SR.GetString("UnsupportedTimeSpanConstructorForm");
            }
        }

        internal static string MathRoundNotSupported
        {
            get
            {
                return SR.GetString("MathRoundNotSupported");
            }
        }

        internal static string NonConstantExpressionsNotSupportedForRounding
        {
            get
            {
                return SR.GetString("NonConstantExpressionsNotSupportedForRounding");
            }
        }

        internal static string CompiledQueryAgainstMultipleShapesNotSupported
        {
            get
            {
                return SR.GetString("CompiledQueryAgainstMultipleShapesNotSupported");
            }
        }

        internal static string IndexOfWithStringComparisonArgNotSupported
        {
            get
            {
                return SR.GetString("IndexOfWithStringComparisonArgNotSupported");
            }
        }

        internal static string LastIndexOfWithStringComparisonArgNotSupported
        {
            get
            {
                return SR.GetString("LastIndexOfWithStringComparisonArgNotSupported");
            }
        }

        internal static string ConvertToCharFromBoolNotSupported
        {
            get
            {
                return SR.GetString("ConvertToCharFromBoolNotSupported");
            }
        }

        internal static string ConvertToDateTimeOnlyForDateTimeOrString
        {
            get
            {
                return SR.GetString("ConvertToDateTimeOnlyForDateTimeOrString");
            }
        }

        internal static string CannotTranslateExpressionToSql
        {
            get
            {
                return SR.GetString("CannotTranslateExpressionToSql");
            }
        }

        internal static string SkipIsValidOnlyOverOrderedQueries
        {
            get
            {
                return SR.GetString("SkipIsValidOnlyOverOrderedQueries");
            }
        }

        internal static string SkipRequiresSingleTableQueryWithPKs
        {
            get
            {
                return SR.GetString("SkipRequiresSingleTableQueryWithPKs");
            }
        }

        internal static string ExpressionNotDeferredQuerySource
        {
            get
            {
                return SR.GetString("ExpressionNotDeferredQuerySource");
            }
        }

        internal static string DeferredMemberWrongType
        {
            get
            {
                return SR.GetString("DeferredMemberWrongType");
            }
        }

        internal static string BadProjectionInSelect
        {
            get
            {
                return SR.GetString("BadProjectionInSelect");
            }
        }

        internal static string WrongDataContext
        {
            get
            {
                return SR.GetString("WrongDataContext");
            }
        }

        internal static string CannotCompareItemsAssociatedWithDifferentTable
        {
            get
            {
                return SR.GetString("CannotCompareItemsAssociatedWithDifferentTable");
            }
        }

        internal static string ColumnCannotReferToItself
        {
            get
            {
                return SR.GetString("ColumnCannotReferToItself");
            }
        }

        internal static string ColumnClrTypeDoesNotAgreeWithExpressionsClrType
        {
            get
            {
                return SR.GetString("ColumnClrTypeDoesNotAgreeWithExpressionsClrType");
            }
        }

        internal static string ConstructedArraysNotSupported
        {
            get
            {
                return SR.GetString("ConstructedArraysNotSupported");
            }
        }

        internal static string ParametersCannotBeSequences
        {
            get
            {
                return SR.GetString("ParametersCannotBeSequences");
            }
        }

        internal static string CapturedValuesCannotBeSequences
        {
            get
            {
                return SR.GetString("CapturedValuesCannotBeSequences");
            }
        }

        internal static string IQueryableCannotReturnSelfReferencingConstantExpression
        {
            get
            {
                return SR.GetString("IQueryableCannotReturnSelfReferencingConstantExpression");
            }
        }

        internal static string CouldNotGetClrType
        {
            get
            {
                return SR.GetString("CouldNotGetClrType");
            }
        }

        internal static string CouldNotGetSqlType
        {
            get
            {
                return SR.GetString("CouldNotGetSqlType");
            }
        }

        internal static string DidNotExpectTypeBinding
        {
            get
            {
                return SR.GetString("DidNotExpectTypeBinding");
            }
        }

        internal static string EmptyCaseNotSupported
        {
            get
            {
                return SR.GetString("EmptyCaseNotSupported");
            }
        }

        internal static string ExpectedNoObjectType
        {
            get
            {
                return SR.GetString("ExpectedNoObjectType");
            }
        }

        internal static string ExpectedBitFoundPredicate
        {
            get
            {
                return SR.GetString("ExpectedBitFoundPredicate");
            }
        }

        internal static string ExpectedPredicateFoundBit
        {
            get
            {
                return SR.GetString("ExpectedPredicateFoundBit");
            }
        }

        internal static string InvalidGroupByExpression
        {
            get
            {
                return SR.GetString("InvalidGroupByExpression");
            }
        }

        internal static string Impossible
        {
            get
            {
                return SR.GetString("Impossible");
            }
        }

        internal static string InfiniteDescent
        {
            get
            {
                return SR.GetString("InfiniteDescent");
            }
        }

        internal static string InvalidReferenceToRemovedAliasDuringDeflation
        {
            get
            {
                return SR.GetString("InvalidReferenceToRemovedAliasDuringDeflation");
            }
        }

        internal static string ReaderUsedAfterDispose
        {
            get
            {
                return SR.GetString("ReaderUsedAfterDispose");
            }
        }

        internal static string TypeBinaryOperatorNotRecognized
        {
            get
            {
                return SR.GetString("TypeBinaryOperatorNotRecognized");
            }
        }

        internal static string UnexpectedFloatingColumn
        {
            get
            {
                return SR.GetString("UnexpectedFloatingColumn");
            }
        }

        internal static string UnexpectedSharedExpression
        {
            get
            {
                return SR.GetString("UnexpectedSharedExpression");
            }
        }

        internal static string UnexpectedSharedExpressionReference
        {
            get
            {
                return SR.GetString("UnexpectedSharedExpressionReference");
            }
        }

        internal static string UnhandledStringTypeComparison
        {
            get
            {
                return SR.GetString("UnhandledStringTypeComparison");
            }
        }

        internal static string UnionIncompatibleConstruction
        {
            get
            {
                return SR.GetString("UnionIncompatibleConstruction");
            }
        }

        internal static string UnionDifferentMembers
        {
            get
            {
                return SR.GetString("UnionDifferentMembers");
            }
        }

        internal static string UnionDifferentMemberOrder
        {
            get
            {
                return SR.GetString("UnionDifferentMemberOrder");
            }
        }

        internal static string UnionOfIncompatibleDynamicTypes
        {
            get
            {
                return SR.GetString("UnionOfIncompatibleDynamicTypes");
            }
        }

        internal static string UnionWithHierarchy
        {
            get
            {
                return SR.GetString("UnionWithHierarchy");
            }
        }

        internal static string IntersectNotSupportedForHierarchicalTypes
        {
            get
            {
                return SR.GetString("IntersectNotSupportedForHierarchicalTypes");
            }
        }

        internal static string ExceptNotSupportedForHierarchicalTypes
        {
            get
            {
                return SR.GetString("ExceptNotSupportedForHierarchicalTypes");
            }
        }

        internal static string GroupingNotSupportedAsOrderCriterion
        {
            get
            {
                return SR.GetString("GroupingNotSupportedAsOrderCriterion");
            }
        }

        internal static string SelectManyDoesNotSupportStrings
        {
            get
            {
                return SR.GetString("SelectManyDoesNotSupportStrings");
            }
        }

        internal static string SkipNotSupportedForSequenceTypes
        {
            get
            {
                return SR.GetString("SkipNotSupportedForSequenceTypes");
            }
        }

        internal static string QueryOnLocalCollectionNotSupported
        {
            get
            {
                return SR.GetString("QueryOnLocalCollectionNotSupported");
            }
        }

        internal static string TypeColumnWithUnhandledSource
        {
            get
            {
                return SR.GetString("TypeColumnWithUnhandledSource");
            }
        }

        internal static string GeneralCollectionMaterializationNotSupported
        {
            get
            {
                return SR.GetString("GeneralCollectionMaterializationNotSupported");
            }
        }

        internal static string SprocsCannotBeComposed
        {
            get
            {
                return SR.GetString("SprocsCannotBeComposed");
            }
        }

        internal static string InsertItemMustBeConstant
        {
            get
            {
                return SR.GetString("InsertItemMustBeConstant");
            }
        }

        internal static string UpdateItemMustBeConstant
        {
            get
            {
                return SR.GetString("UpdateItemMustBeConstant");
            }
        }

        internal static string DatabaseDeleteThroughContext
        {
            get
            {
                return SR.GetString("DatabaseDeleteThroughContext");
            }
        }

        internal static string UnrecognizedProviderMode(object p0)
        {
            string name = "UnrecognizedProviderMode";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CompiledQueryCannotReturnType(object p0)
        {
            string name = "CompiledQueryCannotReturnType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ArgumentEmpty(object p0)
        {
            string name = "ArgumentEmpty";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ArgumentTypeMismatch(object p0)
        {
            string name = "ArgumentTypeMismatch";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CouldNotDetermineSqlType(object p0)
        {
            string name = "CouldNotDetermineSqlType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CouldNotDetermineDbGeneratedSqlType(object p0)
        {
            string name = "CouldNotDetermineDbGeneratedSqlType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CreateDatabaseFailedBecauseOfClassWithNoMembers(object p0)
        {
            string name = "CreateDatabaseFailedBecauseOfClassWithNoMembers";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CreateDatabaseFailedBecauseOfContextWithNoTables(object p0)
        {
            string name = "CreateDatabaseFailedBecauseOfContextWithNoTables";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(object p0)
        {
            string name = "CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string InvalidConnectionArgument(object p0)
        {
            string name = "InvalidConnectionArgument";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string IifReturnTypesMustBeEqual(object p0, object p1)
        {
            string name = "IifReturnTypesMustBeEqual";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string MethodNotMappedToStoredProcedure(object p0)
        {
            string name = "MethodNotMappedToStoredProcedure";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ResultTypeNotMappedToFunction(object p0, object p1)
        {
            string name = "ResultTypeNotMappedToFunction";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string UnexpectedTypeCode(object p0)
        {
            string name = "UnexpectedTypeCode";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string UnsupportedTypeConstructorForm(object p0)
        {
            string name = "UnsupportedTypeConstructorForm";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string WrongNumberOfValuesInCollectionArgument(object p0, object p1, object p2)
        {
            string name = "WrongNumberOfValuesInCollectionArgument";
            object[] objArray = new object[3];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            int index3 = 2;
            object obj3 = p2;
            objArray[index3] = obj3;
            return SR.GetString(name, objArray);
        }

        internal static string LogGeneralInfoMessage(object p0, object p1)
        {
            string name = "LogGeneralInfoMessage";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string LogAttemptingToDeleteDatabase(object p0)
        {
            string name = "LogAttemptingToDeleteDatabase";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string LogStoredProcedureExecution(object p0, object p1)
        {
            string name = "LogStoredProcedureExecution";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string MemberCannotBeTranslated(object p0, object p1)
        {
            string name = "MemberCannotBeTranslated";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string NonConstantExpressionsNotSupportedFor(object p0)
        {
            string name = "NonConstantExpressionsNotSupportedFor";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string SqlMethodOnlyForSql(object p0)
        {
            string name = "SqlMethodOnlyForSql";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string LenOfTextOrNTextNotSupported(object p0)
        {
            string name = "LenOfTextOrNTextNotSupported";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string TextNTextAndImageCannotOccurInDistinct(object p0)
        {
            string name = "TextNTextAndImageCannotOccurInDistinct";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string TextNTextAndImageCannotOccurInUnion(object p0)
        {
            string name = "TextNTextAndImageCannotOccurInUnion";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string MaxSizeNotSupported(object p0)
        {
            string name = "MaxSizeNotSupported";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string NoMethodInTypeMatchingArguments(object p0)
        {
            string name = "NoMethodInTypeMatchingArguments";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CannotConvertToEntityRef(object p0)
        {
            string name = "CannotConvertToEntityRef";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ArgumentWrongType(object p0, object p1, object p2)
        {
            string name = "ArgumentWrongType";
            object[] objArray = new object[3];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            int index3 = 2;
            object obj3 = p2;
            objArray[index3] = obj3;
            return SR.GetString(name, objArray);
        }

        internal static string ArgumentWrongValue(object p0)
        {
            string name = "ArgumentWrongValue";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string InvalidReturnFromSproc(object p0)
        {
            string name = "InvalidReturnFromSproc";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string BinaryOperatorNotRecognized(object p0)
        {
            string name = "BinaryOperatorNotRecognized";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CannotAggregateType(object p0)
        {
            string name = "CannotAggregateType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CannotDeleteTypesOf(object p0)
        {
            string name = "CannotDeleteTypesOf";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ClassLiteralsNotAllowed(object p0)
        {
            string name = "ClassLiteralsNotAllowed";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ClientCaseShouldNotHold(object p0)
        {
            string name = "ClientCaseShouldNotHold";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ClrBoolDoesNotAgreeWithSqlType(object p0)
        {
            string name = "ClrBoolDoesNotAgreeWithSqlType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ColumnIsDefinedInMultiplePlaces(object p0)
        {
            string name = "ColumnIsDefinedInMultiplePlaces";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ColumnIsNotAccessibleThroughGroupBy(object p0)
        {
            string name = "ColumnIsNotAccessibleThroughGroupBy";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ColumnIsNotAccessibleThroughDistinct(object p0)
        {
            string name = "ColumnIsNotAccessibleThroughDistinct";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ColumnReferencedIsNotInScope(object p0)
        {
            string name = "ColumnReferencedIsNotInScope";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CouldNotAssignSequence(object p0, object p1)
        {
            string name = "CouldNotAssignSequence";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string CouldNotTranslateExpressionForReading(object p0)
        {
            string name = "CouldNotTranslateExpressionForReading";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CouldNotHandleAliasRef(object p0)
        {
            string name = "CouldNotHandleAliasRef";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string DidNotExpectAs(object p0)
        {
            string name = "DidNotExpectAs";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string DidNotExpectTypeChange(object p0, object p1)
        {
            string name = "DidNotExpectTypeChange";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string ExpectedClrTypesToAgree(object p0, object p1)
        {
            string name = "ExpectedClrTypesToAgree";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string ExpectedQueryableArgument(object p0, object p1, object p2)
        {
            string name = "ExpectedQueryableArgument";
            object[] objArray = new object[3];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            int index3 = 2;
            object obj3 = p2;
            objArray[index3] = obj3;
            return SR.GetString(name, objArray);
        }

        internal static string InvalidGroupByExpressionType(object p0)
        {
            string name = "InvalidGroupByExpressionType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string InvalidOrderByExpression(object p0)
        {
            string name = "InvalidOrderByExpression";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string InvalidFormatNode(object p0)
        {
            string name = "InvalidFormatNode";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string InvalidSequenceOperatorCall(object p0)
        {
            string name = "InvalidSequenceOperatorCall";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ParameterNotInScope(object p0)
        {
            string name = "ParameterNotInScope";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string MemberAccessIllegal(object p0, object p1, object p2)
        {
            string name = "MemberAccessIllegal";
            object[] objArray = new object[3];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            int index3 = 2;
            object obj3 = p2;
            objArray[index3] = obj3;
            return SR.GetString(name, objArray);
        }

        internal static string MemberCouldNotBeTranslated(object p0, object p1)
        {
            string name = "MemberCouldNotBeTranslated";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string MemberNotPartOfProjection(object p0, object p1)
        {
            string name = "MemberNotPartOfProjection";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string MethodHasNoSupportConversionToSql(object p0)
        {
            string name = "MethodHasNoSupportConversionToSql";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string MethodFormHasNoSupportConversionToSql(object p0, object p1)
        {
            string name = "MethodFormHasNoSupportConversionToSql";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string UnableToBindUnmappedMember(object p0, object p1, object p2)
        {
            string name = "UnableToBindUnmappedMember";
            object[] objArray = new object[3];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            int index3 = 2;
            object obj3 = p2;
            objArray[index3] = obj3;
            return SR.GetString(name, objArray);
        }

        internal static string QueryOperatorNotSupported(object p0)
        {
            string name = "QueryOperatorNotSupported";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string QueryOperatorOverloadNotSupported(object p0)
        {
            string name = "QueryOperatorOverloadNotSupported";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string RequiredColumnDoesNotExist(object p0)
        {
            string name = "RequiredColumnDoesNotExist";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string SimpleCaseShouldNotHold(object p0)
        {
            string name = "SimpleCaseShouldNotHold";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string UnexpectedNode(object p0)
        {
            string name = "UnexpectedNode";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string UnhandledBindingType(object p0)
        {
            string name = "UnhandledBindingType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string UnhandledMemberAccess(object p0, object p1)
        {
            string name = "UnhandledMemberAccess";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string UnmappedDataMember(object p0, object p1, object p2)
        {
            string name = "UnmappedDataMember";
            object[] objArray = new object[3];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            int index3 = 2;
            object obj3 = p2;
            objArray[index3] = obj3;
            return SR.GetString(name, objArray);
        }

        internal static string UnrecognizedExpressionNode(object p0)
        {
            string name = "UnrecognizedExpressionNode";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ValueHasNoLiteralInSql(object p0)
        {
            string name = "ValueHasNoLiteralInSql";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string UnhandledExpressionType(object p0)
        {
            string name = "UnhandledExpressionType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string NonCountAggregateFunctionsAreNotValidOnProjections(object p0)
        {
            string name = "NonCountAggregateFunctionsAreNotValidOnProjections";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string SourceExpressionAnnotation(object p0)
        {
            string name = "SourceExpressionAnnotation";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string SequenceOperatorsNotSupportedForType(object p0)
        {
            string name = "SequenceOperatorsNotSupportedForType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ComparisonNotSupportedForType(object p0)
        {
            string name = "ComparisonNotSupportedForType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string UnsupportedNodeType(object p0)
        {
            string name = "UnsupportedNodeType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string TypeCannotBeOrdered(object p0)
        {
            string name = "TypeCannotBeOrdered";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string InvalidMethodExecution(object p0)
        {
            string name = "InvalidMethodExecution";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CouldNotConvertToPropertyOrField(object p0)
        {
            string name = "CouldNotConvertToPropertyOrField";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string BadParameterType(object p0)
        {
            string name = "BadParameterType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CannotAssignToMember(object p0)
        {
            string name = "CannotAssignToMember";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string MappedTypeMustHaveDefaultConstructor(object p0)
        {
            string name = "MappedTypeMustHaveDefaultConstructor";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string UnsafeStringConversion(object p0, object p1)
        {
            string name = "UnsafeStringConversion";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string CannotAssignNull(object p0)
        {
            string name = "CannotAssignNull";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ProviderNotInstalled(object p0, object p1)
        {
            string name = "ProviderNotInstalled";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string InvalidProviderType(object p0)
        {
            string name = "InvalidProviderType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string InvalidDbGeneratedType(object p0)
        {
            string name = "InvalidDbGeneratedType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CannotMaterializeEntityType(object p0)
        {
            string name = "CannotMaterializeEntityType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CannotMaterializeList(object p0)
        {
            string name = "CannotMaterializeList";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CouldNotConvert(object p0, object p1)
        {
            string name = "CouldNotConvert";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }
    }
}
