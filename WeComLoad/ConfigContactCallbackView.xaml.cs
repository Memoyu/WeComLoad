using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using WeComLoad.Automation;

namespace WeComLoad
{
    /// <summary>
    /// ConfigContactCallbackView.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigContactCallbackView : Window
    {
        private readonly IWeComAdmin _weComAdmin;
        private string _key = string.Empty;
        private string _appid = string.Empty;

        public ConfigContactCallbackView(IWeComAdmin weComAdmin)
        {
            InitializeComponent();
            _weComAdmin = weComAdmin;
        }

        private async void Button_TwoFactorAuthOp_Click(object sender, RoutedEventArgs e)
        {
            // 获取客户联系、通讯录appid (app_open_id为固定值，2000002：通讯录同步助手，2000003：外部联系人)
            var apps = await _weComAdmin.GetCorpAppAsync(true);
            if (apps == null || apps.Data == null)
            {
                richText_resp.Document = new FlowDocument(new Paragraph(new Run("获取企业app信息失败")));
                return;
            }
            var appid = apps.Data.OpenapiApps.FirstOrDefault(a => a.AppOpenId == 2000002)?.AppId;
            var key = await _weComAdmin.CreateTwoFactorAuthOpAsync(appid);
            if (string.IsNullOrWhiteSpace(key))
            {
                tb_hint.Text = "发起配置失败";
                return;
            }
            tb_hint.Text = "请在企微客户端中进行确认操作!!!!!!!!!!!";
            var delay = 1000;
            var count = 1;
            var isCheck = false;
            while (!isCheck)
            {
                var state = await _weComAdmin.QueryTwoFactorAuthOpAsync(key);
                var hint = string.Empty;
                if (delay * count == 60 * 1000)
                {
                    tb_hint.Text = "等待确认操作超时，请重新发起！！！！";
                    return;
                }

                if (state == 0)
                {
                    hint = "等待确认操作中。。。。。。。";
                }
                else
                {
                    isCheck = true;
                    hint = "完成确认，可点击保存配置了";
                }

                richText_resp.Document = new FlowDocument(new Paragraph(new Run($"{hint}\r\n\r\n当前刷新次数：{count}")));

                await Task.Delay(delay);
                count++;

            }
            _appid = appid;
            _key = key;
        }


        private async void Button_SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            var url = tb_url.Text;
            var token = tb_token.Text;
            var aeskey = tb_aeskey.Text;

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(aeskey))
            {
                MessageBox.Show("请填入相关配置信息");
                return;
            }

            var start = url.IndexOf("://") + 3;
            var end = url.IndexOf('/', start);
            var host = url.Substring(start, end - start);
            if (string.IsNullOrWhiteSpace(_key))
            {
                tb_hint.Text = "请重新点击发起配置！！";
                return ;
            }

            var data = await _weComAdmin.ConfigContactCallbackAsync(new ConfigCallbackRequest
            {
                CallbackUrl = url,
                HostUrl = host,
                Token = token,
                AesKey = aeskey,
                CheckKey = _key,
                Appid = _appid,
            });

            richText_resp.Document = new FlowDocument(new Paragraph(new Run(JsonConvert.SerializeObject(data))));
        }
    }
}
