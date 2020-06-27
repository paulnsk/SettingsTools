using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace SettingsTools
{

    public static class AppSettings
    {

        #region private 
        /// <summary>
        /// Assemblies to read app.config from in the order of registration. If a later read config has a property with the same name then its value is overwritten or an error is thrown 
        /// </summary>
        private static readonly List<Assembly> RegisteredAssemblies = new List<Assembly>();
        
        /// <summary>
        /// Functions for converting complex (such as Color) or custom Types from string to T
        /// </summary>
        private static readonly Dictionary<Type, Func<string, object>> Converters = new Dictionary<Type, Func<string, object>>();
        
        private const int MaxErrors = 10;

        private static string AssemblyConfigLocation(Assembly assembly)
        {
            return assembly.Location + ".config";
        }

        private static string GlobalExeConfigLocation()
        {
            var assPath = Assembly.GetEntryAssembly()?.Location ?? "ERROR";
            return assPath + ".config";
        }

        private static void Fail(string errorMessage)
        {
            if (!ContinueOnErrors) throw new Exception(errorMessage);
            if (Errors.Count > MaxErrors)
            {
                Errors[Errors.Count - 1] = $"More errors occurred but only {MaxErrors} are shown";
                return;
            }
            Errors.Add(errorMessage);
        }

        #endregion private 


        #region Public props
        public static readonly List<string> Errors = new List<string>();
        public static bool ThrowExceptionWhenValuesInDifferentConfigsDontMatch { get; set; } = true; 
        public static bool IgnoreGlobalExeConfig { get; set; } = false;
        public static bool ContinueOnErrors { get; set; } = true;
        public static bool HasErrors => Errors.Count > 0;

        #endregion Public props


        #region Public funcs

        public static void RegisterAssembly(Assembly assembly)
        {
            //Console.WriteLine("Assembly.GetEntryAssembly()?.Location: "+ Assembly.GetEntryAssembly()?.Location);

            if (assembly.Location == Assembly.GetEntryAssembly()?.Location) return; //skipping global exe, it is handled separately
            if (RegisteredAssemblies.Any(a => a.Location == assembly.Location)) return; //skipping already registered
            RegisteredAssemblies.Add(assembly);
            //Console.WriteLine("registering " + assembly.Location);
        }

        public static void RegisterAssembly(Type sampletyType)
        {
            var ass = Assembly.GetAssembly(sampletyType);
            if (ass != null) RegisterAssembly(ass);
        }

        public static void RegisterConverter(Type type, Func<string, object> converter)
        {
            Converters[type] = converter;
        }


        #endregion Public funcs

        


        public static T GetSetting<T>(string name)
        {
            return GetSettingImpl<T>(name, null);

        }
        public static T GetSetting<T>(string name, T defaultValue)
        {
            return GetSettingImpl<T>(name, defaultValue.ToString());
        }


        private static T GetSettingImpl<T>(string name, string defaultStringValue = null)
        {

            //Console.WriteLine("Getting setting " + name);
            
            string stringValue;
            try
            {
                stringValue = Values[name];
                if (string.IsNullOrWhiteSpace(stringValue)) throw new Exception("empty string");
            }
            catch (Exception e)
            {
                if (defaultStringValue != null) stringValue = defaultStringValue;
                else
                {
                    Fail($"Unable to get setting [{name}]: " + e.Message + " " + e.InnerException?.Message);
                    return default;
                }
            }

            try
            {
                if (Converters.ContainsKey(typeof(T))) return (T)Converters[typeof(T)](stringValue);
                return (T)Convert.ChangeType(stringValue, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Fail($"Unable to convert value [{stringValue}] for setting [{name}]: " + e.Message + " " + e.InnerException?.Message);
                return default;
            }

        }

        
        private static readonly Lazy<AppSettingsValues> LazyValues = new Lazy<AppSettingsValues>();
        private static AppSettingsValues Values => LazyValues.Value;


        internal class AppSettingsValues
        {
            
            private readonly Dictionary<string, string> _valuesDic = new Dictionary<string, string>();


            private  void ReadOneConfig(string filePath)
            {
                //Console.WriteLine(" ___reading___ " + filePath);

                if (!File.Exists(filePath)) return;
                
                var doc = new XmlDocument(); 
                
                doc.Load(filePath);
                var node = doc.SelectSingleNode("//appSettings");
                if (node == null) return;
                var children = node.ChildNodes;

                foreach (XmlNode child in children)
                {
                    if (child.Attributes == null) continue;
                    string key;
                    string value;
                    try
                    {
                        key = child.Attributes["key"].Value;
                        value = child.Attributes["value"].Value;
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Unable to read {filePath}. Expected '<add key=\"KeyName\" value=\"Value\" />'. Error: " + e.Message);
                    }

                    string existingValue = null;
                    if (_valuesDic.ContainsKey(key)) existingValue = _valuesDic[key];

                    if (existingValue != null && ThrowExceptionWhenValuesInDifferentConfigsDontMatch && existingValue != value)
                    {
                        throw new Exception($"Different values in different configs for [{key}]! Remove duplicate values or set {nameof(ThrowExceptionWhenValuesInDifferentConfigsDontMatch)} to False.");
                    }

                    _valuesDic[key] = value;
                }
            }


            public AppSettingsValues()
            {
                foreach (var registeredAssembly in RegisteredAssemblies) ReadOneConfig(AssemblyConfigLocation(registeredAssembly));
                if (!IgnoreGlobalExeConfig) ReadOneConfig(GlobalExeConfigLocation());
            }

            public string this[string name] => _valuesDic[name];

        }



    }


    
    
}