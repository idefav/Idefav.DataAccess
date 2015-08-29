using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Idefav.Utility
{
    public sealed class ConfigHelper
    {
        public static string GetConfigString(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static bool GetConfigBool(string key)
        {
            bool flag = false;
            string configString = ConfigHelper.GetConfigString(key);
            if (configString != null)
            {
                if (string.Empty != configString)
                {
                    try
                    {
                        flag = bool.Parse(configString);
                    }
                    catch (FormatException ex)
                    {
                    }
                }
            }
            return flag;
        }

        public static Decimal GetConfigDecimal(string key)
        {
            Decimal num = new Decimal(0);
            string configString = ConfigHelper.GetConfigString(key);
            if (configString != null)
            {
                if (string.Empty != configString)
                {
                    try
                    {
                        num = Decimal.Parse(configString);
                    }
                    catch (FormatException ex)
                    {
                    }
                }
            }
            return num;
        }

        public static int GetConfigInt(string key)
        {
            int num = 0;
            string configString = ConfigHelper.GetConfigString(key);
            if (configString != null)
            {
                if (string.Empty != configString)
                {
                    try
                    {
                        num = int.Parse(configString);
                    }
                    catch (FormatException ex)
                    {
                    }
                }
            }
            return num;
        }
    }
}
