using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSMMODEM
{
    /// <summary>
    /// 发送消息
    /// </summary>
    public interface ISendMsg
    {
        /// <summary>
        /// 手机号
        /// </summary>
        string TelNum { get; set; }
        /// <summary>
        /// 短信内容
        /// </summary>
        string Msg { get; set; }
        /// <summary>
        /// 需要发送的时间
        /// </summary>
        DateTime NeedSendTime { get; set; }
    }
}
