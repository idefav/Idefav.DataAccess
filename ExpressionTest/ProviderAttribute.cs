using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 指定要使用的数据库提供程序。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProviderAttribute : Attribute
    {
        private Type providerType;

        /// <summary>
        /// 获取用于构造 <see cref="T:System.Data.Linq.Mapping.ProviderAttribute"/> 的提供程序的类型。
        /// </summary>
        /// 
        /// <returns>
        /// 提供程序的类型。
        /// </returns>
        public Type Type
        {
            get
            {
                return this.providerType;
            }
        }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.Mapping.ProviderAttribute"/> 类的新实例。
        /// </summary>
        public ProviderAttribute()
        {
        }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.Mapping.ProviderAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="type">用于构造 <see cref="T:System.Data.Linq.Mapping.ProviderAttribute"/> 的提供程序类型。</param>
        public ProviderAttribute(Type type)
        {
            this.providerType = type;
        }
    }
}
