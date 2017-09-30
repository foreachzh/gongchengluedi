using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestApp
{
    class Test
    {
        //处理当前接收到的数据包
        public static void AccepteData(byte[] data)
        {
#if DEBUG
            // writelog(ToHexString(data));
#endif
            data = RemoveEscapeChar(data);
            //校验数据完整性和合法性
            //数据长度
            int nLen = data[13];
            nLen |= (int)((((int)data[14]) << 8) & 0xFF00);
            //检查长度是否一致，不一致退出
            if (nLen + 17 > data.Length)
                return;

            //源地址
            int sourceAddress = (int)((((int)data[2]) << 24) & 0xFF000000);
            sourceAddress |= (int)((((int)data[3]) << 16) & 0xFF0000);
            sourceAddress |= (int)((((int)data[4]) << 8) & 0xFF00);
            sourceAddress |= (int)(((int)data[5]) & 0xFF);
            //设备号
            int EquimentCode = (int)((((int)data[6]) << 24) & 0xFF000000);
            EquimentCode |= (int)((((int)data[7]) << 16) & 0xFF0000);
            EquimentCode |= (int)((((int)data[8]) << 8) & 0xFF00);
            EquimentCode |= (int)(((int)data[9]) & 0xFF);

            //命令字
            int orderID = data[10];
            orderID |= (int)((((int)data[11]) << 8) & 0xFF00);

            //命令类型
            int type = data[12];

            //数据区
            byte[] arrData = null;
            if (nLen > 0)
                arrData = new byte[nLen];

            int nLimited = data.Length - 2;
            int nPos = 15;
            for (int i = 0; i < nLen; i++)
            {
                arrData[i] = data[nPos];
                nPos++;

                //如果位置已经到了最后，那么退出
                if (nPos > nLimited)
                {
                    return;
                }
            }

            //最后位置是否正确
            if (nPos != nLimited)
                return;

            //校验码
            //int crc = data[nPos];
            //crc |= (int)((((int)data[nPos + 1]) << 8) & 0xFF00);

            ////检查校验值是否一致
            //int crcResult = DataCrc.CrcCal(data, 0, data.Length - 2);
            //if (crc != crcResult)
            //    return;
        }

        public static void test1(byte[] buffer)
        {
            int nStart = 0;
            int nEnd = 0;
            int nWaitLen = 0;
            byte[] waitBuff = new byte[1024];
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0x7E)
                {
                    nStart = i;
                }
                else if (buffer[i] == 0x8E)
                {
                    nEnd = i;

                    if (nStart == -1)
                    {
                        if (nWaitLen > -1 && (nWaitLen + nEnd) > 18)
                        {
                            //处理上一个未完成数据包
                            byte[] arrData = new byte[nEnd + nWaitLen];
                            if (nWaitLen > 0)
                                Array.Copy(waitBuff, 0, arrData, 0, nWaitLen);
                            if (nEnd > 0)
                                Array.Copy(buffer, 0, arrData, nWaitLen, nEnd);
                            AccepteData( arrData);
                            nWaitLen = -1;
                            nEnd = -1;
                        }
                        else
                        {
                            //将不完整或长度不够的数据包丢弃
                            nWaitLen = -1;
                            nEnd = -1;
                            continue;
                        }
                    }
                    else
                    {
                        //处理当前数据包
                        if (nEnd - nStart > 18)     //数据包长度小于，那么数据丢弃
                        {
                            byte[] arrData = new byte[nEnd - nStart - 1];
                            Array.Copy(buffer, nStart + 1, arrData, 0, nEnd - nStart - 1);
                            AccepteData(arrData);
                        }
                        nStart = -1;
                        nEnd = -1;
                    }
                }
            }
        }

        //去除转义
        public static byte[] RemoveEscapeChar(byte[] bytes)
        {
            int nLen = bytes.Length;
            int nDataLen = nLen;
            for (int i = 0; i < nLen; i++)
            {
                if (bytes[i] == 0x5E)
                {
                    nDataLen--;
                }
            }

            byte[] result = new byte[nDataLen];
            nDataLen = 0;
            for (int i = 0; i < nLen; i++)
            {
                if (bytes[i] == 0x5E)
                {
                    if (bytes[i + 1] == 0x5D)
                        result[nDataLen] = 0x5E;
                    else if (bytes[i + 1] == 0x7D)
                        result[nDataLen] = 0x7E;
                    else if (bytes[i + 1] == 0x8D)
                        result[nDataLen] = 0x8E;
                    nDataLen++;
                    i++;
                }
                else
                {
                    result[nDataLen] = bytes[i];
                    nDataLen++;
                }
            }
            return result;
        }
    }
}
