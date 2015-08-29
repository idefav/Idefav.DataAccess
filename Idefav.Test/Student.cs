using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Idefav.Utility;

namespace Idefav.Test
{
    [TableName("TD_Student")]
    public class Student
    {
        [AutoIncrement]
        [PrimaryKey]
        [TableField]
        public int ID { get; set; }

        [TableField]
        public string StudentName { get; set; }

        [TableField]
        public string ClassName { get; set; }

        [TableField]
        public Decimal? Score { get; set; }

        [TableField]
        public DateTime InTime { get; set; }
    }
}
