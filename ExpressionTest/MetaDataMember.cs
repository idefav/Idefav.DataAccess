using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    public abstract class MetaDataMember
    {
        /// <summary>
        /// 获取包含此数据成员的 <see cref="T:System.Data.Linq.Mapping.MetaType"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 包含当前成员的元类型。
        /// </returns>
        public abstract MetaType DeclaringType { get; }

        /// <summary>
        /// 获取基础 <see cref="T:System.Reflection.MemberInfo"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 成员特性和元数据信息。
        /// </returns>
        public abstract MemberInfo Member { get; }

        /// <summary>
        /// 获取存储此成员数据的成员。
        /// </summary>
        /// 
        /// <returns>
        /// 存储成员。
        /// </returns>
        public abstract MemberInfo StorageMember { get; }

        /// <summary>
        /// 获取与 <see cref="T:System.Reflection.MemberInfo"/> 名称相同的成员名称。
        /// </summary>
        /// 
        /// <returns>
        /// 与 <see cref="T:System.Reflection.MemberInfo"/> 名称相同的名称。
        /// </returns>
        public abstract string Name { get; }

        /// <summary>
        /// 获取数据库中列（或约束）的名称。
        /// </summary>
        /// 
        /// <returns>
        /// 字符串形式的列（或约束）的名称。
        /// </returns>
        public abstract string MappedName { get; }

        /// <summary>
        /// 获取此成员在查询结果的默认布局中的序号位置。
        /// </summary>
        /// 
        /// <returns>
        /// 序号位置。
        /// </returns>
        public abstract int Ordinal { get; }

        /// <summary>
        /// 获取此成员的类型。
        /// </summary>
        /// 
        /// <returns>
        /// 当前成员的类型。
        /// </returns>
        public abstract Type Type { get; }

        /// <summary>
        /// 获取用于获取或设置此成员的值的访问器。
        /// </summary>
        /// 
        /// <returns>
        /// 访问器。
        /// </returns>
        public abstract MetaAccessor MemberAccessor { get; }

        /// <summary>
        /// 获取用于获取或设置此成员的存储值的访问器。
        /// </summary>
        /// 
        /// <returns>
        /// 用于访问此成员的存储值的访问器。
        /// </returns>
        public abstract MetaAccessor StorageAccessor { get; }

        /// <summary>
        /// 获取用于获取并设置此成员的延迟值（而不会导致提取）的访问器。
        /// </summary>
        /// 
        /// <returns>
        /// 用于访问此成员的延迟值的访问器。
        /// </returns>
        public abstract MetaAccessor DeferredValueAccessor { get; }

        /// <summary>
        /// 获取用于获取并设置此成员的延迟源的访问器。
        /// </summary>
        /// 
        /// <returns>
        /// 用于访问此成员的延迟源的访问器。
        /// </returns>
        public abstract MetaAccessor DeferredSourceAccessor { get; }

        /// <summary>
        /// 获取一个值，该值指示默认行为是否为延迟加载此成员。
        /// </summary>
        /// 
        /// <returns>
        /// 如果默认情况下会延迟加载此成员，则为 true；否则为 false。
        /// </returns>
        public abstract bool IsDeferred { get; }

        /// <summary>
        /// 获取一个值，该值指示此成员是否映射到列（或约束）。
        /// </summary>
        /// 
        /// <returns>
        /// 如果此成员映射到列（或约束），则为 true；否则为 false。
        /// </returns>
        public abstract bool IsPersistent { get; }

        /// <summary>
        /// 获取一个值，该值指示此成员是否定义关联关系。
        /// </summary>
        /// 
        /// <returns>
        /// 如果此成员定义了关联关系，则为 true；否则为 false。
        /// </returns>
        public abstract bool IsAssociation { get; }

        /// <summary>
        /// 获取一个值，该值指示此成员是否为类型标识的一部分。
        /// </summary>
        /// 
        /// <returns>
        /// 如果此成员是类型标识的一部分，则为 true；否则为 false。
        /// </returns>
        public abstract bool IsPrimaryKey { get; }

        /// <summary>
        /// 获取一个值，该值指示此成员是否由数据库自动生成。
        /// </summary>
        /// 
        /// <returns>
        /// 如果此成员是由数据库自动生成的，则为 true；否则为 false。
        /// </returns>
        public abstract bool IsDbGenerated { get; }

        /// <summary>
        /// 获取一个值，该值指示此成员是否表示行版本或时间戳。
        /// </summary>
        /// 
        /// <returns>
        /// 如果此成员表示行版本或时间戳，则为 true；否则为 false。
        /// </returns>
        public abstract bool IsVersion { get; }

        /// <summary>
        /// 获取一个值，该值指示此成员是否表示继承鉴别器。
        /// </summary>
        /// 
        /// <returns>
        /// 如果此成员表示继承鉴别器，则为 true；否则为 false。
        /// </returns>
        public abstract bool IsDiscriminator { get; }

        /// <summary>
        /// 获取一个值，该值指示是否可以为此成员赋予 null 值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果可以为此成员赋予 null 值，则为 true；否则为 false。
        /// </returns>
        public abstract bool CanBeNull { get; }

        /// <summary>
        /// 获取相应数据库列的类型。
        /// </summary>
        /// 
        /// <returns>
        /// 以字符串表示的数据库列的类型。
        /// </returns>
        public abstract string DbType { get; }

        /// <summary>
        /// 获取定义计算列的表达式。
        /// </summary>
        /// 
        /// <returns>
        /// 以字符串表示的计算列表达式。
        /// </returns>
        public abstract string Expression { get; }

        /// <summary>
        /// 获取此成员的开放式并发检查策略。
        /// </summary>
        /// 
        /// <returns>
        /// <see cref="T:System.Data.Linq.Mapping.UpdateCheck"/> 枚举。
        /// </returns>
        public abstract UpdateCheck UpdateCheck { get; }

        /// <summary>
        /// 获取此成员在执行插入和更新操作时的读回行为。
        /// </summary>
        /// 
        /// <returns>
        /// <see cref="T:System.Data.Linq.Mapping.AutoSync"/> 枚举。
        /// </returns>
        public abstract AutoSync AutoSync { get; }

        /// <summary>
        /// 获取与此成员相对应的 <see cref="T:System.Data.Linq.Mapping.MetaAssociation"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 如果存在对应的 <see cref="T:System.Data.Linq.Mapping.MetaAssociation"/>，则为该 System.Data.Linq.Mapping.MetaAssociation；否则为 null。
        /// </returns>
        public abstract MetaAssociation Association { get; }

        /// <summary>
        /// 获取用于执行加载操作的 <see cref="T:System.Data.Linq.DataContext"/> 方法。
        /// </summary>
        /// 
        /// <returns>
        /// 类似于 <see cref="T:System.Reflection.MethodInfo"/> 的加载方法。
        /// </returns>
        public abstract MethodInfo LoadMethod { get; }

        /// <summary>
        /// 指定此成员是否由指定类型声明。
        /// </summary>
        /// 
        /// <returns>
        /// 如果此成员是由指定类型声明的，则为 true；否则为 false。
        /// </returns>
        /// <param name="type">要检查的类型。</param>
        public abstract bool IsDeclaredBy(MetaType type);
    }

    /// <summary>
    /// 指定何时测试对象是否有并发冲突。
    /// </summary>
    public enum UpdateCheck
    {
        Always,
        Never,
        WhenChanged,
    }

    /// <summary>
    /// 指示运行时如何在执行插入或更新操作后检索值。
    /// </summary>
    public enum AutoSync
    {
        Default,
        Always,
        Never,
        OnInsert,
        OnUpdate,
    }
}
