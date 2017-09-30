using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TestApp
{
    [DataContract]
    class RetInfo
    {
        [DataMember]
        public int errno { get; set; }
        [DataMember]
        public string error { get; set; }
        [DataMember]
        public retdata data { get; set; } 
    }
    [DataContract]
    class retdata
    {
        /// <summary>
        /// 数据流名称，设备范围内唯一
        /// </summary>
        [DataMember]
        public string id { get; set; }
        /// <summary>
        /// 数据流标签
        /// </summary>
        [DataMember]
        public List<string> tags { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        [DataMember]
        public string unit { get; set; }
        /// <summary>
        /// 单位符号
        /// </summary>
        [DataMember]
        public string unit_symbol { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [DataMember]
        public string create_time { get; set; }
        /// <summary>
        /// 该数据流下数据点最新值
        /// </summary>
        [DataMember]
        public string current_value { get; set; }
        /// <summary>
        /// 该数据流下数据点最新上传时间
        /// </summary>
        [DataMember]
        public string update_at { get; set; }
    }

    [DataContract]
    class RetInfo2
    {
        [DataMember]
        public int errno { get; set; }
        [DataMember]
        public string error { get; set; }
        private List<retdata> data=new List<retdata>();// { get; set; }
        [DataMember]
        public List<retdata> retlist
        {
            get{return data;}
            set { data = value; }
        }
    }
}
