using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 用于启用单个属性（类似于 <see cref="T:System.Data.Linq.EntityRef`1"/>）的延迟加载。
    /// </summary>
    /// <typeparam name="T">延迟源中的元素的类型。</typeparam>
    public struct Link<T>
    {
        private T underlyingValue;
        private IEnumerable<T> source;

        /// <summary>
        /// 获取指示源是否包含值的值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果源包含已分配或加载值（包括 null），则返回 true。
        /// </returns>
        public bool HasValue
        {
            get
            {
                if (this.source != null && !this.HasLoadedValue)
                    return this.HasAssignedValue;
                return true;
            }
        }

        /// <summary>
        /// 指定 <see cref="T:System.Data.Linq.Link`1"/> 是否已加载或分配某值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果 <see cref="T:System.Data.Linq.Link`1"/> 已加载或分配某值，则为 true；否则为 false。
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

        internal bool HasLoadedValue
        {
            get
            {
                return this.source == SourceState<T>.Loaded;
            }
        }

        internal bool HasAssignedValue
        {
            get
            {
                return this.source == SourceState<T>.Assigned;
            }
        }

        internal T UnderlyingValue
        {
            get
            {
                return this.underlyingValue;
            }
        }

        internal IEnumerable<T> Source
        {
            get
            {
                return this.source;
            }
        }

        internal bool HasSource
        {
            get
            {
                if (this.source != null && !this.HasAssignedValue)
                    return !this.HasLoadedValue;
                return false;
            }
        }

        /// <summary>
        /// 获取或设置分配给 <see cref="T:System.Data.Linq.Link`1"/> 或由其加载的值。
        /// </summary>
        /// 
        /// <returns>
        /// 此延迟属性的值。
        /// </returns>
        public T Value
        {
            get
            {
                if (this.HasSource)
                {
                    this.underlyingValue = System.Linq.Enumerable.SingleOrDefault<T>(this.source);
                    this.source = SourceState<T>.Loaded;
                }
                return this.underlyingValue;
            }
            set
            {
                this.underlyingValue = value;
                this.source = SourceState<T>.Assigned;
            }
        }

        /// <summary>
        /// 通过引用属性的值初始化 <see cref="T:System.Data.Linq.Link`1"/> 结构的新实例。
        /// </summary>
        /// <param name="value">属性的值。</param>
        public Link(T value)
        {
            this.underlyingValue = value;
            this.source = (IEnumerable<T>)null;
        }

        /// <summary>
        /// 通过引用源初始化 <see cref="T:System.Data.Linq.Link`1"/> 结构的新实例。
        /// </summary>
        /// <param name="source">源集合。</param>
        public Link(IEnumerable<T> source)
        {
            this.source = source;
            this.underlyingValue = default(T);
        }

        /// <summary>
        /// 通过从其他 <see cref="T:System.Data.Linq.Link`1"/> 实例复制内部状态来初始化 <see cref="T:System.Data.Linq.Link`1"/> 结构的新实例。
        /// </summary>
        /// <param name="link">从其复制的 <see cref="T:System.Data.Linq.Link`1"/> 实例。</param>
        public Link(Link<T> link)
        {
            this.underlyingValue = link.underlyingValue;
            this.source = link.source;
        }
    }
}
