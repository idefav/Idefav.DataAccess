using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ExpressionTest
{
    public class SqlProvider : IReaderProvider, IProvider, IDisposable, IConnectionUser
    {
        private string dbName = string.Empty;
        private OptimizationFlags optimizationFlags = OptimizationFlags.All;
        private bool enableCacheLookup = true;
        private IDataServices services;
        private SqlConnectionManager conManager;
        private TypeSystemProvider typeProvider;
        private SqlFactory sqlFactory;
        private Translator translator;
        private IObjectReaderCompiler readerCompiler;
        private bool disposed;
        private int commandTimeout;
        private TextWriter log;
        private int queryCount;
        private bool checkQueries;
        private SqlProvider.ProviderMode mode;
        private bool deleted;
        private const string SqlCeProviderInvariantName = "System.Data.SqlServerCe.3.5";
        private const string SqlCeDataReaderTypeName = "System.Data.SqlServerCe.SqlCeDataReader";
        private const string SqlCeConnectionTypeName = "System.Data.SqlServerCe.SqlCeConnection";
        private const string SqlCeTransactionTypeName = "System.Data.SqlServerCe.SqlCeTransaction";

        internal SqlProvider.ProviderMode Mode
        {
            get
            {
                this.CheckDispose();
                this.CheckInitialized();
                this.InitializeProviderMode();
                return this.mode;
            }
        }

        private bool IsSqlCe
        {
            get
            {
                DbConnection dbConnection = this.conManager.UseConnection((IConnectionUser)this);
                try
                {
                    if (string.CompareOrdinal(dbConnection.GetType().FullName, "System.Data.SqlServerCe.SqlCeConnection") == 0)
                        return true;
                }
                finally
                {
                    this.conManager.ReleaseConnection((IConnectionUser)this);
                }
                return false;
            }
        }

        private bool IsServer2KOrEarlier
        {
            get
            {
                DbConnection dbConnection = this.conManager.UseConnection((IConnectionUser)this);
                try
                {
                    string serverVersion = dbConnection.ServerVersion;
                    return serverVersion.StartsWith("06.00.", StringComparison.Ordinal) || serverVersion.StartsWith("06.50.", StringComparison.Ordinal) || (serverVersion.StartsWith("07.00.", StringComparison.Ordinal) || serverVersion.StartsWith("08.00.", StringComparison.Ordinal));
                }
                finally
                {
                    this.conManager.ReleaseConnection((IConnectionUser)this);
                }
            }
        }

        private bool IsServer2005
        {
            get
            {
                DbConnection dbConnection = this.conManager.UseConnection((IConnectionUser)this);
                try
                {
                    return dbConnection.ServerVersion.StartsWith("09.00.", StringComparison.Ordinal);
                }
                finally
                {
                    this.conManager.ReleaseConnection((IConnectionUser)this);
                }
            }
        }

        DbConnection IProvider.Connection
        {
            get
            {
                this.CheckDispose();
                this.CheckInitialized();
                return this.conManager.Connection;
            }
        }

        TextWriter IProvider.Log
        {
            get
            {
                this.CheckDispose();
                this.CheckInitialized();
                return this.log;
            }
            set
            {
                this.CheckDispose();
                this.CheckInitialized();
                this.log = value;
            }
        }

        DbTransaction IProvider.Transaction
        {
            get
            {
                this.CheckDispose();
                this.CheckInitialized();
                return this.conManager.Transaction;
            }
            set
            {
                this.CheckDispose();
                this.CheckInitialized();
                this.conManager.Transaction = value;
            }
        }

        int IProvider.CommandTimeout
        {
            get
            {
                this.CheckDispose();
                return this.commandTimeout;
            }
            set
            {
                this.CheckDispose();
                this.commandTimeout = value;
            }
        }

        internal OptimizationFlags OptimizationFlags
        {
            get
            {
                this.CheckDispose();
                return this.optimizationFlags;
            }
            set
            {
                this.CheckDispose();
                this.optimizationFlags = value;
            }
        }

        internal bool CheckQueries
        {
            get
            {
                this.CheckDispose();
                return this.checkQueries;
            }
            set
            {
                this.CheckDispose();
                this.checkQueries = value;
            }
        }

        internal bool EnableCacheLookup
        {
            get
            {
                this.CheckDispose();
                return this.enableCacheLookup;
            }
            set
            {
                this.CheckDispose();
                this.enableCacheLookup = value;
            }
        }

        internal int QueryCount
        {
            get
            {
                this.CheckDispose();
                return this.queryCount;
            }
        }

        internal int MaxUsers
        {
            get
            {
                this.CheckDispose();
                return this.conManager.MaxUsers;
            }
        }

        IDataServices IReaderProvider.Services
        {
            get
            {
                return this.services;
            }
        }

        IConnectionManager IReaderProvider.ConnectionManager
        {
            get
            {
                return (IConnectionManager)this.conManager;
            }
        }

        /// <summary>
        /// 初始化 <see cref="T:System.Data.Linq.SqlClient.SqlProvider"/> 类的新实例。
        /// </summary>
        public SqlProvider()
        {
            this.mode = SqlProvider.ProviderMode.NotYetDecided;
        }

        internal SqlProvider(SqlProvider.ProviderMode mode)
        {
            this.mode = mode;
        }

        private void InitializeProviderMode()
        {
            if (this.mode == SqlProvider.ProviderMode.NotYetDecided)
                this.mode = !this.IsSqlCe ? (!this.IsServer2KOrEarlier ? (!this.IsServer2005 ? SqlProvider.ProviderMode.Sql2008 : SqlProvider.ProviderMode.Sql2005) : SqlProvider.ProviderMode.Sql2000) : SqlProvider.ProviderMode.SqlCE;
            if (this.typeProvider == null)
            {
                switch (this.mode)
                {
                    case SqlProvider.ProviderMode.Sql2000:
                        this.typeProvider = SqlTypeSystem.Create2000Provider();
                        break;
                    case SqlProvider.ProviderMode.Sql2005:
                        this.typeProvider = SqlTypeSystem.Create2005Provider();
                        break;
                    case SqlProvider.ProviderMode.Sql2008:
                        this.typeProvider = SqlTypeSystem.Create2008Provider();
                        break;
                    case SqlProvider.ProviderMode.SqlCE:
                        this.typeProvider = SqlTypeSystem.CreateCEProvider();
                        break;
                }
            }
            if (this.sqlFactory != null)
                return;
            this.sqlFactory = new SqlFactory(this.typeProvider, this.services.Model);
            this.translator = new Translator(this.services, this.sqlFactory, this.typeProvider);
        }

        private void CheckInitialized()
        {
            if (this.services == null)
                throw Error.ContextNotInitialized();
        }

        private void CheckNotDeleted()
        {
            if (this.deleted)
                throw Error.DatabaseDeleteThroughContext();
        }

        void IProvider.Initialize(IDataServices dataServices, object connection)
        {
            if (dataServices == null)
                throw Error.ArgumentNull("dataServices");
            this.services = dataServices;
            DbTransaction dbTransaction = (DbTransaction)null;
            string fileOrServerOrConnectionString = connection as string;
            DbConnection con;
            if (fileOrServerOrConnectionString != null)
            {
                string connectionString = this.GetConnectionString(fileOrServerOrConnectionString);
                this.dbName = this.GetDatabaseName(connectionString);
                if (this.dbName.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
                    this.mode = SqlProvider.ProviderMode.SqlCE;
                if (this.mode == SqlProvider.ProviderMode.SqlCE)
                {
                    DbProviderFactory provider = SqlProvider.GetProvider("System.Data.SqlServerCe.3.5");
                    if (provider == null)
                        throw Error.ProviderNotInstalled((object)this.dbName, (object)"System.Data.SqlServerCe.3.5");
                    con = provider.CreateConnection();
                }
                else
                    con = (DbConnection)new SqlConnection();
                con.ConnectionString = connectionString;
            }
            else
            {
                dbTransaction = (DbTransaction)(connection as SqlTransaction);
                if (dbTransaction == null && connection.GetType().FullName == "System.Data.SqlServerCe.SqlCeTransaction")
                    dbTransaction = connection as DbTransaction;
                if (dbTransaction != null)
                    connection = (object)dbTransaction.Connection;
                con = connection as DbConnection;
                if (con == null)
                    throw Error.InvalidConnectionArgument((object)"connection");
                if (con.GetType().FullName == "System.Data.SqlServerCe.SqlCeConnection")
                    this.mode = SqlProvider.ProviderMode.SqlCE;
                this.dbName = this.GetDatabaseName(con.ConnectionString);
            }
            using (DbCommand command = con.CreateCommand())
                this.commandTimeout = command.CommandTimeout;
            int maxUsers = 1;
            if (con.ConnectionString.IndexOf("MultipleActiveResultSets", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder();
                string connectionString = con.ConnectionString;
                connectionStringBuilder.ConnectionString = connectionString;
                string index = "MultipleActiveResultSets";
                if (string.Compare((string)connectionStringBuilder[index], "true", StringComparison.OrdinalIgnoreCase) == 0)
                    maxUsers = 10;
            }
            this.conManager = new SqlConnectionManager((IProvider)this, con, maxUsers, fileOrServerOrConnectionString != null);
            if (dbTransaction != null)
                this.conManager.Transaction = dbTransaction;
            this.readerCompiler = (IObjectReaderCompiler)new ObjectReaderCompiler(this.mode != SqlProvider.ProviderMode.SqlCE ? (!(con is SqlConnection) ? typeof(DbDataReader) : typeof(SqlDataReader)) : con.GetType().Module.GetType("System.Data.SqlServerCe.SqlCeDataReader"), this.services);
        }

        private static DbProviderFactory GetProvider(string providerName)
        {
            IEnumerable<DataRow> source = System.Linq.Enumerable.OfType<DataRow>((IEnumerable)DbProviderFactories.GetFactoryClasses().Rows);
            Func<DataRow, string> func = (Func<DataRow, string>)(r => (string)r["InvariantName"]);
            Func<DataRow, string> selector = null;
            if (System.Linq.Enumerable.Contains<string>(System.Linq.Enumerable.Select<DataRow, string>(source, selector), providerName, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase))
                return DbProviderFactories.GetFactory(providerName);
            return (DbProviderFactory)null;
        }

        /// <summary>
        /// 释放托管引用，并关闭由 <see cref="T:System.Data.Linq.SqlClient.SqlProvider"/> 打开的连接。
        /// </summary>
        public void Dispose()
        {
            this.disposed = true;
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        /// <summary>
        /// 可选择释放托管引用，并关闭由 <see cref="T:System.Data.Linq.SqlClient.SqlProvider"/> 打开的连接。
        /// </summary>
        /// <param name="disposing">如果释放托管引用并关闭连接，则为 true；否则为 false。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            this.services = (IDataServices)null;
            if (this.conManager != null)
                this.conManager.DisposeConnection();
            this.conManager = (SqlConnectionManager)null;
            this.typeProvider = (TypeSystemProvider)null;
            this.sqlFactory = (SqlFactory)null;
            this.translator = (Translator)null;
            this.readerCompiler = (IObjectReaderCompiler)null;
            this.log = (TextWriter)null;
        }

        internal void CheckDispose()
        {
            if (this.disposed)
                throw Error.ProviderCannotBeUsedAfterDispose();
        }

        private string GetConnectionString(string fileOrServerOrConnectionString)
        {
            if (fileOrServerOrConnectionString.IndexOf('=') >= 0)
                return fileOrServerOrConnectionString;
            DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder();
            if (fileOrServerOrConnectionString.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
            {
                connectionStringBuilder.Add("AttachDBFileName", (object)fileOrServerOrConnectionString);
                connectionStringBuilder.Add("Server", (object)"localhost\\sqlexpress");
                connectionStringBuilder.Add("Integrated Security", (object)"SSPI");
                connectionStringBuilder.Add("User Instance", (object)"true");
                connectionStringBuilder.Add("MultipleActiveResultSets", (object)"true");
            }
            else if (fileOrServerOrConnectionString.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
            {
                connectionStringBuilder.Add("Data Source", (object)fileOrServerOrConnectionString);
            }
            else
            {
                connectionStringBuilder.Add("Server", (object)fileOrServerOrConnectionString);
                connectionStringBuilder.Add("Database", (object)this.services.Model.DatabaseName);
                connectionStringBuilder.Add("Integrated Security", (object)"SSPI");
            }
            return connectionStringBuilder.ToString();
        }

        private string GetDatabaseName(string constr)
        {
            DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = constr;
            if (connectionStringBuilder.ContainsKey("Initial Catalog"))
                return (string)connectionStringBuilder["Initial Catalog"];
            if (connectionStringBuilder.ContainsKey("Database"))
                return (string)connectionStringBuilder["Database"];
            if (connectionStringBuilder.ContainsKey("AttachDBFileName"))
                return (string)connectionStringBuilder["AttachDBFileName"];
            if (connectionStringBuilder.ContainsKey("Data Source") && ((string)connectionStringBuilder["Data Source"]).EndsWith(".sdf", StringComparison.OrdinalIgnoreCase))
                return (string)connectionStringBuilder["Data Source"];
            return this.services.Model.DatabaseName;
        }

        void IProvider.CreateDatabase()
        {
            this.CheckDispose();
            this.CheckInitialized();
            string str1 = (string)null;
            string str2 = (string)null;
            DbConnectionStringBuilder connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = this.conManager.Connection.ConnectionString;
            if (this.conManager.Connection.State == ConnectionState.Closed)
            {
                if (this.mode == SqlProvider.ProviderMode.SqlCE)
                {
                    if (File.Exists(this.dbName))
                        throw Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists((object)this.dbName);
                    Type type1 = this.conManager.Connection.GetType().Module.GetType("System.Data.SqlServerCe.SqlCeEngine");
                    Type type2 = type1;
                    object[] objArray = new object[1];
                    int index = 0;
                    string str3 = connectionStringBuilder.ToString();
                    objArray[index] = (object)str3;
                    object instance = Activator.CreateInstance(type2, objArray);
                    try
                    {
                        type1.InvokeMember("CreateDatabase", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, (Binder)null, instance, new object[0], CultureInfo.InvariantCulture);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                    finally
                    {
                        IDisposable disposable = instance as IDisposable;
                        if (disposable != null)
                            disposable.Dispose();
                    }
                }
                else
                {
                    object obj;
                    if (connectionStringBuilder.TryGetValue("Initial Catalog", out obj))
                    {
                        str1 = obj.ToString();
                        connectionStringBuilder.Remove("Initial Catalog");
                    }
                    if (connectionStringBuilder.TryGetValue("Database", out obj))
                    {
                        str1 = obj.ToString();
                        connectionStringBuilder.Remove("Database");
                    }
                    if (connectionStringBuilder.TryGetValue("AttachDBFileName", out obj))
                    {
                        str2 = obj.ToString();
                        connectionStringBuilder.Remove("AttachDBFileName");
                    }
                }
                this.conManager.Connection.ConnectionString = connectionStringBuilder.ToString();
            }
            else
            {
                if (this.mode == SqlProvider.ProviderMode.SqlCE && File.Exists(this.dbName))
                    throw Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists((object)this.dbName);
                object obj;
                if (connectionStringBuilder.TryGetValue("Initial Catalog", out obj))
                    str1 = obj.ToString();
                if (connectionStringBuilder.TryGetValue("Database", out obj))
                    str1 = obj.ToString();
                if (connectionStringBuilder.TryGetValue("AttachDBFileName", out obj))
                    str2 = obj.ToString();
            }
            if (string.IsNullOrEmpty(str1))
            {
                if (!string.IsNullOrEmpty(str2))
                {
                    str1 = Path.GetFullPath(str2);
                }
                else
                {
                    if (string.IsNullOrEmpty(this.dbName))
                        throw Error.CouldNotDetermineCatalogName();
                    str1 = this.dbName;
                }
            }
            this.conManager.UseConnection((IConnectionUser)this);
            this.conManager.AutoClose = false;
            try
            {
                if (System.Linq.Enumerable.FirstOrDefault<MetaTable>(this.services.Model.GetTables()) == null)
                    throw Error.CreateDatabaseFailedBecauseOfContextWithNoTables((object)this.services.Model.DatabaseName);
                this.deleted = false;
                if (this.mode == SqlProvider.ProviderMode.SqlCE)
                {
                    foreach (MetaTable table in this.services.Model.GetTables())
                    {
                        string createTableCommand = SqlBuilder.GetCreateTableCommand(table);
                        if (!string.IsNullOrEmpty(createTableCommand))
                            this.ExecuteCommand(createTableCommand);
                    }
                    foreach (MetaTable table in this.services.Model.GetTables())
                    {
                        foreach (string command in SqlBuilder.GetCreateForeignKeyCommands(table))
                        {
                            if (!string.IsNullOrEmpty(command))
                                this.ExecuteCommand(command);
                        }
                    }
                }
                else
                {
                    this.ExecuteCommand(SqlBuilder.GetCreateDatabaseCommand(str1, str2, Path.ChangeExtension(str2, ".ldf")));
                    this.conManager.Connection.ChangeDatabase(str1);
                    if (this.mode == SqlProvider.ProviderMode.Sql2005 || this.mode == SqlProvider.ProviderMode.Sql2008)
                    {
                        HashSet<string> hashSet = new HashSet<string>();
                        foreach (MetaTable table in this.services.Model.GetTables())
                        {
                            string schemaForTableCommand = SqlBuilder.GetCreateSchemaForTableCommand(table);
                            if (!string.IsNullOrEmpty(schemaForTableCommand))
                                hashSet.Add(schemaForTableCommand);
                        }
                        foreach (string command in hashSet)
                            this.ExecuteCommand(command);
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (MetaTable table in this.services.Model.GetTables())
                    {
                        string createTableCommand = SqlBuilder.GetCreateTableCommand(table);
                        if (!string.IsNullOrEmpty(createTableCommand))
                            stringBuilder.AppendLine(createTableCommand);
                    }
                    foreach (MetaTable table in this.services.Model.GetTables())
                    {
                        foreach (string str3 in SqlBuilder.GetCreateForeignKeyCommands(table))
                        {
                            if (!string.IsNullOrEmpty(str3))
                                stringBuilder.AppendLine(str3);
                        }
                    }
                    if (stringBuilder.Length <= 0)
                        return;
                    stringBuilder.Insert(0, "SET ARITHABORT ON" + Environment.NewLine);
                    this.ExecuteCommand(stringBuilder.ToString());
                }
            }
            finally
            {
                this.conManager.ReleaseConnection((IConnectionUser)this);
                if (this.conManager.Connection is SqlConnection)
                    SqlConnection.ClearAllPools();
            }
        }

        void IProvider.DeleteDatabase()
        {
            this.CheckDispose();
            this.CheckInitialized();
            if (this.deleted)
                return;
            if (this.mode == SqlProvider.ProviderMode.SqlCE)
            {
                ((IProvider)this).ClearConnection();
                File.Delete(this.dbName);
                this.deleted = true;
            }
            else
            {
                string connectionString = this.conManager.Connection.ConnectionString;
                DbConnection dbConnection = this.conManager.UseConnection((IConnectionUser)this);
                try
                {
                    dbConnection.ChangeDatabase("master");
                    if (dbConnection is SqlConnection)
                        SqlConnection.ClearAllPools();
                    if (this.log != null)
                        this.log.WriteLine(Strings.LogAttemptingToDeleteDatabase((object)this.dbName));
                    this.ExecuteCommand(SqlBuilder.GetDropDatabaseCommand(this.dbName));
                    this.deleted = true;
                }
                finally
                {
                    this.conManager.ReleaseConnection((IConnectionUser)this);
                    if (this.conManager.Connection.State == ConnectionState.Closed && string.Compare(this.conManager.Connection.ConnectionString, connectionString, StringComparison.Ordinal) != 0)
                        this.conManager.Connection.ConnectionString = connectionString;
                }
            }
        }

        bool IProvider.DatabaseExists()
        {
            this.CheckDispose();
            this.CheckInitialized();
            if (this.deleted)
                return false;
            bool flag = false;
            if (this.mode == SqlProvider.ProviderMode.SqlCE)
            {
                flag = File.Exists(this.dbName);
            }
            else
            {
                string connectionString = this.conManager.Connection.ConnectionString;
                try
                {
                    this.conManager.UseConnection((IConnectionUser)this);
                    this.conManager.Connection.ChangeDatabase(this.dbName);
                    this.conManager.ReleaseConnection((IConnectionUser)this);
                    flag = true;
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    if (this.conManager.Connection.State == ConnectionState.Closed && string.Compare(this.conManager.Connection.ConnectionString, connectionString, StringComparison.Ordinal) != 0)
                        this.conManager.Connection.ConnectionString = connectionString;
                }
            }
            return flag;
        }

        void IConnectionUser.CompleteUse()
        {
        }

        void IProvider.ClearConnection()
        {
            this.CheckDispose();
            this.CheckInitialized();
            this.conManager.ClearConnection();
        }

        private void ExecuteCommand(string command)
        {
            if (this.log != null)
            {
                this.log.WriteLine(command);
                this.log.WriteLine();
            }
            DbCommand command1 = this.conManager.Connection.CreateCommand();
            int num = this.commandTimeout;
            command1.CommandTimeout = num;
            DbTransaction transaction = this.conManager.Transaction;
            ((IDbCommand)command1).Transaction = (IDbTransaction)transaction;
            string str = command;
            command1.CommandText = str;
            command1.ExecuteNonQuery();
        }

        ICompiledQuery IProvider.Compile(Expression query)
        {
            this.CheckDispose();
            this.CheckInitialized();
            if (query == null)
                throw Error.ArgumentNull("query");
            this.InitializeProviderMode();
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            SqlProvider.QueryInfo[] queryInfoArray1 = this.BuildQuery(query, annotations);
            this.CheckSqlCompatibility(queryInfoArray1, annotations);
            LambdaExpression lambdaExpression = query as LambdaExpression;
            if (lambdaExpression != null)
                query = lambdaExpression.Body;
            IObjectReaderFactory factory = (IObjectReaderFactory)null;
            ICompiledSubQuery[] subQueries = (ICompiledSubQuery[])null;
            SqlProvider.QueryInfo[] queryInfoArray2 = queryInfoArray1;
            int index = queryInfoArray2.Length - 1;
            SqlProvider.QueryInfo queryInfo = queryInfoArray2[index];
            if (queryInfo.ResultShape == SqlProvider.ResultShape.Singleton)
            {
                subQueries = this.CompileSubQueries(queryInfo.Query);
                factory = this.GetReaderFactory(queryInfo.Query, queryInfo.ResultType);
            }
            else if (queryInfo.ResultShape == SqlProvider.ResultShape.Sequence)
            {
                subQueries = this.CompileSubQueries(queryInfo.Query);
                factory = this.GetReaderFactory(queryInfo.Query, TypeSystem.GetElementType(queryInfo.ResultType));
            }
            return (ICompiledQuery)new SqlProvider.CompiledQuery(this, query, queryInfoArray1, factory, subQueries);
        }

        private ICompiledSubQuery CompileSubQuery(SqlNode query, Type elementType, ReadOnlyCollection<SqlParameter> parameters)
        {
            query = SqlDuplicator.Copy(query);
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            SqlProvider.QueryInfo[] queries = this.BuildQuery(SqlProvider.ResultShape.Sequence, TypeSystem.GetSequenceType(elementType), query, parameters, annotations);
            SqlProvider.QueryInfo queryInfo = queries[0];
            ICompiledSubQuery[] subQueries = this.CompileSubQueries(queryInfo.Query);
            IObjectReaderFactory readerFactory = this.GetReaderFactory(queryInfo.Query, elementType);
            this.CheckSqlCompatibility(queries, annotations);
            return (ICompiledSubQuery)new SqlProvider.CompiledSubQuery(queryInfo, readerFactory, parameters, subQueries);
        }

        IExecuteResult IProvider.Execute(Expression query)
        {
            this.CheckDispose();
            this.CheckInitialized();
            this.CheckNotDeleted();
            if (query == null)
                throw Error.ArgumentNull("query");
            this.InitializeProviderMode();
            query = Funcletizer.Funcletize(query);
            if (this.EnableCacheLookup)
            {
                IExecuteResult cachedResult = this.GetCachedResult(query);
                if (cachedResult != null)
                    return cachedResult;
            }
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            SqlProvider.QueryInfo[] queryInfoArray1 = this.BuildQuery(query, annotations);
            this.CheckSqlCompatibility(queryInfoArray1, annotations);
            LambdaExpression lambdaExpression = query as LambdaExpression;
            if (lambdaExpression != null)
                query = lambdaExpression.Body;
            IObjectReaderFactory factory = (IObjectReaderFactory)null;
            ICompiledSubQuery[] subQueries = (ICompiledSubQuery[])null;
            SqlProvider.QueryInfo[] queryInfoArray2 = queryInfoArray1;
            int index = queryInfoArray2.Length - 1;
            SqlProvider.QueryInfo queryInfo = queryInfoArray2[index];
            if (queryInfo.ResultShape == SqlProvider.ResultShape.Singleton)
            {
                subQueries = this.CompileSubQueries(queryInfo.Query);
                factory = this.GetReaderFactory(queryInfo.Query, queryInfo.ResultType);
            }
            else if (queryInfo.ResultShape == SqlProvider.ResultShape.Sequence)
            {
                subQueries = this.CompileSubQueries(queryInfo.Query);
                factory = this.GetReaderFactory(queryInfo.Query, TypeSystem.GetElementType(queryInfo.ResultType));
            }
            return this.ExecuteAll(query, queryInfoArray1, factory, (object[])null, subQueries);
        }

        private ICompiledSubQuery[] CompileSubQueries(SqlNode query)
        {
            return new SqlProvider.SubQueryCompiler(this).Compile(query);
        }

        private void CheckSqlCompatibility(SqlProvider.QueryInfo[] queries, SqlNodeAnnotations annotations)
        {
            if (this.Mode != SqlProvider.ProviderMode.Sql2000 && this.Mode != SqlProvider.ProviderMode.SqlCE)
                return;
            int index = 0;
            for (int length = queries.Length; index < length; ++index)
                SqlServerCompatibilityCheck.ThrowIfUnsupported(queries[index].Query, annotations, this.Mode);
        }

        private IExecuteResult ExecuteAll(Expression query, SqlProvider.QueryInfo[] queryInfos, IObjectReaderFactory factory, object[] userArguments, ICompiledSubQuery[] subQueries)
        {
            IExecuteResult executeResult = (IExecuteResult)null;
            object lastResult = (object)null;
            int index = 0;
            for (int length = queryInfos.Length; index < length; ++index)
            {
                executeResult = index >= length - 1 ? this.Execute(query, queryInfos[index], factory, (object[])null, userArguments, subQueries, lastResult) : this.Execute(query, queryInfos[index], (IObjectReaderFactory)null, (object[])null, userArguments, subQueries, lastResult);
                if (queryInfos[index].ResultShape == SqlProvider.ResultShape.Return)
                    lastResult = executeResult.ReturnValue;
            }
            return executeResult;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private IExecuteResult GetCachedResult(Expression query)
        {
            object cachedObject = this.services.GetCachedObject(query);
            if (cachedObject != null)
            {
                switch (this.GetResultShape(query))
                {
                    case SqlProvider.ResultShape.Singleton:
                        return (IExecuteResult)new SqlProvider.ExecuteResult((DbCommand)null, (ReadOnlyCollection<SqlParameterInfo>)null, (IObjectReaderSession)null, cachedObject);
                    case SqlProvider.ResultShape.Sequence:
                        // ISSUE: variable of the null type
                        //__Null local1 = null;
                        // ISSUE: variable of the null type
                       // __Null local2 = null;
                        // ISSUE: variable of the null type
                        //__Null local3 = null;
                        Type type1 = typeof(SqlProvider.SequenceOfOne<>);
                        Type[] typeArray = new Type[1];
                        int index1 = 0;
                        Type elementType = TypeSystem.GetElementType(this.GetResultType(query));
                        typeArray[index1] = elementType;
                        Type type2 = type1.MakeGenericType(typeArray);
                        int num = 36;
                        // ISSUE: variable of the null type
                        //__Null local4 = null;
                        object[] args = new object[1];
                        int index2 = 0;
                        object obj = cachedObject;
                        args[index2] = obj;
                        // ISSUE: variable of the null type
                        //__Null local5 = null;
                        object instance = Activator.CreateInstance(type2, (BindingFlags)num, (Binder)null, args, (CultureInfo)null);
                        return (IExecuteResult)new SqlProvider.ExecuteResult((DbCommand)null, (ReadOnlyCollection<SqlParameterInfo>)null, (IObjectReaderSession)null, instance);
                }
            }
            return (IExecuteResult)null;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private IExecuteResult Execute(Expression query, SqlProvider.QueryInfo queryInfo, IObjectReaderFactory factory, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries, object lastResult)
        {
            this.InitializeProviderMode();
            DbConnection dbConnection = this.conManager.UseConnection((IConnectionUser)this);
            try
            {
                DbCommand command = dbConnection.CreateCommand();
                command.CommandText = queryInfo.CommandText;
                command.Transaction = this.conManager.Transaction;
                command.CommandTimeout = this.commandTimeout;
                this.AssignParameters(command, queryInfo.Parameters, userArgs, lastResult);
                this.LogCommand(this.log, command);
                this.queryCount = this.queryCount + 1;
                switch (queryInfo.ResultShape)
                {
                    case SqlProvider.ResultShape.Singleton:
                        DbDataReader reader1 = command.ExecuteReader();
                        IObjectReader objectReader1 = factory.Create(reader1, true, (IReaderProvider)this, parentArgs, userArgs, subQueries);
                        this.conManager.UseConnection((IConnectionUser)objectReader1.Session);
                        try
                        {
                            Type type1 = typeof(SqlProvider.OneTimeEnumerable<>);
                            Type[] typeArray = new Type[1];
                            int index1 = 0;
                            Type resultType = queryInfo.ResultType;
                            typeArray[index1] = resultType;
                            Type type2 = type1.MakeGenericType(typeArray);
                            int num = 52;
                            // ISSUE: variable of the null type
                            //__Null local1 = null;
                            object[] args = new object[1];
                            int index2 = 0;
                            IObjectReader objectReader2 = objectReader1;
                            args[index2] = (object)objectReader2;
                            // ISSUE: variable of the null type
                            //__Null local2 = null;
                            IEnumerable sequence = (IEnumerable)Activator.CreateInstance(type2, (BindingFlags)num, (Binder)null, args, (CultureInfo)null);
                            object obj = (object)null;
                            MethodCallExpression methodCallExpression = query as MethodCallExpression;
                            MethodInfo sequenceMethod;
                            if (methodCallExpression != null && (methodCallExpression.Method.DeclaringType == typeof(Queryable) || methodCallExpression.Method.DeclaringType == typeof(System.Linq.Enumerable)))
                            {
                                string name = methodCallExpression.Method.Name;
                                if (!(name == "First") && !(name == "FirstOrDefault") && !(name == "SingleOrDefault"))
                                {
                                    if (name == "Single")
                                    { }
                                    sequenceMethod = TypeSystem.FindSequenceMethod("Single", sequence);
                                }
                                else
                                    sequenceMethod = TypeSystem.FindSequenceMethod(methodCallExpression.Method.Name, sequence);
                            }
                            else
                                sequenceMethod = TypeSystem.FindSequenceMethod("SingleOrDefault", sequence);
                            if (sequenceMethod != (MethodInfo)null)
                            {
                                try
                                {
                                    MethodInfo methodInfo = sequenceMethod;
                                    // ISSUE: variable of the null type
                                    //__Null local3 = null;
                                    object[] parameters = new object[1];
                                    int index3 = 0;
                                    IEnumerable enumerable = sequence;
                                    parameters[index3] = (object)enumerable;
                                    obj = methodInfo.Invoke((object)null, parameters);
                                }
                                catch (TargetInvocationException ex)
                                {
                                    if (ex.InnerException != null)
                                        throw ex.InnerException;
                                    throw;
                                }
                            }
                            return (IExecuteResult)new SqlProvider.ExecuteResult(command, queryInfo.Parameters, objectReader1.Session, obj);
                        }
                        finally
                        {
                            objectReader1.Dispose();
                        }
                    case SqlProvider.ResultShape.Sequence:
                        DbDataReader reader2 = command.ExecuteReader();
                        IObjectReader objectReader3 = factory.Create(reader2, true, (IReaderProvider)this, parentArgs, userArgs, subQueries);
                        this.conManager.UseConnection((IConnectionUser)objectReader3.Session);
                        Type type3 = typeof(SqlProvider.OneTimeEnumerable<>);
                        Type[] typeArray1 = new Type[1];
                        int index4 = 0;
                        Type elementType1 = TypeSystem.GetElementType(queryInfo.ResultType);
                        typeArray1[index4] = elementType1;
                        Type type4 = type3.MakeGenericType(typeArray1);
                        int num1 = 52;
                        // ISSUE: variable of the null type
                        //__Null local4 = null;
                        object[] args1 = new object[1];
                        int index5 = 0;
                        IObjectReader objectReader4 = objectReader3;
                        args1[index5] = (object)objectReader4;
                        // ISSUE: variable of the null type
                        //__Null local5 = null;
                        IEnumerable source = (IEnumerable)Activator.CreateInstance(type4, (BindingFlags)num1, (Binder)null, args1, (CultureInfo)null);
                        if (typeof(IQueryable).IsAssignableFrom(queryInfo.ResultType))
                            source = (IEnumerable)Queryable.AsQueryable(source);
                        SqlProvider.ExecuteResult executeResult1 = new SqlProvider.ExecuteResult(command, queryInfo.Parameters, objectReader3.Session);
                        MetaFunction function1 = this.GetFunction(query);
                        if (function1 != null && !function1.IsComposable)
                        {
                            Type type1 = typeof(SqlProvider.SingleResult<>);
                            Type[] typeArray2 = new Type[1];
                            int index1 = 0;
                            Type elementType2 = TypeSystem.GetElementType(queryInfo.ResultType);
                            typeArray2[index1] = elementType2;
                            Type type2 = type1.MakeGenericType(typeArray2);
                            int num2 = 52;
                            // ISSUE: variable of the null type
                            //__Null local1 = null;
                            object[] args2 = new object[3];
                            int index2 = 0;
                            IEnumerable enumerable = source;
                            args2[index2] = (object)enumerable;
                            int index3 = 1;
                            SqlProvider.ExecuteResult executeResult2 = executeResult1;
                            args2[index3] = (object)executeResult2;
                            int index6 = 2;
                            DataContext context = this.services.Context;
                            args2[index6] = (object)context;
                            // ISSUE: variable of the null type
                            //__Null local2 = null;
                            source = (IEnumerable)Activator.CreateInstance(type2, (BindingFlags)num2, (Binder)null, args2, (CultureInfo)null);
                        }
                        executeResult1.ReturnValue = (object)source;
                        return (IExecuteResult)executeResult1;
                    case SqlProvider.ResultShape.MultipleResults:
                        IObjectReaderSession session = this.readerCompiler.CreateSession(command.ExecuteReader(), (IReaderProvider)this, parentArgs, userArgs, subQueries);
                        this.conManager.UseConnection((IConnectionUser)session);
                        MetaFunction function2 = this.GetFunction(query);
                        SqlProvider.ExecuteResult executeResult3 = new SqlProvider.ExecuteResult(command, queryInfo.Parameters, session);
                        executeResult3.ReturnValue = (object)new SqlProvider.MultipleResults(this, function2, session, executeResult3);
                        return (IExecuteResult)executeResult3;
                    default:
                        return (IExecuteResult)new SqlProvider.ExecuteResult(command, queryInfo.Parameters, (IObjectReaderSession)null, (object)command.ExecuteNonQuery(), true);
                }
            }
            finally
            {
                this.conManager.ReleaseConnection((IConnectionUser)this);
            }
        }

        private MetaFunction GetFunction(Expression query)
        {
            LambdaExpression lambdaExpression = query as LambdaExpression;
            if (lambdaExpression != null)
                query = lambdaExpression.Body;
            MethodCallExpression methodCallExpression = query as MethodCallExpression;
            if (methodCallExpression != null && typeof(DataContext).IsAssignableFrom(methodCallExpression.Method.DeclaringType))
                return this.services.Model.GetFunction(methodCallExpression.Method);
            return (MetaFunction)null;
        }

        private void LogCommand(TextWriter writer, DbCommand cmd)
        {
            if (writer == null)
                return;
            writer.WriteLine(cmd.CommandText);
            foreach (DbParameter dbParameter in cmd.Parameters)
            {
                int num1 = 0;
                int num2 = 0;
                PropertyInfo property1 = dbParameter.GetType().GetProperty("Precision");
                if (property1 != (PropertyInfo)null)
                    num1 = (int)Convert.ChangeType(property1.GetValue((object)dbParameter, (object[])null), typeof(int), (IFormatProvider)CultureInfo.InvariantCulture);
                PropertyInfo property2 = dbParameter.GetType().GetProperty("Scale");
                if (property2 != (PropertyInfo)null)
                    num2 = (int)Convert.ChangeType(property2.GetValue((object)dbParameter, (object[])null), typeof(int), (IFormatProvider)CultureInfo.InvariantCulture);
                System.Data.SqlClient.SqlParameter sqlParameter = dbParameter as System.Data.SqlClient.SqlParameter;
                TextWriter textWriter = writer;
                string format = "-- {0}: {1} {2} (Size = {3}; Prec = {4}; Scale = {5}) [{6}]";
                object[] objArray = new object[7];
                int index1 = 0;
                string parameterName = dbParameter.ParameterName;
                objArray[index1] = (object)parameterName;
                int index2 = 1;
                // ISSUE: variable of a boxed type
                var local1 = (Enum)dbParameter.Direction;
                objArray[index2] = (object)local1;
                int index3 = 2;
                string str1 = sqlParameter == null ? dbParameter.DbType.ToString() : sqlParameter.SqlDbType.ToString();
                objArray[index3] = (object)str1;
                int index4 = 3;
                string str2 = dbParameter.Size.ToString((IFormatProvider)CultureInfo.CurrentCulture);
                objArray[index4] = (object)str2;
                int index5 = 4;
                // ISSUE: variable of a boxed type
                var local2 = (System.ValueType)num1;
                objArray[index5] = (object)local2;
                int index6 = 5;
                // ISSUE: variable of a boxed type
                var local3 = (System.ValueType)num2;
                objArray[index6] = (object)local3;
                int index7 = 6;
                object obj = sqlParameter == null ? dbParameter.Value : sqlParameter.SqlValue;
                objArray[index7] = obj;
                textWriter.WriteLine(format, objArray);
            }
            TextWriter textWriter1 = writer;
            string format1 = "-- Context: {0}({1}) Model: {2} Build: {3}";
            object[] objArray1 = new object[4];
            int index8 = 0;
            string name1 = this.GetType().Name;
            objArray1[index8] = (object)name1;
            int index9 = 1;
            // ISSUE: variable of a boxed type
            var local = (Enum)this.Mode;
            objArray1[index9] = (object)local;
            int index10 = 2;
            string name2 = this.services.Model.GetType().Name;
            objArray1[index10] = (object)name2;
            int index11 = 3;
            string str = "4.6.79.0";
            objArray1[index11] = (object)str;
            textWriter1.WriteLine(format1, objArray1);
            writer.WriteLine();
        }

        private void AssignParameters(DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parms, object[] userArguments, object lastResult)
        {
            if (parms == null)
                return;
            foreach (SqlParameterInfo sqlParameterInfo in parms)
            {
                DbParameter parameter = cmd.CreateParameter();
                parameter.ParameterName = sqlParameterInfo.Parameter.Name;
                parameter.Direction = sqlParameterInfo.Parameter.Direction;
                if (sqlParameterInfo.Parameter.Direction == ParameterDirection.Input || sqlParameterInfo.Parameter.Direction == ParameterDirection.InputOutput)
                {
                    object obj = sqlParameterInfo.Value;
                    switch (sqlParameterInfo.Type)
                    {
                        case SqlParameterType.UserArgument:
                            try
                            {
                                Delegate accessor = sqlParameterInfo.Accessor;
                                object[] objArray1 = new object[1];
                                int index = 0;
                                object[] objArray2 = userArguments;
                                objArray1[index] = (object)objArray2;
                                obj = accessor.DynamicInvoke(objArray1);
                                break;
                            }
                            catch (TargetInvocationException ex)
                            {
                                throw ex.InnerException;
                            }
                        case SqlParameterType.PreviousResult:
                            obj = lastResult;
                            break;
                    }
                    this.typeProvider.InitializeParameter(sqlParameterInfo.Parameter.SqlType, parameter, obj);
                }
                else
                    this.typeProvider.InitializeParameter(sqlParameterInfo.Parameter.SqlType, parameter, (object)null);
                cmd.Parameters.Add((object)parameter);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        IEnumerable IProvider.Translate(Type elementType, DbDataReader reader)
        {
            this.CheckDispose();
            this.CheckInitialized();
            this.InitializeProviderMode();
            if (elementType == (Type)null)
                throw Error.ArgumentNull("elementType");
            if (reader == null)
                throw Error.ArgumentNull("reader");
            IEnumerator enumerator1 = (IEnumerator)this.GetDefaultFactory(this.services.Model.GetMetaType(elementType)).Create(reader, true, (IReaderProvider)this, (object[])null, (object[])null, (ICompiledSubQuery[])null);
            Type type1 = typeof(SqlProvider.OneTimeEnumerable<>);
            Type[] typeArray = new Type[1];
            int index1 = 0;
            Type type2 = elementType;
            typeArray[index1] = type2;
            Type type3 = type1.MakeGenericType(typeArray);
            int num = 36;
            // ISSUE: variable of the null type
            //var local1 = null;
            object[] args = new object[1];
            int index2 = 0;
            IEnumerator enumerator2 = enumerator1;
            args[index2] = (object)enumerator2;
            // ISSUE: variable of the null type
            //__Null local2 = null;
            return (IEnumerable)Activator.CreateInstance(type3, (BindingFlags)num, (Binder)null, args, (CultureInfo)null);
        }

        IMultipleResults IProvider.Translate(DbDataReader reader)
        {
            this.CheckDispose();
            this.CheckInitialized();
            this.InitializeProviderMode();
            if (reader == null)
                throw Error.ArgumentNull("reader");
            return (IMultipleResults)new SqlProvider.MultipleResults(this, (MetaFunction)null, this.readerCompiler.CreateSession(reader, (IReaderProvider)this, (object[])null, (object[])null, (ICompiledSubQuery[])null), (SqlProvider.ExecuteResult)null);
        }

        string IProvider.GetQueryText(Expression query)
        {
            this.CheckDispose();
            this.CheckInitialized();
            if (query == null)
                throw Error.ArgumentNull("query");
            this.InitializeProviderMode();
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            SqlProvider.QueryInfo[] queryInfoArray = this.BuildQuery(query, annotations);
            StringBuilder stringBuilder = new StringBuilder();
            int index = 0;
            for (int length = queryInfoArray.Length; index < length; ++index)
            {
                SqlProvider.QueryInfo queryInfo = queryInfoArray[index];
                stringBuilder.Append(queryInfo.CommandText);
                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString();
        }

        DbCommand IProvider.GetCommand(Expression query)
        {
            this.CheckDispose();
            this.CheckInitialized();
            if (query == null)
                throw Error.ArgumentNull("query");
            this.InitializeProviderMode();
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            SqlProvider.QueryInfo[] queryInfoArray = this.BuildQuery(query, annotations);
            int index = queryInfoArray.Length - 1;
            SqlProvider.QueryInfo queryInfo = queryInfoArray[index];
            DbCommand command = this.conManager.Connection.CreateCommand();
            command.CommandText = queryInfo.CommandText;
            command.Transaction = this.conManager.Transaction;
            command.CommandTimeout = this.commandTimeout;
            this.AssignParameters(command, queryInfo.Parameters, (object[])null, (object)null);
            return command;
        }

        private SqlProvider.ResultShape GetResultShape(Expression query)
        {
            LambdaExpression lambdaExpression = query as LambdaExpression;
            if (lambdaExpression != null)
                query = lambdaExpression.Body;
            if (query.Type == typeof(void))
                return SqlProvider.ResultShape.Return;
            if (query.Type == typeof(IMultipleResults))
                return SqlProvider.ResultShape.MultipleResults;
            bool flag1 = typeof(IEnumerable).IsAssignableFrom(query.Type);
            ProviderType providerType = this.typeProvider.From(query.Type);
            bool flag2 = !providerType.IsRuntimeOnlyType && !providerType.IsApplicationType;
            bool flag3 = flag2 || !flag1;
            MethodCallExpression methodCallExpression = query as MethodCallExpression;
            if (methodCallExpression != null)
            {
                if (methodCallExpression.Method.DeclaringType == typeof(Queryable) || methodCallExpression.Method.DeclaringType == typeof(System.Linq.Enumerable))
                {
                    string name = methodCallExpression.Method.Name;
                    if (name == "First" || name == "FirstOrDefault" || (name == "Single" || name == "SingleOrDefault"))
                        flag3 = true;
                }
                else if (methodCallExpression.Method.DeclaringType == typeof(DataContext))
                {
                    if (methodCallExpression.Method.Name == "ExecuteCommand")
                        return SqlProvider.ResultShape.Return;
                }
                else if (methodCallExpression.Method.DeclaringType.IsSubclassOf(typeof(DataContext)))
                {
                    MetaFunction function = this.GetFunction(query);
                    if (function != null)
                    {
                        if (!function.IsComposable)
                            flag3 = false;
                        else if (flag2)
                            flag3 = true;
                    }
                }
                else if (methodCallExpression.Method.DeclaringType == typeof(DataManipulation) && methodCallExpression.Method.ReturnType == typeof(int))
                    return SqlProvider.ResultShape.Return;
            }
            if (flag3)
                return SqlProvider.ResultShape.Singleton;
            return flag2 ? SqlProvider.ResultShape.Return : SqlProvider.ResultShape.Sequence;
        }

        private Type GetResultType(Expression query)
        {
            LambdaExpression lambdaExpression = query as LambdaExpression;
            if (lambdaExpression != null)
                query = lambdaExpression.Body;
            return query.Type;
        }

        internal SqlProvider.QueryInfo[] BuildQuery(Expression query, SqlNodeAnnotations annotations)
        {
            this.CheckDispose();
            query = Funcletizer.Funcletize(query);
            QueryConverter queryConverter = new QueryConverter(this.services, this.typeProvider, this.translator, this.sqlFactory);
            switch (this.Mode)
            {
                case SqlProvider.ProviderMode.Sql2000:
                    queryConverter.ConverterStrategy = ConverterStrategy.CanUseScopeIdentity | ConverterStrategy.CanUseRowStatus | ConverterStrategy.CanUseJoinOn;
                    break;
                case SqlProvider.ProviderMode.Sql2005:
                case SqlProvider.ProviderMode.Sql2008:
                    queryConverter.ConverterStrategy = ConverterStrategy.SkipWithRowNumber | ConverterStrategy.CanUseScopeIdentity | ConverterStrategy.CanUseOuterApply | ConverterStrategy.CanUseRowStatus | ConverterStrategy.CanUseJoinOn | ConverterStrategy.CanOutputFromInsert;
                    break;
                case SqlProvider.ProviderMode.SqlCE:
                    queryConverter.ConverterStrategy = ConverterStrategy.CanUseOuterApply;
                    break;
            }
            SqlNode node = queryConverter.ConvertOuter(query);
            return this.BuildQuery(this.GetResultShape(query), this.GetResultType(query), node, (ReadOnlyCollection<SqlParameter>)null, annotations);
        }

        private SqlProvider.QueryInfo[] BuildQuery(SqlProvider.ResultShape resultShape, Type resultType, SqlNode node, ReadOnlyCollection<SqlParameter> parentParameters, SqlNodeAnnotations annotations)
        {
            SqlSupersetValidator supersetValidator = new SqlSupersetValidator();
            if (this.checkQueries)
            {
                supersetValidator.AddValidator((SqlVisitor)new ColumnTypeValidator());
                supersetValidator.AddValidator((SqlVisitor)new LiteralValidator());
            }
            supersetValidator.Validate(node);
            SqlColumnizer columnizer = new SqlColumnizer();
            bool canUseOuterApply = this.Mode == SqlProvider.ProviderMode.Sql2005 || this.Mode == SqlProvider.ProviderMode.Sql2008 || this.Mode == SqlProvider.ProviderMode.SqlCE;
            SqlBinder sqlBinder = new SqlBinder(this.translator, this.sqlFactory, this.services.Model, this.services.Context.LoadOptions, columnizer, canUseOuterApply);
            int num1 = (uint)(this.optimizationFlags & OptimizationFlags.OptimizeLinkExpansions) > 0U ? 1 : 0;
            sqlBinder.OptimizeLinkExpansions = num1 != 0;
            int num2 = (uint)(this.optimizationFlags & OptimizationFlags.SimplifyCaseStatements) > 0U ? 1 : 0;
            sqlBinder.SimplifyCaseStatements = num2 != 0;
            Func<SqlNode, SqlNode> func = (Func<SqlNode, SqlNode>)(n => PreBindDotNetConverter.Convert(n, this.sqlFactory, this.services.Model));
            sqlBinder.PreBinder = func;
            SqlNode node1 = node;
            node = sqlBinder.Bind(node1);
            if (this.checkQueries)
            {
                supersetValidator.AddValidator((SqlVisitor)new ExpectNoAliasRefs());
                supersetValidator.AddValidator((SqlVisitor)new ExpectNoSharedExpressions());
            }
            supersetValidator.Validate(node);
            node = PostBindDotNetConverter.Convert(node, this.sqlFactory, this.Mode);
            SqlRetyper sqlRetyper = new SqlRetyper(this.typeProvider, this.services.Model);
            SqlNode node2 = node;
            node = sqlRetyper.Retype(node2);
            supersetValidator.Validate(node);
            node = new SqlTypeConverter(this.sqlFactory).Visit(node);
            supersetValidator.Validate(node);
            node = new SqlMethodTransformer(this.sqlFactory).Visit(node);
            supersetValidator.Validate(node);
            node = new SqlMultiplexer(this.Mode == SqlProvider.ProviderMode.Sql2008 || this.Mode == SqlProvider.ProviderMode.Sql2005 || this.Mode == SqlProvider.ProviderMode.SqlCE ? SqlMultiplexer.Options.EnableBigJoin : SqlMultiplexer.Options.None, (IEnumerable<SqlParameter>)parentParameters, this.sqlFactory).Multiplex(node);
            supersetValidator.Validate(node);
            node = new SqlFlattener(this.sqlFactory, columnizer).Flatten(node);
            supersetValidator.Validate(node);
            if (this.mode == SqlProvider.ProviderMode.SqlCE)
                node = new SqlRewriteScalarSubqueries(this.sqlFactory).Rewrite(node);
            node = SqlCaseSimplifier.Simplify(node, this.sqlFactory);
            node = new SqlReorderer(this.typeProvider, this.sqlFactory).Reorder(node);
            supersetValidator.Validate(node);
            node = SqlBooleanizer.Rationalize(node, this.typeProvider, this.services.Model);
            if (this.checkQueries)
                supersetValidator.AddValidator((SqlVisitor)new ExpectRationalizedBooleans());
            supersetValidator.Validate(node);
            if (this.checkQueries)
                supersetValidator.AddValidator((SqlVisitor)new ExpectNoFloatingColumns());
            SqlNode node3 = node;
            node = sqlRetyper.Retype(node3);
            supersetValidator.Validate(node);
            SqlAliaser sqlAliaser = new SqlAliaser();
            SqlNode node4 = node;
            node = sqlAliaser.AssociateColumnsWithAliases(node4);
            supersetValidator.Validate(node);
            node = SqlLiftWhereClauses.Lift(node, this.typeProvider, this.services.Model);
            node = SqlLiftIndependentRowExpressions.Lift(node);
            node = SqlOuterApplyReducer.Reduce(node, this.sqlFactory, annotations);
            node = SqlTopReducer.Reduce(node, annotations, this.sqlFactory);
            node = new SqlResolver().Resolve(node);
            supersetValidator.Validate(node);
            SqlNode node5 = node;
            node = sqlAliaser.AssociateColumnsWithAliases(node5);
            supersetValidator.Validate(node);
            node = SqlUnionizer.Unionize(node);
            node = SqlRemoveConstantOrderBy.Remove(node);
            node = new SqlDeflator().Deflate(node);
            supersetValidator.Validate(node);
            node = SqlCrossApplyToCrossJoin.Reduce(node, annotations);
            node = new SqlNamer().AssignNames(node);
            supersetValidator.Validate(node);
            node = new LongTypeConverter(this.sqlFactory).AddConversions(node, annotations);
            supersetValidator.AddValidator((SqlVisitor)new ExpectNoMethodCalls());
            supersetValidator.AddValidator((SqlVisitor)new ValidateNoInvalidComparison());
            supersetValidator.Validate(node);
            SqlParameterizer sqlParameterizer = new SqlParameterizer(this.typeProvider, annotations);
            SqlFormatter sqlFormatter = new SqlFormatter();
            if (this.mode == SqlProvider.ProviderMode.SqlCE || this.mode == SqlProvider.ProviderMode.Sql2005 || this.mode == SqlProvider.ProviderMode.Sql2008)
                sqlFormatter.ParenthesizeTop = true;
            SqlBlock block = node as SqlBlock;
            if (block != null && this.mode == SqlProvider.ProviderMode.SqlCE)
            {
                ReadOnlyCollection<ReadOnlyCollection<SqlParameterInfo>> readOnlyCollection = sqlParameterizer.ParameterizeBlock(block);
                string[] strArray = sqlFormatter.FormatBlock(block, false);
                SqlProvider.QueryInfo[] queryInfoArray = new SqlProvider.QueryInfo[strArray.Length];
                int index = 0;
                for (int length = strArray.Length; index < length; ++index)
                    queryInfoArray[index] = new SqlProvider.QueryInfo((SqlNode)block.Statements[index], strArray[index], readOnlyCollection[index], index < length - 1 ? SqlProvider.ResultShape.Return : resultShape, index < length - 1 ? typeof(int) : resultType);
                return queryInfoArray;
            }
            ReadOnlyCollection<SqlParameterInfo> parameters = sqlParameterizer.Parameterize(node);
            string commandText = sqlFormatter.Format(node);
            SqlProvider.QueryInfo[] queryInfoArray1 = new SqlProvider.QueryInfo[1];
            int index1 = 0;
            SqlProvider.QueryInfo queryInfo = new SqlProvider.QueryInfo(node, commandText, parameters, resultShape, resultType);
            queryInfoArray1[index1] = queryInfo;
            return queryInfoArray1;
        }

        private SqlSelect GetFinalSelect(SqlNode node)
        {
            switch (node.NodeType)
            {
                case SqlNodeType.Block:
                    SqlBlock sqlBlock = (SqlBlock)node;
                    return this.GetFinalSelect((SqlNode)sqlBlock.Statements[sqlBlock.Statements.Count - 1]);
                case SqlNodeType.Select:
                    return (SqlSelect)node;
                default:
                    return (SqlSelect)null;
            }
        }

        private IObjectReaderFactory GetReaderFactory(SqlNode node, Type elemType)
        {
            SqlSelect sqlSelect = node as SqlSelect;
            SqlExpression expression = (SqlExpression)null;
            if (sqlSelect == null && node.NodeType == SqlNodeType.Block)
                sqlSelect = this.GetFinalSelect(node);
            if (sqlSelect != null)
            {
                expression = sqlSelect.Selection;
            }
            else
            {
                SqlUserQuery sqlUserQuery = node as SqlUserQuery;
                if (sqlUserQuery != null && sqlUserQuery.Projection != null)
                    expression = sqlUserQuery.Projection;
            }
            if (expression != null)
                return this.readerCompiler.Compile(expression, elemType);
            return this.GetDefaultFactory(this.services.Model.GetMetaType(elemType));
        }

        private IObjectReaderFactory GetDefaultFactory(MetaType rowType)
        {
            if (rowType == null)
                throw Error.ArgumentNull("rowType");
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            Expression source = (Expression)Expression.Constant((object)null);
            SqlUserQuery query = new SqlUserQuery(string.Empty, (SqlExpression)null, (IEnumerable<SqlExpression>)null, source);
            if (TypeSystem.IsSimpleType(rowType.Type))
            {
                SqlUserColumn sqlUserColumn = new SqlUserColumn(rowType.Type, this.typeProvider.From(rowType.Type), query, "", false, query.SourceExpression);
                query.Columns.Add(sqlUserColumn);
                query.Projection = (SqlExpression)sqlUserColumn;
            }
            else
            {
                SqlUserRow sqlUserRow = new SqlUserRow(rowType.InheritanceRoot, this.typeProvider.GetApplicationType(0), query, source);
                query.Projection = this.translator.BuildProjection((SqlExpression)sqlUserRow, rowType, true, (SqlLink)null, source);
            }
            SqlProvider.QueryInfo[] queryInfoArray = this.BuildQuery(SqlProvider.ResultShape.Sequence, TypeSystem.GetSequenceType(rowType.Type), (SqlNode)query, (ReadOnlyCollection<SqlParameter>)null, annotations);
            int index = queryInfoArray.Length - 1;
            return this.GetReaderFactory(queryInfoArray[index].Query, rowType.Type);
        }

        internal enum ProviderMode
        {
            NotYetDecided,
            Sql2000,
            Sql2005,
            Sql2008,
            SqlCE,
        }

        private class SubQueryCompiler : SqlVisitor
        {
            private SqlProvider provider;
            private List<ICompiledSubQuery> subQueries;

            internal SubQueryCompiler(SqlProvider provider)
            {
                this.provider = provider;
            }

            internal ICompiledSubQuery[] Compile(SqlNode node)
            {
                this.subQueries = new List<ICompiledSubQuery>();
                this.Visit(node);
                return this.subQueries.ToArray();
            }

            internal override SqlSelect VisitSelect(SqlSelect select)
            {
                this.Visit((SqlNode)select.Selection);
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss)
            {
                return (SqlExpression)ss;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq)
            {
                Type elementType = cq.Query.NodeType == SqlNodeType.Multiset ? TypeSystem.GetElementType(cq.ClrType) : cq.ClrType;
                ICompiledSubQuery compiledSubQuery = this.provider.CompileSubQuery((SqlNode)cq.Query.Select, elementType, cq.Parameters.AsReadOnly());
                cq.Ordinal = this.subQueries.Count;
                this.subQueries.Add(compiledSubQuery);
                return (SqlExpression)cq;
            }
        }

        internal class QueryInfo
        {
            private SqlNode query;
            private string commandText;
            private ReadOnlyCollection<SqlParameterInfo> parameters;
            private SqlProvider.ResultShape resultShape;
            private Type resultType;

            internal SqlNode Query
            {
                get
                {
                    return this.query;
                }
            }

            internal string CommandText
            {
                get
                {
                    return this.commandText;
                }
            }

            internal ReadOnlyCollection<SqlParameterInfo> Parameters
            {
                get
                {
                    return this.parameters;
                }
            }

            internal SqlProvider.ResultShape ResultShape
            {
                get
                {
                    return this.resultShape;
                }
            }

            internal Type ResultType
            {
                get
                {
                    return this.resultType;
                }
            }

            internal QueryInfo(SqlNode query, string commandText, ReadOnlyCollection<SqlParameterInfo> parameters, SqlProvider.ResultShape resultShape, Type resultType)
            {
                this.query = query;
                this.commandText = commandText;
                this.parameters = parameters;
                this.resultShape = resultShape;
                this.resultType = resultType;
            }
        }

        internal enum ResultShape
        {
            Return,
            Singleton,
            Sequence,
            MultipleResults,
        }

        private class CompiledQuery : ICompiledQuery
        {
            private DataLoadOptions originalShape;
            private Expression query;
            private SqlProvider.QueryInfo[] queryInfos;
            private IObjectReaderFactory factory;
            private ICompiledSubQuery[] subQueries;

            internal CompiledQuery(SqlProvider provider, Expression query, SqlProvider.QueryInfo[] queryInfos, IObjectReaderFactory factory, ICompiledSubQuery[] subQueries)
            {
                this.originalShape = provider.services.Context.LoadOptions;
                this.query = query;
                this.queryInfos = queryInfos;
                this.factory = factory;
                this.subQueries = subQueries;
            }

            public IExecuteResult Execute(IProvider provider, object[] arguments)
            {
                if (provider == null)
                    throw Error.ArgumentNull("provider");
                SqlProvider sqlProvider = provider as SqlProvider;
                if (sqlProvider == null)
                    throw Error.ArgumentTypeMismatch((object)"provider");
                if (!SqlProvider.CompiledQuery.AreEquivalentShapes(this.originalShape, sqlProvider.services.Context.LoadOptions))
                    throw Error.CompiledQueryAgainstMultipleShapesNotSupported();
                return sqlProvider.ExecuteAll(this.query, this.queryInfos, this.factory, arguments, this.subQueries);
            }

            private static bool AreEquivalentShapes(DataLoadOptions shape1, DataLoadOptions shape2)
            {
                if (shape1 == shape2)
                    return true;
                if (shape1 == null)
                    return shape2.IsEmpty;
                if (shape2 == null)
                    return shape1.IsEmpty;
                return shape1.IsEmpty && shape2.IsEmpty;
            }
        }

        private class CompiledSubQuery : ICompiledSubQuery
        {
            private SqlProvider.QueryInfo queryInfo;
            private IObjectReaderFactory factory;
            private ReadOnlyCollection<SqlParameter> parameters;
            private ICompiledSubQuery[] subQueries;

            internal CompiledSubQuery(SqlProvider.QueryInfo queryInfo, IObjectReaderFactory factory, ReadOnlyCollection<SqlParameter> parameters, ICompiledSubQuery[] subQueries)
            {
                this.queryInfo = queryInfo;
                this.factory = factory;
                this.parameters = parameters;
                this.subQueries = subQueries;
            }

            public IExecuteResult Execute(IProvider provider, object[] parentArgs, object[] userArgs)
            {
                if (parentArgs == null && this.parameters != null && this.parameters.Count != 0)
                    throw Error.ArgumentNull("arguments");
                SqlProvider sqlProvider = provider as SqlProvider;
                if (sqlProvider == null)
                    throw Error.ArgumentTypeMismatch((object)"provider");
                List<SqlParameterInfo> list = new List<SqlParameterInfo>((IEnumerable<SqlParameterInfo>)this.queryInfo.Parameters);
                int index = 0;
                for (int count = this.parameters.Count; index < count; ++index)
                    list.Add(new SqlParameterInfo(this.parameters[index], parentArgs[index]));
                SqlProvider.QueryInfo queryInfo = new SqlProvider.QueryInfo(this.queryInfo.Query, this.queryInfo.CommandText, list.AsReadOnly(), this.queryInfo.ResultShape, this.queryInfo.ResultType);
                return sqlProvider.Execute((Expression)null, queryInfo, this.factory, parentArgs, userArgs, this.subQueries, (object)null);
            }
        }

        private class ExecuteResult : IExecuteResult, IDisposable
        {
            private int iReturnParameter = -1;
            private DbCommand command;
            private ReadOnlyCollection<SqlParameterInfo> parameters;
            private IObjectReaderSession session;
            private object value;
            private bool useReturnValue;
            private bool isDisposed;

            public object ReturnValue
            {
                get
                {
                    if (this.iReturnParameter >= 0)
                        return this.GetParameterValue(this.iReturnParameter);
                    return this.value;
                }
                internal set
                {
                    this.value = value;
                }
            }

            internal ExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session, object value, bool useReturnValue)
              : this(command, parameters, session)
            {
                this.value = value;
                this.useReturnValue = useReturnValue;
                if (((this.command == null ? 0 : (this.parameters != null ? 1 : 0)) & (useReturnValue ? 1 : 0)) == 0)
                    return;
                this.iReturnParameter = this.GetParameterIndex("@RETURN_VALUE");
            }

            internal ExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session)
            {
                this.command = command;
                this.parameters = parameters;
                this.session = session;
            }

            internal ExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session, object value)
              : this(command, parameters, session, value, false)
            {
            }

            private int GetParameterIndex(string paramName)
            {
                int num = -1;
                int index = 0;
                for (int count = this.parameters.Count; index < count; ++index)
                {
                    if (string.Compare(this.parameters[index].Parameter.Name, paramName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        num = index;
                        break;
                    }
                }
                return num;
            }

            internal object GetParameterValue(string paramName)
            {
                int parameterIndex = this.GetParameterIndex(paramName);
                if (parameterIndex >= 0)
                    return this.GetParameterValue(parameterIndex);
                return (object)null;
            }

            public object GetParameterValue(int parameterIndex)
            {
                if (this.parameters == null || parameterIndex < 0 || parameterIndex > this.parameters.Count)
                    throw Error.ArgumentOutOfRange("parameterIndex");
                if (this.session != null && !this.session.IsBuffered)
                    this.session.Buffer();
                SqlParameterInfo sqlParameterInfo = this.parameters[parameterIndex];
                object obj = this.command.Parameters[parameterIndex].Value;
                if (obj == DBNull.Value)
                    obj = (object)null;
                if (obj != null && obj.GetType() != sqlParameterInfo.Parameter.ClrType)
                    return DBConvert.ChangeType(obj, sqlParameterInfo.Parameter.ClrType);
                return obj;
            }

            public void Dispose()
            {
                if (this.isDisposed)
                    return;
                GC.SuppressFinalize((object)this);
                this.isDisposed = true;
                if (this.session == null)
                    return;
                this.session.Dispose();
            }
        }

        private class SequenceOfOne<T> : IEnumerable<T>, IEnumerable
        {
            private T[] sequence;

            internal SequenceOfOne(T value)
            {
                T[] objArray = new T[1];
                int index = 0;
                T obj = value;
                objArray[index] = obj;
                this.sequence = objArray;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ((IEnumerable<T>)this.sequence).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator)this.GetEnumerator();
            }
        }

        private class OneTimeEnumerable<T> : IEnumerable<T>, IEnumerable
        {
            private IEnumerator<T> enumerator;

            internal OneTimeEnumerable(IEnumerator<T> enumerator)
            {
                this.enumerator = enumerator;
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (this.enumerator == null)
                    throw Error.CannotEnumerateResultsMoreThanOnce();
                IEnumerator<T> enumerator = this.enumerator;
                this.enumerator = (IEnumerator<T>)null;
                return enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator)this.GetEnumerator();
            }
        }

        private class SingleResult<T> : ISingleResult<T>, IEnumerable<T>, IEnumerable, IFunctionResult, IDisposable, IListSource
        {
            private IEnumerable<T> enumerable;
            private SqlProvider.ExecuteResult executeResult;
            private DataContext context;
            private IBindingList cachedList;

            public object ReturnValue
            {
                get
                {
                    return this.executeResult.GetParameterValue("@RETURN_VALUE");
                }
            }

            bool IListSource.ContainsListCollection
            {
                get
                {
                    return false;
                }
            }

            internal SingleResult(IEnumerable<T> enumerable, SqlProvider.ExecuteResult executeResult, DataContext context)
            {
                this.enumerable = enumerable;
                this.executeResult = executeResult;
                this.context = context;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this.enumerable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator)this.GetEnumerator();
            }

            public void Dispose()
            {
                GC.SuppressFinalize((object)this);
                this.executeResult.Dispose();
            }

            IList IListSource.GetList()
            {
                if (this.cachedList == null)
                    this.cachedList = BindingList.Create<T>(this.context, (IEnumerable<T>)this);
                return (IList)this.cachedList;
            }
        }

        private class MultipleResults : IMultipleResults, IFunctionResult, IDisposable
        {
            private SqlProvider provider;
            private MetaFunction function;
            private IObjectReaderSession session;
            private bool isDisposed;
            private SqlProvider.ExecuteResult executeResult;

            public object ReturnValue
            {
                get
                {
                    if (this.executeResult != null)
                        return this.executeResult.GetParameterValue("@RETURN_VALUE");
                    return (object)null;
                }
            }

            internal MultipleResults(SqlProvider provider, MetaFunction function, IObjectReaderSession session, SqlProvider.ExecuteResult executeResult)
            {
                this.provider = provider;
                this.function = function;
                this.session = session;
                this.executeResult = executeResult;
            }

            public IEnumerable<T> GetResult<T>()
            {
                MetaType rowType = (MetaType)null;
                if (this.function != null)
                {
                    foreach (MetaType metaType in this.function.ResultRowTypes)
                    {
                        rowType = System.Linq.Enumerable.SingleOrDefault<MetaType>((IEnumerable<MetaType>)metaType.InheritanceTypes, (Func<MetaType, bool>)(it => it.Type == typeof(T)));
                        if (rowType != null)
                            break;
                    }
                }
                if (rowType == null)
                    rowType = this.provider.services.Model.GetMetaType(typeof(T));
                IObjectReader nextResult = this.provider.GetDefaultFactory(rowType).GetNextResult(this.session, false);
                if (nextResult != null)
                    return (IEnumerable<T>)new SqlProvider.SingleResult<T>((IEnumerable<T>)new SqlProvider.OneTimeEnumerable<T>((IEnumerator<T>)nextResult), this.executeResult, this.provider.services.Context);
                this.Dispose();
                return (IEnumerable<T>)null;
            }

            public void Dispose()
            {
                if (this.isDisposed)
                    return;
                GC.SuppressFinalize((object)this);
                this.isDisposed = true;
                if (this.executeResult != null)
                    this.executeResult.Dispose();
                else
                    this.session.Dispose();
            }
        }
    }

    internal class ValidateNoInvalidComparison : SqlVisitor
    {
        internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
        {
            if ((bo.NodeType == SqlNodeType.EQ || bo.NodeType == SqlNodeType.NE || (bo.NodeType == SqlNodeType.EQ2V || bo.NodeType == SqlNodeType.NE2V) || (bo.NodeType == SqlNodeType.GT || bo.NodeType == SqlNodeType.GE || (bo.NodeType == SqlNodeType.LT || bo.NodeType == SqlNodeType.LE))) && (!bo.Left.SqlType.SupportsComparison || !bo.Right.SqlType.SupportsComparison))
                throw Error.UnhandledStringTypeComparison();
            bo.Left = this.VisitExpression(bo.Left);
            bo.Right = this.VisitExpression(bo.Right);
            return (SqlExpression)bo;
        }
    }

    internal class ExpectNoMethodCalls : SqlVisitor
    {
        internal override SqlExpression VisitMethodCall(SqlMethodCall mc)
        {
            throw Error.MethodHasNoSupportConversionToSql((object)mc.Method.Name);
        }

        internal override SqlSelect VisitSelect(SqlSelect select)
        {
            return this.VisitSelectCore(select);
        }
    }

    internal class ExpectNoFloatingColumns : SqlVisitor
    {
        internal override SqlRow VisitRow(SqlRow row)
        {
            foreach (SqlColumn sqlColumn in row.Columns)
                this.Visit((SqlNode)sqlColumn.Expression);
            return row;
        }

        internal override SqlTable VisitTable(SqlTable tab)
        {
            foreach (SqlColumn sqlColumn in tab.Columns)
                this.Visit((SqlNode)sqlColumn.Expression);
            return tab;
        }

        internal override SqlExpression VisitColumn(SqlColumn col)
        {
            throw Error.UnexpectedFloatingColumn();
        }
    }

    internal class ExpectRationalizedBooleans : SqlBooleanMismatchVisitor
    {
        internal ExpectRationalizedBooleans()
        {
        }

        internal override SqlExpression ConvertValueToPredicate(SqlExpression bitExpression)
        {
            throw Error.ExpectedPredicateFoundBit();
        }

        internal override SqlExpression ConvertPredicateToValue(SqlExpression predicateExpression)
        {
            throw Error.ExpectedBitFoundPredicate();
        }
    }

    internal class ExpectNoSharedExpressions : SqlVisitor
    {
        internal override SqlExpression VisitSharedExpression(SqlSharedExpression shared)
        {
            throw Error.UnexpectedSharedExpression();
        }

        internal override SqlExpression VisitSharedExpressionRef(SqlSharedExpressionRef sref)
        {
            throw Error.UnexpectedSharedExpressionReference();
        }
    }

    internal class ExpectNoAliasRefs : SqlVisitor
    {
        internal override SqlExpression VisitAliasRef(SqlAliasRef aref)
        {
            throw Error.UnexpectedNode((object)aref.NodeType);
        }
    }

    internal class SqlColumnizer
    {
        private SqlColumnizer.ColumnNominator nominator;
        private SqlColumnizer.ColumnDeclarer declarer;

        internal SqlColumnizer()
        {
            this.nominator = new SqlColumnizer.ColumnNominator();
            this.declarer = new SqlColumnizer.ColumnDeclarer();
        }

        internal SqlExpression ColumnizeSelection(SqlExpression selection)
        {
            return this.declarer.Declare(selection, this.nominator.Nominate(selection));
        }

        internal static bool CanBeColumn(SqlExpression expression)
        {
            return SqlColumnizer.ColumnNominator.CanBeColumn(expression);
        }

        private class ColumnDeclarer : SqlVisitor
        {
            private HashSet<SqlExpression> candidates;

            internal ColumnDeclarer()
            {
            }

            internal SqlExpression Declare(SqlExpression expression, HashSet<SqlExpression> candidates)
            {
                this.candidates = candidates;
                return (SqlExpression)this.Visit((SqlNode)expression);
            }

            internal override SqlNode Visit(SqlNode node)
            {
                SqlExpression sqlExpression = node as SqlExpression;
                if (sqlExpression == null || !this.candidates.Contains(sqlExpression))
                    return base.Visit(node);
                if (sqlExpression.NodeType == SqlNodeType.Column || sqlExpression.NodeType == SqlNodeType.ColumnRef)
                    return (SqlNode)sqlExpression;
                Type clrType = sqlExpression.ClrType;
                ProviderType sqlType = sqlExpression.SqlType;
                // ISSUE: variable of the null type
                //__Null local1 = null;
                // ISSUE: variable of the null type
                //__Null local2 = null;
                SqlExpression expr = sqlExpression;
                Expression sourceExpression = expr.SourceExpression;
                return (SqlNode)new SqlColumn(clrType, sqlType, (string)null, (MetaDataMember)null, expr, sourceExpression);
            }
        }

        private class ColumnNominator : SqlVisitor
        {
            private bool isBlocked;
            private HashSet<SqlExpression> candidates;

            internal HashSet<SqlExpression> Nominate(SqlExpression expression)
            {
                this.candidates = new HashSet<SqlExpression>();
                this.isBlocked = false;
                this.Visit((SqlNode)expression);
                return this.candidates;
            }

            internal override SqlNode Visit(SqlNode node)
            {
                SqlExpression sqlExpression = node as SqlExpression;
                if (sqlExpression != null)
                {
                    bool flag = this.isBlocked;
                    this.isBlocked = false;
                    if (SqlColumnizer.ColumnNominator.CanRecurseColumnize(sqlExpression))
                        base.Visit((SqlNode)sqlExpression);
                    if (!this.isBlocked)
                    {
                        if (SqlColumnizer.ColumnNominator.CanBeColumn(sqlExpression))
                            this.candidates.Add(sqlExpression);
                        else
                            this.isBlocked = true;
                    }
                    this.isBlocked = this.isBlocked | flag;
                }
                return node;
            }

            internal override SqlExpression VisitSimpleCase(SqlSimpleCase c)
            {
                c.Expression = this.VisitExpression(c.Expression);
                int index = 0;
                for (int count = c.Whens.Count; index < count; ++index)
                    c.Whens[index].Value = this.VisitExpression(c.Whens[index].Value);
                return (SqlExpression)c;
            }

            internal override SqlExpression VisitTypeCase(SqlTypeCase tc)
            {
                tc.Discriminator = this.VisitExpression(tc.Discriminator);
                int index = 0;
                for (int count = tc.Whens.Count; index < count; ++index)
                    tc.Whens[index].TypeBinding = this.VisitExpression(tc.Whens[index].TypeBinding);
                return (SqlExpression)tc;
            }

            internal override SqlExpression VisitClientCase(SqlClientCase c)
            {
                c.Expression = this.VisitExpression(c.Expression);
                int index = 0;
                for (int count = c.Whens.Count; index < count; ++index)
                    c.Whens[index].Value = this.VisitExpression(c.Whens[index].Value);
                return (SqlExpression)c;
            }

            private static bool CanRecurseColumnize(SqlExpression expr)
            {
                switch (expr.NodeType)
                {
                    case SqlNodeType.Nop:
                    case SqlNodeType.ScalarSubSelect:
                    case SqlNodeType.Select:
                    case SqlNodeType.SharedExpressionRef:
                    case SqlNodeType.Value:
                    case SqlNodeType.Link:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.Element:
                    case SqlNodeType.Exists:
                    case SqlNodeType.AliasRef:
                    case SqlNodeType.ClientQuery:
                    case SqlNodeType.Column:
                    case SqlNodeType.ColumnRef:
                        return false;
                    default:
                        return true;
                }
            }

            private static bool IsClientOnly(SqlExpression expr)
            {
                switch (expr.NodeType)
                {
                    case SqlNodeType.OuterJoinedValue:
                        return SqlColumnizer.ColumnNominator.IsClientOnly(((SqlUnary)expr).Operand);
                    case SqlNodeType.SharedExpression:
                    case SqlNodeType.SharedExpressionRef:
                    case SqlNodeType.SimpleExpression:
                    case SqlNodeType.TypeCase:
                    case SqlNodeType.Grouping:
                    case SqlNodeType.Multiset:
                    case SqlNodeType.Nop:
                    case SqlNodeType.DiscriminatedType:
                    case SqlNodeType.Element:
                    case SqlNodeType.Link:
                    case SqlNodeType.AliasRef:
                    case SqlNodeType.ClientArray:
                    case SqlNodeType.ClientCase:
                    case SqlNodeType.ClientQuery:
                        return true;
                    default:
                        return false;
                }
            }

            internal static bool CanBeColumn(SqlExpression expression)
            {
                if (SqlColumnizer.ColumnNominator.IsClientOnly(expression) || expression.NodeType == SqlNodeType.Column || !expression.SqlType.CanBeColumn)
                    return false;
                switch (expression.NodeType)
                {
                    case SqlNodeType.MethodCall:
                    case SqlNodeType.Member:
                    case SqlNodeType.New:
                        return PostBindDotNetConverter.CanConvert((SqlNode)expression);
                    default:
                        return true;
                }
            }
        }
    }

    internal class LiteralValidator : SqlVisitor
    {
        internal override SqlExpression VisitValue(SqlValue value)
        {
            if (!value.IsClientSpecified && value.ClrType.IsClass && (value.ClrType != typeof(string) && value.ClrType != typeof(Type)) && value.Value != null)
                throw Error.ClassLiteralsNotAllowed((object)value.ClrType);
            return (SqlExpression)value;
        }

        internal override SqlExpression VisitBinaryOperator(SqlBinary bo)
        {
            bo.Left = this.VisitExpression(bo.Left);
            return (SqlExpression)bo;
        }
    }

    internal class ColumnTypeValidator : SqlVisitor
    {
        internal override SqlRow VisitRow(SqlRow row)
        {
            int index = 0;
            for (int count = row.Columns.Count; index < count; ++index)
            {
                SqlColumn sqlColumn = row.Columns[index];
                SqlExpression sqlExpression = this.VisitExpression(sqlColumn.Expression);
                if (sqlExpression != null && TypeSystem.GetNonNullableType(sqlColumn.ClrType) != TypeSystem.GetNonNullableType(sqlExpression.ClrType))
                    throw Error.ColumnClrTypeDoesNotAgreeWithExpressionsClrType();
            }
            return row;
        }
    }

    internal class SqlSupersetValidator
    {
        private List<SqlVisitor> validators = new List<SqlVisitor>();

        internal void AddValidator(SqlVisitor validator)
        {
            this.validators.Add(validator);
        }

        internal void Validate(SqlNode node)
        {
            foreach (SqlVisitor sqlVisitor in this.validators)
                sqlVisitor.Visit(node);
        }
    }

    /// <summary>
    /// 表示映射函数或对可变返回序列的查询的结果。
    /// </summary>
    public interface IMultipleResults : IFunctionResult, IDisposable
    {
        /// <summary>
        /// 检索作为指定类型的序列的下一个结果。
        /// </summary>
        /// 
        /// <returns>
        /// 用于循环访问结果的枚举。
        /// </returns>
        /// <typeparam name="TElement">要返回的序列的类型。</typeparam>
        IEnumerable<TElement> GetResult<TElement>();
    }

    /// <summary>
    /// 提供对函数的返回值的访问。
    /// </summary>
    public interface IFunctionResult
    {
        /// <summary>
        /// 获取函数的返回值。
        /// </summary>
        /// 
        /// <returns>
        /// 由函数返回的值。
        /// </returns>
        object ReturnValue { get; }
    }

    internal class SqlParameterInfo
    {
        private SqlParameter parameter;
        private object value;
        private Delegate accessor;

        internal SqlParameterType Type
        {
            get
            {
                if (this.accessor != null)
                    return SqlParameterType.UserArgument;
                return this.parameter.Name == "@ROWCOUNT" ? SqlParameterType.PreviousResult : SqlParameterType.Value;
            }
        }

        internal SqlParameter Parameter
        {
            get
            {
                return this.parameter;
            }
        }

        internal Delegate Accessor
        {
            get
            {
                return this.accessor;
            }
        }

        internal object Value
        {
            get
            {
                return this.value;
            }
        }

        internal SqlParameterInfo(SqlParameter parameter, Delegate accessor)
        {
            this.parameter = parameter;
            this.accessor = accessor;
        }

        internal SqlParameterInfo(SqlParameter parameter, object value)
        {
            this.parameter = parameter;
            this.value = value;
        }

        internal SqlParameterInfo(SqlParameter parameter)
        {
            this.parameter = parameter;
        }
    }

    internal enum SqlParameterType
    {
        Value,
        UserArgument,
        PreviousResult,
    }

    internal interface IConnectionManager
    {
        DbConnection UseConnection(IConnectionUser user);

        void ReleaseConnection(IConnectionUser user);
    }
    internal interface ICompiledSubQuery
    {
        IExecuteResult Execute(IProvider provider, object[] parentArgs, object[] userArgs);
    }

    internal interface IObjectReaderCompiler
    {
        IObjectReaderFactory Compile(SqlExpression expression, Type elementType);

        IObjectReaderSession CreateSession(DbDataReader reader, IReaderProvider provider, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries);
    }

    internal interface IObjectReaderFactory
    {
        IObjectReader Create(DbDataReader reader, bool disposeReader, IReaderProvider provider, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries);

        IObjectReader GetNextResult(IObjectReaderSession session, bool disposeReader);
    }

    internal interface IObjectReader : IEnumerator, IDisposable
    {
        IObjectReaderSession Session { get; }
    }

    internal interface IObjectReaderSession : IConnectionUser, IDisposable
    {
        bool IsBuffered { get; }

        void Buffer();
    }

    internal class SqlConnectionManager : IConnectionManager
    {
        private IProvider provider;
        private DbConnection connection;
        private bool autoClose;
        private bool disposeConnection;
        private DbTransaction transaction;
        private System.Transactions.Transaction systemTransaction;
        private SqlInfoMessageEventHandler infoMessagehandler;
        private List<IConnectionUser> users;
        private int maxUsers;

        internal DbConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        internal int MaxUsers
        {
            get
            {
                return this.maxUsers;
            }
        }

        internal bool AutoClose
        {
            get
            {
                return this.autoClose;
            }
            set
            {
                this.autoClose = value;
            }
        }

        internal DbTransaction Transaction
        {
            get
            {
                return this.transaction;
            }
            set
            {
                if (value == this.transaction)
                    return;
                if (value != null && this.connection != value.Connection)
                    throw Error.TransactionDoesNotMatchConnection();
                this.transaction = value;
            }
        }

        internal SqlConnectionManager(IProvider provider, DbConnection con, int maxUsers, bool disposeConnection)
        {
            this.provider = provider;
            this.connection = con;
            this.maxUsers = maxUsers;
            this.infoMessagehandler = new SqlInfoMessageEventHandler(this.OnInfoMessage);
            this.users = new List<IConnectionUser>(maxUsers);
            this.disposeConnection = disposeConnection;
        }

        public DbConnection UseConnection(IConnectionUser user)
        {
            if (user == null)
                throw Error.ArgumentNull("user");
            if (this.connection.State == ConnectionState.Closed)
            {
                this.connection.Open();
                this.autoClose = true;
                this.AddInfoMessageHandler();
                if (System.Transactions.Transaction.Current != (System.Transactions.Transaction)null)
                    System.Transactions.Transaction.Current.TransactionCompleted += new TransactionCompletedEventHandler(this.OnTransactionCompleted);
            }
            if (this.transaction == null && System.Transactions.Transaction.Current != (System.Transactions.Transaction)null && System.Transactions.Transaction.Current != this.systemTransaction)
            {
                this.ClearConnection();
                this.systemTransaction = System.Transactions.Transaction.Current;
                this.connection.EnlistTransaction(System.Transactions.Transaction.Current);
            }
            if (this.users.Count == this.maxUsers)
                this.BootUser(this.users[0]);
            this.users.Add(user);
            return this.connection;
        }

        private void BootUser(IConnectionUser user)
        {
            bool flag = this.autoClose;
            this.autoClose = false;
            int index = this.users.IndexOf(user);
            if (index >= 0)
                this.users.RemoveAt(index);
            user.CompleteUse();
            this.autoClose = flag;
        }

        internal void DisposeConnection()
        {
            if (this.autoClose)
                this.CloseConnection();
            if (this.connection == null || !this.disposeConnection)
                return;
            this.connection.Dispose();
            this.connection = (DbConnection)null;
        }

        internal void ClearConnection()
        {
            while (this.users.Count > 0)
                this.BootUser(this.users[0]);
        }

        public void ReleaseConnection(IConnectionUser user)
        {
            if (user == null)
                throw Error.ArgumentNull("user");
            int index = this.users.IndexOf(user);
            if (index >= 0)
                this.users.RemoveAt(index);
            if (this.users.Count != 0 || !this.autoClose || (this.transaction != null || !(System.Transactions.Transaction.Current == (System.Transactions.Transaction)null)))
                return;
            this.CloseConnection();
        }

        private void CloseConnection()
        {
            if (this.connection != null && this.connection.State != ConnectionState.Closed)
                this.connection.Close();
            this.RemoveInfoMessageHandler();
            this.autoClose = false;
        }

        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs args)
        {
            if (this.provider.Log == null)
                return;
            this.provider.Log.WriteLine(Strings.LogGeneralInfoMessage((object)args.Source, (object)args.Message));
        }

        private void OnTransactionCompleted(object sender, TransactionEventArgs args)
        {
            if (this.users.Count != 0 || !this.autoClose)
                return;
            this.CloseConnection();
        }

        private void AddInfoMessageHandler()
        {
            SqlConnection sqlConnection = this.connection as SqlConnection;
            if (sqlConnection == null)
                return;
            sqlConnection.InfoMessage += this.infoMessagehandler;
        }

        private void RemoveInfoMessageHandler()
        {
            SqlConnection sqlConnection = this.connection as SqlConnection;
            if (sqlConnection == null)
                return;
            sqlConnection.InfoMessage -= this.infoMessagehandler;
        }
    }

    [Flags]
    internal enum OptimizationFlags
    {
        None = 0,
        SimplifyCaseStatements = 1,
        OptimizeLinkExpansions = 2,
        All = OptimizeLinkExpansions | SimplifyCaseStatements,
    }

    internal interface IConnectionUser
    {
        void CompleteUse();
    }

    internal interface IReaderProvider : IProvider, IDisposable
    {
        IDataServices Services { get; }

        IConnectionManager ConnectionManager { get; }
    }

    internal interface IProvider : IDisposable
    {
        TextWriter Log { get; set; }

        DbConnection Connection { get; }

        DbTransaction Transaction { get; set; }

        int CommandTimeout { get; set; }

        void Initialize(IDataServices dataServices, object connection);

        void ClearConnection();

        void CreateDatabase();

        void DeleteDatabase();

        bool DatabaseExists();

        IExecuteResult Execute(Expression query);

        ICompiledQuery Compile(Expression query);

        IEnumerable Translate(Type elementType, DbDataReader reader);

        IMultipleResults Translate(DbDataReader reader);

        string GetQueryText(Expression query);

        DbCommand GetCommand(Expression query);
    }

    internal interface IDataServices
    {
        DataContext Context { get; }

        MetaModel Model { get; }

        IDeferredSourceFactory GetDeferredSourceFactory(MetaDataMember member);

        object GetCachedObject(Expression query);

        bool IsCachedObject(MetaType type, object instance);

        object InsertLookupCachedObject(MetaType type, object instance);

        void OnEntityMaterialized(MetaType type, object instance);
    }
}
