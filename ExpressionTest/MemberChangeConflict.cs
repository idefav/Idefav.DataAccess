using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 表示以下情况：由于自客户端上次读取成员值后这些值已被更新，因此尝试的更新失败。
    /// </summary>
    public sealed class MemberChangeConflict
    {
        private ObjectChangeConflict conflict;
        private MetaDataMember metaMember;
        private object originalValue;
        private object databaseValue;
        private object currentValue;
        private bool isResolved;

        /// <summary>
        /// 获取发生冲突的成员的原始值。
        /// </summary>
        /// 
        /// <returns>
        /// 发生冲突的成员的原始值。
        /// </returns>
        public object OriginalValue
        {
            get
            {
                return this.originalValue;
            }
        }

        /// <summary>
        /// 获取发生冲突的成员的数据库值。
        /// </summary>
        /// 
        /// <returns>
        /// 发生冲突的对象的值。
        /// </returns>
        public object DatabaseValue
        {
            get
            {
                return this.databaseValue;
            }
        }

        /// <summary>
        /// 获取发生冲突的成员的当前值。
        /// </summary>
        /// 
        /// <returns>
        /// 发生冲突的对象。
        /// </returns>
        public object CurrentValue
        {
            get
            {
                return this.currentValue;
            }
        }

        /// <summary>
        /// 获取有关发生冲突的成员的元数据信息。
        /// </summary>
        /// 
        /// <returns>
        /// 有关发生冲突的成员的信息。
        /// </returns>
        public MemberInfo Member
        {
            get
            {
                return this.metaMember.Member;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示自上次读取或刷新数据库后，是否已更改成员数据。
        /// </summary>
        /// 
        /// <returns>
        /// 如果成员数据已更改，则为 True。
        /// </returns>
        public bool IsModified
        {
            get
            {
                return this.conflict.TrackedObject.HasChangedValue(this.metaMember);
            }
        }

        /// <summary>
        /// 获取一个值，该值指示是否已解决冲突。
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

        internal MemberChangeConflict(ObjectChangeConflict conflict, MetaDataMember metaMember)
        {
            this.conflict = conflict;
            this.metaMember = metaMember;
            this.originalValue = metaMember.StorageAccessor.GetBoxedValue(conflict.Original);
            this.databaseValue = metaMember.StorageAccessor.GetBoxedValue(conflict.Database);
            this.currentValue = metaMember.StorageAccessor.GetBoxedValue(conflict.TrackedObject.Current);
        }

        /// <summary>
        /// 指定要设置为发生冲突的成员的当前值的值。
        /// </summary>
        /// <param name="value">要设置为当前值的值。</param>
        public void Resolve(object value)
        {
            this.conflict.TrackedObject.RefreshMember(this.metaMember, RefreshMode.OverwriteCurrentValues, value);
            this.isResolved = true;
            this.conflict.OnMemberResolved();
        }

        /// <summary>
        /// 使用 <see cref="T:System.Data.Linq.RefreshMode"/> 参数可自动指定要设置为发生冲突的成员的当前值的值。
        /// </summary>
        /// <param name="refreshMode">请参见<see cref="T:System.Data.Linq.RefreshMode"/>。</param>
        public void Resolve(RefreshMode refreshMode)
        {
            this.conflict.TrackedObject.RefreshMember(this.metaMember, refreshMode, this.databaseValue);
            this.isResolved = true;
            this.conflict.OnMemberResolved();
        }
    }
}
