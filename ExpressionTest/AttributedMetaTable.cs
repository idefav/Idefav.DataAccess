using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal sealed class AttributedMetaTable : MetaTable
    {
        private AttributedMetaModel model;
        private string tableName;
        private MetaType rowType;
        private bool hasMethods;
        private MethodInfo insertMethod;
        private MethodInfo updateMethod;
        private MethodInfo deleteMethod;

        public override MetaModel Model
        {
            get
            {
                return (MetaModel)this.model;
            }
        }

        public override string TableName
        {
            get
            {
                return this.tableName;
            }
        }

        public override MetaType RowType
        {
            get
            {
                return this.rowType;
            }
        }

        public override MethodInfo InsertMethod
        {
            get
            {
                this.InitMethods();
                return this.insertMethod;
            }
        }

        public override MethodInfo UpdateMethod
        {
            get
            {
                this.InitMethods();
                return this.updateMethod;
            }
        }

        public override MethodInfo DeleteMethod
        {
            get
            {
                this.InitMethods();
                return this.deleteMethod;
            }
        }

        internal AttributedMetaTable(AttributedMetaModel model, TableAttribute attr, Type rowType)
        {
            this.model = model;
            this.tableName = string.IsNullOrEmpty(attr.Name) ? rowType.Name : attr.Name;
            this.rowType = (MetaType)new AttributedRootType(model, this, rowType);
        }

        private void InitMethods()
        {
            if (this.hasMethods)
                return;
            Type contextType1 = this.model.ContextType;
            string name1 = "Insert" + this.rowType.Name;
            int num1 = 52;
            Type[] argTypes1 = new Type[1];
            int index1 = 0;
            Type type1 = this.rowType.Type;
            argTypes1[index1] = type1;
            this.insertMethod = MethodFinder.FindMethod(contextType1, name1, (BindingFlags)num1, argTypes1);
            Type contextType2 = this.model.ContextType;
            string name2 = "Update" + this.rowType.Name;
            int num2 = 52;
            Type[] argTypes2 = new Type[1];
            int index2 = 0;
            Type type2 = this.rowType.Type;
            argTypes2[index2] = type2;
            this.updateMethod = MethodFinder.FindMethod(contextType2, name2, (BindingFlags)num2, argTypes2);
            Type contextType3 = this.model.ContextType;
            string name3 = "Delete" + this.rowType.Name;
            int num3 = 52;
            Type[] argTypes3 = new Type[1];
            int index3 = 0;
            Type type3 = this.rowType.Type;
            argTypes3[index3] = type3;
            this.deleteMethod = MethodFinder.FindMethod(contextType3, name3, (BindingFlags)num3, argTypes3);
            this.hasMethods = true;
        }
    }
}
