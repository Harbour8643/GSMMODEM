using System;
using System.IO.Ports;

namespace GSMMODEM
{
    class Com : ICom
    {
        private SerialPort serialPort = new SerialPort();
        public event EventHandler DataReceived;
        public Com()
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
        }

        public void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            OnDataReceived(e);
        }

        protected virtual void OnDataReceived(EventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }

        public int BaudRate
        {
            get
            {
                return serialPort.BaudRate;
            }
            set
            {
                serialPort.BaudRate = value;
            }
        }

        public int DataBits
        {
            get
            {
                return serialPort.DataBits;
            }
            set
            {
                serialPort.DataBits = value;
            }
        }

        public bool DtrEnable
        {
            get
            {
                return serialPort.DtrEnable;
            }
            set
            {
                serialPort.DtrEnable = value;
            }
        }

        public Handshake Handshake
        {
            get
            {
                return serialPort.Handshake;
            }
            set
            {
                serialPort.Handshake = value;
            }
        }

        public bool IsOpen
        {
            get { return serialPort.IsOpen; }
        }

        public Parity Parity
        {
            get
            {
                return serialPort.Parity;
            }
            set
            {
                serialPort.Parity = value;
            }
        }

        public string PortName
        {
            get
            {
                return serialPort.PortName;
            }
            set
            {
                serialPort.PortName = value;
            }
        }

        public int ReadTimeout
        {
            get
            {
                return serialPort.ReadTimeout;
            }
            set
            {
                serialPort.ReadTimeout = value;
            }
        }

        public bool RtsEnable
        {
            get
            {
                return serialPort.RtsEnable;
            }
            set
            {
                serialPort.RtsEnable = value;
            }
        }

        public StopBits StopBits
        {
            get
            {
                return serialPort.StopBits;
            }
            set
            {
                serialPort.StopBits = value;
            }
        }



        public void Close()
        {
            serialPort.Close();
        }

        public void DiscardInBuffer()
        {
            serialPort.DiscardInBuffer();
        }

        public void Open()
        {
            serialPort.Open();
        }

        public int ReadByte()
        {
            return serialPort.ReadByte();
        }

        public int ReadChar()
        {
            return serialPort.ReadChar();
        }

        public string ReadExisting()
        {
            string sResult = string.Empty;
            try
            {
                sResult = serialPort.ReadExisting();
            }
            catch (Exception ex)
            {
                //打印日志
                string errTxt = string.Format("  {0}\r\n{1}", ex.Message, ex.StackTrace);
                LogHelpers.Error(errTxt);
                throw ex;
            }
            return sResult;
        }

        public string ReadLine()
        {
            string sResult = string.Empty;
            try
            {
                sResult = serialPort.ReadLine();
            }
            catch (Exception ex)
            {
                //打印日志
                string errTxt = string.Format("  {0}\r\n{1}", ex.Message, ex.StackTrace);
                LogHelpers.Error(errTxt);
                throw ex;
            }
            return sResult;
        }

        public string ReadTo(string value)
        {
            return serialPort.ReadTo(value);
        }

        public void Write(string text)
        {
            serialPort.Write(text);
        }

        public void WriteLine(string text)
        {
            serialPort.WriteLine(text);
        }
    }
}
