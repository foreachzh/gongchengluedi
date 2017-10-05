using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TestApp.fire
{
    public class Tool
    {
        public static string PickupDataStr(string sourcestr)
        {
            // 先匹配四个括号，没有再匹配三个括号
            string regexstr = Regex.Match(sourcestr, "data\":(?<value>.*?)}}}}").Groups["value"].Value;
            if (!string.IsNullOrEmpty(regexstr))
                return regexstr + "}}";
            else
                return Regex.Match(sourcestr, "data\":(?<value>.*?)}}}").Groups["value"].Value + "}";
        }
    }
}
