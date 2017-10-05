using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestApp.fire
{
    public class TaskStep
    {
        public int id { get; set; }
        public string Cmd { get; set; }
        public string CmdStr { get; set; }
        public int time { get; set; }
    }

    public class Packet
    {
        public List<KeyValuePair<string, string>> RecvLst { get; set; }
        public TaskStep step { get; set; }
        public DateTime time { get; set; }
    }

    public class ExeTask
    {
        public Task taskInfo { get; set; }
        public List<TaskStep> stepList { get; set; }
    }
}
