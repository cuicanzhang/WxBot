using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using WxBot.Core;
using WxBot.Http;

namespace WxBot
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            code.Visibility = Visibility.Hidden;
            DoLogin();
        }
        LoginService ls = new LoginService();
        private void DoLogin()
        {
            QRCode.Source = null;
            ((Action)(delegate ()
            {
                MemoryStream qrcode = ls.GetQRCode();
                if (qrcode != null)
                {
                    this.Dispatcher.Invoke((Action)delegate ()
                    {
                        QRCode.Source = BitmapFrame.Create(qrcode, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    });
                }
                else
                {
                    MessageBox.Show("获取二维码失败！");
                }
                object login_result = null;
                while (true)
                {
                    login_result = ls.LoginCheck();
                    if (login_result is MemoryStream)
                    {
                        this.Dispatcher.Invoke((Action)delegate ()
                        {
                            QRCode.Source = BitmapFrame.Create(login_result as MemoryStream, BitmapCreateOptions.None, BitmapCacheOption.Default); ;
                        });
                    }
                    if (login_result is string)  //已完成登录
                    {
                        //访问登录跳转URL
                        
                        var uin = ls.GetSidUid(login_result as string);
                        var md5_uin = HttpService.StringToMD5Hash(uin+"踏天境");
                        JObject result = LoginCore.GetRet(md5_uin);
                        
                        if (result["ret"].ToString() == "1")
                        {
                            this.Dispatcher.Invoke((Action)delegate ()
                            {
                                MainWindow.uin = ls.GetSidUid(login_result as string);
                                MainWindow.MaxSelected = int.Parse(result["MaxSelected"].ToString());
                                this.DialogResult = Convert.ToBoolean(1);
                                this.Close();
                            });
                            break;
                        }
                        else
                        {
                            this.Dispatcher.Invoke((Action)delegate () 
                            {
                                code.Visibility = Visibility.Visible;
                                code.AppendText(uin+ "|"+ md5_uin);
                                //code.AppendText(uin);
                            });
                            break;
                        }                        
                    }
                }
            })).BeginInvoke(null, null);
        }
    }
}
