using TestApp.fire;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TestApp;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest3
    {
        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                string teststr = "{\"action\":{\"state\":1,\"data\":{\"forceList\":[{\"forceId\":1},{\"forceId\":2},{\"forceId\":3}],\"reward\":{\"forceId\":1,\"kind\":\"金币\",\"value\":50},\"info\":[{\"playerName\":\"丿无极灬含蕊\"},{\"playerName\":\"九花向雪\"},{\"playerName\":\"58469\"},{\"playerName\":\"hgjhg\"},{\"playerName\":\"fdgsf\"},{\"playerName\":\"大幂幂\"},{\"playerName\":\"武神降臨關二爺\"},{\"playerName\":\"洪孤菱\"},{\"playerName\":\"永恒゛傲\"},{\"playerName\":\"fdtr\"}]}}}";
                webAction_createrole action = (webAction_createrole)JsonManager.JsonToObject(teststr, typeof(webAction_createrole));
                
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }
    }
}
