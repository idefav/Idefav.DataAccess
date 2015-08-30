using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 表示两个实体类型之间的关联关系。
    /// </summary>
    public abstract class MetaAssociation
    {
        /// <summary>
        /// 获取关联的另一端上的类型。
        /// </summary>
        /// 
        /// <returns>
        /// 类型。
        /// </returns>
        public abstract MetaType OtherType { get; }

        /// <summary>
        /// 获取表示关联的此端上的成员。
        /// </summary>
        /// 
        /// <returns>
        /// 成员。
        /// </returns>
        public abstract MetaDataMember ThisMember { get; }

        /// <summary>
        /// 获取表示反向关联的此关联的另一端上的成员。
        /// </summary>
        /// 
        /// <returns>
        /// 另一端上的成员。
        /// </returns>
        public abstract MetaDataMember OtherMember { get; }

        /// <summary>
        /// 获取表示关联的此端上的值的成员列表。
        /// </summary>
        /// 
        /// <returns>
        /// 一个集合。
        /// </returns>
        public abstract ReadOnlyCollection<MetaDataMember> ThisKey { get; }

        /// <summary>
        /// 获取表示关联的另一端上的值的成员列表。
        /// </summary>
        /// 
        /// <returns>
        /// 返回表示关联的另一端上的值的集合。
        /// </returns>
        public abstract ReadOnlyCollection<MetaDataMember> OtherKey { get; }

        /// <summary>
        /// 获取一个值，该值指示关联是否表示一对多关系。
        /// </summary>
        /// 
        /// <returns>
        /// 如果关联表示一对多关系，则返回 true。
        /// </returns>
        public abstract bool IsMany { get; }

        /// <summary>
        /// 获取一个值，该值指示另一类型是否为此类型的父级。
        /// </summary>
        /// 
        /// <returns>
        /// 如果另一类型为此类型的父级，则返回 true。
        /// </returns>
        public abstract bool IsForeignKey { get; }

        /// <summary>
        /// 获取一个值，该值指示关联是否是唯一的。
        /// </summary>
        /// 
        /// <returns>
        /// 如果关联是唯一的，则返回 true。
        /// </returns>
        public abstract bool IsUnique { get; }

        /// <summary>
        /// 获取一个值，该值指示关联是否可为 null。
        /// </summary>
        /// 
        /// <returns>
        /// 如果关联可为 null，则返回 true。
        /// </returns>
        public abstract bool IsNullable { get; }

        /// <summary>
        /// 获取一个值，该值指示 <see cref="P:System.Data.Linq.Mapping.MetaAssociation.ThisKey"/> 是否构成此类型的标识。
        /// </summary>
        /// 
        /// <returns>
        /// 如果 <see cref="P:System.Data.Linq.Mapping.MetaAssociation.ThisKey"/> 构成关联的标识（主键），则为 true。
        /// </returns>
        public abstract bool ThisKeyIsPrimaryKey { get; }

        /// <summary>
        /// 获取一个值，该值指示 <see cref="P:System.Data.Linq.Mapping.MetaAssociation.OtherKey"/> 是否构成另一类型的标识。
        /// </summary>
        /// 
        /// <returns>
        /// 如果 <see cref="P:System.Data.Linq.Mapping.MetaAssociation.OtherKey"/> 构成另一类型的标识（主键），则为 true。
        /// </returns>
        public abstract bool OtherKeyIsPrimaryKey { get; }

        /// <summary>
        /// 获取删除子级时的行为。
        /// </summary>
        /// 
        /// <returns>
        /// 返回表示规则的字符串；或者如果没有指定删除时的操作，则返回 null。
        /// </returns>
        public abstract string DeleteRule { get; }

        /// <summary>
        /// 获取一个值，该值指示当关联设置为 null 时是否应删除对象。
        /// </summary>
        /// 
        /// <returns>
        /// 如果为 true，则当关联设置为 null 时删除对象。
        /// </returns>
        public abstract bool DeleteOnNull { get; }
    }
}
