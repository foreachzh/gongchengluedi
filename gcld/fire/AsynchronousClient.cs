using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using XRFAppPlat.Logger;
using System.Text.RegularExpressions;

namespace TestApp.fire
{

    // State object for receiving data from remote device.
    /// <summary>
    /// 从远程主机接收数据的状态对象。
    /// </summary>
    public class StateObject
    {
        // Client socket.
        /// <summary>
        /// 远程主机的socket
        /// </summary>
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 4096;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public byte[] tempbuff = null;

    }

    public class AsynchronousClient
    {
        // The port number for the remote device.

        // ManualResetEvent instances signal completion.
        /// <summary>
        /// 连接信号
        /// </summary>
        private  ManualResetEvent m_connectDone = new ManualResetEvent(false);
        /// <summary>
        /// 发送信号
        /// </summary>
        private  ManualResetEvent m_sendDone = new ManualResetEvent(false);

        public  ManualResetEvent SendDone
        {
            get { return m_sendDone; }
            set { m_sendDone = value; }
        }
        /// <summary>
        /// 接收信号
        /// </summary>
        private  ManualResetEvent m_receiveDone = new ManualResetEvent(false);

        public  ManualResetEvent ReceiveDone
        {
            get { return m_receiveDone; }
            set { m_receiveDone = value; }
        }

        // The response from the remote device.
        private static String response = String.Empty;

        private string m_IPAddr = string.Empty;
        private int m_nPort = 0;
        private Socket m_client = null;
        protected List<KeyValuePair<string, string>> m_rcvpackagelst = new List<KeyValuePair<string, string>>();

