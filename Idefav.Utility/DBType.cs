using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idefav.Utility
{
    /// <summary>
    /// 数据库类型
    /// </summary>
   public enum DBType
    {
        SQLServer=1,
        MySQL=2,
        OleDb=3,
        Oracle=4,
        SQLite=5,
    }

    /// <summary>
    /// 排序方向
    /// </summary>
    public enum OrderDirection
    {
        DESC=1,ASC=-1
    }
}
