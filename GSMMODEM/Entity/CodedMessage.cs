using System;
using System.Collections.Generic;
using System.Text;

namespace GSMMODEM
{
    class CodedMessage
    {
        /// <summary>
        /// 构造函数 填入各值
        /// </summary>
        /// <param name="Code">pdu编码后字串</param>
        public CodedMessage(string Code)
        {
            PduCode = Code;
            Length = (Code.Length - Convert.ToInt32(Code.Substring(0, 2), 16) * 2 - 2) / 2;  //计算长度
        }

        public readonly int Length;

        public readonly string PduCode;
    }
}
