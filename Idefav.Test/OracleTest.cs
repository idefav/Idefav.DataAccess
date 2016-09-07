using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Idefav.DbObjects.Oracle;
using Idefav.IDAL;
using Idefav.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Idefav.Test
{
    [TestClass]
    public class OracleTest
    {
        private string ConnStr = @"Data Source=fundtest2;User Id=funddev;Password=FUNDDEV;";
        [TestMethod] 
        public void TestGetPageDataTable()
        {
            DbObject db = (DbObject) Idefav.DbFactory.DBOMaker.CreateDbObj("Oracle",ConnStr);
            db.DbConnectStr = ConnStr;
            int count = 0;
            Assert.AreEqual(db.QueryPageTable("select t.* from fundadmin.cfg_advert t", 1, 10, "eid", "*").Rows.Count>0,true);
             
        }

        public void TestGetPageDataTable_2()
        {
            IDbObject db = DbFactory.DBOMaker.CreateDbObj(DBType.Oracle, ConnStr);
            db.DbConnectStr = ConnStr;
            int count = 0;
            db.QueryPageTable("", 1, 20, out count, "eid", "*", new {});
            Assert.AreEqual(db.QueryPageTable("select t.* from fundadmin.cfg_advert t", 1, 10, "eid", "*").Rows.Count > 0, true);
        }

        [TestMethod]
        public void TestQueryPageTable()
        {
            IDbObject db = DbFactory.DBOMaker.CreateDbObj(DBType.Oracle, ConnStr);
            db.DbConnectStr = ConnStr;
            int count = 0;
           var result= db.QueryPageTable(@"
  select hkfcode, exddate pdate from FUNDADMIN.Hkfd_Bonus where (hkfcode,exddate) not in (select hkfcode,pdate from 
                                                          fundadmin.Hkfd_Chgratio where stype=1 and eisdel=0 
  )
   and eisdel=0 ", 1, 10, out count,
                "hkfcode", OrderDirection.DESC, "*", new {});
            Assert.AreEqual(result!=null &&result.Rows.Count>0,true);
        }
    }
}
