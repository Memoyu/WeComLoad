using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeComLoad.Model;

namespace WeComLoad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_GetQrCode_Click(object sender, RoutedEventArgs e)
        {
        ReGetLoginQrCode:
            var key = await GetLoginAndShowQrCode();
            var authCode = string.Empty;
            var isLogin = false;
            int count = 1;
            var authSource = string.Empty;
            var delay = 4000;
            while (!isLogin)
            {
                var data = await GetLoginStatus(key);
                richText_login_status.Document = new FlowDocument(new Paragraph(new Run($"{data}\r\n\r\n当前刷新次数：{count}")));

                var status = JsonConvert.DeserializeObject<JObject>(data);
                var statusStr = status["data"]["status"]?.ToString();
                if (statusStr.Equals("QRCODE_SCAN_ING"))
                {
                    delay = 1000;
                }
                else if (statusStr.Equals("QRCODE_SCAN_SUCC"))
                {
                    isLogin = true;
                    authCode = status["data"]["auth_code"].ToString();
                    authSource = status["data"]["auth_source"].ToString();
                }
                await Task.Delay(delay);
                count++;
            }
            if (string.IsNullOrWhiteSpace(authCode))
            {
                Trace.WriteLine("授权码为空");
                return;
            }

            if (!authSource.Equals("SOURCE_FROM_WEWORK"))
            {
                MessageBox.Show("请使用企微扫码", "请使用企微扫码", MessageBoxButton.OK, MessageBoxImage.Warning);
                goto ReGetLoginQrCode;
            }

            var login1 = await GetLoginCookie_1(key, authCode);
            richText_login_cookie.Document = new FlowDocument(new Paragraph(new Run(login1)));
            var index = login1.IndexOf('/');
            var login2Url = login1.Substring(index + 1);
            var login2 = await GetLoginCookie_2(login2Url);

            index = login2.IndexOf('/');
            var frameUrl = login2.Substring(index + 1);
            var frame = await GoToFrame(frameUrl);


            var cookieSb = new StringBuilder();
            foreach (var item in GetAllCookies(WeWorkWebRequest.GetCookies))
            {
                cookieSb.Append($"{item.Name} : {item.Value}");
            }
            richText_login_cookie.Document = new FlowDocument(new Paragraph(new Run(cookieSb.ToString())));
        }

        private async void Button_GetCorpDepts_Click(object sender, RoutedEventArgs e)
        {
            var dic = new Dictionary<string, string>
            {
                {"_d2st","a3652781" }
            };
            var url = WeWorkWebRequest.GetQueryUrl("wework_admin/contacts/party/cache", new Dictionary<string, string>
            {
                {"lang","zh_CN" }, {"f", "json"}, {"timeZoneInfo%5Bzone_offset%5D", "-8"}, {"random", "0.32059266720270596"}
            });
            var data = await WeWorkWebRequest.HttpWebRequestPostAsync(url, dic);
            richText_resp.Document = new FlowDocument(new Paragraph(new Run(data)));
        }
        private async void Button_GetExtContactAndUserSecret_Click(object sender, RoutedEventArgs e)
        {
            // 1、获取应用列表
            var dic = new Dictionary<string, string>
            {
                {"_d2st","a7254423" }, {"app_type","0" }
            };
            var url = WeWorkWebRequest.GetQueryUrl("wework_admin/getCorpApplication", new Dictionary<string, string>
            {
                {"lang","zh_CN" }, {"f", "json"}, {"ajax", "1"}, {"timeZoneInfo%5Bzone_offset%5D", "-8"}, {"random", "0.8133268874348787"}
            });
            var data = await WeWorkWebRequest.HttpWebRequestPostAsync(url, dic);
            var model = JsonConvert.DeserializeObject<Base<CorpApp>>(data);
            // 获取客户联系、通讯录appid (app_open_id为固定值，2000002：通讯录同步助手，2000003：外部联系人)
            var appids = model.Data.openapi_app.Where(m => m.app_open_id == 2000002 || m.app_open_id == 2000003).Select(a => a.app_id).ToList();
            foreach (var id in appids)
            {
                dic = new Dictionary<string, string>
                {
                  {"appid",id },  {"business_type","1" }, {"app_type","1" }, {"_d2st","a6755627" }
                };
                url = WeWorkWebRequest.GetQueryUrl("wework_admin/two_factor_auth_operation/create", new Dictionary<string, string>
                {
                    {"lang","zh_CN" }, {"f", "json"}, {"ajax", "1"}, {"timeZoneInfo%5Bzone_offset%5D", "-8"}, {"random", "0.8133268874348787"}
                });
                await WeWorkWebRequest.HttpWebRequestPostAsync(url, dic);

                await Task.Delay(1000);
            }

            richText_resp.Document = new FlowDocument(new Paragraph(new Run("完成Secret发送，请在企微客户端查看")));
        }

        private async Task<string> GetLoginAndShowQrCode()
        {
            var keyUrl = WeWorkWebRequest.GetQueryUrl("wework_admin/wwqrlogin/mng/get_key", new Dictionary<string, string>
            {
                {"r","0.2222327287230379" }
            });
            var qrCodekey = await WeWorkWebRequest.HttpWebRequestGetAsync(keyUrl);
            var json = JsonConvert.DeserializeObject<JObject>(qrCodekey);
            if (json == null || json["data"]["qrcode_key"] == null) throw new Exception("企微二维码Key为空");
            var key = json["data"]["qrcode_key"].ToString();
            var qrCodeUrl = WeWorkWebRequest.GetQueryUrl($"{WeWorkWebRequest.GetBaseUrl}wwqrlogin/mng/qrcode/{key}", new Dictionary<string, string>
            {
                {"login_type","login_admin" }
            });
            byte[] btyarray = GetImageFromResponse(qrCodeUrl);
            MemoryStream ms = new MemoryStream(btyarray);
            imgage_qrcode.Source = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);

            return key;
        }

        private async Task<string> GetLoginStatus(string qrCodeKey)
        {
            var url = WeWorkWebRequest.GetQueryUrl("wework_admin/wwqrlogin/check", new Dictionary<string, string>
            {
                {"qrcode_key",qrCodeKey }, {"status", ""}
            });
            return await WeWorkWebRequest.HttpWebRequestGetAsync(url);
        }

        private async Task<string> GetLoginCookie_1(string qrCodeKey, string authCode)
        {
            var url = WeWorkWebRequest.GetQueryUrl("wework_admin/loginpage_wx", new Dictionary<string, string>
            {
               {"_r","751" }, {"code",authCode }, {"qrcode_key",qrCodeKey }, {"wwqrlogin", "1"}, {"auth_source", "SOURCE_FROM_WEWORK"}
            });
            return await WeWorkWebRequest.HttpWebRequestGetAsync(url);
        }

        private async Task<string> GetLoginCookie_2(string url)
        {
            return await WeWorkWebRequest.HttpWebRequestGetAsync(url);
        }

        private async Task<string> GoToFrame(string url)
        {
            return await WeWorkWebRequest.HttpWebRequestGetAsync(url);
        }

        #region HttpWebRequest

        #endregion

        #region Private

        private byte[] GetImageFromResponse(string url, string cookie = null)
        {
        redo:
            try
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    request.Headers[System.Net.HttpRequestHeader.Cookie] = cookie;
                }

                System.Net.WebResponse response = request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Byte[] buffer = new Byte[1024];
                        int current = 0;
                        do
                        {
                            ms.Write(buffer, 0, current);
                        } while ((current = stream.Read(buffer, 0, buffer.Length)) != 0);

                        return ms.ToArray();
                    }
                }
            }
            catch (System.Net.WebException ex)
            {
                if (ex.Message == "基础连接已经关闭: 发送时发生错误。")
                {
                    goto redo;
                }
                else
                {
                    throw;
                }
            }


        }

        /// <summary>
        /// 遍历CookieContainer
        /// </summary>
        /// <param name="cc"></param>
        /// <returns></returns>
        private List<Cookie> GetAllCookies(CookieContainer cc)
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

        #endregion
    }
}
