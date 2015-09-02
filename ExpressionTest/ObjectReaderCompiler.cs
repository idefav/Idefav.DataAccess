using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class ObjectReaderCompiler : IObjectReaderCompiler
    {
        private static LocalDataStoreSlot cacheSlot = Thread.AllocateDataSlot();
        private static int maxReaderCacheSize = 10;
        private Type dataReaderType;
        private IDataServices services;
        private MethodInfo miDRisDBNull;
        private MethodInfo miBRisDBNull;
        private FieldInfo readerField;
        private FieldInfo bufferReaderField;
        private FieldInfo ordinalsField;
        private FieldInfo globalsField;
        private FieldInfo argsField;

        internal ObjectReaderCompiler(Type dataReaderType, IDataServices services)
        {
            this.dataReaderType = dataReaderType;
            this.services = services;
            this.miDRisDBNull = dataReaderType.GetMethod("IsDBNull", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            this.miBRisDBNull = typeof(DbDataReader).GetMethod("IsDBNull", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Type type1 = typeof(ObjectMaterializer<>);
            Type[] typeArray = new Type[1];
            int index = 0;
            Type type2 = this.dataReaderType;
            typeArray[index] = type2;
            Type type3 = type1.MakeGenericType(typeArray);
            this.ordinalsField = type3.GetField("Ordinals", BindingFlags.Instance | BindingFlags.Public);
            this.globalsField = type3.GetField("Globals", BindingFlags.Instance | BindingFlags.Public);
            this.argsField = type3.GetField("Arguments", BindingFlags.Instance | BindingFlags.Public);
            this.readerField = type3.GetField("DataReader", BindingFlags.Instance | BindingFlags.Public);
            this.bufferReaderField = type3.GetField("BufferReader", BindingFlags.Instance | BindingFlags.Public);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public IObjectReaderFactory Compile(SqlExpression expression, Type elementType)
        {
            object identity = this.services.Context.Mapping.Identity;
            DataLoadOptions loadOptions = this.services.Context.LoadOptions;
            IObjectReaderFactory factory = (IObjectReaderFactory)null;
            ObjectReaderCompiler.ReaderFactoryCache readerFactoryCache = (ObjectReaderCompiler.ReaderFactoryCache)null;
            bool flag = ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(expression);
            if (flag)
            {
                readerFactoryCache = (ObjectReaderCompiler.ReaderFactoryCache)Thread.GetData(ObjectReaderCompiler.cacheSlot);
                if (readerFactoryCache == null)
                {
                    readerFactoryCache = new ObjectReaderCompiler.ReaderFactoryCache(ObjectReaderCompiler.maxReaderCacheSize);
                    Thread.SetData(ObjectReaderCompiler.cacheSlot, (object)readerFactoryCache);
                }
                factory = readerFactoryCache.GetFactory(elementType, this.dataReaderType, identity, loadOptions, expression);
            }
            if (factory == null)
            {
                ObjectReaderCompiler.Generator gen = new ObjectReaderCompiler.Generator(this, elementType);
                DynamicMethod dynamicMethod = this.CompileDynamicMethod(gen, expression, elementType);
                Type type1 = typeof(Func<,>);
                Type[] typeArray1 = new Type[2];
                int index1 = 0;
                Type type2 = typeof(ObjectMaterializer<>);
                Type[] typeArray2 = new Type[1];
                int index2 = 0;
                Type type3 = this.dataReaderType;
                typeArray2[index2] = type3;
                Type type4 = type2.MakeGenericType(typeArray2);
                typeArray1[index1] = type4;
                int index3 = 1;
                Type type5 = elementType;
                typeArray1[index3] = type5;
                Type type6 = type1.MakeGenericType(typeArray1);
                Delegate delegate1 = ((MethodInfo)dynamicMethod).CreateDelegate(type6);
                Type type7 = typeof(ObjectReaderCompiler.ObjectReaderFactory<,>);
                Type[] typeArray3 = new Type[2];
                int index4 = 0;
                Type type8 = this.dataReaderType;
                typeArray3[index4] = type8;
                int index5 = 1;
                Type type9 = elementType;
                typeArray3[index5] = type9;
                Type type10 = type7.MakeGenericType(typeArray3);
                int num = 36;
                // ISSUE: variable of the null type
                //__Null local1 = null;
                object[] args = new object[4];
                int index6 = 0;
                Delegate delegate2 = delegate1;
                args[index6] = (object)delegate2;
                int index7 = 1;
                ObjectReaderCompiler.NamedColumn[] namedColumns = gen.NamedColumns;
                args[index7] = (object)namedColumns;
                int index8 = 2;
                object[] globals = gen.Globals;
                args[index8] = (object)globals;
                int index9 = 3;
                // ISSUE: variable of a boxed type
                var local2 = (System.ValueType)gen.Locals;
                args[index9] = (object)null;
                // ISSUE: variable of the null type
                //__Null local3 = null;
                factory = (IObjectReaderFactory)Activator.CreateInstance(type10, (BindingFlags)num, (Binder)null, args, (CultureInfo)null);
                if (flag)
                {
                    expression = new ObjectReaderCompiler.SourceExpressionRemover().VisitExpression(expression);
                    readerFactoryCache.AddFactory(elementType, this.dataReaderType, identity, loadOptions, expression, factory);
                }
            }
            return factory;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public IObjectReaderSession CreateSession(DbDataReader reader, IReaderProvider provider, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries)
        {
            Type type1 = typeof(ObjectReaderCompiler.ObjectReaderSession<>);
            Type[] typeArray = new Type[1];
            int index1 = 0;
            Type type2 = this.dataReaderType;
            typeArray[index1] = type2;
            Type type3 = type1.MakeGenericType(typeArray);
            int num = 52;
            // ISSUE: variable of the null type
            //__Null local1 = null;
            object[] args = new object[5];
            int index2 = 0;
            DbDataReader dbDataReader = reader;
            args[index2] = (object)dbDataReader;
            int index3 = 1;
            IReaderProvider readerProvider = provider;
            args[index3] = (object)readerProvider;
            int index4 = 2;
            object[] objArray1 = parentArgs;
            args[index4] = (object)objArray1;
            int index5 = 3;
            object[] objArray2 = userArgs;
            args[index5] = (object)objArray2;
            int index6 = 4;
            ICompiledSubQuery[] compiledSubQueryArray = subQueries;
            args[index6] = (object)compiledSubQueryArray;
            // ISSUE: variable of the null type
           // __Null local2 = null;
            return (IObjectReaderSession)Activator.CreateInstance(type3, (BindingFlags)num, (Binder)null, args, (CultureInfo)null);
        }

        private DynamicMethod CompileDynamicMethod(ObjectReaderCompiler.Generator gen, SqlExpression expression, Type elementType)
        {
            Type type1 = typeof(ObjectMaterializer<>);
            Type[] typeArray = new Type[1];
            int index1 = 0;
            Type type2 = this.dataReaderType;
            typeArray[index1] = type2;
            Type type3 = type1.MakeGenericType(typeArray);
            string name = "Read_" + elementType.Name;
            Type returnType = elementType;
            Type[] parameterTypes = new Type[1];
            int index2 = 0;
            Type type4 = type3;
            parameterTypes[index2] = type4;
            int num = 1;
            DynamicMethod dynamicMethod = new DynamicMethod(name, returnType, parameterTypes, num != 0);
            gen.GenerateBody(dynamicMethod.GetILGenerator(), expression);
            return dynamicMethod;
        }

        private class SourceExpressionRemover : SqlDuplicator.DuplicatingVisitor
        {
            internal SourceExpressionRemover()
              : base(true)
            {
            }

            internal override SqlNode Visit(SqlNode node)
            {
                node = base.Visit(node);
                if (node != null)
                    node.ClearSourceExpression();
                return node;
            }

            internal override SqlExpression VisitColumnRef(SqlColumnRef cref)
            {
                SqlExpression sqlExpression = base.VisitColumnRef(cref);
                if (sqlExpression != null && sqlExpression == cref)
                {
                    SqlColumn column = cref.Column;
                    SqlColumn col = new SqlColumn(column.ClrType, column.SqlType, column.Name, column.MetaMember, (SqlExpression)null, column.SourceExpression);
                    int ordinal = column.Ordinal;
                    col.Ordinal = ordinal;
                    sqlExpression = (SqlExpression)new SqlColumnRef(col);
                    col.ClearSourceExpression();
                }
                return sqlExpression;
            }

            internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
            {
                SqlExpression sqlExpression = base.VisitAliasRef(aref);
                if (sqlExpression == null || sqlExpression != aref)
                    return sqlExpression;
                SqlAlias alias = aref.Alias;
                return (SqlExpression)new SqlAliasRef(new SqlAlias((SqlNode)new SqlNop(aref.ClrType, aref.SqlType, (Expression)null)));
            }
        }

        private class ReaderFactoryCache
        {
            private int maxCacheSize;
            private LinkedList<ObjectReaderCompiler.ReaderFactoryCache.CacheInfo> list;

            internal ReaderFactoryCache(int maxCacheSize)
            {
                this.maxCacheSize = maxCacheSize;
                this.list = new LinkedList<ObjectReaderCompiler.ReaderFactoryCache.CacheInfo>();
            }

            internal IObjectReaderFactory GetFactory(Type elementType, Type dataReaderType, object mapping, DataLoadOptions options, SqlExpression projection)
            {
                for (LinkedListNode<ObjectReaderCompiler.ReaderFactoryCache.CacheInfo> node = this.list.First; node != null; node = node.Next)
                {
                    if (elementType == node.Value.elementType && dataReaderType == node.Value.dataReaderType && (mapping == node.Value.mapping && DataLoadOptions.ShapesAreEquivalent(options, node.Value.options)) && ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(projection, node.Value.projection))
                    {
                        this.list.Remove(node);
                        this.list.AddFirst(node);
                        return node.Value.factory;
                    }
                }
                return (IObjectReaderFactory)null;
            }

            internal void AddFactory(Type elementType, Type dataReaderType, object mapping, DataLoadOptions options, SqlExpression projection, IObjectReaderFactory factory)
            {
                this.list.AddFirst(new LinkedListNode<ObjectReaderCompiler.ReaderFactoryCache.CacheInfo>(new ObjectReaderCompiler.ReaderFactoryCache.CacheInfo(elementType, dataReaderType, mapping, options, projection, factory)));
                if (this.list.Count <= this.maxCacheSize)
                    return;
                this.list.RemoveLast();
            }

            private class CacheInfo
            {
                internal Type elementType;
                internal Type dataReaderType;
                internal object mapping;
                internal DataLoadOptions options;
                internal SqlExpression projection;
                internal IObjectReaderFactory factory;

                public CacheInfo(Type elementType, Type dataReaderType, object mapping, DataLoadOptions options, SqlExpression projection, IObjectReaderFactory factory)
                {
                    this.elementType = elementType;
                    this.dataReaderType = dataReaderType;
                    this.options = options;
                    this.mapping = mapping;
                    this.projection = projection;
                    this.factory = factory;
                }
            }
        }

        internal class SqlProjectionComparer
        {
            internal static bool CanBeCompared(SqlExpression node)
            {
                if (node == null)
                    return true;
                switch (node.NodeType)
                {
                    case SqlNodeType.Value:
                    case SqlNodeType.UserColumn:
                    case SqlNodeType.ColumnRef:
                        return true;
                    case SqlNodeType.ValueOf:
                    case SqlNodeType.OuterJoinedValue:
                        return ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(((SqlUnary)node).Operand);
                    case SqlNodeType.TypeCase:
                        SqlTypeCase sqlTypeCase = (SqlTypeCase)node;
                        if (!ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlTypeCase.Discriminator))
                            return false;
                        int index1 = 0;
                        for (int count = sqlTypeCase.Whens.Count; index1 < count; ++index1)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlTypeCase.Whens[index1].Match) || !ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlTypeCase.Whens[index1].TypeBinding))
                                return false;
                        }
                        return true;
                    case SqlNodeType.OptionalValue:
                        return ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(((SqlOptionalValue)node).Value);
                    case SqlNodeType.SearchedCase:
                        SqlSearchedCase sqlSearchedCase = (SqlSearchedCase)node;
                        int index2 = 0;
                        for (int count = sqlSearchedCase.Whens.Count; index2 < count; ++index2)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlSearchedCase.Whens[index2].Match) || !ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlSearchedCase.Whens[index2].Value))
                                return false;
                        }
                        return ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlSearchedCase.Else);
                    case SqlNodeType.JoinedCollection:
                        SqlJoinedCollection joinedCollection = (SqlJoinedCollection)node;
                        if (ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(joinedCollection.Count))
                            return ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(joinedCollection.Expression);
                        return false;
                    case SqlNodeType.MethodCall:
                        SqlMethodCall sqlMethodCall = (SqlMethodCall)node;
                        if (sqlMethodCall.Object != null && !ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlMethodCall.Object))
                            return false;
                        int num = 0;
                        for (int count = sqlMethodCall.Arguments.Count; num < count; ++num)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlMethodCall.Arguments[0]))
                                return false;
                        }
                        return true;
                    case SqlNodeType.Member:
                        return ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(((SqlMember)node).Expression);
                    case SqlNodeType.New:
                        SqlNew sqlNew = (SqlNew)node;
                        int index3 = 0;
                        for (int count = sqlNew.Args.Count; index3 < count; ++index3)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlNew.Args[index3]))
                                return false;
                        }
                        int index4 = 0;
                        for (int count = sqlNew.Members.Count; index4 < count; ++index4)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlNew.Members[index4].Expression))
                                return false;
                        }
                        return true;
                    case SqlNodeType.Link:
                        SqlLink sqlLink = (SqlLink)node;
                        int index5 = 0;
                        for (int count = sqlLink.KeyExpressions.Count; index5 < count; ++index5)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlLink.KeyExpressions[index5]))
                                return false;
                        }
                        return true;
                    case SqlNodeType.Grouping:
                        SqlGrouping sqlGrouping = (SqlGrouping)node;
                        if (ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlGrouping.Key))
                            return ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlGrouping.Group);
                        return false;
                    case SqlNodeType.ClientArray:
                        if (node.SourceExpression.NodeType != ExpressionType.NewArrayInit && node.SourceExpression.NodeType != ExpressionType.NewArrayBounds)
                            return false;
                        SqlClientArray sqlClientArray = (SqlClientArray)node;
                        int index6 = 0;
                        for (int count = sqlClientArray.Expressions.Count; index6 < count; ++index6)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlClientArray.Expressions[index6]))
                                return false;
                        }
                        return true;
                    case SqlNodeType.ClientCase:
                        SqlClientCase sqlClientCase = (SqlClientCase)node;
                        int index7 = 0;
                        for (int count = sqlClientCase.Whens.Count; index7 < count; ++index7)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlClientCase.Whens[index7].Match) || !ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(sqlClientCase.Whens[index7].Value))
                                return false;
                        }
                        return true;
                    case SqlNodeType.ClientQuery:
                        return true;
                    case SqlNodeType.DiscriminatedType:
                        return ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(((SqlDiscriminatedType)node).Discriminator);
                    case SqlNodeType.Lift:
                        return ObjectReaderCompiler.SqlProjectionComparer.CanBeCompared(((SqlLift)node).Expression);
                    default:
                        return false;
                }
            }

            internal static bool AreSimilar(SqlExpression node1, SqlExpression node2)
            {
                if (node1 == node2)
                    return true;
                if (node1 == null || node2 == null || (node1.NodeType != node2.NodeType || node1.ClrType != node2.ClrType) || node1.SqlType != node2.SqlType)
                    return false;
                switch (node1.NodeType)
                {
                    case SqlNodeType.Value:
                        return object.Equals(((SqlValue)node1).Value, ((SqlValue)node2).Value);
                    case SqlNodeType.ValueOf:
                    case SqlNodeType.OuterJoinedValue:
                        return ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(((SqlUnary)node1).Operand, ((SqlUnary)node2).Operand);
                    case SqlNodeType.TypeCase:
                        SqlTypeCase sqlTypeCase1 = (SqlTypeCase)node1;
                        SqlTypeCase sqlTypeCase2 = (SqlTypeCase)node2;
                        if (!ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlTypeCase1.Discriminator, sqlTypeCase2.Discriminator) || sqlTypeCase1.Whens.Count != sqlTypeCase2.Whens.Count)
                            return false;
                        int index1 = 0;
                        for (int count = sqlTypeCase1.Whens.Count; index1 < count; ++index1)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlTypeCase1.Whens[index1].Match, sqlTypeCase2.Whens[index1].Match) || !ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlTypeCase1.Whens[index1].TypeBinding, sqlTypeCase2.Whens[index1].TypeBinding))
                                return false;
                        }
                        return true;
                    case SqlNodeType.UserColumn:
                        return ((SqlUserColumn)node1).Name == ((SqlUserColumn)node2).Name;
                    case SqlNodeType.OptionalValue:
                        return ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(((SqlOptionalValue)node1).Value, ((SqlOptionalValue)node2).Value);
                    case SqlNodeType.SearchedCase:
                        SqlSearchedCase sqlSearchedCase1 = (SqlSearchedCase)node1;
                        SqlSearchedCase sqlSearchedCase2 = (SqlSearchedCase)node2;
                        if (sqlSearchedCase1.Whens.Count != sqlSearchedCase2.Whens.Count)
                            return false;
                        int index2 = 0;
                        for (int count = sqlSearchedCase1.Whens.Count; index2 < count; ++index2)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlSearchedCase1.Whens[index2].Match, sqlSearchedCase2.Whens[index2].Match) || !ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlSearchedCase1.Whens[index2].Value, sqlSearchedCase2.Whens[index2].Value))
                                return false;
                        }
                        return ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlSearchedCase1.Else, sqlSearchedCase2.Else);
                    case SqlNodeType.JoinedCollection:
                        SqlJoinedCollection joinedCollection1 = (SqlJoinedCollection)node1;
                        SqlJoinedCollection joinedCollection2 = (SqlJoinedCollection)node2;
                        if (ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(joinedCollection1.Count, joinedCollection2.Count))
                            return ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(joinedCollection1.Expression, joinedCollection2.Expression);
                        return false;
                    case SqlNodeType.MethodCall:
                        SqlMethodCall sqlMethodCall1 = (SqlMethodCall)node1;
                        SqlMethodCall sqlMethodCall2 = (SqlMethodCall)node2;
                        if (sqlMethodCall1.Method != sqlMethodCall2.Method || !ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlMethodCall1.Object, sqlMethodCall2.Object) || sqlMethodCall1.Arguments.Count != sqlMethodCall2.Arguments.Count)
                            return false;
                        int index3 = 0;
                        for (int count = sqlMethodCall1.Arguments.Count; index3 < count; ++index3)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlMethodCall1.Arguments[index3], sqlMethodCall2.Arguments[index3]))
                                return false;
                        }
                        return true;
                    case SqlNodeType.Member:
                        SqlMember sqlMember1 = (SqlMember)node1;
                        SqlMember sqlMember2 = (SqlMember)node2;
                        if (sqlMember1.Member == sqlMember2.Member)
                            return ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlMember1.Expression, sqlMember2.Expression);
                        return false;
                    case SqlNodeType.New:
                        SqlNew sqlNew1 = (SqlNew)node1;
                        SqlNew sqlNew2 = (SqlNew)node2;
                        if (sqlNew1.Args.Count != sqlNew2.Args.Count || sqlNew1.Members.Count != sqlNew2.Members.Count)
                            return false;
                        int index4 = 0;
                        for (int count = sqlNew1.Args.Count; index4 < count; ++index4)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlNew1.Args[index4], sqlNew2.Args[index4]))
                                return false;
                        }
                        int index5 = 0;
                        for (int count = sqlNew1.Members.Count; index5 < count; ++index5)
                        {
                            if (!MetaPosition.AreSameMember(sqlNew1.Members[index5].Member, sqlNew2.Members[index5].Member) || !ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlNew1.Members[index5].Expression, sqlNew2.Members[index5].Expression))
                                return false;
                        }
                        return true;
                    case SqlNodeType.Link:
                        SqlLink sqlLink1 = (SqlLink)node1;
                        SqlLink sqlLink2 = (SqlLink)node2;
                        if (!MetaPosition.AreSameMember(sqlLink1.Member.Member, sqlLink2.Member.Member) || sqlLink1.KeyExpressions.Count != sqlLink2.KeyExpressions.Count)
                            return false;
                        int index6 = 0;
                        for (int count = sqlLink1.KeyExpressions.Count; index6 < count; ++index6)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlLink1.KeyExpressions[index6], sqlLink2.KeyExpressions[index6]))
                                return false;
                        }
                        return true;
                    case SqlNodeType.Grouping:
                        SqlGrouping sqlGrouping1 = (SqlGrouping)node1;
                        SqlGrouping sqlGrouping2 = (SqlGrouping)node2;
                        if (ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlGrouping1.Key, sqlGrouping2.Key))
                            return ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlGrouping1.Group, sqlGrouping2.Group);
                        return false;
                    case SqlNodeType.ClientArray:
                        SqlClientArray sqlClientArray1 = (SqlClientArray)node1;
                        SqlClientArray sqlClientArray2 = (SqlClientArray)node2;
                        if (sqlClientArray1.Expressions.Count != sqlClientArray2.Expressions.Count)
                            return false;
                        int index7 = 0;
                        for (int count = sqlClientArray1.Expressions.Count; index7 < count; ++index7)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlClientArray1.Expressions[index7], sqlClientArray2.Expressions[index7]))
                                return false;
                        }
                        return true;
                    case SqlNodeType.ClientCase:
                        SqlClientCase sqlClientCase1 = (SqlClientCase)node1;
                        SqlClientCase sqlClientCase2 = (SqlClientCase)node2;
                        if (sqlClientCase1.Whens.Count != sqlClientCase2.Whens.Count)
                            return false;
                        int index8 = 0;
                        for (int count = sqlClientCase1.Whens.Count; index8 < count; ++index8)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlClientCase1.Whens[index8].Match, sqlClientCase2.Whens[index8].Match) || !ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlClientCase1.Whens[index8].Value, sqlClientCase2.Whens[index8].Value))
                                return false;
                        }
                        return true;
                    case SqlNodeType.ClientQuery:
                        SqlClientQuery sqlClientQuery1 = (SqlClientQuery)node1;
                        SqlClientQuery sqlClientQuery2 = (SqlClientQuery)node2;
                        if (sqlClientQuery1.Arguments.Count != sqlClientQuery2.Arguments.Count)
                            return false;
                        int index9 = 0;
                        for (int count = sqlClientQuery1.Arguments.Count; index9 < count; ++index9)
                        {
                            if (!ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(sqlClientQuery1.Arguments[index9], sqlClientQuery2.Arguments[index9]))
                                return false;
                        }
                        return true;
                    case SqlNodeType.ColumnRef:
                        return ((SqlColumnRef)node1).Column.Ordinal == ((SqlColumnRef)node2).Column.Ordinal;
                    case SqlNodeType.DiscriminatedType:
                        return ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(((SqlDiscriminatedType)node1).Discriminator, ((SqlDiscriminatedType)node2).Discriminator);
                    case SqlNodeType.Lift:
                        return ObjectReaderCompiler.SqlProjectionComparer.AreSimilar(((SqlLift)node1).Expression, ((SqlLift)node2).Expression);
                    default:
                        return false;
                }
            }
        }

        private class SideEffectChecker : SqlVisitor
        {
            private bool hasSideEffect;

            internal bool HasSideEffect(SqlNode node)
            {
                this.hasSideEffect = false;
                this.Visit(node);
                return this.hasSideEffect;
            }

            internal override SqlExpression VisitJoinedCollection(SqlJoinedCollection jc)
            {
                this.hasSideEffect = true;
                return (SqlExpression)jc;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                return (SqlExpression)cq;
            }
        }

        private class Generator
        {
            private ObjectReaderCompiler.SideEffectChecker sideEffectChecker = new ObjectReaderCompiler.SideEffectChecker();
            private ObjectReaderCompiler compiler;
            private ILGenerator gen;
            private List<object> globals;
            private List<ObjectReaderCompiler.NamedColumn> namedColumns;
            private LocalBuilder locDataReader;
            private Type elementType;
            private int nLocals;
            private Dictionary<MetaAssociation, int> associationSubQueries;
            private static Type[] readMethodSignature;

            internal object[] Globals
            {
                get
                {
                    return this.globals.ToArray();
                }
            }

            internal ObjectReaderCompiler.NamedColumn[] NamedColumns
            {
                get
                {
                    return this.namedColumns.ToArray();
                }
            }

            internal int Locals
            {
                get
                {
                    return this.nLocals;
                }
            }

            static Generator()
            {
                Type[] typeArray = new Type[1];
                int index = 0;
                Type type = typeof(int);
                typeArray[index] = type;
                ObjectReaderCompiler.Generator.readMethodSignature = typeArray;
            }

            internal Generator(ObjectReaderCompiler compiler, Type elementType)
            {
                this.compiler = compiler;
                this.elementType = elementType;
                this.associationSubQueries = new Dictionary<MetaAssociation, int>();
            }

            internal void GenerateBody(ILGenerator generator, SqlExpression expression)
            {
                this.gen = generator;
                this.globals = new List<object>();
                this.namedColumns = new List<ObjectReaderCompiler.NamedColumn>();
                this.locDataReader = generator.DeclareLocal(this.compiler.dataReaderType);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, this.compiler.readerField);
                generator.Emit(OpCodes.Stloc, this.locDataReader);
                this.GenerateExpressionForType(expression, this.elementType);
                generator.Emit(OpCodes.Ret);
            }

            private Type Generate(SqlNode node)
            {
                return this.Generate(node, (LocalBuilder)null);
            }

            private Type Generate(SqlNode node, LocalBuilder locInstance)
            {
                switch (node.NodeType)
                {
                    case SqlNodeType.Value:
                        return this.GenerateValue((SqlValue)node);
                    case SqlNodeType.ValueOf:
                        return this.GenerateValueOf((SqlUnary)node);
                    case SqlNodeType.TypeCase:
                        return this.GenerateTypeCase((SqlTypeCase)node);
                    case SqlNodeType.UserColumn:
                        return this.GenerateUserColumn((SqlUserColumn)node);
                    case SqlNodeType.OptionalValue:
                        return this.GenerateOptionalValue((SqlOptionalValue)node);
                    case SqlNodeType.OuterJoinedValue:
                        return this.Generate((SqlNode)((SqlUnary)node).Operand);
                    case SqlNodeType.SearchedCase:
                        return this.GenerateSearchedCase((SqlSearchedCase)node);
                    case SqlNodeType.JoinedCollection:
                        return this.GenerateJoinedCollection((SqlJoinedCollection)node);
                    case SqlNodeType.MethodCall:
                        return this.GenerateMethodCall((SqlMethodCall)node);
                    case SqlNodeType.Member:
                        return this.GenerateMember((SqlMember)node);
                    case SqlNodeType.New:
                        return this.GenerateNew((SqlNew)node);
                    case SqlNodeType.Link:
                        return this.GenerateLink((SqlLink)node, locInstance);
                    case SqlNodeType.Grouping:
                        return this.GenerateGrouping((SqlGrouping)node);
                    case SqlNodeType.ClientArray:
                        return this.GenerateClientArray((SqlClientArray)node);
                    case SqlNodeType.ClientCase:
                        return this.GenerateClientCase((SqlClientCase)node, false, locInstance);
                    case SqlNodeType.ClientParameter:
                        return this.GenerateClientParameter((SqlClientParameter)node);
                    case SqlNodeType.ClientQuery:
                        return this.GenerateClientQuery((SqlClientQuery)node, locInstance);
                    case SqlNodeType.ColumnRef:
                        return this.GenerateColumnReference((SqlColumnRef)node);
                    case SqlNodeType.DiscriminatedType:
                        return this.GenerateDiscriminatedType((SqlDiscriminatedType)node);
                    case SqlNodeType.Lift:
                        return this.GenerateLift((SqlLift)node);
                    default:
                        throw Error.CouldNotTranslateExpressionForReading((object)node.SourceExpression);
                }
            }

            private void GenerateAccessBufferReader()
            {
                this.gen.Emit(OpCodes.Ldarg_0);
                this.gen.Emit(OpCodes.Ldfld, this.compiler.bufferReaderField);
            }

            private void GenerateAccessDataReader()
            {
                this.gen.Emit(OpCodes.Ldloc, this.locDataReader);
            }

            private void GenerateAccessOrdinals()
            {
                this.gen.Emit(OpCodes.Ldarg_0);
                this.gen.Emit(OpCodes.Ldfld, this.compiler.ordinalsField);
            }

            private void GenerateAccessGlobals()
            {
                this.gen.Emit(OpCodes.Ldarg_0);
                this.gen.Emit(OpCodes.Ldfld, this.compiler.globalsField);
            }

            private void GenerateAccessArguments()
            {
                this.gen.Emit(OpCodes.Ldarg_0);
                this.gen.Emit(OpCodes.Ldfld, this.compiler.argsField);
            }

            private Type GenerateValue(SqlValue value)
            {
                return this.GenerateConstant(value.ClrType, value.Value);
            }

            private Type GenerateClientParameter(SqlClientParameter cp)
            {
                Delegate @delegate = cp.Accessor.Compile();
                this.GenerateGlobalAccess(this.AddGlobal(@delegate.GetType(), (object)@delegate), @delegate.GetType());
                this.GenerateAccessArguments();
                Type type1 = @delegate.GetType();
                string name = "Invoke";
                int num = 52;
                // ISSUE: variable of the null type
                //__Null local1 = null;
                Type[] types = new Type[1];
                int index = 0;
                Type type2 = typeof(object[]);
                types[index] = type2;
                // ISSUE: variable of the null type
                //__Null local2 = null;
                MethodInfo method = type1.GetMethod(name, (BindingFlags)num, (Binder)null, types, (ParameterModifier[])null);
                this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(method), method);
                return @delegate.Method.ReturnType;
            }

            private Type GenerateValueOf(SqlUnary u)
            {
                this.GenerateExpressionForType(u.Operand, u.Operand.ClrType);
                LocalBuilder local = this.gen.DeclareLocal(u.Operand.ClrType);
                this.gen.Emit(OpCodes.Stloc, local);
                this.gen.Emit(OpCodes.Ldloca, local);
                this.GenerateGetValue(u.Operand.ClrType);
                return u.ClrType;
            }

            private Type GenerateOptionalValue(SqlOptionalValue opt)
            {
                Label label1 = this.gen.DefineLabel();
                Label label2 = this.gen.DefineLabel();
                Type type = this.Generate((SqlNode)opt.HasValue);
                LocalBuilder local = this.gen.DeclareLocal(type);
                this.gen.Emit(OpCodes.Stloc, local);
                this.gen.Emit(OpCodes.Ldloca, local);
                this.GenerateHasValue(type);
                this.gen.Emit(OpCodes.Brfalse, label1);
                this.GenerateExpressionForType(opt.Value, opt.ClrType);
                this.gen.Emit(OpCodes.Br_S, label2);
                this.gen.MarkLabel(label1);
                this.GenerateConstant(opt.ClrType, (object)null);
                this.gen.MarkLabel(label2);
                return opt.ClrType;
            }

            private Type GenerateLift(SqlLift lift)
            {
                return this.GenerateExpressionForType(lift.Expression, lift.ClrType);
            }

            private Type GenerateClientArray(SqlClientArray ca)
            {
                if (!ca.ClrType.IsArray)
                    throw Error.CannotMaterializeList((object)ca.ClrType);
                Type elementType = TypeSystem.GetElementType(ca.ClrType);
                this.GenerateConstInt(ca.Expressions.Count);
                this.gen.Emit(OpCodes.Newarr, elementType);
                int index = 0;
                for (int count = ca.Expressions.Count; index < count; ++index)
                {
                    this.gen.Emit(OpCodes.Dup);
                    this.GenerateConstInt(index);
                    this.GenerateExpressionForType(ca.Expressions[index], elementType);
                    this.GenerateArrayAssign(elementType);
                }
                return ca.ClrType;
            }

            private Type GenerateMember(SqlMember m)
            {
                FieldInfo field = m.Member as FieldInfo;
                if (field != (FieldInfo)null)
                {
                    this.GenerateExpressionForType(m.Expression, m.Expression.ClrType);
                    this.gen.Emit(OpCodes.Ldfld, field);
                    return field.FieldType;
                }
                PropertyInfo propertyInfo = (PropertyInfo)m.Member;
                return this.GenerateMethodCall(new SqlMethodCall(m.ClrType, m.SqlType, propertyInfo.GetGetMethod(), m.Expression, (IEnumerable<SqlExpression>)null, m.SourceExpression));
            }

            private Type GenerateMethodCall(SqlMethodCall mc)
            {
                ParameterInfo[] parameters = mc.Method.GetParameters();
                if (mc.Object != null)
                {
                    Type localType = this.GenerateExpressionForType(mc.Object, mc.Object.ClrType);
                    if (localType.IsValueType)
                    {
                        LocalBuilder local = this.gen.DeclareLocal(localType);
                        this.gen.Emit(OpCodes.Stloc, local);
                        this.gen.Emit(OpCodes.Ldloca, local);
                    }
                }
                int index = 0;
                for (int count = mc.Arguments.Count; index < count; ++index)
                {
                    Type parameterType = parameters[index].ParameterType;
                    if (parameterType.IsByRef)
                    {
                        Type elementType = parameterType.GetElementType();
                        this.GenerateExpressionForType(mc.Arguments[index], elementType);
                        LocalBuilder local = this.gen.DeclareLocal(elementType);
                        this.gen.Emit(OpCodes.Stloc, local);
                        this.gen.Emit(OpCodes.Ldloca, local);
                    }
                    else
                        this.GenerateExpressionForType(mc.Arguments[index], parameterType);
                }
                OpCode methodCallOpCode = ObjectReaderCompiler.Generator.GetMethodCallOpCode(mc.Method);
                if (mc.Object != null && TypeSystem.IsNullableType(mc.Object.ClrType) && methodCallOpCode == OpCodes.Callvirt)
                    this.gen.Emit(OpCodes.Constrained, mc.Object.ClrType);
                this.gen.Emit(methodCallOpCode, mc.Method);
                return mc.Method.ReturnType;
            }

            private static OpCode GetMethodCallOpCode(MethodInfo mi)
            {
                if (!mi.IsStatic && !mi.DeclaringType.IsValueType)
                    return OpCodes.Callvirt;
                return OpCodes.Call;
            }

            private Type GenerateNew(SqlNew sn)
            {
                LocalBuilder localBuilder1 = this.gen.DeclareLocal(sn.ClrType);
                LocalBuilder localBuilder2 = (LocalBuilder)null;
                Label label1 = this.gen.DefineLabel();
                Label label2 = this.gen.DefineLabel();
                if (sn.Args.Count > 0)
                {
                    ParameterInfo[] parameters = sn.Constructor.GetParameters();
                    int index = 0;
                    for (int count = sn.Args.Count; index < count; ++index)
                        this.GenerateExpressionForType(sn.Args[index], parameters[index].ParameterType);
                }
                if (sn.Constructor != (ConstructorInfo)null)
                {
                    this.gen.Emit(OpCodes.Newobj, sn.Constructor);
                    this.gen.Emit(OpCodes.Stloc, localBuilder1);
                }
                else if (sn.ClrType.IsValueType)
                {
                    this.gen.Emit(OpCodes.Ldloca, localBuilder1);
                    this.gen.Emit(OpCodes.Initobj, sn.ClrType);
                }
                else
                {
                    ConstructorInfo constructor = sn.ClrType.GetConstructor(Type.EmptyTypes);
                    this.gen.Emit(OpCodes.Newobj, constructor);
                    this.gen.Emit(OpCodes.Stloc, localBuilder1);
                }
                List<SqlMemberAssign> members1 = sn.Members;
                foreach (SqlMemberAssign sqlMemberAssign in (IEnumerable<SqlMemberAssign>)System.Linq.Enumerable.OrderBy<SqlMemberAssign, int>((IEnumerable<SqlMemberAssign>)members1, (Func<SqlMemberAssign, int>)(m => sn.MetaType.GetDataMember(m.Member).Ordinal)))
                {
                    MetaDataMember dataMember = sn.MetaType.GetDataMember(sqlMemberAssign.Member);
                    if (dataMember.IsPrimaryKey)
                        this.GenerateMemberAssignment(dataMember, localBuilder1, sqlMemberAssign.Expression, (LocalBuilder)null);
                }
                int num1 = 0;
                if (sn.MetaType.IsEntity)
                {
                    LocalBuilder local1 = this.gen.DeclareLocal(sn.ClrType);
                    localBuilder2 = this.gen.DeclareLocal(typeof(bool));
                    Label label3 = this.gen.DefineLabel();
                    num1 = this.AddGlobal(typeof(MetaType), (object)sn.MetaType);
                    Type type1 = typeof(ObjectMaterializer<>);
                    Type[] typeArray = new Type[1];
                    int index1 = 0;
                    Type type2 = this.compiler.dataReaderType;
                    typeArray[index1] = type2;
                    Type type3 = type1.MakeGenericType(typeArray);
                    this.gen.Emit(OpCodes.Ldarg_0);
                    this.GenerateConstInt(num1);
                    this.gen.Emit(OpCodes.Ldloc, localBuilder1);
                    string name = "InsertLookup";
                    int num2 = 52;
                    // ISSUE: variable of the null type
                    //__Null local2 = null;
                    Type[] types = new Type[2];
                    int index2 = 0;
                    Type type4 = typeof(int);
                    types[index2] = type4;
                    int index3 = 1;
                    Type type5 = typeof(object);
                    types[index3] = type5;
                    // ISSUE: variable of the null type
                    //__Null local3 = null;
                    MethodInfo method = type3.GetMethod(name, (BindingFlags)num2, (Binder)null, types, (ParameterModifier[])null);
                    this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(method), method);
                    this.gen.Emit(OpCodes.Castclass, sn.ClrType);
                    this.gen.Emit(OpCodes.Stloc, local1);
                    this.gen.Emit(OpCodes.Ldloc, local1);
                    this.gen.Emit(OpCodes.Ldloc, localBuilder1);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brfalse, label2);
                    this.GenerateConstInt(1);
                    this.gen.Emit(OpCodes.Stloc, localBuilder2);
                    this.gen.Emit(OpCodes.Br_S, label3);
                    this.gen.MarkLabel(label2);
                    this.gen.Emit(OpCodes.Ldloc, local1);
                    this.gen.Emit(OpCodes.Stloc, localBuilder1);
                    this.GenerateConstInt(0);
                    this.gen.Emit(OpCodes.Stloc, localBuilder2);
                    this.gen.MarkLabel(label3);
                }
                List<SqlMemberAssign> members2 = sn.Members;
                foreach (SqlMemberAssign sqlMemberAssign in (IEnumerable<SqlMemberAssign>)System.Linq.Enumerable.OrderBy<SqlMemberAssign, int>((IEnumerable<SqlMemberAssign>)members2, (Func<SqlMemberAssign, int>)(m => sn.MetaType.GetDataMember(m.Member).Ordinal)))
                {
                    MetaDataMember dataMember = sn.MetaType.GetDataMember(sqlMemberAssign.Member);
                    if (!dataMember.IsPrimaryKey)
                        this.GenerateMemberAssignment(dataMember, localBuilder1, sqlMemberAssign.Expression, localBuilder2);
                }
                if (sn.MetaType.IsEntity)
                {
                    this.gen.Emit(OpCodes.Ldloc, localBuilder2);
                    this.GenerateConstInt(0);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brtrue, label1);
                    this.gen.Emit(OpCodes.Ldarg_0);
                    this.GenerateConstInt(num1);
                    this.gen.Emit(OpCodes.Ldloc, localBuilder1);
                    Type type1 = typeof(ObjectMaterializer<>);
                    Type[] typeArray = new Type[1];
                    int index1 = 0;
                    Type type2 = this.compiler.dataReaderType;
                    typeArray[index1] = type2;
                    Type type3 = type1.MakeGenericType(typeArray);
                    string name = "SendEntityMaterialized";
                    int num2 = 52;
                    // ISSUE: variable of the null type
                    //__Null local1 = null;
                    Type[] types = new Type[2];
                    int index2 = 0;
                    Type type4 = typeof(int);
                    types[index2] = type4;
                    int index3 = 1;
                    Type type5 = typeof(object);
                    types[index3] = type5;
                    // ISSUE: variable of the null type
                   // __Null local2 = null;
                    MethodInfo method = type3.GetMethod(name, (BindingFlags)num2, (Binder)null, types, (ParameterModifier[])null);
                    this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(method), method);
                }
                this.gen.MarkLabel(label1);
                this.gen.Emit(OpCodes.Ldloc, localBuilder1);
                return sn.ClrType;
            }

            private void GenerateMemberAssignment(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr, LocalBuilder locStoreInMember)
            {
                Type memberType = TypeSystem.GetMemberType(mm.StorageMember != (MemberInfo)null ? mm.StorageMember : mm.Member);
                if (this.IsDeferrableExpression(expr) && (this.compiler.services.Context.LoadOptions == null || !this.compiler.services.Context.LoadOptions.IsPreloaded(mm.Member)))
                {
                    if (!mm.IsDeferred)
                        return;
                    this.gen.Emit(OpCodes.Ldarg_0);
                    Type type1 = typeof(ObjectMaterializer<>);
                    Type[] typeArray = new Type[1];
                    int index = 0;
                    Type type2 = this.compiler.dataReaderType;
                    typeArray[index] = type2;
                    MethodInfo getMethod = type1.MakeGenericType(typeArray).GetProperty("CanDeferLoad").GetGetMethod();
                    this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(getMethod), getMethod);
                    Label label = this.gen.DefineLabel();
                    this.gen.Emit(OpCodes.Brfalse, label);
                    if (!memberType.IsGenericType)
                        throw Error.DeferredMemberWrongType();
                    Type genericTypeDefinition = memberType.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(EntitySet<>))
                    {
                        this.GenerateAssignDeferredEntitySet(mm, locInstance, expr, locStoreInMember);
                    }
                    else
                    {
                        if (!(genericTypeDefinition == typeof(EntityRef<>)) && !(genericTypeDefinition == typeof(Link<>)))
                            throw Error.DeferredMemberWrongType();
                        this.GenerateAssignDeferredReference(mm, locInstance, expr, locStoreInMember);
                    }
                    this.gen.MarkLabel(label);
                }
                else if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(EntitySet<>))
                    this.GenerateAssignEntitySet(mm, locInstance, expr, locStoreInMember);
                else
                    this.GenerateAssignValue(mm, locInstance, expr, locStoreInMember);
            }

            private void GenerateAssignValue(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr, LocalBuilder locStoreInMember)
            {
                MemberInfo memberInfo = mm.StorageMember != (MemberInfo)null ? mm.StorageMember : mm.Member;
                if (!ObjectReaderCompiler.Generator.IsAssignable(memberInfo))
                    throw Error.CannotAssignToMember((object)memberInfo.Name);
                Type memberType = TypeSystem.GetMemberType(memberInfo);
                Label label = this.gen.DefineLabel();
                bool flag = this.HasSideEffect((SqlNode)expr);
                if (locStoreInMember != null && !flag)
                {
                    this.gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brtrue, label);
                }
                this.GenerateExpressionForType(expr, memberType, mm.DeclaringType.IsEntity ? locInstance : (LocalBuilder)null);
                LocalBuilder local = this.gen.DeclareLocal(memberType);
                this.gen.Emit(OpCodes.Stloc, local);
                if (locStoreInMember != null & flag)
                {
                    this.gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brtrue, label);
                }
                this.GenerateLoadForMemberAccess(locInstance);
                this.gen.Emit(OpCodes.Ldloc, local);
                this.GenerateStoreMember(memberInfo);
                this.gen.MarkLabel(label);
            }

            private static bool IsAssignable(MemberInfo member)
            {
                if (member as FieldInfo != (FieldInfo)null)
                    return true;
                PropertyInfo propertyInfo = member as PropertyInfo;
                if (propertyInfo != (PropertyInfo)null)
                    return propertyInfo.CanWrite;
                return false;
            }

            private void GenerateAssignDeferredEntitySet(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr, LocalBuilder locStoreInMember)
            {
                MemberInfo mi = mm.StorageMember != (MemberInfo)null ? mm.StorageMember : mm.Member;
                Type memberType = TypeSystem.GetMemberType(mi);
                Label label1 = this.gen.DefineLabel();
                Type type1 = typeof(IEnumerable<>).MakeGenericType(memberType.GetGenericArguments());
                bool flag = this.HasSideEffect((SqlNode)expr);
                if (locStoreInMember != null && !flag)
                {
                    this.gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brtrue, label1);
                }
                LocalBuilder local1 = this.gen.DeclareLocal(this.GenerateDeferredSource(expr, locInstance));
                this.gen.Emit(OpCodes.Stloc, local1);
                if (locStoreInMember != null & flag)
                {
                    this.gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brtrue, label1);
                }
                if (mi is FieldInfo || mi is PropertyInfo && ((PropertyInfo)mi).CanWrite)
                {
                    Label label2 = this.gen.DefineLabel();
                    this.GenerateLoadForMemberAccess(locInstance);
                    this.GenerateLoadMember(mi);
                    this.gen.Emit(OpCodes.Ldnull);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brfalse, label2);
                    this.GenerateLoadForMemberAccess(locInstance);
                    ConstructorInfo constructor = memberType.GetConstructor(Type.EmptyTypes);
                    this.gen.Emit(OpCodes.Newobj, constructor);
                    this.GenerateStoreMember(mi);
                    this.gen.MarkLabel(label2);
                }
                this.GenerateLoadForMemberAccess(locInstance);
                this.GenerateLoadMember(mi);
                this.gen.Emit(OpCodes.Ldloc, local1);
                Type type2 = memberType;
                string name = "SetSource";
                int num = 52;
                // ISSUE: variable of the null type
                //__Null local2 = null;
                Type[] types = new Type[1];
                int index = 0;
                Type type3 = type1;
                types[index] = type3;
                // ISSUE: variable of the null type
                //__Null local3 = null;
                MethodInfo method = type2.GetMethod(name, (BindingFlags)num, (Binder)null, types, (ParameterModifier[])null);
                this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(method), method);
                this.gen.MarkLabel(label1);
            }

            private bool HasSideEffect(SqlNode node)
            {
                return this.sideEffectChecker.HasSideEffect(node);
            }

            private void GenerateAssignEntitySet(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr, LocalBuilder locStoreInMember)
            {
                MemberInfo mi = mm.StorageMember != (MemberInfo)null ? mm.StorageMember : mm.Member;
                Type memberType = TypeSystem.GetMemberType(mi);
                Label label1 = this.gen.DefineLabel();
                Type type1 = typeof(IEnumerable<>).MakeGenericType(memberType.GetGenericArguments());
                bool flag = this.HasSideEffect((SqlNode)expr);
                if (locStoreInMember != null && !flag)
                {
                    this.gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brtrue, label1);
                }
                LocalBuilder local1 = this.gen.DeclareLocal(this.Generate((SqlNode)expr, mm.DeclaringType.IsEntity ? locInstance : (LocalBuilder)null));
                this.gen.Emit(OpCodes.Stloc, local1);
                if (locStoreInMember != null & flag)
                {
                    this.gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brtrue, label1);
                }
                if (mi is FieldInfo || mi is PropertyInfo && ((PropertyInfo)mi).CanWrite)
                {
                    Label label2 = this.gen.DefineLabel();
                    this.GenerateLoadForMemberAccess(locInstance);
                    this.GenerateLoadMember(mi);
                    this.gen.Emit(OpCodes.Ldnull);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brfalse, label2);
                    this.GenerateLoadForMemberAccess(locInstance);
                    ConstructorInfo constructor = memberType.GetConstructor(Type.EmptyTypes);
                    this.gen.Emit(OpCodes.Newobj, constructor);
                    this.GenerateStoreMember(mi);
                    this.gen.MarkLabel(label2);
                }
                this.GenerateLoadForMemberAccess(locInstance);
                this.GenerateLoadMember(mi);
                this.gen.Emit(OpCodes.Ldloc, local1);
                Type type2 = memberType;
                string name = "Assign";
                int num = 52;
                // ISSUE: variable of the null type
                //__Null local2 = null;
                Type[] types = new Type[1];
                int index = 0;
                Type type3 = type1;
                types[index] = type3;
                // ISSUE: variable of the null type
                //__Null local3 = null;
                MethodInfo method = type2.GetMethod(name, (BindingFlags)num, (Binder)null, types, (ParameterModifier[])null);
                this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(method), method);
                this.gen.MarkLabel(label1);
            }

            private void GenerateAssignDeferredReference(MetaDataMember mm, LocalBuilder locInstance, SqlExpression expr, LocalBuilder locStoreInMember)
            {
                MemberInfo mi = mm.StorageMember != (MemberInfo)null ? mm.StorageMember : mm.Member;
                Type memberType = TypeSystem.GetMemberType(mi);
                Label label = this.gen.DefineLabel();
                Type type1 = typeof(IEnumerable<>).MakeGenericType(memberType.GetGenericArguments());
                bool flag = this.HasSideEffect((SqlNode)expr);
                if (locStoreInMember != null && !flag)
                {
                    this.gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brtrue, label);
                }
                Type type2 = this.GenerateDeferredSource(expr, locInstance);
                if (!type1.IsAssignableFrom(type2))
                    throw Error.CouldNotConvert((object)type1, (object)type2);
                LocalBuilder local1 = this.gen.DeclareLocal(type2);
                this.gen.Emit(OpCodes.Stloc, local1);
                if (locStoreInMember != null & flag)
                {
                    this.gen.Emit(OpCodes.Ldloc, locStoreInMember);
                    this.GenerateConstInt(0);
                    this.gen.Emit(OpCodes.Ceq);
                    this.gen.Emit(OpCodes.Brtrue, label);
                }
                this.GenerateLoadForMemberAccess(locInstance);
                this.gen.Emit(OpCodes.Ldloc, local1);
                Type type3 = memberType;
                int num = 52;
                // ISSUE: variable of the null type
                //__Null local2 = null;
                Type[] types = new Type[1];
                int index = 0;
                Type type4 = type1;
                types[index] = type4;
                // ISSUE: variable of the null type
                //__Null local3 = null;
                ConstructorInfo constructor = type3.GetConstructor((BindingFlags)num, (Binder)null, types, (ParameterModifier[])null);
                this.gen.Emit(OpCodes.Newobj, constructor);
                this.GenerateStoreMember(mi);
                this.gen.MarkLabel(label);
            }

            private void GenerateLoadForMemberAccess(LocalBuilder loc)
            {
                if (loc.LocalType.IsValueType)
                    this.gen.Emit(OpCodes.Ldloca, loc);
                else
                    this.gen.Emit(OpCodes.Ldloc, loc);
            }

            private bool IsDeferrableExpression(SqlExpression expr)
            {
                if (expr.NodeType == SqlNodeType.Link)
                    return true;
                if (expr.NodeType != SqlNodeType.ClientCase)
                    return false;
                foreach (SqlClientWhen sqlClientWhen in ((SqlClientCase)expr).Whens)
                {
                    if (!this.IsDeferrableExpression(sqlClientWhen.Value))
                        return false;
                }
                return true;
            }

            private Type GenerateGrouping(SqlGrouping grp)
            {
                Type[] genericArguments = grp.ClrType.GetGenericArguments();
                this.GenerateExpressionForType(grp.Key, genericArguments[0]);
                this.Generate((SqlNode)grp.Group);
                Type type1 = typeof(ObjectMaterializer<>);
                Type[] typeArray1 = new Type[1];
                int index1 = 0;
                Type type2 = this.compiler.dataReaderType;
                typeArray1[index1] = type2;
                Type type3 = type1.MakeGenericType(typeArray1);
                string name = "CreateGroup";
                Type[] args = new Type[2];
                int index2 = 0;
                Type type4 = genericArguments[0];
                args[index2] = type4;
                int index3 = 1;
                Type type5 = typeof(IEnumerable<>);
                Type[] typeArray2 = new Type[1];
                int index4 = 0;
                Type type6 = genericArguments[1];
                typeArray2[index4] = type6;
                Type type7 = type5.MakeGenericType(typeArray2);
                args[index3] = type7;
                Type[] typeArray3 = genericArguments;
                MethodInfo staticMethod = TypeSystem.FindStaticMethod(type3, name, args, typeArray3);
                this.gen.Emit(OpCodes.Call, staticMethod);
                return staticMethod.ReturnType;
            }

            private Type GenerateLink(SqlLink link, LocalBuilder locInstance)
            {
                this.gen.Emit(OpCodes.Ldarg_0);
                this.GenerateConstInt(this.AddGlobal(typeof(MetaDataMember), (object)link.Member));
                this.GenerateConstInt(this.AllocateLocal());
                Type type1 = !link.Member.IsAssociation || !link.Member.Association.IsMany ? link.Member.Type : TypeSystem.GetElementType(link.Member.Type);
                if (locInstance != null)
                {
                    this.gen.Emit(OpCodes.Ldloc, locInstance);
                    Type type2 = typeof(ObjectMaterializer<>);
                    Type[] typeArray1 = new Type[1];
                    int index1 = 0;
                    Type type3 = this.compiler.dataReaderType;
                    typeArray1[index1] = type3;
                    MethodInfo method = type2.MakeGenericType(typeArray1).GetMethod("GetNestedLinkSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    Type[] typeArray2 = new Type[1];
                    int index2 = 0;
                    Type type4 = type1;
                    typeArray2[index2] = type4;
                    MethodInfo methodInfo = method.MakeGenericMethod(typeArray2);
                    this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(methodInfo), methodInfo);
                }
                else
                {
                    this.GenerateConstInt(link.KeyExpressions.Count);
                    this.gen.Emit(OpCodes.Newarr, typeof(object));
                    int index1 = 0;
                    for (int count = link.KeyExpressions.Count; index1 < count; ++index1)
                    {
                        this.gen.Emit(OpCodes.Dup);
                        this.GenerateConstInt(index1);
                        this.GenerateExpressionForType(link.KeyExpressions[index1], typeof(object));
                        this.GenerateArrayAssign(typeof(object));
                    }
                    Type type2 = typeof(ObjectMaterializer<>);
                    Type[] typeArray1 = new Type[1];
                    int index2 = 0;
                    Type type3 = this.compiler.dataReaderType;
                    typeArray1[index2] = type3;
                    MethodInfo method = type2.MakeGenericType(typeArray1).GetMethod("GetLinkSource", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    Type[] typeArray2 = new Type[1];
                    int index3 = 0;
                    Type type4 = type1;
                    typeArray2[index3] = type4;
                    MethodInfo methodInfo = method.MakeGenericMethod(typeArray2);
                    this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(methodInfo), methodInfo);
                }
                Type type5 = typeof(IEnumerable<>);
                Type[] typeArray = new Type[1];
                int index = 0;
                Type type6 = type1;
                typeArray[index] = type6;
                return type5.MakeGenericType(typeArray);
            }

            private Type GenerateDeferredSource(SqlExpression expr, LocalBuilder locInstance)
            {
                if (expr.NodeType == SqlNodeType.ClientCase)
                    return this.GenerateClientCase((SqlClientCase)expr, true, locInstance);
                if (expr.NodeType == SqlNodeType.Link)
                    return this.GenerateLink((SqlLink)expr, locInstance);
                throw Error.ExpressionNotDeferredQuerySource();
            }

            private Type GenerateClientQuery(SqlClientQuery cq, LocalBuilder locInstance)
            {
                Type type1 = cq.Query.NodeType == SqlNodeType.Multiset ? TypeSystem.GetElementType(cq.ClrType) : cq.ClrType;
                this.gen.Emit(OpCodes.Ldarg_0);
                this.GenerateConstInt(cq.Ordinal);
                this.GenerateConstInt(cq.Arguments.Count);
                this.gen.Emit(OpCodes.Newarr, typeof(object));
                int index1 = 0;
                for (int count = cq.Arguments.Count; index1 < count; ++index1)
                {
                    this.gen.Emit(OpCodes.Dup);
                    this.GenerateConstInt(index1);
                    Type type2 = cq.Arguments[index1].ClrType;
                    if (cq.Arguments[index1].NodeType == SqlNodeType.ColumnRef)
                    {
                        SqlColumnRef sqlColumnRef = (SqlColumnRef)cq.Arguments[index1];
                        if (type2.IsValueType && !TypeSystem.IsNullableType(type2))
                        {
                            Type type3 = typeof(Nullable<>);
                            Type[] typeArray = new Type[1];
                            int index2 = 0;
                            Type type4 = type2;
                            typeArray[index2] = type4;
                            type2 = type3.MakeGenericType(typeArray);
                        }
                        this.GenerateColumnAccess(type2, sqlColumnRef.SqlType, sqlColumnRef.Column.Ordinal, (LocalBuilder)null);
                    }
                    else
                        this.GenerateExpressionForType(cq.Arguments[index1], cq.Arguments[index1].ClrType);
                    if (type2.IsValueType)
                        this.gen.Emit(OpCodes.Box, type2);
                    this.GenerateArrayAssign(typeof(object));
                }
                Type type5 = typeof(ObjectMaterializer<>);
                Type[] typeArray1 = new Type[1];
                int index3 = 0;
                Type type6 = this.compiler.dataReaderType;
                typeArray1[index3] = type6;
                MethodInfo method = type5.MakeGenericType(typeArray1).GetMethod("ExecuteSubQuery", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(method), method);
                Type type7 = typeof(IEnumerable<>);
                Type[] typeArray2 = new Type[1];
                int index4 = 0;
                Type type8 = type1;
                typeArray2[index4] = type8;
                Type type9 = type7.MakeGenericType(typeArray2);
                this.gen.Emit(OpCodes.Castclass, type9);
                Type type10 = typeof(List<>);
                Type[] typeArray3 = new Type[1];
                int index5 = 0;
                Type type11 = type1;
                typeArray3[index5] = type11;
                Type expectedType = type10.MakeGenericType(typeArray3);
                this.GenerateConvertToType(type9, expectedType);
                return expectedType;
            }

            private Type GenerateJoinedCollection(SqlJoinedCollection jc)
            {
                LocalBuilder local1 = this.gen.DeclareLocal(typeof(int));
                LocalBuilder local2 = this.gen.DeclareLocal(typeof(bool));
                Type clrType = jc.Expression.ClrType;
                Type type1 = typeof(List<>);
                Type[] typeArray1 = new Type[1];
                int index1 = 0;
                Type type2 = clrType;
                typeArray1[index1] = type2;
                Type localType = type1.MakeGenericType(typeArray1);
                LocalBuilder local3 = this.gen.DeclareLocal(localType);
                this.GenerateExpressionForType(jc.Count, typeof(int));
                this.gen.Emit(OpCodes.Stloc, local1);
                this.gen.Emit(OpCodes.Ldloc, local1);
                Type type3 = localType;
                Type[] types1 = new Type[1];
                int index2 = 0;
                Type type4 = typeof(int);
                types1[index2] = type4;
                ConstructorInfo constructor = type3.GetConstructor(types1);
                this.gen.Emit(OpCodes.Newobj, constructor);
                this.gen.Emit(OpCodes.Stloc, local3);
                this.gen.Emit(OpCodes.Ldc_I4_1);
                this.gen.Emit(OpCodes.Stloc, local2);
                Label label1 = this.gen.DefineLabel();
                Label label2 = this.gen.DefineLabel();
                LocalBuilder local4 = this.gen.DeclareLocal(typeof(int));
                this.gen.Emit(OpCodes.Ldc_I4_0);
                this.gen.Emit(OpCodes.Stloc, local4);
                this.gen.Emit(OpCodes.Br, label1);
                this.gen.MarkLabel(label2);
                this.gen.Emit(OpCodes.Ldloc, local4);
                this.gen.Emit(OpCodes.Ldc_I4_0);
                this.gen.Emit(OpCodes.Cgt);
                this.gen.Emit(OpCodes.Ldloc, local2);
                this.gen.Emit(OpCodes.And);
                Label label3 = this.gen.DefineLabel();
                this.gen.Emit(OpCodes.Brfalse, label3);
                this.gen.Emit(OpCodes.Ldarg_0);
                Type type5 = typeof(ObjectMaterializer<>);
                Type[] typeArray2 = new Type[1];
                int index3 = 0;
                Type type6 = this.compiler.dataReaderType;
                typeArray2[index3] = type6;
                MethodInfo method1 = type5.MakeGenericType(typeArray2).GetMethod("Read", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, Type.EmptyTypes, (ParameterModifier[])null);
                this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(method1), method1);
                this.gen.Emit(OpCodes.Stloc, local2);
                this.gen.MarkLabel(label3);
                Label label4 = this.gen.DefineLabel();
                this.gen.Emit(OpCodes.Ldloc, local2);
                this.gen.Emit(OpCodes.Brfalse, label4);
                this.gen.Emit(OpCodes.Ldloc, local3);
                this.GenerateExpressionForType(jc.Expression, clrType);
                Type type7 = localType;
                string name = "Add";
                int num = 20;
                // ISSUE: variable of the null type
                //__Null local5 = null;
                Type[] types2 = new Type[1];
                int index4 = 0;
                Type type8 = clrType;
                types2[index4] = type8;
                // ISSUE: variable of the null type
                //__Null local6 = null;
                MethodInfo method2 = type7.GetMethod(name, (BindingFlags)num, (Binder)null, types2, (ParameterModifier[])null);
                this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(method2), method2);
                this.gen.MarkLabel(label4);
                this.gen.Emit(OpCodes.Ldloc, local4);
                this.gen.Emit(OpCodes.Ldc_I4_1);
                this.gen.Emit(OpCodes.Add);
                this.gen.Emit(OpCodes.Stloc, local4);
                this.gen.MarkLabel(label1);
                this.gen.Emit(OpCodes.Ldloc, local4);
                this.gen.Emit(OpCodes.Ldloc, local1);
                this.gen.Emit(OpCodes.Clt);
                this.gen.Emit(OpCodes.Ldloc, local2);
                this.gen.Emit(OpCodes.And);
                this.gen.Emit(OpCodes.Brtrue, label2);
                this.gen.Emit(OpCodes.Ldloc, local3);
                return localType;
            }

            private Type GenerateExpressionForType(SqlExpression expr, Type type)
            {
                return this.GenerateExpressionForType(expr, type, (LocalBuilder)null);
            }

            private Type GenerateExpressionForType(SqlExpression expr, Type type, LocalBuilder locInstance)
            {
                this.GenerateConvertToType(this.Generate((SqlNode)expr, locInstance), type);
                return type;
            }

            private void GenerateConvertToType(Type actualType, Type expectedType, Type readerMethodType)
            {
                this.GenerateConvertToType(readerMethodType, actualType);
                this.GenerateConvertToType(actualType, expectedType);
            }

            private void GenerateConvertToType(Type actualType, Type expectedType)
            {
                if (!(expectedType != actualType) || !actualType.IsValueType && actualType.IsSubclassOf(expectedType))
                    return;
                if (actualType.IsGenericType)
                    actualType.GetGenericTypeDefinition();
                Type type1 = expectedType.IsGenericType ? expectedType.GetGenericTypeDefinition() : (Type)null;
                Type[] typeArray1 = type1 != (Type)null ? expectedType.GetGenericArguments() : (Type[])null;
                Type type2 = TypeSystem.GetElementType(actualType);
                Type expectedType1 = TypeSystem.GetSequenceType(type2);
                bool flag = expectedType1.IsAssignableFrom(actualType);
                if (expectedType == typeof(object) && actualType.IsValueType)
                    this.gen.Emit(OpCodes.Box, actualType);
                else if (actualType == typeof(object) && expectedType.IsValueType)
                    this.gen.Emit(OpCodes.Unbox_Any, expectedType);
                else if ((actualType.IsSubclassOf(expectedType) || expectedType.IsSubclassOf(actualType)) && (!actualType.IsValueType && !expectedType.IsValueType))
                    this.gen.Emit(OpCodes.Castclass, expectedType);
                else if (type1 == typeof(IEnumerable<>) & flag)
                {
                    if (this.elementType.IsInterface || typeArray1[0].IsInterface || (this.elementType.IsSubclassOf(typeArray1[0]) || typeArray1[0].IsSubclassOf(this.elementType)) || TypeSystem.GetNonNullableType(this.elementType) == TypeSystem.GetNonNullableType(typeArray1[0]))
                    {
                        string name = "Cast";
                        Type[] args = new Type[1];
                        int index1 = 0;
                        Type type3 = expectedType1;
                        args[index1] = type3;
                        Type[] typeArray2 = new Type[1];
                        int index2 = 0;
                        Type type4 = typeArray1[0];
                        typeArray2[index2] = type4;
                        MethodInfo sequenceMethod = TypeSystem.FindSequenceMethod(name, args, typeArray2);
                        this.gen.Emit(OpCodes.Call, sequenceMethod);
                    }
                    else
                    {
                        Type type3 = typeof(ObjectMaterializer<>);
                        Type[] typeArray2 = new Type[1];
                        int index1 = 0;
                        Type type4 = this.compiler.dataReaderType;
                        typeArray2[index1] = type4;
                        Type type5 = type3.MakeGenericType(typeArray2);
                        string name = "Convert";
                        Type[] args = new Type[1];
                        int index2 = 0;
                        Type type6 = expectedType1;
                        args[index2] = type6;
                        Type[] typeArray3 = new Type[1];
                        int index3 = 0;
                        Type type7 = typeArray1[0];
                        typeArray3[index3] = type7;
                        MethodInfo staticMethod = TypeSystem.FindStaticMethod(type5, name, args, typeArray3);
                        this.gen.Emit(OpCodes.Call, staticMethod);
                    }
                }
                else if (expectedType == type2 & flag)
                {
                    string name = "SingleOrDefault";
                    Type[] args = new Type[1];
                    int index1 = 0;
                    Type type3 = expectedType1;
                    args[index1] = type3;
                    Type[] typeArray2 = new Type[1];
                    int index2 = 0;
                    Type type4 = expectedType;
                    typeArray2[index2] = type4;
                    MethodInfo sequenceMethod = TypeSystem.FindSequenceMethod(name, args, typeArray2);
                    this.gen.Emit(OpCodes.Call, sequenceMethod);
                }
                else if (TypeSystem.IsNullableType(expectedType) && TypeSystem.GetNonNullableType(expectedType) == actualType)
                {
                    Type type3 = expectedType;
                    Type[] types = new Type[1];
                    int index = 0;
                    Type type4 = actualType;
                    types[index] = type4;
                    ConstructorInfo constructor = type3.GetConstructor(types);
                    this.gen.Emit(OpCodes.Newobj, constructor);
                }
                else if (TypeSystem.IsNullableType(actualType) && TypeSystem.GetNonNullableType(actualType) == expectedType)
                {
                    LocalBuilder local = this.gen.DeclareLocal(actualType);
                    this.gen.Emit(OpCodes.Stloc, local);
                    this.gen.Emit(OpCodes.Ldloca, local);
                    this.GenerateGetValueOrDefault(actualType);
                }
                else if (type1 == typeof(EntityRef<>) || type1 == typeof(Link<>))
                {
                    if (actualType.IsAssignableFrom(typeArray1[0]))
                    {
                        if (actualType != typeArray1[0])
                            this.GenerateConvertToType(actualType, typeArray1[0]);
                        Type type3 = expectedType;
                        Type[] types = new Type[1];
                        int index = 0;
                        Type type4 = typeArray1[0];
                        types[index] = type4;
                        ConstructorInfo constructor = type3.GetConstructor(types);
                        this.gen.Emit(OpCodes.Newobj, constructor);
                    }
                    else
                    {
                        if (!expectedType1.IsAssignableFrom(actualType))
                            throw Error.CannotConvertToEntityRef((object)actualType);
                        string name = "SingleOrDefault";
                        Type[] args = new Type[1];
                        int index1 = 0;
                        Type type3 = expectedType1;
                        args[index1] = type3;
                        Type[] typeArray2 = new Type[1];
                        int index2 = 0;
                        Type type4 = type2;
                        typeArray2[index2] = type4;
                        MethodInfo sequenceMethod = TypeSystem.FindSequenceMethod(name, args, typeArray2);
                        this.gen.Emit(OpCodes.Call, sequenceMethod);
                        Type type5 = expectedType;
                        Type[] types = new Type[1];
                        int index3 = 0;
                        Type type6 = type2;
                        types[index3] = type6;
                        ConstructorInfo constructor = type5.GetConstructor(types);
                        this.gen.Emit(OpCodes.Newobj, constructor);
                    }
                }
                else if ((expectedType == typeof(IQueryable) || expectedType == typeof(IOrderedQueryable)) && typeof(IEnumerable).IsAssignableFrom(actualType))
                {
                    string name = "AsQueryable";
                    Type[] args = new Type[1];
                    int index = 0;
                    Type type3 = typeof(IEnumerable);
                    args[index] = type3;
                    Type[] typeArray2 = new Type[0];
                    MethodInfo queryableMethod = TypeSystem.FindQueryableMethod(name, args, typeArray2);
                    this.gen.Emit(OpCodes.Call, queryableMethod);
                    if (!(type1 == typeof(IOrderedQueryable)))
                        return;
                    this.gen.Emit(OpCodes.Castclass, expectedType);
                }
                else if (((type1 == typeof(IQueryable<>) ? 1 : (type1 == typeof(IOrderedQueryable<>) ? 1 : 0)) & (flag ? 1 : 0)) != 0)
                {
                    if (type2 != typeArray1[0])
                    {
                        expectedType1 = typeof(IEnumerable<>).MakeGenericType(typeArray1);
                        this.GenerateConvertToType(actualType, expectedType1);
                        type2 = typeArray1[0];
                    }
                    string name = "AsQueryable";
                    Type[] args = new Type[1];
                    int index1 = 0;
                    Type type3 = expectedType1;
                    args[index1] = type3;
                    Type[] typeArray2 = new Type[1];
                    int index2 = 0;
                    Type type4 = type2;
                    typeArray2[index2] = type4;
                    MethodInfo queryableMethod = TypeSystem.FindQueryableMethod(name, args, typeArray2);
                    this.gen.Emit(OpCodes.Call, queryableMethod);
                    if (!(type1 == typeof(IOrderedQueryable<>)))
                        return;
                    this.gen.Emit(OpCodes.Castclass, expectedType);
                }
                else if (type1 == typeof(IOrderedEnumerable<>) & flag)
                {
                    if (type2 != typeArray1[0])
                    {
                        expectedType1 = typeof(IEnumerable<>).MakeGenericType(typeArray1);
                        this.GenerateConvertToType(actualType, expectedType1);
                        type2 = typeArray1[0];
                    }
                    Type type3 = typeof(ObjectMaterializer<>);
                    Type[] typeArray2 = new Type[1];
                    int index1 = 0;
                    Type type4 = this.compiler.dataReaderType;
                    typeArray2[index1] = type4;
                    Type type5 = type3.MakeGenericType(typeArray2);
                    string name = "CreateOrderedEnumerable";
                    Type[] args = new Type[1];
                    int index2 = 0;
                    Type type6 = expectedType1;
                    args[index2] = type6;
                    Type[] typeArray3 = new Type[1];
                    int index3 = 0;
                    Type type7 = type2;
                    typeArray3[index3] = type7;
                    MethodInfo staticMethod = TypeSystem.FindStaticMethod(type5, name, args, typeArray3);
                    this.gen.Emit(OpCodes.Call, staticMethod);
                }
                else if (type1 == typeof(EntitySet<>) & flag)
                {
                    if (type2 != typeArray1[0])
                    {
                        expectedType1 = typeof(IEnumerable<>).MakeGenericType(typeArray1);
                        this.GenerateConvertToType(actualType, expectedType1);
                        actualType = expectedType1;
                        Type type3 = typeArray1[0];
                    }
                    LocalBuilder local1 = this.gen.DeclareLocal(actualType);
                    this.gen.Emit(OpCodes.Stloc, local1);
                    ConstructorInfo constructor = expectedType.GetConstructor(Type.EmptyTypes);
                    this.gen.Emit(OpCodes.Newobj, constructor);
                    LocalBuilder local2 = this.gen.DeclareLocal(expectedType);
                    this.gen.Emit(OpCodes.Stloc, local2);
                    this.gen.Emit(OpCodes.Ldloc, local2);
                    this.gen.Emit(OpCodes.Ldloc, local1);
                    Type type4 = expectedType;
                    string name = "Assign";
                    int num = 52;
                    // ISSUE: variable of the null type
                    //__Null local3 = null;
                    Type[] types = new Type[1];
                    int index = 0;
                    Type type5 = expectedType1;
                    types[index] = type5;
                    // ISSUE: variable of the null type
                    //__Null local4 = null;
                    MethodInfo method = type4.GetMethod(name, (BindingFlags)num, (Binder)null, types, (ParameterModifier[])null);
                    this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(method), method);
                    this.gen.Emit(OpCodes.Ldloc, local2);
                }
                else
                {
                    if (typeof(IEnumerable).IsAssignableFrom(expectedType) & flag)
                    {
                        Type type3 = expectedType;
                        Type type4 = typeof(List<>);
                        Type[] typeArray2 = new Type[1];
                        int index1 = 0;
                        Type type5 = type2;
                        typeArray2[index1] = type5;
                        Type c = type4.MakeGenericType(typeArray2);
                        if (type3.IsAssignableFrom(c))
                        {
                            Type type6 = typeof(List<>);
                            Type[] typeArray3 = new Type[1];
                            int index2 = 0;
                            Type type7 = type2;
                            typeArray3[index2] = type7;
                            Type type8 = type6.MakeGenericType(typeArray3);
                            Type[] types = new Type[1];
                            int index3 = 0;
                            Type type9 = expectedType1;
                            types[index3] = type9;
                            ConstructorInfo constructor = type8.GetConstructor(types);
                            this.gen.Emit(OpCodes.Newobj, constructor);
                            return;
                        }
                    }
                    if (expectedType.IsArray && expectedType.GetArrayRank() == 1 && (!actualType.IsArray && expectedType1.IsAssignableFrom(actualType)) && expectedType.GetElementType().IsAssignableFrom(type2))
                    {
                        string name = "ToArray";
                        Type[] args = new Type[1];
                        int index1 = 0;
                        Type type3 = expectedType1;
                        args[index1] = type3;
                        Type[] typeArray2 = new Type[1];
                        int index2 = 0;
                        Type type4 = type2;
                        typeArray2[index2] = type4;
                        MethodInfo sequenceMethod = TypeSystem.FindSequenceMethod(name, args, typeArray2);
                        this.gen.Emit(OpCodes.Call, sequenceMethod);
                    }
                    else
                    {
                        if (expectedType.IsClass)
                        {
                            Type type3 = typeof(ICollection<>);
                            Type[] typeArray2 = new Type[1];
                            int index = 0;
                            Type type4 = type2;
                            typeArray2[index] = type4;
                            if (type3.MakeGenericType(typeArray2).IsAssignableFrom(expectedType) && expectedType.GetConstructor(Type.EmptyTypes) != (ConstructorInfo)null && expectedType1.IsAssignableFrom(actualType))
                                throw Error.GeneralCollectionMaterializationNotSupported();
                        }
                        if (expectedType == typeof(bool) && actualType == typeof(int))
                        {
                            Label label1 = this.gen.DefineLabel();
                            Label label2 = this.gen.DefineLabel();
                            this.gen.Emit(OpCodes.Ldc_I4_0);
                            this.gen.Emit(OpCodes.Ceq);
                            this.gen.Emit(OpCodes.Brtrue_S, label1);
                            this.gen.Emit(OpCodes.Ldc_I4_1);
                            this.gen.Emit(OpCodes.Br_S, label2);
                            this.gen.MarkLabel(label1);
                            this.gen.Emit(OpCodes.Ldc_I4_0);
                            this.gen.MarkLabel(label2);
                        }
                        else
                        {
                            if (actualType.IsValueType)
                                this.gen.Emit(OpCodes.Box, actualType);
                            this.gen.Emit(OpCodes.Ldtoken, expectedType);
                            MethodInfo method1 = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public);
                            this.gen.Emit(OpCodes.Call, method1);
                            Type type3 = typeof(DBConvert);
                            string name = "ChangeType";
                            int num = 24;
                            // ISSUE: variable of the null type
                            //__Null local1 = null;
                            Type[] types = new Type[2];
                            int index1 = 0;
                            Type type4 = typeof(object);
                            types[index1] = type4;
                            int index2 = 1;
                            Type type5 = typeof(Type);
                            types[index2] = type5;
                            // ISSUE: variable of the null type
                            //__Null local2 = null;
                            // ISSUE: explicit non-virtual call
                            //MethodInfo method2 = type4.GetMethod(type3.GetMethod(name, (BindingFlags)num, null, types, null));
                            //this.gen.Emit(OpCodes.Call, method2);
                            if (expectedType.IsValueType)
                            {
                                this.gen.Emit(OpCodes.Unbox_Any, expectedType);
                            }
                            else
                            {
                                if (!(expectedType != typeof(object)))
                                    return;
                                this.gen.Emit(OpCodes.Castclass, expectedType);
                            }
                        }
                    }
                }
            }

            private Type GenerateColumnReference(SqlColumnRef cref)
            {
                this.GenerateColumnAccess(cref.ClrType, cref.SqlType, cref.Column.Ordinal, (LocalBuilder)null);
                return cref.ClrType;
            }

            private Type GenerateUserColumn(SqlUserColumn suc)
            {
                if (string.IsNullOrEmpty(suc.Name))
                {
                    this.GenerateColumnAccess(suc.ClrType, suc.SqlType, 0, (LocalBuilder)null);
                    return suc.ClrType;
                }
                int count = this.namedColumns.Count;
                this.namedColumns.Add(new ObjectReaderCompiler.NamedColumn(suc.Name, suc.IsRequired));
                Label label1 = this.gen.DefineLabel();
                Label label2 = this.gen.DefineLabel();
                LocalBuilder localBuilder = this.gen.DeclareLocal(typeof(int));
                this.GenerateAccessOrdinals();
                this.GenerateConstInt(count);
                this.GenerateArrayAccess(typeof(int), false);
                this.gen.Emit(OpCodes.Stloc, localBuilder);
                this.gen.Emit(OpCodes.Ldloc, localBuilder);
                this.GenerateConstInt(0);
                this.gen.Emit(OpCodes.Clt);
                this.gen.Emit(OpCodes.Brtrue, label1);
                this.GenerateColumnAccess(suc.ClrType, suc.SqlType, 0, localBuilder);
                this.gen.Emit(OpCodes.Br_S, label2);
                this.gen.MarkLabel(label1);
                this.GenerateDefault(suc.ClrType, false);
                this.gen.MarkLabel(label2);
                return suc.ClrType;
            }

            private void GenerateColumnAccess(Type cType, ProviderType pType, int ordinal, LocalBuilder locOrdinal)
            {
                Type closestRuntimeType = pType.GetClosestRuntimeType();
                MethodInfo readerMethod1 = this.GetReaderMethod(this.compiler.dataReaderType, closestRuntimeType);
                MethodInfo readerMethod2 = this.GetReaderMethod(typeof(DbDataReader), closestRuntimeType);
                Label label1 = this.gen.DefineLabel();
                Label label2 = this.gen.DefineLabel();
                Label label3 = this.gen.DefineLabel();
                this.GenerateAccessBufferReader();
                this.gen.Emit(OpCodes.Ldnull);
                this.gen.Emit(OpCodes.Ceq);
                this.gen.Emit(OpCodes.Brfalse, label3);
                this.GenerateAccessDataReader();
                if (locOrdinal != null)
                    this.gen.Emit(OpCodes.Ldloc, locOrdinal);
                else
                    this.GenerateConstInt(ordinal);
                this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(this.compiler.miDRisDBNull), this.compiler.miDRisDBNull);
                this.gen.Emit(OpCodes.Brtrue, label1);
                this.GenerateAccessDataReader();
                if (locOrdinal != null)
                    this.gen.Emit(OpCodes.Ldloc, locOrdinal);
                else
                    this.GenerateConstInt(ordinal);
                this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(readerMethod1), readerMethod1);
                this.GenerateConvertToType(closestRuntimeType, cType, readerMethod1.ReturnType);
                this.gen.Emit(OpCodes.Br_S, label2);
                this.gen.MarkLabel(label3);
                this.GenerateAccessBufferReader();
                if (locOrdinal != null)
                    this.gen.Emit(OpCodes.Ldloc, locOrdinal);
                else
                    this.GenerateConstInt(ordinal);
                this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(this.compiler.miBRisDBNull), this.compiler.miBRisDBNull);
                this.gen.Emit(OpCodes.Brtrue, label1);
                this.GenerateAccessBufferReader();
                if (locOrdinal != null)
                    this.gen.Emit(OpCodes.Ldloc, locOrdinal);
                else
                    this.GenerateConstInt(ordinal);
                this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(readerMethod2), readerMethod2);
                this.GenerateConvertToType(closestRuntimeType, cType, readerMethod2.ReturnType);
                this.gen.Emit(OpCodes.Br_S, label2);
                this.gen.MarkLabel(label1);
                this.GenerateDefault(cType);
                this.gen.MarkLabel(label2);
            }

            private Type GenerateClientCase(SqlClientCase scc, bool isDeferred, LocalBuilder locInstance)
            {
                LocalBuilder local = this.gen.DeclareLocal(scc.Expression.ClrType);
                this.GenerateExpressionForType(scc.Expression, scc.Expression.ClrType);
                this.gen.Emit(OpCodes.Stloc, local);
                Label label1 = this.gen.DefineLabel();
                Label label2 = this.gen.DefineLabel();
                int index = 0;
                for (int count = scc.Whens.Count; index < count; ++index)
                {
                    if (index > 0)
                    {
                        this.gen.MarkLabel(label1);
                        label1 = this.gen.DefineLabel();
                    }
                    SqlClientWhen sqlClientWhen = scc.Whens[index];
                    if (sqlClientWhen.Match != null)
                    {
                        this.gen.Emit(OpCodes.Ldloc, local);
                        this.GenerateExpressionForType(sqlClientWhen.Match, scc.Expression.ClrType);
                        this.GenerateEquals(local.LocalType);
                        this.gen.Emit(OpCodes.Brfalse, label1);
                    }
                    if (isDeferred)
                        this.GenerateDeferredSource(sqlClientWhen.Value, locInstance);
                    else
                        this.GenerateExpressionForType(sqlClientWhen.Value, scc.ClrType);
                    this.gen.Emit(OpCodes.Br, label2);
                }
                this.gen.MarkLabel(label2);
                return scc.ClrType;
            }

            private Type GenerateTypeCase(SqlTypeCase stc)
            {
                LocalBuilder local = this.gen.DeclareLocal(stc.Discriminator.ClrType);
                this.GenerateExpressionForType(stc.Discriminator, stc.Discriminator.ClrType);
                this.gen.Emit(OpCodes.Stloc, local);
                Label label1 = this.gen.DefineLabel();
                Label label2 = this.gen.DefineLabel();
                bool flag = false;
                int index = 0;
                for (int count = stc.Whens.Count; index < count; ++index)
                {
                    if (index > 0)
                    {
                        this.gen.MarkLabel(label1);
                        label1 = this.gen.DefineLabel();
                    }
                    SqlTypeCaseWhen sqlTypeCaseWhen = stc.Whens[index];
                    if (sqlTypeCaseWhen.Match != null)
                    {
                        this.gen.Emit(OpCodes.Ldloc, local);
                        SqlValue sqlValue = sqlTypeCaseWhen.Match as SqlValue;
                        this.GenerateConstant(local.LocalType, sqlValue.Value);
                        this.GenerateEquals(local.LocalType);
                        this.gen.Emit(OpCodes.Brfalse, label1);
                    }
                    else
                        flag = true;
                    this.GenerateExpressionForType(sqlTypeCaseWhen.TypeBinding, stc.ClrType);
                    this.gen.Emit(OpCodes.Br, label2);
                }
                this.gen.MarkLabel(label1);
                if (!flag)
                    this.GenerateConstant(stc.ClrType, (object)null);
                this.gen.MarkLabel(label2);
                return stc.ClrType;
            }

            private Type GenerateDiscriminatedType(SqlDiscriminatedType dt)
            {
                LocalBuilder localBuilder = this.gen.DeclareLocal(dt.Discriminator.ClrType);
                this.GenerateExpressionForType(dt.Discriminator, dt.Discriminator.ClrType);
                this.gen.Emit(OpCodes.Stloc, localBuilder);
                return this.GenerateDiscriminatedType(dt.TargetType, localBuilder, dt.Discriminator.SqlType);
            }

            private Type GenerateDiscriminatedType(MetaType targetType, LocalBuilder locDiscriminator, ProviderType discriminatorType)
            {
                MetaType metaType1 = (MetaType)null;
                Label label1 = this.gen.DefineLabel();
                Label label2 = this.gen.DefineLabel();
                foreach (MetaType metaType2 in targetType.InheritanceTypes)
                {
                    if (metaType2.InheritanceCode != null)
                    {
                        if (metaType2.IsInheritanceDefault)
                            metaType1 = metaType2;
                        this.gen.Emit(OpCodes.Ldloc, locDiscriminator);
                        object obj = InheritanceRules.InheritanceCodeForClientCompare(metaType2.InheritanceCode, discriminatorType);
                        this.GenerateConstant(locDiscriminator.LocalType, obj);
                        this.GenerateEquals(locDiscriminator.LocalType);
                        this.gen.Emit(OpCodes.Brfalse, label1);
                        this.GenerateConstant(typeof(Type), (object)metaType2.Type);
                        this.gen.Emit(OpCodes.Br, label2);
                        this.gen.MarkLabel(label1);
                        label1 = this.gen.DefineLabel();
                    }
                }
                this.gen.MarkLabel(label1);
                if (metaType1 != null)
                    this.GenerateConstant(typeof(Type), (object)metaType1.Type);
                else
                    this.GenerateDefault(typeof(Type));
                this.gen.MarkLabel(label2);
                return typeof(Type);
            }

            private Type GenerateSearchedCase(SqlSearchedCase ssc)
            {
                Label label1 = this.gen.DefineLabel();
                Label label2 = this.gen.DefineLabel();
                int index = 0;
                for (int count = ssc.Whens.Count; index < count; ++index)
                {
                    if (index > 0)
                    {
                        this.gen.MarkLabel(label1);
                        label1 = this.gen.DefineLabel();
                    }
                    SqlWhen sqlWhen = ssc.Whens[index];
                    if (sqlWhen.Match != null)
                    {
                        this.GenerateExpressionForType(sqlWhen.Match, typeof(bool));
                        this.GenerateConstInt(0);
                        this.gen.Emit(OpCodes.Ceq);
                        this.gen.Emit(OpCodes.Brtrue, label1);
                    }
                    this.GenerateExpressionForType(sqlWhen.Value, ssc.ClrType);
                    this.gen.Emit(OpCodes.Br, label2);
                }
                this.gen.MarkLabel(label1);
                if (ssc.Else != null)
                    this.GenerateExpressionForType(ssc.Else, ssc.ClrType);
                this.gen.MarkLabel(label2);
                return ssc.ClrType;
            }

            private void GenerateEquals(Type type)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                    case TypeCode.DBNull:
                    case TypeCode.String:
                        if (type.IsValueType)
                        {
                            LocalBuilder local1 = this.gen.DeclareLocal(type);
                            LocalBuilder local2 = this.gen.DeclareLocal(type);
                            this.gen.Emit(OpCodes.Stloc, local2);
                            this.gen.Emit(OpCodes.Stloc, local1);
                            this.gen.Emit(OpCodes.Ldloc, local1);
                            this.gen.Emit(OpCodes.Box, type);
                            this.gen.Emit(OpCodes.Ldloc, local2);
                            this.gen.Emit(OpCodes.Box, type);
                        }
                        MethodInfo method = typeof(object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public);
                        this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(method), method);
                        break;
                    default:
                        this.gen.Emit(OpCodes.Ceq);
                        break;
                }
            }

            private void GenerateDefault(Type type)
            {
                this.GenerateDefault(type, true);
            }

            private void GenerateDefault(Type type, bool throwIfNotNullable)
            {
                if (type.IsValueType)
                {
                    if (!throwIfNotNullable || TypeSystem.IsNullableType(type))
                    {
                        LocalBuilder local = this.gen.DeclareLocal(type);
                        this.gen.Emit(OpCodes.Ldloca, local);
                        this.gen.Emit(OpCodes.Initobj, type);
                        this.gen.Emit(OpCodes.Ldloc, local);
                    }
                    else
                    {
                        this.gen.Emit(OpCodes.Ldtoken, type);
                        this.gen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
                        Type type1 = typeof(ObjectMaterializer<>);
                        Type[] typeArray = new Type[1];
                        int index = 0;
                        Type type2 = this.compiler.dataReaderType;
                        typeArray[index] = type2;
                        MethodInfo method = type1.MakeGenericType(typeArray).GetMethod("ErrorAssignmentToNull", BindingFlags.Static | BindingFlags.Public);
                        this.gen.Emit(OpCodes.Call, method);
                        this.gen.Emit(OpCodes.Throw);
                    }
                }
                else
                    this.gen.Emit(OpCodes.Ldnull);
            }

            private MethodInfo GetReaderMethod(Type readerType, Type valueType)
            {
                if (valueType.IsEnum)
                    valueType = valueType.BaseType;
                string name = Type.GetTypeCode(valueType) != TypeCode.Single ? "Get" + valueType.Name : "GetFloat";
                MethodInfo method = readerType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public, (Binder)null, ObjectReaderCompiler.Generator.readMethodSignature, (ParameterModifier[])null);
                if (method == (MethodInfo)null)
                    method = readerType.GetMethod("GetValue", BindingFlags.Instance | BindingFlags.Public, (Binder)null, ObjectReaderCompiler.Generator.readMethodSignature, (ParameterModifier[])null);
                return method;
            }

            private void GenerateHasValue(Type nullableType)
            {
                MethodInfo method = nullableType.GetMethod("get_HasValue", BindingFlags.Instance | BindingFlags.Public);
                this.gen.Emit(OpCodes.Call, method);
            }

            private void GenerateGetValue(Type nullableType)
            {
                MethodInfo method = nullableType.GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);
                this.gen.Emit(OpCodes.Call, method);
            }

            private void GenerateGetValueOrDefault(Type nullableType)
            {
                MethodInfo method = nullableType.GetMethod("GetValueOrDefault", Type.EmptyTypes);
                this.gen.Emit(OpCodes.Call, method);
            }

            private Type GenerateGlobalAccess(int iGlobal, Type type)
            {
                this.GenerateAccessGlobals();
                if (type.IsValueType)
                {
                    this.GenerateConstInt(iGlobal);
                    this.gen.Emit(OpCodes.Ldelem_Ref);
                    Type type1 = typeof(StrongBox<>);
                    Type[] typeArray = new Type[1];
                    int index = 0;
                    Type type2 = type;
                    typeArray[index] = type2;
                    Type cls = type1.MakeGenericType(typeArray);
                    this.gen.Emit(OpCodes.Castclass, cls);
                    FieldInfo field = cls.GetField("Value", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    this.gen.Emit(OpCodes.Ldfld, field);
                }
                else
                {
                    this.GenerateConstInt(iGlobal);
                    this.gen.Emit(OpCodes.Ldelem_Ref);
                    this.GenerateConvertToType(typeof(object), type);
                    this.gen.Emit(OpCodes.Castclass, type);
                }
                return type;
            }

            private int AddGlobal(Type type, object value)
            {
                int count = this.globals.Count;
                if (type.IsValueType)
                {
                    List<object> list = this.globals;
                    Type type1 = typeof(StrongBox<>);
                    Type[] typeArray = new Type[1];
                    int index1 = 0;
                    Type type2 = type;
                    typeArray[index1] = type2;
                    Type type3 = type1.MakeGenericType(typeArray);
                    object[] objArray = new object[1];
                    int index2 = 0;
                    object obj = value;
                    objArray[index2] = obj;
                    object instance = Activator.CreateInstance(type3, objArray);
                    list.Add(instance);
                }
                else
                    this.globals.Add(value);
                return count;
            }

            private int AllocateLocal()
            {
                int num = this.nLocals;
                this.nLocals = num + 1;
                return num;
            }

            private void GenerateStoreMember(MemberInfo mi)
            {
                FieldInfo field = mi as FieldInfo;
                if (field != (FieldInfo)null)
                {
                    this.gen.Emit(OpCodes.Stfld, field);
                }
                else
                {
                    MethodInfo setMethod = ((PropertyInfo)mi).GetSetMethod(true);
                    this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(setMethod), setMethod);
                }
            }

            private void GenerateLoadMember(MemberInfo mi)
            {
                FieldInfo field = mi as FieldInfo;
                if (field != (FieldInfo)null)
                {
                    this.gen.Emit(OpCodes.Ldfld, field);
                }
                else
                {
                    MethodInfo getMethod = ((PropertyInfo)mi).GetGetMethod(true);
                    this.gen.Emit(ObjectReaderCompiler.Generator.GetMethodCallOpCode(getMethod), getMethod);
                }
            }

            private void GenerateArrayAssign(Type type)
            {
                if (type.IsEnum)
                {
                    this.gen.Emit(OpCodes.Stelem, type);
                }
                else
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                            this.gen.Emit(OpCodes.Stelem_I1);
                            break;
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                            this.gen.Emit(OpCodes.Stelem_I2);
                            break;
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                            this.gen.Emit(OpCodes.Stelem_I4);
                            break;
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            this.gen.Emit(OpCodes.Stelem_I8);
                            break;
                        case TypeCode.Single:
                            this.gen.Emit(OpCodes.Stelem_R4);
                            break;
                        case TypeCode.Double:
                            this.gen.Emit(OpCodes.Stelem_R8);
                            break;
                        default:
                            if (type.IsValueType)
                            {
                                this.gen.Emit(OpCodes.Stelem, type);
                                break;
                            }
                            this.gen.Emit(OpCodes.Stelem_Ref);
                            break;
                    }
                }
            }

            private Type GenerateArrayAccess(Type type, bool address)
            {
                if (!type.IsEnum && Type.GetTypeCode(type) == TypeCode.Int32)
                    this.gen.Emit(OpCodes.Ldelem_I4);
                return type;
            }

            private Type GenerateConstant(Type type, object value)
            {
                if (value == null)
                {
                    if (type.IsValueType)
                    {
                        LocalBuilder local = this.gen.DeclareLocal(type);
                        this.gen.Emit(OpCodes.Ldloca, local);
                        this.gen.Emit(OpCodes.Initobj, type);
                        this.gen.Emit(OpCodes.Ldloc, local);
                    }
                    else
                        this.gen.Emit(OpCodes.Ldnull);
                }
                else
                {
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Boolean:
                            this.GenerateConstInt((bool)value ? 1 : 0);
                            break;
                        case TypeCode.SByte:
                            this.GenerateConstInt((int)(sbyte)value);
                            this.gen.Emit(OpCodes.Conv_I1);
                            break;
                        case TypeCode.Int16:
                            this.GenerateConstInt((int)(short)value);
                            this.gen.Emit(OpCodes.Conv_I2);
                            break;
                        case TypeCode.Int32:
                            this.GenerateConstInt((int)value);
                            break;
                        case TypeCode.Int64:
                            this.gen.Emit(OpCodes.Ldc_I8, (long)value);
                            break;
                        case TypeCode.Single:
                            this.gen.Emit(OpCodes.Ldc_R4, (float)value);
                            break;
                        case TypeCode.Double:
                            this.gen.Emit(OpCodes.Ldc_R8, (double)value);
                            break;
                        default:
                            return this.GenerateGlobalAccess(this.AddGlobal(type, value), type);
                    }
                }
                return type;
            }

            private void GenerateConstInt(int value)
            {
                switch (value)
                {
                    case 0:
                        this.gen.Emit(OpCodes.Ldc_I4_0);
                        break;
                    case 1:
                        this.gen.Emit(OpCodes.Ldc_I4_1);
                        break;
                    case 2:
                        this.gen.Emit(OpCodes.Ldc_I4_2);
                        break;
                    case 3:
                        this.gen.Emit(OpCodes.Ldc_I4_3);
                        break;
                    case 4:
                        this.gen.Emit(OpCodes.Ldc_I4_4);
                        break;
                    case 5:
                        this.gen.Emit(OpCodes.Ldc_I4_5);
                        break;
                    case 6:
                        this.gen.Emit(OpCodes.Ldc_I4_6);
                        break;
                    case 7:
                        this.gen.Emit(OpCodes.Ldc_I4_7);
                        break;
                    case 8:
                        this.gen.Emit(OpCodes.Ldc_I4_8);
                        break;
                    case -1:
                        this.gen.Emit(OpCodes.Ldc_I4_M1);
                        break;
                    default:
                        if (value >= -127 && value < 128)
                        {
                            this.gen.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                            break;
                        }
                        this.gen.Emit(OpCodes.Ldc_I4, value);
                        break;
                }
            }
        }

        private struct NamedColumn
        {
            private string name;
            private bool isRequired;

            internal string Name
            {
                get
                {
                    return this.name;
                }
            }

            internal bool IsRequired
            {
                get
                {
                    return this.isRequired;
                }
            }

            internal NamedColumn(string name, bool isRequired)
            {
                this.name = name;
                this.isRequired = isRequired;
            }
        }

        private class ObjectReaderFactory<TDataReader, TObject> : IObjectReaderFactory where TDataReader : DbDataReader
        {
            private Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize;
            private ObjectReaderCompiler.NamedColumn[] namedColumns;
            private object[] globals;
            private int nLocals;

            internal ObjectReaderFactory(Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize, ObjectReaderCompiler.NamedColumn[] namedColumns, object[] globals, int nLocals)
            {
                this.fnMaterialize = fnMaterialize;
                this.namedColumns = namedColumns;
                this.globals = globals;
                this.nLocals = nLocals;
            }

            public IObjectReader Create(DbDataReader dataReader, bool disposeDataReader, IReaderProvider provider, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries)
            {
                return (IObjectReader)new ObjectReaderCompiler.ObjectReaderSession<TDataReader>((TDataReader)dataReader, provider, parentArgs, userArgs, subQueries).CreateReader<TObject>(this.fnMaterialize, this.namedColumns, this.globals, this.nLocals, disposeDataReader);
            }

            public IObjectReader GetNextResult(IObjectReaderSession session, bool disposeDataReader)
            {
                ObjectReaderCompiler.ObjectReaderSession<TDataReader> objectReaderSession = (ObjectReaderCompiler.ObjectReaderSession<TDataReader>)session;
                ObjectReaderCompiler.ObjectReader<TDataReader, TObject> nextResult = objectReaderSession.GetNextResult<TObject>(this.fnMaterialize, this.namedColumns, this.globals, this.nLocals, disposeDataReader);
                // ISSUE: variable of the null type
                //__Null local = null;
                if (!(nextResult == null & disposeDataReader))
                    return (IObjectReader)nextResult;
                objectReaderSession.Dispose();
                return (IObjectReader)nextResult;
            }
        }

        private abstract class ObjectReaderBase<TDataReader> : ObjectMaterializer<TDataReader> where TDataReader : DbDataReader
        {
            protected ObjectReaderCompiler.ObjectReaderSession<TDataReader> session;
            private bool hasRead;
            private bool hasCurrentRow;
            private bool isFinished;
            private IDataServices services;

            internal bool IsBuffered
            {
                get
                {
                    return this.BufferReader != null;
                }
            }

            public override bool CanDeferLoad
            {
                get
                {
                    return this.services.Context.DeferredLoadingEnabled;
                }
            }

            internal ObjectReaderBase(ObjectReaderCompiler.ObjectReaderSession<TDataReader> session, ObjectReaderCompiler.NamedColumn[] namedColumns, object[] globals, object[] arguments, int nLocals)
            {
                this.session = session;
                this.services = session.Provider.Services;
                this.DataReader = session.DataReader;
                this.Globals = globals;
                this.Arguments = arguments;
                if (nLocals > 0)
                    this.Locals = new object[nLocals];
                if (this.session.IsBuffered)
                    this.Buffer();
                this.Ordinals = this.GetColumnOrdinals(namedColumns);
            }

            public override sealed bool Read()
            {
                if (this.isFinished)
                    return false;
                this.hasCurrentRow = this.BufferReader == null ? this.DataReader.Read() : this.BufferReader.Read();
                if (!this.hasCurrentRow)
                {
                    this.isFinished = true;
                    this.session.Finish(this);
                }
                this.hasRead = true;
                return this.hasCurrentRow;
            }

            internal void Buffer()
            {
                if (this.BufferReader != null || !this.hasCurrentRow && this.hasRead)
                    return;
                if (this.session.IsBuffered)
                {
                    this.BufferReader = this.session.GetNextBufferedReader();
                }
                else
                {
                    DataSet dataSet = new DataSet();
                    int num = 0;
                    dataSet.EnforceConstraints = num != 0;
                    DataTable table = new DataTable();
                    dataSet.Tables.Add(table);
                    string[] activeNames = this.session.GetActiveNames();
                    table.Load((IDataReader)new ObjectReaderCompiler.Rereader((DbDataReader)this.DataReader, this.hasCurrentRow, (string[])null), LoadOption.OverwriteChanges);
                    this.BufferReader = (DbDataReader)new ObjectReaderCompiler.Rereader((DbDataReader)table.CreateDataReader(), false, activeNames);
                }
                if (!this.hasCurrentRow)
                    return;
                this.Read();
            }

            public override object InsertLookup(int iMetaType, object instance)
            {
                return this.services.InsertLookupCachedObject((MetaType)this.Globals[iMetaType], instance);
            }

            public override void SendEntityMaterialized(int iMetaType, object instance)
            {
                this.services.OnEntityMaterialized((MetaType)this.Globals[iMetaType], instance);
            }

            public override IEnumerable ExecuteSubQuery(int iSubQuery, object[] parentArgs)
            {
                if (this.session.ParentArguments != null)
                {
                    int length = this.session.ParentArguments.Length;
                    object[] objArray = new object[length + parentArgs.Length];
                    Array.Copy((Array)this.session.ParentArguments, (Array)objArray, length);
                    Array.Copy((Array)parentArgs, 0, (Array)objArray, length, parentArgs.Length);
                    parentArgs = objArray;
                }
                return (IEnumerable)this.session.SubQueries[iSubQuery].Execute((IProvider)this.session.Provider, parentArgs, this.session.UserArguments).ReturnValue;
            }

            public override IEnumerable<T> GetLinkSource<T>(int iGlobalLink, int iLocalFactory, object[] keyValues)
            {
                IDeferredSourceFactory deferredSourceFactory = (IDeferredSourceFactory)this.Locals[iLocalFactory];
                if (deferredSourceFactory == null)
                {
                    deferredSourceFactory = this.services.GetDeferredSourceFactory((MetaDataMember)this.Globals[iGlobalLink]);
                    this.Locals[iLocalFactory] = (object)deferredSourceFactory;
                }
                return (IEnumerable<T>)deferredSourceFactory.CreateDeferredSource(keyValues);
            }

            public override IEnumerable<T> GetNestedLinkSource<T>(int iGlobalLink, int iLocalFactory, object instance)
            {
                IDeferredSourceFactory deferredSourceFactory = (IDeferredSourceFactory)this.Locals[iLocalFactory];
                if (deferredSourceFactory == null)
                {
                    deferredSourceFactory = this.services.GetDeferredSourceFactory((MetaDataMember)this.Globals[iGlobalLink]);
                    this.Locals[iLocalFactory] = (object)deferredSourceFactory;
                }
                return (IEnumerable<T>)deferredSourceFactory.CreateDeferredSource(instance);
            }

            private int[] GetColumnOrdinals(ObjectReaderCompiler.NamedColumn[] namedColumns)
            {
                DbDataReader dbDataReader = this.BufferReader == null ? (DbDataReader)this.DataReader : this.BufferReader;
                if (namedColumns == null || namedColumns.Length == 0)
                    return (int[])null;
                int[] numArray = new int[namedColumns.Length];
                Dictionary<string, int> dictionary = new Dictionary<string, int>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
                int ordinal = 0;
                for (int fieldCount = dbDataReader.FieldCount; ordinal < fieldCount; ++ordinal)
                    dictionary[SqlIdentifier.QuoteCompoundIdentifier(dbDataReader.GetName(ordinal))] = ordinal;
                int index = 0;
                for (int length = namedColumns.Length; index < length; ++index)
                {
                    int num;
                    if (dictionary.TryGetValue(SqlIdentifier.QuoteCompoundIdentifier(namedColumns[index].Name), out num))
                    {
                        numArray[index] = num;
                    }
                    else
                    {
                        if (namedColumns[index].IsRequired)
                            throw Error.RequiredColumnDoesNotExist((object)namedColumns[index].Name);
                        numArray[index] = -1;
                    }
                }
                return numArray;
            }
        }

        private class ObjectReader<TDataReader, TObject> : ObjectReaderCompiler.ObjectReaderBase<TDataReader>, IEnumerator<TObject>, IDisposable, IEnumerator, IObjectReader where TDataReader : DbDataReader
        {
            private Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize;
            private TObject current;
            private bool disposeSession;

            public IObjectReaderSession Session
            {
                get
                {
                    return (IObjectReaderSession)this.session;
                }
            }

            public TObject Current
            {
                get
                {
                    return this.current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return (object)this.Current;
                }
            }

            internal ObjectReader(ObjectReaderCompiler.ObjectReaderSession<TDataReader> session, ObjectReaderCompiler.NamedColumn[] namedColumns, object[] globals, object[] arguments, int nLocals, bool disposeSession, Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize)
              : base(session, namedColumns, globals, arguments, nLocals)
            {
                this.disposeSession = disposeSession;
                this.fnMaterialize = fnMaterialize;
            }

            public void Dispose()
            {
                GC.SuppressFinalize((object)this);
                if (!this.disposeSession)
                    return;
                this.session.Dispose();
            }

            public bool MoveNext()
            {
                if (this.Read())
                {
                    this.current = this.fnMaterialize((ObjectMaterializer<TDataReader>)this);
                    return true;
                }
                this.current = default(TObject);
                this.Dispose();
                return false;
            }

            public void Reset()
            {
            }
        }

        private class ObjectReaderSession<TDataReader> : IObjectReaderSession, IConnectionUser, IDisposable where TDataReader : DbDataReader
        {
            private TDataReader dataReader;
            private ObjectReaderCompiler.ObjectReaderBase<TDataReader> currentReader;
            private IReaderProvider provider;
            private List<DbDataReader> buffer;
            private int iNextBufferedReader;
            private bool isDisposed;
            private bool isDataReaderDisposed;
            private bool hasResults;
            private object[] parentArgs;
            private object[] userArgs;
            private ICompiledSubQuery[] subQueries;

            internal ObjectReaderCompiler.ObjectReaderBase<TDataReader> CurrentReader
            {
                get
                {
                    return this.currentReader;
                }
            }

            internal TDataReader DataReader
            {
                get
                {
                    return this.dataReader;
                }
            }

            internal IReaderProvider Provider
            {
                get
                {
                    return this.provider;
                }
            }

            internal object[] ParentArguments
            {
                get
                {
                    return this.parentArgs;
                }
            }

            internal object[] UserArguments
            {
                get
                {
                    return this.userArgs;
                }
            }

            internal ICompiledSubQuery[] SubQueries
            {
                get
                {
                    return this.subQueries;
                }
            }

            public bool IsBuffered
            {
                get
                {
                    return this.buffer != null;
                }
            }

            internal ObjectReaderSession(TDataReader dataReader, IReaderProvider provider, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries)
            {
                this.dataReader = dataReader;
                this.provider = provider;
                this.parentArgs = parentArgs;
                this.userArgs = userArgs;
                this.subQueries = subQueries;
                this.hasResults = true;
            }

            internal void Finish(ObjectReaderCompiler.ObjectReaderBase<TDataReader> finishedReader)
            {
                if (this.currentReader != finishedReader)
                    return;
                this.CheckNextResults();
            }

            private void CheckNextResults()
            {
                this.hasResults = !this.dataReader.IsClosed && this.dataReader.NextResult();
                this.currentReader = (ObjectReaderCompiler.ObjectReaderBase<TDataReader>)null;
                if (this.hasResults)
                    return;
                this.Dispose();
            }

            internal DbDataReader GetNextBufferedReader()
            {
                if (this.iNextBufferedReader >= this.buffer.Count)
                    return (DbDataReader)null;
                List<DbDataReader> list = this.buffer;
                int num = this.iNextBufferedReader;
                this.iNextBufferedReader = num + 1;
                int index = num;
                return list[index];
            }

            public void Buffer()
            {
                if (this.buffer != null)
                    return;
                if (this.currentReader != null && !this.currentReader.IsBuffered)
                {
                    this.currentReader.Buffer();
                    this.CheckNextResults();
                }
                this.buffer = new List<DbDataReader>();
                while (this.hasResults)
                {
                    DataSet dataSet = new DataSet();
                    int num = 0;
                    dataSet.EnforceConstraints = num != 0;
                    DataTable table = new DataTable();
                    dataSet.Tables.Add(table);
                    string[] activeNames = this.GetActiveNames();
                    table.Load((IDataReader)new ObjectReaderCompiler.Rereader((DbDataReader)this.dataReader, false, (string[])null), LoadOption.OverwriteChanges);
                    this.buffer.Add((DbDataReader)new ObjectReaderCompiler.Rereader((DbDataReader)table.CreateDataReader(), false, activeNames));
                    this.CheckNextResults();
                }
            }

            internal string[] GetActiveNames()
            {
                string[] strArray = new string[this.DataReader.FieldCount];
                int ordinal = 0;
                for (int fieldCount = this.DataReader.FieldCount; ordinal < fieldCount; ++ordinal)
                    strArray[ordinal] = this.DataReader.GetName(ordinal);
                return strArray;
            }

            public void CompleteUse()
            {
                this.Buffer();
            }

            public void Dispose()
            {
                if (this.isDisposed)
                    return;
                GC.SuppressFinalize((object)this);
                this.isDisposed = true;
                if (!this.isDataReaderDisposed)
                {
                    this.isDataReaderDisposed = true;
                    this.dataReader.Dispose();
                }
                this.provider.ConnectionManager.ReleaseConnection((IConnectionUser)this);
            }

            internal ObjectReaderCompiler.ObjectReader<TDataReader, TObject> CreateReader<TObject>(Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize, ObjectReaderCompiler.NamedColumn[] namedColumns, object[] globals, int nLocals, bool disposeDataReader)
            {
                ObjectReaderCompiler.ObjectReader<TDataReader, TObject> objectReader = new ObjectReaderCompiler.ObjectReader<TDataReader, TObject>(this, namedColumns, globals, this.userArgs, nLocals, disposeDataReader, fnMaterialize);
                this.currentReader = (ObjectReaderCompiler.ObjectReaderBase<TDataReader>)objectReader;
                return objectReader;
            }

            internal ObjectReaderCompiler.ObjectReader<TDataReader, TObject> GetNextResult<TObject>(Func<ObjectMaterializer<TDataReader>, TObject> fnMaterialize, ObjectReaderCompiler.NamedColumn[] namedColumns, object[] globals, int nLocals, bool disposeDataReader)
            {
                if (this.buffer != null)
                {
                    if (this.iNextBufferedReader >= this.buffer.Count)
                        return (ObjectReaderCompiler.ObjectReader<TDataReader, TObject>)null;
                }
                else
                {
                    if (this.currentReader != null)
                    {
                        this.currentReader.Buffer();
                        this.CheckNextResults();
                    }
                    if (!this.hasResults)
                        return (ObjectReaderCompiler.ObjectReader<TDataReader, TObject>)null;
                }
                ObjectReaderCompiler.ObjectReader<TDataReader, TObject> objectReader = new ObjectReaderCompiler.ObjectReader<TDataReader, TObject>(this, namedColumns, globals, this.userArgs, nLocals, disposeDataReader, fnMaterialize);
                this.currentReader = (ObjectReaderCompiler.ObjectReaderBase<TDataReader>)objectReader;
                return objectReader;
            }
        }

        private class Rereader : DbDataReader, IDisposable
        {
            private bool first;
            private DbDataReader reader;
            private string[] names;

            public override int Depth
            {
                get
                {
                    return this.reader.Depth;
                }
            }

            public override bool IsClosed
            {
                get
                {
                    return this.reader.IsClosed;
                }
            }

            public override int RecordsAffected
            {
                get
                {
                    return this.reader.RecordsAffected;
                }
            }

            public override int FieldCount
            {
                get
                {
                    return this.reader.FieldCount;
                }
            }

            public override object this[int i]
            {
                get
                {
                    return this.reader[i];
                }
            }

            public override object this[string name]
            {
                get
                {
                    return this.reader[name];
                }
            }

            public override bool HasRows
            {
                get
                {
                    if (!this.first)
                        return this.reader.HasRows;
                    return true;
                }
            }

            internal Rereader(DbDataReader reader, bool hasCurrentRow, string[] names)
            {
                this.reader = reader;
                this.first = hasCurrentRow;
                this.names = names;
            }

            public override bool Read()
            {
                if (!this.first)
                    return this.reader.Read();
                this.first = false;
                return true;
            }

            public override string GetName(int i)
            {
                if (this.names != null)
                    return this.names[i];
                return this.reader.GetName(i);
            }

            public override void Close()
            {
            }

            public override bool NextResult()
            {
                return false;
            }

            public override DataTable GetSchemaTable()
            {
                return this.reader.GetSchemaTable();
            }

            public override bool GetBoolean(int i)
            {
                return this.reader.GetBoolean(i);
            }

            public override byte GetByte(int i)
            {
                return this.reader.GetByte(i);
            }

            public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
            {
                return this.reader.GetBytes(i, fieldOffset, buffer, bufferOffset, length);
            }

            public override char GetChar(int i)
            {
                return this.reader.GetChar(i);
            }

            public override long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
            {
                return this.reader.GetChars(i, fieldOffset, buffer, bufferOffset, length);
            }

            public override string GetDataTypeName(int i)
            {
                return this.reader.GetDataTypeName(i);
            }

            public override DateTime GetDateTime(int i)
            {
                return this.reader.GetDateTime(i);
            }

            public override Decimal GetDecimal(int i)
            {
                return this.reader.GetDecimal(i);
            }

            public override double GetDouble(int i)
            {
                return this.reader.GetDouble(i);
            }

            public override Type GetFieldType(int i)
            {
                return this.reader.GetFieldType(i);
            }

            public override float GetFloat(int i)
            {
                return this.reader.GetFloat(i);
            }

            public override Guid GetGuid(int i)
            {
                return this.reader.GetGuid(i);
            }

            public override short GetInt16(int i)
            {
                return this.reader.GetInt16(i);
            }

            public override int GetInt32(int i)
            {
                return this.reader.GetInt32(i);
            }

            public override long GetInt64(int i)
            {
                return this.reader.GetInt64(i);
            }

            public override int GetOrdinal(string name)
            {
                return this.reader.GetOrdinal(name);
            }

            public override string GetString(int i)
            {
                return this.reader.GetString(i);
            }

            public override object GetValue(int i)
            {
                return this.reader.GetValue(i);
            }

            public override int GetValues(object[] values)
            {
                return this.reader.GetValues(values);
            }

            public override bool IsDBNull(int i)
            {
                return this.reader.IsDBNull(i);
            }

            public override IEnumerator GetEnumerator()
            {
                return this.reader.GetEnumerator();
            }
        }

        internal class Group<K, T> : IGrouping<K, T>, IEnumerable<T>, IEnumerable
        {
            private K key;
            private IEnumerable<T> items;

            K IGrouping<K, T>.Key
            {
                get
                {
                    return this.key;
                }
            }

            internal Group(K key, IEnumerable<T> items)
            {
                this.key = key;
                this.items = items;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator)this.GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this.items.GetEnumerator();
            }
        }

        internal class OrderedResults<T> : IOrderedEnumerable<T>, IEnumerable<T>, IEnumerable
        {
            private List<T> values;

            internal OrderedResults(IEnumerable<T> results)
            {
                this.values = results as List<T>;
                if (this.values != null)
                    return;
                this.values = new List<T>(results);
            }

            IOrderedEnumerable<T> IOrderedEnumerable<T>.CreateOrderedEnumerable<K>(Func<T, K> keySelector, IComparer<K> comparer, bool descending)
            {
                throw Error.NotSupported();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)this.values).GetEnumerator();
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return (IEnumerator<T>)this.values.GetEnumerator();
            }
        }
    }
}
