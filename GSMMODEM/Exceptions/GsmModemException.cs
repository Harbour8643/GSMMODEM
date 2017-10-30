using System;
using System.Collections.Generic;
using System.Text;

namespace GSMMODEM
{
    /// <summary>
    /// gsmmodem异常，程序异常
    /// </summary>
    /// <remarks></remarks>
    public class GsmModemException : Exception
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public GsmModemException() : base() { }

        /// <summary>
        /// 有一个参数的构造函数
        /// </summary>
        /// <param name="str">异常提示字符串</param>
        public GsmModemException(String str) : base(str) { }
    }
}
