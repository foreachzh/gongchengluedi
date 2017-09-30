using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TestApp.fire
{
    public class RoleCreate
    {
        public static string GetForceInfo(HttpClient2 client)
        {
            string outputstr = client.DownloadBytearr2(GlobalVal.ServerURL + "/root/gateway.action?command=player@getForceInfo", string.Empty, false);
            webAction_createrole action = (webAction_createrole)JsonManager.JsonToObject(outputstr, typeof(webAction_createrole));

            Reward jiangli = action.action.data.reward;
            /*魏:1 蜀:2 吴:3*/
            outputstr = client.Post_retbyte2(GlobalVal.ServerURL + "/root/gateway.action?command=player@setPlayerForce", 
                string.Empty, "forceId=" + jiangli.forceId);

            Console.WriteLine(outputstr);

            return outputstr;
        }
    }
}
