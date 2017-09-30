using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using XRFAppPlat.Logger;

namespace TestApp
{
    class ForwardService
    {
        private static bool bStart = false;
        public static bool BStart { get { return bStart; } set { bStart = value; } }
        private Dictionary<string, RetInfo> m_infolist=new Dictionary<string, RetInfo>();
        public Dictionary<string, RetInfo> Infolist
        {
            get { return m_infolist; }
            set { m_infolist = value; }
        }
        private void ServerThread()
        {
            while (bStart)
            {


                Thread.Sleep(1000);
            }
        }

        public static bool SendByteArr(string IpAddr, int nPort, byte[] bytearr)
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(IPAddress.Parse(IpAddr), nPort);

                NetworkStream ns = tcpClient.GetStream();

                if (ns.CanWrite)
                {
                    ns.Write(bytearr, 0, bytearr.Length);
                    ns.Write(bytearr, 0, bytearr.Length);
                }
                else
                {
                    MessageBox.Show("不能写入数据流", "终止", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    //Console.WriteLine("You cannot write data to this stream.");
                    tcpClient.Close();

                    // Closing the tcpClient instance does not close the network stream.
                    ns.Close();
                    return false;
                }


                ns.Close();
                tcpClient.Close();

                return true;
            }
            catch (Exception ex)
            {
                ConsoleLog.Instance.writeInformationLog("转发数据失败,Reason=" + ex.Message);
            }
            return false;
        }
        // 9378306,ds_test,"8WR7HZu6SMuThybMFOlBYyKVjFA="
        public RetState GetPointCurrentValue(string device_id, string datastream_id, string api_key, ref RetInfo retvalue)
        {
            RetState state = new RetState();
            HttpClient2 client = new HttpClient2();
            // string query_URL = "http://api.heclouds.com/dtu/parser";
            string queryURL = string.Format("http://api.heclouds.com/devices/{0}/datastreams/{1}", device_id, datastream_id);
            client.AddRequestHeader("api-key", api_key);
            string retstr ="";
            try
            {
                 retstr = client.DownloadString(queryURL, string.Empty);
            }
            catch (Exception ex)
            {
                retstr = string.Format("从平台获取数据时发生异常,ErrorMsg={0};StackTrace={1}", ex.Message, ex.StackTrace);
                state.StateInfo = retstr;
                return state;
            }
            RetInfo result = JsonManager.JsonToObject(retstr, typeof(RetInfo)) as RetInfo;
            if (result != null && result.data != null)
            {
                state.success = true;
                retvalue = result;
                // ConsoleLog.Instance.writeInformationLog("" + result.data.current_value);
            }
            else
            {
                string value = "返回字符串为" + retstr + ";序列化失败！";
                state.StateInfo = value;
                // ConsoleLog.Instance.writeInformationLog();
            }
            return state;
        }

        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public static void ExchageThread()
        {
            ForwardService service = new ForwardService();
            // string sendstr = "";
            RetInfo retinfo = new RetInfo();
            string api_key = "8WR7HZu6SMuThybMFOlBYyKVjFA=";
            string ipaddr = AppSetting.GetValue("txtIPAddress");
            string portstr = AppSetting.GetValue("txtPort");
            if (string.IsNullOrEmpty(portstr))
            {
                ConsoleLog.Instance.writeInformationLog("端口号不能为空！"); return;
            }
            if (string.IsNullOrEmpty(ipaddr))
            {
                ConsoleLog.Instance.writeInformationLog("服务器IP地址不能为空！"); return;
            }

            IPAddress ip;
            if (!IPAddress.TryParse(ipaddr, out ip))
            {
                ConsoleLog.Instance.writeInformationLog("服务器IP地址格式不正确"); return;
            }
            while (bStart)
            {
                try
                {
                    RetState state = service.GetPointCurrentValue("9378306", "ds_test", api_key, ref retinfo);
                    if (!state.success)
                    {
                        ConsoleLog.Instance.writeInformationLog("异常=" + state.StateInfo);
                        return;
                    }
                    if (retinfo.errno != 0)
                    {
                        ConsoleLog.Instance.writeInformationLog("获取数据流发生错误,ErrorMsg=" + retinfo.error);
                        return;
                    }

                    bool bSend = false;
                    RetInfo info2 = new RetInfo();
                    if (service.Infolist.TryGetValue(api_key, out info2) == false)
                    {
                        bSend = true;
                        service.Infolist.Add(api_key, retinfo);
                    }
                    else
                    {
                        // 比对时间,不同则更新
                        if (info2.data.update_at != retinfo.data.update_at)
                        {
                            bSend = true;
                            service.Infolist[api_key] = info2;
                        }
                    }
                    if (bSend)
                    {
                        ConsoleLog.Instance.writeInformationLog("最新数据流时间:"+retinfo.data.update_at +";准备发送数据:"+retinfo.data.current_value);
                        byte[] bytearr = ForwardService.strToToHexByte(retinfo.data.current_value);
                        bool bSuccess = ForwardService.SendByteArr("111.38.122.83", 6761, bytearr);
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLog.Instance.writeInformationLog("发生了未捕获的异常,ErrorMsg="+ex.Message+";StackTrace="+ex.StackTrace);
                }

                Thread.Sleep(30 * 1000);
            }

        }
    }

    public class RetState
    {
        public bool success;
        public string StateInfo;
        public RetState()
        {
            success = false;
            StateInfo = "";
        }
    }
}
