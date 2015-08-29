using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Idefav.Utility
{
    public class VSProject
    {
        public void AddClass(string filename, string classname)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);
            switch (xmlDocument.DocumentElement.FirstChild.Name)
            {
                case "CSHARP":
                    this.AddClass2003(filename, classname);
                    break;
                case "PropertyGroup":
                    this.AddClass2005(filename, classname);
                    break;
            }
        }

        public void AddClass2003(string filename, string classname)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.ChildNodes)
            {
                foreach (XmlElement xmlElement1 in xmlNode)
                {
                    if (xmlElement1.Name == "Files")
                    {
                        foreach (XmlElement xmlElement2 in (XmlNode)xmlElement1)
                        {
                            if (xmlElement2.Name == "Include")
                            {
                                XmlElement element = xmlDocument.CreateElement("File", xmlDocument.DocumentElement.NamespaceURI);
                                element.SetAttribute("RelPath", classname);
                                element.SetAttribute("SubType", "Code");
                                element.SetAttribute("BuildAction", "Compile");
                                xmlElement2.AppendChild((XmlNode)element);
                                break;
                            }
                        }
                    }
                }
            }
            xmlDocument.Save(filename);
        }

        public void AddClass2005(string filename, string classname)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);
            foreach (XmlElement xmlElement in xmlDocument.DocumentElement.ChildNodes)
            {
                if (xmlElement.Name == "ItemGroup")
                {
                    string innerText = xmlElement.ChildNodes[0].InnerText;
                    if (xmlElement.ChildNodes[0].Name == "Compile")
                    {
                        XmlElement element = xmlDocument.CreateElement("Compile", xmlDocument.DocumentElement.NamespaceURI);
                        element.SetAttribute("Include", classname);
                        xmlElement.AppendChild((XmlNode)element);
                        break;
                    }
                }
            }
            xmlDocument.Save(filename);
        }

        public void AddClass2008(string filename, string classname)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);
            foreach (XmlElement xmlElement in xmlDocument.DocumentElement.ChildNodes)
            {
                if (xmlElement.Name == "ItemGroup")
                {
                    string innerText = xmlElement.ChildNodes[0].InnerText;
                    if (xmlElement.ChildNodes[0].Name == "Compile")
                    {
                        XmlElement element = xmlDocument.CreateElement("Compile", xmlDocument.DocumentElement.NamespaceURI);
                        element.SetAttribute("Include", classname);
                        xmlElement.AppendChild((XmlNode)element);
                        break;
                    }
                }
            }
            xmlDocument.Save(filename);
        }

        public void AddClass2005Aspx(string filename, string aspxname)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);
            foreach (XmlElement xmlElement in xmlDocument.DocumentElement.ChildNodes)
            {
                if (xmlElement.Name == "ItemGroup")
                {
                    string innerText = xmlElement.ChildNodes[0].InnerText;
                    if (xmlElement.ChildNodes[0].Name == "Compile")
                    {
                        XmlElement element = xmlDocument.CreateElement("Compile", xmlDocument.DocumentElement.NamespaceURI);
                        element.SetAttribute("Include", aspxname);
                        xmlElement.AppendChild((XmlNode)element);
                        break;
                    }
                }
            }
            xmlDocument.Save(filename);
        }

        public void AddMethodToClass(string ClassFile, string strContent)
        {
            if (!File.Exists(ClassFile))
                return;
            string str1 = File.ReadAllText(ClassFile, Encoding.Default);
            if (str1.IndexOf(" class ") <= 0)
                return;
            int num1 = str1.LastIndexOf("}");
            int num2 = str1.Substring(0, num1 - 1).LastIndexOf("}");
            string str2 = str1.Substring(0, num2 - 1) + "\r\n" + strContent + "\r\n}\r\n}";
            StreamWriter streamWriter = new StreamWriter(ClassFile, false, Encoding.Default);
            streamWriter.Write(str2);
            streamWriter.Flush();
            streamWriter.Close();
        }
    }
}
