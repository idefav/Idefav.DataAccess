using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Idefav.DbFactory;
using Idefav.Utility;

namespace Idefav.Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string conn =
                @"Data Source=.\SQL2008EX;Initial Catalog=DBTest;User Id=sa;Password=sa123; ";
            var db = DBOMaker.CreateDbObj("SQLServer");
            db.DbConnectStr = conn;
            var dt = db.Query("select * from students");
            DataTable d = db.QueryDataTable("select * from students");

            d = db.QueryPageTable("select * from students where studentname like '%'+" + db.GetParameterName("Name") + "+'%' ", 1, 5, "ID desc", "*", new KeyValuePair<string, object>(db.GetParameterName("Name"), "S"));
            var student = db.QueryModels<Student>("select * from students");
            Student s = new Student();
            s.StudentName = "学生1";
            s.ClassId = 12;
            s.Score = 100;
            var result = db.Insert(s);

        }


    }

    [TableName("Students")]
    public class Student
    {
        [AutoIncrement]
        [PrimaryKey]
        public int ID { get; set; }

        [TableField]
        public string StudentName { get; set; }

        [TableField]
        public int ClassId { get; set; }

        [TableField("Sore")]
        public int? Score { get; set; }
    }
}
