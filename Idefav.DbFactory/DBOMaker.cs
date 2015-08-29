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

        private static object CreateObject(string path, string TypeName)
        {
            object obj = DBOMaker.cache.GetObject((object)TypeName);
            if (obj == null)
            {
                try
                {
                    obj = Assembly.Load(path).CreateInstance(TypeName);
                    DBOMaker.cache.SaveCache((object)TypeName, obj);
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
            }
            return obj;
        }

        public static IDbObject CreateDbObj(string dbTypename)
        {
            return (IDbObject)DBOMaker.CreateObject("Idefav.DbObjects", "Idefav.DbObjects." + dbTypename + ".DbObject");
        }

        
    }
}
