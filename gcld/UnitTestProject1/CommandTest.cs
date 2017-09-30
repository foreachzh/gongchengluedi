using TestApp.fire;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Collections.Generic;

namespace UnitTestProject1
{
    
    
    /// <summary>
    ///这是 CommandTest 的测试类，旨在
    ///包含所有 CommandTest 单元测试
    ///</summary>
    [TestClass()]
    public class CommandTest
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
        ///Command 构造函数 的测试
        ///</summary>
        [TestMethod()]
        public void CommandConstructorTest()
        {
            try
            {
                byte[] recvarr = new byte[4];
                recvarr[0] = recvarr[1] = recvarr[2] = 0;
                recvarr[3] = (byte)92;
                int nTotalLen = (int)(recvarr[0] << 24) + (int)(recvarr[1] << 16) + (int)(recvarr[2] << 8) + (int)(recvarr[3]);
                

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
            //string cmd = "login_user"; // TODO: 初始化为适当的值
            //Dictionary<string, string> lst = new Dictionary<string,string>(); // TODO: 初始化为适当的值
            //lst.Add("userkey", "E9E35DB7386B9C611F329767B07CA10B");
            //Command target = new Command(cmd, lst);
            //Assert.Inconclusive("TODO: 实现用来验证目标的代码");
        }
    }
}
