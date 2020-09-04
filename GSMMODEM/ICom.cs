using System;
using System.IO.Ports;

namespace GSMMODEM
{
    /// <summary>
    /// 串口接口
    /// </summary>
    public interface ICom
    {
        /// <summary>
        /// 获取或设置串行波特率。
        /// </summary>
        int BaudRate { get; set; }

        /// <summary>
        /// DataBits
        /// </summary>
        int DataBits { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值在串行通信过程中启用数据终端就绪 (DTR) 信号。
        /// </summary>
        bool DtrEnable { get; set; }

        /// <summary>
        /// 指定为 System.IO.Ports.SerialPort 对象建立串行端口通信时使用的控制协议。
        /// </summary>
        Handshake Handshake { get; set; }

        /// <summary>
        /// IsOpen
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 为 System.IO.Ports.SerialPort 对象指定奇偶校验位。
        /// </summary>
        Parity Parity { get; set; }

        /// <summary>
        /// PortName
        /// </summary>
        string PortName { get; set; }

        /// <summary>
        /// ReadTimeout
        /// </summary>
        int ReadTimeout { get; set; }

        /// <summary>
        /// RtsEnable
        /// </summary>
        bool RtsEnable { get; set; }

        /// <summary>
        /// StopBits
        /// </summary>
        StopBits StopBits { get; set; }

        /// <summary>
        /// 串口收到数据时引发事件
        /// </summary>
        /// <remarks></remarks>
        event EventHandler DataReceived;

        /// <summary>
        /// Close
        /// </summary>
        void Close();

        /// <summary>
        /// Discards the in buffer.
        /// </summary>
        void DiscardInBuffer();

        /// <summary>
        ///  Opens this instance.
        /// </summary>
        void Open();

        /// <summary>
        ///  Reads the byte.
        /// </summary>
        /// <returns></returns>
        int ReadByte();

        /// <summary>
        /// Reads the char.
        /// </summary>
        /// <returns></returns>
        int ReadChar();

        /// <summary>
        /// Reads the existing.
        /// </summary>
        /// <returns></returns>
        string ReadExisting();

        /// <summary>
        /// Reads the line.
        /// </summary>
        /// <returns></returns>
        string ReadLine();

        /// <summary>
        /// ReadTo
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        string ReadTo(string value);

        /// <summary>
        ///  Writes the specified text.
        /// </summary>
        /// <param name="text"></param>
        void Write(string text);

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="text"></param>
        void WriteLine(string text);
    }
}
