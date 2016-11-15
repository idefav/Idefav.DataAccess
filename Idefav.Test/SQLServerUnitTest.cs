using System;
using System.Collections.Generic;
using Idefav.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Idefav.Test
{
    [TestClass]
    public class SQLServerUnitTest
    {
        private string ConnStr = @"Data Source=139.196.39.76;Initial Catalog=DB_News;Persist Security Info=True;User ID=sa;Password=idefav20160523";
        [TestMethod]
        public void TestIsExist()
        {
            DbObjects.SQLServer.DbObject db = new DbObjects.SQLServer.DbObject(ConnStr);
            db.DbConnectStr = ConnStr;
            Assert.AreEqual(db.IsExist(new Student() { ID = 1 }), true);

        }

        [TestMethod]
        public void TestInsert()
        {
            //DbObjects.SQLServer.DbObject db = new DbObjects.SQLServer.DbObject(ConnStr);
            //db.DbConnectStr = ConnStr;
            //Student student=new Student();
            //student.ID = 4;
            //student.StudentName = "Student4";
            //student.ClassName = "Class4";
            //student.Score = 30;
            //student.InTime=DateTime.Now;
            //Student stu2=new Student();
            //stu2.ID = 4; 
            //stu2.StudentName = "stu5";
            //stu2.ClassName = "Class5";
            //stu2.Score = 40;
            //stu2.InTime = DateTime.Now;
            //Assert.AreEqual(db.ExecuteTrans(trans =>
            //{
            //    db.Insert(student, trans);
            //    db.Insert(stu2, trans);
                
            //    return true;
            //}),true);
            //Assert.AreEqual(db.Insert(student),true);
        }

        [TestMethod]
        public void TestUpdate()
        {
            DbObjects.SQLServer.DbObject db = new DbObjects.SQLServer.DbObject(ConnStr);
            db.DbConnectStr = ConnStr;
            Student student = db.QueryModel<Student>("select * from td_student where id=" + db.GetParameterName("id"),
                new KeyValuePair<string, object>(db.GetParameterName("id"), 6));
            student.ID =6;
            student.StudentName = "Studentfsafasfasf";
            student.ClassName = "Class4sfasdfas";
            student.InTime=DateTime.Now;
            student.Score = 30;
            db.Update(student);
        }

        [TestMethod]
        public void TestUpate2()
        {
            DbObjects.SQLServer.DbObject db = new DbObjects.SQLServer.DbObject(ConnStr);
            db.DbConnectStr = ConnStr;

            db.Update("sore="+db.GetParameterName("sore"), "where sore>=100", "students", null,new KeyValuePair<string, object>(db.GetParameterName("sore"),50));
        }


        [TestMethod]
        public void TestToSQL()
        {
            Student student = new Student();
            student.ID = 4;
            student.StudentName = "Student4";
            student.ClassName = "Class4";
            ExpressionToSQL tosql=new ExpressionToSQL();
            tosql.ToSQL<Student>(c => c.ID == 1&&c.ClassName=="class1"&&c.Score>(decimal) 10.5&&c.InTime>DateTime.Now);
           
        }

        [TestMethod]
        public void TestDelete()
        {
            DbObjects.SQLServer.DbObject db = new DbObjects.SQLServer.DbObject(ConnStr);
            db.DbConnectStr = ConnStr;
            Student student=new Student();
            student.ID = 6;
            Assert.AreEqual(db.Delete(student),true);
        }

        [TestMethod]
        public void TestDelete2()
        {
            DbObjects.SQLServer.DbObject db = new DbObjects.SQLServer.DbObject(ConnStr);
            db.DbConnectStr = ConnStr;
            Assert.AreEqual(db.Delete("where sore<" + db.GetParameterName("sore"), "students", null,
                new KeyValuePair<string, object>(db.GetParameterName("sore"), 50)),true);
        }

        [TestMethod]
        public void TestQueryModels()
        {
            DbObjects.SQLServer.DbObject db = new DbObjects.SQLServer.DbObject(ConnStr);
            db.DbConnectStr = ConnStr;
            var models = db.QueryModels<Student>("select * from td_student");
            Assert.AreEqual(models.Count>0 ,true);
        }
    }
}
