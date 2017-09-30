using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.Text.RegularExpressions;
using XRFAppPlat.Logger;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestApp
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //textBox1.Text = HttpUtility.UrlEncode(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //textBox1.Text = HttpUtility.UrlDecode(textBox1.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //textBox1.Text = Regex.Unescape(textBox1.Text);
        }
        //


        // 16进制格式string 转byte[]：

//public static byte[] GetBytes(string hexString, out int discarded)
//{            

//    discarded = 0;
//    string newString = "";
//    char c;// remove all none A-F, 0-9, charactersfor (int i=0; i<hexString.Length; i++)

//{              

//  c = hexString[i];
//    if (IsHexDigit(c))                    
//        newString += c;
//    else    
//        discarded++;            

//}// if odd number of characters, discard last character
//    if (newString.Length % 2 != 0){                
//        discarded++;                
//        newString = newString.Substring(0, newString.Length-1);
//    }

//    int byteLength = newString.Length / 2;
//    byte[] bytes = newbyte[byteLength];
//    string hex;
//    int j = 0;
//    for (int i=0; i<bytes.Length; i++)
//    {
//        hex = new String(new Char[] { newString[j], newString[j + 1] });
//        bytes[i] = HexToByte(hex);
//        j = j + 2;
//    }

//    return bytes;       
//}


        //private string byte2str(byte[] arr)
        //{
        //    string retstr = "";
        //    for (int i = 0; i < arr.Length; i++)
        //    {
        //        retstr += arr[i].ToString("X2") + " ";
        //    }
        //    return retstr;
        //}

        private void button5_Click(object sender, EventArgs e)
        {
            
        }
        Thread thread = null;
        private void button6_Click(object sender, EventArgs e)
        {
            button6.Enabled = false;
            if (!ForwardService.BStart)
            {
                button6.Text = "关闭服务";
                ForwardService.BStart = true;
                thread = new Thread(ForwardService.ExchageThread);
                thread.IsBackground = true;
                thread.Start();
                ConsoleLog.Instance.writeInformationLog("开启转发服务...");
            }
            else
            {
                ForwardService.BStart = false;
                button6.Text = "关闭服务";
                if (thread != null)
                {
                    thread.Abort();
                    thread = null;
                }
                ConsoleLog.Instance.writeInformationLog("关闭转发服务成功");
            }
            button6.Enabled = true;
        }
        // public bool bStart;

        private void button7_Click(object sender, EventArgs e)
        {
            //string teststr = "{\"errno\": 0,\"error\":\"succ\",\"data\":"+
            //    "{[\"id\":\"temperature\",\"tags\":[\"Tag1\",\"Tag2\"],\"unit\":\"celsius\","
            //    +"\"unit_symbol\":\"C\",\"create_time\":\"2017-07-11 10:22:22\","+
            //    "\"current_value\":\"1234\",\"update_at\":\"2017-07-08 10:33:38\"],[\"id\":\"humi\",\"tags\""
            //    +":[\"Tag1\",\"Tag2\"],\"unit_symbol\":\"%\",\"create_time\":"
            //    + "\"2017-07-10 10:22:22\",\"current_value\":\"456\",\"update_at\":\"2017-07-08 10:33:38\"]}}";

            string teststr2 = "{\"errno\": 0,\"error\":\"succ\",\"data\":[{\"id\":\"temperature\",\"tags\":[\"Tag1\",\"Tag2\"],\"unit\":\"celsius\",\"unit_symbol\":\"C\",\"create_time\":\"2017-07-11 10:22:22\",\"current_value\":\"1234\",\"update_at\":\"2017-07-08 10:33:38\"},{\"id\":\"humi\",\"tags\":[\"Tag1\",\"Tag2\"],\"unit_symbol\":\"%\",\"create_time\":\"2017-07-10 10:22:22\",\"current_value\":\"456\",\"update_at\":\"2017-07-08 10:33:38\"}]}";
            RetInfo2 result = JsonManager.JsonToObject(teststr2, typeof(RetInfo2)) as RetInfo2;
            if (result.retlist != null)
            {
                for (int i = 0; i < result.retlist.Count; i++)
                {
                    retdata data = result.retlist[i];
                    ConsoleLog.Instance.writeInformationLog(
                        string.Format("id={0};update_at={1};create_time={2}; current_value={3}",
                        data.id, data.update_at, data.create_time, data.current_value));
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string teststr = "{\"errno\":0,\"data\":{\"update_at\":\"2017-07-08 16:50:33\",\"id\":\"ds_test\",\"create_time\":\"Date(2017-07-07 11:32:52)\",\"current_value\":\"2A3931303332237872667A6E73706A2373616D706C652A20\"},\"error\":\"succ\"}";
            RetInfo result = JsonManager.JsonToObject(teststr, typeof(RetInfo)) as RetInfo;
            if (result != null && result.data != null)
                ConsoleLog.Instance.writeInformationLog("" + result.data.current_value);
            else
                ConsoleLog.Instance.writeInformationLog("返回字符串为"+teststr+";序列化失败！");
        }
        private ConsoleLog logger;
        private void Form1_Load(object sender, EventArgs e)
        {
            logger = ConsoleLog.Instance;
            logger.StartFromLog();
            logger.MemoBox = richTextBox1;

            txtIPAddress.Text = AppSetting.GetValue("txtIPAddress");
            txtPort.Text = AppSetting.GetValue("txtPort");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
            ConsoleLog.Instance.writeInformationLog("关闭转发服务成功");
        }

        private void txtIPAddress_TextChanged(object sender, EventArgs e)
        {
            AppSetting.SetValue("txtIPAddress", txtIPAddress.Text);
        }

        private void txtPort_TextChanged(object sender, EventArgs e)
        {
            AppSetting.SetValue("txtPort", txtPort.Text);
        }
    }
}
