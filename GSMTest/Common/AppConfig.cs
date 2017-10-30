using Harbour.Utils;
using System;

namespace GSMTest.Common
{
    /// <summary>
    /// WebApi全局IP
    /// </summary>
    public static class AppConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public static string SocketIP
        {
            get
            {
                return ConfigHelper.GetConfigString("SocketIP"); 
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static int SocketProt
        {
            get
            {
                return int.Parse(ConfigHelper.GetConfigString("SocketProt")); 
            }
        }
        /// <summary>
        /// COM端口
        /// </summary>
        public static String GtwId
        {
            get
            {
                return ConfigHelper.GetConfigString("GtwId"); 
            }
        }
    }
}
