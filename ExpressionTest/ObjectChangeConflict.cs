using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 表示对一个或多个开放式并发冲突的更新尝试。
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public sealed class ObjectChangeConflict
    {
        private ChangeConflictSession session;
        private TrackedObject trackedObject;
        private bool isResolved;
        private ReadOnlyCollection<MemberChangeConflict> memberConflicts;
        private object database;
        private object original;
        private bool? isDeleted;

        internal ChangeConflictSession Session
        {
            get
            {
                return this.session;
            }
        }

        internal TrackedObject TrackedObject
        {
            get
            {
                return this.trackedObject;
            }
        }

        /// <summary>
        /// 获取发生冲突的对象。
        /// </summary>
        /// 
        /// <returns>
        /// 发生冲突的对象。
        /// </returns>
        public object Object
        {
            get
            {
                return this.trackedObject.Current;
            }
        }

        internal object Original
        {
            get
            {
                return this.original;
            }
        }

        /// <summary>
        /// 获取指示是否已解决此对象的冲突的值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果冲突已解决，则为 True。
        /// </returns>
        public bool IsResolved
        {
            get
            {
                return this.isResolved;
            }
        }

        /// <summary>
        /// 获取指示是否已从数据库中删除发生冲突的对象的值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果该对象已被删除，则为 True。
        /// </returns>
        public bool IsDeleted
        {
            get
            {
                if (this.isDeleted.HasValue)
                    return this.isDeleted.Value;
                return this.Database == null;
            }
        }

        internal object Database
        {
            get
            {
                if (this.database == null)
                    this.database = this.session.RefreshContext.Services.GetObjectByKey(this.trackedObject.Type, CommonDataServices.GetKeyValues(this.trackedObject.Type, this.original));
                return this.database;
            }
        }

        /// <summary>
        /// 获取导致更新失败的所有成员冲突的集合。
        /// </summary>
        /// 
        /// <returns>
        /// 成员冲突的集合。
        /// </returns>
        public ReadOnlyCollection<MemberChangeConflict> MemberConflicts
        {
            get
            {
                if (this.memberConflicts == null)
                {
                    List<MemberChangeConflict> list = new List<MemberChangeConflict>();
                    if (this.Database != null)
                    {
                        foreach (MetaDataMember metaDataMember in this.trackedObject.Type.PersistentDataMembers)
                        {
                            if (!metaDataMember.IsAssociation && this.HasMemberConflict(metaDataMember))
                                list.Add(new MemberChangeConflict(this, metaDataMember));
                        }
                    }
                    this.memberConflicts = list.AsReadOnly();
                }
                return this.memberConflicts;
            }
        }

        internal ObjectChangeConflict(ChangeConflictSession session, TrackedObject trackedObject)
        {
            this.session = session;
            this.trackedObject = trackedObject;
            TrackedObject trackedObject1 = trackedObject;
            object original = trackedObject1.Original;
            this.original = trackedObject1.CreateDataCopy(original);
        }

        internal ObjectChangeConflict(ChangeConflictSession session, TrackedObject trackedObject, bool isDeleted)
          : this(session, trackedObject)
        {
            this.isDeleted = new bool?(isDeleted);
        }

        /// <summary>
        /// 通过保留当前值并重置基线原始值以匹配较新数据库值来解决成员冲突。
        /// </summary>
        public void Resolve()
        {
            this.Resolve(RefreshMode.KeepCurrentValues, true);
        }

        /// <summary>
        /// 使用指定的 <see cref="T:System.Data.Linq.RefreshMode"/> 来解决成员冲突。
        /// </summary>
        /// <param name="refreshMode"><see cref="T:System.Data.Linq.RefreshMode"/> 的相应选择。</param>
        public void Resolve(RefreshMode refreshMode)
        {
            this.Resolve(refreshMode, false);
        }

        /// <summary>
        /// 解决保留当前值且表示基线原始值的成员冲突。
        /// </summary>
        /// <param name="refreshMode"><see cref="T:System.Data.Linq.RefreshMode"/> 的相应选择。</param><param name="autoResolveDeletes">如果为 true，则自动解决由数据库中不再存在的已修改对象产生的冲突。</param>
        public void Resolve(RefreshMode refreshMode, bool autoResolveDeletes)
        {
            if (autoResolveDeletes && this.IsDeleted)
            {
                this.ResolveDelete();
            }
            else
            {
                if (this.Database == null)
                    throw Error.RefreshOfDeletedObject();
                this.trackedObject.Refresh(refreshMode, this.Database);
                this.isResolved = true;
            }
        }

        private void ResolveDelete()
        {
            if (!this.trackedObject.IsDeleted)
                this.trackedObject.ConvertToDeleted();
            this.Session.Context.Services.RemoveCachedObjectLike(this.trackedObject.Type, this.trackedObject.Original);
            this.trackedObject.AcceptChanges();
            this.isResolved = true;
        }

        private bool HasMemberConflict(MetaDataMember member)
        {
            object boxedValue1 = member.StorageAccessor.GetBoxedValue(this.original);
            if (!member.DeclaringType.Type.IsAssignableFrom(this.database.GetType()))
                return false;
            object boxedValue2 = member.StorageAccessor.GetBoxedValue(this.database);
            return !this.AreEqual(member, boxedValue1, boxedValue2);
        }

        private bool AreEqual(MetaDataMember member, object v1, object v2)
        {
            if (v1 == null && v2 == null)
                return true;
            if (v1 == null || v2 == null)
                return false;
            if (member.Type == typeof(char[]))
                return this.AreEqual((char[])v1, (char[])v2);
            if (member.Type == typeof(byte[]))
                return this.AreEqual((byte[])v1, (byte[])v2);
            return object.Equals(v1, v2);
        }

        private bool AreEqual(char[] a1, char[] a2)
        {
            if (a1.Length != a2.Length)
                return false;
            int index = 0;
            for (int length = a1.Length; index < length; ++index)
            {
                if ((int)a1[index] != (int)a2[index])
                    return false;
            }
            return true;
        }

        private bool AreEqual(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;
            int index = 0;
            for (int length = a1.Length; index < length; ++index)
            {
                if ((int)a1[index] != (int)a2[index])
                    return false;
            }
            return true;
        }

        internal void OnMemberResolved()
        {
            if (this.IsResolved)
                return;
            IEnumerable<MemberChangeConflict> source = System.Linq.Enumerable.AsEnumerable<MemberChangeConflict>((IEnumerable<MemberChangeConflict>)this.memberConflicts);
            Func<MemberChangeConflict, bool> func = (Func<MemberChangeConflict, bool>)(m => m.IsResolved);
            Func<MemberChangeConflict, bool> predicate = null;
            if (Enumerable.Count<MemberChangeConflict>(source, predicate) != this.memberConflicts.Count)
                return;
            this.Resolve(RefreshMode.KeepCurrentValues, false);
        }
    }
}
