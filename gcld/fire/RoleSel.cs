using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XRFAppPlat.Logger;
// using zlib;

namespace TestApp.fire
{
    public class RoleSel
    {
        private HttpClient2 m_client;
        public HttpClient2 Client
        {
            get { return m_client; }
            set { m_client = value; }
        }
        private List<webRole> m_rolelist = new List<webRole>();

        public List<webRole> Rolelist
        {
            get { return m_rolelist; }
            set { m_rolelist = value; }
        }

        private webDetailRole m_roledetail;
        public webDetailRole Roledetail
        {
            get { return m_roledetail; }
            set { m_roledetail = value; }
        }

        private Task m_curTask;
        public Task CurTask
        {
            get { return m_curTask; }
            set { m_curTask = value; }
        }

        public Area[] AreaList { get; set; }

        private string m_tasktxtPath;
        public string TasktxtPath
        {
            get { return m_tasktxtPath; }
            set { m_tasktxtPath = value; }
        }

        /*
         http://s1344.gc.aoshitang.com/root/gateway.action?command=player@getPlayerList
         */

        public RoleSel(HttpClient2 httpclient)
        {
            m_client = httpclient;
        }

        public ExcuteState getPlayerList()
        {
            ExcuteState state = new ExcuteState();
            try
            {
                string jsontext = null;
                //byte[] arrOutput = m_client.DownloadBytearr(GlobalVal.CmdURL + "player@getPlayerList", string.Empty, false);
                //byte[] arrDescrypt = ZlibCompress.DecompressBytes(arrOutput);
                while (true)
                {
                    string outputstr = m_client.DownloadBytearr2(GlobalVal.CmdURL + "player@getPlayerList", string.Empty, false);

                    jsontext = Regex.Match(outputstr, "\\[(?<value>.*?)\\]").Groups["value"].Value;

                    if (string.IsNullOrEmpty(jsontext))
                    {// 创角
                        RoleCreate.GetForceInfo(m_client);
                    }
                    else
                        break;
                }

                // 目前只有一个角色
                webRole webrole = (webRole)JsonManager.JsonToObject(jsontext, typeof(webRole));
                if (webrole != null)
                    Rolelist.Add(webrole);
                else
                {
                    state.Description = "getPlayerList() 转化json为对象失败, json=" + jsontext;
                    return state;
                }

                state.State = IdentityCode.Success;
            }
            catch (Exception ex)
            {
                state.Description = "发生异常="+ex.Message+";\r\n"+ex.StackTrace;
                return state;
            }
            return state;
        }

        public ExcuteState getPlayerInfo(webRole role)
        {
            ExcuteState state = new ExcuteState();
            try
            {
                string URL = GlobalVal.ServerURL + "/root/gateway.action";
                string postdata = "command=player%40getPlayerInfo&version=11%2E01%2E17%2E1&token=1&info=3391&playerId="+role.playerId;

                string outputstr = m_client.Post_retbyte2(URL, string.Empty, postdata);
                ConsoleLog.Instance.writeInformationLog(outputstr);

                string jsontest = Regex.Match(outputstr, "player\":(?<value>.*?)\\},").Groups["value"].Value+"}";
                webDetailRole detailinfo = (webDetailRole)JsonManager.JsonToObject(jsontest, typeof(webDetailRole));
                m_roledetail = detailinfo;

                jsontest = Regex.Match(outputstr, "curTask\":(?<value>.*?)\\]\\},").Groups["value"].Value + "]}";
                webCurTask taskInfo = (webCurTask)JsonManager.JsonToObject(jsontest, typeof(webCurTask));
                if (taskInfo.tasks != null && taskInfo.tasks.Count() > 0)
                {
                    m_curTask = taskInfo.tasks[0];
                    // 把信息写入配置文件
                    TaskObj task = TaskObj.getCurTask(m_curTask);
                    // 把任务写入txt
                    // 再获取一次角色列表
                    if (!string.IsNullOrEmpty(TasktxtPath) && File.Exists(TasktxtPath)&&task != null)
                    {
                        List<TaskObj> tasklst = null;
                        TaskObj.getTasklistFromTxt(TasktxtPath, ref tasklst);
                        // 搜索列表
                        TaskObj tobj = tasklst.Find( delegate(TaskObj user){  return user.TaskName == task.TaskName;  });
                        if (tobj == null)
                        {//写入文档
                            TaskObj.WriteData2File(TasktxtPath, task.Obj2String());
                        }
                    }
                }
                state.State = IdentityCode.Success;
            }
            catch (Exception ex)
            {
                state.Description = "发生异常=" + ex.Message + ";\r\n" + ex.StackTrace;
                return state;
            }
            return state;
        }

