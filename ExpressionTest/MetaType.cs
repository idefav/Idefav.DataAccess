using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    public abstract class MetaType
    {
        /// <summary>
        /// 获取包含此 <see cref="T:System.Data.Linq.Mapping.MetaType"/> 的 <see cref="T:System.Data.Linq.Mapping.MetaModel"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 包含元模型。
        /// </returns>
        public abstract MetaModel Model { get; }

        /// <summary>
        /// 获取将此 <see cref="T:System.Data.Linq.Mapping.MetaType"/> 用于行定义的 <see cref="T:System.Data.Linq.Mapping.MetaTable"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 一个使用当前元类型定义行的元表。
        /// </returns>
        public abstract MetaTable Table { get; }

        /// <summary>
        /// 获取基础公共语言运行时 (CLR) 类型。
        /// </summary>
        /// 
        /// <returns>
        /// 关联的 CLR 类型。
        /// </returns>
        public abstract Type Type { get; }

        /// <summary>
        /// 获取 <see cref="T:System.Data.Linq.Mapping.MetaType"/> 的名称。
        /// </summary>
        /// 
        /// <returns>
        /// 当前元类型的名称。
        /// </returns>
        public abstract string Name { get; }

        /// <summary>
        /// 获取一个值，该值指示 <see cref="T:System.Data.Linq.Mapping.MetaType"/> 是否为实体类型。
        /// </summary>
        /// 
        /// <returns>
        /// 如果 <see cref="T:System.Data.Linq.Mapping.MetaType"/> 是实体类型，则为 true；否则为 false。
        /// </returns>
        public abstract bool IsEntity { get; }

        /// <summary>
        /// 获取一个值，该值指示基础类型是否可实例化为查询结果。
        /// </summary>
        /// 
        /// <returns>
        /// 如果基础类型可以实例化为查询结果，则为 true；否则为 false。
        /// </returns>
        public abstract bool CanInstantiate { get; }

        /// <summary>
        /// 获取表示自动生成的标识列的成员。
        /// </summary>
        /// 
        /// <returns>
        /// 表示自动生成的标识列的成员；如果没有自动生成的标识列，则为 null。
        /// </returns>
        public abstract MetaDataMember DBGeneratedIdentityMember { get; }

        /// <summary>
        /// 获取此 <see cref="T:System.Data.Linq.Mapping.MetaType"/> 的行版本或时间戳列。
        /// </summary>
        /// 
        /// <returns>
        /// 表示此元类型的行版本或时间戳列的元数据成员；如果不存在这样的元数据成员，则为 null。
        /// </returns>
        public abstract MetaDataMember VersionMember { get; }

        /// <summary>
        /// 获取表示继承鉴别器列的成员。
        /// </summary>
        /// 
        /// <returns>
        /// 表示继承鉴别器列的成员；如果不存在继承鉴别器列，则为 null。
        /// </returns>
        public abstract MetaDataMember Discriminator { get; }

        /// <summary>
        /// 获取一个值，该值指示该类型是否具有任何可能需要进行开放式并发冲突测试的持久性成员。
        /// </summary>
        /// 
        /// <returns>
        /// 如果该类型包含具有 <see cref="F:System.Data.Linq.Mapping.UpdateCheck.Never"/> 以外的 <see cref="T:System.Data.Linq.Mapping.UpdateCheck"/> 策略的任何持久性成员，则为 true；否则为 false。
        /// </returns>
        public abstract bool HasUpdateCheck { get; }

        /// <summary>
        /// 获取一个值，该值指示该类型是否是已映射继承层次结构的一部分。
        /// </summary>
        /// 
        /// <returns>
        /// 如果该类型是已映射继承层次结构的一部分，则为 true；否则为 false。
        /// </returns>
        public abstract bool HasInheritance { get; }

        /// <summary>
        /// 获取一个值，该值指示此类型是否定义继承代码。
        /// </summary>
        /// 
        /// <returns>
        /// 如果此类型定义继承代码，则为 true；否则为 false。
        /// </returns>
        public abstract bool HasInheritanceCode { get; }

        /// <summary>
        /// 获取一个值，该值指示此类型是否定义继承代码。
        /// </summary>
        /// 
        /// <returns>
        /// 如果此类型定义继承代码，则为 true；否则为 false。
        /// </returns>
        public abstract object InheritanceCode { get; }

        /// <summary>
        /// 获取一个值，该值指示是否将此类型用作继承层次结构的默认值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果将此类型用作继承层次结构的默认值，则为 true；否则为 false。
        /// </returns>
        public abstract bool IsInheritanceDefault { get; }

        /// <summary>
        /// 获取继承层次结构的根类型。
        /// </summary>
        /// 
        /// <returns>
        /// 根类型。
        /// </returns>
        public abstract MetaType InheritanceRoot { get; }

        /// <summary>
        /// 获取继承层次结构中的基元类型。
        /// </summary>
        /// 
        /// <returns>
        /// 当前继承层次结构的基元类型。
        /// </returns>
        public abstract MetaType InheritanceBase { get; }

        /// <summary>
        /// 获取一个值，该值指示是否将此类型用作继承层次结构的默认值。
        /// </summary>
        /// 
        /// <returns>
        /// 继承映射中默认类型的元数据。
        /// </returns>
        public abstract MetaType InheritanceDefault { get; }

        /// <summary>
        /// 获取由继承层次结构定义的所有类型的集合。
        /// </summary>
        /// 
        /// <returns>
        /// 当前继承层次结构中元类型的集合。
        /// </returns>
        public abstract ReadOnlyCollection<MetaType> InheritanceTypes { get; }

        /// <summary>
        /// 获取一个值，该值指示当前的 <see cref="T:System.Data.Linq.Mapping.MetaType"/> 或其任何基类型是否具有 OnLoaded 方法。
        /// </summary>
        /// 
        /// <returns>
        /// 如果该元类型或任何基元类型具有 OnLoaded 方法，则为 true；否则为 false。
        /// </returns>
        public abstract bool HasAnyLoadMethod { get; }

        /// <summary>
        /// 获取一个值，该值指示 <see cref="T:System.Data.Linq.Mapping.MetaType"/> 或其任何基类型是否具有 OnValidate 方法。
        /// </summary>
        /// 
        /// <returns>
        /// 如果该元类型或任何基元类型具有 OnValidate 方法，则为 true；否则为 false。
        /// </returns>
        public abstract bool HasAnyValidateMethod { get; }

        /// <summary>
        /// 获取继承层次结构中直接派生类型的枚举。
        /// </summary>
        /// 
        /// <returns>
        /// 元类型的枚举。
        /// </returns>
        public abstract ReadOnlyCollection<MetaType> DerivedTypes { get; }

        /// <summary>
        /// 获取所有数据成员（字段和属性）的枚举。
        /// </summary>
        /// 
        /// <returns>
        /// 数据成员的集合。
        /// </returns>
        public abstract ReadOnlyCollection<MetaDataMember> DataMembers { get; }

        /// <summary>
        /// 获取所有持久性数据成员的集合。
        /// </summary>
        /// 
        /// <returns>
        /// 当前类型中所有元数据成员的集合。
        /// </returns>
        public abstract ReadOnlyCollection<MetaDataMember> PersistentDataMembers { get; }

        /// <summary>
        /// 获取定义类型的唯一标识的所有数据成员的枚举。
        /// </summary>
        /// 
        /// <returns>
        /// 成员的枚举，这些成员定义类型的唯一标识。
        /// </returns>
        public abstract ReadOnlyCollection<MetaDataMember> IdentityMembers { get; }

        /// <summary>
        /// 获取所有关联的枚举。
        /// </summary>
        /// 
        /// <returns>
        /// 关联的集合。
        /// </returns>
        public abstract ReadOnlyCollection<MetaAssociation> Associations { get; }

        /// <summary>
        /// 获取有关此元类型包含的 OnLoaded 方法的信息。
        /// </summary>
        /// 
        /// <returns>
        /// 有关此元类型的 OnLoaded 方法的说明。
        /// </returns>
        public abstract MethodInfo OnLoadedMethod { get; }

        /// <summary>
        /// 获取有关此元类型包含的 OnValidate 方法的信息。
        /// </summary>
        /// 
        /// <returns>
        /// 有关此元类型的 OnValidate 方法的说明。
        /// </returns>
        public abstract MethodInfo OnValidateMethod { get; }

        /// <summary>
        /// 获取继承子类型的 <see cref="T:System.Data.Linq.Mapping.MetaType"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 继承子类型的 <see cref="T:System.Data.Linq.Mapping.MetaType"/>。
        /// </returns>
        /// <param name="type">子类型。</param>
        public abstract MetaType GetInheritanceType(Type type);

        /// <summary>
        /// 获取与指定的继承代码关联的元类型。
        /// </summary>
        /// 
        /// <returns>
        /// 与指定的继承代码关联的元类型。
        /// </returns>
        /// <param name="code">继承代码。</param>
        public abstract MetaType GetTypeForInheritanceCode(object code);

        /// <summary>
        /// 获取与指定成员关联的 <see cref="T:System.Data.Linq.Mapping.MetaDataMember"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 如果存在与指定的成员关联的 <see cref="T:System.Data.Linq.Mapping.MetaDataMember"/>，则为该 System.Data.Linq.Mapping.MetaDataMember；否则为 null。
        /// </returns>
        /// <param name="member">为其查找关联的 <see cref="T:System.Data.Linq.Mapping.MetaDataMember"/> 的成员。</param>
        public abstract MetaDataMember GetDataMember(MemberInfo member);
    }
}
