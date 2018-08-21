using GSMMODEM;
using System;

namespace GSMTest
{
    public class SendMsg : ISendMsg
    {
        /// <summary>
        /// 手机号
        /// </summary>
        public string TelNum { get; set; }
        /// <summary>
        /// 短信内容
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 需要发送的时间
        /// </summary>
        public DateTime NeedSendTime { get; set; }
    }
}
