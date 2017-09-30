using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace TestApp.fire
{
    public class sockets:IDisposable
    {
        private string m_IPAddr = string.Empty;
        private int m_nPort = 0;
        TcpClient m_tcpClient = new TcpClient();
        public sockets(string IpAddr, int nPort)
        {
            m_IPAddr = IpAddr;
            m_nPort = nPort;

            m_tcpClient.Connect(IPAddress.Parse(m_IPAddr), m_nPort);
        }

        public bool SendByteArr(byte[] bytearr, ref string retstr)
        {
            try
            {
                NetworkStream ns = m_tcpClient.GetStream();

                if (ns.CanWrite)
                {
                    ns.Write(bytearr, 0, bytearr.Length);
                    ns.Flush();

                    byte[] recvarr = ReceiveByteArray(ns);
                    // 获取待解码字节流
                    int nPos = 4 + 32 + 4;//起始位置
                    int nTotalLen = (int)(recvarr[0] << 24) + (int)(recvarr[1] << 16) + (int)(recvarr[2] << 8) + (int)(recvarr[3]);
                    int nLen = nTotalLen - 32-4;
                    byte[] outputarr = new byte[nLen];
                    Array.Copy(recvarr, nPos, outputarr, 0, nLen);
                    // 解码
                    byte[] arrDescrypt = ZlibCompress.DecompressBytes(outputarr);
                    string outputstr = System.Text.Encoding.UTF8.GetString(arrDescrypt);
                    retstr = outputstr;
                }
                else
                {
                    Console.WriteLine("不能写入数据流");
                    //Console.WriteLine("You cannot write data to this stream.");
                    m_tcpClient.Close();

                    // Closing the tcpClient instance does not close the network stream.
                    ns.Close();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("转发数据失败,Reason=" + ex.Message);
            }
            return false;
        }

        internal byte[] ReceiveByteArray(NetworkStream stream)
        {
            try
            {
                int bufferlen = 4096;
                byte[] resultbyte = new byte[bufferlen];

                int offset = 0, bytesread = 0;
                while (offset < bufferlen)
                {
                    bytesread = stream.Read(resultbyte, offset, bufferlen - offset);
                    if (bytesread == 0)
                        throw new Exception("网络异常断开，数据读取不完整。");
                    else
                        offset += bytesread;
                }
                return resultbyte;
            }
            catch (Exception)
            {
                throw;
            }
        }  
        
        public void Dispose()
        {
            m_tcpClient.Close();
        }
    }
}
