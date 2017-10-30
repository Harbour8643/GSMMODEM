using GSMMODEM;
using System;
namespace GSMTest
{
    class Program
    {
        static void Main(string[] args)
        {
            bool opend = false;
            string sResult = "";
            GsmModem mGsmModem = null;
            try
            {
                mGsmModem = new GsmModem();

                mGsmModem.AutoDelMsg = true;
                mGsmModem.ComPort = "COM3";
                mGsmModem.BaudRate = 115200;
                opend = mGsmModem.Open(out sResult);
                if (opend)
                {
                    Console.WriteLine(string.Format("{0},{1} 打开成功", mGsmModem.ComPort, mGsmModem.BaudRate));
                }
                for (int i = 0; i < 20; i++)
                    mGsmModem.SendMsg("10086", "CXYE");
            }
            catch (Exception ex)
            {
                if (mGsmModem.IsOpen)
                {
                    mGsmModem.Close();
                }
                Console.WriteLine(string.Format("{0},{1} 打开失败[{2}]", mGsmModem.ComPort, mGsmModem.BaudRate, sResult));
                Console.WriteLine(ex);
            }
            finally
            {
                if (mGsmModem.IsOpen)
                {
                    mGsmModem.Close();
                }
            }
            Console.ReadKey();
        }
    }
}
