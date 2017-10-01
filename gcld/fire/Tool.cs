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
            return Regex.Match(sourcestr, "data\":(?<value>.*?)}}}").Groups["value"].Value + "}";
        }
    }
}
