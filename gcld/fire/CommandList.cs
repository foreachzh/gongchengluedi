using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestApp.fire
{
    public class CommandList
    {
        public static string HeatBeat = "player@game";// 报文格式:...Wplayer@game.........................pkey=1503404221152_95b1143de12a8054ea752e263d62578b
        public static string HEART_BEAT_TEST= "player@ttest";
        public static string HEART_BEAT_TEST2 = "player@ltest";

        /// <summary>
        /// 创角
        /// selfService("player@setPlayerForce","{\"forceId\":" + forceId + "}");
        /// </summary>
        public static string FORCES_GET_FORCEINFO = "player@getForceInfo";

        /// <summary>
        /// Common.service.send(RmList.MC_GET_BUILDING_INFO,{"type":param1},this.result);
        /// ...*building@getBuildingInfo............type=1
        /// </summary>
        public static string MC_GET_BUILDING_INFO = "building@getBuildingInfo";// 

        public static string GENERAL_GET_GENERALSIMPLEINFO2 = "general@getGeneralSimpleInfo2";
        // ...$general@getGeneralSimpleInfo2.......

        public static string MC_UPGRADE_BUILDING = "building@upgradeBuilding";// 民居升级 ...0building@upgradeBuilding...........9buildingId=1
        public static string xxx = "incense@getIncenseInfo";// 木材祭祀

        /*
        "curTask":{"tasks":[{"type":1,"state":1,"taskId":2,"group":0,"index":0,"taskName":"安居乐业1","introShort":"升级民居一<br>等级达到2级","introLong":"升级民居一等级达到2级","processStr":"","requestCompleted":false,"markTrace":"0","iosMarktrace":"0","newTrace":"2","areaId":1,"pic":"task3","plot":""}]},
        "curTask":{"tasks":[{"type":1,"state":1,"taskId":3,"group":0,"index":0,"taskName":"安居乐业2","introShort":"升级民居一<br>等级达到3级","introLong":"升级民居一等级达到3级","processStr":"","requestCompleted":false,"markTrace":"0","iosMarktrace":"0","newTrace":"3","areaId":1,"pic":"task3","plot":""}]}
        // ...0building@upgradeBuilding............buildingId=1
        // 
        // ...:task@finishTask....................=group=0&type=1&index=0
        // 问题:任务花了多长时间？
        // 问题:怎么知道是建房子，而不是伐木场？
         */
        /// <summary>
        ///          Common.service.send(RmList.FINISHI_TASK,{ "type":_loc2_.taskType, "group":_loc2_.group, "index":_loc2_.index },this.result);
        /// </summary>
        public static string FINISHI_TASK = "task@finishTask";// ...:task@finishTask....................Btype=1&index=0&group=0

        public static string GUILD_TASK = "task@guideUpdate";//...-task@guideUpdate...................CguideId=4
        // ...$building@getMainCityInfo............

        public CommandList(string pskey, AsynchronousClient client)
        {
            m_client = client;
            m_pkey2 = pskey;
        }
        
        public static CmdType getCurCmdType(string cmd)
        {
            CmdType CommandType = CmdType.Unknown;
            if (cmd == "login_user")
                CommandType = CmdType.登录;
            else if (cmd == "building@upgradeBuilding")
                CommandType = CmdType.升级民居;
            else if (cmd == "player@game")
                CommandType = CmdType.心跳1;
            else if (cmd == CommandList.HEART_BEAT_TEST)
                CommandType = CmdType.心跳2;

            return CommandType;
        }

        public static void SendGetBuildingInfo(AsynchronousClient client, int ntype)
        {
            Dictionary<string, string> lst = new Dictionary<string,string>();
            lst.Add("type", ntype.ToString());
            Command cmd3 = new Command(CommandList.MC_GET_BUILDING_INFO, lst);
            client.Send(cmd3.outputarr);
            client.SendDone.WaitOne();
        }

        public static void SendHeatBeat(AsynchronousClient client, string pkey2)
        {
            Dictionary<string, string> lst = new Dictionary<string, string>();
            lst.Clear();
            lst.Add("pkey", pkey2);
            Command cmd2 = new Command(CommandList.HeatBeat, lst);

            client.Send(cmd2.outputarr);
            client.SendDone.WaitOne();

            // lst.Clear();
            // cmd2 = new Command(CommandList.HEART_BEAT_TEST2, lst);

            // client.Send(cmd2.outputarr);
            // client.SendDone.WaitOne();
        }

        private AsynchronousClient m_client;
        private string m_pkey2;

        public string Pkey2
        {
            get { return m_pkey2; }
            set { m_pkey2 = value; }
        }
        public AsynchronousClient Client
        {
            get { return m_client; }
            set { m_client = value; }
        }

        public static void FinishTask(AsynchronousClient client, int group, int type, int index)
        {
            Dictionary<string, string> lst = new Dictionary<string, string>();
            lst.Clear();
            lst.Add("group", group.ToString());
            lst.Add("type", type.ToString());
            lst.Add("index", index.ToString());
            Command cmd2 = new Command(CommandList.HeatBeat, lst);

            client.Send(cmd2.outputarr);
        }

        public static void GetMainCityInfo(AsynchronousClient client)
        {
            Command cmd = new Command("building@getMainCityInfo", "");
            client.Send(cmd.outputarr);
        }

        public void HeatBeatThread()
        {
            while (true)
            {
                if (m_client != null && !string.IsNullOrEmpty(m_pkey2))
                {
                    SendHeatBeat(m_client, m_pkey2);
                }
                Thread.Sleep(30 * 1000);
            }
        }
    }



    public enum CmdType { Unknown, 升级民居, 登录, 心跳1, 心跳2 };
}
