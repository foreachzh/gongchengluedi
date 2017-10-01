using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using XRFAppPlat.Logger;

namespace TestApp.fire
{
    public class RoleInfoStoreClient : AsynchronousClient
    {
        public RoleInfoStoreClient(string ServerIP, int nport)
            :base(ServerIP, nport)
        {

        }

        public void PrintBuildingInfo()
        { //push@building
            KeyValuePair<string, string> pair = m_rcvpackagelst.Find(x => { return x.Key == "push@building"; });
            if (!default(KeyValuePair<string, string>).Equals(pair))
            {
                string jsonstr = pair.Value;
                // 提取值
                string value = Tool.PickupDataStr(jsonstr);

                webOutput output = (webOutput)JsonManager.JsonToObject(value, typeof(webOutput));
                if (output == null)
                    return;

                string outputInfostr = "各材料当前产量:";
                string[] outputNameArr = { "", "银两", "木材", "粮食", "铁矿" };
                if (output.Output.OutputInfo != null)
                {
                    IList<OutputInfo> lst = output.Output.OutputInfo;
                    foreach (OutputInfo outinfo in lst)
                    {
                        outputInfostr += string.Format("{0}：{1}\t", outputNameArr[outinfo.BuildingType], outinfo.RealValue);
                    }
                }

                ConsoleLog.Instance.writeInformationLog(outputInfostr);
            }
            else
                ConsoleLog.Instance.writeInformationLog("没有产出信息");
        }

        public string getUserKey()
        {
            KeyValuePair<string, string> pair = m_rcvpackagelst.Find(obj => obj.Key == "login_user");
            if (default(KeyValuePair<string, string>).Equals(pair))
                return "";
            return pair.Value;
        }

        public void WriteDataLog()
        {
            foreach (var item in m_rcvpackagelst)
            {
                ConsoleLog.Instance.writeInformationLog(item.Key + "=" + item.Value);
            }
        }

        public Area[] m_AreaList { get; set; }
        private webBuildingData m_Farm;

        public webBuildingData Farm
        {
            get { return m_Farm; }
            set { m_Farm = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ntype">=1:银两;=2:木材;=3:粮食;=4:铁矿;=5:兵营出兵速度</param>
        /// <returns></returns>
        public int GetCurrentOutputByType(int ntype)
        {
            if (m_Farm != null)
                return m_Farm.totalOutput[ntype-1].output;
            // 如果
            if (m_AreaList == null)
                return -1;

            if (ntype <= 0 || ntype > 5)
                return -1;

            if (ntype >= m_AreaList.Count())
                return -1;

            return m_AreaList[ntype - 1].totalOutput[0].output;
        }

        public int GetIdbyType(int ntype)
        {
            if (m_Farm != null)
                return m_Farm.buildings[ntype - 1].id;

            if (m_AreaList == null)
                return -1;


            if (ntype <= 0 || ntype > 5)
                return -1;

            if (ntype >= m_AreaList.Count())
                return -1;

            return m_AreaList[ntype - 1].id;
        }

        public void SetJson2AreaLst()
        {
            KeyValuePair<string, string> pair1 = m_rcvpackagelst.Find(obj => obj.Key == "building@getMainCityInfo");
            if (!default(KeyValuePair<string, string>).Equals(pair1))
            {
                string maininfo = Regex.Match(pair1.Value, "data\":(?<value>.*?)}}").Groups["value"].Value;
                webMainCityData data = (webMainCityData)JsonManager.JsonToObject(maininfo, typeof(webMainCityData));
                Area[] areaArr = (Area[])data.areas;
                m_AreaList = areaArr;
            }
        }

        public string GetBuildingInfo()
        {
            KeyValuePair<string, string> pair = m_rcvpackagelst.Find(obj => obj.Key == "building@getBuildingInfo");
            if (!default(KeyValuePair<string, string>).Equals(pair))
                return Tool.PickupDataStr( pair.Value);

            return "";
        }

        public webBuildingData GetBuildingInfo2()
        {
            string str = GetBuildingInfo();
            if(string.IsNullOrEmpty(str))
                return null;

            webBuildingData build = (webBuildingData)JsonManager.JsonToObject(str, typeof(webBuildingData));
            return build;
        }

        public bool RemoveKey(string key)
        {
            KeyValuePair<string, string> pair = m_rcvpackagelst.Find(obj => obj.Key == key);
            if (!default(KeyValuePair<string, string>).Equals(pair))
                m_rcvpackagelst.Remove(pair);

            return true;
        }

        public void HeatbeatThread()
        {
            while (true)
            {
                Thread.Sleep(30 * 1000);
                CommandList.SendHeatBeat(this, getUserKey());
            }
        }
    }
}
