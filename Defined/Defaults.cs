using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NancyDemon
{
    internal static class Defaults
    {
        private const string ASM_PATH = "bin/";
        private const string CODE_PATH = "App_Code/";
        private const bool IGNORE = true;
        private const string IGN_FILE = ".nancyignore";
        private const bool AUTOF5 = false;
        private const int AUTO_TIME = 10;

        /// <summary>
        /// Nancy.Assemblies
        /// </summary>
        private const string T_ASM = "Nancy.Assemblies";
        /// <summary>
        /// Nancy.Codes
        /// </summary>
        private const string T_CDS = "Nancy.Codes";
        /// <summary>
        /// Nancy.UseIgnore
        /// </summary>
        private const string T_UIG = "Nancy.UseIgnore";
        /// <summary>
        /// Nancy.IgnoreFile
        /// </summary>
        private const string T_IGF = "Nancy.IgnoreFile";
        /// <summary>
        /// Nancy.AutoRefresh
        /// </summary>
        private const string T_ARF = "Nancy.AutoRefresh";
        /// <summary>
        /// Nancy.RefreshInterval
        /// </summary>
        private const string T_ARI = "Nancy.RefreshInterval";

        public static void ReloadConfig()
        {
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static string AssemblyPath
        {
            get
            {
                string val = ConfigurationManager.AppSettings[T_ASM];
                return val != null ? val : ASM_PATH;
            }
        }

        public static string CodesPath
        {
            get
            {
                string val = ConfigurationManager.AppSettings[T_CDS];
                return val != null ? val : ASM_PATH;
            }
        }

        public static bool IsUseIgnoreFile
        {
            get
            {
                bool tmp;
                string val = ConfigurationManager.AppSettings[T_UIG];
                return bool.TryParse(val, out tmp) ? tmp : IGNORE;
            }
        }

        public static string IgnoreFileName
        {
            get
            {
                string val = ConfigurationManager.AppSettings[T_IGF];
                return val != null ? val : ASM_PATH;
            }
        }

        public static bool ISAutoRefresh
        {
            get
            {
                bool tmp;
                string val = ConfigurationManager.AppSettings[T_ARF];
                return bool.TryParse(val, out tmp) ? tmp : AUTOF5;
            }
        }

        public static int AutoRefreshInterval
        {
            get
            {
                int tmp;
                string val = ConfigurationManager.AppSettings[T_ARI];
                return int.TryParse(val, out tmp) ? tmp : AUTO_TIME;
            }
        }
    }
}
