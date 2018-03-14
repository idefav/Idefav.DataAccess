[TOC]
# 使用说明
 
## 一、基本使用
* 支持的数据库类型
```c#
public enum DBType
  {
    SQLServer = 1,
    MySQL = 2, 
    OleDb = 3,
    Oracle = 4,
    SQLite = 5,
  }
```
### 1、 创建数据访问实例
```c#
IDbObject db=DBOMaker.CreateDbObj(DBType.SQLServer, "连接字符串");
```
* 1、查询示例

```c#
IDbObject db = DBOMaker.CreateDbObj(DBType.SQLServer, AppSettings.COMMONSETTINGS.DbConfig);
var model = db.QueryModel<Demo1>(
    "select * from DB_Test.dbo.td_test where taskname=" + db.GetParameterName("taskname"),
    new { taskname = taskname });
```
```c#
[TableName("DB_Test.dbo.td_test")]
public class Demo1
{
    public string Guid { get; set; }
    public DateTime? UpdatedDay { get; set; }
    [PrimaryKey]
    public string TaskName { get; set; }
    public int? TimeUsed { get; set; }
    public DateTime UpdateTime { get; set; }
    public bool Status { get; set; }
    public string CurrentKeyWord { get; set; }
    public int? CurrentPage { get; set; }


}
```

* 2、DataTable
```c#
IDbObject db = DBOMaker.CreateDbObj(DBType.SQLServer, dbconn);
var table =db.QueryDataTable(
        "SELECT * FROM dbo.SysObjects WHERE ID = object_id(N'[" + GetTableName(tablename) + "]') AND OBJECTPROPERTY(ID, 'IsTable') = 1");
```

* 3、DataSet
```c#
/// <summary>
/// 查询
/// </summary>
/// <param name="SQLString">sql语句</param>
/// <param name="parameters">参数</param>
/// <returns></returns>
DataSet Query(string SQLString, object parameters=null);
```

* 4、DataReader
```c#
/// <summary>
/// 使用DataReader查询
/// </summary>
/// <param name="sql">Sql语句</param>
/// <param name="parameters">参数</param>
/// <returns></returns>
IDataReader QueryDataReader(string sql, object parameters=null);
```

* 5、分页
```c#
/// <summary>
/// 分页查询
/// </summary>
/// <param name="sqlstr">查询语句</param>
/// <param name="pageNo">页码</param>
/// <param name="pageSize">页面大小</param>
/// <param name="orderby">排序字段</param>
/// <param name="select">选择的字段</param>
/// <param name="parameters">参数</param>
/// <returns></returns>
DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string orderby, string select,
    object parameters=null);
```

```c#
/// <summary>
/// 分页查询
/// </summary>
/// <param name="sqlstr">sql语句</param>
/// <param name="pageNo">页码</param>
/// <param name="pageSize">页面大小</param>
/// <param name="orderby">排序</param>
/// <param name="direction">排序方向</param>
/// <param name="select">选择的字段</param>
/// <param name="parameters">参数</param>
/// <returns></returns>
DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, string orderby, OrderDirection direction, string select,
    object parameters=null);
```

```c#
/// <summary>
/// 分页查询 并返回总记录数量
/// </summary>
/// <param name="sqlstr">SQL语句</param>
/// <param name="pageNo">页码</param>
/// <param name="pageSize">分页大小</param>
/// <param name="count">总记录数量</param>
/// <param name="orderby">排序字段</param>
/// <param name="select">选择的字段</param>
/// <param name="parameters">参数</param>
/// <returns></returns>
DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, out int count, string orderby, string select, object parameters=null);
```

```c#
/// <summary>
/// 分页查询 并返回总记录数量
/// </summary>
/// <param name="sqlstr">sql</param>
/// <param name="pageNo">页码</param>
/// <param name="pageSize">分页大小</param>
/// <param name="count">总记录数量</param>
/// <param name="orderby">排序字段</param>
/// <param name="direction">排序方向</param>
/// <param name="select">选择的字段</param>
/// <param name="parameters">参数</param>
/// <returns></returns>
DataTable QueryPageTable(string sqlstr, int pageNo, int pageSize, out int count, string orderby, OrderDirection direction, string select, object parameters=null);
```
* 2、查询并返回Model
```c#
/// <summary>
/// 查询
/// </summary>
/// <typeparam name="T">模型类型</typeparam>
/// <param name="sql">sql语句</param>
/// <param name="parameters">参数</param>
/// <returns></returns>
T QueryModel<T>(string sql, object parameters=null) where T : class, new();
```

```c#
/// <summary>
/// 查询
/// </summary>
/// <typeparam name="T">模型类型</typeparam>
/// <param name="sql">sql语句</param>
/// <param name="parameters">参数</param>
/// <returns></returns>
List<T> QueryModels<T>(string sql, object parameters=null) where T : class, new();
```

## 2、模型使用说明
### 相关特性
* TableName
设置表名称和描述
如果Model的名称和数据库表名称不一致就需要设置一下
`[TableName("dbtabname")]`

* TableField
设置表的字段
如果模型的表的字段和数据库里面的不一致就需要设置一下
`[TableField("DBTableField")]`

* AutoIncrement
标识自增长字段
`[AutoIncrement]`

* PrimaryKey
设置主键字段
`[PrimaryKey]`