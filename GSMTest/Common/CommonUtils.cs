using Harbour.Utils;
using System;

namespace GSMTest.Common
{
    public static class CommonUtils
    {
        //日志方法
        public static void WriteLog(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            try
            {
                string Path = AppDomain.CurrentDomain.BaseDirectory + "Log\\";
                LogHelpers.Write(str, null, Path);
            }
            catch (Exception e)
            {
            }
        }

        //日志方法
        public static void WriteLogErrSendMsg(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;
            try
            {
                string Path = AppDomain.CurrentDomain.BaseDirectory + "Log\\ErrSend\\";
                LogHelpers.Write(str, "ErrSendMsg", Path);
            }
            catch (Exception e)
            {
            }
        }

        //日志方法
        public static void WriteLogON(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;
            try
            {
                string Path = AppDomain.CurrentDomain.BaseDirectory + "Log\\";
                LogHelpers.Write(str, "ON", Path);
            }
            catch (Exception e)
            {
            }
        }
    }
}
