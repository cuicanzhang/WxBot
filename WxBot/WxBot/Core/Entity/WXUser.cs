using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using WxBot.Http;

namespace WxBot.Core.Entity
{
    [Serializable]
    public class WXUser
    {
        /// <summary>
        /// 微信唯一标识
        /// </summary>
        public string uin { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 头像url
        /// </summary>
        public string HeadImgUrl { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string RemarkName { get; set; }
        /// <summary>
        /// 性别  男1 女2 其他0
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 前面
        /// </summary>
        public string Signature { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 拼音全拼
        /// </summary>
        private string _pyQuanPin;
        public string PYQuanPin
        {
            get
            {
                return _pyQuanPin;
            }
            set
            {
                _pyQuanPin = value;
            }
        }
        /// <summary>
        /// 备注名拼音全屏
        /// </summary>
        private string _remarkPYQuanPin;
        public string RemarkPYQuanPin
        {
            get
            {
                return _remarkPYQuanPin;
            }
            set
            {
                _remarkPYQuanPin = value;
            }
        }
        private MemoryStream _icon = null;
        private bool _loading_icon = false;

        /// <summary>
        /// 头像
        /// </summary>
        public MemoryStream Icon
        {
            get
            {
                if (_icon == null && !_loading_icon)
                {
                    _loading_icon = true;
                    ((Action)(delegate ()
                    {
                        WXService wxs = new WXService();
                        if (UserName.Contains("@@"))  //讨论组
                        {
                            _icon = wxs.GetIcon(HeadImgUrl, uin);
                        }
                        else if (UserName.Contains("@"))  //好友
                        {
                            _icon = wxs.GetIcon(HeadImgUrl, uin);
                        }
                        else
                        {
                            _icon = wxs.GetIcon(HeadImgUrl, uin);
                        }
                        _loading_icon = false;
                    })).BeginInvoke(null, null);
                }
                return _icon;
            }
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string ShowName
        {
            get
            {
                return (RemarkName == null || RemarkName == "") ? NickName : RemarkName;
            }
        }
        /// <summary>
        /// 显示的拼音全拼
        /// </summary>
        public string ShowPinYin
        {
            get
            {
                return (_remarkPYQuanPin == null || _remarkPYQuanPin == "") ? _pyQuanPin : _remarkPYQuanPin;
            }
        }


    }
}

/// <summary>
/// 微信群内成员信息
/// </summary>
[Serializable]
public class GroupWxUser
{
    public string UserName { get; set; }
    public string NickName { get; set; }
    public string DisplayName { get; set; }
    public string AttrStatus { get; set; }
}


