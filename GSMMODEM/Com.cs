using System;
using System.IO.Ports;

namespace GSMMODEM
{
    class Com : ICom
    {
        private SerialPort sp = new SerialPort();
        public event EventHandler DataReceived;
        public Com()
        {
            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
        }

        public void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            OnDataReceived(e);
        }

        protected virtual void OnDataReceived(EventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(this, e);
            }
        }

        public int BaudRate
        {
            get
            {
                return sp.BaudRate;
            }
            set
            {
                sp.BaudRate = value;
            }
        }

        public int DataBits
        {
            get
            {
                return sp.DataBits;
            }
            set
            {
                sp.DataBits = value;
            }
        }

        public bool DtrEnable
        {
            get
            {
                return sp.DtrEnable;
            }
            set
            {
                sp.DtrEnable = value;
            }
        }

        public Handshake Handshake
        {
            get
            {
                return sp.Handshake;
            }
            set
            {
                sp.Handshake = value;
            }
        }

        public bool IsOpen
        {
            get { return sp.IsOpen; }
        }

        public Parity Parity
        {
            get
            {
                return sp.Parity;
            }
            set
            {
                sp.Parity = value;
            }
        }

        public string PortName
        {
            get
            {
                return sp.PortName;
            }
            set
            {
                sp.PortName = value;
            }
        }

        public int ReadTimeout
        {
            get
            {
                return sp.ReadTimeout;
            }
            set
            {
                sp.ReadTimeout = value;
            }
        }

        public bool RtsEnable
        {
            get
            {
                return sp.RtsEnable;
            }
            set
            {
                sp.RtsEnable = value;
            }
        }

        public StopBits StopBits
        {
            get
            {
                return sp.StopBits;
            }
            set
            {
                sp.StopBits = value;
            }
        }



        public void Close()
        {
            sp.Close();
        }

        public void DiscardInBuffer()
        {
            sp.DiscardInBuffer();
        }

        public void Open()
        {
            sp.Open();
        }

        public int ReadByte()
        {
            return sp.ReadByte();
        }

        public int ReadChar()
        {
            return sp.ReadChar();
        }

        public string ReadExisting()
        {
            string sResult = string.Empty;
            try
            {
                sResult = sp.ReadExisting();
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
                sResult = sp.ReadLine();
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
            return sp.ReadTo(value);
        }

        public void Write(string text)
        {
            sp.Write(text);
        }

        public void WriteLine(string text)
        {
            sp.WriteLine(text);
        }
    }
}
