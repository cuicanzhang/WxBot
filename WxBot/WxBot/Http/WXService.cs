using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WxBot.Core;

namespace WxBot.Http
{
    class WXService
    {
        public string Uin { get; set; }
        public string Sid { get; set; }
        public string DeviceID { get; set; }
        public string BaseUrl { get; set; }
        public string PushUrl { get; set; }
        public string UploadUrl { get; set; }

        public JObject WxInit()
        {
            string init_json = "{{\"BaseRequest\":{{\"Uin\":\"{0}\",\"Sid\":\"{1}\",\"Skey\":\"{2}\",\"DeviceID\":\"{3}\"}}}}";

            if (Uin != null && Sid != null)
            {
                string pass_ticket = LoginCore.GetPassTicket(Uin).PassTicket; ;//这个位置过来了
                string skey = LoginCore.GetPassTicket(Uin).SKey; ;
                init_json = string.Format(init_json, Uin, Sid, skey, DeviceID);
                var url = BaseUrl + Constant._init_url + "&pass_ticket=" + pass_ticket;
                byte[] bytes = HttpService.SendPostRequest(url, init_json, Uin);
                string init_str = Encoding.UTF8.GetString(bytes);
                JObject init_result = JsonConvert.DeserializeObject(init_str) as JObject;
                return init_result;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 获取头像
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public MemoryStream GetIcon(string username, string uin = "")
        {
            try
            {
                string url = BaseUrl + Constant._geticon_url + username;
                byte[] bytes = HttpService.SendGetRequest(url, uin);
                return new MemoryStream(bytes);
            }

            catch (Exception ex)
            {
                MessageBox.Show("GetIcon" + ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <returns></returns>
        public JObject GetContact()
        {
            byte[] bytes = HttpService.SendGetRequest(BaseUrl + Constant._getcontact_url, Uin);
            string contact_str = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(contact_str) as JObject;
        }
        public JObject BatGetContact(List<string> groupUserName)
        {
            var entity = LoginCore.GetPassTicket(Uin);
            var _jstr = string.Empty;
            foreach (var username in groupUserName)
            {
                _jstr += string.Format("{{{{\"UserName\":\"{0}\",\"ChatRoomId\":\"\"}}}},",
                    username, "");
            }
            string json = "{{" +
                "\"BaseRequest\":{{\"Uin\":{0}," +
                "\"Sid\":\"{1}\"," +
                "\"Skey\":\"{2}\"," +
                "\"DeviceID\":\"{4}\"}}," +
                "\"Count\":{3}," +
                "\"List\":[" +
                    _jstr.TrimEnd(',') +
                    "]" +
                "}}";
            try
            {
                json = string.Format(json, Uin, Sid, entity.SKey, groupUserName.Count, DeviceID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("BatGetContact" + ex.Message);
                //写日志
                Tools.WriteLog(ex.ToString());
            }

            string url = string.Format(BaseUrl + Constant._getbatcontact_url, HttpService.GetTimeStamp(), entity.PassTicket);
            byte[] bytes = HttpService.SendPostRequest(url, json, Uin);
            string contact_str = Encoding.UTF8.GetString(bytes);

            return JsonConvert.DeserializeObject(contact_str) as JObject;
        }
        public string WxSyncCheck()
        {
            string sync_key = "";
            try
            {
                var _syncKey = LoginCore.GetSyncKey(Uin);
                foreach (KeyValuePair<string, string> p in _syncKey)
                {
                    sync_key += p.Key + "_" + p.Value + "%7C";
                }
                sync_key = sync_key.TrimEnd('%', '7', 'C');

                var entity = LoginCore.GetPassTicket(Uin);
                if (Sid != null && Uin != null)
                {
                    var _synccheck_url = string.Format(PushUrl + Constant._synccheck_url, Sid, Uin, sync_key, (long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds, entity.SKey.Replace("@", "%40"), DeviceID);

                    byte[] bytes = HttpService.SendGetRequest(_synccheck_url + "&_=" + DateTime.Now.Ticks, Uin);

                    if (bytes != null)
                    {
                        //string contact_str = Encoding.UTF8.GetString(bytes);
                        return Encoding.UTF8.GetString(bytes);
                        //string retcode = contact_str.ToString().Split(new string[] { "\"" }, StringSplitOptions.None)[1];
                        //string selector = contact_str.ToString().Split(new string[] { "\"" }, StringSplitOptions.None)[3];
                        //string[]rs= { retcode, selector };
                        //return contact_str;
                        //return new string[]{ retcode, selector };
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("WxSyncCheck" + ex.Message);
                return "";
                //return null;
            }
        }
    }
}
