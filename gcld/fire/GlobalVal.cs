using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestApp.fire
{
    public class GlobalVal
    {
        private static readonly string m_centerURL = "http://www.aoshitang.com";

        public static string CenterURL
        {
            get { return m_centerURL; }
        }

        private static readonly string m_lgnURL = "http://gc.aoshitang.com";

        public static string LgnURL
        {
            get { return GlobalVal.m_lgnURL; }
        }

        private static string m_cmdURL = "http://s1344.gc.aoshitang.com" + "/root/gateway.action?command=";

        public static string CmdURL
        {
            get { return GlobalVal.m_cmdURL; }
            set { GlobalVal.m_cmdURL = value; }
        }

        private static string m_ServerURL = string.Empty;

        public static string ServerURL
        {
            get { return GlobalVal.m_ServerURL; }
            set { GlobalVal.m_ServerURL = value; }
        }
        
    }
}
