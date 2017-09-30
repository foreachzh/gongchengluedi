using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;

namespace TestApp
{
    public class HttpClient
        {
            private int _maxtimeout;
            private bool addCookie;
            private bool allowAutoRedirect;
            private System.Net.CookieContainer cc;
            private CookieCollection cookies;
            private Encoding encoder;
            private NameValueCollection headerNameValue;
            private bool isPhoneClient;
            private bool keepalive;
            private bool needAddCookieRequest;
            private bool needGzip;
            private string requestCookie;
            private bool requestCookieDomain;
            private bool requestCookiePath;
            private string responseHeaderData;
            private string responseuri;
            private string url;
            private string userAgent;
            private string acceptLanguage;
            public HttpStatusCode statucode;

            public HttpClient()
            {
                this.addCookie = true;
                this.needGzip = true;
                this.needAddCookieRequest = true;
                this.headerNameValue = new NameValueCollection();
                //xp ie: Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E)
                // win7 ie: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko
                // chrome: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.112 Safari/537.36
                this.userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.112 Safari/537.36";
                this.cc = new System.Net.CookieContainer();
                this.cookies = new CookieCollection();
                this._maxtimeout = 0x7530;
                ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(
                    ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(this.CheckValidationResult));
                this.encoder = Encoding.Default;
                this.keepalive = true;
                this.acceptLanguage = "en-us,zh-cn;q=0.7,zh;q=0.3";
            }

            public string AcceptLanguage
            {
                set { this.acceptLanguage = value; }
                get { return this.acceptLanguage; }
            }

            public HttpClient(string _url)
                : this()
            {
                this.url = _url;
            }

            public void AddOneCookie(Cookie cookie)
            {
                lock (this.cookies.SyncRoot)
                {
                    this.cookies.Add(cookie);
                }
            }

            public void AddRequestHeader(string name, string value)
            {
                foreach (string str in this.headerNameValue.AllKeys)
                {
                    if (str.Equals(name))
                    {
                        this.headerNameValue.Remove(name);
                        break;
                    }
                }
                this.headerNameValue.Add(name, value);
            }

            private void appendcookie(string url)
            {
                if (!string.IsNullOrEmpty(this.requestCookie))
                {
                    System.Text.RegularExpressions.Match match = null;
                    string[] strArray = this.requestCookie.Split(new char[] { ';' });
                    if (strArray.Length > 0)
                    {
                        lock (this.cookies.SyncRoot)
                        {
                            string text1 = strArray[0];
                            foreach (string str in strArray)
                            {
                                if (!string.IsNullOrEmpty(str))
                                {
                                    match = Regex.Match(str, "(?<name>[^=]+)=(?<value>[^=].+)");
                                    if (match.Success)
                                    {
                                        Cookie cookie = new Cookie(match.Groups["name"].Value.Trim(), match.Groups["value"].Value.Trim());
                                        if (!string.IsNullOrEmpty(url))
                                        {
                                            Uri uri = new Uri(url);
                                            this.cc.Add(uri, cookie);
                                        }
                                        this.cookies.Add(cookie);
                                    }
                                }
                            }
                            return;
                        }
                    }
                    if (!string.IsNullOrEmpty(this.requestCookie))
                    {
                        match = Regex.Match(this.requestCookie, "(?<name>[^=]+)=(?<value>[^=].+)");
                        if (match.Success)
                        {
                            Cookie cookie2 = new Cookie(match.Groups["name"].Value.Trim(), match.Groups["value"].Value.Trim());
                            if (!string.IsNullOrEmpty(url))
                            {
                                Uri uri2 = new Uri(url);
                                this.cc.Add(uri2, cookie2);
                            }
                            lock (this.cookies.SyncRoot)
                            {
                                this.cookies.Add(cookie2);
                            }
                        }
                    }
                }
            }

