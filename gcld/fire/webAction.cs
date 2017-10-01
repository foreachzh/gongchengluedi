using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace TestApp.fire
{
    [DataContract]
    public class ForceList
    {
        public int forceId { get; set; }
    }
    [DataContract]
    public class Reward
    {
         [DataMember]
        public int forceId { get; set; }
         [DataMember]
        public string kind { get; set; }
         [DataMember]
        public int value { get; set; }
    }
    [DataContract]
    public class Info
    {
         [DataMember]
        public string playerName { get; set; }
    }
    [DataContract]
    public class Data
    {
         [DataMember]
        public ForceList[] forceList { get; set; }
         [DataMember]
        public Reward reward { get; set; }
         [DataMember]
        public Info[] info { get; set; }
    }
    [DataContract]
    public class Action
    {
         [DataMember]
        public int state { get; set; }

         [DataMember]
        public Data data { get; set; }
    }
    [DataContract]
    public class webAction_createrole
    {
         [DataMember]
        public Action action { get; set; }
    }
}
