using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Windows;
using System.Collections.Specialized;
using System.Linq;

using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace WxBot.Http
{
    public class HttpService
    {
        /// <summary>
        /// cookie容器
        /// </summary>
        public static Dictionary<string, CookieContainer> CookiesContainerDic = new Dictionary<string, CookieContainer>();


        public static byte[] SendGetRequest(string url, string uid)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "get";

                if (!string.IsNullOrEmpty(uid))
                {
                    CookieContainer _cookiesContainer = CookiesContainerDic[uid];

                    if (_cookiesContainer == null)
                    {
                        _cookiesContainer = new CookieContainer();
                    }
                    if (!CookiesContainerDic.ContainsKey(uid))
                    {
                        CookiesContainerDic.Add(uid, _cookiesContainer);
                    }

                    request.CookieContainer = _cookiesContainer;  //启用cookie
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream response_stream = response.GetResponseStream();

                int count = (int)response.ContentLength;
                int offset = 0;
                byte[] buf = new byte[count];
                while (count > 0)  //读取返回数据
                {
                    int n = response_stream.Read(buf, offset, count);
                    if (n == 0) break;
                    count -= n;
                    offset += n;
                }
                return buf;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SendGetRequest" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// get请求，并返回cookie
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <returns></returns>
        public static byte[] SendGetRequest(string url, ref CookieContainer cookieContainer)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "get";
                request.CookieContainer = cookieContainer;  //启用cookie               

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream response_stream = response.GetResponseStream();

                int count = (int)response.ContentLength;
                int offset = 0;
                byte[] buf = new byte[count];
                while (count > 0)  //读取返回数据
                {
                    int n = response_stream.Read(buf, offset, count);
                    if (n == 0) break;
                    count -= n;
                    offset += n;
                }
                return buf;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SendGetRequest Cook" + ex.Message);
                return null;
            }
        }

        public static byte[] SendPostRequest(string url, string body, string uid)
        {
            try
            {
                byte[] request_body = Encoding.UTF8.GetBytes(body);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "post";
                request.ContentLength = request_body.Length;

                Stream request_stream = request.GetRequestStream();

                request_stream.Write(request_body, 0, request_body.Length);

                if (!string.IsNullOrEmpty(uid))
                {
                    CookieContainer _cookiesContainer = null;
                    if (CookiesContainerDic.ContainsKey(uid))
                    {
                        _cookiesContainer = CookiesContainerDic[uid];
                    }

                    if (_cookiesContainer == null)
                    {
                        _cookiesContainer = new CookieContainer();
                    }
                    if (!CookiesContainerDic.ContainsKey(uid))
                    {
                        CookiesContainerDic.Add(uid, _cookiesContainer);
                    }

                    request.CookieContainer = _cookiesContainer;  //启用cookie
                }


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream response_stream = response.GetResponseStream();

                int count = (int)response.ContentLength;
                int offset = 0;
                byte[] buf = new byte[count];
                while (count > 0)  //读取返回数据
                {
                    int n = response_stream.Read(buf, offset, count);
                    if (n == 0) break;
                    count -= n;
                    offset += n;
                }
                return buf;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SendPostRequest" + ex.Message);
                return null;
            }
        }
        public static bool CheckValidationResult(object sender, X509Certificate certificate,
X509Chain chain, SslPolicyErrors errors)
        {  // 总是接受  
            return true;
        }



        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        /// <summary>
        /// Http下载文件
        /// </summary>
        public static string HttpDownloadFile(string url, string uid, string path)
        {
            try
            {

                byte[] bytes = SendGetRequest(url, uid);

                using (StreamWriter sw = new StreamWriter(path, false))
                {
                    sw.Write(bytes);
                }

                File.WriteAllBytes(path, bytes);
                //////创建本地文件写入流
                //Stream stream = new FileStream(path, FileMode.Create);
                //stream.Write(bytes, 0, bytes.Length);

                return path;


            }
            catch (Exception ex)
            {
                MessageBox.Show("HttpDownloadFile" + ex.Message);
                return null;
            }
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
        public static string GetMD5(string fileName)
        {
            try
            {
                var file = new FileStream(fileName, FileMode.Open);
                var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString().ToLower();
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetMD5" + ex.Message);
                //throw new Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
                return null;
            }
        }
    }
}
