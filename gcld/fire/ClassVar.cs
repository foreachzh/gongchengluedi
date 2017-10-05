using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TestApp.fire
{
    class ClassVar
    {
    }

    [DataContract]
    public class CipherCode
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        [DataMember]
        public string tp { get; set; }
        /// <summary>
        /// 密钥?
        /// </summary>
        [DataMember]
        public string public_exponent { get; set; }
        /// <summary>
        /// 获取是否成功
        /// </summary>
        [DataMember]
        public bool success { get; set; }
        /// <summary>
        /// 密钥2
        /// </summary>
        [DataMember]
        public string public_modulus { get; set; }
    }
    [DataContract]
    public class webPersonalInfo
    {
        [DataMember]
        public string latestVisiteOn { get; set; }
        // 
        [DataMember]
        public string playerid { get; set; }

        [DataMember]
        public webGameInfo phistory { get; set; }
    }
    [DataContract]
    public class webLoginInfo
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        [DataMember]
        public string ts { get; set; }

        [DataMember]
        public UserFlag current_user { get; set; }

        /// <summary>
        /// 获取是否成功
        /// </summary>
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string key { get; set; }
    }
    [DataContract]
    public class UserFlag
    {
        [DataMember]
        public string encryptInfo { get; set; }
    }

    [DataContract]
    public class webGameInfo
    {
        [DataMember]
        public string loginDatetime { get; set; }

        [DataMember]
        public string playerName { get; set; }
        //
        [DataMember]
        public string gameId { get; set; }
        [DataMember]
        public string serverName { get; set; }
        [DataMember]
        public string gameName { get; set; }

        [DataMember]
        public string id { get; set; }

        [DataMember]
        public string gameUrl { get; set; }

        //
        [DataMember]
        public string playerId { get; set; }
        [DataMember]
        public int server { get; set; }
        [DataMember]
        public string serverUrl { get; set; }
    }

    [DataContract]
    public class webRole
    {
        [DataMember]
        public int playerId { get; set; }
        [DataMember]
        public int playerLv { get; set; }
        [DataMember]
        public string playerName { get; set; }
        [DataMember]
        public int pic { get; set; }
        [DataMember]
        public int consumLv { get; set; }
        [DataMember]
        public bool isDelete { get; set; }
        [DataMember]
        public int forceId { get; set; }
        [DataMember]
        public int defaultPay { get; set; }
    }

    [DataContract]
    public class webDetailRole
    {
        [DataMember]
        public string serverKey { get; set; }
        [DataMember]
        public int playerId { get; set; }
        [DataMember]
        public int ast { get; set; }
        [DataMember]
        public int playerLv { get; set; }
        [DataMember]
        public string playerName { get; set; }
        [DataMember]
        public string userId { get; set; }
        [DataMember]
        public int forceId { get; set; }
        [DataMember]
        public int gold { get; set; }
        [DataMember]
        public int uGold { get; set; }
        [DataMember]
        public string pkey { get; set; }
        [DataMember]
        public string pkey2 { get; set; }
        [DataMember]
        public bool openTrade { get; set; }
        /// <summary>
        /// 银两
        /// </summary>
        [DataMember]
        public int copper { get; set; }

        [DataMember]
        public int copperMax { get; set; }

        [DataMember]
        public int copperOutput { get; set; }
        /// <summary>
        /// 粮食
        /// </summary>
        [DataMember]
        public int food { get; set; }
        [DataMember]
        public int foodMax { get; set; }
        [DataMember]
        public int foodOutput { get; set; }

        [DataMember]
        public int wood { get; set; }
        [DataMember]
        public int woodMax { get; set; }
        [DataMember]
        public int woodOutput { get; set; }

        [DataMember]
        public int iron { get; set; }
        [DataMember]
        public int ironMax { get; set; }
        [DataMember]
        public int ironOutput { get; set; }


        [DataMember]
        public int exp { get; set; }
        [DataMember]
        public int expNeed { get; set; }// 

        [DataMember]
        public int forces { get; set; }
        [DataMember]
        public int forcesMax { get; set; }// 

        [DataMember]
        public int fbLike { get; set; }// 
        [DataMember]
        public bool inPveBattle { get; set; }
        [DataMember]
        public bool inOccupyBattle { get; set; }
    }

    
    public class Command
    {
        public CmdType CommandType { get; set; }
        /// <summary>
        /// 包序
        /// </summary>
        public static int token{get;set;}
        /// <summary>
        /// 命令
        /// </summary>
        public string command{get;set;}

        public string commandstr { get; set; }
        /// <summary>
        /// 报文长度
        /// </summary>
        public int dataLength { get; set; }

        public byte[] outputarr { get; set; }

        public Dictionary<string, string> sendlst;

        static Command()
        {
            token = 0;
        }

        public Command(string cmd, Dictionary<string, string> lst)
        {
            CommandType = CommandList.getCurCmdType(cmd);

            token++;

            command = cmd;
            sendlst = lst;

            string str = string.Empty;
            if (lst != null)
            {
                foreach (var item in lst)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        str = str + "&";
                    }
                    str += item.Key + "=" + item.Value;
                }
            }

            int nDataLength = 4/*包头，标识长度*/+ 32/*命令*/+ 4/*包序*/+ str.Length;
            outputarr = new byte[nDataLength];

            int m = nDataLength - 4;
            byte[] bLen = new byte[4];
            bLen[3] = (byte)(m & 0xFF);
            bLen[2] = (byte)((m & 0xFF00) >> 8);
            bLen[1] = (byte)((m & 0xFF0000) >> 16);
            bLen[0] = (byte)((m >> 24) & 0xFF);  

            System.Text.Encoding encoder = Encoding.UTF8;  
            byte[] bCmd = encoder.GetBytes(cmd);
            byte[] bToken = new byte[4]; // BitConverter.GetBytes(token);
            bToken[3] = (byte)(token & 0xFF);
            bToken[2] = (byte)((token & 0xFF00)>>8);
            bToken[1] = (byte)((token & 0xFF0000) >> 16);
            bToken[0] = (byte)((token>>24)&0xff);
            byte[] bLst = encoder.GetBytes(str);

            Array.Copy(bLen, outputarr, 4);
            Array.Copy(bCmd, 0, outputarr, 4, bCmd.Length);
            Array.Copy(bToken, 0, outputarr, 36, 4);// outputarr.SetValue(token, 36);
            Array.Copy(bLst, 0, outputarr, 40, bLst.Length);
            
        }

        public Command(byte[] arr)
        {
            outputarr = arr;
            int nLen = (int)(arr[0] << 24) + (int)(arr[1] << 16) +  (int)(arr[2] << 8) +  (int)(arr[3]);

            dataLength = nLen;

            byte[] bcmd = new byte[32];
            Array.Copy(arr, 4, bcmd, 0, 32);
            string cmdstr = System.Text.Encoding.Default.GetString(bcmd).TrimEnd('\0');
            command = cmdstr;

            int cmdstrLen = dataLength - (/*  4+包头，标识长度 不包括包头部分*/32/*命令*/+ 4/*包序*/);
            if (cmdstrLen == 0)
            {
                commandstr = "";
                return;
            }
            else if (cmdstrLen < 0)
            {
                commandstr = "未知异常";
                return;
            }
            try
            {
                byte[] bcmdstr = new byte[cmdstrLen];
                Array.Copy(arr, 40, bcmdstr, 0, cmdstrLen);
                commandstr = System.Text.Encoding.Default.GetString(bcmdstr).TrimEnd('\0');
            }
            catch (Exception ex)
            {
                Console.WriteLine(""+ex.Message);
            }
        }

        public Command(string cmd, string cmdstr)
        {

            token++;

            command = cmd;
            // sendlst = lst;

            //string str = string.Empty;
            //if (lst != null)
            //{
            //    foreach (var item in lst)
            //    {
            //        if (!string.IsNullOrEmpty(str))
            //        {
            //            str = str + "&";
            //        }
            //        str += item.Key + "=" + item.Value;
            //    }
            //}
            System.Text.Encoding encoder = Encoding.UTF8;
            byte[] bLst = encoder.GetBytes(cmdstr);

            int nDataLength = 4/*包头，标识长度*/+ 32/*命令*/+ 4/*包序*/+ bLst.Length;
            outputarr = new byte[nDataLength];

            int m = nDataLength - 4;
            byte[] bLen = new byte[4];
            bLen[3] = (byte)(m & 0xFF);
            bLen[2] = (byte)((m & 0xFF00) >> 8);
            bLen[1] = (byte)((m & 0xFF0000) >> 16);
            bLen[0] = (byte)((m >> 24) & 0xFF);

            
            byte[] bCmd = encoder.GetBytes(cmd);
            byte[] bToken = new byte[4]; // BitConverter.GetBytes(token);
            bToken[3] = (byte)(token & 0xFF);
            bToken[2] = (byte)((token & 0xFF00) >> 8);
            bToken[1] = (byte)((token & 0xFF0000) >> 16);
            bToken[0] = (byte)((token >> 24) & 0xff);
            

            Array.Copy(bLen, outputarr, 4);
            Array.Copy(bCmd, 0, outputarr, 4, bCmd.Length);
            Array.Copy(bToken, 0, outputarr, 36, 4);// outputarr.SetValue(token, 36);
            Array.Copy(bLst, 0, outputarr, 40, bLst.Length);
        }
        //private static int m_heattoken = 1;
        //public Command(string cmd)
        //{
        //    if (cmd == CommandList.HEART_BEAT_TEST2)
        //    {

        //    }
        //}
    }
}
