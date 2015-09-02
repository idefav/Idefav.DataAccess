using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    public class DataContext : IDisposable
    {
        private bool objectTrackingEnabled = true;
        private bool deferredLoadingEnabled = true;
        private CommonDataServices services;
        private IProvider provider;
        private Dictionary<MetaTable, ITable> tables;
        private bool disposed;
        private bool isInSubmitChanges;
        private DataLoadOptions loadOptions;
        private ChangeConflictCollection conflicts;
        private static MethodInfo _miExecuteQuery;

        internal CommonDataServices Services
        {
            get
            {
                this.CheckDispose();
                return this.services;
            }
        }

        /// <summary>
        /// 获取由框架使用的连接。
        /// </summary>
        /// 
        /// <returns>
        /// 由框架使用的连接。
        /// </returns>
        public DbConnection Connection
        {
            get
            {
                this.CheckDispose();
                return this.provider.Connection;
            }
        }

        /// <summary>
        /// 获取或设置要用于访问数据库的 .NET Framework 的本地事务。
        /// </summary>
        /// 
        /// <returns>
        /// 执行查询和命令时由 <see cref="T:System.Data.Linq.DataContext"/> 使用的事务对象。
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public DbTransaction Transaction
        {
            get
            {
                this.CheckDispose();
                return this.provider.Transaction;
            }
            set
            {
                this.CheckDispose();
                this.provider.Transaction = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值增大查询的超时期限，如果不增大则会在默认超时期限间出现超时。
        /// </summary>
        /// 
        /// <returns>
        /// 一个整数值，该值增大查询的超时期限，如果不增大则会在默认超时期限间出现超时。
        /// </returns>
        public int CommandTimeout
        {
            get
            {
                this.CheckDispose();
                return this.provider.CommandTimeout;
            }
            set
            {
                this.CheckDispose();
                this.provider.CommandTimeout = value;
            }
        }

        /// <summary>
        /// 获取或设置要写入的 SQL 查询或命令的目标。
        /// </summary>
        /// 
        /// <returns>
        /// 要用于编写命令的 <see cref="T:System.IO.TextReader"/>。
        /// </returns>
        public TextWriter Log
        {
            get
            {
                this.CheckDispose();
                return this.provider.Log;
            }
            set
            {
                this.CheckDispose();
                this.provider.Log = value;
            }
        }

        /// <summary>
        /// 获取或设置一个值，该值指示是否启用对象跟踪。
        /// </summary>
        /// 
        /// <returns>
        /// 如果启用跟踪对象，则为 true；否则为false 。默认值为 true。
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool ObjectTrackingEnabled
        {
            get
            {
                this.CheckDispose();
                return this.objectTrackingEnabled;
            }
            set
            {
                this.CheckDispose();
                if (this.Services.HasCachedObjects)
                    throw Error.OptionsCannotBeModifiedAfterQuery();
                this.objectTrackingEnabled = value;
                if (!this.objectTrackingEnabled)
                    this.deferredLoadingEnabled = false;
                this.services.ResetServices();
            }
        }

        /// <summary>
        /// 获取或设置指示延迟加载是一对多还是一对一的关系的值。
        /// </summary>
        /// 
        /// <returns>
        /// 如果启用延迟加载，则为 true；否则为 false。
        /// </returns>
        public bool DeferredLoadingEnabled
        {
            get
            {
                this.CheckDispose();
                return this.deferredLoadingEnabled;
            }
            set
            {
                this.CheckDispose();
                if (this.Services.HasCachedObjects)
                    throw Error.OptionsCannotBeModifiedAfterQuery();
                if (!this.ObjectTrackingEnabled & value)
                    throw Error.DeferredLoadingRequiresObjectTracking();
                this.deferredLoadingEnabled = value;
            }
        }

        /// <summary>
        /// 获取映射所基于的 <see cref="T:System.Data.Linq.Mapping.MetaModel"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 在数据库和域对象之间的映射。
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public MetaModel Mapping
        {
            get
            {
                this.CheckDispose();
                return this.services.Model;
            }
        }

        internal IProvider Provider
        {
            get
            {
                this.CheckDispose();
                return this.provider;
            }
        }

        /// <summary>
        /// 获取或设置与此 <see cref="T:System.Data.Linq.DataContext"/> 关联的 <see cref="T:System.Data.Linq.DataLoadOptions"/>。
        /// </summary>
        /// 
        /// <returns>
        /// 相关数据的预提取加载选项。
        /// </returns>
        public DataLoadOptions LoadOptions
        {
            get
            {
                this.CheckDispose();
                return this.loadOptions;
            }
            set
            {
                this.CheckDispose();
                if (this.services.HasCachedObjects && value != this.loadOptions)
                    throw Error.LoadOptionsChangeNotAllowedAfterQuery();
                if (value != null)
                    value.Freeze();
                this.loadOptions = value;
            }
        }

        /// <summary>
        /// 获取调用 <see cref="M:System.Data.Linq.DataContext.SubmitChanges"/> 时导致并发冲突的对象的集合。
        /// </summary>
        /// 
        /// <returns>
        /// 导致并发冲突的对象的集合。
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public ChangeConflictCollection ChangeConflicts
        {
            get
            {
                this.CheckDispose();
                return this.conflicts;
            }
        }

        private DataContext()
        {
        }

        /// <summary>
        /// 通过引用文件源来初始化 <see cref="T:System.Data.Linq.DataContext"/> 类的新实例。
        /// </summary>
        /// <param name="fileOrServerOrConnection">此参数可以是下列项之一：SQL Server Express 数据库所在的文件的名称。数据库所在的服务器的名称。在此情况下，提供程序对用户使用默认数据库。一个完整的连接字符串。LINQ to SQL 仅将字符串传递给提供程序，而不进行修改。</param>
        public DataContext(string fileOrServerOrConnection)
        {
            if (fileOrServerOrConnection == null)
                throw Error.ArgumentNull("fileOrServerOrConnection");
            this.InitWithDefaultMapping((object)fileOrServerOrConnection);
        }

        /// <summary>
        /// 通过引用文件源和映射源初始化 <see cref="T:System.Data.Linq.DataContext"/> 类的新实例。
        /// </summary>
        /// <param name="fileOrServerOrConnection">此参数可以是下列项之一：SQL Server Express 数据库所在的文件的名称。数据库所在的服务器的名称。在此情况下，提供程序对用户使用默认数据库。一个完整的连接字符串。LINQ to SQL 仅将字符串传递给提供程序，而不进行修改。</param><param name="mapping">映射的源。</param>
        public DataContext(string fileOrServerOrConnection, MappingSource mapping)
        {
            if (fileOrServerOrConnection == null)
                throw Error.ArgumentNull("fileOrServerOrConnection");
            if (mapping == null)
                throw Error.ArgumentNull("mapping");
            this.Init((object)fileOrServerOrConnection, mapping);
        }

        /// <summary>
        /// 通过引用由 .NET Framework 使用的连接来初始化 <see cref="T:System.Data.Linq.DataContext"/> 类的新实例。
        /// </summary>
        /// <param name="connection">由 .NET Framework 使用的连接。</param>
        public DataContext(IDbConnection connection)
        {
            if (connection == null)
                throw Error.ArgumentNull("connection");
            this.InitWithDefaultMapping((object)connection);
        }

        /// <summary>
        /// 通过引用连接和映射源初始化 <see cref="T:System.Data.Linq.DataContext"/> 类的新实例。
        /// </summary>
        /// <param name="connection">由 .NET Framework 使用的连接。</param><param name="mapping">映射的源。</param>
        public DataContext(IDbConnection connection, MappingSource mapping)
        {
            if (connection == null)
                throw Error.ArgumentNull("connection");
            if (mapping == null)
                throw Error.ArgumentNull("mapping");
            this.Init((object)connection, mapping);
        }

        internal DataContext(DataContext context)
        {
            if (context == null)
                throw Error.ArgumentNull("context");
            this.Init((object)context.Connection, context.Mapping.MappingSource);
            this.LoadOptions = context.LoadOptions;
            this.Transaction = context.Transaction;
            this.Log = context.Log;
            this.CommandTimeout = context.CommandTimeout;
        }

        /// <summary>
        /// 释放由 <see cref="T:System.Data.Linq.DataContext"/> 类的当前实例占用的所有资源。
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            this.disposed = true;
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        /// <summary>
        /// 释放由 <see cref="T:System.Data.Linq.DataContext"/> 类使用的非托管资源，还可以另外再释放托管资源。
        /// </summary>
        /// <param name="disposing">true 表示释放托管资源和非托管资源；false 表示仅释放非托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (this.provider != null)
            {
                this.provider.Dispose();
                this.provider = (IProvider)null;
            }
            this.services = (CommonDataServices)null;
            this.tables = (Dictionary<MetaTable, ITable>)null;
            this.loadOptions = (DataLoadOptions)null;
        }

        internal void CheckDispose()
        {
            if (this.disposed)
                throw Error.DataContextCannotBeUsedAfterDispose();
        }

        private void InitWithDefaultMapping(object connection)
        {
            this.Init(connection, (MappingSource)new AttributeMappingSource());
        }

        internal object Clone()
        {
            this.CheckDispose();
            Type type = this.GetType();
            object[] objArray = new object[2];
            int index1 = 0;
            DbConnection connection = this.Connection;
            objArray[index1] = (object)connection;
            int index2 = 1;
            MappingSource mappingSource = this.Mapping.MappingSource;
            objArray[index2] = (object)mappingSource;
            return Activator.CreateInstance(type, objArray);
        }

        private void Init(object connection, MappingSource mapping)
        {
            MetaModel model = mapping.GetModel(this.GetType());
            this.services = new CommonDataServices(this, model);
            this.conflicts = new ChangeConflictCollection();
            if (!(model.ProviderType != (Type)null))
                throw Error.ProviderTypeNull();
            Type providerType = model.ProviderType;
            if (!typeof(IProvider).IsAssignableFrom(providerType))
                throw Error.ProviderDoesNotImplementRequiredInterface((object)providerType, (object)typeof(IProvider));
            this.provider = (IProvider)Activator.CreateInstance(providerType);
            this.provider.Initialize((IDataServices)this.services, connection);
            this.tables = new Dictionary<MetaTable, ITable>();
            this.InitTables((object)this);
        }

        internal void ClearCache()
        {
            this.CheckDispose();
            this.services.ResetServices();
        }

        internal void VerifyTrackingEnabled()
        {
            this.CheckDispose();
            if (!this.ObjectTrackingEnabled)
                throw Error.ObjectTrackingRequired();
        }

        internal void CheckNotInSubmitChanges()
        {
            this.CheckDispose();
            if (this.isInSubmitChanges)
                throw Error.CannotPerformOperationDuringSubmitChanges();
        }

        internal void CheckInSubmitChanges()
        {
            this.CheckDispose();
            if (!this.isInSubmitChanges)
                throw Error.CannotPerformOperationOutsideSubmitChanges();
        }

        /// <summary>
        /// 返回特定类型的对象的集合，其中类型由 <paramref name="TEntity"/> 参数定义。
        /// </summary>
        /// 
        /// <returns>
        /// <paramref name="TEntity"/> 参数定义的对象集合。
        /// </returns>
        /// <typeparam name="TEntity">要返回的对象的类型。</typeparam><filterpriority>2</filterpriority>
        public Table<TEntity> GetTable<TEntity>() where TEntity : class
        {
            this.CheckDispose();
            MetaTable table1 = this.services.Model.GetTable(typeof(TEntity));
            if (table1 == null)
                throw Error.TypeIsNotMarkedAsTable((object)typeof(TEntity));
            ITable table2 = this.GetTable(table1);
            if (table2.ElementType != typeof(TEntity))
                throw Error.CouldNotGetTableForSubtype((object)typeof(TEntity), (object)table1.RowType.Type);
            return (Table<TEntity>)table2;
        }

        /// <summary>
        /// 返回特定类型的对象的集合，其中类型由 <paramref name="type"/> 参数定义。
        /// </summary>
        /// 
        /// <returns>
        /// <paramref name="type"/> 参数定义的对象集合。
        /// </returns>
        /// <param name="type">要返回的对象的类型。</param><filterpriority>2</filterpriority>
        public ITable GetTable(Type type)
        {
            this.CheckDispose();
            if (type == (Type)null)
                throw Error.ArgumentNull("type");
            MetaTable table = this.services.Model.GetTable(type);
            if (table == null)
                throw Error.TypeIsNotMarkedAsTable((object)type);
            if (table.RowType.Type != type)
                throw Error.CouldNotGetTableForSubtype((object)type, (object)table.RowType.Type);
            return this.GetTable(table);
        }

        private ITable GetTable(MetaTable metaTable)
        {
            ITable table;
            if (!this.tables.TryGetValue(metaTable, out table))
            {
                DataContext.ValidateTable(metaTable);
                Type type1 = typeof(Table<>);
                Type[] typeArray = new Type[1];
                int index1 = 0;
                Type type2 = metaTable.RowType.Type;
                typeArray[index1] = type2;
                Type type3 = type1.MakeGenericType(typeArray);
                int num = 52;
                // ISSUE: variable of the null type
                //__Null local1 = null;
                object[] args = new object[2];
                int index2 = 0;
                args[index2] = (object)this;
                int index3 = 1;
                MetaTable metaTable1 = metaTable;
                args[index3] = (object)metaTable1;
                // ISSUE: variable of the null type
                //__Null local2 = null;
                table = (ITable)Activator.CreateInstance(type3, (BindingFlags)num, (Binder)null, args, (CultureInfo)null);
                this.tables.Add(metaTable, table);
            }
            return table;
        }

        private static void ValidateTable(MetaTable metaTable)
        {
            foreach (MetaAssociation metaAssociation in metaTable.RowType.Associations)
            {
                if (!metaAssociation.ThisMember.DeclaringType.IsEntity)
                    throw Error.NonEntityAssociationMapping((object)metaAssociation.ThisMember.DeclaringType.Type, (object)metaAssociation.ThisMember.Name, (object)metaAssociation.ThisMember.DeclaringType.Type);
                if (!metaAssociation.OtherType.IsEntity)
                    throw Error.NonEntityAssociationMapping((object)metaAssociation.ThisMember.DeclaringType.Type, (object)metaAssociation.ThisMember.Name, (object)metaAssociation.OtherType.Type);
            }
        }

        private void InitTables(object schema)
        {
            foreach (FieldInfo fieldInfo in schema.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                Type fieldType = fieldInfo.FieldType;
                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Table<>) && (ITable)fieldInfo.GetValue(schema) == null)
                {
                    ITable table = this.GetTable(fieldType.GetGenericArguments()[0]);
                    fieldInfo.SetValue(schema, (object)table);
                }
            }
        }

        /// <summary>
        /// 确定是否可以打开关联数据库。
        /// </summary>
        /// 
        /// <returns>
        /// 如果可以打开指定的数据库，则为 true；否则为 false。
        /// </returns>
        public bool DatabaseExists()
        {
            this.CheckDispose();
            return this.provider.DatabaseExists();
        }

        /// <summary>
        /// 在服务器上创建数据库。
        /// </summary>
        public void CreateDatabase()
        {
            this.CheckDispose();
            this.provider.CreateDatabase();
        }

        /// <summary>
        /// 删除关联数据库。
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void DeleteDatabase()
        {
            this.CheckDispose();
            this.provider.DeleteDatabase();
        }

        /// <summary>
        /// 计算要插入、更新或删除的已修改对象的集，并执行相应命令以实现对数据库的更改。
        /// </summary>
        public void SubmitChanges()
        {
            this.CheckDispose();
            this.SubmitChanges(ConflictMode.FailOnFirstConflict);
        }

        /// <summary>
        /// 将对检索到的对象所做的更改发送到基础数据库，并指定提交失败时要采取的操作。
        /// </summary>
        /// <param name="failureMode">提交失败时要采取的操作。有效参数包括：<see cref="F:System.Data.Linq.ConflictMode.FailOnFirstConflict"/><see cref="F:System.Data.Linq.ConflictMode.ContinueOnConflict"/></param>
        public virtual void SubmitChanges(ConflictMode failureMode)
        {
            this.CheckDispose();
            this.CheckNotInSubmitChanges();
            this.VerifyTrackingEnabled();
            this.conflicts.Clear();
            try
            {
                this.isInSubmitChanges = true;
                if (System.Transactions.Transaction.Current == (System.Transactions.Transaction)null && this.provider.Transaction == null)
                {
                    bool flag = false;
                    DbTransaction dbTransaction = (DbTransaction)null;
                    try
                    {
                        if (this.provider.Connection.State == ConnectionState.Open)
                            this.provider.ClearConnection();
                        if (this.provider.Connection.State == ConnectionState.Closed)
                        {
                            this.provider.Connection.Open();
                            flag = true;
                        }
                        dbTransaction = this.provider.Connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                        this.provider.Transaction = dbTransaction;
                        new ChangeProcessor(this.services, this).SubmitChanges(failureMode);
                        this.AcceptChanges();
                        this.provider.ClearConnection();
                        dbTransaction.Commit();
                    }
                    catch
                    {
                        if (dbTransaction != null)
                            dbTransaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        this.provider.Transaction = (DbTransaction)null;
                        if (flag)
                            this.provider.Connection.Close();
                    }
                }
                else
                {
                    new ChangeProcessor(this.services, this).SubmitChanges(failureMode);
                    this.AcceptChanges();
                }
            }
            finally
            {
                this.isInSubmitChanges = false;
            }
        }

        /// <summary>
        /// 按照指定模式刷新实体对象。
        /// </summary>
        /// <param name="mode">一个指定如何处理开放式并发冲突的值。</param><param name="entity">要刷新的对象。</param><filterpriority>2</filterpriority>
        public void Refresh(RefreshMode mode, object entity)
        {
            this.CheckDispose();
            this.CheckNotInSubmitChanges();
            this.VerifyTrackingEnabled();
            if (entity == null)
                throw Error.ArgumentNull("entity");
            Array instance = Array.CreateInstance(entity.GetType(), 1);
            instance.SetValue(entity, 0);
            this.Refresh(mode, (IEnumerable)instance);
        }

        /// <summary>
        /// 按照指定模式刷新实体对象的数组。
        /// </summary>
        /// <param name="mode">一个指定如何处理开放式并发冲突的值。</param><param name="entities">要刷新的实体对象的数组。</param><filterpriority>2</filterpriority>
        public void Refresh(RefreshMode mode, params object[] entities)
        {
            this.CheckDispose();
            if (entities == null)
                throw Error.ArgumentNull("entities");
            this.Refresh(mode, (IEnumerable)entities);
        }

        /// <summary>
        /// 按照指定模式刷新实体对象的集合。
        /// </summary>
        /// <param name="mode">一个指定如何处理开放式并发冲突的值。</param><param name="entities">要刷新的实体的集合。</param><filterpriority>2</filterpriority>
        public void Refresh(RefreshMode mode, IEnumerable entities)
        {
            this.CheckDispose();
            this.CheckNotInSubmitChanges();
            this.VerifyTrackingEnabled();
            if (entities == null)
                throw Error.ArgumentNull("entities");
            List<object> list = System.Linq.Enumerable.ToList<object>(System.Linq.Enumerable.Cast<object>(entities));
            DataContext refreshContext = this.CreateRefreshContext();
            foreach (object obj in list)
            {
                this.GetTable(this.services.Model.GetMetaType(obj.GetType()).InheritanceRoot.Type);
                TrackedObject trackedObject = this.services.ChangeTracker.GetTrackedObject(obj);
                if (trackedObject == null)
                    throw Error.UnrecognizedRefreshObject();
                if (trackedObject.IsNew)
                    throw Error.RefreshOfNewObject();
                object[] keyValues = CommonDataServices.GetKeyValues(trackedObject.Type, trackedObject.Original);
                object objectByKey = refreshContext.Services.GetObjectByKey(trackedObject.Type, keyValues);
                if (objectByKey == null)
                    throw Error.RefreshOfDeletedObject();
                trackedObject.Refresh(mode, objectByKey);
            }
        }

        internal DataContext CreateRefreshContext()
        {
            this.CheckDispose();
            return new DataContext(this);
        }

        private void AcceptChanges()
        {
            this.CheckDispose();
            this.VerifyTrackingEnabled();
            this.services.ChangeTracker.AcceptChanges();
        }

        internal string GetQueryText(IQueryable query)
        {
            this.CheckDispose();
            if (query == null)
                throw Error.ArgumentNull("query");
            return this.provider.GetQueryText(query.Expression);
        }

        /// <summary>
        /// 获取提供有关由 LINQ to SQL 生成的 SQL 命令的信息。
        /// </summary>
        /// 
        /// <returns>
        /// 请求的命令信息对象。
        /// </returns>
        /// <param name="query">要检索其 SQL 命令信息的查询。</param><filterpriority>2</filterpriority>
        public DbCommand GetCommand(IQueryable query)
        {
            this.CheckDispose();
            if (query == null)
                throw Error.ArgumentNull("query");
            return this.provider.GetCommand(query.Expression);
        }

        internal string GetChangeText()
        {
            this.CheckDispose();
            this.VerifyTrackingEnabled();
            return new ChangeProcessor(this.services, this).GetChangeText();
        }

        /// <summary>
        /// 获取由 <see cref="T:System.Data.Linq.DataContext"/>跟踪的被修改对象。
        /// </summary>
        /// 
        /// <returns>
        /// 该对象集返回为三个只读的集合。
        /// </returns>
        public ChangeSet GetChangeSet()
        {
            this.CheckDispose();
            return new ChangeProcessor(this.services, this).GetChangeSet();
        }

        /// <summary>
        /// 直接对数据库执行 SQL 命令。
        /// </summary>
        /// 
        /// <returns>
        /// 执行命令的修改的行数。
        /// </returns>
        /// <param name="command">要执行的 SQL 命令。</param><param name="parameters">要传递给命令的参数数组。注意下面的行为：如果数组中的对象的数目小于命令字符串中已标识的最大数，则会引发异常。如果数组包含未在命令字符串中引用的对象，则不会引发异常。如果任一参数为 null，则该参数会转换为 DBNull.Value。</param><filterpriority>2</filterpriority>
        public int ExecuteCommand(string command, params object[] parameters)
        {
            this.CheckDispose();
            if (command == null)
                throw Error.ArgumentNull("command");
            if (parameters == null)
                throw Error.ArgumentNull("parameters");
            MethodInfo methodInfo = (MethodInfo)MethodBase.GetCurrentMethod();
            object[] objArray1 = new object[2];
            int index1 = 0;
            string str = command;
            objArray1[index1] = (object)str;
            int index2 = 1;
            object[] objArray2 = parameters;
            objArray1[index2] = (object)objArray2;
            return (int)this.ExecuteMethodCall((object)this, methodInfo, objArray1).ReturnValue;
        }

        /// <summary>
        /// 直接对数据库执行 SQL 查询并返回对象。
        /// </summary>
        /// 
        /// <returns>
        /// 由查询返回的对象的集合。
        /// </returns>
        /// <param name="query">要执行的 SQL 查询。</param><param name="parameters">要传递给命令的参数数组。注意下面的行为：如果数组中的对象的数目小于命令字符串中已标识的最大数，则会引发异常。如果数组包含未在命令字符串中引用的对象，则不会引发异常。如果某参数为 null，则该参数会转换为 DBNull.Value。</param><typeparam name="TResult">返回的集合中的元素的类型。</typeparam><filterpriority>2</filterpriority>
        public IEnumerable<TResult> ExecuteQuery<TResult>(string query, params object[] parameters)
        {
            this.CheckDispose();
            if (query == null)
                throw Error.ArgumentNull("query");
            if (parameters == null)
                throw Error.ArgumentNull("parameters");
            MethodInfo methodInfo1 = (MethodInfo)MethodBase.GetCurrentMethod();
            Type[] typeArray = new Type[1];
            int index1 = 0;
            Type type = typeof(TResult);
            typeArray[index1] = type;
            MethodInfo methodInfo2 = methodInfo1.MakeGenericMethod(typeArray);
            object[] objArray1 = new object[2];
            int index2 = 0;
            string str = query;
            objArray1[index2] = (object)str;
            int index3 = 1;
            object[] objArray2 = parameters;
            objArray1[index3] = (object)objArray2;
            return (IEnumerable<TResult>)this.ExecuteMethodCall((object)this, methodInfo2, objArray1).ReturnValue;
        }

        /// <summary>
        /// 直接对数据库执行 SQL 查询。
        /// </summary>
        /// 
        /// <returns>
        /// 由查询返回的对象的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 集合。
        /// </returns>
        /// <param name="elementType">要返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 的类型。使查询结果中的列与对象中的字段或属性相匹配的算法如下所示：如果字段或属性映射到特定列名称，则结果集中应包含该列名称。如果未映射字段或属性，则结果集中应包含其名称与该字段或属性相同的列。通过先查找区分大小写的匹配来执行比较。如果未找到匹配项，则会继续搜索不区分大小写的匹配项。如果同时满足下列所有条件，则该查询应当返回（除延迟加载的对象外的）对象的所有跟踪的字段和属性：<paramref name="T"/> 是由 <see cref="T:System.Data.Linq.DataContext"/> 显式跟踪的实体。<see cref="P:System.Data.Linq.DataContext.ObjectTrackingEnabled"/> 为 true。实体具有主键。否则会引发异常。</param><param name="query">要执行的 SQL 查询。</param><param name="parameters">要传递给命令的参数数组。注意下面的行为：如果数组中的对象的数目小于命令字符串中已标识的最大数，则会引发异常。如果数组包含未在命令字符串中引用的对象，则不会引发异常。如果某参数为 null，则该参数会转换为 DBNull.Value。</param><filterpriority>2</filterpriority>
        public IEnumerable ExecuteQuery(Type elementType, string query, params object[] parameters)
        {
            this.CheckDispose();
            if (elementType == (Type)null)
                throw Error.ArgumentNull("elementType");
            if (query == null)
                throw Error.ArgumentNull("query");
            if (parameters == null)
                throw Error.ArgumentNull("parameters");
            if (DataContext._miExecuteQuery == (MethodInfo)null)
                DataContext._miExecuteQuery = System.Linq.Enumerable.Single<MethodInfo>((IEnumerable<MethodInfo>)typeof(DataContext).GetMethods(), (Func<MethodInfo, bool>)(m =>
                {
                    if (m.Name == "ExecuteQuery")
                        return m.GetParameters().Length == 2;
                    return false;
                }));
            MethodInfo methodInfo1 = DataContext._miExecuteQuery;
            Type[] typeArray = new Type[1];
            int index1 = 0;
            Type type = elementType;
            typeArray[index1] = type;
            MethodInfo methodInfo2 = methodInfo1.MakeGenericMethod(typeArray);
            object[] objArray1 = new object[2];
            int index2 = 0;
            string str = query;
            objArray1[index2] = (object)str;
            int index3 = 1;
            object[] objArray2 = parameters;
            objArray1[index3] = (object)objArray2;
            return (IEnumerable)this.ExecuteMethodCall((object)this, methodInfo2, objArray1).ReturnValue;
        }

        /// <summary>
        /// 执行数据库存储过程或与指定的 CLR 方法关联的标量函数。
        /// </summary>
        /// 
        /// <returns>
        /// 执行指定方法的结果（返回值和输出参数）。
        /// </returns>
        /// <param name="instance">方法调用的实例（当前对象）。</param><param name="methodInfo">标识与数据库方法相对应的 CLR 方法。</param><param name="parameters">要传递给命令的参数数组。</param>
        protected internal IExecuteResult ExecuteMethodCall(object instance, MethodInfo methodInfo, params object[] parameters)
        {
            this.CheckDispose();
            if (instance == null)
                throw Error.ArgumentNull("instance");
            if (methodInfo == (MethodInfo)null)
                throw Error.ArgumentNull("methodInfo");
            if (parameters == null)
                throw Error.ArgumentNull("parameters");
            return this.provider.Execute(this.GetMethodCall(instance, methodInfo, parameters));
        }

        /// <summary>
        /// 执行与指定的 CLR 方法相关联的表值数据库函数。
        /// </summary>
        /// 
        /// <returns>
        /// 由数据库查询返回的最终值的集合。
        /// </returns>
        /// <param name="instance">方法调用的实例（当前对象）。</param><param name="methodInfo">标识与数据库方法相对应的 CLR 方法的 <see cref="T:System.Reflection.MethodInfo"/>。</param><param name="parameters">要传递给命令的参数数组。</param><typeparam name="TResult">返回的集合中的元素的类型。</typeparam>
        protected internal IQueryable<TResult> CreateMethodCallQuery<TResult>(object instance, MethodInfo methodInfo, params object[] parameters)
        {
            this.CheckDispose();
            if (instance == null)
                throw Error.ArgumentNull("instance");
            if (methodInfo == (MethodInfo)null)
                throw Error.ArgumentNull("methodInfo");
            if (parameters == null)
                throw Error.ArgumentNull("parameters");
            if (!typeof(IQueryable<TResult>).IsAssignableFrom(methodInfo.ReturnType))
                throw Error.ExpectedQueryableArgument((object)"methodInfo", (object)typeof(IQueryable<TResult>));
            return (IQueryable<TResult>)new DataQuery<TResult>(this, this.GetMethodCall(instance, methodInfo, parameters));
        }

        private Expression GetMethodCall(object instance, MethodInfo methodInfo, params object[] parameters)
        {
            this.CheckDispose();
            if (parameters.Length == 0)
                return (Expression)Expression.Call((Expression)Expression.Constant(instance), methodInfo);
            ParameterInfo[] parameters1 = methodInfo.GetParameters();
            List<Expression> list = new List<Expression>(parameters.Length);
            int index = 0;
            for (int length = parameters.Length; index < length; ++index)
            {
                Type type = parameters1[index].ParameterType;
                if (type.IsByRef)
                    type = type.GetElementType();
                list.Add((Expression)Expression.Constant(parameters[index], type));
            }
            return (Expression)Expression.Call((Expression)Expression.Constant(instance), methodInfo, (IEnumerable<Expression>)list);
        }

        /// <summary>
        /// 在插入重写方法中执行，以向 LINQ to SQL 重新委托生成和执行插入操作的动态 SQL 的任务。
        /// </summary>
        /// <param name="entity">要插入的实体。</param>
        protected internal void ExecuteDynamicInsert(object entity)
        {
            this.CheckDispose();
            if (entity == null)
                throw Error.ArgumentNull("entity");
            this.CheckInSubmitChanges();
            TrackedObject trackedObject = this.services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject == null)
                throw Error.CannotPerformOperationForUntrackedObject();
            this.services.ChangeDirector.DynamicInsert(trackedObject);
        }

        /// <summary>
        /// 在更新重写方法中执行，以向 LINQ to SQL 重新委托生成和执行更新操作的动态 SQL 的任务。
        /// </summary>
        /// <param name="entity">要更新的实体。</param>
        protected internal void ExecuteDynamicUpdate(object entity)
        {
            this.CheckDispose();
            if (entity == null)
                throw Error.ArgumentNull("entity");
            this.CheckInSubmitChanges();
            TrackedObject trackedObject = this.services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject == null)
                throw Error.CannotPerformOperationForUntrackedObject();
            if (this.services.ChangeDirector.DynamicUpdate(trackedObject) == 0)
                throw new ChangeConflictException();
        }

        /// <summary>
        /// 在删除重写方法中执行，以向 LINQ to SQL 重新委托生成和执行删除操作的动态 SQL 的任务。
        /// </summary>
        /// <param name="entity">要删除的实体。</param>
        protected internal void ExecuteDynamicDelete(object entity)
        {
            this.CheckDispose();
            if (entity == null)
                throw Error.ArgumentNull("entity");
            this.CheckInSubmitChanges();
            TrackedObject trackedObject = this.services.ChangeTracker.GetTrackedObject(entity);
            if (trackedObject == null)
                throw Error.CannotPerformOperationForUntrackedObject();
            if (this.services.ChangeDirector.DynamicDelete(trackedObject) == 0)
                throw new ChangeConflictException();
        }

        /// <summary>
        /// 将现有 <see cref="T:System.Data.Common.DbDataReader"/> 转换为对象。
        /// </summary>
        /// 
        /// <returns>
        /// 由转换返回的对象的集合。
        /// </returns>
        /// <param name="reader">要转换的 <see cref="T:System.Data.IDataReader"/>。</param><typeparam name="TResult">要返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 的类型。</typeparam>
        public IEnumerable<TResult> Translate<TResult>(DbDataReader reader)
        {
            this.CheckDispose();
            return (IEnumerable<TResult>)this.Translate(typeof(TResult), reader);
        }

        /// <summary>
        /// 将现有 <see cref="T:System.Data.Common.DbDataReader"/> 转换为对象。
        /// </summary>
        /// 
        /// <returns>
        /// 由转换返回的对象的列表。
        /// </returns>
        /// <param name="elementType">要返回的 <see cref="T:System.Collections.Generic.IEnumerable`1"/> 的类型。使查询结果中的列与对象中的字段和属性相匹配的算法如下所示：如果字段或属性映射到特定列名称，则结果集中应包含该列名称。如果未映射字段或属性，则结果集中应包含其名称与该字段或属性相同的列。通过先查找区分大小写的匹配来执行比较。如果未找到匹配项，则会继续搜索不区分大小写的匹配项。如果同时满足下列所有条件，则该查询应当返回（除延迟加载的对象外的）对象的所有跟踪的字段和属性：<paramref name="T"/> 是由 <see cref="T:System.Data.Linq.DataContext"/> 显式跟踪的实体。<see cref="P:System.Data.Linq.DataContext.ObjectTrackingEnabled"/> 为 true。实体具有主键。否则会引发异常。</param><param name="reader">要转换的 <see cref="T:System.Data.IDataReader"/>。</param>
        public IEnumerable Translate(Type elementType, DbDataReader reader)
        {
            this.CheckDispose();
            if (elementType == (Type)null)
                throw Error.ArgumentNull("elementType");
            if (reader == null)
                throw Error.ArgumentNull("reader");
            return this.provider.Translate(elementType, reader);
        }

        /// <summary>
        /// 将现有 <see cref="T:System.Data.Common.DbDataReader"/> 转换为对象。
        /// </summary>
        /// 
        /// <returns>
        /// 由转换返回的对象的列表。
        /// </returns>
        /// <param name="reader">要转换的 <see cref="T:System.Data.IDataReader"/>。</param>
        public IMultipleResults Translate(DbDataReader reader)
        {
            this.CheckDispose();
            if (reader == null)
                throw Error.ArgumentNull("reader");
            return this.provider.Translate(reader);
        }

        internal void ResetLoadOptions()
        {
            this.CheckDispose();
            this.loadOptions = (DataLoadOptions)null;
        }
    }

    /// <summary>
    /// 提供保存更改的容器。
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public sealed class ChangeSet
    {
        private ReadOnlyCollection<object> inserts;
        private ReadOnlyCollection<object> deletes;
        private ReadOnlyCollection<object> updates;

        /// <summary>
        /// 获取已插入到 <see cref="T:System.Data.Linq.ChangeSet"/> 中的实体的列表。
        /// </summary>
        /// 
        /// <returns>
        /// 插入实体的 <see cref="T:System.Collections.IList"/>。
        /// </returns>
        public IList<object> Inserts
        {
            get
            {
                return (IList<object>)this.inserts;
            }
        }

        /// <summary>
        /// 获取已从 <see cref="T:System.Data.Linq.ChangeSet"/> 中删除的实体的列表。
        /// </summary>
        /// 
        /// <returns>
        /// 删除实体的 <see cref="T:System.Collections.IList"/>。
        /// </returns>
        public IList<object> Deletes
        {
            get
            {
                return (IList<object>)this.deletes;
            }
        }

        /// <summary>
        /// 获取已在 <see cref="T:System.Data.Linq.ChangeSet"/> 中更新的实体的列表。
        /// </summary>
        /// 
        /// <returns>
        /// 更新实体的 <see cref="T:System.Collections.IList"/>。
        /// </returns>
        public IList<object> Updates
        {
            get
            {
                return (IList<object>)this.updates;
            }
        }

        internal ChangeSet(ReadOnlyCollection<object> inserts, ReadOnlyCollection<object> deletes, ReadOnlyCollection<object> updates)
        {
            this.inserts = inserts;
            this.deletes = deletes;
            this.updates = updates;
        }

        /// <summary>
        /// 返回表示当前 <see cref="T:System.Data.Linq.ChangeSet"/> 的字符串。
        /// </summary>
        /// 
        /// <returns>
        /// 表示当前 <see cref="T:System.Data.Linq.ChangeSet"/> 的字符串。
        /// </returns>
        public override string ToString()
        {
            string str1 = "{";
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            string format = "Inserts: {0}, Deletes: {1}, Updates: {2}";
            object[] objArray = new object[3];
            int index1 = 0;
            // ISSUE: variable of a boxed type
           var local1 = (ValueType)this.Inserts.Count;
            objArray[index1] = (object)local1;
            int index2 = 1;
            // ISSUE: variable of a boxed type
            var local2 = (ValueType)this.Deletes.Count;
            objArray[index2] = (object)local2;
            int index3 = 2;
            // ISSUE: variable of a boxed type
           var local3 = (ValueType)this.Updates.Count;
            objArray[index3] = (object)local3;
            string str2 = string.Format((IFormatProvider)invariantCulture, format, objArray);
            string str3 = "}";
            return str1 + str2 + str3;
        }
    }

    /// <summary>
    /// 指定应何时报告并发冲突。
    /// </summary>
    public enum ConflictMode
    {
        FailOnFirstConflict,
        ContinueOnConflict,
    }
}
