using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExpressionTest
{
    /// <summary>
    /// 表示基础数据库中特定类型的表。
    /// </summary>
    /// <typeparam name="TEntity">表中的数据的类型。</typeparam>
    public sealed class Table<TEntity> : IQueryable<TEntity>, IEnumerable<TEntity>, IEnumerable, IQueryable, IQueryProvider, ITable, IListSource, ITable<TEntity> where TEntity : class
    {
        private DataContext context;
        private MetaTable metaTable;
        private IBindingList cachedList;

        /// <summary>
        /// 获取已用于检索此 <see cref="T:System.Data.Linq.Table`1"/> 的 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 已用于检索表的数据上下文。
        /// </returns>
        public DataContext Context
        {
            get
            {
                return this.context;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示包含在此 <see cref="T:System.Data.Linq.Table`1"/> 实例中的实体类型是否具有主键。
        /// </summary>
        /// 
        /// <returns>
        /// 如果实体类型不具有主键，则为 true；否则为 false。
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return !this.metaTable.RowType.IsEntity;
            }
        }

        Expression IQueryable.Expression
        {
            get
            {
                return (Expression)Expression.Constant((object)this);
            }
        }

        Type IQueryable.ElementType
        {
            get
            {
                return typeof(TEntity);
            }
        }

        IQueryProvider IQueryable.Provider
        {
            get
            {
                return (IQueryProvider)this;
            }
        }

        bool IListSource.ContainsListCollection
        {
            get
            {
                return false;
            }
        }

        internal Table(DataContext context, MetaTable metaTable)
        {
            this.context = context;
            this.metaTable = metaTable;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull("expression");
            Type elementType = TypeSystem.GetElementType(expression.Type);
            Type type1 = typeof(IQueryable<>);
            Type[] typeArray1 = new Type[1];
            int index1 = 0;
            Type type2 = elementType;
            typeArray1[index1] = type2;
            Type type3 = type1.MakeGenericType(typeArray1);
            if (!type3.IsAssignableFrom(expression.Type))
                throw Error.ExpectedQueryableArgument((object)"expression", (object)type3);
            Type type4 = typeof(DataQuery<>);
            Type[] typeArray2 = new Type[1];
            int index2 = 0;
            Type type5 = elementType;
            typeArray2[index2] = type5;
            Type type6 = type4.MakeGenericType(typeArray2);
            object[] objArray = new object[2];
            int index3 = 0;
            DataContext dataContext = this.context;
            objArray[index3] = (object)dataContext;
            int index4 = 1;
            Expression expression1 = expression;
            objArray[index4] = (object)expression1;
            return (IQueryable)Activator.CreateInstance(type6, objArray);
        }

        IQueryable<TResult> IQueryProvider.CreateQuery<TResult>(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull("expression");
            if (!typeof(IQueryable<TResult>).IsAssignableFrom(expression.Type))
                throw Error.ExpectedQueryableArgument((object)"expression", (object)typeof(IEnumerable<TResult>));
            return (IQueryable<TResult>)new DataQuery<TResult>(this.context, expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return this.context.Provider.Execute(expression).ReturnValue;
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            return (TResult)this.context.Provider.Execute(expression).ReturnValue;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.GetEnumerator();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// 获取一个能够循环访问集合的枚举数。
        /// </summary>
        /// 
        /// <returns>
        /// 一个可用于循环访问集合的枚举器。
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return ((IEnumerable<TEntity>)this.context.Provider.Execute((Expression)Expression.Constant((object)this)).ReturnValue).GetEnumerator();
        }

        IList IListSource.GetList()
        {
            if (this.cachedList == null)
                this.cachedList = this.GetNewBindingList();
            return (IList)this.cachedList;
        }

        /// <summary>
        /// 创建用于绑定到数据源的新列表。
        /// </summary>
        /// 
        /// <returns>
        /// 用于绑定到数据源的新 <see cref="T:System.ComponentModel.IBindingList"/>。
        /// </returns>
        public IBindingList GetNewBindingList()
        {
            return BindingList.Create<TEntity>(this.context, (IEnumerable<TEntity>)this);
        }

        /// <summary>
        /// 将处于 pending insert 状态的实体添加到此 <see cref="T:System.Data.Linq.Table`1"/>。
        /// </summary>
        /// <param name="entity">要添加的实体。</param>
        public void InsertOnSubmit(TEntity entity)
        {
            if ((object)entity == null)
                throw Error.ArgumentNull("entity");
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            MetaType inheritanceType = this.metaTable.RowType.GetInheritanceType(entity.GetType());
            if (!Table<TEntity>.IsTrackableType(inheritanceType))
                throw Error.TypeCouldNotBeAdded((object)inheritanceType.Type);
            TrackedObject trackedObject = this.context.Services.ChangeTracker.GetTrackedObject((object)entity);
            if (trackedObject == null)
                this.context.Services.ChangeTracker.Track((object)entity).ConvertToNew();
            else if (trackedObject.IsWeaklyTracked)
                trackedObject.ConvertToNew();
            else if (trackedObject.IsDeleted)
                trackedObject.ConvertToPossiblyModified();
            else if (trackedObject.IsRemoved)
                trackedObject.ConvertToNew();
            else if (!trackedObject.IsNew)
                throw Error.CantAddAlreadyExistingItem();
        }

        void ITable.InsertOnSubmit(object entity)
        {
            if (entity == null)
                throw Error.ArgumentNull("entity");
            TEntity entity1 = entity as TEntity;
            if ((object)entity1 == null)
                throw Error.EntityIsTheWrongType();
            this.InsertOnSubmit(entity1);
        }

        /// <summary>
        /// 以 pending insert 状态将集合中的所有实体添加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entities">要添加的实体。</param><typeparam name="TSubEntity">要插入的元素的类型。</typeparam>
        public void InsertAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            if (entities == null)
                throw Error.ArgumentNull("entities");
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            foreach (TSubEntity subEntity in System.Linq.Enumerable.ToList<TSubEntity>(entities))
                this.InsertOnSubmit((TEntity)subEntity);
        }

        void ITable.InsertAllOnSubmit(IEnumerable entities)
        {
            if (entities == null)
                throw Error.ArgumentNull("entities");
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            List<object> list = System.Linq.Enumerable.ToList<object>(System.Linq.Enumerable.Cast<object>(entities));
            ITable table = (ITable)this;
            foreach (object entity in list)
                table.InsertOnSubmit(entity);
        }

        private static bool IsTrackableType(MetaType type)
        {
            return type != null && type.CanInstantiate && (!type.HasInheritance || type.HasInheritanceCode);
        }

        /// <summary>
        /// 将此表中的实体置为 pending delete 状态。
        /// </summary>
        /// <param name="entity">要删除的实体。</param>
        public void DeleteOnSubmit(TEntity entity)
        {
            if ((object)entity == null)
                throw Error.ArgumentNull("entity");
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            TrackedObject trackedObject = this.context.Services.ChangeTracker.GetTrackedObject((object)entity);
            if (trackedObject == null)
                throw Error.CannotRemoveUnattachedEntity();
            if (trackedObject.IsNew)
            {
                trackedObject.ConvertToRemoved();
            }
            else
            {
                if (!trackedObject.IsPossiblyModified && !trackedObject.IsModified)
                    return;
                trackedObject.ConvertToDeleted();
            }
        }

        void ITable.DeleteOnSubmit(object entity)
        {
            if (entity == null)
                throw Error.ArgumentNull("entity");
            TEntity entity1 = entity as TEntity;
            if ((object)entity1 == null)
                throw Error.EntityIsTheWrongType();
            this.DeleteOnSubmit(entity1);
        }

        /// <summary>
        /// 将集合中的所有实体置于 pending delete 状态。
        /// </summary>
        /// <param name="entities">要删除的实体。</param><typeparam name="TSubEntity">要删除的元素的类型。</typeparam><filterpriority>2</filterpriority>
        public void DeleteAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            if (entities == null)
                throw Error.ArgumentNull("entities");
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            foreach (TSubEntity subEntity in System.Linq.Enumerable.ToList<TSubEntity>(entities))
                this.DeleteOnSubmit((TEntity)subEntity);
        }

        void ITable.DeleteAllOnSubmit(IEnumerable entities)
        {
            if (entities == null)
                throw Error.ArgumentNull("entities");
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            List<object> list = System.Linq.Enumerable.ToList<object>(System.Linq.Enumerable.Cast<object>(entities));
            ITable table = (ITable)this;
            foreach (object entity in list)
                table.DeleteOnSubmit(entity);
        }

        /// <summary>
        /// 如果执行开放式并发检查时需要原始值，请将已断开连接或“已分离”的实体附加到新的 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entity">要附加的实体的原始值。</param>
        public void Attach(TEntity entity)
        {
            if ((object)entity == null)
                throw Error.ArgumentNull("entity");
            this.Attach(entity, false);
        }

        void ITable.Attach(object entity)
        {
            if (entity == null)
                throw Error.ArgumentNull("entity");
            TEntity entity1 = entity as TEntity;
            if ((object)entity1 == null)
                throw Error.EntityIsTheWrongType();
            this.Attach(entity1, false);
        }

        /// <summary>
        /// 以已修改或未修改状态将实体附加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entity">要附加的实体。</param><param name="asModified">附加实体，如修改，则为 true；附加实体，如未修改，则为 false。</param>
        public void Attach(TEntity entity, bool asModified)
        {
            if ((object)entity == null)
                throw Error.ArgumentNull("entity");
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            MetaType inheritanceType = this.metaTable.RowType.GetInheritanceType(entity.GetType());
            if (!Table<TEntity>.IsTrackableType(inheritanceType))
                throw Error.TypeCouldNotBeTracked((object)inheritanceType.Type);
            if (asModified && (inheritanceType.VersionMember != null ? 1 : (!inheritanceType.HasUpdateCheck ? 1 : 0)) == 0)
                throw Error.CannotAttachAsModifiedWithoutOriginalState();
            TrackedObject trackedObject = this.Context.Services.ChangeTracker.GetTrackedObject((object)entity);
            if (trackedObject != null && !trackedObject.IsWeaklyTracked)
                throw Error.CannotAttachAlreadyExistingEntity();
            if (trackedObject == null)
                trackedObject = this.context.Services.ChangeTracker.Track((object)entity, true);
            if (asModified)
                trackedObject.ConvertToModified();
            else
                trackedObject.ConvertToUnmodified();
            if (this.Context.Services.InsertLookupCachedObject(inheritanceType, (object)entity) != (object)entity)
                throw new DuplicateKeyException((object)entity, Strings.CantAddAlreadyExistingKey);
            trackedObject.InitializeDeferredLoaders();
        }

        void ITable.Attach(object entity, bool asModified)
        {
            if (entity == null)
                throw Error.ArgumentNull("entity");
            TEntity entity1 = entity as TEntity;
            if ((object)entity1 == null)
                throw Error.EntityIsTheWrongType();
            this.Attach(entity1, asModified);
        }

        /// <summary>
        /// 通过指定实体及其原始状态，以已修改或未修改状态将实体附加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entity">要附加的实体。</param><param name="original">与包含原始值的数据成员具有相同实体类型的实例。</param>
        public void Attach(TEntity entity, TEntity original)
        {
            if ((object)entity == null)
                throw Error.ArgumentNull("entity");
            if ((object)original == null)
                throw Error.ArgumentNull("original");
            if (entity.GetType() != original.GetType())
                throw Error.OriginalEntityIsWrongType();
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            MetaType inheritanceType = this.metaTable.RowType.GetInheritanceType(entity.GetType());
            if (!Table<TEntity>.IsTrackableType(inheritanceType))
                throw Error.TypeCouldNotBeTracked((object)inheritanceType.Type);
            TrackedObject trackedObject = this.context.Services.ChangeTracker.GetTrackedObject((object)entity);
            if (trackedObject != null && !trackedObject.IsWeaklyTracked)
                throw Error.CannotAttachAlreadyExistingEntity();
            if (trackedObject == null)
                trackedObject = this.context.Services.ChangeTracker.Track((object)entity, true);
            trackedObject.ConvertToPossiblyModified((object)original);
            if (this.Context.Services.InsertLookupCachedObject(inheritanceType, (object)entity) != (object)entity)
                throw new DuplicateKeyException((object)entity, Strings.CantAddAlreadyExistingKey);
            trackedObject.InitializeDeferredLoaders();
        }

        void ITable.Attach(object entity, object original)
        {
            if (entity == null)
                throw Error.ArgumentNull("entity");
            if (original == null)
                throw Error.ArgumentNull("original");
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            TEntity entity1 = entity as TEntity;
            if ((object)entity1 == null)
                throw Error.EntityIsTheWrongType();
            if (entity.GetType() != original.GetType())
                throw Error.OriginalEntityIsWrongType();
            this.Attach(entity1, (TEntity)original);
        }

        /// <summary>
        /// 以已修改或未修改状态将集合的所有实体附加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entities">实体的集合。</param><typeparam name="TSubEntity">要附加的实体的类型。</typeparam>
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity
        {
            if (entities == null)
                throw Error.ArgumentNull("entities");
            this.AttachAll<TSubEntity>(entities, false);
        }

        void ITable.AttachAll(IEnumerable entities)
        {
            if (entities == null)
                throw Error.ArgumentNull("entities");
            ((ITable)this).AttachAll(entities, false);
        }

        /// <summary>
        /// 以已修改或未修改状态将集合的所有实体附加到 <see cref="T:System.Data.Linq.DataContext"/>。
        /// </summary>
        /// <param name="entities">实体的集合。</param><param name="asModified">如果对象拥有时间戳或 RowVersion 成员，则为 true；如果执行开放式并发检查时要使用原始值，则为 false。</param><typeparam name="TSubEntity">要附加的实体的类型。</typeparam>
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities, bool asModified) where TSubEntity : TEntity
        {
            if (entities == null)
                throw Error.ArgumentNull("entities");
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            foreach (TSubEntity subEntity in System.Linq.Enumerable.ToList<TSubEntity>(entities))
                this.Attach((TEntity)subEntity, asModified);
        }

        void ITable.AttachAll(IEnumerable entities, bool asModified)
        {
            if (entities == null)
                throw Error.ArgumentNull("entities");
            this.CheckReadOnly();
            this.context.CheckNotInSubmitChanges();
            this.context.VerifyTrackingEnabled();
            List<object> list = System.Linq.Enumerable.ToList<object>(System.Linq.Enumerable.Cast<object>(entities));
            ITable table = (ITable)this;
            foreach (object entity in list)
                table.Attach(entity, asModified);
        }

        /// <summary>
        /// 返回包含实体的原始状态的 <see cref="T:System.Data.Linq.Table`1"/> 实例。
        /// </summary>
        /// 
        /// <returns>
        /// 包含实体的原始状态的 <see cref="T:System.Data.Linq.Table`1"/> 实例。
        /// </returns>
        /// <param name="entity">要返回其原始状态的实体。</param><filterpriority>2</filterpriority>
        public TEntity GetOriginalEntityState(TEntity entity)
        {
            if ((object)entity == null)
                throw Error.ArgumentNull("entity");
            MetaType metaType = this.Context.Mapping.GetMetaType(entity.GetType());
            if (metaType == null || !metaType.IsEntity)
                throw Error.EntityIsTheWrongType();
            TrackedObject trackedObject1 = this.Context.Services.ChangeTracker.GetTrackedObject((object)entity);
            if (trackedObject1 == null)
                return default(TEntity);
            if (trackedObject1.Original != null)
            {
                TrackedObject trackedObject2 = trackedObject1;
                object original = trackedObject2.Original;
                return (TEntity)trackedObject2.CreateDataCopy(original);
            }
            TrackedObject trackedObject3 = trackedObject1;
            object current = trackedObject3.Current;
            return (TEntity)trackedObject3.CreateDataCopy(current);
        }

        object ITable.GetOriginalEntityState(object entity)
        {
            if (entity == null)
                throw Error.ArgumentNull("entity");
            TEntity entity1 = entity as TEntity;
            if ((object)entity1 == null)
                throw Error.EntityIsTheWrongType();
            return (object)this.GetOriginalEntityState(entity1);
        }

        /// <summary>
        /// 返回包含其当前值和原始值的已修改成员的数组。
        /// </summary>
        /// 
        /// <returns>
        /// 包含其当前和原始值的已修改成员的数组。
        /// </returns>
        /// <param name="entity">从其获取数组的实体。</param><filterpriority>2</filterpriority>
        public ModifiedMemberInfo[] GetModifiedMembers(TEntity entity)
        {
            if ((object)entity == null)
                throw Error.ArgumentNull("entity");
            MetaType metaType = this.Context.Mapping.GetMetaType(entity.GetType());
            if (metaType == null || !metaType.IsEntity)
                throw Error.EntityIsTheWrongType();
            TrackedObject trackedObject = this.Context.Services.ChangeTracker.GetTrackedObject((object)entity);
            if (trackedObject != null)
                return System.Linq.Enumerable.ToArray<ModifiedMemberInfo>(trackedObject.GetModifiedMembers());
            return new ModifiedMemberInfo[0];
        }

        ModifiedMemberInfo[] ITable.GetModifiedMembers(object entity)
        {
            if (entity == null)
                throw Error.ArgumentNull("entity");
            TEntity entity1 = entity as TEntity;
            if ((object)entity1 == null)
                throw Error.EntityIsTheWrongType();
            return this.GetModifiedMembers(entity1);
        }

        private void CheckReadOnly()
        {
            if (this.IsReadOnly)
                throw Error.CannotPerformCUDOnReadOnlyTable((object)this.ToString());
        }

        /// <summary>
        /// 返回表示表的字符串。
        /// </summary>
        /// 
        /// <returns>
        /// 表格的字符串表示形式。
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return "Table(" + typeof(TEntity).Name + ")";
        }
    }

    /// <summary>
    /// 当尝试使用正在使用的键将对象添加到标识缓存时引发。
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public class DuplicateKeyException : InvalidOperationException
    {
        private object duplicate;

        /// <summary>
        /// 获取导致异常的对象。
        /// </summary>
        /// 
        /// <returns>
        /// 导致异常的对象。
        /// </returns>
        public object Object
        {
            get
            {
                return this.duplicate;
            }
        }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.DuplicateKeyException"/> 类的新实例。
        /// </summary>
        /// <param name="duplicate">导致引发异常的重复键。</param>
        public DuplicateKeyException(object duplicate)
        {
            this.duplicate = duplicate;
        }

        /// <summary>
        /// 通过引用重复键并提供错误消息来初始化 <see cref="T:System.Data.Linq.DuplicateKeyException"/> 类的新实例。
        /// </summary>
        /// <param name="duplicate">导致引发异常的重复键。</param><param name="message">当引发异常时要显示的消息。</param>
        public DuplicateKeyException(object duplicate, string message)
          : base(message)
        {
            this.duplicate = duplicate;
        }

        /// <summary>
        /// 通过引用重复键、提供错误消息并指定导致引发此异常的异常来初始化 <see cref="T:System.Data.Linq.DuplicateKeyException"/> 类的新实例。
        /// </summary>
        /// <param name="duplicate">导致引发异常的重复键。</param><param name="message">当引发异常时要显示的消息。</param><param name="innerException">导致引发 <see cref="T:System.Data.Linq.DuplicateKeyException"/> 异常的上一个异常。</param>
        public DuplicateKeyException(object duplicate, string message, Exception innerException)
          : base(message, innerException)
        {
            this.duplicate = duplicate;
        }
    }

    internal sealed class DataQuery<T> : IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T>, IEnumerable, IQueryable, IOrderedQueryable, IQueryProvider, IListSource
    {
        private DataContext context;
        private Expression queryExpression;
        private IBindingList cachedList;

        Expression IQueryable.Expression
        {
            get
            {
                return this.queryExpression;
            }
        }

        Type IQueryable.ElementType
        {
            get
            {
                return typeof(T);
            }
        }

        IQueryProvider IQueryable.Provider
        {
            get
            {
                return (IQueryProvider)this;
            }
        }

        bool IListSource.ContainsListCollection
        {
            get
            {
                return false;
            }
        }

        public DataQuery(DataContext context, Expression expression)
        {
            this.context = context;
            this.queryExpression = expression;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull("expression");
            Type elementType = TypeSystem.GetElementType(expression.Type);
            Type type1 = typeof(IQueryable<>);
            Type[] typeArray1 = new Type[1];
            int index1 = 0;
            Type type2 = elementType;
            typeArray1[index1] = type2;
            Type type3 = type1.MakeGenericType(typeArray1);
            if (!type3.IsAssignableFrom(expression.Type))
                throw Error.ExpectedQueryableArgument((object)"expression", (object)type3);
            Type type4 = typeof(DataQuery<>);
            Type[] typeArray2 = new Type[1];
            int index2 = 0;
            Type type5 = elementType;
            typeArray2[index2] = type5;
            Type type6 = type4.MakeGenericType(typeArray2);
            object[] objArray = new object[2];
            int index3 = 0;
            DataContext dataContext = this.context;
            objArray[index3] = (object)dataContext;
            int index4 = 1;
            Expression expression1 = expression;
            objArray[index4] = (object)expression1;
            return (IQueryable)Activator.CreateInstance(type6, objArray);
        }

        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            if (expression == null)
                throw Error.ArgumentNull("expression");
            if (!typeof(IQueryable<S>).IsAssignableFrom(expression.Type))
                throw Error.ExpectedQueryableArgument((object)"expression", (object)typeof(IEnumerable<S>));
            return (IQueryable<S>)new DataQuery<S>(this.context, expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return this.context.Provider.Execute(expression).ReturnValue;
        }

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)this.context.Provider.Execute(expression).ReturnValue;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.context.Provider.Execute(this.queryExpression).ReturnValue).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)this.context.Provider.Execute(this.queryExpression).ReturnValue).GetEnumerator();
        }

        IList IListSource.GetList()
        {
            if (this.cachedList == null)
                this.cachedList = this.GetNewBindingList();
            return (IList)this.cachedList;
        }

        internal IBindingList GetNewBindingList()
        {
            return BindingList.Create<T>(this.context, (IEnumerable<T>)this);
        }

        public override string ToString()
        {
            return this.context.Provider.GetQueryText(this.queryExpression);
        }
    }

    internal static class BindingList
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static IBindingList Create<T>(DataContext context, IEnumerable<T> sequence)
        {
            List<T> list1 = System.Linq.Enumerable.ToList<T>(sequence);
            MetaTable table1 = context.Services.Model.GetTable(typeof(T));
            if (table1 == null)
                return (IBindingList)new SortableBindingList<T>((IList<T>)list1);
            ITable table2 = context.GetTable(table1.RowType.Type);
            Type type1 = typeof(DataBindingList<>);
            Type[] typeArray = new Type[1];
            int index1 = 0;
            Type type2 = table1.RowType.Type;
            typeArray[index1] = type2;
            Type type3 = type1.MakeGenericType(typeArray);
            int num = 52;
            // ISSUE: variable of the null type
            //__Null local1 = null;
            object[] args = new object[2];
            int index2 = 0;
            List<T> list2 = list1;
            args[index2] = (object)list2;
            int index3 = 1;
            ITable table3 = table2;
            args[index3] = (object)table3;
            // ISSUE: variable of the null type
            //__Null local2 = null;
            return (IBindingList)Activator.CreateInstance(type3, (BindingFlags)num, (Binder)null, args, (CultureInfo)null);
        }
    }

    internal class SortableBindingList<T> : BindingList<T>
    {
        private bool isSorted;
        private PropertyDescriptor sortProperty;
        private ListSortDirection sortDirection;

        protected override ListSortDirection SortDirectionCore
        {
            get
            {
                return this.sortDirection;
            }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get
            {
                return this.sortProperty;
            }
        }

        protected override bool IsSortedCore
        {
            get
            {
                return this.isSorted;
            }
        }

        protected override bool SupportsSortingCore
        {
            get
            {
                return true;
            }
        }

        internal SortableBindingList(IList<T> list)
          : base(list)
        {
        }

        protected override void RemoveSortCore()
        {
            this.isSorted = false;
            this.sortProperty = (PropertyDescriptor)null;
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            if (!SortableBindingList<T>.PropertyComparer.IsAllowable(prop.PropertyType))
                return;
            ((List<T>)this.Items).Sort((IComparer<T>)new SortableBindingList<T>.PropertyComparer(prop, direction));
            this.sortDirection = direction;
            this.sortProperty = prop;
            this.isSorted = true;
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        internal class PropertyComparer : System.Collections.Generic.Comparer<T>
        {
            private PropertyDescriptor prop;
            private IComparer comparer;
            private ListSortDirection direction;
            private bool useToString;

            internal PropertyComparer(PropertyDescriptor prop, ListSortDirection direction)
            {
                if (prop.ComponentType != typeof(T))
                    throw new MissingMemberException(typeof(T).Name, prop.Name);
                this.prop = prop;
                this.direction = direction;
                if (SortableBindingList<T>.PropertyComparer.OkWithIComparable(prop.PropertyType))
                {
                    Type type = typeof(System.Collections.Generic.Comparer<>);
                    Type[] typeArray = new Type[1];
                    int index = 0;
                    Type propertyType = prop.PropertyType;
                    typeArray[index] = propertyType;
                    this.comparer = (IComparer)type.MakeGenericType(typeArray).GetProperty("Default").GetValue((object)null, (object[])null);
                    this.useToString = false;
                }
                else
                {
                    if (!SortableBindingList<T>.PropertyComparer.OkWithToString(prop.PropertyType))
                        return;
                    this.comparer = (IComparer)StringComparer.CurrentCultureIgnoreCase;
                    this.useToString = true;
                }
            }

            public override int Compare(T x, T y)
            {
                object obj1 = this.prop.GetValue((object)x);
                object obj2 = this.prop.GetValue((object)y);
                if (this.useToString)
                {
                    obj1 = obj1 != null ? (object)obj1.ToString() : (object)(string)null;
                    obj2 = obj2 != null ? (object)obj2.ToString() : (object)(string)null;
                }
                if (this.direction == ListSortDirection.Ascending)
                    return this.comparer.Compare(obj1, obj2);
                return this.comparer.Compare(obj2, obj1);
            }

            protected static bool OkWithToString(Type t)
            {
                if (!t.Equals(typeof(XNode)))
                    return t.IsSubclassOf(typeof(XNode));
                return true;
            }

            protected static bool OkWithIComparable(Type t)
            {
                if (t.GetInterface("IComparable") != (Type)null)
                    return true;
                if (t.IsGenericType)
                    return t.GetGenericTypeDefinition() == typeof(Nullable<>);
                return false;
            }

            public static bool IsAllowable(Type t)
            {
                if (!SortableBindingList<T>.PropertyComparer.OkWithToString(t))
                    return SortableBindingList<T>.PropertyComparer.OkWithIComparable(t);
                return true;
            }
        }
    }

    internal class DataBindingList<TEntity> : SortableBindingList<TEntity> where TEntity : class
    {
        private Table<TEntity> data;
        private TEntity addNewInstance;
        private TEntity cancelNewInstance;
        private bool addingNewInstance;

        internal DataBindingList(IList<TEntity> sequence, Table<TEntity> data)
          : base(sequence != null ? sequence : (IList<TEntity>)new List<TEntity>())
        {
            if (sequence == null)
                throw Error.ArgumentNull("sequence");
            if (data == null)
                throw Error.ArgumentNull("data");
            this.data = data;
        }

        protected override object AddNewCore()
        {
            this.addingNewInstance = true;
            this.addNewInstance = (TEntity)base.AddNewCore();
            return (object)this.addNewInstance;
        }

        protected override void InsertItem(int index, TEntity item)
        {
            base.InsertItem(index, item);
            if (this.addingNewInstance || index < 0 || index > this.Count)
                return;
            this.data.InsertOnSubmit(item);
        }

        protected override void RemoveItem(int index)
        {
            if (index >= 0 && index < this.Count && (object)this[index] == (object)this.cancelNewInstance)
                this.cancelNewInstance = default(TEntity);
            else
                this.data.DeleteOnSubmit(this[index]);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TEntity item)
        {
            TEntity entity = this[index];
            base.SetItem(index, item);
            if (index < 0 || index >= this.Count)
                return;
            if ((object)entity == (object)this.addNewInstance)
            {
                this.addNewInstance = default(TEntity);
                this.addingNewInstance = false;
            }
            else
                this.data.DeleteOnSubmit(entity);
            this.data.InsertOnSubmit(item);
        }

        protected override void ClearItems()
        {
            this.data.DeleteAllOnSubmit<TEntity>((IEnumerable<TEntity>)System.Linq.Enumerable.ToList<TEntity>((IEnumerable<TEntity>)this.data));
            base.ClearItems();
        }

        public override void EndNew(int itemIndex)
        {
            if (itemIndex >= 0 && itemIndex < this.Count && (object)this[itemIndex] == (object)this.addNewInstance)
            {
                this.data.InsertOnSubmit(this.addNewInstance);
                this.addNewInstance = default(TEntity);
                this.addingNewInstance = false;
            }
            base.EndNew(itemIndex);
        }

        public override void CancelNew(int itemIndex)
        {
            if (itemIndex >= 0 && itemIndex < this.Count && (object)this[itemIndex] == (object)this.addNewInstance)
            {
                this.cancelNewInstance = this.addNewInstance;
                this.addNewInstance = default(TEntity);
                this.addingNewInstance = false;
            }
            base.CancelNew(itemIndex);
        }
    }
}