        //public List<KeyValuePair<string, string>> RecvLst
        //{
        //    get { return m_rcvpackagelst; }
        //    set { m_rcvpackagelst = value; }
        //}
        public AsynchronousClient(string ServerIP, int nport)
        {
            m_IPAddr = ServerIP;
            m_nPort = nport;
            m_client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ServerIP), nport);
            m_client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), m_client);

            m_connectDone.WaitOne();//阻止线程，等待连接完成信号

            Thread t = new Thread(CheckThread);
            t.IsBackground = true;
            t.Start();
        }

        ~AsynchronousClient()
        {
            if (m_client != null)
            {
                m_client.Shutdown(SocketShutdown.Both);
                m_client.Close();
                m_client = null;
            }
        }

        //private static void StartClient()
        //{
        //    // Connect to a remote device.
        //    try
        //    {
        //        // Establish the remote endpoint for the socket.
        //        // The name of the 
        //        // remote device is "host.contoso.com".
        //        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        //        IPAddress ipAddress = ipHostInfo.AddressList[0];
        //        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

        //        // Create a TCP/IP socket.
        //        Socket client = new Socket(AddressFamily.InterNetwork,
        //            SocketType.Stream, ProtocolType.Tcp);

        //        // Connect to the remote endpoint.异步连接到远程主机
        //        client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
        //        connectDone.WaitOne();//阻止线程，等待连接完成信号

        //        // Send test data to the remote device.
        //        Send(client, "This is a test<EOF>");
        //        sendDone.WaitOne();//阻止线程，等待发送完成信号

        //        // Receive the response from the remote device.
        //        Receive(client);
        //        receiveDone.WaitOne();//阻止线程，等待接收完成信号

        //        // Write the response to the console.
        //        Console.WriteLine("Response received : {0}", response);

        //        // Release the socket.
        //        client.Shutdown(SocketShutdown.Both);
        //        client.Close();

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        /// <summary>
        /// 连接完成回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                m_connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// 接收数据。
        /// </summary>
        /// <param name="client"></param>
        public void Receive(/*Socket client*/)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = m_client;

                // Begin receiving the data from the remote device.
                m_client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        /// <summary>
        /// 接收完成回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0&&bytesRead< 4096 && state.tempbuff == null)
                {
                    byte[] recvarr = state.buffer;
                    byte[] temparr = new byte[bytesRead];
                    Array.Copy(recvarr, 0, temparr, 0, bytesRead);
                    lock (m_lock2)
                    {
                        m_barrLst.Add(temparr);
                    }
                    m_receiveDone.Set();//将事件状态设置为终止状态，允许一个或多个等待线程继续。

                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else if(bytesRead == 4096)
                {
                    if (state.tempbuff == null)
                    {
                        state.tempbuff = state.buffer;
                        state.buffer = new byte[4096];
                    }
                    else
                    {// 重新开辟空间并接收数据
                        byte[] barr = new byte[state.tempbuff.Length + state.buffer.Length];
                        Array.Copy(state.tempbuff, 0, barr, 0, state.tempbuff.Length);
                        Array.Copy(state.buffer, 0, barr, state.tempbuff.Length, state.buffer.Length);
                        state.tempbuff = barr;
                    }
                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);

                }
                else
                {
                    // All the data has arrived; put it in response.
                    //if (state.sb.Length > 1)
                    //{
                    //    response = state.sb.ToString();
                    //}
                    // Signal that all bytes have been received.
                    
                    // There might be more data, so store the data received so far.
                    // state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    byte[] barr = new byte[state.tempbuff.Length + state.buffer.Length];
                    Array.Copy(state.tempbuff, 0, barr, 0, state.tempbuff.Length);
                    Array.Copy(state.buffer, 0, barr, state.tempbuff.Length, state.buffer.Length);
                    state.tempbuff = barr;

                    byte[] recvarr = state.tempbuff;
                    byte[] temparr = new byte[barr.Length-4096+bytesRead];
                    Array.Copy(recvarr, 0, temparr, 0, temparr.Length);
                    lock (m_lock2)
                    {
                        m_barrLst.Add(temparr);
                    }

                    state.tempbuff = null;

                    m_receiveDone.Set();
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void CheckThread()
        {
            while (true)
            {
                Thread.Sleep(50);
                if (m_barrLst.Count > 0)
                {
                    byte[] barr = m_barrLst[0];
                    lock (m_lock2)
                    {
                        m_barrLst.RemoveAt(0);
                    }
                    DealData(barr);
                }
            }
        }

        private List<byte[]> m_barrLst = new List<byte[]>();
        private static readonly object m_lock2 = new object();
        protected static readonly object m_lock = new object();
        private void DealData(byte[] recvarr)
        {
            List<KeyValuePair<string, string>> lst= getZipByteArr(recvarr);
            lock (m_lock)
            {
                m_rcvpackagelst.AddRange(lst);
            }
            for (int i = 0; i < lst.Count; i++)
            {
                KeyValuePair<string, string> pair = lst[i];
                ConsoleLog.Instance.writeInformationLog("报文解析:"+pair.Key+"="+pair.Value);
            }
            //int nPos = 4 + 32 + 4;//起始位置
            //int nTotalLen = (int)(recvarr[0] << 24) + (int)(recvarr[1] << 16) + (int)(recvarr[2] << 8) + (int)(recvarr[3]);
            //int nLen = nTotalLen - 32 - 4;
            //if (nLen < 10000 && nLen > 0)
            //{
            //    byte[] outputarr = new byte[nLen];
            //    Array.Copy(recvarr, nPos, outputarr, 0, nLen);
            //    // 解码
            //    byte[] arrDescrypt = ZlibCompress.DecompressBytes(outputarr);
            //    string outputstr = System.Text.Encoding.UTF8.GetString(arrDescrypt);
            //    // 获取指令
            //    byte[] arrcipercode = new byte[32];
            //    Array.Copy(recvarr, 4, arrcipercode, 0, 32);
            //    string cipercodestr = System.Text.Encoding.UTF8.GetString(arrcipercode).Replace("\0", "");

            //    m_rcvpackagelst.Add(new KeyValuePair<string, string>(cipercodestr, outputstr));
            //    // 
            //}
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += "0x"+bytes[i].ToString("X2") + ", ";
                }
            }
            return returnStr;
        }

        public static List<KeyValuePair<string, string>> getZipByteArr(byte[] recvarr)
        {
            List<KeyValuePair<string, string>> lst = new List<KeyValuePair<string, string>>();
            int index = 0;

            try
            {
                while (index != -1)
                {
                    int nPos = 4 + 32 + 4 + index;//起始位置
                    int nTotalLen = (int)(recvarr[index + 0] << 24) + (int)(recvarr[index + 1] << 16) + (int)(recvarr[index + 2] << 8) + (int)(recvarr[index + 3]);
                    int nLen = nTotalLen - 32 - 4;
                    if (nLen > recvarr.Length - index)
                    {// 格式不正确的报文不解析
                        byte[] temparr = new byte[recvarr.Length - index];
                        Array.Copy(recvarr, index, temparr, 0, recvarr.Length - index);
                        // 打印 出错字符串
                        string str = byteToHexStr(temparr);

                        System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
                        //String StringMessage = encoder.GetString(recvarr, index, 40);

                        ConsoleLog.Instance.writeInformationLog("无法解析的报文,字节数组=" + str+";iindex="+index);

                        return lst;
                    }
                    else if (nLen < 0)
                        break;

                    byte[] outputarr = new byte[nLen];

                    Array.Copy(recvarr, nPos, outputarr, 0, nLen);

                    byte[] cmd = new byte[32];
                    Array.Copy(recvarr, index + 4, cmd, 0, 32);
                    string cmdstr = System.Text.Encoding.Default.GetString(cmd).TrimEnd('\0');

                    // 将字节流解析为字符串
                    byte[] arrDescrypt = ZlibCompress.DecompressBytes(outputarr);
                    if (arrDescrypt != null)
                    {
                        string outputstr = System.Text.Encoding.UTF8.GetString(arrDescrypt);
                        lst.Add(new KeyValuePair<string, string>(cmdstr, outputstr));
                    }


                    if (index + nTotalLen + 4 < recvarr.Length)
                    {
                        index += nTotalLen + 4;
                    }
                    else
                        index = -1;
                }
            }
            catch (Exception ex)
            {
                // 打印 出错字符串
                string str = byteToHexStr(recvarr);

                ConsoleLog.Instance.writeInformationLog("解析时发生异常,字节数组=" + str+";index="+index);
                Console.WriteLine(ex.Message);
            }
            return lst;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        //private void Send(Socket client, String data)
        //{
        //    // Convert the string data to byte data using ASCII encoding.
        //    byte[] byteData = Encoding.ASCII.GetBytes(data);

        //    // Begin sending the data to the remote device.
        //    client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        //}

        public void Send(byte[] byteData)
        {
            try
            {
                if (m_client.Connected)
                    m_client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), m_client);
                else
                    ConsoleLog.Instance.writeInformationLog("已断开连接");
            }
            catch (Exception ex)
            {
                ConsoleLog.Instance.writeInformationLog("发送数据异常,ErrorMsg="+ex.Message);
            }
        }

        /// <summary>
        /// 发送数据完成回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                m_sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public void SendCmd(string cmd1, string param1, string param2)
        {//
            Dictionary<string, string> paralist = new Dictionary<string, string>();
            paralist.Add(param1, param2);
            Command cmd = new Command(cmd1, paralist);
            Send(cmd.outputarr);
        }

        public bool bOpen()
        {
            return !m_client.Connected;
        }
        //public static int Main(String[] args)
        //{
        //    StartClient();
        //    return 0;
        //}
    }
}