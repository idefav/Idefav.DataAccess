using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Idefav.Utility
{
    public class INIFile
    {
        public string path;

        public INIFile(string INIPath)
        {
            this.path = INIPath;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, byte[] retVal, int size, string filePath);

        public void IniWriteValue(string Section, string Key, string Value)
        {
            INIFile.WritePrivateProfileString(Section, Key, Value, this.path);
        }

        public void ClearAllSection()
        {
            this.IniWriteValue((string)null, (string)null, (string)null);
        }

        public void ClearSection(string Section)
        {
            this.IniWriteValue(Section, (string)null, (string)null);
        }

        public string IniReadValue(string Section, string Key)
        {
            StringBuilder retVal = new StringBuilder((int)byte.MaxValue);
            INIFile.GetPrivateProfileString(Section, Key, "", retVal, (int)byte.MaxValue, this.path);
            return retVal.ToString();
        }

        public byte[] IniReadValues(string section, string key)
        {
            byte[] retVal = new byte[(int)byte.MaxValue];
            INIFile.GetPrivateProfileString(section, key, "", retVal, (int)byte.MaxValue, this.path);
            return retVal;
        }

        public string[] IniReadValues()
        {
            return this.ByteToString(this.IniReadValues((string)null, (string)null));
        }

        private string[] ByteToString(byte[] sectionByte)
        {
            return new ASCIIEncoding().GetString(sectionByte).Split(new char[1]);
        }

        public string[] IniReadValues(string Section)
        {
            return this.ByteToString(this.IniReadValues(Section, (string)null));
        }
    }
}
