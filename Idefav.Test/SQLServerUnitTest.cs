﻿using System;
using Idefav.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Idefav.Test
{
    [TestClass]
    public class SQLServerUnitTest
    {
        private string ConnStr = @"Data Source=.\SQL2012;Initial Catalog=DBTest;User Id=sa;Password=sa123;";
        [TestMethod]
        public void TestIsExist()
        {
            DbObjects.SQLServer.DbObject db = new DbObjects.SQLServer.DbObject();
            db.DbConnectStr = ConnStr;
            Assert.AreEqual(db.IsExist(new Student() { ID = 1 }), true);

        }

        [TestMethod]
        public void TestInsert()
        {
            DbObjects.SQLServer.DbObject db = new DbObjects.SQLServer.DbObject();
            db.DbConnectStr = ConnStr;
            Student student=new Student();
            student.ID = 4;
            student.StudentName = "Student4";
            student.ClassName = "Class4";
            student.Score = 30;
            Student stu2=new Student();
            stu2.ID = 4; 
            stu2.StudentName = "stu5";
            stu2.ClassName = "Class5";
            stu2.Score = 40;
            Assert.AreEqual(db.ExceuteTrans(trans =>
            {
                db.Insert(student, trans);
                db.Insert(stu2, trans);
                
                return true;
            }),true);
            //Assert.AreEqual(db.Insert(student),true);
        }

        [TestMethod]
        public void TestUpdate()
        {
            DbObjects.SQLServer.DbObject db = new DbObjects.SQLServer.DbObject();
            db.DbConnectStr = ConnStr;
            Student student = new Student();
            student.ID = 4;
            student.StudentName = "Student4";
            student.ClassName = "Class4";
            student.Score = 30;
            db.Update(student, t => t.ClassName=="test"&&t.ID==1);
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
    }
}