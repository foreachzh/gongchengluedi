using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using XRFAppPlat.Logger;

namespace TestApp.fire
{
    public class TaskExecute
    {
        private RoleInfoStoreClient m_client = null;
        public TaskExecute(RoleInfoStoreClient client)
        {
            m_client = client;
        }
        private List<ExecutingTask> m_tasklist = new List<ExecutingTask>();
        public bool ExecuteTask(RoleSel role,  string txtpath, ref Task updatetask)
        {
            // 从自动任务文档中查找解决方案
            //List<TaskObj> lst = null;
            //TaskObj.getTasklistFromTxt(txtpath, ref lst);
            //TaskObj tobj = lst.Find(delegate(TaskObj user) { return user.TaskName == task.taskName; });

            //Dictionary<string, string> querylst = new Dictionary<string,string>();
            //if (!string.IsNullOrEmpty(tobj.TargetID))
            //    querylst.Add(tobj.TargetStr, tobj.TargetID);
            //else
            //    querylst.Add(tobj.TargetStr, task.areaId.ToString());
            Task task = role.CurTask;
            try
            {
                if (task.taskName == "经济发展")
                {
                    
                    string buildingstr = m_client.GetBuildingInfo();
                    if(!string.IsNullOrEmpty(buildingstr))
                    {
                        m_client.RemoveKey("building@getBuildingInfo");
                        webBuildingData buildingData =
                            (webBuildingData)JsonManager.JsonToObject(buildingstr, typeof(webBuildingData));

                        if (buildingData.totalOutput[0].output < 500)
                        {
                            // 查看当前产出是否>预期产出
                            Building[] bldarr = (Building[])buildingData.buildings;
                            List<Building> bldlst = bldarr.ToList();
                            // 获取非 银库的建筑
                            List<Building> bdlst = bldlst.FindAll(delegate(Building build) { return build.resType == 1; });
                            bdlst.Sort(delegate(Building x, Building y)
                            {
                                return x.lv.CompareTo(y.lv);
                            });
                            foreach (Building build in bdlst)
                            {
                                ExecutingTask mybld = m_tasklist.Find(delegate(ExecutingTask task1) { return task1.BuildInfo.name == build.name; });
                                if (build.upgrade.upgradeEnable && mybld == null)// 允许升级
                                {// 查看各材料是否够用
                                    Dictionary<string, string> paralist = new Dictionary<string, string>();
                                    paralist.Add("buildingId", build.id.ToString());
                                    Command cmd = new Command(CommandList.MC_UPGRADE_BUILDING, paralist);
                                    m_client.Send(cmd.outputarr);
                                    m_tasklist.Add(new ExecutingTask(build, build.upgrade.time, DateTime.Now));
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // 完成任务
                            CommandList.FinishTask(m_client, task.group, task.type, task.index);
                            return true;
                        }
                    }
                    else
                    {// 发送消息 获取当前建筑状态
                        CommandList.SendGetBuildingInfo(m_client, 1);
                        Thread.Sleep(1000);
                    }
                }
                else if (task.taskName == "消灭土匪")
                {// 怎样知道任务是否完成
                    //KeyValuePair<string, string> pair1 = m_client.RecvLst.Find(obj => obj.Key == "push@task");
                    //if (default(KeyValuePair<string, string>).Equals(pair1))
                    //{
                    //    // 完成任务
                    //    CommandList.FinishTask(m_client, task.group, task.type, task.index);
                    //    Thread.Sleep(1000 * 3);
                    //    return true;
                    //}
                    //else
                    //{// 更新当前任务
                    //    ConsoleLog.Instance.writeInformationLog(pair1.Value);
                    //}
                    // 会pushTask，报文如下:
                    // ....push@task...........................x.mQ;K.@..+2.
                }
                else if(task.taskName.Contains("伐木造林1"))
                {
                    int nWoodOutput = m_client.GetCurrentOutputByType(2);
                    if (nWoodOutput == -1)
                    {
                        // 查看type是否=2
                        int id = m_client.GetIdbyType(2);
                        CommandList.SendGetBuildingInfo(m_client, id);
                        Thread.Sleep(1000 * 3);
                        return false;
                    }
                    else if (nWoodOutput < 70)
                    {
                        // 查看是否有木材产出信息
                        // 查看 树林信息，没有则发送
                        string buildinfo = m_client.GetBuildingInfo();
                        if (string.IsNullOrEmpty(buildinfo))
                        {
                            int id = m_client.GetIdbyType(2);
                            CommandList.SendGetBuildingInfo(m_client, id);
                            Thread.Sleep(1000 * 3);
                            m_client.Farm = m_client.GetBuildingInfo2();
                            return false;
                        }
                        else
                        {
                            int nSleep=0;
                            //树林信息
                            ConsoleLog.Instance.writeInformationLog("林场信息=" + buildinfo);
                            webBuildingData wdInfo = (webBuildingData)JsonManager.JsonToObject(buildinfo, typeof(webBuildingData));
                            foreach (Building build in wdInfo.buildings)
                            {
                                if (build.type == 0)
                                {
                                    m_client.SendCmd(CommandList.MC_UPGRADE_BUILDING, "buildingId", build.id.ToString());
                                    nSleep = build.upgrade.time;
                                }
                            }

                            Thread.Sleep(nSleep);
                            
                            int id = m_client.GetIdbyType(2);
                            CommandList.SendGetBuildingInfo(m_client, id);
                            Thread.Sleep(1000 * 3);
                            m_client.Farm = m_client.GetBuildingInfo2();
                            return false;
                        }
                    }
                    else
                    {// 完成任务 
                        CommandList.FinishTask(m_client, task.group, task.type, task.index);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Instance.writeInformationLog("异常信息="+ex.Message);
            }
            return false;
        }
    }
    public class ExecutingTask{
        public Building BuildInfo{get;set;}
        public int Delay{get;set;}
        public DateTime Start{get;set;}
        public ExecutingTask(Building build, int delay, DateTime now)
        {
            BuildInfo = build;
            Delay = delay;
            Start = now;
        }
    }
}
