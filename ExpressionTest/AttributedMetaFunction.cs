using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal sealed class AttributedMetaFunction : MetaFunction
    {
        private static ReadOnlyCollection<MetaParameter> _emptyParameters = new List<MetaParameter>(0).AsReadOnly();
        private static ReadOnlyCollection<MetaType> _emptyTypes = new List<MetaType>(0).AsReadOnly();
        private AttributedMetaModel model;
        private MethodInfo methodInfo;
        private FunctionAttribute functionAttrib;
        private MetaParameter returnParameter;
        private ReadOnlyCollection<MetaParameter> parameters;
        private ReadOnlyCollection<MetaType> rowTypes;

        public override MetaModel Model
        {
            get
            {
                return (MetaModel)this.model;
            }
        }

        public override MethodInfo Method
        {
            get
            {
                return this.methodInfo;
            }
        }

        public override string Name
        {
            get
            {
                return this.methodInfo.Name;
            }
        }

        public override string MappedName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.functionAttrib.Name))
                    return this.functionAttrib.Name;
                return this.methodInfo.Name;
            }
        }

        public override bool IsComposable
        {
            get
            {
                return this.functionAttrib.IsComposable;
            }
        }

        public override ReadOnlyCollection<MetaParameter> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public override MetaParameter ReturnParameter
        {
            get
            {
                return this.returnParameter;
            }
        }

        public override bool HasMultipleResults
        {
            get
            {
                return this.methodInfo.ReturnType == typeof(IMultipleResults);
            }
        }

        public override ReadOnlyCollection<MetaType> ResultRowTypes
        {
            get
            {
                return this.rowTypes;
            }
        }

        public AttributedMetaFunction(AttributedMetaModel model, MethodInfo mi)
        {
            this.model = model;
            this.methodInfo = mi;
            this.rowTypes = AttributedMetaFunction._emptyTypes;
            this.functionAttrib = Attribute.GetCustomAttribute((MemberInfo)mi, typeof(FunctionAttribute), false) as FunctionAttribute;
            ResultTypeAttribute[] resultTypeAttributeArray = (ResultTypeAttribute[])Attribute.GetCustomAttributes((MemberInfo)mi, typeof(ResultTypeAttribute));
            if (resultTypeAttributeArray.Length == 0 && mi.ReturnType == typeof(IMultipleResults))
                throw Error.NoResultTypesDeclaredForFunction((object)mi.Name);
            if (resultTypeAttributeArray.Length > 1 && mi.ReturnType != typeof(IMultipleResults))
                throw Error.TooManyResultTypesDeclaredForFunction((object)mi.Name);
            if (resultTypeAttributeArray.Length <= 1 && mi.ReturnType.IsGenericType && (mi.ReturnType.GetGenericTypeDefinition() == typeof(IEnumerable<>) || mi.ReturnType.GetGenericTypeDefinition() == typeof(ISingleResult<>) || mi.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>)))
            {
                Type elementType = TypeSystem.GetElementType(mi.ReturnType);
                List<MetaType> list = new List<MetaType>(1);
                MetaType metaType = this.GetMetaType(elementType);
                list.Add(metaType);
                this.rowTypes = list.AsReadOnly();
            }
            else if (resultTypeAttributeArray.Length != 0)
            {
                List<MetaType> list = new List<MetaType>();
                foreach (ResultTypeAttribute resultTypeAttribute in resultTypeAttributeArray)
                {
                    MetaType metaType = this.GetMetaType(resultTypeAttribute.Type);
                    if (!list.Contains(metaType))
                        list.Add(metaType);
                }
                this.rowTypes = list.AsReadOnly();
            }
            else
                this.returnParameter = (MetaParameter)new AttributedMetaParameter(this.methodInfo.ReturnParameter);
            ParameterInfo[] parameters = mi.GetParameters();
            if (parameters.Length != 0)
            {
                List<MetaParameter> list = new List<MetaParameter>(parameters.Length);
                int index = 0;
                for (int length = parameters.Length; index < length; ++index)
                {
                    AttributedMetaParameter attributedMetaParameter = new AttributedMetaParameter(parameters[index]);
                    list.Add((MetaParameter)attributedMetaParameter);
                }
                this.parameters = list.AsReadOnly();
            }
            else
                this.parameters = AttributedMetaFunction._emptyParameters;
        }

        private MetaType GetMetaType(Type type)
        {
            MetaTable tableNoLocks = this.model.GetTableNoLocks(type);
            if (tableNoLocks != null)
                return tableNoLocks.RowType.GetInheritanceType(type);
            return (MetaType)new AttributedRootType(this.model, (AttributedMetaTable)null, type);
        }
    }

    internal sealed class AttributedRootType : AttributedMetaType
    {
        private Dictionary<Type, MetaType> types;
        private Dictionary<object, MetaType> codeMap;
        private ReadOnlyCollection<MetaType> inheritanceTypes;
        private MetaType inheritanceDefault;

        public override bool HasInheritance
        {
            get
            {
                return this.types != null;
            }
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes
        {
            get
            {
                return this.inheritanceTypes;
            }
        }

        public override MetaType InheritanceDefault
        {
            get
            {
                return this.inheritanceDefault;
            }
        }

        internal AttributedRootType(AttributedMetaModel model, AttributedMetaTable table, Type type)
          : base((MetaModel)model, (MetaTable)table, type, (MetaType)null)
        {
            InheritanceMappingAttribute[] mappingAttributeArray = (InheritanceMappingAttribute[])type.GetCustomAttributes(typeof(InheritanceMappingAttribute), true);
            if (mappingAttributeArray.Length != 0)
            {
                if (this.Discriminator == null)
                    throw Error.NoDiscriminatorFound((object)type);
                if (!MappingSystem.IsSupportedDiscriminatorType(this.Discriminator.Type))
                    throw Error.DiscriminatorClrTypeNotSupported((object)this.Discriminator.DeclaringType.Name, (object)this.Discriminator.Name, (object)this.Discriminator.Type);
                this.types = new Dictionary<Type, MetaType>();
                this.types.Add(type, (MetaType)this);
                this.codeMap = new Dictionary<object, MetaType>();
                foreach (InheritanceMappingAttribute mappingAttribute in mappingAttributeArray)
                {
                    if (!type.IsAssignableFrom(mappingAttribute.Type))
                        throw Error.InheritanceTypeDoesNotDeriveFromRoot((object)mappingAttribute.Type, (object)type);
                    if (mappingAttribute.Type.IsAbstract)
                        throw Error.AbstractClassAssignInheritanceDiscriminator((object)mappingAttribute.Type);
                    AttributedMetaType inheritedType = this.CreateInheritedType(type, mappingAttribute.Type);
                    if (mappingAttribute.Code == null)
                        throw Error.InheritanceCodeMayNotBeNull();
                    if (inheritedType.inheritanceCode != null)
                        throw Error.InheritanceTypeHasMultipleDiscriminators((object)mappingAttribute.Type);
                    object obj = DBConvert.ChangeType(mappingAttribute.Code, this.Discriminator.Type);
                    foreach (object objA in this.codeMap.Keys)
                    {
                        if (obj.GetType() == typeof(string) && ((string)obj).Trim().Length == 0 && (objA.GetType() == typeof(string) && ((string)objA).Trim().Length == 0) || object.Equals(objA, obj))
                            throw Error.InheritanceCodeUsedForMultipleTypes(obj);
                    }
                    inheritedType.inheritanceCode = obj;
                    this.codeMap.Add(obj, (MetaType)inheritedType);
                    if (mappingAttribute.IsDefault)
                    {
                        if (this.inheritanceDefault != null)
                            throw Error.InheritanceTypeHasMultipleDefaults((object)type);
                        this.inheritanceDefault = (MetaType)inheritedType;
                    }
                }
                if (this.inheritanceDefault == null)
                    throw Error.InheritanceHierarchyDoesNotDefineDefault((object)type);
            }
            if (this.types != null)
            {
                this.inheritanceTypes = System.Linq.Enumerable.ToList<MetaType>((IEnumerable<MetaType>)this.types.Values).AsReadOnly();
            }
            else
            {
                MetaType[] metaTypeArray = new MetaType[1];
                int index = 0;
                metaTypeArray[index] = (MetaType)this;
                this.inheritanceTypes = System.Linq.Enumerable.ToList<MetaType>((IEnumerable<MetaType>)metaTypeArray).AsReadOnly();
            }
            this.Validate();
        }

        private void Validate()
        {
            Dictionary<object, string> dictionary = new Dictionary<object, string>();
            foreach (MetaType type in this.InheritanceTypes)
            {
                if (type != this && ((TableAttribute[])type.Type.GetCustomAttributes(typeof(TableAttribute), false)).Length != 0)
                    throw Error.InheritanceSubTypeIsAlsoRoot((object)type.Type);
                foreach (MetaDataMember metaDataMember in type.PersistentDataMembers)
                {
                    if (metaDataMember.IsDeclaredBy(type))
                    {
                        if (metaDataMember.IsDiscriminator && !this.HasInheritance)
                            throw Error.NonInheritanceClassHasDiscriminator((object)type);
                        if (!metaDataMember.IsAssociation && !string.IsNullOrEmpty(metaDataMember.MappedName))
                        {
                            object key = InheritanceRules.DistinguishedMemberName(metaDataMember.Member);
                            string str;
                            if (dictionary.TryGetValue(key, out str))
                            {
                                if (str != metaDataMember.MappedName)
                                    throw Error.MemberMappedMoreThanOnce((object)metaDataMember.Member.Name);
                            }
                            else
                                dictionary.Add(key, metaDataMember.MappedName);
                        }
                    }
                }
            }
        }

        private AttributedMetaType CreateInheritedType(Type root, Type type)
        {
            MetaType metaType;
            if (!this.types.TryGetValue(type, out metaType))
            {
                metaType = (MetaType)new AttributedMetaType(this.Model, this.Table, type, (MetaType)this);
                this.types.Add(type, metaType);
                if (type != root && type.BaseType != typeof(object))
                    this.CreateInheritedType(root, type.BaseType);
            }
            return (AttributedMetaType)metaType;
        }

        public override MetaType GetInheritanceType(Type type)
        {
            if (type == this.Type)
                return (MetaType)this;
            MetaType metaType = (MetaType)null;
            if (this.types != null)
                this.types.TryGetValue(type, out metaType);
            return metaType;
        }
    }

    internal sealed class AttributedMetaParameter : MetaParameter
    {
        private ParameterInfo parameterInfo;
        private ParameterAttribute paramAttrib;

        public override ParameterInfo Parameter
        {
            get
            {
                return this.parameterInfo;
            }
        }

        public override string Name
        {
            get
            {
                return this.parameterInfo.Name;
            }
        }

        public override string MappedName
        {
            get
            {
                if (this.paramAttrib != null && this.paramAttrib.Name != null)
                    return this.paramAttrib.Name;
                return this.parameterInfo.Name;
            }
        }

        public override Type ParameterType
        {
            get
            {
                return this.parameterInfo.ParameterType;
            }
        }

        public override string DbType
        {
            get
            {
                if (this.paramAttrib != null && this.paramAttrib.DbType != null)
                    return this.paramAttrib.DbType;
                return (string)null;
            }
        }

        public AttributedMetaParameter(ParameterInfo parameterInfo)
        {
            this.parameterInfo = parameterInfo;
            this.paramAttrib = Attribute.GetCustomAttribute(parameterInfo, typeof(ParameterAttribute), false) as ParameterAttribute;
        }
    }

    /// <summary>
    /// 为存储过程方法参数启用映射详细信息规范。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false)]
    public sealed class ParameterAttribute : Attribute
    {
        private string name;
        private string dbType;

        /// <summary>
        /// 获取或设置参数的名称。
        /// </summary>
        /// 
        /// <returns>
        /// 字符串形式的名称。
        /// </returns>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        /// <summary>
        /// 获取或设置提供程序特定的数据库的参数类型。
        /// </summary>
        /// 
        /// <returns>
        /// 字符串形式的类型。
        /// </returns>
        public string DbType
        {
            get
            {
                return this.dbType;
            }
            set
            {
                this.dbType = value;
            }
        }
    }

    /// <summary>
    /// 用于为具有不同结果类型的函数指定每种类型的结果。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ResultTypeAttribute : Attribute
    {
        private Type type;

        /// <summary>
        /// 为具有不同结果类型的函数获取有效的或预期的类型映射。
        /// </summary>
        /// 
        /// <returns>
        /// 结果的类型 (<see cref="T:System.Type"/>)。
        /// </returns>
        public Type Type
        {
            get
            {
                return this.type;
            }
        }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.Mapping.ResultTypeAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="type">具有不同结果类型的函数所返回的结果的类型。</param>
        public ResultTypeAttribute(Type type)
        {
            this.type = type;
        }
    }
}
