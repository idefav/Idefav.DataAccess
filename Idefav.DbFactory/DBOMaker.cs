using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Idefav.IDAL;
using Idefav.Utility;

namespace Idefav.DbFactory
{
    public class DBOMaker
    {
        private static Cache cache = new Cache();

        private static object CreateObject(string path, string TypeName,string connStr)
        {
            var cacheKey = TypeName + "_" + connStr;
            object obj = DBOMaker.cache.GetObject((object)cacheKey);
            if (obj == null)
            {
                try
                {
                    obj = Assembly.Load(path).CreateInstance(TypeName,true,BindingFlags.Default,null,new object[]{connStr},null,null );
                    DBOMaker.cache.SaveCache((object)cacheKey, obj);
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
            }
            return obj;
        }

        public static IDbObject CreateDbObj(string dbTypename,string connStr)
        {
            return (IDbObject)DBOMaker.CreateObject("Idefav.DbObjects", "Idefav.DbObjects." + dbTypename + ".DbObject",connStr);
        }

        public static IDbObject CreateDbObj(DBType dbtype,string connStr)
        {
            return CreateDbObj(dbtype.ToString(),connStr);
        }
        
    }
}
