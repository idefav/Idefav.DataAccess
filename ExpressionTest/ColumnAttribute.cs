using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    /// <summary>
    /// 将类与数据库表中的列相关联。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ColumnAttribute : DataAttribute
    {
        private bool canBeNull = true;
        private string dbtype;
        private string expression;
        private bool isPrimaryKey;
        private bool isDBGenerated;
        private bool isVersion;
        private bool isDiscriminator;
        private UpdateCheck check;
        private AutoSync autoSync;
        private bool canBeNullSet;

        /// <summary>
        /// 获取或设置数据库列的类型。
        /// </summary>
        /// 
        /// <returns>
        /// 请参阅“备注”。
        /// </returns>
        public string DbType
        {
            get
            {
                return this.dbtype;
            }
            set
            {
                this.dbtype = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示列是否为数据库中的计算列。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值 = 空。
        /// </returns>
        public string Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                this.expression = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示该类成员是否表示作为表的整个主键或部分主键的列。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值 = false。
        /// </returns>
        public bool IsPrimaryKey
        {
            get
            {
                return this.isPrimaryKey;
            }
            set
            {
                this.isPrimaryKey = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示列是否包含数据库自动生成的值。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值 = false。
        /// </returns>
        public bool IsDbGenerated
        {
            get
            {
                return this.isDBGenerated;
            }
            set
            {
                this.isDBGenerated = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示成员的列类型是否为数据库时间戳或版本号。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值 = false。
        /// </returns>
        public bool IsVersion
        {
            get
            {
                return this.isVersion;
            }
            set
            {
                this.isVersion = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示 LINQ to SQL 如何进行开放式并发冲突的检测。
        /// </summary>
        /// 
        /// <returns>
        /// 除非 <see cref="P:System.Data.Linq.Mapping.ColumnAttribute.IsVersion"/> 对某个成员为 true，否则默认值为 <see cref="F:System.Data.Linq.Mapping.UpdateCheck.Always"/>。其他值为 <see cref="F:System.Data.Linq.Mapping.UpdateCheck.Never"/> 和 <see cref="F:System.Data.Linq.Mapping.UpdateCheck.WhenChanged"/>。
        /// </returns>
        public UpdateCheck UpdateCheck
        {
            get
            {
                return this.check;
            }
            set
            {
                this.check = value;
            }
        }

        /// <summary>
        /// 获取或设置 <see cref="T:System.Data.Linq.Mapping.AutoSync"/> 枚举。
        /// </summary>
        /// 
        /// <returns>
        /// <see cref="T:System.Data.Linq.Mapping.AutoSync"/> 值。
        /// </returns>
        public AutoSync AutoSync
        {
            get
            {
                return this.autoSync;
            }
            set
            {
                this.autoSync = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示列是否包含 LINQ to SQL 继承层次结构的鉴别器值。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值 = false。
        /// </returns>
        public bool IsDiscriminator
        {
            get
            {
                return this.isDiscriminator;
            }
            set
            {
                this.isDiscriminator = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示列是否可包含 null 值。
        /// </summary>
        /// 
        /// <returns>
        /// 默认值 = true。
        /// </returns>
        public bool CanBeNull
        {
            get
            {
                return this.canBeNull;
            }
            set
            {
                this.canBeNullSet = true;
                this.canBeNull = value;
            }
        }

        internal bool CanBeNullSet
        {
            get
            {
                return this.canBeNullSet;
            }
        }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.Mapping.ColumnAttribute"/> 类的新实例。
        /// </summary>
        public ColumnAttribute()
        {
            this.check = UpdateCheck.Always;
        }
    }
}
