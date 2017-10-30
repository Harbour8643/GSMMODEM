using System;
using System.Collections.Generic;
using System.Text;

namespace GSMMODEM
{
    class Com : ICom
    {
        private System.IO.Ports.SerialPort sp = new System.IO.Ports.SerialPort();
        public event EventHandler DataReceived;
        public Com()
        {
            sp.DataReceived +=new System.IO.Ports.SerialDataReceivedEventHandler(sp_DataReceived);
        }

        public void sp_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
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

        public System.IO.Ports.Handshake Handshake
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

        public System.IO.Ports.Parity Parity
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

        public System.IO.Ports.StopBits StopBits
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
