using System;
using System.IO.Ports;

namespace GSMMODEM
{
    /// <summary>
    /// 串口接口
    /// </summary>
    /// <remarks></remarks>
    public interface ICom
    {
        /// <summary>
        /// 获取或设置串行波特率。
        /// </summary>
        /// <value>The baud rate.</value>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// 指定的波特率小于或等于零，或者大于设备所允许的最大波特率。
        ///   </exception>
        ///   
        /// <exception cref="System.IO.IOException">
        /// 此端口处于无效状态。- 或 - 尝试设置基础端口状态失败。例如，从此 System.IO.Ports.SerialPort 对象传递的参数无效。
        ///   </exception>
        /// <remarks></remarks>
        int BaudRate { get; set; }

        /// <summary>
        /// Gets or sets the data bits.
        /// </summary>
        /// <value>The data bits.</value>
        /// <remarks></remarks>
        int DataBits { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值在串行通信过程中启用数据终端就绪 (DTR) 信号。
        /// </summary>
        /// <value><c>true</c> if [DTR enable]; otherwise, <c>false</c>.</value>
        /// <remarks></remarks>
        bool DtrEnable { get; set; }

        /// <summary>
        /// Gets or sets the handshake.
        /// </summary>
        /// <value>The handshake.</value>
        /// <remarks></remarks>
        Handshake Handshake { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is open.
        /// </summary>
        /// <remarks></remarks>
        bool IsOpen { get; }

        /// <summary>
        /// Gets or sets the parity.
        /// </summary>
        /// <value>The parity.</value>
        /// <remarks></remarks>
        Parity Parity { get; set; }

        /// <summary>
        /// Gets or sets the name of the port.
        /// </summary>
        /// <value>The name of the port.</value>
        /// <remarks></remarks>
        string PortName { get; set; }

        /// <summary>
        /// Gets or sets the read timeout.
        /// </summary>
        /// <value>The read timeout.</value>
        /// <remarks></remarks>
        int ReadTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [RTS enable].
        /// </summary>
        /// <value><c>true</c> if [RTS enable]; otherwise, <c>false</c>.</value>
        /// <remarks></remarks>
        bool RtsEnable { get; set; }

        /// <summary>
        /// Gets or sets the stop bits.
        /// </summary>
        /// <value>The stop bits.</value>
        /// <remarks></remarks>
        StopBits StopBits { get; set; }

        /// <summary>
        /// 串口收到数据时引发事件
        /// </summary>
        /// <remarks></remarks>
        event EventHandler DataReceived;

        /// <summary>
        /// Closes this instance.
        /// </summary>
        /// <remarks></remarks>
        void Close();

        /// <summary>
        /// Discards the in buffer.
        /// </summary>
        /// <remarks></remarks>
        void DiscardInBuffer();

        /// <summary>
        /// Opens this instance.
        /// </summary>
        /// <remarks></remarks>
        void Open();

        /// <summary>
        /// Reads the byte.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        int ReadByte();

        /// <summary>
        /// Reads the char.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        int ReadChar();

        /// <summary>
        /// Reads the existing.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        string ReadExisting();

        /// <summary>
        /// Reads the line.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        string ReadLine();

        /// <summary>
        /// Reads to.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        string ReadTo(string value);

        /// <summary>
        /// Writes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <remarks></remarks>
        void Write(string text);

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <remarks></remarks>
        void WriteLine(string text);
    }
}
