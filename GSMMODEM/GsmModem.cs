/*----------------------------------------------------------------
 *类库GSMMODEM完成通过短信猫发送和接收短信
 *开源地址：http://code.google.com/p/gsmmodem/
**----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GSMMODEM
{
    /// <summary>
    /// 设备类，完成短信发送 接收等
    /// </summary>
    public class GsmModem
    {
        #region 构造函数
        /// <summary>
        /// 默认构造函数 完成有关初始化工作
        /// </summary>
        /// <remarks>默认 端口号：COM1，波特率：9600</remarks>
        public GsmModem()
            : this("COM1", 9600)
        { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="comPort">串口号</param>
        /// <param name="baudRate">波特率</param>
        public GsmModem(string comPort, int baudRate)
        {
            _com = new Com();

            _com.PortName = comPort;          //
            _com.BaudRate = baudRate;
            _com.ReadTimeout = 10000;          //读超时时间 发送短信时间的需要
            _com.RtsEnable = true;            //必须为true 这样串口才能接收到数据

            _com.DataReceived += new EventHandler(sp_DataReceived);
        }

        //单元测试用构造函数
        internal GsmModem(ICom com)
        {
            _com = com;
            _com.ReadTimeout = 10000;          //读超时时间 发送短信时间的需要
            _com.RtsEnable = true;            //必须为true 这样串口才能接收到数据
            _com.DataReceived += new EventHandler(sp_DataReceived);
        }

        #endregion 构造函数

        #region 私有字段
        private ICom _com;//私有字段 串口对象
        private Queue<int> newMsgIndexQueue = new Queue<int>();//新消息序号
        private string msgCenter = string.Empty;//短信中心号码
        #endregion 私有字段

        #region 属性

        /// <summary>
        /// 串口号 运行时只读 设备打开状态写入将引发异常
        /// 提供对串口端口号的访问
        /// </summary>
        public string ComPort
        {
            get
            {
                return _com.PortName;
            }
            set
            {
                _com.PortName = value;
            }
        }

        /// <summary>
        /// 波特率 可读写
        /// 提供对串口波特率的访问
        /// </summary>
        public int BaudRate
        {
            get
            {
                return _com.BaudRate;
            }
            set
            {
                _com.BaudRate = value;
            }
        }

        /// <summary>
        /// 设备是否打开
        /// 对串口IsOpen属性访问
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return _com.IsOpen;
            }
        }

        private bool autoDelMsg = false;

        /// <summary>
        /// 对autoDelMsg访问
        /// 设置是否在阅读短信后自动删除 SIM 卡内短信存档
        /// 默认为 false 
        /// </summary>
        public bool AutoDelMsg
        {
            get
            {
                return autoDelMsg;
            }
            set
            {
                autoDelMsg = value;
            }
        }

        private string ModemStatusMsg = "";

        /// <summary>
        /// ModemMsg
        /// 用于存储和清理ModemMsg等待处理提示状态等信息
        /// 默认为 空
        /// </summary>
        public string modemStatusMsg
        {
            get
            {
                return ModemStatusMsg;
            }
            set
            {
                ModemStatusMsg = value;
            }
        }

        private int recvMsgLoc = 1;

        /// <summary>
        ///  RecvMsgLoc
        /// 设置接收短信后存储位置，1 Sim卡 2 直接串口输出（大部分Modem、部分手机不支持）
        /// 默认为 1 
        /// </summary>
        public int RecvMsgLoc
        {
            get
            {
                return recvMsgLoc;
            }
            set
            {
                recvMsgLoc = value;
            }
        }
        #endregion

        #region 收到短信事件

        /// <summary>
        /// 收到短信息事件 OnRecieved 
        /// 收到短信将引发此事件
        /// </summary>
        public event EventHandler SmsRecieved;

        #endregion

        #region 串口收到数据检测短信收到

        /// <summary>
        /// 从串口收到数据 串口事件
        /// 程序未完成需要的可自己添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sp_DataReceived(object sender, EventArgs e)
        {
            string temp = "";
            try
            {
                temp = _com.ReadLine();
            }
            catch (Exception ex)
            {
                //打印日志
                string errTxt = string.Format("  sp_DataReceived ReadLine Exception:{0}\r\n{1}\r\n{2}", "暂不处理", ex.Message, ex.StackTrace);
                LogHelpers.Error(errTxt);
                //throw new Exception("sp_DataReceived ReadLine Exception:" + ex.ToString() );
                //这个异常暂时不处理，如果Modem不能正常通讯，应在此断开连接重连；
                ModemStatusMsg = "sp_DataReceived ReadLine Exception:" + ex.ToString();
                temp = "";
            }

            if (temp.Length > 8)
            {
                if (temp.Substring(0, 6) == "+CMTI:")
                {
                    newMsgIndexQueue.Enqueue(Convert.ToInt32(temp.Split(',')[1]));  //存储新信息序号
                    OnSmsRecieved(e);                                //触发事件
                }
            }
        }

        /// <summary>
        /// 保护虚方法，引发收到短信事件
        /// </summary>
        /// <param name="e">事件数据</param>
        public virtual void OnSmsRecieved(EventArgs e)
        {
            if (SmsRecieved != null)
            {
                SmsRecieved(this, e);
            }
        }

        #endregion

        #region 方法

        #region 设备打开与关闭

        /// <summary>
        /// 设备打开函数，无法打开时将引发异常
        /// </summary>
        public void Open()
        {
            string sResult = "";
            bool bResult = Open(out sResult);
        }

        /// <summary>
        /// 返回连接信息的Open方法
        /// </summary>
        public bool Open(out string sResult)
        {
            //如果串口已打开 则先关闭
            sResult = "";
            if (_com.IsOpen)
            {
                _com.Close();
            }

            _com.Open();

            //初始化设备
            if (_com.IsOpen)
            {
                try
                {
                    _com.DataReceived -= sp_DataReceived;
                    _com.Write("ATE0\r");
                    Thread.Sleep(200);
                    sResult += " ATE0:" + _com.ReadExisting();
                    _com.Write("AT+CMGF=0\r");
                    Thread.Sleep(200);
                    sResult += " AT+CMGF=0:" + _com.ReadExisting();
                    _com.Write("AT+CNMI=2," + recvMsgLoc + "\r");
                    Thread.Sleep(200);
                    sResult += " AT+CNMI=2," + recvMsgLoc + ":" + _com.ReadExisting();
                    //绑定事件
                    _com.DataReceived += sp_DataReceived;

                    return true;
                }
                catch (Exception ex)
                {
                    //打印日志
                    string errTxt = string.Format("  Connect Send AT Exception Result:{0}\r\n{1}\r\n{2}", sResult, ex.Message, ex.StackTrace);
                    LogHelpers.Error(errTxt);
                    throw new Exception(" Connect Send AT Exception:" + ex.ToString() + " Result:" + sResult);
                }
            }
            return false;
        }

        /// <summary>
        /// 设备关闭函数
        /// </summary>
        public void Close()
        {
            try
            {
                _com.Close();
            }
            catch (Exception ex)
            {
                //打印日志
                string errTxt = string.Format("  Close:{0}\r\n{1}\r\n{2}", "设备关闭函数", ex.Message, ex.StackTrace);
                LogHelpers.Error(errTxt);
                throw ex;
            }
        }

        #endregion 设备打开与关闭

        #region 获取和设置设备有关信息

        /// <summary>
        /// 获取机器码
        /// </summary>
        /// <returns>机器码字符串（设备厂商，本机号码）</returns>
        public string GetMachineNo()
        {
            string result = SendAT("AT+CGMR");

            if (result.Length > 7 && result.Substring(result.Length - 4, 3).Trim() == "OK")
            {
                result = result.Substring(0, result.Length - 5).Trim();
            }
            else
            {
                //打印日志
                string errTxt = string.Format("  获取机器码失败:{0}", result);
                LogHelpers.Error(errTxt);
                throw new Exception("获取机器码失败:" + result);
            }
            return result;
        }

        /// <summary>
        /// 设置短信中心号码
        /// </summary>
        /// <param name="msgCenterNo">短信中心号码</param>
        public void SetMsgCenterNo(string msgCenterNo)
        {
            msgCenter = msgCenterNo;
        }

        /// <summary>
        /// 获取短信中心号码
        /// </summary>
        /// <returns></returns>
        public string GetMsgCenterNo()
        {
            string tmp = string.Empty;
            if (msgCenter != null && msgCenter.Length != 0)
            {
                return msgCenter;
            }
            else
            {
                tmp = SendAT("AT+CSCA?");
                if (tmp.Substring(tmp.Length - 4, 3).Trim() == "OK")
                {
                    return tmp.Split('\"')[1].Trim();
                }
                else
                {
                    //打印日志
                    string errTxt = string.Format("  获取短信中心失败:{0}", tmp);
                    LogHelpers.Error(errTxt);
                    throw new Exception("获取短信中心失败:" + tmp);
                }
            }
        }

        #endregion 获取和设置设备有关信息

        #region 发送AT指令

        /// <summary>
        /// 发送AT指令 逐条发送AT指令 调用一次发送一条指令
        /// 能返回一个OK或ERROR算一条指令
        /// </summary>
        /// <param name="ATCom">AT指令</param>
        /// <returns>发送指令后返回的字符串</returns>
        public string SendAT(string ATCom)
        {
            string result = string.Empty;
            //忽略接收缓冲区内容，准备发送
            _com.DiscardInBuffer();

            //注销事件关联，为发送做准备
            _com.DataReceived -= sp_DataReceived;

            //发送AT指令
            try
            {
                _com.Write(ATCom + "\r");
            }
            catch (Exception ex)
            {
                //打印日志
                string errTxt = string.Format("  SendAT:{0}\r\n{1}\r\n{2}", "发送AT指令", ex.Message, ex.StackTrace);
                LogHelpers.Error(errTxt);
                _com.DataReceived += sp_DataReceived;
                throw ex;
            }

            //接收数据 循环读取数据 直至收到“OK”或“ERROR”
            string temp = string.Empty;
            DateTime startTime = DateTime.Now;
            try
            {
                while (!(temp.Contains("OK") || temp.Contains("ERROR")))  //&& (DateTime.Now.CompareTo(StartTime) < 60000)
                {
                    temp = _com.ReadExisting();
                    result += temp;
                    Thread.Sleep(100);
                    if ((DateTime.Now - startTime).Seconds > 15)//运行时间大于20s，结束循环
                    {
                        //打印日志
                        LogHelpers.Error(string.Format("  获取短信返回状态码超时，result:{0}", result));
                        throw new Exception("  Read:" + temp);
                        //break;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                //打印日志
                string errTxt = string.Format("  Read:{0}\r\n{1}\r\n{2}", temp, ex.Message, ex.StackTrace);
                LogHelpers.Error(errTxt);
                throw new Exception(ex.ToString() + "  Read:" + temp);
            }
            finally
            {
                //事件重新绑定 正常监视串口数据
                _com.DataReceived += sp_DataReceived;
            }
        }

        #endregion 发送AT指令

        #region 发送短信

        /// <summary>
        /// 发送短信
        /// 发送失败将引发异常
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <param name="msg">短信内容</param>
        public void SendMsg(string phone, string msg)
        {
            PDUEncoding pe = new PDUEncoding();
            pe.ServiceCenterAddress = msgCenter;                    //短信中心号码 服务中心地址

            string tmp = string.Empty;
            foreach (CodedMessage cm in pe.PDUEncoder(phone, msg))
            {
                try
                {
                    //注销事件关联，为发送做准备,本命令直接发送不写Sim卡，CMSS从Sim卡发送，CMGW写Sim卡
                    _com.DataReceived -= sp_DataReceived;

                    _com.Write("AT+CMGS=" + cm.Length.ToString() + "\r");
                    _com.ReadTo(">");
                    _com.DiscardInBuffer();

                    //事件重新绑定 正常监视串口数据
                    _com.DataReceived += sp_DataReceived;

                    tmp = SendAT(cm.PduCode + (char)(26));  //26 Ctrl+Z ascii码
                }
                catch (Exception ex)
                {
                    //打印日志
                    string errTxt = string.Format("  短信发送失败:{0}\r\n{1}\r\n{2}", "SendMsg", ex.Message, ex.StackTrace);
                    LogHelpers.Error(errTxt);
                    throw new Exception("短信发送失败:" + ex.ToString());
                }
                if (tmp.Contains("OK"))
                {
                    continue;
                }
                //打印日志
                LogHelpers.Error(string.Format("  短信发送失败:{0}", tmp));
                throw new Exception("短信发送失败:" + tmp);
            }
        }

        #endregion 发送短信

        #region 读取短信

        /// <summary>
        /// 获取未读信息列表
        /// </summary>
        /// <returns>未读信息列表（中心号码，手机号码，发送时间，短信内容）</returns>
        public List<DecodedMessage> GetUnreadMsg(out string sInfo)
        {
            return GetReceiveMsg(0, out sInfo);
        }

        /// <summary>
        /// 获取已读或未读信息列表
        /// </summary>
        /// <returns>未读信息列表（中心号码，手机号码，发送时间，短信内容）</returns>
        public List<DecodedMessage> GetReceiveMsg(int iMsgType, out string sInfo)
        {
            List<DecodedMessage> result = new List<DecodedMessage>();
            string[] temp = null;
            string tmp = string.Empty;
            string sRead = string.Empty;
            int iCurIndex = 0;
            sInfo = "";

            if (iMsgType != 1) iMsgType = 0;

            tmp = SendAT("AT+CMGL=" + iMsgType);

            if (tmp.Contains("OK"))
            {
                temp = tmp.Split('\r');
                PDUEncoding pe = new PDUEncoding();
                foreach (string str in temp)
                {
                    if (str != null && str.Length > 6)   //短信PDU长度仅仅短信中心就18个字符
                    {
                        sRead = str.Replace((char)(13), ' ').Trim();
                        if (sRead.Substring(0, 6) == "+CMGL:")
                        {
                            iCurIndex = Convert.ToInt32(sRead.Split(',')[0].Substring(6));  //存储新信息序号
                        }
                        if (sRead.Length > 30)
                        {
                            try
                            {
                                //sInfo +=  " ReadPDUindex: " + iCurIndex + " sReadPDU:" + sRead;
                                result.Add(pe.PDUDecoder(iCurIndex, sRead));
                            }
                            catch (Exception ex)
                            {
                                sInfo += " DECODER:" + ex.ToString() + " ReadPDUindex: " + iCurIndex + " sReadPDU:" + sRead;
                                //打印日志
                                string errTxt = string.Format("  sInfo:{0}\r\n{1}\r\n{2}", sInfo, ex.Message, ex.StackTrace);
                                LogHelpers.Error(errTxt);
                                //return result;
                                //throw ex;
                            }
                            if (AutoDelMsg)
                            {
                                try
                                {
                                    DeleteMsgByIndex(iCurIndex);
                                }
                                catch (Exception ex)
                                {
                                    sInfo += " DEL:" + ex.ToString();
                                    //打印日志
                                    string errTxt = string.Format("  sInfo:{0}\r\n{1}\r\n{2}", sInfo, ex.Message, ex.StackTrace);
                                    LogHelpers.Error(errTxt);
                                    //throw ex;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 读取新消息
        /// </summary>
        /// <returns>新消息解码后内容</returns>
        /// <remarks>建议在收到短信事件中调用</remarks>
        public DecodedMessage ReadNewMsg(out int sMsgIndex)
        {
            sMsgIndex = newMsgIndexQueue.Dequeue();
            return ReadMsgByIndex(sMsgIndex);
        }

        /// <summary>
        /// 按序号读取短信
        /// </summary>
        /// <param name="index">序号</param>
        /// <returns>信息字符串 (中心号码，手机号码，发送时间，短信内容)</returns>
        public DecodedMessage ReadMsgByIndex(int index)
        {
            string temp = string.Empty;
            //string msgCenter, phone, msg, time;
            PDUEncoding pe = new PDUEncoding();
            try
            {
                temp = SendAT("AT+CMGR=" + index.ToString());
            }
            catch (Exception ex)
            {
                //打印日志
                string errTxt = string.Format("  ReadMsgByIndex:{0}\r\n{1}\r\n{2}", temp, ex.Message, ex.StackTrace);
                LogHelpers.Error(errTxt);
                throw ex;
            }

            if (temp.Contains("ERROR"))
            {
                //打印日志
                LogHelpers.Error("没有此短信");
                throw new Exception("没有此短信");
            }
            temp = temp.Split((char)(13))[2];       //取出PDU串(char)(13)为0x0a即\r 按\r分为多个字符串 第3个是PDU串

            //pe.PDUDecoder(temp, out msgCenter, out phone, out msg, out time);

            if (AutoDelMsg)
            {
                try
                {
                    DeleteMsgByIndex(index);
                }
                catch (Exception ex)
                {
                    //打印日志
                    string errTxt = string.Format("  {0}\r\n{1}", ex.Message, ex.StackTrace);
                    LogHelpers.Error(errTxt);
                    throw ex;
                }
            }

            return pe.PDUDecoder(index, temp.Replace((char)(13), ' ').Trim());
            //return msgCenter + "," + phone + "," + time + "," + msg;
        }

        #endregion 读取短信

        #region 删除短信

        /// <summary>
        /// 按索引号删除短信
        /// </summary>
        /// <param name="index">The index.</param>
        public void DeleteMsgByIndex(int index)
        {

            string sATResult = "";
            sATResult = SendAT("AT+CMGD=" + index.ToString()).Trim();

            if (sATResult.Contains("OK"))
            {
                return;
            }
            //打印日志
            LogHelpers.Error("删除失败:" + sATResult);
            throw new Exception("删除失败:" + sATResult);
        }

        /// <summary>
        /// 删除已读短信
        /// </summary>
        /// <returns>返回已删除条数（Out执行结果）</returns>
        public int DeletereadMsg(out string sResult)
        {
            List<DecodedMessage> result = new List<DecodedMessage>();
            //string[] temp = null;
            string tmp = string.Empty;

            sResult = string.Empty;
            int iResult = 0;         //改用直接删除命令，无法取得已删除条数；

            tmp = SendAT("AT+CMGD=1,1");
            sResult = tmp;

            /*
            int iCurIndex = 0;
            tmp = SendAT("AT+CMGL=1");
            if (tmp.Contains("OK"))
            {
                temp = tmp.Split('\r');
                PDUEncoding pe = new PDUEncoding();
                foreach (string str in temp)
                {
                    if (str != null && str.Length > 6)  //只处理首行和PDU内容 //短信PDU长度仅仅短信中心就18个字符
                    {
                        if (str.Substring(0, 6) == "+CMGL:")
                        {
                            iCurIndex = Convert.ToInt32(str.Split(',')[0].Substring(6));  //存储新信息序号
                            DeleteMsgByIndex(iCurIndex);
                            iResult++;
                            sResult += iCurIndex + ",";//【" + str + "】0[" + str.Split(',')[0] + "]1[" + str.Split(',')[1] + "]";
                        }
                    }
                }

            }*/

            return iResult;
        }

        #endregion 删除短信

        #region 扩展指令

        /// <summary>
        /// 设置Sim卡槽号，仅适用于MTK手机
        /// </summary>
        public bool SetMTKSim(int SimCardNo, out string sATResult)
        {
            SimCardNo = SimCardNo + 3;
            sATResult = "";// "AT+ESUO=" + SimCardNo + " Result:";
            try
            {
                sATResult += SendAT("AT+ESUO=" + SimCardNo);

                if (sATResult.Contains("OK"))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                //打印日志
                string errTxt = string.Format("  AT+ESUO=:{0}\r\n{1}\r\n{2}", SimCardNo, ex.Message, ex.StackTrace);
                LogHelpers.Error(errTxt);
                throw new Exception("AT+ESUO=" + SimCardNo + " Fail:" + ex.ToString());
            }
            return false;
            //throw new Exception("设置失败:" + sATResult);  
        }

        /// <summary>
        /// 设置串口是否禁止休眠
        /// </summary>
        public bool SetMTKSimSleep(bool bSleep, out string sATResult)
        {
            sATResult = "";// "AT+ESUO=" + SimCardNo + " Result:";
            try
            {
                if (bSleep)
                {
                    sATResult += SendAT("AT+ESLP=1");
                }
                else
                {
                    sATResult += SendAT("AT+ESLP=0");
                }

                if (sATResult.Contains("OK"))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                //打印日志
                string errTxt = string.Format("  AT+ESUO=:{0}\r\n{1}\r\n{2}", bSleep.ToString(), ex.Message, ex.StackTrace);
                LogHelpers.Error(errTxt);
                throw new Exception("AT+ESUO=" + bSleep.ToString() + " Fail:" + ex.ToString());
            }
            return false;
            //throw new Exception("设置失败:" + sATResult);  
        }
        #endregion 扩展指令

        #endregion
    }

}
