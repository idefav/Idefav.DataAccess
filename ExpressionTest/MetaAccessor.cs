using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    public abstract class MetaAccessor
    {
        /// <summary>
        /// 获取此访问器访问的成员的类型。
        /// </summary>
        /// 
        /// <returns>
        /// 成员的类型。
        /// </returns>
        public abstract Type Type { get; }

        /// <summary>
        /// 指定对其设置值或从其获取值的一个对象。
        /// </summary>
        /// 
        /// <returns>
        /// 此实例的已装箱值。
        /// </returns>
        /// <param name="instance">从其获取值或对其设置值的实例。</param>
        public abstract object GetBoxedValue(object instance);

        /// <summary>
        /// 将值设置为对象。
        /// </summary>
        /// <param name="instance">要将值设置到其中的实例。</param><param name="value">要设置的值。</param>
        public abstract void SetBoxedValue(ref object instance, object value);

        /// <summary>
        /// 指定该实例是否包含已加载或分配的值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果该实例当前具有已加载或已分配的值，则为 true；否则为 false。
        /// </returns>
        /// <param name="instance">要查看的实例。</param>
        public virtual bool HasValue(object instance)
        {
            return true;
        }

        /// <summary>
        /// 指定该实例是否包含已分配的值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果该实例当前具有已分配的值，则为 true；否则为 false。
        /// </returns>
        /// <param name="instance">要查看的实例。</param>
        public virtual bool HasAssignedValue(object instance)
        {
            return true;
        }

        /// <summary>
        /// 指定该实例是否包含从延迟的源加载的值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果该实例当前具有从延迟源加载的值，则为 true；否则为 false。
        /// </returns>
        /// <param name="instance">要查看的实例。</param>
        public virtual bool HasLoadedValue(object instance)
        {
            return false;
        }
    }
}
