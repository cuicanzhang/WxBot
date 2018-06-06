using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WxBot.Core.Entity;
using WxBot.Http;

namespace WxBot.Core
{
    class LoginCore
    {
        private static Dictionary<string, Dictionary<string, string>> SyncKeyDic = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, PassTicketEntity> _passticket_dic = new Dictionary<string, PassTicketEntity>();
        
        public static void PassTicket(string uin ,PassTicketEntity entity)
        {
            //序列化登录passticket
            WxSerializable s = new WxSerializable(uin, EnumContainer.SerializType.pass_ticket);
            s.Serializable(entity);
            if (_passticket_dic.ContainsKey(uin))
                _passticket_dic.Remove(uin);
            _passticket_dic.Add(uin, entity);
        }
        public static PassTicketEntity GetPassTicket(string uin)
        {
            if (string.IsNullOrEmpty(uin))
                uin = string.Empty;
            if (_passticket_dic.ContainsKey(uin))
            {
                return _passticket_dic[uin];
            }
            else
            {
                WxSerializable s = new WxSerializable(uin, EnumContainer.SerializType.pass_ticket);
                //if (_passticket_dic.ContainsKey(uin))
                //    _passticket_dic.Remove(uin);
                ////先判断下键值是否存在要不卡死头像只能显示一个用户的
                //if (uin=="0")
                //{
                //    return null;
                //}
                //if (!_passticket_dic.ContainsKey("1"))
                //{
                //    _passticket_dic.Add(uin, (PassTicketEntity)s.DeSerializable());
                //}
                //return (PassTicketEntity)s.DeSerializable();
                try
                {
                    if (_passticket_dic.ContainsKey(uin))
                        _passticket_dic.Remove(uin);
                    _passticket_dic.Remove(uin);
                    return (PassTicketEntity)s.DeSerializable();
                }
                catch
                {
                    return null;
                }
            }
        }
        public static void InitCookie(string uin)
        {
            WxSerializable s = new WxSerializable(uin, EnumContainer.SerializType.cookie);
            var cookies_dic = (Dictionary<string, CookieContainer>)s.DeSerializable();
            HttpService.CookiesContainerDic = cookies_dic;

        }
        public static void SyncKey(string uin, Dictionary<string, string> dic)
        {
            if (SyncKeyDic.ContainsKey(uin))
                SyncKeyDic.Remove(uin);
            SyncKeyDic.Add(uin, dic);
        }
        public static Dictionary<string, string> GetSyncKey(string uin)
        {
            if (SyncKeyDic.ContainsKey(uin))
                return SyncKeyDic[uin];
            else
                return null;
        }



        public static string GenerateCheckCode(int l)
        {  //产生l位的随机字符串
            int number;
            char code;
            string checkCode = String.Empty;

            System.Random random = new Random();

            for (int i = 0; i < 15; i++)
            {
                number = random.Next();
                code = (char)('0' + (char)(number % 10));
                checkCode += code.ToString();
            }

            return checkCode;
        }
        public static string GetRet(string text)
        {
            var INFO = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text));
            var getURl = "http://wx.wlin.xyz/api.php?info=" + INFO;
            Uri uri = new Uri(getURl);
            HttpWebRequest getUrl = WebRequest.Create(uri) as HttpWebRequest;
            getUrl.Method = "GET";
            HttpWebResponse response = getUrl.GetResponse() as HttpWebResponse;
            Stream respStream = response.GetResponseStream();
            StreamReader stream = new StreamReader(respStream, Encoding.UTF8);
            string respStr = stream.ReadToEnd();
            stream.Close();

            JObject init_result = JsonConvert.DeserializeObject(respStr) as JObject;
            var code = init_result["ret"].ToString();
            return code;
        }
    }
}