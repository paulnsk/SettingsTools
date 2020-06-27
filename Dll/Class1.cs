using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dll
{
    public static class Class1
    {


        public static string ThisDllPath()
        {
            return Assembly.GetExecutingAssembly().Location;
            
        }

        public static string GlobalExePath()
        {
            return Assembly.GetEntryAssembly()?.Location ?? "ERROR";
        }


        //public static string GetSetting(string name)
        //{
        //    return ConfigurationManager.AppSettings[name];
        //}

    }



}
