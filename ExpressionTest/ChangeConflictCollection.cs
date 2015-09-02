using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 返回并发冲突中涉及的对象的集合。
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public sealed class ChangeConflictCollection : ICollection<ObjectChangeConflict>, IEnumerable<ObjectChangeConflict>, IEnumerable, ICollection
    {
        private List<ObjectChangeConflict> conflicts;

        /// <summary>
        /// 返回集合中的冲突数。
        /// </summary>
        /// 
        /// <returns>
        /// 整数
        /// </returns>
        public int Count
        {
            get
            {
                return this.conflicts.Count;
            }
        }

        /// <summary>
        /// 返回发生冲突的项。
        /// </summary>
        /// 
        /// <returns>
        /// 一个表示发生冲突的项的 <see cref="T:System.Data.Linq.ObjectChangeConflict"/>。
        /// </returns>
        /// <param name="index">发生冲突的项的集合中的索引。</param>
        public ObjectChangeConflict this[int index]
        {
            get
            {
                return this.conflicts[index];
            }
        }

        bool ICollection<ObjectChangeConflict>.IsReadOnly
        {
            get
            {
                return true;
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
                return (object)null;
            }
        }

        internal ChangeConflictCollection()
        {
            this.conflicts = new List<ObjectChangeConflict>();
        }

        void ICollection<ObjectChangeConflict>.Add(ObjectChangeConflict item)
        {
            throw Error.CannotAddChangeConflicts();
        }

        /// <summary>
        /// 指定是否从集合中移除指定冲突。
        /// </summary>
        /// 
        /// <returns>
        /// 如果从集合移除 <see cref="T:System.Data.Linq.ObjectChangeConflict"/>，则返回 true。
        /// </returns>
        /// <param name="item">要移除的冲突。</param>
        public bool Remove(ObjectChangeConflict item)
        {
            return this.conflicts.Remove(item);
        }

        /// <summary>
        /// 从集合中移除所有冲突。
        /// </summary>
        public void Clear()
        {
            this.conflicts.Clear();
        }

        /// <summary>
        /// 指定给定冲突是否为集合的成员。
        /// </summary>
        /// 
        /// <returns>
        /// 如果指定的冲突是集合的一个成员，则返回 true。
        /// </returns>
        /// <param name="item">指定的冲突。</param>
        public bool Contains(ObjectChangeConflict item)
        {
            return this.conflicts.Contains(item);
        }

        /// <summary>
        /// 有关此成员的说明，请参见 <see cref="M:System.Collections.ICollection.CopyTo(System.Array,System.Int32)"/>。
        /// </summary>
        /// <param name="array">要复制到的数组。</param><param name="arrayIndex">从其开始复制的数组索引。</param>
        public void CopyTo(ObjectChangeConflict[] array, int arrayIndex)
        {
            this.conflicts.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 返回集合的枚举器。
        /// </summary>
        /// 
        /// <returns>
        /// 集合的枚举器。
        /// </returns>
        public IEnumerator<ObjectChangeConflict> GetEnumerator()
        {
            return (IEnumerator<ObjectChangeConflict>)this.conflicts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.conflicts.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)this.conflicts).CopyTo(array, index);
        }

        /// <summary>
        /// 通过使用指定策略解决集合中的所有冲突。
        /// </summary>
        /// <param name="mode"><see cref="T:System.Data.Linq.RefreshMode"/> 中可用的选项之一。</param>
        public void ResolveAll(RefreshMode mode)
        {
            this.ResolveAll(mode, true);
        }

        /// <summary>
        /// 通过使用指定策略解决集合中的所有冲突。
        /// </summary>
        /// <param name="mode">用于解决冲突的策略。</param><param name="autoResolveDeletes">如果为 true，则自动解决由数据库中不再存在的已修改对象产生的冲突。</param>
        public void ResolveAll(RefreshMode mode, bool autoResolveDeletes)
        {
            foreach (ObjectChangeConflict objectChangeConflict in this.conflicts)
            {
                if (!objectChangeConflict.IsResolved)
                    objectChangeConflict.Resolve(mode, autoResolveDeletes);
            }
        }

        internal void Fill(List<ObjectChangeConflict> conflictList)
        {
            this.conflicts = conflictList;
        }
    }
}
