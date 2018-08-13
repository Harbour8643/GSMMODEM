using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GSMMODEM
{
    /// <summary>
    /// 简单消息发送
    /// </summary>
    public class SimGSMMsg<TMsg> where TMsg : ISendMsg
    {
        //消息队列
        private ConcurrentQueue<TMsg> _conQuSendMsg;
        //发送消息线程                                   
        private Thread _quSendMsgThread;
        private GsmModem srv;

        /// <summary>
        /// 队列循环等待时间，默认每2min执行一次
        /// </summary>
        public int MilsecTimeout { get; set; } = 1000 * 2;
        /// <summary>
        /// 重发次数，默认重发两次
        /// </summary>
        public int ReSendTimes { get; set; } = 2;
        /// <summary>
        /// 重发失败后是否重启短信猫，默认重启
        /// </summary>
        public bool IsReStart { get; set; } = true;
        /// <summary>
        /// 服务状态
        /// </summary>
        public bool IsOpen { get; private set; } = false;


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="gsmModem"></param>
        public SimGSMMsg(GsmModem gsmModem)
        {
            // 初始化.
            srv = gsmModem;

            this._conQuSendMsg = new ConcurrentQueue<TMsg>();
            _quSendMsgThread = new Thread(new ThreadStart(QuSendMsg));
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        public void StartService()
        {
            try
            {
                // 启动服务...
                string sResult = "";
                IsOpen = srv.Open(out sResult);
                if (!IsOpen)
                {
                    LogHelpers.Error("GMS开启服务失败:" + sResult);
                    return;
                }
                _quSendMsgThread.Start();
                LogHelpers.Write("GMS开启服务成功:" + sResult);
            }
            catch (Exception e)
            {
                LogHelpers.Error("GMS开启服务失败:" + e.StackTrace);
            }
        }
        /// <summary>
        /// 停止服务
        /// </summary>
        public void StopService()
        {
            try
            {
                _quSendMsgThread.Abort();
                // 停止服务
                srv.Close();
                IsOpen = false;
                LogHelpers.Error("GMS停止服务成功！");
            }
            catch (Exception e)
            {
                IsOpen = false;
                LogHelpers.Error("GMS服务停止失败:" + e.StackTrace);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMsg(TMsg msg)
        {
            _conQuSendMsg.Enqueue(msg);
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msgList"></param>
        public void SendMsg(List<TMsg> msgList)
        {
            msgList.ForEach(e => _conQuSendMsg.Enqueue(e));
        }

        /// <summary>
        /// 获取队列中所有消息
        /// </summary>
        /// <returns></returns>
        public List<TMsg> GetAllMsg()
        {
            return _conQuSendMsg.ToList();
        }

        //循环扫面消息队列并发送消息
        private void QuSendMsg()
        {
            while (true)
            {
                //当有需要发送的消息时
                if (_conQuSendMsg.Count(e => e.NeedSendTime <= DateTime.Now) > 0)
                {
                    int count = _conQuSendMsg.Count;
                    for (int i = 0; i < count; i++)
                    {
                        TMsg sendMsg = default(TMsg);
                        bool gotElement = _conQuSendMsg.TryDequeue(out sendMsg);
                        if (!gotElement || sendMsg == null)
                            continue;
                        if (sendMsg.NeedSendTime > DateTime.Now)
                        {
                            _conQuSendMsg.Enqueue(sendMsg);
                            continue;
                        }
                        OKSend(sendMsg);
                    }
                }
                Thread.Sleep(MilsecTimeout);
            }
        }
        //发送消息
        private void OKSend(TMsg sendMsg)
        {
            int i = 0;
            while (true)
            {
                i++;
                try
                {
                    srv.SendMsg(sendMsg.TelNum, sendMsg.Msg);
                    string logTxt = string.Format("消息发送成功:{0}---{1}", sendMsg.TelNum, sendMsg.Msg);
                    LogHelpers.Error(logTxt);
                    break;
                }
                catch (Exception ex)
                {
                    string logExTxt = string.Format("短信发送失败:第{0}次--{1}:{2}\n{3}\n{4}\n", i, sendMsg.TelNum, sendMsg.Msg, ex.Message, ex.StackTrace);
                    LogHelpers.Error(logExTxt);

                    Thread.Sleep(2 * 1000);
                    if (i > ReSendTimes)
                    {
                        ReStartService();
                        try
                        {
                            srv.SendMsg(sendMsg.TelNum, sendMsg.Msg);
                            string logTxt = string.Format("消息发送成功:{0}---{1}", sendMsg.TelNum, sendMsg.Msg);
                            LogHelpers.Error(logTxt);
                            break;
                        }
                        catch (Exception rEx)
                        {
                            string logTxt = string.Format("短信重启后发送失败:---{0}:{1}\n{2}\n{3}", sendMsg.TelNum, sendMsg.Msg, rEx.Message, rEx.StackTrace);
                            LogHelpers.Error(logTxt);
                            break;
                        }
                    }
                }
            }
        }
        //重启短信猫
        private void ReStartService()
        {
            LogHelpers.Error("GMS服务开始重启");
            try
            {
                srv.Close();
                IsOpen = false;
            }
            catch (Exception e)
            {
                string exMsg = string.Format("GMS服务重启时关闭失败:{0}\n{1}", e.Message, e.StackTrace);
                LogHelpers.Error(exMsg);
            }
            try
            {
                // 初始化.
                GsmModem newSrv = new GsmModem();
                newSrv.AutoDelMsg = srv.AutoDelMsg;
                newSrv.ComPort = srv.ComPort;
                newSrv.BaudRate = srv.BaudRate;
                // 启动服务...
                string sResult = "";
                IsOpen = newSrv.Open(out sResult);
                if (!IsOpen)
                {
                    LogHelpers.Error("GMS开启服务失败:" + sResult);
                    return;
                }
                srv = newSrv;
                LogHelpers.Error("GMS服务重启成功");
            }
            catch (Exception e)
            {
                string exMsg = string.Format("GMS服务重启失败:{0}\n{1}", e.Message, e.StackTrace);
                LogHelpers.Error(exMsg);
            }
        }
    }
}
