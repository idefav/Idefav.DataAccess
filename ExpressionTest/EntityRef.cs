using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 为 LINQ to SQL 应用程序中的一对多关系的单一实例方提供延迟加载和关系维护。
    /// </summary>
    /// <typeparam name="TEntity">目标实体的类型。</typeparam>
    public struct EntityRef<TEntity> where TEntity : class
    {
        private IEnumerable<TEntity> source;
        private TEntity entity;

        /// <summary>
        /// 获取或设置目标实体。
        /// </summary>
        /// 
        /// <returns>
        /// 目标实体。
        /// </returns>
        public TEntity Entity
        {
            get
            {
                if (this.HasSource)
                {
                    this.entity = System.Linq.Enumerable.SingleOrDefault<TEntity>(this.source);
                    this.source = SourceState<TEntity>.Loaded;
                }
                return this.entity;
            }
            set
            {
                this.entity = value;
                this.source = SourceState<TEntity>.Assigned;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示是否已加载或分配目标。
        /// </summary>
        /// 
        /// <returns>
        /// 如果已加载或分配目标，则为 True。
        /// </returns>
        public bool HasLoadedOrAssignedValue
        {
            get
            {
                if (!this.HasLoadedValue)
                    return this.HasAssignedValue;
                return true;
            }
        }

        internal bool HasValue
        {
            get
            {
                if (this.source != null && !this.HasLoadedValue)
                    return this.HasAssignedValue;
                return true;
            }
        }

        internal bool HasLoadedValue
        {
            get
            {
                return this.source == SourceState<TEntity>.Loaded;
            }
        }

        internal bool HasAssignedValue
        {
            get
            {
                return this.source == SourceState<TEntity>.Assigned;
            }
        }

        internal bool HasSource
        {
            get
            {
                if (this.source != null && !this.HasLoadedValue)
                    return !this.HasAssignedValue;
                return false;
            }
        }

        internal IEnumerable<TEntity> Source
        {
            get
            {
                return this.source;
            }
        }

        internal TEntity UnderlyingValue
        {
            get
            {
                return this.entity;
            }
        }

        /// <summary>
        /// 通过指定目标实体初始化 <see cref="T:System.Data.Linq.EntityRef`1"/> 类的新实例。
        /// </summary>
        /// <param name="entity">目标实体。</param>
        public EntityRef(TEntity entity)
        {
            this.entity = entity;
            this.source = SourceState<TEntity>.Assigned;
        }

        /// <summary>
        /// 通过指定源初始化 <see cref="T:System.Data.Linq.EntityRef`1"/> 类的新实例。
        /// </summary>
        /// <param name="source">引用源。</param>
        public EntityRef(IEnumerable<TEntity> source)
        {
            this.source = source;
            this.entity = default(TEntity);
        }

        /// <summary>
        /// 通过引用目标实体初始化 <see cref="T:System.Data.Linq.EntityRef`1"/> 类的新实例。
        /// </summary>
        /// <param name="entityRef">目标实体。</param>
        public EntityRef(EntityRef<TEntity> entityRef)
        {
            this.source = entityRef.source;
            this.entity = entityRef.entity;
        }
    }
}
