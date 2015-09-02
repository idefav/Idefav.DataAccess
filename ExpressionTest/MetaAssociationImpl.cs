using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
    internal abstract class MetaAssociationImpl : MetaAssociation
    {
        private static char[] keySeparators;

        static MetaAssociationImpl()
        {
            char[] chArray = new char[1];
            int index = 0;
            int num = 44;
            chArray[index] = (char)num;
            MetaAssociationImpl.keySeparators = chArray;
        }

        protected static ReadOnlyCollection<MetaDataMember> MakeKeys(MetaType mtype, string keyFields)
        {
            string[] strArray = keyFields.Split(MetaAssociationImpl.keySeparators);
            MetaDataMember[] metaDataMemberArray = new MetaDataMember[strArray.Length];
            for (int index = 0; index < strArray.Length; ++index)
            {
                strArray[index] = strArray[index].Trim();
                MemberInfo[] member = mtype.Type.GetMember(strArray[index], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (member == null || member.Length != 1)
                    throw Error.BadKeyMember((object)strArray[index], (object)keyFields, (object)mtype.Name);
                metaDataMemberArray[index] = mtype.GetDataMember(member[0]);
                if (metaDataMemberArray[index] == null)
                    throw Error.BadKeyMember((object)strArray[index], (object)keyFields, (object)mtype.Name);
            }
            return new List<MetaDataMember>((IEnumerable<MetaDataMember>)metaDataMemberArray).AsReadOnly();
        }

        protected static bool AreEqual(IEnumerable<MetaDataMember> key1, IEnumerable<MetaDataMember> key2)
        {
            using (IEnumerator<MetaDataMember> enumerator1 = key1.GetEnumerator())
            {
                using (IEnumerator<MetaDataMember> enumerator2 = key2.GetEnumerator())
                {
                    bool flag1 = enumerator1.MoveNext();
                    bool flag2;
                    for (flag2 = enumerator2.MoveNext(); flag1 & flag2; flag2 = enumerator2.MoveNext())
                    {
                        if (enumerator1.Current != enumerator2.Current)
                            return false;
                        flag1 = enumerator1.MoveNext();
                    }
                    if (flag1 != flag2)
                        return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            string format = "{0} ->{1} {2}";
            object[] objArray = new object[3];
            int index1 = 0;
            string name1 = this.ThisMember.DeclaringType.Name;
            objArray[index1] = (object)name1;
            int index2 = 1;
            string str = this.IsMany ? "*" : "";
            objArray[index2] = (object)str;
            int index3 = 2;
            string name2 = this.OtherType.Name;
            objArray[index3] = (object)name2;
            return string.Format((IFormatProvider)invariantCulture, format, objArray);
        }
    }
}
