using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    public sealed class EntitySet<TEntity> : IList, ICollection, IEnumerable, IList<TEntity>, ICollection<TEntity>, IEnumerable<TEntity>, IListSource where TEntity : class
    {
        private IEnumerable<TEntity> source;
        private ItemList<TEntity> entities;
        private ItemList<TEntity> removedEntities;
        private Action<TEntity> onAdd;
        private Action<TEntity> onRemove;
        private TEntity onAddEntity;
        private TEntity onRemoveEntity;
        private int version;
        private ListChangedEventHandler onListChanged;
        private bool isModified;
        private bool isLoaded;
        private bool listChanged;
        private IBindingList cachedList;

        /// <summary>
        /// 获取 <see cref="T:System.Data.Linq.EntitySet`1"/> 集合中的实体数。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示实体数的整数。
        /// </returns>
        public int Count
        {
            get
            {
                this.Load();
                return this.entities.Count;
            }
        }

        /// <summary>
        /// 获取或设置位于指定索引处的元素。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示项的 <see cref="T:System.Data.Linq.EntitySet`1"/>。
        /// </returns>
        /// <param name="index">元素的索引。</param>
        public TEntity this[int index]
        {
            get
            {
                this.Load();
                if (index < 0 || index >= this.entities.Count)
                    throw Error.ArgumentOutOfRange("index");
                return this.entities[index];
            }
            set
            {
                this.Load();
                if (index < 0 || index >= this.entities.Count)
                    throw Error.ArgumentOutOfRange("index");
                if ((object)value == null || this.IndexOf(value) >= 0)
                    throw Error.ArgumentOutOfRange("value");
                this.CheckModify();
                this.OnRemove(this.entities[index]);
                this.OnListChanged(ListChangedType.ItemDeleted, index);
                this.OnAdd(value);
                this.entities[index] = value;
                this.OnModified();
                this.OnListChanged(ListChangedType.ItemAdded, index);
            }
        }

        /// <summary>
        /// 指定此 <see cref="T:System.Data.Linq.EntitySet`1"/> 是否具有尚未执行的延迟查询。
        /// </summary>
        /// 
        /// <returns>
        /// 如果尚未执行延迟查询，则为 true；否则为 false。
        /// </returns>
        public bool IsDeferred
        {
            get
            {
                return this.HasSource;
            }
        }

        internal bool HasValues
        {
            get
            {
                if (this.source != null && !this.HasAssignedValues)
                    return this.HasLoadedValues;
                return true;
            }
        }

        /// <summary>
        /// 指定 <see cref="T:System.Data.Linq.EntitySet`1"/> 是否已加载或分配某值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果 <see cref="T:System.Data.Linq.EntitySet`1"/> 已加载或分配某值，则返回 true。
        /// </returns>
        public bool HasLoadedOrAssignedValues
        {
            get
            {
                if (!this.HasAssignedValues)
                    return this.HasLoadedValues;
                return true;
            }
        }

        internal bool HasAssignedValues
        {
            get
            {
                return this.isModified;
            }
        }

        internal bool HasLoadedValues
        {
            get
            {
                return this.isLoaded;
            }
        }

        internal bool HasSource
        {
            get
            {
                if (this.source != null)
                    return !this.HasLoadedValues;
                return false;
            }
        }

        internal bool IsLoaded
        {
            get
            {
                return this.isLoaded;
            }
        }

        internal IEnumerable<TEntity> Source
        {
            get
            {
                return this.source;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return (object)this[index];
            }
            set
            {
                TEntity entity = value as TEntity;
                if (value == null)
                    throw Error.ArgumentOutOfRange("value");
                this[index] = entity;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return (object)this;
            }
        }

        bool ICollection<TEntity>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool IListSource.ContainsListCollection
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 当列表的内容发生更改时发生。
        /// </summary>
        public event ListChangedEventHandler ListChanged
        {
            add
            {
                this.onListChanged = this.onListChanged + value;
            }
            remove
            {
                this.onListChanged = this.onListChanged - value;
            }
        }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.EntitySet`1"/> 类的新实例。
        /// </summary>
        public EntitySet()
        {
        }

        /// <summary>
        /// 在为添加和移除操作提供处理程序的同时，初始化 <see cref="T:System.Data.Linq.EntitySet`1"/> 类的新实例。
        /// </summary>
        /// <param name="onAdd"><see cref="M:System.Data.Linq.EntitySet`1.Add(`0)"/> 的委托。</param><param name="onRemove"><see cref="M:System.Data.Linq.EntitySet`1.Remove(`0)"/> 的委托。</param>
        public EntitySet(Action<TEntity> onAdd, Action<TEntity> onRemove)
        {
            this.onAdd = onAdd;
            this.onRemove = onRemove;
        }

        internal EntitySet(EntitySet<TEntity> es, bool copyNotifications)
        {
            this.source = es.source;
            foreach (TEntity entity in es.entities)
                this.entities.Add(entity);
            foreach (TEntity entity in es.removedEntities)
                this.removedEntities.Add(entity);
            this.version = es.version;
            if (!copyNotifications)
                return;
            this.onAdd = es.onAdd;
            this.onRemove = es.onRemove;
        }

        /// <summary>
        /// 添加实体。
        /// </summary>
        /// <param name="entity">要添加的实体。</param>
        public void Add(TEntity entity)
        {
            if ((object)entity == null)
                throw Error.ArgumentNull("entity");
            if ((object)entity == (object)this.onAddEntity)
                return;
            this.CheckModify();
            if (!this.entities.Contains(entity))
            {
                this.OnAdd(entity);
                if (this.HasSource)
                    this.removedEntities.Remove(entity);
                this.entities.Add(entity);
                this.OnListChanged(ListChangedType.ItemAdded, this.entities.IndexOf(entity));
            }
            this.OnModified();
        }

        /// <summary>
        /// 添加实体的集合。
        /// </summary>
        /// <param name="collection">要添加的集合。</param>
        public void AddRange(IEnumerable<TEntity> collection)
        {
            if (collection == null)
                throw Error.ArgumentNull("collection");
            this.CheckModify();
            collection = (IEnumerable<TEntity>)System.Linq.Enumerable.ToList<TEntity>(collection);
            foreach (TEntity entity in collection)
            {
                if (!this.entities.Contains(entity))
                {
                    this.OnAdd(entity);
                    if (this.HasSource)
                        this.removedEntities.Remove(entity);
                    this.entities.Add(entity);
                    this.OnListChanged(ListChangedType.ItemAdded, this.entities.IndexOf(entity));
                }
            }
            this.OnModified();
        }

        /// <summary>
        /// 将 <see cref="T:System.Data.Linq.EntitySet`1"/> 集合分配给其他 <see cref="T:System.Data.Linq.EntitySet`1"/> 集合。
        /// </summary>
        /// <param name="entitySource">要分配的集合。</param>
        public void Assign(IEnumerable<TEntity> entitySource)
        {
            if (this == entitySource)
                return;
            this.Clear();
            if (entitySource != null)
                this.AddRange(entitySource);
            this.isLoaded = true;
        }

        /// <summary>
        /// 移除所有项。
        /// </summary>
        public void Clear()
        {
            this.Load();
            this.CheckModify();
            if (this.entities.Items != null)
            {
                foreach (TEntity entity in new List<TEntity>((IEnumerable<TEntity>)this.entities.Items))
                    this.Remove(entity);
            }
            this.entities = new ItemList<TEntity>();
            this.OnModified();
            this.OnListChanged(ListChangedType.Reset, 0);
        }

        /// <summary>
        /// 指定 <see cref="T:System.Data.Linq.EntitySet`1"/> 是否包含特定实体。
        /// </summary>
        /// 
        /// <returns>
        /// 如果 <see cref="T:System.Data.Linq.EntitySet`1"/> 包含该实体，则为 true；否则为 false。
        /// </returns>
        /// <param name="entity">实体。</param>
        public bool Contains(TEntity entity)
        {
            return this.IndexOf(entity) >= 0;
        }

        /// <summary>
        /// 将 <see cref="T:System.Data.Linq.EntitySet`1"/> 复制到数组。
        /// </summary>
        /// <param name="array">要复制到的数组。</param><param name="arrayIndex">数组中的起始索引。</param>
        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            this.Load();
            if (this.entities.Count <= 0)
                return;
            Array.Copy((Array)this.entities.Items, 0, (Array)array, arrayIndex, this.entities.Count);
        }

        /// <summary>
        /// 返回一个循环访问集合的枚举器。
        /// </summary>
        /// 
        /// <returns>
        /// 一个 <see cref="T:System.Collections.Generic.IEnumerator`1"/>。
        /// </returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            this.Load();
            return (IEnumerator<TEntity>)new EntitySet<TEntity>.Enumerator(this);
        }

        internal IEnumerable<TEntity> GetUnderlyingValues()
        {
            return (IEnumerable<TEntity>)new EntitySet<TEntity>.UnderlyingValues(this);
        }

        /// <summary>
        /// 返回实体的索引。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示索引的整数。
        /// </returns>
        /// <param name="entity">要返回其索引的实体。</param>
        public int IndexOf(TEntity entity)
        {
            this.Load();
            return this.entities.IndexOf(entity);
        }

        /// <summary>
        /// 在索引位置处插入实体。
        /// </summary>
        /// <param name="index">表示要插入实体的位置的索引。</param><param name="entity">要插入的实体。</param>
        public void Insert(int index, TEntity entity)
        {
            this.Load();
            if (index < 0 || index > this.Count)
                throw Error.ArgumentOutOfRange("index");
            if ((object)entity == null || this.IndexOf(entity) >= 0)
                throw Error.ArgumentOutOfRange("entity");
            this.CheckModify();
            this.entities.Insert(index, entity);
            this.OnListChanged(ListChangedType.ItemAdded, index);
            this.OnAdd(entity);
        }

        /// <summary>
        /// 加载 <see cref="T:System.Data.Linq.EntitySet`1"/>。
        /// </summary>
        public void Load()
        {
            if (!this.HasSource)
                return;
            ItemList<TEntity> itemList = this.entities;
            this.entities = new ItemList<TEntity>();
            foreach (TEntity entity in this.source)
                this.entities.Add(entity);
            foreach (TEntity entity in itemList)
                this.entities.Include(entity);
            foreach (TEntity entity in this.removedEntities)
                this.entities.Remove(entity);
            this.source = SourceState<TEntity>.Loaded;
            this.isLoaded = true;
            this.removedEntities = new ItemList<TEntity>();
        }

        private void OnModified()
        {
            this.isModified = true;
        }

        /// <summary>
        /// 移除实体。
        /// </summary>
        /// 
        /// <returns>
        /// 如果已成功移除该实体，则为 true；否则为 false。
        /// </returns>
        /// <param name="entity">要移除的实体。</param>
        public bool Remove(TEntity entity)
        {
            if ((object)entity == null || (object)entity == (object)this.onRemoveEntity)
                return false;
            this.CheckModify();
            int index = -1;
            bool flag = false;
            if (this.HasSource)
            {
                if (!this.removedEntities.Contains(entity))
                {
                    this.OnRemove(entity);
                    index = this.entities.IndexOf(entity);
                    if (index != -1)
                        this.entities.RemoveAt(index);
                    else
                        this.removedEntities.Add(entity);
                    flag = true;
                }
            }
            else
            {
                index = this.entities.IndexOf(entity);
                if (index != -1)
                {
                    this.OnRemove(entity);
                    this.entities.RemoveAt(index);
                    flag = true;
                }
            }
            if (flag)
            {
                this.OnModified();
                if (index != -1)
                    this.OnListChanged(ListChangedType.ItemDeleted, index);
            }
            return flag;
        }

        /// <summary>
        /// 移除指定索引处的实体。
        /// </summary>
        /// <param name="index">要移除的实体的索引。</param>
        public void RemoveAt(int index)
        {
            this.Load();
            if (index < 0 || index >= this.Count)
                throw Error.ArgumentOutOfRange("index");
            this.CheckModify();
            this.OnRemove(this.entities[index]);
            this.entities.RemoveAt(index);
            this.OnModified();
            this.OnListChanged(ListChangedType.ItemDeleted, index);
        }

        /// <summary>
        /// 设置 <see cref="T:System.Data.Linq.EntitySet`1"/> 的源。
        /// </summary>
        /// <param name="entitySource"><see cref="T:System.Data.Linq.EntitySet`1"/> 的源。</param>
        public void SetSource(IEnumerable<TEntity> entitySource)
        {
            if (this.HasAssignedValues || this.HasLoadedValues)
                throw Error.EntitySetAlreadyLoaded();
            this.source = entitySource;
        }

        private void CheckModify()
        {
            if ((object)this.onAddEntity != null || (object)this.onRemoveEntity != null)
                throw Error.ModifyDuringAddOrRemove();
            this.version = this.version + 1;
        }

        private void OnAdd(TEntity entity)
        {
            if (this.onAdd == null)
                return;
            TEntity entity1 = this.onAddEntity;
            this.onAddEntity = entity;
            try
            {
                this.onAdd(entity);
            }
            finally
            {
                this.onAddEntity = entity1;
            }
        }

        private void OnRemove(TEntity entity)
        {
            if (this.onRemove == null)
                return;
            TEntity entity1 = this.onRemoveEntity;
            this.onRemoveEntity = entity;
            try
            {
                this.onRemove(entity);
            }
            finally
            {
                this.onRemoveEntity = entity1;
            }
        }

        int IList.Add(object value)
        {
            TEntity entity = value as TEntity;
            if ((object)entity == null || this.IndexOf(entity) >= 0)
                throw Error.ArgumentOutOfRange("value");
            this.CheckModify();
            int count = this.entities.Count;
            this.entities.Add(entity);
            this.OnAdd(entity);
            return count;
        }

        bool IList.Contains(object value)
        {
            return this.Contains(value as TEntity);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf(value as TEntity);
        }

        void IList.Insert(int index, object value)
        {
            TEntity entity = value as TEntity;
            if (value == null)
                throw Error.ArgumentOutOfRange("value");
            this.Insert(index, entity);
        }

        void IList.Remove(object value)
        {
            this.Remove(value as TEntity);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.Load();
            if (this.entities.Count <= 0)
                return;
            Array.Copy((Array)this.entities.Items, 0, array, index, this.entities.Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.GetEnumerator();
        }

        private void OnListChanged(ListChangedType type, int index)
        {
            this.listChanged = true;
            if (this.onListChanged == null)
                return;
            this.onListChanged((object)this, new ListChangedEventArgs(type, index));
        }

        IList IListSource.GetList()
        {
            if (this.cachedList == null || this.listChanged)
            {
                this.cachedList = this.GetNewBindingList();
                this.listChanged = false;
            }
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
            return (IBindingList)new EntitySetBindingList<TEntity>((IList<TEntity>)System.Linq.Enumerable.ToList<TEntity>((IEnumerable<TEntity>)this), this);
        }

        private class UnderlyingValues : IEnumerable<TEntity>, IEnumerable
        {
            private EntitySet<TEntity> entitySet;

            internal UnderlyingValues(EntitySet<TEntity> entitySet)
            {
                this.entitySet = entitySet;
            }

            public IEnumerator<TEntity> GetEnumerator()
            {
                return (IEnumerator<TEntity>)new EntitySet<TEntity>.Enumerator(this.entitySet);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator)this.GetEnumerator();
            }
        }

        private class Enumerable : IEnumerable<TEntity>, IEnumerable
        {
            private EntitySet<TEntity> entitySet;

            public Enumerable(EntitySet<TEntity> entitySet)
            {
                this.entitySet = entitySet;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator)this.GetEnumerator();
            }

            public IEnumerator<TEntity> GetEnumerator()
            {
                return (IEnumerator<TEntity>)new EntitySet<TEntity>.Enumerator(this.entitySet);
            }
        }

        private class Enumerator : IEnumerator<TEntity>, IDisposable, IEnumerator
        {
            private EntitySet<TEntity> entitySet;
            private TEntity[] items;
            private int index;
            private int endIndex;
            private int version;

            public TEntity Current
            {
                get
                {
                    return this.items[this.index];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return (object)this.items[this.index];
                }
            }

            public Enumerator(EntitySet<TEntity> entitySet)
            {
                this.entitySet = entitySet;
                this.items = entitySet.entities.Items;
                this.index = -1;
                this.endIndex = entitySet.entities.Count - 1;
                this.version = entitySet.version;
            }

            public void Dispose()
            {
                GC.SuppressFinalize((object)this);
            }

            public bool MoveNext()
            {
                if (this.version != this.entitySet.version)
                    throw Error.EntitySetModifiedDuringEnumeration();
                if (this.index == this.endIndex)
                    return false;
                this.index = this.index + 1;
                return true;
            }

            void IEnumerator.Reset()
            {
                if (this.version != this.entitySet.version)
                    throw Error.EntitySetModifiedDuringEnumeration();
                this.index = -1;
            }
        }
    }

    internal class EntitySetBindingList<TEntity> : SortableBindingList<TEntity> where TEntity : class
    {
        private EntitySet<TEntity> data;
        private TEntity addNewInstance;
        private TEntity cancelNewInstance;
        private bool addingNewInstance;

        internal EntitySetBindingList(IList<TEntity> sequence, EntitySet<TEntity> data)
          : base(sequence)
        {
            if (sequence == null)
                throw Error.ArgumentNull("sequence");
            if (data == null)
                throw Error.ArgumentNull("data");
            this.data = data;
        }

        private void ThrowEntitySetErrorsIfTypeInappropriate()
        {
            Type type = typeof(TEntity);
            if (type.IsAbstract)
                throw Error.EntitySetDataBindingWithAbstractBaseClass((object)type.Name);
            if (type.GetConstructor(Type.EmptyTypes) == (ConstructorInfo)null)
                throw Error.EntitySetDataBindingWithNonPublicDefaultConstructor((object)type.Name);
        }

        protected override object AddNewCore()
        {
            this.ThrowEntitySetErrorsIfTypeInappropriate();
            this.addingNewInstance = true;
            this.addNewInstance = (TEntity)base.AddNewCore();
            return (object)this.addNewInstance;
        }

        protected override void InsertItem(int index, TEntity item)
        {
            base.InsertItem(index, item);
            if (this.addingNewInstance || index < 0 || index > this.Count)
                return;
            this.data.Insert(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (index >= 0 && index < this.Count && (object)this[index] == (object)this.cancelNewInstance)
                this.cancelNewInstance = default(TEntity);
            else
                this.data.Remove(this[index]);
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
                this.data.Remove(entity);
            this.data.Insert(index, item);
        }

        protected override void ClearItems()
        {
            this.data.Clear();
            base.ClearItems();
        }

        public override void EndNew(int itemIndex)
        {
            if (itemIndex >= 0 && itemIndex < this.Count && (object)this[itemIndex] == (object)this.addNewInstance)
            {
                this.data.Add(this.addNewInstance);
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

    internal static class SourceState<T>
    {
        internal static readonly IEnumerable<T> Loaded = (IEnumerable<T>)new T[0];
        internal static readonly IEnumerable<T> Assigned = (IEnumerable<T>)new T[0];
    }

    internal struct ItemList<T> where T : class
    {
        private T[] items;
        private int count;

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public T[] Items
        {
            get
            {
                return this.items;
            }
        }

        public T this[int index]
        {
            get
            {
                return this.items[index];
            }
            set
            {
                this.items[index] = value;
            }
        }

        public void Add(T item)
        {
            if (this.items == null || this.items.Length == this.count)
                this.GrowItems();
            this.items[this.count] = item;
            this.count = this.count + 1;
        }

        public bool Contains(T item)
        {
            return this.IndexOf(item) >= 0;
        }

        public ItemList<T>.Enumerator GetEnumerator()
        {
            ItemList<T>.Enumerator enumerator;
            enumerator.items = this.items;
            enumerator.index = -1;
            enumerator.endIndex = this.count - 1;
            return enumerator;
        }

        public bool Include(T item)
        {
            if (this.LastIndexOf(item) >= 0)
                return false;
            this.Add(item);
            return true;
        }

        public int IndexOf(T item)
        {
            for (int index = 0; index < this.count; ++index)
            {
                if ((object)this.items[index] == (object)item)
                    return index;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            if (this.items == null || this.items.Length == this.count)
                this.GrowItems();
            if (index < this.count)
                Array.Copy((Array)this.items, index, (Array)this.items, index + 1, this.count - index);
            this.items[index] = item;
            this.count = this.count + 1;
        }

        public int LastIndexOf(T item)
        {
            int index = this.count;
            while (index > 0)
            {
                --index;
                if ((object)this.items[index] == (object)item)
                    return index;
            }
            return -1;
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index < 0)
                return false;
            this.RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            this.count = this.count - 1;
            if (index < this.count)
                Array.Copy((Array)this.items, index + 1, (Array)this.items, index, this.count - index);
            this.items[this.count] = default(T);
        }

        private void GrowItems()
        {
            Array.Resize<T>(ref this.items, this.count == 0 ? 4 : this.count * 2);
        }

        public struct Enumerator
        {
            internal T[] items;
            internal int index;
            internal int endIndex;

            public T Current
            {
                get
                {
                    return this.items[this.index];
                }
            }

            public bool MoveNext()
            {
                if (this.index == this.endIndex)
                    return false;
                this.index = this.index + 1;
                return true;
            }
        }
    }
}
