using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Idefav.Utility
{
    public class StringPlus
    {
        private StringBuilder str;

        public string Value
        {
            get
            {
                return this.str.ToString();
            }
        }

        public StringPlus()
        {
            this.str = new StringBuilder();
        }

        public string Space(int SpaceNum)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < SpaceNum; ++index)
                stringBuilder.Append("\t");
            return stringBuilder.ToString();
        }

        public string Append(string Text)
        {
            this.str.Append(Text);
            return this.str.ToString();
        }

        public string AppendLine()
        {
            this.str.Append("\r\n");
            return this.str.ToString();
        }

        public string AppendLine(string Text)
        {
            this.str.Append(Text + "\r\n");
            return this.str.ToString();
        }

        public string AppendSpace(int SpaceNum, string Text)
        {
            this.str.Append(this.Space(SpaceNum));
            this.str.Append(Text);
            return this.str.ToString();
        }

        public string AppendSpaceLine(int SpaceNum, string Text)
        {
            this.str.Append(this.Space(SpaceNum));
            this.str.Append(Text);
            this.str.Append("\r\n");
            return this.str.ToString();
        }

        public override string ToString()
        {
            return this.str.ToString();
        }

        public void DelLastComma()
        {
            string str = this.str.ToString();
            int length = str.LastIndexOf(",");
            if (length <= 0)
                return;
            this.str = new StringBuilder();
            this.str.Append(str.Substring(0, length));
        }

        public void DelLastChar(string strchar)
        {
            string str = this.str.ToString();
            int length = str.LastIndexOf(strchar);
            if (length <= 0)
                return;
            this.str = new StringBuilder();
            this.str.Append(str.Substring(0, length));
        }

        public void Remove(int Start, int Num)
        {
            this.str.Remove(Start, Num);
        }
    }
}
