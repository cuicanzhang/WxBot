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
                byte[] bytes = HttpService.SendGetRequest(BaseUrl + Constant._geticon_url + username, uin);
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
        /// <summary>
        /// 微信同步
        /// </summary>
        /// <returns></returns> 
        public JObject WxSync()
        {
            var entity = LoginCore.GetPassTicket(Uin);
            string sync_json = "{{\"BaseRequest\" : {{\"DeviceID\":\"{6}\",\"Sid\":\"{1}\", \"Skey\":\"{5}\", \"Uin\":\"{0}\"}},\"SyncKey\" : {{\"Count\":{2},\"List\": [{3}]}},\"rr\" :{4}}}";
            string sync_keys = "";
            var _syncKey = LoginCore.GetSyncKey(Uin);
            foreach (KeyValuePair<string, string> p in _syncKey)
            {
                sync_keys += "{\"Key\":" + p.Key + ",\"Val\":" + p.Value + "},";
            }
            sync_keys = sync_keys.TrimEnd(',');
            sync_json = string.Format(sync_json, this.Uin, this.Sid, _syncKey.Count, sync_keys, (long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds, entity.SKey, DeviceID);

            if (this.Sid != null && this.Uin != null)
            {
                byte[] bytes = HttpService.SendPostRequest(BaseUrl + Constant._sync_url + this.Sid + "&lang=zh_CN&skey=" + entity.SKey + "&pass_ticket=" + entity.PassTicket, sync_json, this.Uin);
                string sync_str = Encoding.UTF8.GetString(bytes);
                if (sync_str == null)
                {
                    return null;
                }
                JObject sync_resul = JsonConvert.DeserializeObject(sync_str) as JObject;
                // Dictionary<string, string> ss = new Dictionary<string, string>();
                if (sync_resul["SyncKey"]["Count"].ToString() != "1")
                {
                    _syncKey.Clear();
                    foreach (JObject key in sync_resul["SyncKey"]["List"])
                    {
                        _syncKey.Add(key["Key"].ToString(), key["Val"].ToString());
                    }
                }

                return sync_resul;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        public void SendMsg(string msg, string from, string to, int type, string Uin, string Sid)
        {
            string msg_json = "{{" +
            "\"BaseRequest\":{{" +
                "\"DeviceID\" : \"{10}\"," +
                "\"Sid\" : \"{0}\"," +
                "\"Skey\" : \"{6}\"," +
                "\"Uin\" : \"{1}\"" +
            "}}," +
            "\"Msg\" : {{" +
                "\"ClientMsgId\" : {8}," +
                "\"Content\" : \"{2}\"," +
                "\"FromUserName\" : \"{3}\"," +
                "\"LocalID\" : {9}," +
                "\"ToUserName\" : \"{4}\"," +
                "\"Type\" : {5}" +
            "}}," +
            "\"rr\" : {7}" +
            "}}";
            var entity = LoginCore.GetPassTicket(Uin);
            if (Sid != null && Uin != null)
            {
                //((long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds) * 10000
                msg_json = string.Format(msg_json, Sid, Uin, msg, from, to, type, entity.SKey, DateTime.Now.Millisecond, DateTime.Now.Millisecond, DateTime.Now.Millisecond, DeviceID);
                byte[] bytes = HttpService.SendPostRequest(BaseUrl + Constant._sendmsg_url + Sid + "&lang=zh_CN&pass_ticket=" + entity.PassTicket, msg_json, Uin);
                string send_result = Encoding.UTF8.GetString(bytes);
                var aa = send_result;
            }
            //((Action)delegate()
            //{
            //    //存储发送消息
            //    var b = WxOperateLogDal.AddWxsendmsglog(Uin, from, msg,to,from);
            //}).BeginInvoke(null, null);
        }
        /// <summary>
        /// 发送图片
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        public void SendMsgImg1(string msg, string from, string to, int type, string Uin, string Sid)
        {
            string msg_json = "{{" +
            "\"BaseRequest\":{{" +
                "\"DeviceID\" : \"{0}\"," +
                "\"Sid\" : \"{1}\"," +
                "\"Skey\" : \"{2}\"," +
                "\"Uin\" : \"{3}\"" +
            "}}," +
            "\"Msg\" : {{" +
                "\"ClientMsgId\" : {4}," +
                "\"Content\" : \"\"," +
                "\"FromUserName\" : \"{5}\"," +
                "\"LocalID\" : {6}," +
                "\"MediaId\" : \"{7}\"," +
                "\"ToUserName\" : \"{8}\"," +
                "\"Type\" : {9}" +
            "}}," +
            "\"rr\" : {10}" +
            "}}";
            var entity = LoginCore.GetPassTicket(Uin);
            if (Sid != null && Uin != null)
            {
                //((long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds) * 10000
                msg_json = string.Format(msg_json, DeviceID, Sid, entity.SKey, Uin, DateTime.Now.Millisecond, from, DateTime.Now.Millisecond, msg, to, type, DateTime.Now.Millisecond);
                byte[] bytes = HttpService.SendPostRequest(BaseUrl + Constant._sendmsgimg_url + "&pass_ticket=" + entity.PassTicket, msg_json, Uin);
                string send_result = Encoding.UTF8.GetString(bytes);
                var aa = send_result;
            }
            //((Action)delegate()
            //{
            //    //存储发送消息
            //    var b = WxOperateLogDal.AddWxsendmsglog(Uin, from, msg,to,from);
            //}).BeginInvoke(null, null);
        }
        /// <summary>
        /// 发送图片
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        public void SendMsgImg(string MediaId, string msg, string from, string to, int type, string Uin, string Sid)
        {
            string msg_json = "{{" +
            "\"BaseRequest\":{{" +
                "\"DeviceID\" : \"{0}\"," +
                "\"Sid\" : \"{1}\"," +
                "\"Skey\" : \"{2}\"," +
                "\"Uin\" : \"{3}\"" +
            "}}," +
            "\"Msg\" : {{" +
                "\"ClientMsgId\" : {4}," +
                "\"Content\" : \"{5}\"," +
                "\"FromUserName\" : \"{6}\"," +
                "\"LocalID\" : {7}," +
                "\"MediaId\" : \"{8}\"," +
                "\"ToUserName\" : \"{9}\"," +
                "\"Type\" : {10}" +
            "}}," +
            "\"rr\" : {11}" +
            "}}";
            var entity = LoginCore.GetPassTicket(Uin);
            if (Sid != null && Uin != null)
            {
                //((long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds) * 10000
                msg_json = string.Format(msg_json, DeviceID, Sid, entity.SKey, Uin, DateTime.Now.Millisecond, msg, from, DateTime.Now.Millisecond, MediaId, to, type, DateTime.Now.Millisecond);
                byte[] bytes = HttpService.SendPostRequest(BaseUrl + Constant._sendmsgimg_url + "&pass_ticket=" + entity.PassTicket, msg_json, Uin);
                string send_result = Encoding.UTF8.GetString(bytes);
                var aa = send_result;
            }
            //((Action)delegate()
            //{
            //    //存储发送消息
            //    var b = WxOperateLogDal.AddWxsendmsglog(Uin, from, msg,to,from);
            //}).BeginInvoke(null, null);
        }
        /// <summary>
        /// 发送表情
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        public void SendEmoticon(string EMoticonMd5, string from, string to, int type, string Uin, string Sid)
        {
            string msg_json = "{{" +
            "\"BaseRequest\":{{" +
                "\"DeviceID\" : \"{0}\"," +
                "\"Sid\" : \"{1}\"," +
                "\"Skey\" : \"{2}\"," +
                "\"Uin\" : \"{3}\"" +
            "}}," +
            "\"Msg\" : {{" +
                "\"ClientMsgId\" : {4}," +
                "\"EMoticonMd5\" : \"{5}\"," +
                "\"FromUserName\" : \"{6}\"," +
                "\"LocalID\" : {7}," +
                "\"ToUserName\" : \"{8}\"," +
                "\"Type\" : {9}" +
            "}}," +
            "\"rr\" : {10}" +
            "}}";
            var entity = LoginCore.GetPassTicket(Uin);
            if (Sid != null && Uin != null)
            {
                //((long)(DateTime.Now.ToUniversalTime() - new System.DateTime(1970, 1, 1)).TotalMilliseconds) * 10000
                msg_json = string.Format(msg_json, DeviceID, Sid, entity.SKey, Uin, DateTime.Now.Millisecond, EMoticonMd5, from, DateTime.Now.Millisecond, to, type, DateTime.Now.Millisecond);
                byte[] bytes = HttpService.SendPostRequest(BaseUrl + Constant._webwxsendemoticon_url + "&pass_ticket=" + entity.PassTicket, msg_json, Uin);
                string send_result = Encoding.UTF8.GetString(bytes);
                var aa = send_result;
            }
            //((Action)delegate()
            //{
            //    //存储发送消息
            //    var b = WxOperateLogDal.AddWxsendmsglog(Uin, from, msg,to,from);
            //}).BeginInvoke(null, null);
        }
    }
}