            public bool CheckValidationResult(object obj, X509Certificate ceft, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }
                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    if (string.Equals(status.Status.ToString(), "NotTimeValid", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }

            public void ClearCookie()
            {
                lock (this.cookies.SyncRoot)
                {
                    this.cc = new System.Net.CookieContainer();
                    this.cookies = new CookieCollection();
                }
            }

            private List<KeyValuePair<string, string>> ConvertDicToNvc(Dictionary<string, string> postingData)
            {
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
                foreach (string str in postingData.Keys)
                {
                    list.Add(new KeyValuePair<string, string>(str, postingData[str]));
                }
                return list;
            }

            private HttpWebRequest createJosnPostRequest(string url, string refer, byte[] bytes)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = this._maxtimeout;
                request.ReadWriteTimeout = this._maxtimeout;
                request.ServicePoint.Expect100Continue = false;
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";
                if (this.needAddCookieRequest)
                {
                    this.setRequestCookie(request);
                }
                if (!string.IsNullOrEmpty(this.requestCookie))
                {
                    this.appendcookie(url);
                }
                request.CookieContainer = this.cc;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Method = "POST";
                request.UserAgent = this.userAgent;
                request.Headers.Add("Cache-Control", "no-cache");
                request.Headers.Add("UA-CPU", "x86");
                if (this.needGzip)
                {
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "deflate");
                }
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, acceptLanguage);
                foreach (string str in this.headerNameValue)
                {
                    request.Headers.Add(str, this.headerNameValue[str]);
                }
                if (!string.IsNullOrEmpty(refer))
                {
                    request.Referer = refer;
                }
                request.ContentLength = bytes.Length;
                if (this.keepalive)
                {
                    request.KeepAlive = true;
                }
                request.AllowAutoRedirect = this.AllowAutoRedirect;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    // stream.Close();
                }
                return request;
            }

            private HttpWebRequest createPostRequest(string url, string refer, byte[] bytes)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = this._maxtimeout;
                request.ReadWriteTimeout = this._maxtimeout;
                request.ServicePoint.Expect100Continue = false;
                request.ContentType = "application/x-www-form-urlencoded";
                if (this.needAddCookieRequest)
                {
                    this.setRequestCookie(request);
                }
                if (!string.IsNullOrEmpty(this.requestCookie))
                {
                    this.appendcookie(url);
                }
                request.CookieContainer = this.cc;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Accept = "application/json,text/javascript,*/*;q=0.01";
                request.Method = "POST";
                request.UserAgent = this.userAgent;
                request.Headers.Add("Cache-Control", "no-cache");
                request.Headers.Add("UA-CPU", "x86");
                if (this.needGzip)
                {
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "deflate");
                }
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, acceptLanguage);
                foreach (string str in this.headerNameValue)
                {
                    request.Headers.Add(str, this.headerNameValue[str]);
                }
                if (!string.IsNullOrEmpty(refer))
                {
                    request.Referer = refer;
                }
                request.ContentLength = bytes.Length;
                if (this.keepalive)
                {
                    request.KeepAlive = true;
                }
                request.AllowAutoRedirect = this.AllowAutoRedirect;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    //stream.Close();
                }
                return request;
            }

            public HttpWebRequest createRequest(string url, string refer)
            {
                if (string.IsNullOrEmpty(url))
                {
                    throw new NullReferenceException("url is null");
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ServicePoint.Expect100Continue = false;
                request.Timeout = this._maxtimeout;
                request.ReadWriteTimeout = this._maxtimeout * 2;
                if (this.needAddCookieRequest)
                {
                    this.setRequestCookie(request);
                }
                if (!string.IsNullOrEmpty(this.requestCookie))
                {
                    this.appendcookie(url);
                }
                request.CookieContainer = this.CookieContainer;
                request.Credentials = CredentialCache.DefaultCredentials;

                request.Method = "GET";
                request.UserAgent = this.userAgent;
                request.Headers.Add("Cache-Control", "max-age=0");
                //request.Headers.Add("UA-CPU", "x86");
                if (this.needGzip)
                {
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "deflate");
                    //request.Headers.Add("Origin", "https://m.facebook.com");
                    //request.Headers.Add("Upgrade-Insecure-Requests", "1");
                }
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, acceptLanguage);
                foreach (string str in this.headerNameValue)
                {
                    request.Headers.Add(str, this.headerNameValue[str]);
                }
                if (this.KeepAlive)
                {
                    request.KeepAlive = true;
                }
                request.AllowAutoRedirect = this.allowAutoRedirect;
                if (!string.IsNullOrEmpty(refer))
                {
                    request.Referer = refer;
                }
                return request;
            }

            private HttpWebRequest CreateRequest(string url, string refer, List<KeyValuePair<string, string>> postingData, List<HttpUploadingFile> files)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AllowAutoRedirect = false;
                request.CookieContainer = new System.Net.CookieContainer();
                request.ServicePoint.Expect100Continue = false;
                if (this.needAddCookieRequest)
                {
                    this.setRequestCookie(request);
                }
                if (!string.IsNullOrEmpty(this.requestCookie))
                {
                    this.appendcookie(url);
                }
                request.Headers.Add("Accept-Language", "zh-CN");
                request.Accept = "application/json,text/javascript,*/*;q=0.01";
                request.Credentials = CredentialCache.DefaultCredentials;
                request.UserAgent = this.userAgent;
                request.KeepAlive = false;
                request.CookieContainer = this.cc;
                request.Referer = refer;
                request.Method = "POST";
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                string str = "\r\n";
                string str2 = Guid.NewGuid().ToString().Replace("-", "");
                request.ContentType = "multipart/form-data; boundary=" + str2;
                if (this.needGzip)
                {
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "deflate");
                }
                foreach (KeyValuePair<string, string> pair in postingData)
                {
                    writer.Write("--" + str2 + str);
                    writer.Write("Content-Disposition: form-data; name=\"{0}\"{1}{1}", pair.Key, str);
                    writer.Write(pair.Value + str);
                }
                foreach (HttpUploadingFile file in files)
                {
                    writer.Write("--" + str2 + str);
                    writer.Write("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", file.FieldName, file.FileName, str);
                    if (file.FieldName == "video_file_chunk")
                    {
                        writer.Write("Content-Type: video/avi" + str + str);
                    }
                    else
                    {
                        writer.Write("Content-Type: application/octet-stream" + str + str);
                    }
                    writer.Flush();
                    if (file.Data != null)
                    {
                        stream.Write(file.Data, 0, file.Data.Length);
                    }
                    writer.Write(str);
                    writer.Write("--" + str2 + str);
                }
                writer.Flush();
                using (Stream stream2 = request.GetRequestStream())
                {
                    stream.WriteTo(stream2);
                    //stream.Flush();
                    //long npos = stream.Position;
                    //byte[] buff = new byte[stream.Length];
                    //stream.Seek(0, SeekOrigin.Begin);
                    //stream.Read(buff, 0, buff.Length);
                    //FileStream fs = new FileStream("D:\\c#projects\\FBUploadPhoto0226\\FBUploadPhoto\\FBUploadPhoto\\bin\\Debug\\Log.txt", FileMode.OpenOrCreate);
                    //fs.Write(buff, 0, buff.Length);
                    //fs.Close();
                }
                // 。。。

                request.AddRange(0);
                return request;
            }

            public Stream DownloadAnyFile(string url, string refer)
            {
                Stream responseStream;
                HttpWebRequest request = this.createRequest(url, refer);
                request.Accept = "image/webp,image/*,*/*;q=0.8";
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        responseStream = response.GetResponseStream();
                        statucode = response.StatusCode;
                        if (response.Headers.Get("Content-Encoding") != null)
                        {
                            if (response.Headers.Get("Content-Encoding").ToLower().Equals("deflate"))
                            {
                                responseStream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress);
                            }
                            else
                            {
                                responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                            }
                        }
                        else
                        {
                            responseStream = response.GetResponseStream();
                        }
                        if (!string.IsNullOrEmpty(response.Headers.Get("Set-Cookie")))
                        {
                            lock (this.cookies.SyncRoot)
                            {
                                this.cookies.Add(this.CookieContainer.GetCookies(new Uri(url)));
                            }
                        }
                        responseStream.Dispose();
                        responseStream = null;
                    }
                }
                catch (Exception )
                {
                    throw ;
                }
                return responseStream;
            }

            public Bitmap DownloadFile(string url, string refer)
            {
                HttpWebRequest request = this.createRequest(url, refer);
                request.Accept = "image/webp,image/*,*/*;q=0.8";
                Bitmap bitmap = null;
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        Stream responseStream = response.GetResponseStream();
                        statucode = response.StatusCode;
                        if (response.Headers.Get("Content-Encoding") != null)
                        {
                            if (response.Headers.Get("Content-Encoding").ToLower().Equals("deflate"))
                            {
                                responseStream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress);
                            }
                            else
                            {
                                responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                            }
                        }
                        else
                        {
                            responseStream = response.GetResponseStream();
                        }
                        if (responseStream != null)
                        {
                            bitmap = (Bitmap)Image.FromStream(responseStream);
                        }
                        if (!string.IsNullOrEmpty(response.Headers.Get("Set-Cookie")))
                        {
                            lock (this.cookies.SyncRoot)
                            {
                                this.cookies.Add(this.CookieContainer.GetCookies(new Uri(url)));
                            }
                        }
                        responseStream.Dispose();
                        responseStream = null;
                        // response.Close();
                    }
                }
                catch (Exception)
                {
                    return null;
                }
                return bitmap;
            }

            public string DownloadString(string refer)
            {
                return this.DownloadString(this.url, refer);
            }

            public string DownloadString(string urlstr, string refer)
            {
                return this.DownloadString(urlstr, refer, false);
            }

            public string DownloadString(string urlstr, string refer, bool newCookie)
            {
                try
                {
                    string str3;
                    HttpWebRequest request = this.createRequest(urlstr, refer);
                    //request.Accept = "image/webp,image/*,*/*;q=0.8";
                    request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    try
                    {
                        /*using (*/

                        //{
                        statucode = response.StatusCode;

                        //处理网页Byte
                        byte[] ResponseByte = GetByte(ref response);

                        if (ResponseByte != null & ResponseByte.Length > 0)
                        {
                            //设置编码
                            Encoding encoder = SetEncoding(ref response, ResponseByte);
                            //得到返回的HTML
                            str3 = encoder.GetString(ResponseByte);
                        }
                        else
                        {
                            //没有返回任何Html代码
                            str3 = string.Empty;
                        }

                        //if (response.Headers.Get("Content-Encoding") != null)
                        //{
                        //    if (response.Headers.Get("Content-Encoding").ToLower().Equals("deflate"))
                        //    {
                        //        stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress);
                        //        reader = new StreamReader(stream, this.encoder);
                        //    }
                        //    else
                        //    {
                        //        stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                        //        reader = new StreamReader(stream, this.encoder);
                        //    }
                        //}
                        //else
                        //{
                        //    reader = new StreamReader(response.GetResponseStream(), this.encoder);
                        //}
                        // str = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(this.responseHeaderData))
                        {
                            this.responseHeaderData = response.Headers.Get(this.responseHeaderData);
                            this.responseuri = response.ResponseUri.AbsoluteUri;
                        }
                        if (!string.IsNullOrEmpty(response.Headers.Get("Set-Cookie")))
                        {
                            lock (this.cookies.SyncRoot)
                            {
                                for (int i = 0; i < response.Cookies.Count; i++)
                                {
                                    Cookie cookie = response.Cookies[i];
                                    if (cookie.Value == "deleted")
                                    {
                                        RemoveOneCookie(cookie);
                                    }
                                    else
                                    {
                                        SetOneCookie(cookie);
                                    }
                                }
                                //    this.cookies.Add(response.Cookies);
                            }
                        }
                        //}
                    }
                    catch (WebException exception)
                    {
                        if (exception.Status == WebExceptionStatus.Timeout)
                        {
                            return string.Empty;
                        }
                        str3 = string.Empty;
                    }
                    catch (Exception exception2)
                    {
                        throw new Exception("DownloadString_Url=" + urlstr + ",refUrl=" + refer, exception2);
                    }
                    response.Close();
                    return str3;
                }
                catch (WebException exception)
                {
                    if (exception.Status == WebExceptionStatus.Timeout)
                    {
                        return string.Empty;
                    }
                    throw new Exception("DownloadString_Url=" + urlstr + ",refUrl=" + refer, exception);
                }
                catch (Exception exception2)
                {
                    throw new Exception("DownloadString_Url=" + urlstr + ",refUrl=" + refer, exception2);
                }
            }

            public byte[] DownloadBytearr(string urlstr, string refer, bool newCookie)
            {
                try
                {
                    string str3;
                    byte[] ResponseByte=null;
                    HttpWebRequest request = this.createRequest(urlstr, refer);
                    //request.Accept = "image/webp,image/*,*/*;q=0.8";
                    request.Accept = "*/*";
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    try
                    {
                        /*using (*/

                        //{
                        statucode = response.StatusCode;

                        //处理网页Byte
                        ResponseByte = GetByte(ref response);

                        if (ResponseByte != null & ResponseByte.Length > 0)
                        {
                            //设置编码
                            Encoding encoder = SetEncoding(ref response, ResponseByte);
                            //得到返回的HTML
                            str3 = encoder.GetString(ResponseByte);
                        }
                        else
                        {
                            //没有返回任何Html代码
                            str3 = string.Empty;
                        }

                        // str = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(this.responseHeaderData))
                        {
                            this.responseHeaderData = response.Headers.Get(this.responseHeaderData);
                            this.responseuri = response.ResponseUri.AbsoluteUri;
                        }
                        if (!string.IsNullOrEmpty(response.Headers.Get("Set-Cookie")))
                        {
                            lock (this.cookies.SyncRoot)
                            {
                                for (int i = 0; i < response.Cookies.Count; i++)
                                {
                                    Cookie cookie = response.Cookies[i];
                                    if (cookie.Value == "deleted")
                                    {
                                        RemoveOneCookie(cookie);
                                    }
                                    else
                                    {
                                        SetOneCookie(cookie);
                                    }
                                }
                                //    this.cookies.Add(response.Cookies);
                            }
                        }
                        //}
                    }
                    catch (WebException exception)
                    {
                        if (exception.Status == WebExceptionStatus.Timeout)
                        {
                            return null;
                        }
                        str3 = string.Empty;
                    }
                    catch (Exception exception2)
                    {
                        throw new Exception("DownloadString_Url=" + urlstr + ",refUrl=" + refer, exception2);
                    }
                    response.Close();
                    return ResponseByte;
                }
                catch (WebException exception)
                {
                    if (exception.Status == WebExceptionStatus.Timeout)
                    {
                        return null;
                    }
                    throw new Exception("DownloadString_Url=" + urlstr + ",refUrl=" + refer, exception);
                }
                catch (Exception exception2)
                {
                    throw new Exception("DownloadString_Url=" + urlstr + ",refUrl=" + refer, exception2);
                }
            }

            public string DownloadStringWithRePath(string rpath, string refer)
            {
                if (string.IsNullOrEmpty(rpath))
                {
                    return this.DownloadString(this.url, refer);
                }
                if (rpath[0] != '/')
                {
                    rpath = '/' + rpath;
                }
                string urlstr = this.url + rpath;
                return this.DownloadString(urlstr, refer);
            }

            public string DownloadStringWithRePath(string rpath, string refer, bool newCookie)
            {
                if (string.IsNullOrEmpty(rpath))
                {
                    return this.DownloadString(this.url, refer);
                }
                if (rpath[0] != '/')
                {
                    rpath = '/' + rpath;
                }
                string urlstr = this.url + rpath;
                return this.DownloadString(urlstr, refer, newCookie);
            }

            public string getCookiesToStr(string uri)
            {
                string domain = GetDomain(uri);
                CookieCollection collect = this.cc.GetCookies(new Uri(uri));
                string str = string.Empty;
                //lock (this.cookies.SyncRoot)
                //{
                foreach (Cookie cookie in collect)
                {
                    string str2 = str;
                    str = str2 + cookie.Name + "=" + cookie.Value + ";";
                }
                // }
                return str;
            }

            public string getOneCookieValue(string strKey)
            {
                string str = string.Empty;
                lock (this.cookies.SyncRoot)
                {
                    foreach (Cookie cookie in this.cookies)
                    {
                        if (cookie.Name.Equals(strKey))
                        {
                            return cookie.Value;
                        }
                    }
                    return str;
                }
                //return str;
            }

            public string getOneCookieValue2(string strKey)
            {
                List<Cookie> lst = GetAllCookies(this.cc);
                foreach (Cookie cook in lst)
                {
                    if (cook.Name == strKey)
                        return cook.Value;
                }
                return string.Empty;
            }

            public static List<Cookie> GetAllCookies(CookieContainer cc)
            {
                List<Cookie> lstCookies = new List<Cookie>();
                Hashtable table = (Hashtable)cc.GetType().InvokeMember("m_domainTable",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
                    System.Reflection.BindingFlags.Instance, null, cc, new object[] { });
                foreach (object pathList in table.Values)
                {
                    SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                        | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                    foreach (CookieCollection colCookies in lstCookieCol.Values)
                        foreach (Cookie c in colCookies) lstCookies.Add(c);
                }
                return lstCookies;
            }

            [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern bool InternetGetCookie(string url, string name, StringBuilder data, ref int dataSize);
            public string Post(string url, string refer, string postingdata)
            {
                byte[] bytes = this.encoder.GetBytes(postingdata);
                string str = null;
                HttpWebResponse response = null;
                //StreamReader reader = null;
                Stream stream = null;
                try
                {
                    using (response = (HttpWebResponse)this.createPostRequest(url, refer, bytes).GetResponse())
                    {
                        //{
                        statucode = response.StatusCode;

                        //处理网页Byte
                        byte[] ResponseByte = GetByte(ref response);

                        if (ResponseByte != null & ResponseByte.Length > 0)
                        {
                            //设置编码
                            Encoding encoder = SetEncoding(ref response, ResponseByte);
                            //得到返回的HTML
                            str = encoder.GetString(ResponseByte);
                        }
                        else
                        {
                            //没有返回任何Html代码
                            str = string.Empty;
                        }
                        if (!string.IsNullOrEmpty(this.responseHeaderData))
                        {
                            this.responseHeaderData = response.Headers.Get(this.responseHeaderData);
                        }
                        if (!string.IsNullOrEmpty(response.Headers.Get("Set-Cookie")))
                        {
                            lock (this.cookies.SyncRoot)
                            {
                                for (int i = 0; i < response.Cookies.Count; i++)
                                {
                                    Cookie cookie = response.Cookies[i];
                                    if (cookie.Value == "deleted")
                                    {
                                        RemoveOneCookie(cookie);
                                    }
                                    else
                                    {
                                        SetOneCookie(cookie);
                                    }
                                }
                            }
                        }
                        if (stream != null)
                        {
                            stream.Dispose();
                        }
                        stream = null;
                        // reader.Dispose();
                        // reader = null;
                        response.Close();
                    }
                    return str;
                }
                catch (Exception exception)
                {
                    throw new Exception("POST_Url=" + url + ",errormessage=" + exception.Message + ",PostData=" + postingdata, exception);
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }
                //return str;
            }

            public byte[] Post_retbyte(string url, string refer, string postingdata)
            {
                byte[] bytes = this.encoder.GetBytes(postingdata);
                string str = null;
                byte[] ResponseByte = null;
                HttpWebResponse response = null;
                //StreamReader reader = null;
                Stream stream = null;
                try
                {
                    using (response = (HttpWebResponse)this.createPostRequest(url, refer, bytes).GetResponse())
                    {
                        //{
                        statucode = response.StatusCode;

                        //处理网页Byte
                        ResponseByte = GetByte(ref response);

                        //if (ResponseByte != null & ResponseByte.Length > 0)
                        //{
                        //    //设置编码
                        //    Encoding encoder = SetEncoding(ref response, ResponseByte);
                        //    //得到返回的HTML
                        //    str = encoder.GetString(ResponseByte);
                        //}
                        //else
                        //{
                        //    //没有返回任何Html代码
                        //    str = string.Empty;
                        //}
                        if (!string.IsNullOrEmpty(this.responseHeaderData))
                        {
                            this.responseHeaderData = response.Headers.Get(this.responseHeaderData);
                        }
                        if (!string.IsNullOrEmpty(response.Headers.Get("Set-Cookie")))
                        {
                            lock (this.cookies.SyncRoot)
                            {
                                for (int i = 0; i < response.Cookies.Count; i++)
                                {
                                    Cookie cookie = response.Cookies[i];
                                    if (cookie.Value == "deleted")
                                    {
                                        RemoveOneCookie(cookie);
                                    }
                                    else
                                    {
                                        SetOneCookie(cookie);
                                    }
                                }
                            }
                        }
                        if (stream != null)
                        {
                            stream.Dispose();
                        }
                        stream = null;
                        // reader.Dispose();
                        // reader = null;
                        response.Close();
                    }
                    return ResponseByte;
                }
                catch (Exception exception)
                {
                    throw new Exception("POST_Url=" + url + ",errormessage=" + exception.Message + ",PostData=" + postingdata, exception);
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }
                //return str;
            }

            public string PostByReUrl(string rpath, string refer, string data)
            {
                if (string.IsNullOrEmpty(rpath))
                {
                    return this.Post(this.url, refer, data);
                }
                if (rpath[0] != '/')
                {
                    rpath = '/' + rpath;
                }
                string url = this.url + rpath;
                return this.Post(url, refer, data);
            }

            public string PostFile(string urlstr, string refer, Dictionary<string, string> postingData, List<HttpUploadingFile> files)
            {
                return this.PostFile(urlstr, refer, postingData, files, null);
            }

            public string PostFile(string urlstr, string refer, Dictionary<string, string> postingData, List<HttpUploadingFile> files, List<KeyValuePair<string, string>> nvc)
            {
                string str3;
                if (nvc == null)
                {
                    nvc = new List<KeyValuePair<string, string>>();
                    nvc = this.ConvertDicToNvc(postingData);
                }
                System.GC.Collect();
                HttpWebRequest request = this.CreateRequest(urlstr, refer, nvc, files);
                string str = string.Empty;
                StreamReader reader = null;
                Stream stream = null;
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        //using (Stream stream2 = request.GetRequestStream())
                        //{
                        //    byte[] buff = new byte[stream2.Length];
                        //    stream2.Read(buff, 0, buff.Length);
                        //    FileStream fs = new FileStream("D:\\c#projects\\FBUploadPhoto0226\\FBUploadPhoto\\FBUploadPhoto\\bin\\Debug\\Log.txt", FileMode.OpenOrCreate);
                        //    fs.Write(buff, 0, buff.Length);
                        //    fs.Close();
                        //}

                        //MessageBox.Show(str111);
                        if (response.Headers.Get("Content-Encoding") != null)
                        {
                            if (response.Headers.Get("Content-Encoding").ToLower().Equals("deflate"))
                            {
                                stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress);
                                reader = new StreamReader(stream, this.encoder);
                            }
                            else
                            {
                                stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                                reader = new StreamReader(stream, this.encoder);
                            }
                        }
                        else
                        {
                            reader = new StreamReader(response.GetResponseStream(), this.encoder);
                        }
                        str = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(this.responseHeaderData))
                        {
                            this.responseHeaderData = response.Headers.Get(this.responseHeaderData);
                            this.responseuri = response.ResponseUri.AbsoluteUri;
                        }
                        if (!string.IsNullOrEmpty(response.Headers.Get("Set-Cookie")))
                        {
                            lock (this.cookies.SyncRoot)
                            {
                                this.cookies.Add(response.Cookies);
                            }
                        }
                        if (stream != null)
                        {
                            stream.Dispose();
                        }
                        stream = null;
                        //reader.Dispose();
                        reader = null;
                        //response.Close();
                        str3 = str;
                    }
                    if (request != null)
                        request.Abort();
                }
                catch (WebException exception)
                {
                    if (exception.Status == WebExceptionStatus.Timeout)
                    {
                        return string.Empty;
                    }
                    str3 = string.Empty;
                }
                catch (Exception exception2)
                {
                    throw new Exception("Url=" + this.url + ",refUrl=" + refer, exception2);
                }

                return str3;
            }

            public string PostJson(string url, string refer, string data)
            {
                byte[] bytes = this.encoder.GetBytes(data);
                string str = null;
                HttpWebResponse response = null;
                StreamReader reader = null;
                Stream stream = null;
                try
                {
                    using (response = (HttpWebResponse)this.createJosnPostRequest(url, refer, bytes).GetResponse())
                    {
                        if (response.Headers.Get("Content-Encoding") != null)
                        {
                            if (response.Headers.Get("Content-Encoding").ToLower().Equals("deflate"))
                            {
                                stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress);
                                reader = new StreamReader(stream, this.encoder);
                            }
                            else
                            {
                                stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                                reader = new StreamReader(stream, this.encoder);
                            }
                        }
                        else
                        {
                            reader = new StreamReader(response.GetResponseStream(), this.encoder);
                        }
                        str = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(this.responseHeaderData))
                        {
                            this.responseHeaderData = response.Headers.Get(this.responseHeaderData);
                        }
                        if (!string.IsNullOrEmpty(response.Headers.Get("Set-Cookie")))
                        {
                            lock (this.cookies.SyncRoot)
                            {
                                this.cookies.Add(response.Cookies);
                            }
                        }
                        if (stream != null)
                        {
                            stream.Close();
                        }
                        stream = null;
                        // reader.Close();
                        reader = null;
                        // response.Close();
                    }
                    return str;
                }
                catch (Exception exception)
                {
                    throw new Exception("Url=" + url + ",refUrl=" + refer + ",PostData=" + data, exception);
                }
                finally
                {
                    if (response != null)
                    {
                        //response.Close();
                    }
                }
                //return str;
            }

            public void RemoveOneCookie(Cookie cookie)
            {
                lock (this.cookies.SyncRoot)
                {
                    CookieCollection cookies = new CookieCollection();
                    foreach (Cookie cookie2 in this.cookies)
                    {
                        if (!cookie2.Name.Equals(cookie.Name) /*|| !cookie2.Domain.Equals(cookie.Domain)*/)
                        {
                            cookies.Add(cookie2);
                        }
                    }
                    this.cookies = cookies;
                }
            }

            public void SetOneCookie(Cookie cookie)
            {
                lock (this.cookies.SyncRoot)
                {
                    CookieCollection cookies = new CookieCollection();
                    if (this.cookies.Count == 0)
                    {
                        cookies.Add(cookie);
                    }
                    else
                    {
                        foreach (Cookie cookie2 in this.cookies)
                        {
                            if (!cookie2.Name.Equals(cookie.Name) /*|| !cookie2.Domain.Equals(cookie.Domain)*/)
                            {
                                cookies.Add(cookie2);
                            }
                            else
                            {
                                cookies.Add(cookie);
                            }
                        }
                    }
                    this.cookies = cookies;
                }
            }

            public void RemoveOneCookie(string cookieName)
            {
                lock (this.cookies.SyncRoot)
                {
                    CookieCollection cookies = new CookieCollection();
                    foreach (Cookie cookie in this.cookies)
                    {
                        if (!cookie.Name.Equals(cookieName))
                        {
                            cookies.Add(cookie);
                        }
                    }
                    this.cookies = cookies;
                }
            }

            public void RemoveRequestHeader(string name)
            {
                this.headerNameValue.Remove(name);
            }

            string GetDomain(string url)
            {
                string[] tmpArray = url.Split('/');
                if (tmpArray.Length > 3)
                {
                    string[] tmpary2 = tmpArray[2].Split('.');
                    return tmpary2[1] + "." + tmpary2[2];
                }
                return null;
            }

            public static bool CookieContainer_Contains(CookieContainer cookieContainer, Uri uri, Cookie cookie)
            {
                CookieCollection collection = cookieContainer.GetCookies(uri);
                for (int i = 0; i < collection.Count; i++)
                {
                    Cookie mycookie = collection[i];
                    if (mycookie.Name == cookie.Name)
                        return true;
                }
                return false;
            }

            private void setRequestCookie(HttpWebRequest request)
            {
                lock (this.cookies.SyncRoot)
                {
                    if (this.addCookie && (this.cookies.Count > 0))
                    {
                        List<string> list = new List<string>();
                        foreach (Cookie cookie in this.cookies)
                        {
                            string item = string.Empty;
                            string path = cookie.Path;
                            if ((!string.IsNullOrEmpty(path) && path.Contains("/")) && !path.EndsWith("/"))
                            {
                                path = path.Substring(0, path.LastIndexOf("/") + 1);
                            }
                            if (this.requestCookieDomain && !string.IsNullOrEmpty(cookie.Domain))
                            {
                                item = string.Format("{0}_{1}_{2}", cookie.Name, path, cookie.Domain);
                            }
                            else if (this.requestCookiePath && !string.IsNullOrEmpty(path))
                            {
                                item = string.Format("{0}_{1}", cookie.Name, path);
                            }
                            else
                            {
                                item = string.Format("{0}", cookie.Name);
                            }
                            if (!list.Contains(item))
                            {
                                try
                                {
                                    if (this.requestCookieDomain && !string.IsNullOrEmpty(cookie.Domain))
                                    {
                                        if (request.RequestUri.Host.Contains(cookie.Domain))
                                        {
                                            if (cookie.Domain.StartsWith("."))
                                            {
                                                this.CookieContainer.Add(request.RequestUri, new Cookie(cookie.Name, cookie.Value, "/", cookie.Domain.Substring(1)));
                                            }
                                            else
                                            {
                                                this.CookieContainer.Add(request.RequestUri, new Cookie(cookie.Name, cookie.Value, "/", cookie.Domain));
                                            }
                                        }
                                    }
                                    else if (this.requestCookiePath && !string.IsNullOrEmpty(path))
                                    {
                                        this.CookieContainer.Add(request.RequestUri, new Cookie(cookie.Name, cookie.Value, path));
                                    }
                                    else if (cookie.Value.Contains(","))
                                    {
                                        this.CookieContainer.Add(request.RequestUri, new Cookie(cookie.Name, HttpUtility.UrlEncode(cookie.Value)));
                                    }
                                    else
                                    {
                                        //this.CookieContainer.Add(request.RequestUri, new Cookie(cookie.Name, cookie.Value));
                                    }
                                    list.Add(item);
                                }
                                catch (Exception exception)
                                {
                                    throw new Exception(request.RequestUri.ToString(), exception);
                                }
                            }
                        }
                    }
                }
            }

            public void SetTimeOut(int timeout)
            {
                this._maxtimeout = timeout;
            }

            public void SetUserAgentAndrioIE4()
            {
                this.userAgent = "Mozilla/5.0 (Linux; U; Android 4.1.1; en-us; sdk Build/JRO03E) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";
            }

            public void SetUserAgentChrome()
            {
                this.userAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.111 Safari/537.36";
            }

            public void SetUserAgentIE7()
            {
                this.userAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E)";
            }

            public void SetUserAgentIE9()
            {
                this.userAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 5.1; Trident/5.0; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E)";
            }

            public bool AddCookie
            {
                get
                {
                    return this.addCookie;
                }
                set
                {
                    this.addCookie = value;
                }
            }

            public bool AllowAutoRedirect
            {
                get
                {
                    return this.allowAutoRedirect;
                }
                set
                {
                    this.allowAutoRedirect = value;
                }
            }

            public System.Net.CookieContainer CookieContainer
            {
                get
                {
                    return this.cc;
                }
                set
                {
                    this.cc = value;
                }
            }

            public CookieCollection Cookies
            {
                get
                {
                    return this.cookies;
                }
                set
                {
                    if (value != null)
                    {
                        this.cookies = value;
                    }
                }
            }

            public Encoding Encoder
            {
                get
                {
                    return this.encoder;
                }
                set
                {
                    this.encoder = value;
                }
            }

            public bool IsPhoneClient
            {
                get
                {
                    return this.isPhoneClient;
                }
                set
                {
                    this.isPhoneClient = value;
                }
            }

            public bool KeepAlive
            {
                get
                {
                    return this.keepalive;
                }
                set
                {
                    this.keepalive = value;
                }
            }

            public int MaxTimeout
            {
                get
                {
                    return this._maxtimeout;
                }
                set
                {
                    this._maxtimeout = value;
                }
            }

            public bool NeedAddCookieRequest
            {
                get
                {
                    return this.needAddCookieRequest;
                }
                set
                {
                    this.needAddCookieRequest = value;
                }
            }

            public bool NeedGzip
            {
                get
                {
                    return this.needGzip;
                }
                set
                {
                    this.needGzip = value;
                }
            }

            public string RequestCookie
            {
                get
                {
                    return this.requestCookie;
                }
                set
                {
                    this.requestCookie = value;
                }
            }

            public bool RequestCookieDomain
            {
                get
                {
                    return this.requestCookieDomain;
                }
                set
                {
                    this.requestCookieDomain = value;
                }
            }

            public bool RequestCookiePath
            {
                get
                {
                    return this.requestCookiePath;
                }
                set
                {
                    this.requestCookiePath = value;
                }
            }

            public string ResponseHeaderData
            {
                get
                {
                    return this.responseHeaderData;
                }
                set
                {
                    this.responseHeaderData = value;
                }
            }

            public string Responseuri
            {
                get
                {
                    return this.responseuri;
                }
                set
                {
                    this.responseuri = value;
                }
            }

            public string Url
            {
                get
                {
                    return this.url;
                }
                set
                {
                    this.url = value;
                }
            }

            public string UserAgent
            {
                get
                {
                    return this.userAgent;
                }
                set
                {
                    this.userAgent = value;
                }
            }

            public class HttpUploadingFile
            {
                private byte[] data;
                private string fieldName;
                private string fileName;

                public HttpUploadingFile(string fileName, string fieldName)
                {
                    this.fileName = fileName;
                    this.fieldName = fieldName;
                    using (FileStream stream = new FileStream(fileName, FileMode.Open))
                    {
                        byte[] buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                        this.data = buffer;
                    }
                }

                public HttpUploadingFile(byte[] data, string fileName, string fieldName)
                {
                    this.data = data;
                    this.fileName = fileName;
                    this.fieldName = fieldName;
                }

                public byte[] Data
                {
                    get
                    {
                        return this.data;
                    }
                    set
                    {
                        this.data = value;
                    }
                }

                public string FieldName
                {
                    get
                    {
                        return this.fieldName;
                    }
                    set
                    {
                        this.fieldName = value;
                    }
                }

                public string FileName
                {
                    get
                    {
                        return this.fileName;
                    }
                    set
                    {
                        this.fileName = value;
                    }
                }
            }


            /// <summary>
            /// 4.0以下.net版本取数据使用
            /// </summary>
            /// <param name="streamResponse">流</param>
            private MemoryStream GetMemoryStream(Stream streamResponse)
            {
                MemoryStream _stream = new MemoryStream();
                int Length = 256;
                Byte[] buffer = new Byte[Length];
                int bytesRead = streamResponse.Read(buffer, 0, Length);
                while (bytesRead > 0)
                {
                    _stream.Write(buffer, 0, bytesRead);
                    bytesRead = streamResponse.Read(buffer, 0, Length);
                }
                return _stream;
            }

            /// <summary>
            /// 提取网页Byte
            /// </summary>
            /// <returns></returns>
            private byte[] GetByte(ref HttpWebResponse response)
            {
                byte[] ResponseByte = null;
                MemoryStream _stream = new MemoryStream();

                //GZIIP处理
                if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                {
                    //开始读取流并设置编码方式
                    _stream = GetMemoryStream(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress));
                }
                else
                {
                    //开始读取流并设置编码方式
                    _stream = GetMemoryStream(response.GetResponseStream());
                }
                //获取Byte
                ResponseByte = _stream.ToArray();
                _stream.Close();
                return ResponseByte;
            }

            /// <summary>
            /// 设置编码
            /// </summary>
            /// <param name="item">HttpItem</param>
            /// <param name="result">HttpResult</param>
            /// <param name="ResponseByte">byte[]</param>
            /// private Encoding encoding;
            private Encoding SetEncoding(ref HttpWebResponse response, byte[] ResponseByte)
            {
                Encoding encoding = null;
                //从这里开始我们要无视编码了
                if (encoding == null)
                {
                    Match meta = Regex.Match(Encoding.Default.GetString(ResponseByte), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                    string c = string.Empty;
                    if (meta != null && meta.Groups.Count > 0)
                    {
                        c = meta.Groups[1].Value.ToLower().Trim();
                    }
                    if (c.Length > 2)
                    {
                        try
                        {
                            encoding = Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
                        }
                        catch
                        {
                            if (string.IsNullOrEmpty(response.CharacterSet))
                            {
                                encoding = Encoding.UTF8;
                            }
                            else
                            {
                                encoding = Encoding.GetEncoding(response.CharacterSet);
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(response.CharacterSet))
                        {
                            encoding = Encoding.UTF8;
                        }
                        else
                        {
                            encoding = Encoding.GetEncoding(response.CharacterSet);
                        }
                    }
                }
                return encoding;
            }

            public static byte[] StreamToBytes(Stream stream)
            {
                List<byte> bytes = new List<byte>();
                int temp = stream.ReadByte();
                while (temp != -1)
                {
                    bytes.Add((byte)temp);
                    temp = stream.ReadByte();
                }

                return bytes.ToArray();
            }
        }
}
