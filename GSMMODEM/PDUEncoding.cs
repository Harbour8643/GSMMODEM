
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace GSMMODEM
{
    /// <summary>
    /// PDU编解码类，完成PDU短信格式的编码与解码
    /// 代码不是很安全，投入使用的话需要一定的改动 
    /// 私有类，只能命名空间内部使用 调试此类时须设为公有 完成后去掉public
    /// </summary>
    internal class PDUEncoding
    {
        /// <summary>
        /// 各个字段和属性 
        /// 字段 为 位组值（解码后） 属性对外显示 正常值（解码前）
        /// </summary>
        #region PDU编码和解码所需各段位组值

        private string serviceCenterAddress = "00";
        /// <summary>
        /// 消息服务中心(1-12个8位组)
        /// </summary>
        public string ServiceCenterAddress
        {
            get
            {
                int len = 2 * Convert.ToInt32(serviceCenterAddress.Substring(0, 2), 16);
                string result = serviceCenterAddress.Substring(4, len - 2);

                result = ParityChange(result);
                result = result.TrimEnd('F', 'f');
                return result;
            }
            set
            {
                if (value == null || value.Length == 0)      //号码为空
                {
                    serviceCenterAddress = "00";
                }
                else
                {
                    value = value.TrimStart('+');

                    /*
                     * 由于86只适用于国内，因而不加
                    if (value.Substring(0, 2) != "86")
                    {
                        value = "86" + value;
                    }
                     */

                    value = "91" + ParityChange(value);
                    serviceCenterAddress = (value.Length / 2).ToString("X2") + value;
                }

            }
        }

        private string protocolDataUnitType = "11";
        /// <summary>
        /// 协议数据单元类型(1个8位组)
        /// </summary>
        public string ProtocolDataUnitType
        {
            set
            {
                protocolDataUnitType = value;
            }
            get
            {
                return protocolDataUnitType;
            }
        }

        private string messageReference = "00";
        /// <summary>
        /// 所有成功的短信发送参考数目（0..255）
        /// (1个8位组)
        /// </summary>
        public string MessageReference
        {
            get
            {
                return "00";
            }
            set
            {
                messageReference = value;
            }
        }

        private string originatorAddress = "00";
        /// <summary>
        /// 发送方地址（手机号码）(2-12个8位组) 仅接收有效 只读属性
        /// </summary>
        public string OriginatorAddress
        {
            get
            {
                int len = Convert.ToInt32(originatorAddress.Substring(0, 2), 16);    //十六进制字符串转为整形数据
                string result = string.Empty;

                if (len % 2 == 1)       //号码长度是奇数，长度加1 编码时加了F
                {
                    len++;
                }

                result = originatorAddress.Substring(4, len);
                result = ParityChange(result).TrimEnd('F', 'f');    //奇偶互换，并去掉结尾F

                return result;
            }
        }

        private string destinationAddress = "00";
        /// <summary>
        /// 接收方地址（手机号码）(2-12个8位组) 仅发送有效 只写
        /// </summary>
        public string DestinationAddress
        {
            set
            {
                if (value == null || value.Length == 0)      //号码为空
                {
                    throw new ArgumentNullException("目的号码不允许为空");
                }
                else
                {
                    value = value.TrimStart('+');

                    if (value.Substring(0, 2) == "86")
                    {
                        value = value.TrimStart('8', '6');
                    }

                    int len = value.Length;
                    value = ParityChange(value);

                    destinationAddress = len.ToString("X2") + "A1" + value;
                }
            }
        }

        private string protocolIdentifer = "00";
        /// <summary>
        /// 参数显示消息中心以何种方式处理消息内容
        /// （比如FAX,Voice）(1个8位组)
        /// </summary>
        public string ProtocolIdentifer
        {
            get
            {
                return protocolIdentifer;
            }
            set
            {
                protocolIdentifer = value;
            }
        }

        private string dataCodingScheme = "08";
        /// <summary>
        /// 参数显示用户数据编码方案(1个8位组)
        /// </summary>
        public string DataCodingScheme
        {
            get
            {
                return dataCodingScheme;
            }
            set
            {
                dataCodingScheme = value;
            }
        }

        private string serviceCenterTimeStamp = "";
        /// <summary>
        /// 消息中心收到消息时的时间戳(7个8位组)  仅接收有效 只读属性
        /// </summary>
        public string ServiceCenterTimeStamp
        {
            get
            {
                string result = ParityChange(serviceCenterTimeStamp);
                result = "20" + result.Substring(0, 12);            //年加开始的“20”

                return result;
            }
        }

        private string validityPeriod = "C4";       //暂时固定有效期
        /// <summary>
        /// 短消息有效期(0,1,7个8位组)
        /// </summary>
        public string ValidityPeriod
        {
            get
            {
                return "C4";
            }
            set
            {
                validityPeriod = value;
            }
        }

        private string userDataLength = "00";
        /// <summary>
        /// 用户数据长度(1个8位组)
        /// </summary>
        public string UserDataLength
        {
            get
            {
                return (userData.Length / 2).ToString("X2");
            }
            set
            {
                userDataLength = value;
            }
        }

        private string userData = "";
        /// <summary>
        /// 用户数据(0-140个8位组)
        /// </summary>
        public string UserData
        {
            get
            {
                string result = string.Empty;
                try
                {
                    // 采纳 pinghua.huang建议，加入 3G 手机19编码判断为USC2需求
                    if (dataCodingScheme.Substring(1, 1) == "8" || dataCodingScheme.Substring(1, 1) == "9")             //USC2编码
                    {
                        int len = Convert.ToInt32(userDataLength, 16) * 2;

                        //四个一组，每组译为一个USC2字符
                        for (int i = 0; i < len; i += 4)
                        {
                            string temp = userData.Substring(i, 4);

                            int byte1 = Convert.ToInt16(temp, 16);

                            result += ((char)byte1).ToString();
                        }
                    }
                     else if (dataCodingScheme.Substring(1, 1) == "4")    //8bit编码
                    {
                        result = PDU8bitContentDecoder(userData);
                    }
                    else
                    {
                        result = PDU7bitContentDecoder(userData);
                        //result = Gsm7bitDecoding(userData);
                    }
                }
                catch (Exception ex)
                {
                    //由于解码出来SubString中引用的长度与字StrPdu实际长度不一致，因而加大异常捕获范围
                    throw new Exception(ex.Message + " GetUserData:" + userData);
                }

                return result;
            }
            set
            {
                if (dataCodingScheme.Substring(1, 1) == "8")           //USC2编码使用
                {
                    userData = string.Empty;
                    Encoding encodingUTF = Encoding.BigEndianUnicode;

                    byte[] Bytes = encodingUTF.GetBytes(value);

                    for (int i = 0; i < Bytes.Length; i++)
                    {
                        userData += BitConverter.ToString(Bytes, i, 1);
                    }
                    userDataLength = (userData.Length / 2).ToString("X2");
                }
                else                                                                //7bit编码使用
                {
                    userData = string.Empty;
                    userDataLength = value.Length.ToString("X2");                  //7bit编码 用户数据长度：源字符串长度

                    Encoding encodingAsscii = Encoding.ASCII;
                    byte[] bytes = encodingAsscii.GetBytes(value);

                    string temp = string.Empty;                                     //存储中间字符串 二进制串
                    string tmp;
                    byte btemp;
                    for (int i = value.Length; i > 0; i--)                          //高低交换 二进制串
                    {
                        //对ASCII码转换成7Bit PDU时，需进行转换；
                        btemp = ASCII2EQ7BIT(bytes[i - 1]);
                        tmp = Convert.ToString(btemp, 2);
                        while (tmp.Length < 7)                                      //不够7位，补齐
                        {
                            tmp = "0" + tmp;
                        }
                        temp += tmp;
                    }

                    for (int i = temp.Length; i > 0; i -= 8)                    //每8位取位为一个字符 即完成编码
                    {
                        if (i > 8)
                        {
                            userData += Convert.ToInt32(temp.Substring(i - 8, 8), 2).ToString("X2");
                        }
                        else
                        {
                            userData += Convert.ToInt32(temp.Substring(0, i), 2).ToString("X2");
                        }
                    }

                }
            }
        }

        #endregion PDU编码和解码所需各段位组值

        #region 私有方法

        /// <summary>
        /// 奇偶互换 (+F)
        /// </summary>
        /// <param name="str">要被转换的字符串</param>
        /// <returns>转换后的结果字符串</returns>
        private string ParityChange(string str)
        {
            string result = string.Empty;

            if (str.Length % 2 != 0)         //奇字符串 补F
            {
                str += "F";
            }
            for (int i = 0; i < str.Length; i += 2)     //奇偶互换
            {
                result += str[i + 1];
                result += str[i];
            }

            return result;
        }

        /// <summary>
        /// 判断字符串中是否不含中文字符（是否是ASCII字符串）
        /// </summary>
        /// <param name="str">要判断的字符串</param>
        /// <returns>bool值 是ASCII字符串，返回True；否则false</returns>
        private bool IsASCII(string str)
        {
            int strLen = str.Length;

            //字符串的字节数，字母占1位，汉字占2位,注意，一定要UTF8
            int byteLen = System.Text.Encoding.UTF8.GetBytes(str).Length;

            //字符数和字节数相等，则全部是ASCII码字符；不相等 则字节数大于字符数 含有汉字字符
            return (strLen == byteLen);
        }

        /// <summary>
        /// 十六进制字符串转二进制字节串
        /// </summary>
        /// <param name="hex">十六进制字符串</param>
        /// <returns>转化得到的byte[]</returns>
        private byte[] Hex2Bin(string hex)
        {
            byte[] bin = new byte[hex.Length / 2];  //结果字节串

            for (int i = 0; i < hex.Length; i += 2)
            {
                //两个字符一组 转化为一个字节
                bin[i / 2] = (byte)Convert.ToByte((hex[i].ToString() + hex[i + 1].ToString()), 16);
            }

            return bin;
        }

        /// <summary>
        /// byte数组转换为字符串 byte最高位0 忽略 每个byte 7个字符
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <returns>结果字符串</returns>
        private string Bin2BinStringof8Bit(byte[] bytes)
        {
            string result = string.Empty;

            foreach(byte b in bytes)
            {
                string tmp = Convert.ToString(b, 2);
                while (tmp.Length < 8)
                {
                    tmp = "0" + tmp;        //前导零补足8bit
                }
                result += tmp;
            }

            return result;
        }

        /// <summary> 
        /// 二进制字符串转化为16进制字符串 每4位一个十六进制字符
        /// </summary>
        /// <param name="bin">二进制字符串</param>
        /// <returns></returns>
        private string BinStringof8Bit2AsciiwithReverse(string bin)
        {
            
            string temp = bin;
            int byteLen = temp.Length / 7;

            //对于7字节编码，转化为Bit7时，将补7bit0,对于这种情况，需按照消息里标示长度去掉补0字符
            int removeLen = temp.Length % 7;
            int pduMsgLen = Convert.ToInt32(userDataLength, 16);
            if (pduMsgLen == (temp.Length / 7 - 1))
            {
                removeLen = 7;
                byteLen = pduMsgLen;
            }

            byte[] bytes = new byte[byteLen];
            //二进制 不是7倍数 去除前导0
            temp = temp.Remove(0, removeLen);

            for (int i = 0; i < temp.Length; i += 7)
            {
                bytes[i / 7] = EQ7BIT2ASCII(Convert.ToByte(temp.Substring(i, 7), 2));
            }

            Array.Reverse(bytes);

            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// PDU7bit的解码，供UserData的get访问器调用
        /// </summary>
        /// <param name="userData">数据部分PDU字符串</param>
        /// <returns></returns>
        private string PDU7bitContentDecoder(string userData)
        {
            string result = string.Empty;
            byte[] b;

            b = Hex2Bin(userData);

            Array.Reverse(b);       //字节串翻转

            result = Bin2BinStringof8Bit(b);

            result = BinStringof8Bit2AsciiwithReverse(result);

            return result;
        }


        /// <summary>
        /// PDU8bit的解码，供UserData的get访问器调用
        /// </summary>
        /// <param name="userData">数据部分PDU字符串</param>
        /// <returns></returns>
        private string PDU8bitContentDecoder(string userData)
        {
            byte[] buf = new byte[userData.Length / 2];

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < userData.Length; i += 2)
            {

                buf[i / 2] = byte.Parse(userData.Substring(i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            }

            return Encoding.ASCII.GetString(buf).Replace("\0", "");
        }

        /// <summary>
        /// PDU编码器，完成PDU编码(USC2编码，超过70个字时 分多条发送，PDU各个串之间逗号分隔)
        /// </summary>
        /// <param name="phone">目的手机号码</param>
        /// <param name="Text">短信内容</param>
        /// <returns>编码后的PDU字符串 长短信时 逗号分隔</returns>
        private List<CodedMessage> PDUUSC2Encoder(string phone, string Text)
        {
            dataCodingScheme = "08";
            DestinationAddress = phone;

            List<CodedMessage> result = new List<CodedMessage>();

            if (Text.Length > 70)
            {
                //长短信设TP-UDHI位为1 PDU-type = “51”
                ProtocolDataUnitType = "51";

                //计算长短信条数
                int count = Text.Length / 67;

                if (Text.Length % 67 != 0)
                {
                    count++;
                }

                for (int i = 0; i < count; i++)
                {
                    //如果不是最后一条
                    if (i < count - 1)
                    {
                        UserData = Text.Substring(i * 67, 67);

                        result.Add(new CodedMessage(serviceCenterAddress + protocolDataUnitType
                            + messageReference + destinationAddress + protocolIdentifer
                             + dataCodingScheme + validityPeriod + (userData.Length / 2 + 6).ToString("X2")
                             + "05000339" + count.ToString("X2") + (i + 1).ToString("X2") + userData));
                    }
                    else
                    {
                        UserData = Text.Substring(i * 67);

                        if (userData != null || userData.Length != 0)
                        {

                            result.Add(new CodedMessage(serviceCenterAddress + protocolDataUnitType
                                + messageReference + destinationAddress + protocolIdentifer
                                 + dataCodingScheme + validityPeriod + (userData.Length / 2 + 6).ToString("X2")
                                 + "05000339" + count.ToString("X2") + (i + 1).ToString("X2") + userData));
                        }
                    }
                }
                //return result;
            }
            else
            {
                //调整一下写法
                //不是长短信
                ProtocolDataUnitType = "11";
                UserData = Text;
                result.Add(new CodedMessage(serviceCenterAddress + protocolDataUnitType
                    + messageReference + destinationAddress + protocolIdentifer
                    + dataCodingScheme + validityPeriod + userDataLength + userData));
            }               
            return result;
        }

        /// <summary>
        /// 7bit 编码(超过160个字时 分多条发送，PDU各个串之间逗号分隔)
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <param name="Text">短信内容</param>
        /// <returns>编码后的字符串 长短信时 逗号分隔</returns>
        private List<CodedMessage> PDU7BitEncoder(string phone, string Text)
        {
            dataCodingScheme = "00";
            DestinationAddress = phone;

            List<CodedMessage> result = new List<CodedMessage>();

            if (Text.Length > 160)
            {
                //长短信设TP-UDHI位为1 PDU-type = “51”
                ProtocolDataUnitType = "51";

                //计算长短信条数
                int count = Text.Length / 153;

                //如果有下一条
                if (Text.Length % 153 != 0)
                {
                    count++;
                }

                for (int i = 0; i < count; i++)
                {
                    //如果不是最后一条
                    if (i < count - 1)
                    {
                        UserData = Text.Substring(i * 153 + 1, 152);    //去掉第一个字母（特殊编码）

                        result.Add(new CodedMessage(serviceCenterAddress + protocolDataUnitType
                            + messageReference + destinationAddress + protocolIdentifer
                             + dataCodingScheme + validityPeriod + (160).ToString("X2")
                             + "05000339" + count.ToString("X2") + (i + 1).ToString("X2")
                             + ((int)(new ASCIIEncoding().GetBytes(Text.Substring(i * 153, 1))[0] << 1)).ToString("X2") + userData));
                    }
                    else
                    {
                        UserData = Text.Substring(i * 153 + 1);

                        int len = Text.Substring(i * 153).Length;

                        if (userData != null || userData.Length != 0)
                        {
                            result.Add(new CodedMessage(serviceCenterAddress + protocolDataUnitType
                                + messageReference + destinationAddress + protocolIdentifer
                                 + dataCodingScheme + validityPeriod + (len + 7).ToString("X2")
                                 + "05000339" + count.ToString("X2") + (i + 1).ToString("X2")
                                 + ((int)(new ASCIIEncoding().GetBytes(Text.Substring(i * 153, 1))[0] << 1)).ToString("X2")
                                 + userData));
                        }
                    }
                }

                return result;
            }

            UserData = Text;

            result.Add(new CodedMessage(serviceCenterAddress + protocolDataUnitType
                + messageReference + destinationAddress + protocolIdentifer
                + dataCodingScheme + validityPeriod + userDataLength + userData));

            return result;
        }

        #endregion 私有方法

        #region 编码

        public List<CodedMessage> PDUEncoder(string phone, string text)
        {
            if (IsASCII(text))
            {
                return PDU7BitEncoder(phone, text);
            }
            else
            {
                return PDUUSC2Encoder(phone, text);
            }
        }

        #endregion 编码

        #region 解码

        #region 解码注释掉
        /*
        /// <summary>
        /// 完成手机或短信猫收到PDU格式短信的解码 暂时仅支持中文编码
        /// 未用DCS部分
        /// </summary>
        /// <param name="strPDU">短信PDU字符串</param>
        /// <param name="msgCenter">短消息服务中心 输出</param>
        /// <param name="phone">发送方手机号码 输出</param>
        /// <param name="msg">短信内容 输出</param>
        /// <param name="time">时间字符串 输出</param>
        public void PDUDecoder(string strPDU, out string msgCenter, out string phone, out string msg, out string time)
        {
            int lenSCA = Convert.ToInt32(strPDU.Substring(0, 2), 16) * 2 + 2;       //短消息中心占长度
            serviceCenterAddress = strPDU.Substring(0, lenSCA);

            int lenOA = Convert.ToInt32(strPDU.Substring(lenSCA + 2, 2),16);           //OA占用长度
            if (lenOA % 2 == 1)                                                     //奇数则加1 F位
            {
                lenOA++;
            }
            lenOA += 4;                 //加号码编码的头部长度
            originatorAddress = strPDU.Substring(lenSCA + 2, lenOA);

            dataCodingScheme = strPDU.Substring(lenSCA + lenOA + 4, 2);             //DCS赋值，区分解码7bit

            serviceCenterTimeStamp = strPDU.Substring(lenSCA + lenOA + 6, 14);

            userDataLength = strPDU.Substring(lenSCA + lenOA + 20, 2);
            int lenUD = Convert.ToInt32(userDataLength, 16) * 2;
            userData = strPDU.Substring(lenSCA + lenOA + 22);

            msgCenter = ServiceCenterAddress;
            phone = OriginatorAddress;
            msg = UserData;
            time = ServiceCenterTimeStamp;
        }
        */
        #endregion 解码注释掉

        /// <summary>
        /// 重载 解码，返回信息字符串 格式 
        /// </summary>
        /// <param name="strPDU">短信PDU字符串</param>
        /// <returns>信息字符串（MMNN,中心号码，手机号码，发送时间，短信内容 MM这批短信总条数 NN本条所在序号）</returns>
        public DecodedMessage PDUDecoder(string strPDU) {
            return PDUDecoder(0, strPDU);
        }


        public DecodedMessage PDUDecoder(int SmsIndex,string strPDU)
        {
            
			int lenSCA = 0; //错误PDU时可能抛出异常
            int lenOA;
            int lenPDU = strPDU.Length;
            try
            {
                lenSCA = Convert.ToInt32(strPDU.Substring(0, 2), 16) * 2 + 2;       //短消息中心占长度

                //int lenSCA = Convert.ToInt32(strPDU.Substring(0, 2), 16) * 2 + 2;       //短消息中心占长度

                serviceCenterAddress = strPDU.Substring(0, lenSCA);

                //PDU-type位组
                protocolDataUnitType = strPDU.Substring(lenSCA, 2);

                lenOA = Convert.ToInt32(strPDU.Substring(lenSCA + 2, 2), 16);           //OA占用长度
                if (lenOA % 2 == 1)                                                     //奇数则加1 F位
                {
                    lenOA++;
                }
                lenOA += 4;                 //加号码编码的头部长度
                originatorAddress = strPDU.Substring(lenSCA + 2, lenOA);

                dataCodingScheme = strPDU.Substring(lenSCA + lenOA + 4, 2);             //DCS赋值，区分解码7bit

                serviceCenterTimeStamp = strPDU.Substring(lenSCA + lenOA + 6, 14);

                userDataLength = strPDU.Substring(lenSCA + lenOA + 20, 2);
                int lenUD = Convert.ToInt32(userDataLength, 16) * 2;
            }
            catch (Exception ex)
            {
                //由于解码出来SubString中引用的长度与字StrPdu实际长度不一致，因而加大异常捕获范围
                throw new Exception(ex.Message + " PDU:" + strPDU);
            }

            if ((Convert.ToInt32(protocolDataUnitType, 16) & 0x40) != 0)    //长短信
            {
                if (dataCodingScheme.Substring(1, 1) == "8")           //USC2 长短信 去掉消息头
                {
                    try
                    {
                        userDataLength = (Convert.ToInt16(strPDU.Substring(lenSCA + lenOA + 20, 2), 16) - 6).ToString("X2");
                        userData = strPDU.Substring(lenSCA + lenOA + 22 + 6 * 2);
                    }
                    catch (Exception ex)
                    {
                        //由于解码出来SubString中引用的长度与字StrPdu实际长度不一致，因而加大异常捕获范围
                        throw new Exception(ex.Message + " LongMSG1-PDU:" + strPDU);
                    }
                    return new DecodedMessage(SmsIndex,strPDU.Substring(lenSCA + lenOA + 22 + 4 * 2, 2 * 2)
                        + strPDU.Substring(lenSCA + lenOA + 22 + 3 * 2, 2)
                        , ServiceCenterAddress
                        , ServiceCenterTimeStamp.Substring(0, 4) + "-" + ServiceCenterTimeStamp.Substring(4, 2) + "-"
                        + ServiceCenterTimeStamp.Substring(6, 2) + " " + ServiceCenterTimeStamp.Substring(8, 2) + ":"
                        + ServiceCenterTimeStamp.Substring(10, 2) + ":" + ServiceCenterTimeStamp.Substring(12, 2)
                        , OriginatorAddress, UserData);
                }
                else
                {
                    try
                    {
                        userData = strPDU.Substring(lenSCA + lenOA + 22 + 6 * 2 + 1 * 2);   //消息头六字节,第一字节特殊译码 >>7
                    }
                    catch (Exception ex)
                    {
                        //由于解码出来SubString中引用的长度与字StrPdu实际长度不一致，因而加大异常捕获范围
                        throw new Exception(ex.Message + " LongMSG2-PDU:" + strPDU);
                    }
                    //首字节译码 
                    byte byt = Convert.ToByte(strPDU.Substring(lenSCA + lenOA + 22 + 6 * 2, 2), 16);
                    char first = (char)(byt >> 1);

                    return new DecodedMessage(SmsIndex,strPDU.Substring(lenSCA + lenOA + 22 + 4 * 2, 2 * 2)
                        + strPDU.Substring(lenSCA + lenOA + 22 + 3 * 2, 2)
                        , ServiceCenterAddress
                        , ServiceCenterTimeStamp.Substring(0, 4) + "-" + ServiceCenterTimeStamp.Substring(4, 2) + "-"
                        + ServiceCenterTimeStamp.Substring(6, 2) + " " + ServiceCenterTimeStamp.Substring(8, 2) + ":"
                        + ServiceCenterTimeStamp.Substring(10, 2) + ":" + ServiceCenterTimeStamp.Substring(12, 2)
                        , OriginatorAddress
                        , first + UserData);
                }
            }

            try
            {
                userData = strPDU.Substring(lenSCA + lenOA + 22);
            }
            catch (Exception ex)
            {
                //由于解码出来SubString中引用的长度与字StrPdu实际长度不一致，因而加大异常捕获范围
                throw new Exception(ex.Message + " SMSUserData-PDU:" + strPDU);
            }
            return new DecodedMessage(SmsIndex,"010100"
                , ServiceCenterAddress
                , ServiceCenterTimeStamp.Substring(0, 4) + "-" + ServiceCenterTimeStamp.Substring(4, 2) + "-"
                + ServiceCenterTimeStamp.Substring(6, 2) + " " + ServiceCenterTimeStamp.Substring(8, 2) + ":"
                + ServiceCenterTimeStamp.Substring(10, 2) + ":" + ServiceCenterTimeStamp.Substring(12, 2)
                , OriginatorAddress
                ,UserData);
        } 

        //短信7Bit编码与ASCII码不一致的对应关系转换
        //Daniel Wang 2011-09-01
        //
        public static byte EQ7BIT2ASCII(byte EQ7Bit) {

            byte bResult = 0;
            switch (EQ7Bit)
            {
                case 0: bResult = 64; break;
                case 1: bResult = 163; break;
                case 2: bResult = 36; break;
                case 3: bResult = 165; break;
                case 4: bResult = 232; break;
                case 5: bResult = 233; break;
                case 6: bResult = 249; break;
                case 7: bResult = 236; break;
                case 8: bResult = 242; break;
                case 9: bResult = 199; break;
                case 11: bResult = 216; break;
                case 12: bResult = 248; break;
                case 14: bResult = 197; break;
                case 15: bResult = 229; break;
                case 17: bResult = 95; break;
                case 28: bResult = 198; break;
                case 29: bResult = 230; break;
                case 30: bResult = 223; break;
                case 31: bResult = 201; break;
                case 36: bResult = 164; break;
                case 64: bResult = 161; break;
                case 91: bResult = 196; break;
                case 92: bResult = 204; break;
                case 93: bResult = 209; break;
                case 94: bResult = 220; break;
                case 95: bResult = 167; break;
                case 96: bResult = 191; break;
                case 123: bResult = 228; break;
                case 124: bResult = 246; break;
                case 125: bResult = 241; break;
                case 126: bResult = 252; break;
                case 127: bResult = 224; break;
                default: bResult = EQ7Bit; break;
            }
            return bResult;
        }
        //Ascii to EQ7Bit
        public static byte ASCII2EQ7BIT(byte ASCIIC)
        {

            byte bResult = 0;
            switch (ASCIIC)
            {
                case 64: bResult = 0; break;
                case 163: bResult = 1; break;
                case 36: bResult = 2; break;
                case 165: bResult = 3; break;
                case 232: bResult = 4; break;
                case 233: bResult = 5; break;
                case 249: bResult = 6; break;
                case 236: bResult = 7; break;
                case 242: bResult = 8; break;
                case 199: bResult = 9; break;
                case 216: bResult = 11; break;
                case 248: bResult = 12; break;
                case 197: bResult = 14; break;
                case 229: bResult = 15; break;
                case 95: bResult = 17; break;
                case 198: bResult = 28; break;
                case 230: bResult = 29; break;
                case 223: bResult = 30; break;
                case 201: bResult = 31; break;
                case 164: bResult = 36; break;
                case 161: bResult = 64; break;
                case 196: bResult = 91; break;
                case 204: bResult = 92; break;
                case 209: bResult = 93; break;
                case 220: bResult = 94; break;
                case 167: bResult = 95; break;
                case 191: bResult = 96; break;
                case 228: bResult = 123; break;
                case 246: bResult = 124; break;
                case 241: bResult = 125; break;
                case 252: bResult = 126; break;
                case 224: bResult = 127; break;
                default: bResult = ASCIIC; break;
            }
            return bResult;
        }
        /*
         * 完整对应关系
           EQ7BIT2ASCII[0]  := 64;
           EQ7BIT2ASCII[1]  := 163;
           EQ7BIT2ASCII[2]  := 36;
           EQ7BIT2ASCII[3]  := 165;
           EQ7BIT2ASCII[4]  := 232;
           EQ7BIT2ASCII[5]  := 233;
           EQ7BIT2ASCII[6]  := 249;
           EQ7BIT2ASCII[7]  := 236;
           EQ7BIT2ASCII[8]  := 242;
           EQ7BIT2ASCII[9]  := 199;
           EQ7BIT2ASCII[10] := 10;
           EQ7BIT2ASCII[11] := 216;
           EQ7BIT2ASCII[12] := 248;
           EQ7BIT2ASCII[13] := 13;
           EQ7BIT2ASCII[14] := 197;
           EQ7BIT2ASCII[15] := 229;
           EQ7BIT2ASCII[16] := Nono;
           EQ7BIT2ASCII[17] := 95;
           EQ7BIT2ASCII[18] := Nono;
           EQ7BIT2ASCII[19] := Nono;
           EQ7BIT2ASCII[20] := Nono;
           EQ7BIT2ASCII[21] := Nono;
           EQ7BIT2ASCII[22] := Nono;
           EQ7BIT2ASCII[23] := Nono;
           EQ7BIT2ASCII[24] := Nono;
           EQ7BIT2ASCII[25] := Nono;
           EQ7BIT2ASCII[26] := Nono;
           EQ7BIT2ASCII[27] := Nono;
           EQ7BIT2ASCII[28] := 198;
           EQ7BIT2ASCII[29] := 230;
           EQ7BIT2ASCII[30] := 223;
           EQ7BIT2ASCII[31] := 201;
           EQ7BIT2ASCII[32] := 32;
           EQ7BIT2ASCII[33] := 33;
           EQ7BIT2ASCII[34] := 34;
           EQ7BIT2ASCII[35] := 35;
           EQ7BIT2ASCII[36] := 164;
           EQ7BIT2ASCII[37] := 37;
           EQ7BIT2ASCII[38] := 38;
           EQ7BIT2ASCII[39] := 39;
           EQ7BIT2ASCII[40] := 40;
           EQ7BIT2ASCII[41] := 41;
           EQ7BIT2ASCII[42] := 42;
           EQ7BIT2ASCII[43] := 43;
           EQ7BIT2ASCII[44] := 44;
           EQ7BIT2ASCII[45] := 45;
           EQ7BIT2ASCII[46] := 46;
           EQ7BIT2ASCII[47] := 47;
           EQ7BIT2ASCII[48] := 48;
           EQ7BIT2ASCII[49] := 49;
           EQ7BIT2ASCII[50] := 50;
           EQ7BIT2ASCII[51] := 51;
           EQ7BIT2ASCII[52] := 52;
           EQ7BIT2ASCII[53] := 53;
           EQ7BIT2ASCII[54] := 54;
           EQ7BIT2ASCII[55] := 55;
           EQ7BIT2ASCII[56] := 56;
           EQ7BIT2ASCII[57] := 57;
           EQ7BIT2ASCII[58] := 58;
           EQ7BIT2ASCII[59] := 59;
           EQ7BIT2ASCII[60] := 60;
           EQ7BIT2ASCII[61] := 61;
           EQ7BIT2ASCII[62] := 62;
           EQ7BIT2ASCII[63] := 63;
           EQ7BIT2ASCII[64] := 161;
           EQ7BIT2ASCII[65] := 65;
           EQ7BIT2ASCII[66] := 66;
           EQ7BIT2ASCII[67] := 67;
           EQ7BIT2ASCII[68] := 68;
           EQ7BIT2ASCII[69] := 69;
           EQ7BIT2ASCII[70] := 70;
           EQ7BIT2ASCII[71] := 71;
           EQ7BIT2ASCII[72] := 72;
           EQ7BIT2ASCII[73] := 73;
           EQ7BIT2ASCII[74] := 74;
           EQ7BIT2ASCII[75] := 75;
           EQ7BIT2ASCII[76] := 76;
           EQ7BIT2ASCII[77] := 77;
           EQ7BIT2ASCII[78] := 78;
           EQ7BIT2ASCII[79] := 79;
           EQ7BIT2ASCII[80] := 80;
           EQ7BIT2ASCII[81] := 81;
           EQ7BIT2ASCII[82] := 82;
           EQ7BIT2ASCII[83] := 83;
           EQ7BIT2ASCII[84] := 84;
           EQ7BIT2ASCII[85] := 85;
           EQ7BIT2ASCII[86] := 86;
           EQ7BIT2ASCII[87] := 87;
           EQ7BIT2ASCII[88] := 88;
           EQ7BIT2ASCII[89] := 89;
           EQ7BIT2ASCII[90] := 90;
           EQ7BIT2ASCII[91] := 196;
           EQ7BIT2ASCII[92] := 204;
           EQ7BIT2ASCII[93] := 209;
           EQ7BIT2ASCII[94] := 220;
           EQ7BIT2ASCII[95] := 167;
           EQ7BIT2ASCII[96] := 191;
           EQ7BIT2ASCII[97] := 97;
           EQ7BIT2ASCII[98] := 98;
           EQ7BIT2ASCII[99] := 99;
           EQ7BIT2ASCII[100] := 100;
           EQ7BIT2ASCII[101] := 101;
           EQ7BIT2ASCII[102] := 102;
           EQ7BIT2ASCII[103] := 103;
           EQ7BIT2ASCII[104] := 104;
           EQ7BIT2ASCII[105] := 105;
           EQ7BIT2ASCII[106] := 106;
           EQ7BIT2ASCII[107] := 107;
           EQ7BIT2ASCII[108] := 108;
           EQ7BIT2ASCII[109] := 109;
           EQ7BIT2ASCII[110] := 110;
           EQ7BIT2ASCII[111] := 111;
           EQ7BIT2ASCII[112] := 112;
           EQ7BIT2ASCII[113] := 113;
           EQ7BIT2ASCII[114] := 114;
           EQ7BIT2ASCII[115] := 115;
           EQ7BIT2ASCII[116] := 116;
           EQ7BIT2ASCII[117] := 117;
           EQ7BIT2ASCII[118] := 118;
           EQ7BIT2ASCII[119] := 119;
           EQ7BIT2ASCII[120] := 120;
           EQ7BIT2ASCII[121] := 121;
           EQ7BIT2ASCII[122] := 122;
           EQ7BIT2ASCII[123] := 228;
           EQ7BIT2ASCII[124] := 246;
           EQ7BIT2ASCII[125] := 241;
           EQ7BIT2ASCII[126] := 252;
           EQ7BIT2ASCII[127] := 224;
         */

        #endregion 解码

    }
}
