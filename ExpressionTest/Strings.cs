using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{

    internal static class Strings
    {
        internal static string CouldNotGetSqlType
        {
            get
            {
                return SR.GetString("CouldNotGetSqlType");
            }
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
        internal static string ColumnIsDefinedInMultiplePlaces(object p0)
        {
            string name = "ColumnIsDefinedInMultiplePlaces";
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
        internal static string ValueHasNoLiteralInSql(object p0)
        {
            string name = "ValueHasNoLiteralInSql";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string CannotCompareItemsAssociatedWithDifferentTable
        {
            get
            {
                return SR.GetString("CannotCompareItemsAssociatedWithDifferentTable");
            }
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
        internal static string InvalidFormatNode(object p0)
        {
            string name = "InvalidFormatNode";
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
        internal static string CouldNotGetClrType
        {
            get
            {
                return SR.GetString("CouldNotGetClrType");
            }
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
        internal static string UnexpectedTypeCode(object p0)
        {
            string name = "UnexpectedTypeCode";
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
        internal static string EmptyCaseNotSupported
        {
            get
            {
                return SR.GetString("EmptyCaseNotSupported");
            }
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
        internal static string TextNTextAndImageCannotOccurInDistinct(object p0)
        {
            string name = "TextNTextAndImageCannotOccurInDistinct";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string InheritanceTypeHasMultipleDefaults(object p0)
        {
            string name = "InheritanceTypeHasMultipleDefaults";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string InheritanceHierarchyDoesNotDefineDefault(object p0)
        {
            string name = "InheritanceHierarchyDoesNotDefineDefault";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string InheritanceCodeUsedForMultipleTypes(object p0)
        {
            string name = "InheritanceCodeUsedForMultipleTypes";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string InheritanceTypeHasMultipleDiscriminators(object p0)
        {
            string name = "InheritanceTypeHasMultipleDiscriminators";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string InheritanceCodeMayNotBeNull
        {
            get
            {
                return SR.GetString("InheritanceCodeMayNotBeNull");
            }
        }
        internal static string AbstractClassAssignInheritanceDiscriminator(object p0)
        {
            string name = "AbstractClassAssignInheritanceDiscriminator";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string InheritanceTypeDoesNotDeriveFromRoot(object p0, object p1)
        {
            string name = "InheritanceTypeDoesNotDeriveFromRoot";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }
        internal static string DiscriminatorClrTypeNotSupported(object p0, object p1, object p2)
        {
            string name = "DiscriminatorClrTypeNotSupported";
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
        internal static string NoDiscriminatorFound(object p0)
        {
            string name = "NoDiscriminatorFound";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string MemberMappedMoreThanOnce(object p0)
        {
            string name = "MemberMappedMoreThanOnce";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string NonInheritanceClassHasDiscriminator(object p0)
        {
            string name = "NonInheritanceClassHasDiscriminator";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string InheritanceSubTypeIsAlsoRoot(object p0)
        {
            string name = "InheritanceSubTypeIsAlsoRoot";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string TooManyResultTypesDeclaredForFunction(object p0)
        {
            string name = "TooManyResultTypesDeclaredForFunction";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string NoResultTypesDeclaredForFunction(object p0)
        {
            string name = "NoResultTypesDeclaredForFunction";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string BadKeyMember(object p0, object p1, object p2)
        {
            string name = "BadKeyMember";
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

        internal static string InvalidDeleteOnNullSpecification(object p0)
        {
            string name = "InvalidDeleteOnNullSpecification";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string MismatchedThisKeyOtherKey(object p0, object p1)
        {
            string name = "MismatchedThisKeyOtherKey";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }
        internal static string IncorrectAutoSyncSpecification(object p0)
        {
            string name = "IncorrectAutoSyncSpecification";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string BadStorageProperty(object p0, object p1, object p2)
        {
            string name = "BadStorageProperty";
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
        internal static string InvalidFieldInfo(object p0, object p1, object p2)
        {
            string name = "InvalidFieldInfo";
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
        internal static string PrimaryKeyInSubTypeNotSupported(object p0, object p1)
        {
            string name = "PrimaryKeyInSubTypeNotSupported";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }
        internal static string UnableToAssignValueToReadonlyProperty(object p0)
        {
            string name = "UnableToAssignValueToReadonlyProperty";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string CouldNotCreateAccessorToProperty(object p0, object p1, object p2)
        {
            string name = "CouldNotCreateAccessorToProperty";
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

        internal static string LinkAlreadyLoaded
        {
            get
            {
                return SR.GetString("LinkAlreadyLoaded");
            }
        }
        internal static string EntityRefAlreadyLoaded
        {
            get
            {
                return SR.GetString("EntityRefAlreadyLoaded");
            }
        }
        internal static string UnhandledDeferredStorageType(object p0)
        {
            string name = "UnhandledDeferredStorageType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string TwoMembersMarkedAsPrimaryKeyAndDBGenerated(object p0, object p1)
        {
            string name = "TwoMembersMarkedAsPrimaryKeyAndDBGenerated";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }
        internal static string IdentityClrTypeNotSupported(object p0, object p1, object p2)
        {
            string name = "IdentityClrTypeNotSupported";
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
        internal static string TwoMembersMarkedAsRowVersion(object p0, object p1)
        {
            string name = "TwoMembersMarkedAsRowVersion";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string TwoMembersMarkedAsInheritanceDiscriminator(object p0, object p1)
        {
            string name = "TwoMembersMarkedAsInheritanceDiscriminator";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }
        internal static string MappingOfInterfacesMemberIsNotSupported(object p0, object p1)
        {
            string name = "MappingOfInterfacesMemberIsNotSupported";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }
        internal static string UnmappedClassMember(object p0, object p1)
        {
            string name = "UnmappedClassMember";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
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
        internal static string GeneralCollectionMaterializationNotSupported
        {
            get
            {
                return SR.GetString("GeneralCollectionMaterializationNotSupported");
            }
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
        internal static string ExpressionNotDeferredQuerySource
        {
            get
            {
                return SR.GetString("ExpressionNotDeferredQuerySource");
            }
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
        internal static string DeferredMemberWrongType
        {
            get
            {
                return SR.GetString("DeferredMemberWrongType");
            }
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
        internal static string CouldNotTranslateExpressionForReading(object p0)
        {
            string name = "CouldNotTranslateExpressionForReading";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string UnhandledStringTypeComparison
        {
            get
            {
                return SR.GetString("UnhandledStringTypeComparison");
            }
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

        internal static string LenOfTextOrNTextNotSupported(object p0)
        {
            string name = "LenOfTextOrNTextNotSupported";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string InvalidReferenceToRemovedAliasDuringDeflation
        {
            get
            {
                return SR.GetString("InvalidReferenceToRemovedAliasDuringDeflation");
            }
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
        internal static string SourceExpressionAnnotation(object p0)
        {
            string name = "SourceExpressionAnnotation";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string UnexpectedFloatingColumn
        {
            get
            {
                return SR.GetString("UnexpectedFloatingColumn");
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
        internal static string ConvertToCharFromBoolNotSupported
        {
            get
            {
                return SR.GetString("ConvertToCharFromBoolNotSupported");
            }
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
        internal static string TransactionDoesNotMatchConnection
        {
            get
            {
                return SR.GetString("TransactionDoesNotMatchConnection");
            }
        }
        internal static string CompiledQueryAgainstMultipleShapesNotSupported
        {
            get
            {
                return SR.GetString("CompiledQueryAgainstMultipleShapesNotSupported");
            }
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
        internal static string CannotEnumerateResultsMoreThanOnce
        {
            get
            {
                return SR.GetString("CannotEnumerateResultsMoreThanOnce");
            }
        }
        internal static string UnexpectedSharedExpressionReference
        {
            get
            {
                return SR.GetString("UnexpectedSharedExpressionReference");
            }
        }

        internal static string UnexpectedSharedExpression
        {
            get
            {
                return SR.GetString("UnexpectedSharedExpression");
            }
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
        internal static string InvalidUseOfGenericMethodAsMappedFunction(object p0)
        {
            string name = "InvalidUseOfGenericMethodAsMappedFunction";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string QueryOnLocalCollectionNotSupported
        {
            get
            {
                return SR.GetString("QueryOnLocalCollectionNotSupported");
            }
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
        internal static string DidNotExpectTypeBinding
        {
            get
            {
                return SR.GetString("DidNotExpectTypeBinding");
            }
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
        internal static string ComparisonNotSupportedForType(object p0)
        {
            string name = "ComparisonNotSupportedForType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string UnionDifferentMemberOrder
        {
            get
            {
                return SR.GetString("UnionDifferentMemberOrder");
            }
        }
        internal static string UnionDifferentMembers
        {
            get
            {
                return SR.GetString("UnionDifferentMembers");
            }
        }
        internal static string UnionWithHierarchy
        {
            get
            {
                return SR.GetString("UnionWithHierarchy");
            }
        }
        internal static string UnionIncompatibleConstruction
        {
            get
            {
                return SR.GetString("UnionIncompatibleConstruction");
            }
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
        internal static string MathRoundNotSupported
        {
            get
            {
                return SR.GetString("MathRoundNotSupported");
            }
        }
        internal static string IndexOfWithStringComparisonArgNotSupported
        {
            get
            {
                return SR.GetString("IndexOfWithStringComparisonArgNotSupported");
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
        internal static string LastIndexOfWithStringComparisonArgNotSupported
        {
            get
            {
                return SR.GetString("LastIndexOfWithStringComparisonArgNotSupported");
            }
        }
        internal static string ConvertToDateTimeOnlyForDateTimeOrString
        {
            get
            {
                return SR.GetString("ConvertToDateTimeOnlyForDateTimeOrString");
            }
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
        internal static string ToStringOnlySupportedForPrimitiveTypes
        {
            get
            {
                return SR.GetString("ToStringOnlySupportedForPrimitiveTypes");
            }
        }
        internal static string UnsupportedTimeSpanConstructorForm
        {
            get
            {
                return SR.GetString("UnsupportedTimeSpanConstructorForm");
            }
        }
        internal static string UnsupportedDateTimeOffsetConstructorForm
        {
            get
            {
                return SR.GetString("UnsupportedDateTimeOffsetConstructorForm");
            }
        }

        internal static string UnsupportedDateTimeConstructorForm
        {
            get
            {
                return SR.GetString("UnsupportedDateTimeConstructorForm");
            }
        }
        internal static string UnsupportedStringConstructorForm
        {
            get
            {
                return SR.GetString("UnsupportedStringConstructorForm");
            }
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
        internal static string ClassLiteralsNotAllowed(object p0)
        {
            string name = "ClassLiteralsNotAllowed";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string ColumnClrTypeDoesNotAgreeWithExpressionsClrType
        {
            get
            {
                return SR.GetString("ColumnClrTypeDoesNotAgreeWithExpressionsClrType");
            }
        }

        internal static string UpdateItemMustBeConstant
        {
            get
            {
                return SR.GetString("UpdateItemMustBeConstant");
            }
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
        internal static string InsertItemMustBeConstant
        {
            get
            {
                return SR.GetString("InsertItemMustBeConstant");
            }
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
        internal static string InvalidSequenceOperatorCall(object p0)
        {
            string name = "InvalidSequenceOperatorCall";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
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
        internal static string NonConstantExpressionsNotSupportedForRounding
        {
            get
            {
                return SR.GetString("NonConstantExpressionsNotSupportedForRounding");
            }
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
        internal static string NonCountAggregateFunctionsAreNotValidOnProjections(object p0)
        {
            string name = "NonCountAggregateFunctionsAreNotValidOnProjections";
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
        internal static string TypeCannotBeOrdered(object p0)
        {
            string name = "TypeCannotBeOrdered";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string GroupingNotSupportedAsOrderCriterion
        {
            get
            {
                return SR.GetString("GroupingNotSupportedAsOrderCriterion");
            }
        }
        internal static string ExceptNotSupportedForHierarchicalTypes
        {
            get
            {
                return SR.GetString("ExceptNotSupportedForHierarchicalTypes");
            }
        }
        internal static string IntersectNotSupportedForHierarchicalTypes
        {
            get
            {
                return SR.GetString("IntersectNotSupportedForHierarchicalTypes");
            }
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
        internal static string InvalidReturnFromSproc(object p0)
        {
            string name = "InvalidReturnFromSproc";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string SprocsCannotBeComposed
        {
            get
            {
                return SR.GetString("SprocsCannotBeComposed");
            }
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

        internal static string SkipRequiresSingleTableQueryWithPKs
        {
            get
            {
                return SR.GetString("SkipRequiresSingleTableQueryWithPKs");
            }
        }
        internal static string SkipNotSupportedForSequenceTypes
        {
            get
            {
                return SR.GetString("SkipNotSupportedForSequenceTypes");
            }
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

        internal static string CannotMaterializeEntityType(object p0)
        {
            string name = "CannotMaterializeEntityType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
        internal static string WrongDataContext
        {
            get
            {
                return SR.GetString("WrongDataContext");
            }
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
        internal static string TypeBinaryOperatorNotRecognized
        {
            get
            {
                return SR.GetString("TypeBinaryOperatorNotRecognized");
            }
        }
        internal static string ColumnCannotReferToItself
        {
            get
            {
                return SR.GetString("ColumnCannotReferToItself");
            }
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
        internal static string BadProjectionInSelect
        {
            get
            {
                return SR.GetString("BadProjectionInSelect");
            }
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
        internal static string ParametersCannotBeSequences
        {
            get
            {
                return SR.GetString("ParametersCannotBeSequences");
            }
        }
        internal static string ConstructedArraysNotSupported
        {
            get
            {
                return SR.GetString("ConstructedArraysNotSupported");
            }
        }
        internal static string IQueryableCannotReturnSelfReferencingConstantExpression
        {
            get
            {
                return SR.GetString("IQueryableCannotReturnSelfReferencingConstantExpression");
            }
        }
        internal static string CapturedValuesCannotBeSequences
        {
            get
            {
                return SR.GetString("CapturedValuesCannotBeSequences");
            }
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

        internal static string UnsupportedNodeType(object p0)
        {
            string name = "UnsupportedNodeType";
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
        internal static string CannotTranslateExpressionToSql
        {
            get
            {
                return SR.GetString("CannotTranslateExpressionToSql");
            }
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
        internal static string CouldNotDetermineCatalogName
        {
            get
            {
                return SR.GetString("CouldNotDetermineCatalogName");
            }
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
        internal static string ProviderCannotBeUsedAfterDispose
        {
            get
            {
                return SR.GetString("ProviderCannotBeUsedAfterDispose");
            }
        }
        internal static string DatabaseDeleteThroughContext
        {
            get
            {
                return SR.GetString("DatabaseDeleteThroughContext");
            }
        }   
        internal static string ContextNotInitialized
        {
            get
            {
                return SR.GetString("ContextNotInitialized");
            }
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
        internal static string OwningTeam
        {
            get
            {
                return SR.GetString("OwningTeam");
            }
        }

        internal static string CannotAddChangeConflicts
        {
            get
            {
                return SR.GetString("CannotAddChangeConflicts");
            }
        }

        internal static string CannotRemoveChangeConflicts
        {
            get
            {
                return SR.GetString("CannotRemoveChangeConflicts");
            }
        }

        internal static string UnableToDetermineDataContext
        {
            get
            {
                return SR.GetString("UnableToDetermineDataContext");
            }
        }

        internal static string CannotRemoveUnattachedEntity
        {
            get
            {
                return SR.GetString("CannotRemoveUnattachedEntity");
            }
        }

        internal static string CouldNotAttach
        {
            get
            {
                return SR.GetString("CouldNotAttach");
            }
        }

        internal static string EntitySetAlreadyLoaded
        {
            get
            {
                return SR.GetString("EntitySetAlreadyLoaded");
            }
        }

        internal static string EntitySetModifiedDuringEnumeration
        {
            get
            {
                return SR.GetString("EntitySetModifiedDuringEnumeration");
            }
        }

        internal static string ExpectedUpdateDeleteOrChange
        {
            get
            {
                return SR.GetString("ExpectedUpdateDeleteOrChange");
            }
        }

        internal static string ModifyDuringAddOrRemove
        {
            get
            {
                return SR.GetString("ModifyDuringAddOrRemove");
            }
        }

        internal static string ProviderTypeNull
        {
            get
            {
                return SR.GetString("ProviderTypeNull");
            }
        }

        internal static string UnrecognizedRefreshObject
        {
            get
            {
                return SR.GetString("UnrecognizedRefreshObject");
            }
        }

        internal static string ObjectTrackingRequired
        {
            get
            {
                return SR.GetString("ObjectTrackingRequired");
            }
        }

        internal static string OptionsCannotBeModifiedAfterQuery
        {
            get
            {
                return SR.GetString("OptionsCannotBeModifiedAfterQuery");
            }
        }

        internal static string DeferredLoadingRequiresObjectTracking
        {
            get
            {
                return SR.GetString("DeferredLoadingRequiresObjectTracking");
            }
        }

        internal static string SubqueryNotAllowedAfterFreeze
        {
            get
            {
                return SR.GetString("SubqueryNotAllowedAfterFreeze");
            }
        }

        internal static string IncludeNotAllowedAfterFreeze
        {
            get
            {
                return SR.GetString("IncludeNotAllowedAfterFreeze");
            }
        }

        internal static string LoadOptionsChangeNotAllowedAfterQuery
        {
            get
            {
                return SR.GetString("LoadOptionsChangeNotAllowedAfterQuery");
            }
        }

        internal static string IncludeCycleNotAllowed
        {
            get
            {
                return SR.GetString("IncludeCycleNotAllowed");
            }
        }

        internal static string SubqueryMustBeSequence
        {
            get
            {
                return SR.GetString("SubqueryMustBeSequence");
            }
        }

        internal static string RefreshOfDeletedObject
        {
            get
            {
                return SR.GetString("RefreshOfDeletedObject");
            }
        }

        internal static string RefreshOfNewObject
        {
            get
            {
                return SR.GetString("RefreshOfNewObject");
            }
        }

        internal static string DataContextCannotBeUsedAfterDispose
        {
            get
            {
                return SR.GetString("DataContextCannotBeUsedAfterDispose");
            }
        }

        internal static string InsertCallbackComment
        {
            get
            {
                return SR.GetString("InsertCallbackComment");
            }
        }

        internal static string UpdateCallbackComment
        {
            get
            {
                return SR.GetString("UpdateCallbackComment");
            }
        }

        internal static string DeleteCallbackComment
        {
            get
            {
                return SR.GetString("DeleteCallbackComment");
            }
        }

        internal static string RowNotFoundOrChanged
        {
            get
            {
                return SR.GetString("RowNotFoundOrChanged");
            }
        }

        internal static string CycleDetected
        {
            get
            {
                return SR.GetString("CycleDetected");
            }
        }

        internal static string CantAddAlreadyExistingItem
        {
            get
            {
                return SR.GetString("CantAddAlreadyExistingItem");
            }
        }

        internal static string CantAddAlreadyExistingKey
        {
            get
            {
                return SR.GetString("CantAddAlreadyExistingKey");
            }
        }

        internal static string DatabaseGeneratedAlreadyExistingKey
        {
            get
            {
                return SR.GetString("DatabaseGeneratedAlreadyExistingKey");
            }
        }

        internal static string InsertAutoSyncFailure
        {
            get
            {
                return SR.GetString("InsertAutoSyncFailure");
            }
        }

        internal static string InvalidLoadOptionsLoadMemberSpecification
        {
            get
            {
                return SR.GetString("InvalidLoadOptionsLoadMemberSpecification");
            }
        }

        internal static string EntityIsTheWrongType
        {
            get
            {
                return SR.GetString("EntityIsTheWrongType");
            }
        }

        internal static string OriginalEntityIsWrongType
        {
            get
            {
                return SR.GetString("OriginalEntityIsWrongType");
            }
        }

        internal static string CannotAttachAlreadyExistingEntity
        {
            get
            {
                return SR.GetString("CannotAttachAlreadyExistingEntity");
            }
        }

        internal static string CannotAttachAsModifiedWithoutOriginalState
        {
            get
            {
                return SR.GetString("CannotAttachAsModifiedWithoutOriginalState");
            }
        }

        internal static string CannotPerformOperationDuringSubmitChanges
        {
            get
            {
                return SR.GetString("CannotPerformOperationDuringSubmitChanges");
            }
        }

        internal static string CannotPerformOperationOutsideSubmitChanges
        {
            get
            {
                return SR.GetString("CannotPerformOperationOutsideSubmitChanges");
            }
        }

        internal static string CannotPerformOperationForUntrackedObject
        {
            get
            {
                return SR.GetString("CannotPerformOperationForUntrackedObject");
            }
        }

        internal static string CannotAttachAddNonNewEntities
        {
            get
            {
                return SR.GetString("CannotAttachAddNonNewEntities");
            }
        }

        internal static string QueryWasCompiledForDifferentMappingSource
        {
            get
            {
                return SR.GetString("QueryWasCompiledForDifferentMappingSource");
            }
        }

        internal static string InconsistentAssociationAndKeyChange(object p0, object p1)
        {
            string name = "InconsistentAssociationAndKeyChange";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string ArgumentTypeHasNoIdentityKey(object p0)
        {
            string name = "ArgumentTypeHasNoIdentityKey";
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

        internal static string ColumnMappedMoreThanOnce(object p0)
        {
            string name = "ColumnMappedMoreThanOnce";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string CouldNotGetTableForSubtype(object p0, object p1)
        {
            string name = "CouldNotGetTableForSubtype";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string CouldNotRemoveRelationshipBecauseOneSideCannotBeNull(object p0, object p1, object p2)
        {
            string name = "CouldNotRemoveRelationshipBecauseOneSideCannotBeNull";
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

        internal static string ExpectedQueryableArgument(object p0, object p1)
        {
            string name = "ExpectedQueryableArgument";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string KeyIsWrongSize(object p0, object p1)
        {
            string name = "KeyIsWrongSize";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string KeyValueIsWrongType(object p0, object p1)
        {
            string name = "KeyValueIsWrongType";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string IdentityChangeNotAllowed(object p0, object p1)
        {
            string name = "IdentityChangeNotAllowed";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string DbGeneratedChangeNotAllowed(object p0, object p1)
        {
            string name = "DbGeneratedChangeNotAllowed";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string ProviderDoesNotImplementRequiredInterface(object p0, object p1)
        {
            string name = "ProviderDoesNotImplementRequiredInterface";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string TypeCouldNotBeAdded(object p0)
        {
            string name = "TypeCouldNotBeAdded";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string TypeCouldNotBeRemoved(object p0)
        {
            string name = "TypeCouldNotBeRemoved";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string TypeCouldNotBeTracked(object p0)
        {
            string name = "TypeCouldNotBeTracked";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string TypeIsNotEntity(object p0)
        {
            string name = "TypeIsNotEntity";
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

        internal static string UnhandledBindingType(object p0)
        {
            string name = "UnhandledBindingType";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string SubqueryDoesNotSupportOperator(object p0)
        {
            string name = "SubqueryDoesNotSupportOperator";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string SubqueryNotSupportedOn(object p0)
        {
            string name = "SubqueryNotSupportedOn";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string SubqueryNotSupportedOnType(object p0, object p1)
        {
            string name = "SubqueryNotSupportedOnType";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string CannotChangeInheritanceType(object p0, object p1, object p2, object p3)
        {
            string name = "CannotChangeInheritanceType";
            object[] objArray = new object[4];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            int index3 = 2;
            object obj3 = p2;
            objArray[index3] = obj3;
            int index4 = 3;
            object obj4 = p3;
            objArray[index4] = obj4;
            return SR.GetString(name, objArray);
        }

        internal static string TypeIsNotMarkedAsTable(object p0)
        {
            string name = "TypeIsNotMarkedAsTable";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string NonEntityAssociationMapping(object p0, object p1, object p2)
        {
            string name = "NonEntityAssociationMapping";
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

        internal static string CannotPerformCUDOnReadOnlyTable(object p0)
        {
            string name = "CannotPerformCUDOnReadOnlyTable";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string UpdatesFailedMessage(object p0, object p1)
        {
            string name = "UpdatesFailedMessage";
            object[] objArray = new object[2];
            int index1 = 0;
            object obj1 = p0;
            objArray[index1] = obj1;
            int index2 = 1;
            object obj2 = p1;
            objArray[index2] = obj2;
            return SR.GetString(name, objArray);
        }

        internal static string EntitySetDataBindingWithAbstractBaseClass(object p0)
        {
            string name = "EntitySetDataBindingWithAbstractBaseClass";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }

        internal static string EntitySetDataBindingWithNonPublicDefaultConstructor(object p0)
        {
            string name = "EntitySetDataBindingWithNonPublicDefaultConstructor";
            object[] objArray = new object[1];
            int index = 0;
            object obj = p0;
            objArray[index] = obj;
            return SR.GetString(name, objArray);
        }
    }
}
