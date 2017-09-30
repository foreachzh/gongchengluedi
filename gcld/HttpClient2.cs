using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestApp.fire;

namespace TestApp
{
    public class HttpClient2: HttpClient
    {
        public string DownloadBytearr2(string URL, string refer, bool newCookie)
        {
            byte[] arrOutput = DownloadBytearr(URL, string.Empty, false);
            byte[] arrDescrypt = ZlibCompress.DecompressBytes(arrOutput);
            string outputstr = System.Text.Encoding.UTF8.GetString(arrDescrypt);
            return outputstr;
        }

        public string Post_retbyte2(string URL, string refer, string postingdata)
        {
            byte[] arrOutput = Post_retbyte(URL, refer, postingdata);
            byte[] arrDescrypt = ZlibCompress.DecompressBytes(arrOutput);
            string outputstr = System.Text.Encoding.UTF8.GetString(arrDescrypt);
            return outputstr;
        }
    }
}
