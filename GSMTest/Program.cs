using GSMMODEM;
using System;
using System.Collections.Generic;

namespace GSMTest
{
    class Program
    {
        static void Main(string[] args)
        {
            SimGSMMsgTest();
            //GsmModemTest();
        }

        static void SimGSMMsgTest()
        {
            //波特率,指定发送端口.115200/9600
            GsmModem srv = new GsmModem("COM3", 115200);
            srv.AutoDelMsg = true;

            SimGSMMsg<SendMsg> gsmMessage = new SimGSMMsg<SendMsg>(srv);
            gsmMessage.StartService();
            Console.WriteLine("SimGSMMsg 打开成功!");

            string telNum = "10086";
            List<SendMsg> sendMsgList = new List<SendMsg>();
            for (int i = 0; i < 5; i++)
            {
                sendMsgList.Add(new SendMsg() { TelNum = telNum, Msg = "CXYE", NeedSendTime = DateTime.Now });
            };
            try
            {
                gsmMessage.SendMsg(sendMsgList);

                bool isEnd = true;
                while (isEnd)
                {
                    if (gsmMessage.GetAllMsg().Count == 0)
                        isEnd = false;
                }
                Console.WriteLine("SimGSMMsg 发送完成!");
            }
            catch (Exception ex)
            {
                if (gsmMessage.IsOpen)
                    gsmMessage.StopService();
                Console.WriteLine(ex);
            }
            finally
            {
                if (gsmMessage.IsOpen)
                    gsmMessage.StopService();
            }
            Console.ReadKey();
        }
        static void GsmModemTest()
        {
            bool opend = false;
            string sResult = "";
            GsmModem mGsmModem = null;
            try
            {
                mGsmModem = new GsmModem("COM3", 115200);
                mGsmModem.AutoDelMsg = true;
                opend = mGsmModem.Open(out sResult);
                if (opend)
                {
                    Console.WriteLine(string.Format("{0},{1} 打开成功", mGsmModem.ComPort, mGsmModem.BaudRate));
                }
                for (int i = 0; i < 5; i++)
                    mGsmModem.SendMsg("10086", "CXYE");
            }
            catch (Exception ex)
            {
                if (mGsmModem.IsOpen)
                    mGsmModem.Close();
                Console.WriteLine(string.Format("{0},{1} 打开失败[{2}]", mGsmModem.ComPort, mGsmModem.BaudRate, sResult));
                Console.WriteLine(ex);
            }
            finally
            {
                if (mGsmModem.IsOpen)
                    mGsmModem.Close();
            }
            Console.ReadKey();
        }
    }
}
