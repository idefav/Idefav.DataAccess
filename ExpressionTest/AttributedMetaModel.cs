using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal class AttributedMetaModel : MetaModel
    {
        private ReaderWriterLock @lock = new ReaderWriterLock();
        private MappingSource mappingSource;
        private Type contextType;
        private Type providerType;
        private Dictionary<Type, MetaType> metaTypes;
        private Dictionary<Type, MetaTable> metaTables;
        private ReadOnlyCollection<MetaTable> staticTables;
        private Dictionary<MetaPosition, MetaFunction> metaFunctions;
        private string dbName;
        private bool initStaticTables;
        private bool initFunctions;

        public override MappingSource MappingSource
        {
            get
            {
                return this.mappingSource;
            }
        }

        public override Type ContextType
        {
            get
            {
                return this.contextType;
            }
        }

        public override string DatabaseName
        {
            get
            {
                return this.dbName;
            }
        }

        public override Type ProviderType
        {
            get
            {
                return this.providerType;
            }
        }

        internal AttributedMetaModel(MappingSource mappingSource, Type contextType)
        {
            this.mappingSource = mappingSource;
            this.contextType = contextType;
            this.metaTypes = new Dictionary<Type, MetaType>();
            this.metaTables = new Dictionary<Type, MetaTable>();
            this.metaFunctions = new Dictionary<MetaPosition, MetaFunction>();
            ProviderAttribute[] providerAttributeArray = (ProviderAttribute[])this.contextType.GetCustomAttributes(typeof(ProviderAttribute), true);
            this.providerType = providerAttributeArray == null || providerAttributeArray.Length != 1 ? typeof(SqlProvider) : providerAttributeArray[0].Type;
            DatabaseAttribute[] databaseAttributeArray = (DatabaseAttribute[])this.contextType.GetCustomAttributes(typeof(DatabaseAttribute), false);
            this.dbName = databaseAttributeArray == null || databaseAttributeArray.Length == 0 ? this.contextType.Name : databaseAttributeArray[0].Name;
        }

        public override IEnumerable<MetaTable> GetTables()
        {
            this.InitStaticTables();
            if (this.staticTables.Count > 0)
                return (IEnumerable<MetaTable>)this.staticTables;
            this.@lock.AcquireReaderLock(-1);
            try
            {
                return System.Linq.Enumerable.Distinct<MetaTable>(System.Linq.Enumerable.Where<MetaTable>((IEnumerable<MetaTable>)this.metaTables.Values, (Func<MetaTable, bool>)(x => x != null)));
            }
            finally
            {
                this.@lock.ReleaseReaderLock();
            }
        }

        private void InitStaticTables()
        {
            if (this.initStaticTables)
                return;
            this.@lock.AcquireWriterLock(-1);
            try
            {
                if (this.initStaticTables)
                    return;
                HashSet<MetaTable> hashSet = new HashSet<MetaTable>();
                for (Type type = this.contextType; type != typeof(DataContext); type = type.BaseType)
                {
                    foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        Type fieldType = fieldInfo.FieldType;
                        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Table<>))
                        {
                            Type rowType = fieldType.GetGenericArguments()[0];
                            hashSet.Add(this.GetTableNoLocks(rowType));
                        }
                    }
                    foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        Type propertyType = propertyInfo.PropertyType;
                        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Table<>))
                        {
                            Type rowType = propertyType.GetGenericArguments()[0];
                            hashSet.Add(this.GetTableNoLocks(rowType));
                        }
                    }
                }
                this.staticTables = new List<MetaTable>((IEnumerable<MetaTable>)hashSet).AsReadOnly();
                this.initStaticTables = true;
            }
            finally
            {
                this.@lock.ReleaseWriterLock();
            }
        }

        private void InitFunctions()
        {
            if (this.initFunctions)
                return;
            this.@lock.AcquireWriterLock(-1);
            try
            {
                if (this.initFunctions)
                    return;
                if (this.contextType != typeof(DataContext))
                {
                    for (Type type = this.contextType; type != typeof(DataContext); type = type.BaseType)
                    {
                        foreach (MethodInfo mi in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            if (AttributedMetaModel.IsUserFunction(mi))
                            {
                                if (mi.IsGenericMethodDefinition)
                                    throw Error.InvalidUseOfGenericMethodAsMappedFunction((object)mi.Name);
                                MetaPosition key = new MetaPosition((MemberInfo)mi);
                                if (!this.metaFunctions.ContainsKey(key))
                                {
                                    MetaFunction metaFunction = (MetaFunction)new AttributedMetaFunction(this, mi);
                                    this.metaFunctions.Add(key, metaFunction);
                                    foreach (MetaType metaType1 in metaFunction.ResultRowTypes)
                                    {
                                        foreach (MetaType metaType2 in metaType1.InheritanceTypes)
                                        {
                                            if (!this.metaTypes.ContainsKey(metaType2.Type))
                                                this.metaTypes.Add(metaType2.Type, metaType2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                this.initFunctions = true;
            }
            finally
            {
                this.@lock.ReleaseWriterLock();
            }
        }

        private static bool IsUserFunction(MethodInfo mi)
        {
            return Attribute.GetCustomAttribute((MemberInfo)mi, typeof(FunctionAttribute), false) != null;
        }

        public override MetaTable GetTable(Type rowType)
        {
            if (rowType == (Type)null)
                throw Error.ArgumentNull("rowType");
            this.@lock.AcquireReaderLock(-1);
            try
            {
                MetaTable metaTable;
                if (this.metaTables.TryGetValue(rowType, out metaTable))
                    return metaTable;
            }
            finally
            {
                this.@lock.ReleaseReaderLock();
            }
            this.@lock.AcquireWriterLock(-1);
            try
            {
                return this.GetTableNoLocks(rowType);
            }
            finally
            {
                this.@lock.ReleaseWriterLock();
            }
        }

        internal MetaTable GetTableNoLocks(Type rowType)
        {
            MetaTable metaTable;
            if (!this.metaTables.TryGetValue(rowType, out metaTable))
            {
                Type type = AttributedMetaModel.GetRoot(rowType) ?? rowType;
                TableAttribute[] tableAttributeArray = (TableAttribute[])type.GetCustomAttributes(typeof(TableAttribute), true);
                if (tableAttributeArray.Length == 0)
                {
                    this.metaTables.Add(rowType, (MetaTable)null);
                }
                else
                {
                    if (!this.metaTables.TryGetValue(type, out metaTable))
                    {
                        metaTable = (MetaTable)new AttributedMetaTable(this, tableAttributeArray[0], type);
                        foreach (MetaType metaType in metaTable.RowType.InheritanceTypes)
                            this.metaTables.Add(metaType.Type, metaTable);
                    }
                    if (metaTable.RowType.GetInheritanceType(rowType) == null)
                    {
                        this.metaTables.Add(rowType, (MetaTable)null);
                        return (MetaTable)null;
                    }
                }
            }
            return metaTable;
        }

        private static Type GetRoot(Type derivedType)
        {
            for (; derivedType != (Type)null && derivedType != typeof(object); derivedType = derivedType.BaseType)
            {
                if (((TableAttribute[])derivedType.GetCustomAttributes(typeof(TableAttribute), false)).Length != 0)
                    return derivedType;
            }
            return (Type)null;
        }

        public override MetaType GetMetaType(Type type)
        {
            if (type == (Type)null)
                throw Error.ArgumentNull("type");
            MetaType metaType = (MetaType)null;
            this.@lock.AcquireReaderLock(-1);
            try
            {
                if (this.metaTypes.TryGetValue(type, out metaType))
                    return metaType;
            }
            finally
            {
                this.@lock.ReleaseReaderLock();
            }
            MetaTable table = this.GetTable(type);
            if (table != null)
                return table.RowType.GetInheritanceType(type);
            this.InitFunctions();
            this.@lock.AcquireWriterLock(-1);
            try
            {
                if (!this.metaTypes.TryGetValue(type, out metaType))
                {
                    metaType = (MetaType)new UnmappedType((MetaModel)this, type);
                    this.metaTypes.Add(type, metaType);
                }
            }
            finally
            {
                this.@lock.ReleaseWriterLock();
            }
            return metaType;
        }

        public override MetaFunction GetFunction(MethodInfo method)
        {
            if (method == (MethodInfo)null)
                throw Error.ArgumentNull("method");
            this.InitFunctions();
            MetaFunction metaFunction = (MetaFunction)null;
            this.metaFunctions.TryGetValue(new MetaPosition((MemberInfo)method), out metaFunction);
            return metaFunction;
        }

        public override IEnumerable<MetaFunction> GetFunctions()
        {
            this.InitFunctions();
            return (IEnumerable<MetaFunction>)System.Linq.Enumerable.ToList<MetaFunction>((IEnumerable<MetaFunction>)this.metaFunctions.Values).AsReadOnly();
        }
    }
}
