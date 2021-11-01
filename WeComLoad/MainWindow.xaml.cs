using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using WeComLoad.Model;

namespace WeComLoad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WeComWebRequest _weComWebReq;
        public MainWindow()
        {
            InitializeComponent();
            _weComWebReq = new WeComWebRequest();
        }

        /// <summary>
        /// 获取登录二维码并在扫码确认后登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            foreach (var item in GetAllCookies(_weComWebReq.Cookies))
            {
                cookieSb.Append($"{item.Name} : {item.Value}");
            }
            richText_login_cookie.Document = new FlowDocument(new Paragraph(new Run(cookieSb.ToString())));
        }

        /// <summary>
        /// 获取企业部门列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_GetCorpDepts_Click(object sender, RoutedEventArgs e)
        {
            var model = await _weComWebReq.GetCorpDeptAsync();
            richText_resp.Document = new FlowDocument(new Paragraph(new Run(JsonConvert.SerializeObject(model))));
        }

        /// <summary>
        /// 获取应用列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_GetAgents_Click(object sender, RoutedEventArgs e)
        {
            var model = await _weComWebReq.GetCorpAppAsync();
            richText_resp.Document = new FlowDocument(new Paragraph(new Run(JsonConvert.SerializeObject(model))));
        }
        /// <summary>
        /// 查看客户联系、通讯录Secret
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_GetExtContactAndUserSecret_Click(object sender, RoutedEventArgs e)
        {
            // 1、获取应用列表
            var model = await _weComWebReq.GetCorpAppAsync();
            // 获取客户联系、通讯录appid (app_open_id为固定值，2000002：通讯录同步助手，2000003：外部联系人)
            var appids = model.Data.openapi_app.Where(m => m.app_open_id == 2000002 || m.app_open_id == 2000003).Select(a => a.app_id).ToList();
            foreach (var id in appids)
            {
                var dic = new Dictionary<string, string>
                {
                  {"appid",id },  {"business_type","1" }, {"app_type","1" }, {"_d2st", _weComWebReq.GetD2st() }
                };
                var url = _weComWebReq.GetQueryUrl("wework_admin/two_factor_auth_operation/create", new Dictionary<string, string>
                {
                    {"lang","zh_CN" }, {"f", "json"}, {"ajax", "1"}, {"timeZoneInfo%5Bzone_offset%5D", "-8"}, {"random", _weComWebReq.GetRandom()}
                });
                await _weComWebReq.HttpWebRequestPostAsync(url, dic);

                await Task.Delay(1000);
            }

            richText_resp.Document = new FlowDocument(new Paragraph(new Run("完成Secret发送，请在企微客户端查看")));
        }

        /// <summary>
        /// 创建应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_CreateAgent_Click(object sender, RoutedEventArgs e)
        {
            var name = "章鱼触手";

            var appModel = await _weComWebReq.GetCorpAppAsync(true);
            var agent = appModel.Data.openapi_app.FirstOrDefault(a => a.name.Equals(name));
            if (agent != null)
            {
                richText_resp.Document = new FlowDocument(new Paragraph(new Run($"应用已存在，AgentId = {agent.app_open_id}")));
                return;
            }

            var deptModel = await _weComWebReq.GetCorpDeptAsync();
            var pid = deptModel.Data.party_list.list.Where(p => p.openapi_partyid.Equals("1")).FirstOrDefault();
            if (pid == null)
            {
                richText_resp.Document = new FlowDocument(new Paragraph(new Run($"企业部门列表异常")));
                return;
            }
            // wework_admin/apps/addOpenApiApp?lang=zh_CN&f=json&ajax=1&timeZoneInfo%5Bzone_offset%5D=-8&random=0.33112845648469147
            var description = "这是章鱼";
            var english_name = "";
            var english_description = "";
            var logoimage = "http://p.qlogo.cn/bizmail/Qiabx6eW3f7yic7dk0QOpdUW3X0ic2QONc4f95JoGcMUoIS2Pl4fqgZlQ/0";
            var visible_pid = pid.partyid;
            var dic = new Dictionary<string, string>
            {
                { "name",name },
                { "description",description },
                { "english_name",english_name },
                { "english_description",english_description },
                { "app_open","true" },
                { "logoimage",logoimage },
                { "visible_pid[]",visible_pid },
                { "_d2st", _weComWebReq.GetD2st() }
            };
            var url = _weComWebReq.GetQueryUrl("wework_admin/apps/addOpenApiApp", new Dictionary<string, string>
            {
                {"lang","zh_CN" }, {"f", "json"}, {"ajax", "1"}, {"timeZoneInfo%5Bzone_offset%5D", "-8"}, {"random", _weComWebReq.GetRandom()}
            });
            var res = await _weComWebReq.HttpWebRequestPostAsync(url, dic);
            richText_resp.Document = new FlowDocument(new Paragraph(new Run($"创建成功 res:{res}")));
        }

        #region 登录操作

        private async Task<string> GetLoginAndShowQrCode()
        {
            var keyUrl = _weComWebReq.GetQueryUrl("wework_admin/wwqrlogin/mng/get_key", new Dictionary<string, string>
            {
                {"r","0.2222327287230379" }
            });
            var qrCodekey = await _weComWebReq.HttpWebRequestGetAsync(keyUrl);
            var json = JsonConvert.DeserializeObject<JObject>(qrCodekey);
            if (json == null || json["data"]["qrcode_key"] == null) throw new Exception("企微二维码Key为空");
            var key = json["data"]["qrcode_key"].ToString();
            var qrCodeUrl = _weComWebReq.GetQueryUrl($"{_weComWebReq.BaseUrl}wwqrlogin/mng/qrcode/{key}", new Dictionary<string, string>
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
            var url = _weComWebReq.GetQueryUrl("wework_admin/wwqrlogin/check", new Dictionary<string, string>
            {
                {"qrcode_key",qrCodeKey }, {"status", ""}
            });
            return await _weComWebReq.HttpWebRequestGetAsync(url);
        }

        private async Task<string> GetLoginCookie_1(string qrCodeKey, string authCode)
        {
            var url = _weComWebReq.GetQueryUrl("wework_admin/loginpage_wx", new Dictionary<string, string>
            {
               {"_r","751" }, {"code",authCode }, {"qrcode_key",qrCodeKey }, {"wwqrlogin", "1"}, {"auth_source", "SOURCE_FROM_WEWORK"}
            });
            return await _weComWebReq.HttpWebRequestGetAsync(url);
        }

        private async Task<string> GetLoginCookie_2(string url)
        {
            return await _weComWebReq.HttpWebRequestGetAsync(url);
        }

        private async Task<string> GoToFrame(string url)
        {
            return await _weComWebReq.HttpWebRequestGetAsync(url);
        }

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