        private string m_serverAddr;

        public string ServerAddr
        {
            get { return m_serverAddr; }
            set { m_serverAddr = value; }
        }
        private int m_serverPort;

        public int ServerPort
        {
            get { return m_serverPort; }
            set { m_serverPort = value; }
        }
        public bool getServerIP()
        {
            string xmlURL = GlobalVal.ServerURL + "/Config.xml";

            string retstr = m_client.DownloadString(xmlURL, string.Empty);

            string serverstr = Regex.Match(retstr, "socketServiceUrl value=\"(?<value>.*?)\"").Groups["value"].Value;
            if (string.IsNullOrEmpty(serverstr))
                return false;

            string[] lst = serverstr.Split(':');
            if (lst.Length != 2)
                return false;

            m_serverAddr = lst[0];
            m_serverPort = Int32.Parse(lst[1]);
            return true;
        }


    }

    public class TaskObj
    {
        public string TaskName { get; set; }
        public string TaskDetail { get; set; }
        public string CmdStr { get; set; }
        public string TargetStr { get; set; }
        public string TargetID { get; set; }

        public TaskObj(string taskName, string detail, string cmdstr, string Target)
        {
            TaskName = taskName;
            TaskDetail = detail;
            CmdStr = cmdstr;
            TargetStr = Target;
        }

        public TaskObj(string taskName, string detail, string cmdstr, string Target, int id)
        {
            TaskName = taskName;
            TaskDetail = detail;
            CmdStr = cmdstr;
            TargetStr = Target;
            TargetID = id.ToString();
        }

        public string Obj2String()
        {
            if(string.IsNullOrEmpty(TargetID) )
                return TaskName + "-" + TaskDetail + "-" + CmdStr + "，" + TargetStr;
            return TaskName + "-" + TaskDetail + "-" + CmdStr + "，" + TargetStr+","+TargetID;
        }

        public static TaskObj ConvertStr2Task(string str)
        {
            string[] lst = str.Split('-');
            if (lst.Length < 3)
                return null;

            string[] lst2 = lst[2].Split(',');
            string targetstr = "";
            if (lst2.Length <= 2)
            {
                targetstr = lst2[1];
                TaskObj task = new TaskObj(lst[0], lst[1], lst2[0], targetstr);
                return task;
            }
            else if (lst2.Length == 3)
            {
                targetstr = lst2[1];
                TaskObj task = new TaskObj(lst[0], lst[1], lst2[0], lst2[1], Int32.Parse(lst2[2]));
                return task;
            }
            return null;
        }

        public static TaskObj getCurTask(Task task)
        {
            if (task.taskName.Contains("安居乐业"))
            {
                TaskObj curTask = new TaskObj(task.taskName, task.introLong, "building@upgradeBuilding", "buildingId", task.areaId);
                return curTask;
            }
            else if (task.taskName.Contains("经济发展"))
            {
 
            }
            return null;
        }

        public static bool getTasklistFromTxt(string taskPath, ref List<TaskObj> lst)
        {
            if(string.IsNullOrEmpty(taskPath)||!File.Exists(taskPath))
                return false;

            if (lst == null)
                lst = new List<TaskObj>();
            else
                lst.Clear();

            string[] strlst  = File.ReadAllLines(taskPath);
            for (int i = 0; i < strlst.Length; i++)
            {
                TaskObj task =ConvertStr2Task( strlst[i]);
                lst.Add(task);
            }

            return true;
        }

        public static bool WriteData2File(string filePath,string strContent)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
                {
                    file.WriteLine(strContent);// 直接追加文件末尾，换行 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("文件打开失败{0}", ex.ToString());
            }
            return true; 
        }
    }
}
