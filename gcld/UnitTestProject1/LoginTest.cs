using TestApp.fire;
using TestApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using XRFAppPlat.Logger;
using System.Text.RegularExpressions;

namespace UnitTestProject1
{
    
    
    /// <summary>
    ///这是 LoginTest 的测试类，旨在
    ///包含所有 LoginTest 单元测试
    ///</summary>
    [TestClass()]
    public class LoginTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        // 
        //编写测试时，还可使用以下特性:
        //
        //使用 ClassInitialize 在运行类中的第一个测试前先运行代码
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //使用 ClassCleanup 在运行完类中的所有测试后再运行代码
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //使用 TestInitialize 在运行每个测试前先运行代码
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //使用 TestCleanup 在运行完每个测试后运行代码
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///LoginGame 的测试
        ///</summary>
        [TestMethod()]
        public void LoginGameTest()
        {
            try
            {
                Login target = new Login(); // TODO: 初始化为适当的值
                target.Uname = "foreach";
                target.Passwd = "8891129zh";
                // ExcuteState expected = null; // TODO: 初始化为适当的值
                ExcuteState actual;
                actual = target.LoginGame();
                if (actual.State == IdentityCode.Fail)
                    return;

                bool bSuccess = target.getUserkeyFromCookie();
                bSuccess = target.getPkeyFromCookie();
                RoleSel role = new RoleSel(target.Client);
                role.TasktxtPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "自动任务列表.txt";
                role.StrServerName = target.StrServerName;
                actual = role.getPlayerList();
                if (actual.State == IdentityCode.Fail)
                    return;

                if (role.Rolelist.Count > 0)
                {
                    webRole roler = role.Rolelist[0];
                    actual = role.getPlayerInfo(roler);
                    if (actual.State == IdentityCode.Fail)
                        return;
                }


                bool bret = role.getServerIP();
                if (!bret)
                    return;


                Dictionary<string, string> lst = new Dictionary<string, string>(); // TODO: 初始化为适当的值
                lst.Add("userkey", target.Userkey);
                Command cmd = new Command("login_user", lst);
                byte[] sendarr = cmd.outputarr;

                RoleInfoStoreClient client = new RoleInfoStoreClient(role.ServerAddr, role.ServerPort);
                client.Send(sendarr);
                client.SendDone.WaitOne();

                client.Receive();
                client.ReceiveDone.WaitOne();

                // 

                int i = 0;
                while (i++ < 5)
                {
                    Thread.Sleep(1000);
                }

                // 开启线程 发送心跳报文
                CommandList cmdlst = new CommandList(role.Roledetail.pkey2, client);
                Thread heatThread = new Thread(cmdlst.HeatBeatThread);
                heatThread.IsBackground = true;
                heatThread.Start();

                // CommandList.SendGetBuildingInfo(client, 1);

                // 获取主城信息
                CommandList.GetMainCityInfo(client);

                // Thread.Sleep(100);
                Command cmd4 = new Command(CommandList.GENERAL_GET_GENERALSIMPLEINFO2, lst);
                client.Send(cmd4.outputarr);
                client.SendDone.WaitOne();

                i = 0;
                while (i++ < 5)
                {
                    Thread.Sleep(1000);
                }
                // 获取主城信息
                client.SetJson2AreaLst();

                i = 0;
                // 获取当前任务，执行
                Task curTask = role.CurTask;
                TaskExecute tasker = new TaskExecute(client);

                while (i++ < 60)
                {
                    Thread.Sleep(1000);
                    bret = tasker.ExecuteTask(ref role);
                    if (!bret)
                    {
                        Thread.Sleep(1000 * 5);
                        break;
                    }
                }

                client.PrintInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message+";"+ex.StackTrace);
            }
            // Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }


    }
}
