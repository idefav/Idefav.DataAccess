using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Idefav.Utility
{
    public static class DatatypeMap
    {
        public static Hashtable LoadFromCfg(XmlDocument doc, string TypeName)
        {
            try
            {
                Hashtable hashtable = new Hashtable();
                XmlNode xmlNode1 = doc.SelectSingleNode("Map/" + TypeName);
                if (xmlNode1 != null)
                {
                    foreach (XmlNode xmlNode2 in xmlNode1.ChildNodes)
                        hashtable.Add((object)xmlNode2.Attributes["key"].Value, (object)xmlNode2.Attributes["value"].Value);
                }
                return hashtable;
            }
            catch (Exception ex)
            {
                throw new Exception("Load DatatypeMap file fail:" + ex.Message);
            }
        }

        public static Hashtable LoadFromCfg(string filename, string xpathTypeName)
        {
            try
            {
                Hashtable hashtable = new Hashtable();
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(filename);
                XmlNode xmlNode1 = xmlDocument.SelectSingleNode("Map/" + xpathTypeName);
                if (xmlNode1 != null)
                {
                    foreach (XmlNode xmlNode2 in xmlNode1.ChildNodes)
                        hashtable.Add((object)xmlNode2.Attributes["key"].Value, (object)xmlNode2.Attributes["value"].Value);
                }
                return hashtable;
            }
            catch (Exception ex)
            {
                throw new Exception("Load DatatypeMap file fail:" + ex.Message);
            }
        }

        public static string GetValueFromCfg(string filename, string xpathTypeName, string Key)
        {
            try
            {
                Hashtable hashtable = new Hashtable();
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(filename);
                XmlNode xmlNode1 = xmlDocument.SelectSingleNode("Map/" + xpathTypeName);
                if (xmlNode1 != null)
                {
                    foreach (XmlNode xmlNode2 in xmlNode1.ChildNodes)
                        hashtable.Add((object)xmlNode2.Attributes["key"].Value, (object)xmlNode2.Attributes["value"].Value);
                }
                object obj = hashtable[(object)Key];
                if (obj != null)
                    return obj.ToString();
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("Load DatatypeMap file fail:" + ex.Message);
            }
        }

        public static bool SaveCfg(string filename, string NodeText, Hashtable list)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlNode node = xmlDocument.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                xmlDocument.AppendChild(node);
                XmlElement element1 = xmlDocument.CreateElement("", NodeText, "");
                xmlDocument.AppendChild((XmlNode)element1);
                foreach (DictionaryEntry dictionaryEntry in list)
                {
                    XmlElement element2 = xmlDocument.CreateElement("", NodeText, "");
                    XmlAttribute attribute1 = xmlDocument.CreateAttribute("key");
                    attribute1.Value = dictionaryEntry.Key.ToString();
                    element2.Attributes.Append(attribute1);
                    XmlAttribute attribute2 = xmlDocument.CreateAttribute("value");
                    attribute2.Value = dictionaryEntry.Value.ToString();
                    element2.Attributes.Append(attribute2);
                    element1.AppendChild((XmlNode)element2);
                }
                xmlDocument.Save(filename);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Save DatatypeMap file fail:" + ex.Message);
            }
        }
    }
}
