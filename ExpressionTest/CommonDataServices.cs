using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class CommonDataServices : IDataServices
    {
        private DataContext context;
        private MetaModel metaModel;
        private IdentityManager identifier;
        private ChangeTracker tracker;
        private ChangeDirector director;
        private bool hasCachedObjects;
        private Dictionary<MetaDataMember, IDeferredSourceFactory> factoryMap;

        public DataContext Context
        {
            get
            {
                return this.context;
            }
        }

        public MetaModel Model
        {
            get
            {
                return this.metaModel;
            }
        }

        internal IdentityManager IdentityManager
        {
            get
            {
                return this.identifier;
            }
        }

        internal ChangeTracker ChangeTracker
        {
            get
            {
                return this.tracker;
            }
        }

        internal ChangeDirector ChangeDirector
        {
            get
            {
                return this.director;
            }
        }

        internal bool HasCachedObjects
        {
            get
            {
                return this.hasCachedObjects;
            }
        }

        internal CommonDataServices(DataContext context, MetaModel model)
        {
            this.context = context;
            this.metaModel = model;
            bool asReadOnly = !context.ObjectTrackingEnabled;
            this.identifier = IdentityManager.CreateIdentityManager(asReadOnly);
            this.tracker = ChangeTracker.CreateChangeTracker(this, asReadOnly);
            this.director = ChangeDirector.CreateChangeDirector(context);
            this.factoryMap = new Dictionary<MetaDataMember, IDeferredSourceFactory>();
        }

        internal void SetModel(MetaModel model)
        {
            this.metaModel = model;
        }

        internal IEnumerable<RelatedItem> GetParents(MetaType type, object item)
        {
            return this.GetRelations(type, item, true);
        }

        internal IEnumerable<RelatedItem> GetChildren(MetaType type, object item)
        {
            return this.GetRelations(type, item, false);
        }

        private IEnumerable<RelatedItem> GetRelations(MetaType type, object item, bool isForeignKey)
        {
            foreach (MetaDataMember metaDataMember in type.PersistentDataMembers)
            {
                if (metaDataMember.IsAssociation)
                {
                    MetaType otherType = metaDataMember.Association.OtherType;
                    if (metaDataMember.Association.IsForeignKey == isForeignKey)
                    {
                        object value = (object)null;
                        value = !metaDataMember.IsDeferred ? metaDataMember.StorageAccessor.GetBoxedValue(item) : metaDataMember.DeferredValueAccessor.GetBoxedValue(item);
                        if (value != null)
                        {
                            if (metaDataMember.Association.IsMany)
                            {
                                foreach (object obj in (IEnumerable)value)
                                    yield return new RelatedItem(otherType.GetInheritanceType(obj.GetType()), obj);
                            }
                            else
                                yield return new RelatedItem(otherType.GetInheritanceType(value.GetType()), value);
                        }
                        value = (object)null;
                    }
                    otherType = (MetaType)null;
                }
            }
        }

        internal void ResetServices()
        {
            this.hasCachedObjects = false;
            bool asReadOnly = !this.context.ObjectTrackingEnabled;
            this.identifier = IdentityManager.CreateIdentityManager(asReadOnly);
            this.tracker = ChangeTracker.CreateChangeTracker(this, asReadOnly);
            this.factoryMap = new Dictionary<MetaDataMember, IDeferredSourceFactory>();
        }

        internal static object[] GetKeyValues(MetaType type, object instance)
        {
            List<object> list = new List<object>();
            foreach (MetaDataMember metaDataMember in type.IdentityMembers)
                list.Add(metaDataMember.MemberAccessor.GetBoxedValue(instance));
            return list.ToArray();
        }

        internal static object[] GetForeignKeyValues(MetaAssociation association, object instance)
        {
            List<object> list = new List<object>();
            foreach (MetaDataMember metaDataMember in association.ThisKey)
                list.Add(metaDataMember.MemberAccessor.GetBoxedValue(instance));
            return list.ToArray();
        }

        internal object GetCachedObject(MetaType type, object[] keyValues)
        {
            if (type == null)
                throw Error.ArgumentNull("type");
            if (!type.IsEntity)
                return (object)null;
            return this.identifier.Find(type, keyValues);
        }

        internal object GetCachedObjectLike(MetaType type, object instance)
        {
            if (type == null)
                throw Error.ArgumentNull("type");
            if (!type.IsEntity)
                return (object)null;
            return this.identifier.FindLike(type, instance);
        }

        public bool IsCachedObject(MetaType type, object instance)
        {
            if (type == null)
                throw Error.ArgumentNull("type");
            if (!type.IsEntity)
                return false;
            return this.identifier.FindLike(type, instance) == instance;
        }

        public object InsertLookupCachedObject(MetaType type, object instance)
        {
            if (type == null)
                throw Error.ArgumentNull("type");
            this.hasCachedObjects = true;
            if (!type.IsEntity)
                return instance;
            return this.identifier.InsertLookup(type, instance);
        }

        public bool RemoveCachedObjectLike(MetaType type, object instance)
        {
            if (type == null)
                throw Error.ArgumentNull("type");
            if (!type.IsEntity)
                return false;
            return this.identifier.RemoveLike(type, instance);
        }

        public void OnEntityMaterialized(MetaType type, object instance)
        {
            if (type == null)
                throw Error.ArgumentNull("type");
            this.tracker.FastTrack(instance);
            if (!type.HasAnyLoadMethod)
                return;
            CommonDataServices.SendOnLoaded(type, instance);
        }

        private static void SendOnLoaded(MetaType type, object item)
        {
            if (type == null)
                return;
            CommonDataServices.SendOnLoaded(type.InheritanceBase, item);
            if (!(type.OnLoadedMethod != (MethodInfo)null))
                return;
            try
            {
                type.OnLoadedMethod.Invoke(item, new object[0]);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }

        internal Expression GetObjectQuery(MetaType type, object[] keyValues)
        {
            if (type == null)
                throw Error.ArgumentNull("type");
            if (keyValues == null)
                throw Error.ArgumentNull("keyValues");
            return this.GetObjectQuery(type, CommonDataServices.BuildKeyExpressions(keyValues, type.IdentityMembers));
        }

        internal Expression GetObjectQuery(MetaType type, Expression[] keyValues)
        {
            ITable table = this.context.GetTable(type.InheritanceRoot.Type);
            ParameterExpression parameterExpression1 = Expression.Parameter(table.ElementType, "p");
            Expression left = (Expression)null;
            int index1 = 0;
            for (int count = type.IdentityMembers.Count; index1 < count; ++index1)
            {
                MetaDataMember metaDataMember = type.IdentityMembers[index1];
                Expression right = (Expression)Expression.Equal(metaDataMember.Member is FieldInfo ? (Expression)Expression.Field((Expression)parameterExpression1, (FieldInfo)metaDataMember.Member) : (Expression)Expression.Property((Expression)parameterExpression1, (PropertyInfo)metaDataMember.Member), keyValues[index1]);
                left = left != null ? (Expression)Expression.And(left, right) : right;
            }
            Type type1 = typeof(Queryable);
            string methodName = "Where";
            Type[] typeArguments = new Type[1];
            int index2 = 0;
            Type elementType = table.ElementType;
            typeArguments[index2] = elementType;
            Expression[] expressionArray = new Expression[2];
            int index3 = 0;
            Expression expression = table.Expression;
            expressionArray[index3] = expression;
            int index4 = 1;
            Expression body = left;
            ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
            int index5 = 0;
            ParameterExpression parameterExpression2 = parameterExpression1;
            parameterExpressionArray[index5] = parameterExpression2;
            LambdaExpression lambdaExpression = Expression.Lambda(body, parameterExpressionArray);
            expressionArray[index4] = (Expression)lambdaExpression;
            return (Expression)Expression.Call(type1, methodName, typeArguments, expressionArray);
        }

        internal Expression GetDataMemberQuery(MetaDataMember member, Expression[] keyValues)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            if (keyValues == null)
                throw Error.ArgumentNull("keyValues");
            if (member.IsAssociation)
            {
                MetaAssociation association = member.Association;
                Type type1 = association.ThisMember.DeclaringType.InheritanceRoot.Type;
                Expression source = (Expression)Expression.Constant((object)this.context.GetTable(type1));
                if (type1 != association.ThisMember.DeclaringType.Type)
                {
                    Type type2 = typeof(System.Linq.Enumerable);
                    string methodName = "Cast";
                    Type[] typeArguments = new Type[1];
                    int index1 = 0;
                    Type type3 = association.ThisMember.DeclaringType.Type;
                    typeArguments[index1] = type3;
                    Expression[] expressionArray = new Expression[1];
                    int index2 = 0;
                    Expression expression = source;
                    expressionArray[index2] = expression;
                    source = (Expression)Expression.Call(type2, methodName, typeArguments, expressionArray);
                }
                Type type4 = typeof(System.Linq.Enumerable);
                string methodName1 = "FirstOrDefault";
                Type[] typeArguments1 = new Type[1];
                int index3 = 0;
                Type type5 = association.ThisMember.DeclaringType.Type;
                typeArguments1[index3] = type5;
                Expression[] expressionArray1 = new Expression[1];
                int index4 = 0;
                Expression expression1 = Translator.WhereClauseFromSourceAndKeys(source, System.Linq.Enumerable.ToArray<MetaDataMember>((IEnumerable<MetaDataMember>)association.ThisKey), keyValues);
                expressionArray1[index4] = expression1;
                Expression thisInstance = (Expression)Expression.Call(type4, methodName1, typeArguments1, expressionArray1);
                Expression otherSource = (Expression)Expression.Constant((object)this.context.GetTable(association.OtherType.InheritanceRoot.Type));
                if (association.OtherType.Type != association.OtherType.InheritanceRoot.Type)
                {
                    Type type2 = typeof(System.Linq.Enumerable);
                    string methodName2 = "Cast";
                    Type[] typeArguments2 = new Type[1];
                    int index1 = 0;
                    Type type3 = association.OtherType.Type;
                    typeArguments2[index1] = type3;
                    Expression[] expressionArray2 = new Expression[1];
                    int index2 = 0;
                    Expression expression2 = otherSource;
                    expressionArray2[index2] = expression2;
                    otherSource = (Expression)Expression.Call(type2, methodName2, typeArguments2, expressionArray2);
                }
                return Translator.TranslateAssociation(this.context, association, otherSource, keyValues, thisInstance);
            }
            Expression objectQuery = this.GetObjectQuery(member.DeclaringType, keyValues);
            Type elementType = TypeSystem.GetElementType(objectQuery.Type);
            ParameterExpression parameterExpression1 = Expression.Parameter(elementType, "p");
            Expression expression3 = (Expression)parameterExpression1;
            if (elementType != member.DeclaringType.Type)
                expression3 = (Expression)Expression.Convert(expression3, member.DeclaringType.Type);
            MemberExpression memberExpression = member.Member is PropertyInfo ? Expression.Property(expression3, (PropertyInfo)member.Member) : Expression.Field(expression3, (FieldInfo)member.Member);
            ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
            int index5 = 0;
            ParameterExpression parameterExpression2 = parameterExpression1;
            parameterExpressionArray[index5] = parameterExpression2;
            LambdaExpression lambdaExpression1 = Expression.Lambda((Expression)memberExpression, parameterExpressionArray);
            Type type6 = typeof(Queryable);
            string methodName3 = "Select";
            Type[] typeArguments3 = new Type[2];
            int index6 = 0;
            Type type7 = elementType;
            typeArguments3[index6] = type7;
            int index7 = 1;
            Type type8 = lambdaExpression1.Body.Type;
            typeArguments3[index7] = type8;
            Expression[] expressionArray3 = new Expression[2];
            int index8 = 0;
            Expression expression4 = objectQuery;
            expressionArray3[index8] = expression4;
            int index9 = 1;
            LambdaExpression lambdaExpression2 = lambdaExpression1;
            expressionArray3[index9] = (Expression)lambdaExpression2;
            return (Expression)Expression.Call(type6, methodName3, typeArguments3, expressionArray3);
        }

        private static Expression[] BuildKeyExpressions(object[] keyValues, ReadOnlyCollection<MetaDataMember> keyMembers)
        {
            Expression[] expressionArray = new Expression[keyValues.Length];
            int index = 0;
            for (int count = keyMembers.Count; index < count; ++index)
            {
                MetaDataMember metaDataMember = keyMembers[index];
                Expression expression = (Expression)Expression.Constant(keyValues[index], metaDataMember.Type);
                expressionArray[index] = expression;
            }
            return expressionArray;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public IDeferredSourceFactory GetDeferredSourceFactory(MetaDataMember member)
        {
            if (member == null)
                throw Error.ArgumentNull("member");
            IDeferredSourceFactory deferredSourceFactory1;
            if (this.factoryMap.TryGetValue(member, out deferredSourceFactory1))
                return deferredSourceFactory1;
            Type type1 = !member.IsAssociation || !member.Association.IsMany ? member.Type : TypeSystem.GetElementType(member.Type);
            Type type2 = typeof(CommonDataServices.DeferredSourceFactory<>);
            Type[] typeArray = new Type[1];
            int index1 = 0;
            Type type3 = type1;
            typeArray[index1] = type3;
            Type type4 = type2.MakeGenericType(typeArray);
            int num = 36;
            // ISSUE: variable of the null type
            //__Null local1 = null;
            object[] args = new object[2];
            int index2 = 0;
            MetaDataMember metaDataMember = member;
            args[index2] = (object)metaDataMember;
            int index3 = 1;
            args[index3] = (object)this;
            // ISSUE: variable of the null type
            //__Null local2 = null;
            IDeferredSourceFactory deferredSourceFactory2 = (IDeferredSourceFactory)Activator.CreateInstance(type4, (BindingFlags)num, (Binder)null, args, (CultureInfo)null);
            this.factoryMap.Add(member, deferredSourceFactory2);
            return deferredSourceFactory2;
        }

        public object GetCachedObject(Expression query)
        {
            if (query == null)
                return (object)null;
            MethodCallExpression methodCallExpression = query as MethodCallExpression;
            if (methodCallExpression == null || methodCallExpression.Arguments.Count < 1 || methodCallExpression.Arguments.Count > 2)
                return (object)null;
            if (methodCallExpression.Method.DeclaringType != typeof(Queryable))
                return (object)null;
            string name = methodCallExpression.Method.Name;
            if (!(name == "Where") && !(name == "First") && (!(name == "FirstOrDefault") && !(name == "Single")) && !(name == "SingleOrDefault"))
                return (object)null;
            if (methodCallExpression.Arguments.Count == 1)
                return this.GetCachedObject(methodCallExpression.Arguments[0]);
            UnaryExpression unaryExpression = methodCallExpression.Arguments[1] as UnaryExpression;
            if (unaryExpression == null || unaryExpression.NodeType != ExpressionType.Quote)
                return (object)null;
            LambdaExpression predicate = unaryExpression.Operand as LambdaExpression;
            if (predicate == null)
                return (object)null;
            ConstantExpression constantExpression = methodCallExpression.Arguments[0] as ConstantExpression;
            if (constantExpression == null)
                return (object)null;
            ITable table1 = constantExpression.Value as ITable;
            if (table1 == null)
                return (object)null;
            if (TypeSystem.GetElementType(query.Type) != table1.ElementType)
                return (object)null;
            MetaTable table2 = this.metaModel.GetTable(table1.ElementType);
            object[] keyValues = this.GetKeyValues(table2.RowType, predicate);
            if (keyValues != null)
                return this.GetCachedObject(table2.RowType, keyValues);
            return (object)null;
        }

        internal object[] GetKeyValues(MetaType type, LambdaExpression predicate)
        {
            if (predicate == null)
                throw Error.ArgumentNull("predicate");
            if (predicate.Parameters.Count != 1)
                return (object[])null;
            Dictionary<MetaDataMember, object> keys = new Dictionary<MetaDataMember, object>();
            if (this.GetKeysFromPredicate(type, keys, predicate.Body) && keys.Count == type.IdentityMembers.Count)
                return System.Linq.Enumerable.ToArray<object>(System.Linq.Enumerable.Select<KeyValuePair<MetaDataMember, object>, object>((IEnumerable<KeyValuePair<MetaDataMember, object>>)System.Linq.Enumerable.OrderBy<KeyValuePair<MetaDataMember, object>, int>((IEnumerable<KeyValuePair<MetaDataMember, object>>)keys, (Func<KeyValuePair<MetaDataMember, object>, int>)(kv => kv.Key.Ordinal)), (Func<KeyValuePair<MetaDataMember, object>, object>)(kv => kv.Value)));
            return (object[])null;
        }

        private bool GetKeysFromPredicate(MetaType type, Dictionary<MetaDataMember, object> keys, Expression expr)
        {
            BinaryExpression binaryExpression = expr as BinaryExpression;
            if (binaryExpression == null)
            {
                MethodCallExpression methodCallExpression = expr as MethodCallExpression;
                if (methodCallExpression == null || !(methodCallExpression.Method.Name == "op_Equality") || methodCallExpression.Arguments.Count != 2)
                    return false;
                binaryExpression = Expression.Equal(methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]);
            }
            switch (binaryExpression.NodeType)
            {
                case ExpressionType.And:
                    if (this.GetKeysFromPredicate(type, keys, binaryExpression.Left))
                        return this.GetKeysFromPredicate(type, keys, binaryExpression.Right);
                    return false;
                case ExpressionType.Equal:
                    if (!CommonDataServices.GetKeyFromPredicate(type, keys, binaryExpression.Left, binaryExpression.Right))
                        return CommonDataServices.GetKeyFromPredicate(type, keys, binaryExpression.Right, binaryExpression.Left);
                    return true;
                default:
                    return false;
            }
        }

        private static bool GetKeyFromPredicate(MetaType type, Dictionary<MetaDataMember, object> keys, Expression mex, Expression vex)
        {
            MemberExpression memberExpression = mex as MemberExpression;
            if (memberExpression == null || memberExpression.Expression == null || (memberExpression.Expression.NodeType != ExpressionType.Parameter || memberExpression.Expression.Type != type.Type) || !type.Type.IsAssignableFrom(memberExpression.Member.ReflectedType) && !memberExpression.Member.ReflectedType.IsAssignableFrom(type.Type))
                return false;
            MetaDataMember dataMember = type.GetDataMember(memberExpression.Member);
            if (!dataMember.IsPrimaryKey || keys.ContainsKey(dataMember))
                return false;
            ConstantExpression constantExpression1 = vex as ConstantExpression;
            if (constantExpression1 != null)
            {
                keys.Add(dataMember, constantExpression1.Value);
                return true;
            }
            InvocationExpression invocationExpression = vex as InvocationExpression;
            if (invocationExpression != null && invocationExpression.Arguments != null && invocationExpression.Arguments.Count == 0)
            {
                ConstantExpression constantExpression2 = invocationExpression.Expression as ConstantExpression;
                if (constantExpression2 != null)
                {
                    keys.Add(dataMember, ((Delegate)constantExpression2.Value).DynamicInvoke());
                    return true;
                }
            }
            return false;
        }

        internal object GetObjectByKey(MetaType type, object[] keyValues)
        {
            return this.GetCachedObject(type, keyValues) ?? System.Linq.Enumerable.SingleOrDefault<object>(System.Linq.Enumerable.OfType<object>((IEnumerable)this.context.Provider.Execute(this.GetObjectQuery(type, keyValues)).ReturnValue));
        }

        private class DeferredSourceFactory<T> : IDeferredSourceFactory
        {
            private MetaDataMember member;
            private CommonDataServices services;
            private ICompiledQuery query;
            private bool refersToPrimaryKey;
            private T[] empty;

            internal DeferredSourceFactory(MetaDataMember member, CommonDataServices services)
            {
                this.member = member;
                this.services = services;
                this.refersToPrimaryKey = this.member.IsAssociation && this.member.Association.OtherKeyIsPrimaryKey;
                this.empty = new T[0];
            }

            public IEnumerable CreateDeferredSource(object instance)
            {
                if (instance == null)
                    throw Error.ArgumentNull("instance");
                return (IEnumerable)new CommonDataServices.DeferredSourceFactory<T>.DeferredSource(this, instance);
            }

            public IEnumerable CreateDeferredSource(object[] keyValues)
            {
                if (keyValues == null)
                    throw Error.ArgumentNull("keyValues");
                return (IEnumerable)new CommonDataServices.DeferredSourceFactory<T>.DeferredSource(this, (object)keyValues);
            }

            private IEnumerator<T> Execute(object instance)
            {
                ReadOnlyCollection<MetaDataMember> readOnlyCollection = !this.member.IsAssociation ? this.member.DeclaringType.IdentityMembers : this.member.Association.ThisKey;
                object[] keyValues = new object[readOnlyCollection.Count];
                int index1 = 0;
                for (int count = readOnlyCollection.Count; index1 < count; ++index1)
                {
                    object boxedValue = readOnlyCollection[index1].StorageAccessor.GetBoxedValue(instance);
                    keyValues[index1] = boxedValue;
                }
                if (this.HasNullForeignKey(keyValues))
                    return ((IEnumerable<T>)this.empty).GetEnumerator();
                T cached;
                if (this.TryGetCachedObject(keyValues, out cached))
                {
                    T[] objArray = new T[1];
                    int index2 = 0;
                    T obj = cached;
                    objArray[index2] = obj;
                    return ((IEnumerable<T>)objArray).GetEnumerator();
                }
                if (!(this.member.LoadMethod != (MethodInfo)null))
                    return this.ExecuteKeyQuery(keyValues);
                try
                {
                    MethodInfo loadMethod = this.member.LoadMethod;
                    DataContext context = this.services.Context;
                    object[] parameters = new object[1];
                    int index2 = 0;
                    object obj1 = instance;
                    parameters[index2] = obj1;
                    object obj2 = loadMethod.Invoke((object)context, parameters);
                    if (!typeof(T).IsAssignableFrom(this.member.LoadMethod.ReturnType))
                        return ((IEnumerable<T>)obj2).GetEnumerator();
                    T[] objArray = new T[1];
                    int index3 = 0;
                    T obj3 = (T)obj2;
                    objArray[index3] = obj3;
                    return ((IEnumerable<T>)objArray).GetEnumerator();
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                        throw ex.InnerException;
                    throw;
                }
            }

            private IEnumerator<T> ExecuteKeys(object[] keyValues)
            {
                if (this.HasNullForeignKey(keyValues))
                    return ((IEnumerable<T>)this.empty).GetEnumerator();
                T cached;
                if (!this.TryGetCachedObject(keyValues, out cached))
                    return this.ExecuteKeyQuery(keyValues);
                T[] objArray = new T[1];
                int index = 0;
                T obj = cached;
                objArray[index] = obj;
                return ((IEnumerable<T>)objArray).GetEnumerator();
            }

            private bool HasNullForeignKey(object[] keyValues)
            {
                if (this.refersToPrimaryKey)
                {
                    bool flag = false;
                    int index = 0;
                    for (int length = keyValues.Length; index < length; ++index)
                        flag |= keyValues[index] == null;
                    if (flag)
                        return true;
                }
                return false;
            }

            private bool TryGetCachedObject(object[] keyValues, out T cached)
            {
                cached = default(T);
                if (this.refersToPrimaryKey)
                {
                    object cachedObject = this.services.GetCachedObject(this.member.IsAssociation ? this.member.Association.OtherType : this.member.DeclaringType, keyValues);
                    if (cachedObject != null)
                    {
                        cached = (T)cachedObject;
                        return true;
                    }
                }
                return false;
            }

            private IEnumerator<T> ExecuteKeyQuery(object[] keyValues)
            {
                if (this.query == null)
                {
                    ParameterExpression parameterExpression1 = Expression.Parameter(typeof(object[]), "keys");
                    Expression[] keyValues1 = new Expression[keyValues.Length];
                    ReadOnlyCollection<MetaDataMember> readOnlyCollection = this.member.IsAssociation ? this.member.Association.OtherKey : this.member.DeclaringType.IdentityMembers;
                    int index1 = 0;
                    for (int length = keyValues.Length; index1 < length; ++index1)
                    {
                        MetaDataMember metaDataMember = readOnlyCollection[index1];
                        keyValues1[index1] = (Expression)Expression.Convert((Expression)Expression.ArrayIndex((Expression)parameterExpression1, (Expression)Expression.Constant((object)index1)), metaDataMember.Type);
                    }
                    Expression dataMemberQuery = this.services.GetDataMemberQuery(this.member, keyValues1);
                    ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                    int index2 = 0;
                    ParameterExpression parameterExpression2 = parameterExpression1;
                    parameterExpressionArray[index2] = parameterExpression2;
                    this.query = this.services.Context.Provider.Compile((Expression)Expression.Lambda(dataMemberQuery, parameterExpressionArray));
                }
                ICompiledQuery compiledQuery = this.query;
                IProvider provider = this.services.Context.Provider;
                object[] arguments = new object[1];
                int index = 0;
                object[] objArray = keyValues;
                arguments[index] = (object)objArray;
                return ((IEnumerable<T>)compiledQuery.Execute(provider, arguments).ReturnValue).GetEnumerator();
            }

            private class DeferredSource : IEnumerable<T>, IEnumerable
            {
                private CommonDataServices.DeferredSourceFactory<T> factory;
                private object instance;

                internal DeferredSource(CommonDataServices.DeferredSourceFactory<T> factory, object instance)
                {
                    this.factory = factory;
                    this.instance = instance;
                }

                public IEnumerator<T> GetEnumerator()
                {
                    object[] keyValues = this.instance as object[];
                    if (keyValues != null)
                        return this.factory.ExecuteKeys(keyValues);
                    return this.factory.Execute(this.instance);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return (IEnumerator)this.GetEnumerator();
                }
            }
        }
    }

    /// <summary>
    /// 用于弱类型查询方案。
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public interface ITable : IQueryable, IEnumerable
    {
        /// <summary>
        /// 获取已用于检索此 <see cref="T:System.Data.Linq.ITable"/> 的 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 用于检索 <see cref="T:System.Data.Linq.ITable"/> 的 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </returns>
        DataContext Context { get; }

        /// <summary>
        /// 指示此 <see cref="T:System.Data.Linq.ITable"/> 实例中包含的实体的类型是否具有主键。
        /// </summary>
        /// 
        /// <returns>
        /// 如果实体类型不具有主键，则返回 true；否则返回 false。
        /// </returns>
        bool IsReadOnly { get; }

        /// <summary>
        /// 将处于 pending insert 状态的实体添加到此表。
        /// </summary>
        /// <param name="entity">要添加的实体。</param>
        void InsertOnSubmit(object entity);

        /// <summary>
        /// 以 pending insert 状态将集合中的所有实体添加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entities">要添加的实体。</param>
        void InsertAllOnSubmit(IEnumerable entities);

        /// <summary>
        /// 以未修改状态将实体附加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entity">要附加的实体。</param>
        void Attach(object entity);

        /// <summary>
        /// 以已修改或未修改状态将集合的所有实体附加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entity">实体的集合。</param><param name="asModified">如果为 true，则以修改状态附加这些实体。</param>
        void Attach(object entity, bool asModified);

        /// <summary>
        /// 通过指定实体及其原始状态，以已修改或未修改状态将实体附加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entity">要附加的实体。</param><param name="original">与包含原始值的数据成员具有相同实体类型的实例。</param>
        void Attach(object entity, object original);

        /// <summary>
        /// 以已修改或未修改状态将集合的所有实体附加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entities">实体的集合。</param>
        void AttachAll(IEnumerable entities);

        /// <summary>
        /// 以已修改或未修改状态将集合的所有实体附加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entities">实体的集合。</param><param name="asModified">如果为 true，则以修改状态附加这些实体。</param>
        void AttachAll(IEnumerable entities, bool asModified);

        /// <summary>
        /// 将此表中的实体置为 pending delete 状态。
        /// </summary>
        /// <param name="entity">要移除的实体。</param>
        void DeleteOnSubmit(object entity);

        /// <summary>
        /// 将集合中的所有实体置于 pending delete 状态。
        /// </summary>
        /// <param name="entities">从其移除所有项的集合。</param>
        void DeleteAllOnSubmit(IEnumerable entities);

        /// <summary>
        /// 检索原始值。
        /// </summary>
        /// 
        /// <returns>
        /// 原始实体的副本。如果未跟踪传入的实体，则该值为 null。必须先附加由客户端发送回的已断开连接的实体，然后 <see cref="T:System.Data.Linq.DataContext"/> 才能开始跟踪实体的状态。新附加的实体的“原始状态”是根据客户端提供的值设立的。数据上下文不跟踪已断开连接的实体的状态。
        /// </returns>
        /// <param name="entity">要检索其原始值的实体。</param>
        object GetOriginalEntityState(object entity);

        /// <summary>
        /// 返回包含其当前值和原始值的已修改成员的数组。
        /// </summary>
        /// <param name="entity">从其获取数组的实体。</param>
        ModifiedMemberInfo[] GetModifiedMembers(object entity);
    }
    /// <summary>
    /// 表示基础数据库中特定类型的表。
    /// </summary>
    /// <typeparam name="TEntity"/>
    public interface ITable<TEntity> : IQueryable<TEntity>, IEnumerable<TEntity>, IEnumerable, IQueryable where TEntity : class
    {
        /// <summary>
        /// 重写时，将 pending insert 状态的实体添加到此 <see cref="T:System.Data.Linq.ITable`1"/>。
        /// </summary>
        /// <param name="entity">要插入的对象。</param>
        void InsertOnSubmit(TEntity entity);

        /// <summary>
        /// 重写时，如果执行开放式并发检查时需要原始值，请将已断开连接或“已分离”的实体附加到新的 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entity">要添加的对象。</param>
        void Attach(TEntity entity);

        /// <summary>
        /// 重写时，将此表中的实体置为 pending delete 状态。
        /// </summary>
        /// <param name="entity">要删除的对象。</param>
        void DeleteOnSubmit(TEntity entity);
    }
    internal interface ICompiledQuery
    {
        IExecuteResult Execute(IProvider provider, object[] arguments);
    }

    /// <summary>
    /// 提供对执行查询的返回值或结果的访问。
    /// </summary>
    public interface IExecuteResult : IDisposable
    {
        /// <summary>
        /// 获取执行的查询的返回值或结果。
        /// </summary>
        /// 
        /// <returns>
        /// 执行的查询的值或结果。
        /// </returns>
        object ReturnValue { get; }

        /// <summary>
        /// 提供对第 n 个输出参数的访问。
        /// </summary>
        /// 
        /// <returns>
        /// 一个包含指定参数的值的对象。
        /// </returns>
        /// <param name="parameterIndex">要检索的参数的索引。</param>
        object GetParameterValue(int parameterIndex);
    }

    internal abstract class ChangeTracker
    {
        internal abstract TrackedObject Track(object obj);

        internal abstract TrackedObject Track(object obj, bool recurse);

        internal abstract void FastTrack(object obj);

        internal abstract bool IsTracked(object obj);

        internal abstract TrackedObject GetTrackedObject(object obj);

        internal abstract void StopTracking(object obj);

        internal abstract void AcceptChanges();

        internal abstract IEnumerable<TrackedObject> GetInterestingObjects();

        internal static ChangeTracker CreateChangeTracker(CommonDataServices dataServices, bool asReadOnly)
        {
            if (asReadOnly)
                return (ChangeTracker)new ChangeTracker.ReadOnlyChangeTracker();
            return (ChangeTracker)new ChangeTracker.StandardChangeTracker(dataServices);
        }

        private class StandardChangeTracker : ChangeTracker
        {
            private Dictionary<object, ChangeTracker.StandardChangeTracker.StandardTrackedObject> items;
            private PropertyChangingEventHandler onPropertyChanging;
            private CommonDataServices services;

            internal StandardChangeTracker(CommonDataServices services)
            {
                this.services = services;
                this.items = new Dictionary<object, ChangeTracker.StandardChangeTracker.StandardTrackedObject>();
                this.onPropertyChanging = new PropertyChangingEventHandler(this.OnPropertyChanging);
            }

            private static MetaType TypeFromDiscriminator(MetaType root, object discriminator)
            {
                foreach (MetaType metaType in root.InheritanceTypes)
                {
                    if (ChangeTracker.StandardChangeTracker.IsSameDiscriminator(discriminator, metaType.InheritanceCode))
                        return metaType;
                }
                return root.InheritanceDefault;
            }

            private static bool IsSameDiscriminator(object discriminator1, object discriminator2)
            {
                if (discriminator1 == discriminator2)
                    return true;
                if (discriminator1 == null || discriminator2 == null)
                    return false;
                return discriminator1.Equals(discriminator2);
            }

            internal override TrackedObject Track(object obj)
            {
                return this.Track(obj, false);
            }

            internal override TrackedObject Track(object obj, bool recurse)
            {
                MetaType metaType = this.services.Model.GetMetaType(obj.GetType());
                Dictionary<object, object> visited = new Dictionary<object, object>();
                return this.Track(metaType, obj, visited, recurse, 1);
            }

            private TrackedObject Track(MetaType mt, object obj, Dictionary<object, object> visited, bool recurse, int level)
            {
                ChangeTracker.StandardChangeTracker.StandardTrackedObject standardTrackedObject1 = (ChangeTracker.StandardChangeTracker.StandardTrackedObject)this.GetTrackedObject(obj);
                if (standardTrackedObject1 != null || visited.ContainsKey(obj))
                    return (TrackedObject)standardTrackedObject1;
                bool flag = level > 1;
                MetaType type = mt;
                object obj1 = obj;
                int num = flag ? 1 : 0;
                ChangeTracker.StandardChangeTracker.StandardTrackedObject standardTrackedObject2 = new ChangeTracker.StandardChangeTracker.StandardTrackedObject(this, type, obj1, obj1, num != 0);
                if (standardTrackedObject2.HasDeferredLoaders)
                    throw Error.CannotAttachAddNonNewEntities();
                this.items.Add(obj, standardTrackedObject2);
                this.Attach(obj);
                Dictionary<object, object> dictionary = visited;
                object key = obj;
                dictionary.Add(key, key);
                if (recurse)
                {
                    foreach (RelatedItem relatedItem in this.services.GetParents(mt, obj))
                        this.Track(relatedItem.Type, relatedItem.Item, visited, recurse, level + 1);
                    foreach (RelatedItem relatedItem in this.services.GetChildren(mt, obj))
                        this.Track(relatedItem.Type, relatedItem.Item, visited, recurse, level + 1);
                }
                return (TrackedObject)standardTrackedObject2;
            }

            internal override void FastTrack(object obj)
            {
                this.Attach(obj);
            }

            internal override void StopTracking(object obj)
            {
                this.Detach(obj);
                this.items.Remove(obj);
            }

            internal override bool IsTracked(object obj)
            {
                if (!this.items.ContainsKey(obj))
                    return this.IsFastTracked(obj);
                return true;
            }

            private bool IsFastTracked(object obj)
            {
                return this.services.IsCachedObject(this.services.Model.GetTable(obj.GetType()).RowType, obj);
            }

            internal override TrackedObject GetTrackedObject(object obj)
            {
                ChangeTracker.StandardChangeTracker.StandardTrackedObject standardTrackedObject;
                if (!this.items.TryGetValue(obj, out standardTrackedObject) && this.IsFastTracked(obj))
                    return (TrackedObject)this.PromoteFastTrackedObject(obj);
                return (TrackedObject)standardTrackedObject;
            }

            private ChangeTracker.StandardChangeTracker.StandardTrackedObject PromoteFastTrackedObject(object obj)
            {
                Type type = obj.GetType();
                return this.PromoteFastTrackedObject(this.services.Model.GetTable(type).RowType.GetInheritanceType(type), obj);
            }

            private ChangeTracker.StandardChangeTracker.StandardTrackedObject PromoteFastTrackedObject(MetaType type, object obj)
            {
                MetaType type1 = type;
                object obj1 = obj;
                ChangeTracker.StandardChangeTracker.StandardTrackedObject standardTrackedObject = new ChangeTracker.StandardChangeTracker.StandardTrackedObject(this, type1, obj1, obj1);
                this.items.Add(obj, standardTrackedObject);
                return standardTrackedObject;
            }

            private void Attach(object obj)
            {
                INotifyPropertyChanging propertyChanging = obj as INotifyPropertyChanging;
                if (propertyChanging != null)
                    propertyChanging.PropertyChanging += this.onPropertyChanging;
                else
                    this.OnPropertyChanging(obj, (PropertyChangingEventArgs)null);
            }

            private void Detach(object obj)
            {
                INotifyPropertyChanging propertyChanging = obj as INotifyPropertyChanging;
                if (propertyChanging == null)
                    return;
                propertyChanging.PropertyChanging -= this.onPropertyChanging;
            }

            private void OnPropertyChanging(object sender, PropertyChangingEventArgs args)
            {
                ChangeTracker.StandardChangeTracker.StandardTrackedObject standardTrackedObject;
                if (this.items.TryGetValue(sender, out standardTrackedObject))
                {
                    standardTrackedObject.StartTracking();
                }
                else
                {
                    if (!this.IsFastTracked(sender))
                        return;
                    this.PromoteFastTrackedObject(sender).StartTracking();
                }
            }

            internal override void AcceptChanges()
            {
                foreach (TrackedObject trackedObject in new List<ChangeTracker.StandardChangeTracker.StandardTrackedObject>((IEnumerable<ChangeTracker.StandardChangeTracker.StandardTrackedObject>)this.items.Values))
                    trackedObject.AcceptChanges();
            }

            internal override IEnumerable<TrackedObject> GetInterestingObjects()
            {
                foreach (ChangeTracker.StandardChangeTracker.StandardTrackedObject standardTrackedObject in this.items.Values)
                {
                    if (standardTrackedObject.IsInteresting)
                        yield return (TrackedObject)standardTrackedObject;
                }
                Dictionary<object, ChangeTracker.StandardChangeTracker.StandardTrackedObject>.ValueCollection.Enumerator enumerator = new Dictionary<object, ChangeTracker.StandardChangeTracker.StandardTrackedObject>.ValueCollection.Enumerator();
            }

            private class StandardTrackedObject : TrackedObject
            {
                private ChangeTracker.StandardChangeTracker tracker;
                private MetaType type;
                private object current;
                private object original;
                private ChangeTracker.StandardChangeTracker.StandardTrackedObject.State state;
                private BitArray dirtyMemberCache;
                private bool haveInitializedDeferredLoaders;
                private bool isWeaklyTracked;

                internal override bool IsWeaklyTracked
                {
                    get
                    {
                        return this.isWeaklyTracked;
                    }
                }

                internal override MetaType Type
                {
                    get
                    {
                        return this.type;
                    }
                }

                internal override object Current
                {
                    get
                    {
                        return this.current;
                    }
                }

                internal override object Original
                {
                    get
                    {
                        return this.original;
                    }
                }

                internal override bool IsNew
                {
                    get
                    {
                        return this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.New;
                    }
                }

                internal override bool IsDeleted
                {
                    get
                    {
                        return this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Deleted;
                    }
                }

                internal override bool IsRemoved
                {
                    get
                    {
                        return this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Removed;
                    }
                }

                internal override bool IsDead
                {
                    get
                    {
                        return this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Dead;
                    }
                }

                internal override bool IsModified
                {
                    get
                    {
                        if (this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Modified)
                            return true;
                        if (this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.PossiblyModified && this.current != this.original)
                            return this.HasChangedValues();
                        return false;
                    }
                }

                internal override bool IsUnmodified
                {
                    get
                    {
                        if (this.state != ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.PossiblyModified)
                            return false;
                        if (this.current != this.original)
                            return !this.HasChangedValues();
                        return true;
                    }
                }

                internal override bool IsPossiblyModified
                {
                    get
                    {
                        if (this.state != ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Modified)
                            return this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.PossiblyModified;
                        return true;
                    }
                }

                internal override bool IsInteresting
                {
                    get
                    {
                        if (this.state != ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.New && this.state != ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Deleted && this.state != ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Modified && (this.state != ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.PossiblyModified || this.current == this.original))
                            return this.CanInferDelete();
                        return true;
                    }
                }

                internal override bool HasDeferredLoaders
                {
                    get
                    {
                        foreach (MetaAssociation metaAssociation in this.Type.Associations)
                        {
                            if (this.HasDeferredLoader(metaAssociation.ThisMember))
                                return true;
                        }
                        foreach (MetaDataMember deferredMember in System.Linq.Enumerable.Where<MetaDataMember>((IEnumerable<MetaDataMember>)this.Type.PersistentDataMembers, (Func<MetaDataMember, bool>)(p =>
                        {
                            if (p.IsDeferred)
                                return !p.IsAssociation;
                            return false;
                        })))
                        {
                            if (this.HasDeferredLoader(deferredMember))
                                return true;
                        }
                        return false;
                    }
                }

                internal StandardTrackedObject(ChangeTracker.StandardChangeTracker tracker, MetaType type, object current, object original)
                {
                    if (current == null)
                        throw Error.ArgumentNull("current");
                    this.tracker = tracker;
                    this.type = type.GetInheritanceType(current.GetType());
                    this.current = current;
                    this.original = original;
                    this.state = ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.PossiblyModified;
                    this.dirtyMemberCache = new BitArray(this.type.DataMembers.Count);
                }

                internal StandardTrackedObject(ChangeTracker.StandardChangeTracker tracker, MetaType type, object current, object original, bool isWeaklyTracked)
                  : this(tracker, type, current, original)
                {
                    this.isWeaklyTracked = isWeaklyTracked;
                }

                public override string ToString()
                {
                    return this.type.Name + ":" + this.GetState();
                }

                private string GetState()
                {
                    switch (this.state)
                    {
                        case ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.New:
                        case ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Deleted:
                        case ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Removed:
                        case ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Dead:
                            return this.state.ToString();
                        default:
                            return this.IsModified ? "Modified" : "Unmodified";
                    }
                }

                internal override bool CanInferDelete()
                {
                    if (this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Modified || this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.PossiblyModified)
                    {
                        foreach (MetaAssociation metaAssociation in this.Type.Associations)
                        {
                            if (metaAssociation.DeleteOnNull && metaAssociation.IsForeignKey && (!metaAssociation.IsNullable && !metaAssociation.IsMany) && (metaAssociation.ThisMember.StorageAccessor.HasAssignedValue(this.Current) && metaAssociation.ThisMember.StorageAccessor.GetBoxedValue(this.Current) == null))
                                return true;
                        }
                    }
                    return false;
                }

                internal override void ConvertToNew()
                {
                    this.original = (object)null;
                    this.state = ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.New;
                }

                internal override void ConvertToPossiblyModified()
                {
                    this.state = ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.PossiblyModified;
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToModified()
                {
                    this.state = ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Modified;
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToPossiblyModified(object originalState)
                {
                    this.state = ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.PossiblyModified;
                    this.original = this.CreateDataCopy(originalState);
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToDeleted()
                {
                    this.state = ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Deleted;
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToDead()
                {
                    this.state = ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Dead;
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToRemoved()
                {
                    this.state = ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Removed;
                    this.isWeaklyTracked = false;
                }

                internal override void ConvertToUnmodified()
                {
                    this.state = ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.PossiblyModified;
                    this.original = !(this.current is INotifyPropertyChanging) ? this.CreateDataCopy(this.current) : this.current;
                    this.ResetDirtyMemberTracking();
                    this.isWeaklyTracked = false;
                }

                internal override void AcceptChanges()
                {
                    if (this.IsWeaklyTracked)
                    {
                        this.InitializeDeferredLoaders();
                        this.isWeaklyTracked = false;
                    }
                    if (this.IsDeleted)
                        this.ConvertToDead();
                    else if (this.IsNew)
                    {
                        this.InitializeDeferredLoaders();
                        this.ConvertToUnmodified();
                    }
                    else
                    {
                        if (!this.IsPossiblyModified)
                            return;
                        this.ConvertToUnmodified();
                    }
                }

                private void AssignMember(object instance, MetaDataMember mm, object value)
                {
                    if (!(this.current is INotifyPropertyChanging))
                        mm.StorageAccessor.SetBoxedValue(ref instance, value);
                    else
                        mm.MemberAccessor.SetBoxedValue(ref instance, value);
                }

                private void ResetDirtyMemberTracking()
                {
                    this.dirtyMemberCache.SetAll(false);
                }

                internal override void Refresh(RefreshMode mode, object freshInstance)
                {
                    this.SynchDependentData();
                    this.UpdateDirtyMemberCache();
                    Type type = freshInstance.GetType();
                    foreach (MetaDataMember member in this.type.PersistentDataMembers)
                    {
                        RefreshMode mode1 = member.IsDbGenerated ? RefreshMode.OverwriteCurrentValues : mode;
                        if (mode1 != RefreshMode.KeepCurrentValues && !member.IsAssociation && (this.Type.Type == type || member.DeclaringType.Type.IsAssignableFrom(type)))
                        {
                            object boxedValue = member.StorageAccessor.GetBoxedValue(freshInstance);
                            this.RefreshMember(member, mode1, boxedValue);
                        }
                    }
                    this.original = this.CreateDataCopy(freshInstance);
                    if (mode != RefreshMode.OverwriteCurrentValues)
                        return;
                    this.ResetDirtyMemberTracking();
                }

                private void UpdateDirtyMemberCache()
                {
                    foreach (MetaDataMember mm in this.type.PersistentDataMembers)
                    {
                        if ((!mm.IsAssociation || !mm.Association.IsMany) && (!this.dirtyMemberCache.Get(mm.Ordinal) && this.HasChangedValue(mm)))
                            this.dirtyMemberCache.Set(mm.Ordinal, true);
                    }
                }

                internal override void RefreshMember(MetaDataMember mm, RefreshMode mode, object freshValue)
                {
                    if (mode == RefreshMode.KeepCurrentValues || this.HasChangedValue(mm) && mode != RefreshMode.OverwriteCurrentValues)
                        return;
                    object boxedValue = mm.StorageAccessor.GetBoxedValue(this.current);
                    if (object.Equals(freshValue, boxedValue))
                        return;
                    mm.StorageAccessor.SetBoxedValue(ref this.current, freshValue);
                    foreach (MetaDataMember metaDataMember in this.GetAssociationsForKey(mm))
                    {
                        if (!metaDataMember.Association.IsMany)
                        {
                            IEnumerable deferredSource = this.tracker.services.GetDeferredSourceFactory(metaDataMember).CreateDeferredSource(this.current);
                            if (metaDataMember.StorageAccessor.HasValue(this.current))
                                this.AssignMember(this.current, metaDataMember, System.Linq.Enumerable.SingleOrDefault<object>(System.Linq.Enumerable.Cast<object>(deferredSource)));
                        }
                    }
                }

                private IEnumerable<MetaDataMember> GetAssociationsForKey(MetaDataMember key)
                {
                    foreach (MetaDataMember metaDataMember in this.type.PersistentDataMembers)
                    {
                        if (metaDataMember.IsAssociation && metaDataMember.Association.ThisKey.Contains(key))
                            yield return metaDataMember;
                    }
                }

                internal override object CreateDataCopy(object instance)
                {
                    Type type = instance.GetType();
                    object instance1 = Activator.CreateInstance(this.Type.Type);
                    foreach (MetaDataMember member in this.tracker.services.Model.GetTable(type).RowType.InheritanceRoot.GetInheritanceType(type).PersistentDataMembers)
                    {
                        if (!(this.Type.Type != type) || member.DeclaringType.Type.IsAssignableFrom(type))
                        {
                            if (member.IsDeferred)
                            {
                                if (!member.IsAssociation)
                                {
                                    if (member.StorageAccessor.HasValue(instance))
                                    {
                                        object boxedValue = member.DeferredValueAccessor.GetBoxedValue(instance);
                                        member.DeferredValueAccessor.SetBoxedValue(ref instance1, boxedValue);
                                    }
                                    else
                                    {
                                        IEnumerable deferredSource = this.tracker.services.GetDeferredSourceFactory(member).CreateDeferredSource(instance1);
                                        member.DeferredSourceAccessor.SetBoxedValue(ref instance1, (object)deferredSource);
                                    }
                                }
                            }
                            else
                            {
                                object boxedValue = member.StorageAccessor.GetBoxedValue(instance);
                                member.StorageAccessor.SetBoxedValue(ref instance1, boxedValue);
                            }
                        }
                    }
                    return instance1;
                }

                internal void StartTracking()
                {
                    if (this.original != this.current)
                        return;
                    this.original = this.CreateDataCopy(this.current);
                }

                internal override bool SynchDependentData()
                {
                    bool flag1 = false;
                    foreach (MetaAssociation metaAssociation in this.Type.Associations)
                    {
                        MetaDataMember thisMember = metaAssociation.ThisMember;
                        if (metaAssociation.IsForeignKey)
                        {
                            int num1 = thisMember.StorageAccessor.HasAssignedValue(this.current) ? 1 : 0;
                            bool flag2 = thisMember.StorageAccessor.HasLoadedValue(this.current);
                            int num2 = flag2 ? 1 : 0;
                            if ((num1 | num2) != 0)
                            {
                                object boxedValue = thisMember.StorageAccessor.GetBoxedValue(this.current);
                                if (boxedValue != null)
                                {
                                    int index = 0;
                                    for (int count = metaAssociation.ThisKey.Count; index < count; ++index)
                                    {
                                        metaAssociation.ThisKey[index].StorageAccessor.SetBoxedValue(ref this.current, metaAssociation.OtherKey[index].StorageAccessor.GetBoxedValue(boxedValue));
                                        flag1 = true;
                                    }
                                }
                                else if (metaAssociation.IsNullable)
                                {
                                    if (thisMember.IsDeferred || this.original != null && thisMember.MemberAccessor.GetBoxedValue(this.original) != null)
                                    {
                                        int index = 0;
                                        for (int count = metaAssociation.ThisKey.Count; index < count; ++index)
                                        {
                                            MetaDataMember mm = metaAssociation.ThisKey[index];
                                            if (mm.CanBeNull)
                                            {
                                                if (this.original != null && this.HasChangedValue(mm))
                                                {
                                                    if (mm.StorageAccessor.GetBoxedValue(this.current) != null)
                                                        throw Error.InconsistentAssociationAndKeyChange((object)mm.Member.Name, (object)thisMember.Member.Name);
                                                }
                                                else
                                                {
                                                    mm.StorageAccessor.SetBoxedValue(ref this.current, (object)null);
                                                    flag1 = true;
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (!flag2)
                                {
                                    StringBuilder stringBuilder = new StringBuilder();
                                    foreach (MetaDataMember metaDataMember in metaAssociation.ThisKey)
                                    {
                                        if (stringBuilder.Length > 0)
                                            stringBuilder.Append(", ");
                                        stringBuilder.AppendFormat("{0}.{1}", (object)this.Type.Name.ToString(), (object)metaDataMember.Name);
                                    }
                                    throw Error.CouldNotRemoveRelationshipBecauseOneSideCannotBeNull((object)metaAssociation.OtherType.Name, (object)this.Type.Name, (object)stringBuilder);
                                }
                            }
                        }
                    }
                    if (this.type.HasInheritance)
                    {
                        if (this.original != null)
                        {
                            object boxedValue1 = this.type.Discriminator.MemberAccessor.GetBoxedValue(this.current);
                            MetaType metaType1 = ChangeTracker.StandardChangeTracker.TypeFromDiscriminator(this.type, boxedValue1);
                            object boxedValue2 = this.type.Discriminator.MemberAccessor.GetBoxedValue(this.original);
                            MetaType metaType2 = ChangeTracker.StandardChangeTracker.TypeFromDiscriminator(this.type, boxedValue2);
                            if (metaType1 != metaType2)
                                throw Error.CannotChangeInheritanceType(boxedValue2, boxedValue1, (object)this.original.GetType().Name, (object)metaType1);
                        }
                        else
                        {
                            MetaType inheritanceType = this.type.GetInheritanceType(this.current.GetType());
                            if (inheritanceType.HasInheritanceCode)
                            {
                                this.type.Discriminator.MemberAccessor.SetBoxedValue(ref this.current, inheritanceType.InheritanceCode);
                                flag1 = true;
                            }
                        }
                    }
                    return flag1;
                }

                internal override bool HasChangedValue(MetaDataMember mm)
                {
                    if (this.current == this.original)
                        return false;
                    if (mm.IsAssociation && mm.Association.IsMany)
                        return mm.StorageAccessor.HasAssignedValue(this.original);
                    if (mm.StorageAccessor.HasValue(this.current))
                    {
                        if (this.original != null && mm.StorageAccessor.HasValue(this.original))
                        {
                            if (this.dirtyMemberCache.Get(mm.Ordinal))
                                return true;
                            object boxedValue = mm.MemberAccessor.GetBoxedValue(this.original);
                            return !object.Equals(mm.MemberAccessor.GetBoxedValue(this.current), boxedValue);
                        }
                        if (mm.IsDeferred && mm.StorageAccessor.HasAssignedValue(this.current))
                            return true;
                    }
                    return false;
                }

                internal override bool HasChangedValues()
                {
                    if (this.current == this.original)
                        return false;
                    if (this.IsNew)
                        return true;
                    foreach (MetaDataMember mm in this.type.PersistentDataMembers)
                    {
                        if (!mm.IsAssociation && this.HasChangedValue(mm))
                            return true;
                    }
                    return false;
                }

                internal override IEnumerable<ModifiedMemberInfo> GetModifiedMembers()
                {
                    foreach (MetaDataMember metaDataMember in this.type.PersistentDataMembers)
                    {
                        MetaDataMember mm = metaDataMember;
                        if (this.IsModifiedMember(mm))
                        {
                            object currentValue = mm.MemberAccessor.GetBoxedValue(this.current);
                            if (this.original != null && mm.StorageAccessor.HasValue(this.original))
                                yield return new ModifiedMemberInfo(mm.Member, currentValue, mm.MemberAccessor.GetBoxedValue(this.original));
                            else if (this.original == null || mm.IsDeferred && !mm.StorageAccessor.HasLoadedValue(this.current))
                                yield return new ModifiedMemberInfo(mm.Member, currentValue, (object)null);
                            currentValue = (object)null;
                        }
                        mm = (MetaDataMember)null;
                    }
                }

                private bool IsModifiedMember(MetaDataMember member)
                {
                    if (member.IsAssociation || member.IsPrimaryKey || (member.IsVersion || member.IsDbGenerated) || !member.StorageAccessor.HasAssignedValue(this.current))
                        return false;
                    if (this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.Modified)
                        return true;
                    if (this.state == ChangeTracker.StandardChangeTracker.StandardTrackedObject.State.PossiblyModified)
                        return this.HasChangedValue(member);
                    return false;
                }

                private bool HasDeferredLoader(MetaDataMember deferredMember)
                {
                    if (!deferredMember.IsDeferred)
                        return false;
                    MetaAccessor storageAccessor = deferredMember.StorageAccessor;
                    if (storageAccessor.HasAssignedValue(this.current) || storageAccessor.HasLoadedValue(this.current))
                        return false;
                    return (IEnumerable)deferredMember.DeferredSourceAccessor.GetBoxedValue(this.current) != null;
                }

                internal override void InitializeDeferredLoaders()
                {
                    if (!this.tracker.services.Context.DeferredLoadingEnabled)
                        return;
                    foreach (MetaAssociation metaAssociation in this.Type.Associations)
                    {
                        if (!this.IsPendingGeneration((IEnumerable<MetaDataMember>)metaAssociation.ThisKey))
                            this.InitializeDeferredLoader(metaAssociation.ThisMember);
                    }
                    foreach (MetaDataMember deferredMember in System.Linq.Enumerable.Where<MetaDataMember>((IEnumerable<MetaDataMember>)this.Type.PersistentDataMembers, (Func<MetaDataMember, bool>)(p =>
                    {
                        if (p.IsDeferred)
                            return !p.IsAssociation;
                        return false;
                    })))
                    {
                        if (!this.IsPendingGeneration((IEnumerable<MetaDataMember>)this.Type.IdentityMembers))
                            this.InitializeDeferredLoader(deferredMember);
                    }
                    this.haveInitializedDeferredLoaders = true;
                }

                private void InitializeDeferredLoader(MetaDataMember deferredMember)
                {
                    MetaAccessor storageAccessor = deferredMember.StorageAccessor;
                    if (storageAccessor.HasAssignedValue(this.current) || storageAccessor.HasLoadedValue(this.current))
                        return;
                    MetaAccessor deferredSourceAccessor = deferredMember.DeferredSourceAccessor;
                    IEnumerable enumerable = (IEnumerable)deferredSourceAccessor.GetBoxedValue(this.current);
                    if (enumerable == null)
                    {
                        IEnumerable deferredSource = this.tracker.services.GetDeferredSourceFactory(deferredMember).CreateDeferredSource(this.current);
                        deferredSourceAccessor.SetBoxedValue(ref this.current, (object)deferredSource);
                    }
                    else if (enumerable != null && !this.haveInitializedDeferredLoaders)
                        throw Error.CannotAttachAddNonNewEntities();
                }

                internal override bool IsPendingGeneration(IEnumerable<MetaDataMember> key)
                {
                    if (this.IsNew)
                    {
                        foreach (MetaDataMember keyMember in key)
                        {
                            if (this.IsMemberPendingGeneration(keyMember))
                                return true;
                        }
                    }
                    return false;
                }

                internal override bool IsMemberPendingGeneration(MetaDataMember keyMember)
                {
                    if (this.IsNew && keyMember.IsDbGenerated)
                        return true;
                    foreach (MetaAssociation metaAssociation in this.type.Associations)
                    {
                        if (metaAssociation.IsForeignKey)
                        {
                            int index = metaAssociation.ThisKey.IndexOf(keyMember);
                            if (index > -1)
                            {
                                object obj = !metaAssociation.ThisMember.IsDeferred ? metaAssociation.ThisMember.StorageAccessor.GetBoxedValue(this.current) : metaAssociation.ThisMember.DeferredValueAccessor.GetBoxedValue(this.current);
                                if (obj != null && !metaAssociation.IsMany)
                                {
                                    ChangeTracker.StandardChangeTracker.StandardTrackedObject standardTrackedObject = (ChangeTracker.StandardChangeTracker.StandardTrackedObject)this.tracker.GetTrackedObject(obj);
                                    if (standardTrackedObject != null)
                                    {
                                        MetaDataMember keyMember1 = metaAssociation.OtherKey[index];
                                        return standardTrackedObject.IsMemberPendingGeneration(keyMember1);
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }

                private enum State
                {
                    New,
                    Deleted,
                    PossiblyModified,
                    Modified,
                    Removed,
                    Dead,
                }
            }
        }

        private class ReadOnlyChangeTracker : ChangeTracker
        {
            internal override TrackedObject Track(object obj)
            {
                return (TrackedObject)null;
            }

            internal override TrackedObject Track(object obj, bool recurse)
            {
                return (TrackedObject)null;
            }

            internal override void FastTrack(object obj)
            {
            }

            internal override bool IsTracked(object obj)
            {
                return false;
            }

            internal override TrackedObject GetTrackedObject(object obj)
            {
                return (TrackedObject)null;
            }

            internal override void StopTracking(object obj)
            {
            }

            internal override void AcceptChanges()
            {
            }

            internal override IEnumerable<TrackedObject> GetInterestingObjects()
            {
                return (IEnumerable<TrackedObject>)new TrackedObject[0];
            }
        }
    }

    internal struct RelatedItem
    {
        internal MetaType Type;
        internal object Item;

        internal RelatedItem(MetaType type, object item)
        {
            this.Type = type;
            this.Item = item;
        }
    }

    internal abstract class TrackedObject
    {
        internal abstract MetaType Type { get; }

        internal abstract object Current { get; }

        internal abstract object Original { get; }

        internal abstract bool IsInteresting { get; }

        internal abstract bool IsNew { get; }

        internal abstract bool IsDeleted { get; }

        internal abstract bool IsModified { get; }

        internal abstract bool IsUnmodified { get; }

        internal abstract bool IsPossiblyModified { get; }

        internal abstract bool IsRemoved { get; }

        internal abstract bool IsDead { get; }

        internal abstract bool IsWeaklyTracked { get; }

        internal abstract bool HasDeferredLoaders { get; }

        internal abstract bool HasChangedValues();

        internal abstract IEnumerable<ModifiedMemberInfo> GetModifiedMembers();

        internal abstract bool HasChangedValue(MetaDataMember mm);

        internal abstract bool CanInferDelete();

        internal abstract void AcceptChanges();

        internal abstract void ConvertToNew();

        internal abstract void ConvertToPossiblyModified();

        internal abstract void ConvertToPossiblyModified(object original);

        internal abstract void ConvertToUnmodified();

        internal abstract void ConvertToModified();

        internal abstract void ConvertToDeleted();

        internal abstract void ConvertToRemoved();

        internal abstract void ConvertToDead();

        internal abstract void Refresh(RefreshMode mode, object freshInstance);

        internal abstract void RefreshMember(MetaDataMember member, RefreshMode mode, object freshValue);

        internal abstract object CreateDataCopy(object instance);

        internal abstract bool SynchDependentData();

        internal abstract bool IsPendingGeneration(IEnumerable<MetaDataMember> keyMembers);

        internal abstract bool IsMemberPendingGeneration(MetaDataMember keyMember);

        internal abstract void InitializeDeferredLoaders();
    }

    /// <summary>
    /// 具有已在 LINQ to SQL 应用程序中修改的成员的值。
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public struct ModifiedMemberInfo
    {
        private MemberInfo member;
        private object current;
        private object original;

        /// <summary>
        /// 获取已修改成员的成员信息。
        /// </summary>
        /// 
        /// <returns>
        /// 有关发生冲突的成员的信息。
        /// </returns>
        public MemberInfo Member
        {
            get
            {
                return this.member;
            }
        }

        /// <summary>
        /// 获取已修改成员的当前值。
        /// </summary>
        /// 
        /// <returns>
        /// 成员的值。
        /// </returns>
        public object CurrentValue
        {
            get
            {
                return this.current;
            }
        }

        /// <summary>
        /// 获取已修改成员的原始值。
        /// </summary>
        /// 
        /// <returns>
        /// 已修改成员的原始值。
        /// </returns>
        public object OriginalValue
        {
            get
            {
                return this.original;
            }
        }

        internal ModifiedMemberInfo(MemberInfo member, object current, object original)
        {
            this.member = member;
            this.current = current;
            this.original = original;
        }
    }

    /// <summary>
    /// 定义 <see cref="DataContext.Refresh"/> 方法如何处理开放式并发冲突。
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public enum RefreshMode
    {
        KeepCurrentValues,
        KeepChanges,
        OverwriteCurrentValues,
    }

    internal interface IDeferredSourceFactory
    {
        IEnumerable CreateDeferredSource(object instance);

        IEnumerable CreateDeferredSource(object[] keyValues);
    }
}
