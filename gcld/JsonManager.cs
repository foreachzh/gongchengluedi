using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization.Json;

namespace TestApp
{
    public class JsonManager
    {
        /// <summary>
        /// 将对象转化为JSON文件
        /// </summary>
        /// <param name="objData"></param>
        /// <returns></returns>
        public static string ObjectToJson(object objData)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(objData.GetType());
            string jsonText = "";

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, objData);
                //jsonText = Encoding.UTF8.GetString(stream.ToArray());
                jsonText = Encoding.UTF8.GetString(stream.ToArray());
                jsonText = jsonText.Replace("k__BackingField", "");
            }
            return jsonText;
        }

        /// <summary>
        /// JSON数据转化为对象
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static object JsonToObject(string jsonText, Type type)
        {
            object result = null;
            //jsonText = jsonText.Replace("k__BackingField", "");
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonText)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(type);
                result = serializer.ReadObject(ms);
            }

            return result;
        }

        public static void test()
        {
            // EquipmentList objList = JsonManager.JsonToObject(sData, typeof(EquipmentList)) as EquipmentList;
        }
    }
}
